using UnityEngine;

namespace ScriptableStateMachine {
    public abstract class StateCondition : ScriptableObject {
        public abstract bool Evaluate(StatefulEntity entity);
        public abstract void ResetFlags();
    }
    
    /*
    Use this class to create a condition that is based on a boolean flag for example
    you can use the following as an example with whatever events ssytem you are using
    so that a transition is triggered when an event is triggered
    [CreateAssetMenu(menuName = "State Machine/Conditions/Event State Condition")]
    public class EventStateCondition : StateCondition {
         public IEvent listenerStateEvent;
        private bool m_flag;
        public void OnEnable() => listenerStateEvent.Register(OnEventTriggered);
        public void OnDisable() => listenerStateEvent.DeRegister(OnEventTriggered);
        private void OnEventTriggered(IEvent newEvent) => m_flag = true;

        public override bool Evaluate(StatefulEntity entity) {
            var result = m_flag;
            m_flag = false;
            return result;
        }

        public override void ResetFlags() => m_flag = false;
    }
    */
}