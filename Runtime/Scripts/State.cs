using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace ScriptableStateMachine {
    [CreateAssetMenu(menuName = "State Machine/State")]
    public class State : ScriptableObject {
        [SerializeField, HideInInspector] public string stateUID;
        [SerializeField, HideInInspector] private StateAction[] onEnterActions;
        [SerializeField, HideInInspector]  private StateAction[] onExitActions;
        [SerializeField, HideInInspector]  private StateAction[] updateActions;
        [SerializeField, HideInInspector]  private StateAction[] fixedUpdateActions;

        private StatefulEntity m_statefulEntity;

        private StateAction[] m_instancedOnEnterActions;
        private StateAction[] m_instancedOnExitActions;
        private StateAction[] m_instancedUpdateActions;
        private StateAction[] m_instancedFixedUpdateActions;

        public virtual void Initialize(StatefulEntity context) {
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

        public virtual void Update() => ExecuteActions(m_instancedUpdateActions);

        public virtual void FixedUpdate() => ExecuteActions(m_instancedFixedUpdateActions);

        public virtual void OnEnter() => ExecuteActions(m_instancedOnEnterActions);

        public virtual void OnExit() => ExecuteActions(m_instancedOnExitActions);

        private void ExecuteActions(StateAction[] actions) {
            if (actions == null) return;
            if (actions.Length == 0) return;
            foreach (var action in actions) action.Execute();
        }
    }
}