using UnityEditor;
using UnityEngine;

namespace FailCake.Hierarchy.Editor
{
    internal sealed class BoldFolderNameComponent : HierarchyComponent
    {
        #region PRIVATE

        private GUIStyle _style;
        private readonly GUIContent _content = new();

        #endregion

        public override void DrawLabel(HierarchyRowContext ctx)
        {
            if (!ctx.IsFolder || ctx.IsRenaming()) return;

            this.Ensure();
            this._content.text = ctx.Go.name;

            var sel = ctx.SelectionRect;
            float textWidth  = this._style.CalcSize(this._content).x;
            float labelX     = Mathf.Floor(sel.x + 18F);
            float maxWidth   = Mathf.Max(0F, sel.width - 18F);
            float labelWidth = Mathf.Min(Mathf.Ceil(textWidth) + 6F, maxWidth);

            var rect = new Rect(labelX, Mathf.Floor(sel.y), labelWidth, sel.height);
            EditorGUI.DrawRect(rect, ctx.GetCompositeRowBG());

            this._style.normal.textColor = Selection.Contains(ctx.Go)
                ? Color.white
                : EditorStyles.label.normal.textColor;

            GUI.Label(rect, this._content, this._style);
        }

        #region PRIVATE

        private void Ensure()
        {
            if (this._style != null) return;
            this._style = new GUIStyle(EditorStyles.label)
            {
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(0, 0, 0, 0),
                margin  = new RectOffset(0, 0, 0, 0),
                alignment = TextAnchor.MiddleLeft,
            };
        }

        #endregion
    }
}
