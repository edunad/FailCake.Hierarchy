using UnityEditor;
using UnityEngine;

namespace FailCake.Hierarchy.Editor
{
    internal static class HierarchyRowUtil
    {
        public static Color GetRowBG(GameObject go)
        {
            bool selected = Selection.Contains(go);
            if (selected)
            {
                bool focused = IsHierarchyFocused();
                return EditorGUIUtility.isProSkin
                    ? (focused ? new Color32( 44,  93, 135, 255) : new Color32( 77,  77,  77, 255))
                    : (focused ? new Color32( 58, 114, 176, 255) : new Color32(143, 143, 143, 255));
            }
            return EditorGUIUtility.isProSkin
                ? new Color32( 56,  56,  56, 255)
                : new Color32(200, 200, 200, 255);
        }

        public static bool IsRenamingRow(GameObject go)
        {
            return GUIUtility.keyboardControl != 0
                && EditorGUIUtility.editingTextField
                && Selection.activeGameObject == go;
        }

        public static bool IsHierarchyFocused()
        {
            var focused = EditorWindow.focusedWindow;
            return focused != null && focused.GetType().Name == "SceneHierarchyWindow";
        }
    }
}
