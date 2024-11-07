using ScriptableStateMachine.Editor;
using UnityEditor;
using UnityEditorInternal;

namespace ScriptableStateMachine.Editor {
    [CustomEditor(typeof(StateMachine))]
    public class StateMachineEditor : UnityEditor.Editor {
        private StateMachine m_stateMachine;
        
        private ReorderableList m_transitionList;
        private ReorderableList m_anyTransitionList;
        
        protected void OnEnable() {
            m_stateMachine = target as StateMachine;
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
        }
    }
}