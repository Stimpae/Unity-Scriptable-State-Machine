using UnityEngine;

namespace ScriptableStateMachine {
    public abstract class StateAction : ScriptableObject {
        private StatefulEntity m_statefulEntity;
        public virtual void Initialize(StatefulEntity context) {
            m_statefulEntity = context;
        }
        public abstract void Execute();
    }
}