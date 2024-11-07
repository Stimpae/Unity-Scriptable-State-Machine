using System;
using System.Collections.Generic;
using System.IO;
using ScriptableStateMachine;
using ScriptableStateMachine.Editor;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

public class StateMachineCreatorWindow : EditorWindow {
    private enum ActionType { ENTER, EXIT, UPDATE, FIXED_UPDATE }
    
    [SerializeField] private VisualTreeAsset visualTreeAsset = default;

    [SerializeField] private Object stateActionTemplate;
    [SerializeField] private Object stateConditionTemplate;

    private string m_scriptName;
    private string m_scriptPath;
    private string m_objectPath;
    private ActionType m_actionType;
    private List<string> m_folders;
    
    private ObjectField m_stateMachineField;
    private ObjectField m_stateField;
    
    [MenuItem("Utility/State Machine Script Creator")]
    public static void ShowExample() {
        var wnd = GetWindow<StateMachineCreatorWindow>(true, "State Machine Script Creator");
        wnd.position = new Rect(Screen.width / 2, Screen.height / 2, 400, 200);
        wnd.maxSize = new Vector2(400, 200);
        wnd.minSize = new Vector2(400, 200);
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
        m_scriptPath = m_folders[0];
        m_objectPath = m_folders[0];
    }

    public void CreateGUI() {
        var visualTree = visualTreeAsset.Instantiate();
        rootVisualElement.Add(visualTree);
        
        var tabView = rootVisualElement.Q<TabView>("TabView");
        if (tabView != null) {
            tabView.style.flexGrow = 1;
            tabView.style.display = DisplayStyle.Flex;
        }
        
        DrawTabs("ConditionTab");
        DrawTabs("ActionTab");
    }

    private void DrawTabs(string tabName) {
        var container = rootVisualElement.Q(tabName);
        
        var scriptPathField = new DropdownField("Script Path", m_folders, 0);
        scriptPathField.RegisterValueChangedCallback(evt => m_scriptPath = evt.newValue);
        container.Add(scriptPathField);
        
        var objectPathField = new DropdownField("Object Path", m_folders, 0);
        objectPathField.RegisterValueChangedCallback(evt => m_objectPath = evt.newValue);
        container.Add(objectPathField);
        
        var scriptNameField = new TextField("Script Name");
        scriptNameField.RegisterValueChangedCallback(evt => m_scriptName = evt.newValue);
        container.Add(scriptNameField);
        
        DrawTabSpecificFields(tabName, container);
        
        var createScriptButton = new Button { text = "Create Scriptable Object Script" };
        createScriptButton.clicked += () => {
            if (tabName == "ConditionTab") {
                GenerateScript(stateConditionTemplate, "Condition");
            }
            else if (tabName == "ActionTab") {
                GenerateScript(stateActionTemplate, "Action");
            }
        };
        container.Add(createScriptButton);

        var createObjectButton = new Button { text = "Create Scriptable Object" };
        createObjectButton.clicked += () => {
            GenerateScriptableObject(tabName == "ConditionTab" ? "Condition" : "Action");
        };
        container.Add(createObjectButton);
        createObjectButton.SetEnabled(false);
    }

    private void GenerateScriptableObject(string typeName) {
        var objectPath = $"{m_objectPath}/{m_scriptName}.asset";
        if (File.Exists(objectPath)) {
            Debug.LogError($"Object already exists at path {objectPath}.");
            return;
        }
        
        ScriptableObject newAction = ScriptableObject.CreateInstance(m_scriptName);
        if (newAction != null) {
            AssetDatabase.CreateAsset(newAction, objectPath);
            AssetDatabase.SaveAssets();
            
            switch (typeName) {
                case "Condition":
                    var condition = newAction as StateCondition;
                    if (condition != null) { }
                    break;
                case "Action":
                    var action = newAction as StateAction;
                    if (action != null) {
                        AddActionToCorrespondingList(m_actionType, action, m_stateField.);
                    }
                    break;
            }
            
            
            Debug.Log($"Created new ScriptableObject at {objectPath}");
        } else {
            Debug.LogError($"Failed to create ScriptableObject of type {m_scriptName}. Ensure the script was created correctly.");
        }
    }

    private void GenerateScript(Object template, string type) {
        if (template == null) {
            Debug.LogError("Template is null.");
            return;
        }
        var templateContent = ScriptGenerationUtility.GetScriptTemplateContents(template);
        templateContent = templateContent.Replace("TEMPLATECLASSNAME", m_scriptName);
        var scriptPath = $"{m_scriptPath}/{m_scriptName}.cs";
        
        if (File.Exists(scriptPath)) {
            Debug.LogError($"Script already exists at path {scriptPath}.");
            return;
        }
        File.WriteAllText(scriptPath, templateContent);
        AssetDatabase.Refresh();
    }
    
    private void AddActionToCorrespondingList(ActionType actionType, ScriptableObject action, SerializedObject state) {
        var listProperty = state.FindProperty(actionType switch {
            ActionType.ENTER => "onEnterActions",
            ActionType.EXIT => "onExitActions",
            ActionType.UPDATE => "updateActions",
            ActionType.FIXED_UPDATE => "fixedUpdateActions",
            _ => throw new ArgumentOutOfRangeException()
        });
        listProperty.arraySize++;
        listProperty.GetArrayElementAtIndex(listProperty.arraySize - 1).objectReferenceValue = action;
        state.ApplyModifiedProperties();
    }
    
    
    private void DrawTabSpecificFields(string tabName, VisualElement container) {
        if (tabName == "ConditionTab") {
            m_stateMachineField = new ObjectField("Target State Machine") { objectType = typeof(StateMachine) };
            container.Add(m_stateMachineField);
        } else if (tabName == "ActionTab") {
            var actionTypeField = new EnumField("Action Type", ActionType.ENTER);
            actionTypeField.RegisterValueChangedCallback(evt => m_actionType = (ActionType)evt.newValue);
            container.Add(actionTypeField);
            
            m_stateField = new ObjectField("Target State") { objectType = typeof(State) };
            container.Add(m_stateField);
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