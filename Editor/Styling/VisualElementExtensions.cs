using UnityEngine;
using UnityEngine.UIElements;

namespace ScriptableStateMachine.Editor.Styling {
    public static class VisualElementExtensions {
        
        public static void AddPadding(this VisualElement element, Vector4 padding) {
            element.style.paddingTop = padding.x;
            element.style.paddingRight = padding.y;
            element.style.paddingBottom = padding.z;
            element.style.paddingLeft = padding.w;
        }
        
        public static void AddMargin(this VisualElement element, Vector4 margin) {
            element.style.marginTop = margin.x;
            element.style.marginRight = margin.y;
            element.style.marginBottom = margin.z;
            element.style.marginLeft = margin.w;
        }
        
        public static void AddFlex(this VisualElement element, float grow, float shrink, float basis) {
            element.style.flexGrow = grow;
            element.style.flexShrink = shrink;
            element.style.flexBasis = basis;
        }
        
        public static void SetWidthPercentage(this VisualElement element, float percentage) {
            element.style.width = new StyleLength(Length.Percent(percentage));
        }
    }
}