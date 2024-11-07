using UnityEngine;

namespace ScriptableStateMachine {
    public abstract class StatefulEntity : MonoBehaviour{
        [SerializeField] private StateMachine stateMachine;

        protected StateMachine instancedStateMachine;
        
        protected virtual void Awake() {
            instancedStateMachine = Instantiate(stateMachine);
            instancedStateMachine.Initialize(this);
        }
        protected virtual void Update() => instancedStateMachine.Update();
        protected virtual void FixedUpdate() => instancedStateMachine.FixedUpdate();
    }
}