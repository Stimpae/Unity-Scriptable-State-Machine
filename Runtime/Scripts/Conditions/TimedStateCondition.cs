using System;
using UnityEngine;

namespace ScriptableStateMachine {
    [CreateAssetMenu(menuName = "State Machine/Conditions/Timed State Condition")]
    public class TimedStateCondition : StateCondition {
        public float time;
        private float m_currentTime;
        
        public override void ResetFlags() {
            m_currentTime = time;
        }

        private void OnEnable() {
            ResetFlags();
        }

        public override bool Evaluate(StatefulEntity entity) {
            if (!(m_currentTime > 0)) return true;
            m_currentTime -= Time.deltaTime;
            return false;
        }
    }
}