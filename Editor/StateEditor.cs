using System;
using System.Collections;
using System.IO;
using System.Linq;
using ScriptableStateMachine.Editor.Styling;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ScriptableStateMachine.Editor {
    [CustomEditor(typeof(State))]
    public class StateEditor : UnityEditor.Editor {
        private State m_target;
        private ReorderableList m_onEnterActions;
        private ReorderableList m_onExitActions;
        private ReorderableList m_updateActions;
        private ReorderableList m_fixedUpdateActions;
        
        private EditorSavedPref<bool> m_showOnEnterActions;
        private EditorSavedPref<bool> m_showOnExitActions;
        private EditorSavedPref<bool> m_showUpdateActions;
        private EditorSavedPref<bool> m_showFixedUpdateActions;
        
        private EditorSavedPref<int> m_actionScriptPathIndex;
        private EditorSavedPref<int>  m_actionScriptablePathIndex;
        
        private string m_scriptName = "";
        private ActionType m_selectedActionType = ActionType.ENTER;
        private string[] m_folderPaths;

        private SerializedProperty m_uniqueIdProperty;
        
        private enum ActionType { ENTER, EXIT, UPDATE, FIXED_UPDATE }
        
        protected void OnEnable() {
            m_target = (State)target;
            LoadEditorPrefs();
            InitializeReorderableLists();
            m_folderPaths = AssetDatabase.GetAllAssetPaths().Where(path => Directory.Exists(path) && path.StartsWith("Assets/")).ToArray();
            
            m_uniqueIdProperty = serializedObject.FindProperty("stateUID");
            
        }

        private void LoadEditorPrefs() {
            m_showOnEnterActions = new EditorSavedPref<bool>($"{target.GetInstanceID()}.ShowOnEnter", true);
            m_showOnExitActions = new EditorSavedPref<bool>($"{target.GetInstanceID()}.ShowOnExit", true);
            m_showUpdateActions = new EditorSavedPref<bool>($"{target.GetInstanceID()}.ShowOnUpdate", true);
            m_showFixedUpdateActions = new EditorSavedPref<bool>($"{target.GetInstanceID()}.ShowOnFixedUpdate", true);
            m_actionScriptPathIndex = new EditorSavedPref<int>($"{target.GetInstanceID()}.ActionScriptPathIndex", 0);
            m_actionScriptablePathIndex = new EditorSavedPref<int>($"{target.GetInstanceID()}.ActionScriptablePathIndex", 0);
        }

        private void InitializeReorderableLists() {
            m_onEnterActions = CreateReorderableList("onEnterActions");
            m_onExitActions = CreateReorderableList("onExitActions");
            m_updateActions = CreateReorderableList("updateActions");
            m_fixedUpdateActions = CreateReorderableList("fixedUpdateActions");
        }

        private ReorderableList CreateReorderableList(string propertyName) {
            var property = serializedObject.FindProperty(propertyName);
            var list = new ReorderableList(serializedObject, property, true, true, true, true);
            list.drawElementCallback = (rect, index, isActive, isFocused) => {
                var element = property.GetArrayElementAtIndex(index);
                EditorGUI.PropertyField(rect, element, GUIContent.none);
            };
            list.drawHeaderCallback = rect => EditorGUI.LabelField(rect, property.displayName);
            return list;
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            serializedObject.Update();
            
            EditorGUILayout.Space();
            EditorGUIStyling.DrawInlineButton(m_uniqueIdProperty, "Generate New UID", () => {
                m_uniqueIdProperty.stringValue = Guid.NewGuid().ToString();
                serializedObject.ApplyModifiedProperties();
            });
            
            EditorGUILayout.Space();
            EditorGUIStyling.DrawTitle("State Actions", "Create and manage actions for this state " +
                                                        "if needed use the helper buttons.");
            EditorGUIStyling.DrawBoxGroup(DrawFoldouts, 20, 0);
            EditorGUIStyling.DrawSplitter();
            EditorGUIStyling.DrawBoxGroup(DrawCreateActionSection, 7, 0, false);
            EditorGUIStyling.DrawSplitter();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawFoldouts() {
            EditorGUILayout.Space();
            m_showOnEnterActions.Value = EditorGUILayout.Foldout(m_showOnEnterActions.Value, "On Enter Actions");
            if (m_showOnEnterActions.Value) m_onEnterActions.DoLayoutList();
            m_showOnExitActions.Value = EditorGUILayout.Foldout(m_showOnExitActions.Value, "On Exit Actions");
            if (m_showOnExitActions.Value) m_onExitActions.DoLayoutList();
            m_showUpdateActions.Value = EditorGUILayout.Foldout(m_showUpdateActions.Value, "Update Actions");
            if (m_showUpdateActions.Value) m_updateActions.DoLayoutList();
            m_showFixedUpdateActions.Value = EditorGUILayout.Foldout(m_showFixedUpdateActions.Value, "Fixed Update Actions");
            if (m_showFixedUpdateActions.Value) m_fixedUpdateActions.DoLayoutList();
            EditorGUILayout.Space();
        }
        
        private void DrawCreateActionSection() {
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Create New Action", EditorStyles.boldLabel);
            m_actionScriptPathIndex.Value = EditorGUILayout.Popup("Script Path", m_actionScriptPathIndex.Value, m_folderPaths);
            m_actionScriptablePathIndex.Value = EditorGUILayout.Popup("Scriptable Object Path", m_actionScriptablePathIndex.Value, m_folderPaths);
            m_scriptName = EditorGUILayout.TextField("Script Name", m_scriptName);
            m_selectedActionType = (ActionType)EditorGUILayout.EnumPopup("Action Type", m_selectedActionType);
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Create Action Scriptable Object Script")) {
                CreateActionScript();
                EditorUtility.DisplayDialog("Script Creation", "Script created and compilation started. Please wait for it to complete before creating the ScriptableObject.", "OK");
            }

            EditorGUI.BeginDisabledGroup(FindTypeInAllAssemblies(m_scriptName) == null);
            if (GUILayout.Button("Create Action Scriptable Object")) CreateScriptableObject();
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.Space();
        }

        private void CreateActionScript() {
            string scriptFolderPath = m_folderPaths[m_actionScriptPathIndex.Value];
            string scriptFilePath = Path.Combine(scriptFolderPath, $"{m_scriptName}.cs");
            string scriptContent = GenerateActionScriptContent(m_scriptName, m_selectedActionType);
            File.WriteAllText(scriptFilePath, scriptContent);
            AssetDatabase.Refresh();
        }

        private void CreateScriptableObject() {
            string scriptableObjectFolderPath = m_folderPaths[m_actionScriptablePathIndex.Value];
            string scriptableObjectPath = Path.Combine(scriptableObjectFolderPath, $"{m_scriptName}.asset");
            ScriptableObject newAction = ScriptableObject.CreateInstance(m_scriptName);
            if (newAction != null) {
                AssetDatabase.CreateAsset(newAction, scriptableObjectPath);
                AssetDatabase.SaveAssets();
                AddToCorrespondingList(newAction);
                Debug.Log($"Created new ScriptableObject at {scriptableObjectPath}");
            } else {
                Debug.LogError($"Failed to create ScriptableObject of type {m_scriptName}. Ensure the script was created correctly.");
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

        private string GenerateActionScriptContent(string actionName, ActionType actionType) {
            return $@"
using UnityEngine;
using RuntimeUtilities.StateMachine;

[CreateAssetMenu(menuName = ""State Machine/Actions/{actionType}Action"")]
public class {actionName} : StateAction {{
    public override void Execute() {{
        // TODO: Implement {actionType} logic here
    }}
}}";
        }

        private void AddToCorrespondingList(ScriptableObject action) {
            var listProperty = serializedObject.FindProperty(m_selectedActionType switch {
                ActionType.ENTER => "onEnterActions",
                ActionType.EXIT => "onExitActions",
                ActionType.UPDATE => "updateActions",
                ActionType.FIXED_UPDATE => "fixedUpdateActions",
                _ => throw new ArgumentOutOfRangeException()
            });
            listProperty.arraySize++;
            listProperty.GetArrayElementAtIndex(listProperty.arraySize - 1).objectReferenceValue = action;
            serializedObject.ApplyModifiedProperties();
        }
    }
}