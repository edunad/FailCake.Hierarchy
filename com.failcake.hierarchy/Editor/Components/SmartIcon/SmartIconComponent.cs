using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FailCake.Hierarchy.Editor
{
    internal sealed class SmartIconComponent : HierarchyComponent
    {
        #region PRIVATE

        private bool _enabled;
        private readonly List<Component> _buffer = new(8);

        #endregion

        public override int Priority => 5;
        public override bool IsEnabled => this._enabled;

        public SmartIconComponent()
        {
            HierarchySettings.Instance.AddListener(HierarchySetting.SmartIconShow, this.Reload);
            this.Reload();
        }

        public override bool DrawIcon(HierarchyRowContext ctx)
        {
            if (ctx.IsFolder) return false;

            var icon = this.Resolve(ctx.Go);
            if (icon == null) return false;

            var sel = ctx.SelectionRect;
            var iconRect = new Rect(Mathf.Floor(sel.x), Mathf.Floor(sel.y), 16F, 16F);
            EditorGUI.DrawRect(iconRect, ctx.GetCompositeRowBG());
            GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
            return true;
        }

        #region PRIVATE

        private Texture2D Resolve(GameObject go)
        {
            this._buffer.Clear();
            go.GetComponents(this._buffer);

            int nonTransformCount = 0;
            Component primary = null;
            for (int i = 0; i < this._buffer.Count; i++)
            {
                var c = this._buffer[i];
                if (c == null) return null;
                if (c is Transform || c is Folder) continue;
                nonTransformCount++;
                if (primary == null) primary = c;
                if (nonTransformCount > 1) return null;
            }

            if (nonTransformCount == 0) return Icons.TransformIcon;
            return Icons.GetIconForComponent(primary);
        }

        private void Reload() => this._enabled = HierarchySettings.Instance.GetBool(HierarchySetting.SmartIconShow);

        #endregion
    }
}
