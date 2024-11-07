using System;
using UnityEngine;

namespace ScriptableStateMachine {
    [System.Serializable]
    public class StateTransition {
       public State toState;
       public StateCondition condition;
       public StateTransition(State toState, StateCondition condition) {
           this.toState = toState;
           this.condition = condition;
       }
       
    }
}