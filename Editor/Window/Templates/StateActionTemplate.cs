using UnityEngine;

namespace ScriptableStateMachine.Editor {
    [CreateAssetMenu(menuName = "State Machine/Actions/TEMPLATECLASSNAME_Action")]
    public class TEMPLATECLASSNAME : StateAction {
        private StatefulEntity m_statefulEntity;
        public override void Initialize(StatefulEntity context) {
            m_statefulEntity = context;
        }

        public override void Execute() {
            
        }
    }
}