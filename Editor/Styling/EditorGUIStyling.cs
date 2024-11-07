using System;
using UnityEditor;
using UnityEngine;

namespace ScriptableStateMachine.Editor.Styling {
    public static class EditorGUIStyling {
        public static GUIStyle ContainerStyle(RectOffset padding, bool dark = false) {
            var style = new GUIStyle(GUI.skin.box) { padding = padding };
            if (!dark) {
                style.normal.background = new Texture2D(0, 0);
            }

            return style;
        }

        public static void DrawIdentifierLine(Rect target, Color targetColor, float thickness = 3f) {
            var identifierRect = new Rect {
                x = target.xMin + 5,
                y = target.yMin - 0,
                width = thickness,
                height = target.height + 0,
                xMin = 15f,
                xMax = 18f
            };
            EditorGUI.DrawRect(identifierRect, targetColor);
        }

        public static void DrawTitle(string text, string subtext = "", int fontSize = 12, bool drawLine = true) {
            GUIStyle style = new GUIStyle(GUI.skin.label) {
                fontStyle = FontStyle.Bold,
                fontSize = fontSize
            };
            GUILayout.Label(text, style);
            if (!string.IsNullOrEmpty(subtext)) GUILayout.Label(subtext, GUI.skin.label);
            if (drawLine) DrawSplitter();
        }

        public static void DrawBoxGroup(Action action, int leftPadding, int colorIndex, bool drawLine = true) {
            EditorGUILayout.BeginVertical(ContainerStyle(new RectOffset(leftPadding, 7, 0, 0), true));
            Rect verticalGroup = EditorGUILayout.BeginVertical();
            var color = GetColor(colorIndex);
            if (drawLine) DrawIdentifierLine(verticalGroup, color);
            action?.Invoke();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
        }
        
        public static void DrawInlineButton(SerializedProperty property,string text, Action action, int colorIndex = 11) {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(property, GUIContent.none);
            var color = GUI.backgroundColor;
            GUI.backgroundColor = GetColor(colorIndex);
            if (GUILayout.Button(text)) action?.Invoke();
            GUI.backgroundColor = color;
            EditorGUILayout.EndHorizontal();
        }

        public static void DrawColouredHelpBox(string message, MessageType messageType, int colourIndex = 11) {
            var color = GUI.backgroundColor;
            GUI.backgroundColor = GetColor(colourIndex);
            EditorGUILayout.HelpBox(message, messageType);
            GUI.backgroundColor = color;
        }
        
        public static void DrawSplitter(float xMin = 0f) {
            var splitterRect = GUILayoutUtility.GetRect(1f, 1f);
            splitterRect.xMin = xMin;
            splitterRect.width += 4f;
            if (Event.current.type != EventType.Repaint) {
                return;
            }
            EditorGUI.DrawRect(splitterRect, new Color(0.12f, 0.12f, 0.12f, 1.333f));
        }

        public static Color GetColor(int index) {
            return index switch {
                0 => new Color32(128, 128, 128, 255),
                1 => new Color32(255, 165, 0, 255),
                2 => new Color32(30, 144, 255, 255),
                3 => new Color32(128, 128, 0, 255),
                4 => new Color32(255, 0, 0, 255),
                5 => new Color32(135, 206, 235, 255),
                6 => new Color32(255, 235, 205, 255),
                7 => new Color32(255, 127, 80, 255),
                8 => new Color32(139, 0, 139, 255),
                9 => new Color32(218, 165, 32, 255),
                10 => new Color32(255, 100, 20, 255),
                11 => new Color32(45, 45, 45, 255),
                _ => new Color32(255, 255, 255, 255)
            };
        }
    }
}