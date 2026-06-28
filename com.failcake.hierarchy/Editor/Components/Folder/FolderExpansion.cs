using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FailCake.Hierarchy.Editor
{
    [InitializeOnLoad]
    internal static class FolderExpansion
    {
        #region PRIVATE

        private static HashSet<int> _expanded = new();
        private static HashSet<int> _building = new();
        private static bool _freshData;

        #endregion

        static FolderExpansion()
        {
            EditorApplication.update -= PromoteBuilding;
            EditorApplication.update += PromoteBuilding;
        }

        public static void RecordVisibleRow(GameObject go)
        {
            if (Event.current == null || Event.current.type != EventType.Repaint) return;
            if (go == null) return;

            _freshData = true;

            var chain = FolderChain.GetFolderChain(go);
            if (chain == null) return;

            for (int i = 0; i < chain.Count; i++)
            {
                var f = chain[i];
                if (f == null) continue;
                if (f.gameObject == go) continue;
                _building.Add(f.gameObject.GetInstanceID());
            }
        }

        public static bool IsFolderExpanded(int folderInstanceId)
        {
            return _expanded.Contains(folderInstanceId);
        }

        #region PRIVATE

        private static void PromoteBuilding()
        {
            if (!_freshData) return;
            _freshData = false;

            (_expanded, _building) = (_building, _expanded);
            _building.Clear();
        }

        #endregion
    }
}
