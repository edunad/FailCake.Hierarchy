using UnityEngine;

namespace FailCake.Hierarchy.Editor
{
    internal sealed class DividerComponent : HierarchyComponent
    {
        #region PRIVATE

        private bool  _enabled;
        private Color _lineColor;

        #endregion

        public override bool IsEnabled => this._enabled;

        public DividerComponent()
        {
            HierarchySettings.Instance.AddListener(HierarchySetting.DividerShow,    this.Reload);
            HierarchySettings.Instance.AddListener(HierarchySetting.SeparatorColor, this.Reload);
            this.Reload();
        }

        public override bool TryConsumeRow(HierarchyRowContext ctx)
        {
            if (ctx.IsFolder) return false;
            if (!Divider.IsDivider(ctx.Go)) return false;
            if (ctx.IsRenaming()) return false;

            if (ctx.IsRepaint)
                Divider.Draw(ctx.Go, ctx.SelectionRect, ctx.GetRowBG(), this._lineColor);
            return true;
        }

        #region PRIVATE

        private void Reload()
        {
            this._enabled = HierarchySettings.Instance.GetBool(HierarchySetting.DividerShow);
            var c = HierarchySettings.Instance.GetColor(HierarchySetting.SeparatorColor);
            c.a = 1F;
            this._lineColor = c;
        }

        #endregion
    }
}
