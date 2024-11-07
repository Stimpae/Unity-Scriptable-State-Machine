using System;
using UnityEditor;

namespace ScriptableStateMachine.Editor {
    public class EditorSavedPref<T> {
        private readonly string m_name;
        private readonly T m_defaultValue;

        public T Value {
            get => GetEditorPref();
            set => SetEditorPref(value);
        }

        public EditorSavedPref(string name, T defaultValue) {
            m_name = name;
            m_defaultValue = defaultValue;
        }

        private T GetEditorPref() {
            return typeof(T) switch {
                { } t when t == typeof(bool) => (T)(object)EditorPrefs.GetBool(m_name, (bool)(object)m_defaultValue),
                { } t when t == typeof(int) => (T)(object)EditorPrefs.GetInt(m_name, (int)(object)m_defaultValue),
                { } t when t == typeof(float) => (T)(object)EditorPrefs.GetFloat(m_name, (float)(object)m_defaultValue),
                { } t when t == typeof(string) => (T)(object)EditorPrefs.GetString(m_name, (string)(object)m_defaultValue),
                _ => throw new System.NotSupportedException($"Type {typeof(T)} is not supported.")
            };
        }

        private void SetEditorPref(T value) {
            switch (value) {
                case bool boolValue:
                    EditorPrefs.SetBool(m_name, boolValue);
                    break;
                case int intValue:
                    EditorPrefs.SetInt(m_name, intValue);
                    break;
                case float floatValue:
                    EditorPrefs.SetFloat(m_name, floatValue);
                    break;
                case string stringValue:
                    EditorPrefs.SetString(m_name, stringValue);
                    break;
                default:
                    throw new System.NotSupportedException($"Type {typeof(T)} is not supported.");
            }
        }
    }
}