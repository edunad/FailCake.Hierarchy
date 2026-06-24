using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FailCake.Hierarchy.Editor
{
    internal static class Icons
    {
        #region PRIVATE

        private static readonly Dictionary<string, Texture2D> _iconCache = new();

        #endregion

        public static Texture2D VisibilityOnIcon  => GetBuiltin("scenevis_visible_hover");
        public static Texture2D VisibilityOffIcon => GetBuiltin("scenevis_hidden_hover");

        public static Texture2D TransformIcon  => GetBuiltin("Transform Icon");

        public static Texture2D FolderClosedIcon => GetBuiltin("Folder Icon");
        public static Texture2D FolderOpenIcon   => GetBuiltin("FolderOpened Icon");

        public static Texture2D GetIconForComponent(Component c)
        {
            if (c == null) return null;
            var content = EditorGUIUtility.ObjectContent(c, c.GetType());
            return content?.image as Texture2D;
        }

        #region PRIVATE

        private static Texture2D GetBuiltin(string name)
        {
            if (_iconCache.TryGetValue(name, out var cached)) return cached;

            Texture2D tex = null;
            try
            {
                var content = EditorGUIUtility.IconContent(name);
                tex = content?.image as Texture2D;
            }
            catch { }

            _iconCache[name] = tex;
            return tex;
        }

        #endregion
    }
}
