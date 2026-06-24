using System.Collections.Generic;
using UnityEngine;

namespace FailCake.Hierarchy.Editor
{
    public enum LayoutStatus
    {
        Success,
        Partly,
        Failed,
    }

    public abstract class HierarchyColumn
    {
        public Rect rect = new(0, 0, 16, 16);

        #region PRIVATE

        protected bool _enabled;

        #endregion

        public virtual LayoutStatus Layout(GameObject go, Rect selectionRect, ref Rect curRect, float maxWidth)
            => LayoutStatus.Success;

        public virtual void Draw(GameObject go, Rect selectionRect) { }

        public virtual void EventHandler(GameObject go, Event evt) { }

        public virtual void DisabledHandler(GameObject go) { }

        public virtual bool IsEnabled() => this._enabled;

        #region PRIVATE

        protected static void CollectRecursive(GameObject root, List<GameObject> result, int maxDepth = int.MaxValue)
        {
            if (root == null) return;
            result.Add(root);
            if (maxDepth <= 0) return;

            var t = root.transform;
            for (int i = t.childCount - 1; i >= 0; i--)
                CollectRecursive(t.GetChild(i).gameObject, result, maxDepth - 1);
        }

        #endregion
    }
}
