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
    }
}