using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ScriptableStateMachine.Editor {
    [CustomEditor(typeof(StatefulEntity), true)]
    public class StatefulEntityEditor : UnityEditor.Editor {
        public override VisualElement CreateInspectorGUI() {
            // Create a container for the custom UI
            var root = new VisualElement();

            // Add the default inspector
            var defaultInspector = new IMGUIContainer(() => DrawDefaultInspector());
            root.Add(defaultInspector);

            // Add a label for debug information
            var debugLabel = new Label("Debug Info") {
                style = { unityFontStyleAndWeight = FontStyle.Bold }
            };
            root.Add(debugLabel);

            // Add a runtime state label
            var currentStateLabel = new Label("Current State: None");
            root.Add(currentStateLabel);

            // Add a message for non-Play Mode
            var playModeMessage = new HelpBox("Enter Play Mode to debug the current state.", HelpBoxMessageType.Info);
            root.Add(playModeMessage);

            // Register an update callback for runtime state
            root.schedule.Execute(() => {
                if (Application.isPlaying) {
                    playModeMessage.style.display = DisplayStyle.None;

                    var statefulEntity = (StatefulEntity)target;
                    if (statefulEntity.instancedStateMachine != null) {
                        var currentState = statefulEntity.instancedStateMachine.CurrentState;
                        currentStateLabel.text = $"Current State: {(currentState != null ? currentState.name : "None")}";
                    } else {
                        currentStateLabel.text = "Current State: Not Initialized";
                    }
                } else {
                    playModeMessage.style.display = DisplayStyle.Flex;
                }
            }).Every(200); // Update every 100ms

            return root;
        }
    }
}
