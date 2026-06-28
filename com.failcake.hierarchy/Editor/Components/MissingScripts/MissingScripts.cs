using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FailCake.Hierarchy.Editor
{
    internal static class MissingScripts
    {
        #region PRIVATE

        private static readonly Dictionary<int, int> _cache = new();

        #endregion

        public static void Invalidate() => _cache.Clear();

        public static int Count(GameObject go)
        {
            if (go == null) return 0;
            int id = go.GetInstanceID();
            if (_cache.TryGetValue(id, out var cached)) return cached;

            int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
            _cache[id] = count;
            return count;
        }

        public static bool Has(GameObject go) => Count(go) > 0;

        public static void RemoveFrom(GameObject go)
        {
            if (go == null) return;
            Undo.RegisterCompleteObjectUndo(go, "Remove Missing Scripts");
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
            EditorUtility.SetDirty(go);
            Invalidate();
        }
    }
}
