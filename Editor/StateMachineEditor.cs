using System.Collections.Generic;
using ScriptableStateMachine;
using ScriptableStateMachine.Editor.Styling;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ScriptableStateMachine.Editor
{
    [CustomEditor(typeof(StateMachine))]
    public class StateMachineEditor : UnityEditor.Editor
    {
        private VisualElement m_root;
        private MultiColumnListView m_anyTransitionsListView;
        private MultiColumnListView m_transitionsListView;


        public static VisualElement GetSpaceBlock(float height, float width) {
            return new VisualElement() { style = {
                height = height,
                width = width,
                alignSelf = Align.Center,
                flexShrink = 0,
            } };
        }
        
        public static VisualElement GetSplitter(float padding, float marginCorrection) {
            var splitter = new VisualElement() { style = {
                height = 1,
                borderTopWidth = 1,
                borderTopColor = new StyleColor(new Color(0.12f, 0.12f, 0.12f, 1.333f)),
                paddingTop = padding,
                paddingBottom = padding,
                flexShrink = 0,
                flexGrow = 0,
                marginLeft = -marginCorrection,
                marginRight = -marginCorrection,
            } };
        
            splitter.name = "Splitter";
            return splitter;
        }
        
        public override VisualElement CreateInspectorGUI()
        {
            m_root = new VisualElement();
            
            var multiColumListView = new MultiColumnListView {
                bindingPath = "anyTransitions",
                showBoundCollectionSize = false,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                reorderable = true,
                showAddRemoveFooter = true,
            };
            
            multiColumListView.columns.Add(new Column { bindingPath = "toState", title = "To State", stretchable = true, resizable = false});
            multiColumListView.columns.Add(new Column { bindingPath = "condition", title = "Condition", stretchable = true,resizable = false});
            
            multiColumListView.Bind(serializedObject);
            
            var transitionsListView = new MultiColumnListView {
                bindingPath = "transitions",
                showBoundCollectionSize = false,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                reorderable = true,
                showAddRemoveFooter = true,
            };
            
            transitionsListView.columns.Add(new Column { bindingPath = "fromState", title = "From State", stretchable = true, resizable = false});
            transitionsListView.columns.Add(new Column { bindingPath = "transition.toState", title = "To State", stretchable = true,resizable = false});
            transitionsListView.columns.Add(new Column { bindingPath = "transition.condition", title = "Condition", stretchable = true,resizable = false});
            
            transitionsListView.Bind(serializedObject);
            
            m_root.Add(new PropertyField(serializedObject.FindProperty("initializationState")));
            m_root.Add(GetSpaceBlock(10, 0));
            
            var anyTransitionTitle = new Label("Any Transitions");
            anyTransitionTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            anyTransitionTitle.AddMargin(new Vector4(0, 0, 5,0));
            m_root.Add(anyTransitionTitle);
            m_root.Add(GetSplitter(2,20));
            m_root.Add(multiColumListView);
            m_root.Add(GetSpaceBlock(5, 0));
            var transitionsTitle = new Label("Transitions");
            transitionsTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            transitionsTitle.AddMargin(new Vector4(0, 0, 5,0));
            m_root.Add(transitionsTitle);
            m_root.Add(GetSplitter(2,20));
            m_root.Add(transitionsListView);
            
            return m_root;
        }
        
        
    }
}
