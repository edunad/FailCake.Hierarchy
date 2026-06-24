using UnityEditor;
using UnityEngine;

namespace FailCake.Hierarchy.Editor
{

    [InitializeOnLoad]
    internal static class HierarchyInitializer
    {
        #region PRIVATE

        private static HierarchyDrawer _hierarchy;

        #endregion

        static HierarchyInitializer()
        {
            EditorApplication.hierarchyWindowItemByEntityIdOnGUI -= OnHierarchyItem;
            EditorApplication.hierarchyWindowItemByEntityIdOnGUI += OnHierarchyItem;

            EditorApplication.hierarchyChanged         -= OnHierarchyChanged;
            EditorApplication.hierarchyChanged         += OnHierarchyChanged;

            Undo.undoRedoPerformed                     -= OnUndoRedo;
            Undo.undoRedoPerformed                     += OnUndoRedo;
        }

        #region PRIVATE

        private static void EnsureReady()
        {
            if (_hierarchy == null) _hierarchy = new HierarchyDrawer();
        }

        private static void OnHierarchyItem(EntityId instanceId, Rect selectionRect)
        {
            EnsureReady();
            _hierarchy.HierarchyWindowItemOnGUI(instanceId, selectionRect);
        }

        private static void OnHierarchyChanged()
        {
            EnsureReady();
            _hierarchy.InvalidateCaches();
        }

        private static void OnUndoRedo()
        {
            EnsureReady();
            _hierarchy.InvalidateCaches();
            EditorApplication.RepaintHierarchyWindow();
        }

        #endregion
    }
}
