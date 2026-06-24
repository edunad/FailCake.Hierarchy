using System.Collections.Generic;
using UnityEngine;

namespace FailCake.Hierarchy.Editor
{
    internal static class FolderChain
    {
        #region PRIVATE

        private static readonly Dictionary<int, bool> _isFolderCache = new();
        private static readonly Dictionary<int, List<Folder>> _chainCache = new();
        private static readonly HashSet<int> _noChainCache = new();

        #endregion

        public static void Invalidate()
        {
            _isFolderCache.Clear();
            _chainCache.Clear();
            _noChainCache.Clear();
        }

        public static bool IsFolder(GameObject go)
        {
            if (go == null) return false;
            int id = go.GetInstanceID();
            if (_isFolderCache.TryGetValue(id, out var cached)) return cached;

            bool has = go.TryGetComponent<Folder>(out _);
            _isFolderCache[id] = has;
            return has;
        }

        public static List<Folder> GetFolderChain(GameObject go)
        {
            if (go == null) return null;
            int id = go.GetInstanceID();

            if (_chainCache.TryGetValue(id, out var cached)) return cached;
            if (_noChainCache.Contains(id)) return null;

            List<Folder> list = null;
            var t = go.transform;
            while (t != null)
            {
                if (t.TryGetComponent<Folder>(out var f))
                    (list ??= new List<Folder>(4)).Add(f);
                t = t.parent;
            }

            if (list == null)
            {
                _noChainCache.Add(id);
                return null;
            }
            _chainCache[id] = list;
            return list;
        }
    }
}
