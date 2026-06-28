using UnityEditor;
using UnityEngine;

namespace FailCake.Hierarchy.Editor
{
    internal sealed class SeparatorComponent : HierarchyComponent
    {
        #region PRIVATE

        private bool  _enabled;
        private bool  _showRowShading;
        private Color _separator, _even, _odd;

        #endregion

        public override bool IsEnabled => this._enabled;

        public SeparatorComponent()
        {
            HierarchySettings.Instance.AddListener(HierarchySetting.SeparatorShow,           this.Reload);
            HierarchySettings.Instance.AddListener(HierarchySetting.SeparatorColor,          this.Reload);
            HierarchySettings.Instance.AddListener(HierarchySetting.SeparatorShowRowShading, this.Reload);
            HierarchySettings.Instance.AddListener(HierarchySetting.SeparatorEvenRowColor,   this.Reload);
            HierarchySettings.Instance.AddListener(HierarchySetting.SeparatorOddRowColor,    this.Reload);
            this.Reload();
        }

        public override void DrawBackground(HierarchyRowContext ctx)
        {
            var sel = ctx.SelectionRect;

            var line = new Rect(0F, sel.y, sel.x + sel.width, 1F);
            EditorGUI.DrawRect(line, this._separator);

            if (!this._showRowShading) return;

            float rowH = sel.height > 0F ? sel.height : 16F;
            var fill = new Rect(0F, sel.y + 1F, sel.x + sel.width, sel.height - 1F);
            bool even = Mathf.FloorToInt(((fill.y - 4F) / rowH) % 2F) == 0;
            EditorGUI.DrawRect(fill, even ? this._even : this._odd);
        }

        #region PRIVATE

        private void Reload()
        {
            this._enabled        = HierarchySettings.Instance.GetBool (HierarchySetting.SeparatorShow);
            this._separator      = HierarchySettings.Instance.GetColor(HierarchySetting.SeparatorColor);
            this._showRowShading = HierarchySettings.Instance.GetBool (HierarchySetting.SeparatorShowRowShading);
            this._even           = HierarchySettings.Instance.GetColor(HierarchySetting.SeparatorEvenRowColor);
            this._odd            = HierarchySettings.Instance.GetColor(HierarchySetting.SeparatorOddRowColor);
        }

        #endregion
    }
}
