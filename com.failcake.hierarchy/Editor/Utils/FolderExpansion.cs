using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FailCake.Hierarchy.Editor
{
    [InitializeOnLoad]
    internal static class FolderExpansion
    {
        #region PRIVATE

        private static HashSet<EntityId> _expanded = new();
        private static HashSet<EntityId> _building = new();
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

            var t = go.transform.parent;
            while (t != null)
            {
                if (t.TryGetComponent<Folder>(out _))
                    _building.Add(t.gameObject.GetEntityId());
                t = t.parent;
            }
        }

        public static bool IsFolderExpanded(EntityId folderInstanceId)
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
