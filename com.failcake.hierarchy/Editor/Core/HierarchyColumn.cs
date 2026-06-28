#region

using System.Collections.Generic;
using UnityEngine;

#endregion

namespace FailCake.Hierarchy.Editor
{
    public enum LayoutStatus
    {
        Success,
        Partly,
        Skip,
        Failed
    }

    public enum ColumnKind
    {
        Components,
        Layer,
        Static,
        Visibility
    }

    public abstract class HierarchyColumn
    {
        public Rect rect = new Rect(0, 0, 16, 16);

        #region PRIVATE

        protected bool _enabled;

        #endregion

        public virtual LayoutStatus Layout(GameObject go, Rect selectionRect, ref Rect curRect, float maxWidth) {
            return LayoutStatus.Success;
        }

        public virtual void Draw(GameObject go, Rect selectionRect) { }

        public virtual void EventHandler(GameObject go, Event evt) { }

        public virtual void DisabledHandler(GameObject go) { }

        public virtual bool IsEnabled() {
            return this._enabled;
        }

        #region PRIVATE

        protected static void CollectRecursive(GameObject root, List<GameObject> result, int maxDepth = int.MaxValue) {
            if (root == null) return;
            result.Add(root);
            if (maxDepth <= 0) return;

            Transform t = root.transform;
            for (int i = t.childCount - 1; i >= 0; i--) HierarchyColumn.CollectRecursive(t.GetChild(i).gameObject, result, maxDepth - 1);
        }

        #endregion
    }
}