using UnityEditor;
using UnityEngine;

namespace FailCake.Hierarchy.Editor
{
    internal sealed class SeparatorColumn : HierarchyColumn
    {
        #region PRIVATE

        private Color _separator;
        private Color _even;
        private Color _odd;
        private bool  _showRowShading;

        #endregion

        public SeparatorColumn()
        {
            HierarchySettings.Instance.AddListener(HierarchySetting.SeparatorShow,           this.OnSettingsChanged);
            HierarchySettings.Instance.AddListener(HierarchySetting.SeparatorColor,          this.OnSettingsChanged);
            HierarchySettings.Instance.AddListener(HierarchySetting.SeparatorShowRowShading, this.OnSettingsChanged);
            HierarchySettings.Instance.AddListener(HierarchySetting.SeparatorEvenRowColor,   this.OnSettingsChanged);
            HierarchySettings.Instance.AddListener(HierarchySetting.SeparatorOddRowColor,    this.OnSettingsChanged);
            this.OnSettingsChanged();
        }

        public override void Draw(GameObject go, Rect selectionRect)
        {
            this.rect.x = 0;
            this.rect.y = selectionRect.y;
            this.rect.width = selectionRect.x + selectionRect.width;
            this.rect.height = 1;
            EditorGUI.DrawRect(this.rect, this._separator);

            if (this._showRowShading)
            {
                float rowH = selectionRect.height > 0 ? selectionRect.height : 16F;
                selectionRect.width += selectionRect.x;
                selectionRect.x = 0;
                selectionRect.y += 1;
                selectionRect.height -= 1;
                bool evenRow = Mathf.FloorToInt(((selectionRect.y - 4) / rowH) % 2F) == 0;
                EditorGUI.DrawRect(selectionRect, evenRow ? this._even : this._odd);
            }
        }

        #region PRIVATE

        private void OnSettingsChanged()
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
