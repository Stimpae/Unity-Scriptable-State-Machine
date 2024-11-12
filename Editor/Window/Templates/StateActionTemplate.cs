using ScriptableStateMachine;
using UnityEngine;

//[CreateAssetMenu(menuName = "State Machine/Actions/TEMPLATEACTIONCLASSNAME_Action")]
public class TEMPLATEACTIONCLASSNAME : StateAction {
    private StatefulEntity m_statefulEntity;

    public override void Initialize(StatefulEntity context) {
        m_statefulEntity = context;
    }

    public override void Execute() {
    }
}