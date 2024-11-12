using ScriptableStateMachine;
using UnityEngine;

//[CreateAssetMenu(menuName = "State Machine/States/TEMPLATECONDITIONCLASSNAME_State")]
public class TEMPLATECONDITIONCLASSNAME : StateCondition {
    public override bool Evaluate(StatefulEntity entity) {
        return true;
    }

    public override void ResetFlags() {
    }
}