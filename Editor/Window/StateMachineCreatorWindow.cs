using System;
using System.Collections.Generic;
using System.IO;
using ScriptableStateMachine;
using ScriptableStateMachine.Editor;
using ScriptableStateMachine.Editor.Styling;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Serialization; 
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

public class StateMachineCreatorWindow : EditorWindow {
    private enum ActionType {
        ENTER,
        EXIT,
        UPDATE,
        FIXED_UPDATE
    }

    [SerializeField] private VisualTreeAsset visualTreeAsset = default;

    [SerializeField] private Object stateActionTemplate;
    [SerializeField] private Object stateConditionTemplate;

    private string m_actionStripName;
    private string m_actionScriptPath;
    private string m_actionObjectPath;
    private string m_conditionScriptName;
    private string m_conditionScriptPath;
    private string m_conditionObjectPath;
    private ActionType m_actionType;
    private List<string> m_folders;

    private ObjectField m_stateMachineField;
    private ObjectField m_stateField;
    
    [MenuItem("Utility/State Machine Script Creator")]
    public static void ShowExample() {
        var wnd = GetWindow<StateMachineCreatorWindow>(true, "State Machine Script Creator");
        wnd.position = new Rect(Screen.width / 2, Screen.height / 2, 600, 400);
        wnd.maxSize = new Vector2(600, 400);
        wnd.minSize = new Vector2(600, 400);
        wnd.ShowUtility();
    }

    private List<string> GetAllFolders(string root) {
        var folders = new List<string>();
        foreach (var directory in Directory.GetDirectories(root, "*", SearchOption.AllDirectories)) {
            folders.Add(directory.Replace("\\", "/"));
        }

        return folders;
    }

    private void OnEnable() {
        m_folders = GetAllFolders("Assets/");
        m_actionScriptPath = m_folders[0];
        m_conditionScriptPath = m_folders[0];
        m_actionObjectPath = m_folders[0];
        m_conditionObjectPath = m_folders[0];
    }

    public void CreateGUI() {
        var visualTree = visualTreeAsset.Instantiate();
        visualTree.name = "StateMachineCreatorWindowContainer";
        visualTree.style.flexGrow = 1;
        
        rootVisualElement.Add(visualTree);
        
        var tabView = rootVisualElement.Q<TabView>("TabView");
        if (tabView != null) {
            tabView.style.flexGrow = 1;
            tabView.style.display = DisplayStyle.Flex;
        }
        
        var stateMachineTab = rootVisualElement.Q<Tab>("StateMachineTab");
        stateMachineTab.iconImage = EditorGUIUtility.FindTexture("Collab.Build");
        
        var tab = rootVisualElement.Q<Tab>("ActionTab");
        tab.iconImage = EditorGUIUtility.FindTexture("d_UnityEditor.Graphs.AnimatorControllerTool@2x");
        
        var conditionTab = rootVisualElement.Q<Tab>("ConditionTab");
        conditionTab.iconImage = EditorGUIUtility.FindTexture("d_UnityEditor.ConsoleWindow@2x");
        
        var settingsTab = rootVisualElement.Q<Tab>("SettingsTab");
        settingsTab.iconImage = EditorGUIUtility.FindTexture("SettingsIcon@2x");
        settingsTab.tabHeader.style.marginTop = 50;
        
        var headerContainer = rootVisualElement.Q("unity-tab-view__header-container");
        
        var label = new Label("State Machine Script Creator v1.0");
        label.SetWidthPercentage(100);
        label.AddPadding(new Vector4(0, 4, 4, 4));
        label.style.color = new StyleColor(Color.gray);
        label.style.flexGrow = 1;
        label.style.alignContent = Align.Center;
        label.style.alignSelf = Align.Center;
        label.style.unityTextAlign = TextAnchor.LowerCenter;
        label.style.fontSize = 10;
        headerContainer.Add(label);
        
        DrawConditionTab();
        DrawActionTabs();
    }

    private void OnInspectorUpdate() {
        rootVisualElement.Q<Button>("CreateConditionScriptableButton").SetEnabled(FindTypeInAllAssemblies(m_conditionScriptName) != null);
        rootVisualElement.Q<DropdownField>("ConditionObjectPathField").SetEnabled(FindTypeInAllAssemblies(m_conditionScriptName) != null);
        rootVisualElement.Q<Button>("CreateActionScriptableButton").SetEnabled(FindTypeInAllAssemblies(m_actionStripName) != null);
        rootVisualElement.Q<DropdownField>("ActionObjectPathField").SetEnabled(FindTypeInAllAssemblies(m_actionStripName) != null);
    }

    private void DrawConditionTab() {
        var container = rootVisualElement.Q("ConditionTab");
        
        // use these as headers/info boxes of information
        var helpBox = new HelpBox("If you want to attach this condition to a state machine in the editor when it is created, " +
                                  "select your target state machine below", HelpBoxMessageType.None);
        container.Add(helpBox);
        
        container.Add(GetSplitter(5,  10));
        
        m_stateField = new ObjectField("Target State Machine") { objectType = typeof(StateMachine) };
        container.Add(m_stateField);
        
        // use these as headers/info boxes of information
        var generationHelpBox = new HelpBox("Fill out required script name target save paths and then generate the action script." +
                                            " allow a few seconds after compilation before creating the scriptable object", HelpBoxMessageType.None);
        container.Add(generationHelpBox);
        
        container.Add(GetSplitter(5, 10));
        
        var scriptNameField = new TextField("Script Name");
        scriptNameField.RegisterValueChangedCallback(evt => m_conditionScriptName = evt.newValue);
        scriptNameField.style.marginTop = 4;
        container.Add(scriptNameField);
        
        var scriptPathField = new DropdownField("Script Path", m_folders, 0);
        scriptPathField.RegisterValueChangedCallback(evt => m_actionScriptPath = evt.newValue);
     
        container.Add(scriptPathField);
        
        var objectPathField = new DropdownField("Object Path", m_folders, 0);
        objectPathField.RegisterValueChangedCallback(evt => m_conditionObjectPath = evt.newValue);
        objectPathField.name = "ConditionObjectPathField";
        container.Add(objectPathField);
        
        var createScriptButton = new Button { text = "Create Scriptable Object Script" };
        createScriptButton.clicked += () => {
            GenerateScript(stateConditionTemplate, "TEMPLATECONDITIONCLASSNAME", m_conditionScriptName, m_conditionScriptPath);
        };
        container.Add(createScriptButton);
        
        var createScriptableButton = new Button { text = "Create Scriptable Object" };
        createScriptableButton.clicked += () => GenerateScriptableObject(m_conditionScriptName, m_conditionObjectPath);
        createScriptableButton.name = "CreateConditionScriptableButton";
        container.Add(createScriptableButton);
    }
    
    private void DrawActionTabs() {
        var container = rootVisualElement.Q("ActionTab");
        
        // use these as headers/info boxes of information
        var helpBox = new HelpBox("If you want to attach this action to a state in the editor when it is created, " +
                                  "select the action type from the drop down menu and the drop the target state scriptable object below", HelpBoxMessageType.None);
        container.Add(helpBox);
        
        container.Add(GetSplitter(5, 10));
        
        var actionTypeField = new EnumField("Action Type", ActionType.ENTER);
        actionTypeField.RegisterValueChangedCallback(evt => m_actionType = (ActionType)evt.newValue);
        container.Add(actionTypeField);

        m_stateField = new ObjectField("Target State") { objectType = typeof(State) };
        container.Add(m_stateField);
        
        // use these as headers/info boxes of information
        var generationHelpBox = new HelpBox("Fill out required script name target save paths and then generate the action script." +
                                            " allow a few seconds after compilation before creating the scriptable object", HelpBoxMessageType.None);
        container.Add(generationHelpBox);
        
        container.Add(GetSplitter(5, 10));
        
        var scriptNameField = new TextField("Script Name");
        scriptNameField.RegisterValueChangedCallback(evt => m_actionStripName = evt.newValue);
        scriptNameField.style.marginTop = 4;
        container.Add(scriptNameField);
        
        var scriptPathField = new DropdownField("Script Path", m_folders, 0);
        scriptPathField.RegisterValueChangedCallback(evt => m_actionScriptPath = evt.newValue);
     
        container.Add(scriptPathField);
        
        var actionScriptablePathField = new DropdownField("Object Path", m_folders, 0);
        actionScriptablePathField.RegisterValueChangedCallback(evt => m_actionObjectPath = evt.newValue);
        actionScriptablePathField.name = "ActionObjectPathField";
        container.Add(actionScriptablePathField);
        
        var createScriptButton = new Button { text = "Create Scriptable Object Script" };
        createScriptButton.clicked += () => {
                GenerateScript(stateActionTemplate, "TEMPLATEACTIONCLASSNAME", m_actionStripName, m_actionScriptPath);
        };
        container.Add(createScriptButton);
        
        var createScriptableButton = new Button { text = "Create Scriptable Object" };
        createScriptableButton.clicked += () => GenerateScriptableObject(m_actionStripName, m_actionObjectPath);
        createScriptableButton.name = "CreateActionScriptableButton";
        container.Add(createScriptableButton);
    }
    
    public static VisualElement FlexibleSpace {
        get {
            return new VisualElement() { style = {
                flexGrow = 1 
            } };
        }
    }
    
    public static VisualElement GetSpaceBlock(float height, float width) {
        return new VisualElement() { style = {
            height = height,
            width = width,
            alignSelf = Align.Center,
            flexShrink = 0,
        } };
    }
    
    public static VisualElement GetSplitter(float padding, float marginCorrection) {
        var splitter = new VisualElement() { style = {
            height = 1,
            borderTopWidth = 1,
            borderTopColor = new StyleColor(new Color(0.12f, 0.12f, 0.12f, 1.333f)),
            paddingTop = padding,
            paddingBottom = padding,
            flexShrink = 0,
            flexGrow = 0,
            marginLeft = -marginCorrection,
            marginRight = -marginCorrection,
        } };
        
        splitter.name = "Splitter";
        return splitter;
    }

    private void GenerateScript(Object template, string templateName, string scriptName, string scriptPath) {
        if (template == null) {
            Debug.LogError("Template is null.");
            return;
        }
        var templateContent = ScriptGenerationUtility.GetScriptTemplateContents(template);
        templateContent = templateContent.Replace(templateName, scriptName);
        var path = $"{scriptPath}/{scriptName}.cs";
        
        if (File.Exists(path)) {
            Debug.LogError($"Script already exists at path {path}.");
        }
        
        File.WriteAllText(path, templateContent);
        AssetDatabase.Refresh();
    }

    private void GenerateScriptableObject(string scriptName, string objectPath) {
        var path = $"{objectPath}/{scriptName}.asset";
        
        if (File.Exists(path)) {
            Debug.LogError($"Object already exists at path {path}.");
        }
        
        var newAction = ScriptableObject.CreateInstance(scriptName);
        if (newAction != null) {
            AssetDatabase.CreateAsset(newAction, path);
            AssetDatabase.SaveAssets();
            //todo AddToCorrespondingList(newAction);
            Debug.Log($"Created new ScriptableObject at {path}");
        } else {
            Debug.LogError($"Failed to create ScriptableObject of type {scriptName}. Ensure the script was created correctly.");
        }
    }
    
    private Type FindTypeInAllAssemblies(string typeName) {
        if (string.IsNullOrEmpty(typeName)) return null;
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
            var type = assembly.GetType(typeName);
            if (type != null) return type;
        }
        return null;
    }
    

}