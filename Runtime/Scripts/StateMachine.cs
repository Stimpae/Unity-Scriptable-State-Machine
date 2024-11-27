using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace ScriptableStateMachine {
    
    [Serializable]
    public struct Transition {
        public State fromState;
        public StateTransition transition;
    }
    
    [CreateAssetMenu(menuName = "State Machine/State Machine", fileName = "DefaultStateMachine")]
    public class StateMachine : ScriptableObject {
        public State initializationState;
        [SerializeField] public List<Transition> transitions = new();
        [SerializeField] public List<StateTransition> anyTransitions = new();

        public State CurrentState => m_currentNode.State;

        private readonly Dictionary<string, StateNode> m_nodes = new();
        private readonly HashSet<StateTransition> m_anyTransitions = new();

        private StateNode m_currentNode;
        private StatefulEntity m_context;

        public void Initialize(StatefulEntity context) {
            m_context = context;
            SetupTransitions();
            SetState(GetOrAddNode(initializationState).State);
        }

        private void SetupTransitions() {
            foreach (var transition in transitions) {
                if(transition.fromState == null || transition.transition.toState == null) continue;
                AddTransition(transition.fromState, transition.transition.toState, transition.transition.condition);
            }

            foreach (var transition in anyTransitions) {
                if(transition.toState == null) continue;
                AddAnyTransition(transition.toState, transition.condition);
            }
        }
        
        public void Update() {
            var transition = GetTransition();
            if (transition != null) {
                ChangeState(transition.toState);
                foreach (var node in m_nodes.Values) {
                    ResetPredicateFlags(node.Transitions);
                }

                ResetPredicateFlags(anyTransitions);
            }
            
            m_currentNode.State?.Update();
        }

        private static void ResetPredicateFlags(IEnumerable<StateTransition> transitions) {
            foreach (var transition in transitions) {
                transition.condition.ResetFlags();
            }
        }

        public void FixedUpdate() {
            m_currentNode.State?.FixedUpdate();
        }

        private void SetState(State state) {
            m_currentNode = m_nodes[state.stateUID];
        
            m_currentNode.State?.OnEnter();
        }

        private void ChangeState(State state) {
            if (state == m_currentNode.State)
                return;

            var previousState = m_currentNode.State;
            var nextState = m_nodes[state.stateUID].State;

            previousState?.OnExit();
            nextState.OnEnter();
            m_currentNode = m_nodes[state.stateUID];
        }

        private void AddTransition(State from, State to, StateCondition condition) {
            GetOrAddNode(from).AddTransition(GetOrAddNode(to).State, condition);
        }

        private void AddAnyTransition(State to, StateCondition condition) {
            m_anyTransitions.Add(new StateTransition(GetOrAddNode(to).State, condition));
        }

        private StateTransition GetTransition() {
            foreach (var transition in m_anyTransitions) {
                if (transition.condition.Evaluate(m_context)) return transition;
            }

            if (m_currentNode == null) {
                Debug.LogError($"Current node is null for state machine - {name}");
                return null;
            }

            return m_currentNode.Transitions.FirstOrDefault(transition => transition.condition.Evaluate(m_context));
        }

        private StateNode GetOrAddNode(State state) {
            var node = m_nodes.GetValueOrDefault(state.stateUID);
            if (node != null) return node;
            node = new StateNode(state);
            m_nodes[state.stateUID] = node;
            return node;
        }

        private class StateNode {
            public State State { get; }
            public HashSet<StateTransition> Transitions { get; }

            public StateNode(State state) {
                State = Instantiate(state);
                Transitions = new HashSet<StateTransition>();
            }

            public void AddTransition(State to, StateCondition stateCondition) {
                var instancedCondition = Instantiate(stateCondition);
                Transitions.Add(new StateTransition(to, instancedCondition));
            }
        }
    }
}