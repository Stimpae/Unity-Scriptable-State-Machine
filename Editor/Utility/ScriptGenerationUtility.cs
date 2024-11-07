using System.IO;
using UnityEditor;
using UnityEngine;

namespace ScriptableStateMachine.Editor {
    public static class ScriptGenerationUtility { 
        public static string GetScriptTemplateContents(Object scriptFile) {
            string templatePath = AssetDatabase.GetAssetPath(scriptFile);
            string content = File.ReadAllText(templatePath);
            return content;
        }
    }
}