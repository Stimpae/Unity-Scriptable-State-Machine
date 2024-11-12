using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace ScriptableStateMachine {
    [CreateAssetMenu(menuName = "State Machine/State")]
    public sealed class State : ScriptableObject {
        [SerializeField] public string stateUID;
        [SerializeField] private StateAction[] onEnterActions;
        [SerializeField]  private StateAction[] onExitActions;
        [SerializeField]  private StateAction[] updateActions;
        [SerializeField]  private StateAction[] fixedUpdateActions;

        private StatefulEntity m_statefulEntity;

        private StateAction[] m_instancedOnEnterActions;
        private StateAction[] m_instancedOnExitActions;
        private StateAction[] m_instancedUpdateActions;
        private StateAction[] m_instancedFixedUpdateActions;

        public void Initialize(StatefulEntity context) {
            m_statefulEntity = context;
            m_instancedOnEnterActions = InitializeActions(onEnterActions, context);
            m_instancedOnExitActions = InitializeActions(onExitActions, context);
            m_instancedUpdateActions = InitializeActions(updateActions, context);
            m_instancedFixedUpdateActions = InitializeActions(fixedUpdateActions, context);
        }

        private StateAction[] InitializeActions(StateAction[] actions, StatefulEntity context) {
            if (actions == null) return null;
            var instances = new StateAction[actions.Length];
            for (int i = 0; i < actions.Length; i++) {
                instances[i] = Instantiate(actions[i]);
                instances[i].Initialize(context);
            }
            return instances;
        }

        public void Update() => ExecuteActions(m_instancedUpdateActions);

        public void FixedUpdate() => ExecuteActions(m_instancedFixedUpdateActions);

        public void OnEnter() => ExecuteActions(m_instancedOnEnterActions);

        public void OnExit() => ExecuteActions(m_instancedOnExitActions);

        private void ExecuteActions(StateAction[] actions) {
            if (actions == null) return;
            if (actions.Length == 0) return;
            foreach (var action in actions) action.Execute();
        }
    }
}