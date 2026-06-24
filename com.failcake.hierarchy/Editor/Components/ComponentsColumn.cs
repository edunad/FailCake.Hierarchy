using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FailCake.Hierarchy.Editor
{

    internal sealed class ComponentsColumn : HierarchyColumn
    {
        #region PRIVATE

        private const int IconSize = 14;
        private const int IconGap  = 1;
        private const int LeftPad  = 2;

        private Color _tooltipBG;
        private GUIStyle _tooltipStyle;

        private readonly List<Component> _buffer = new(8);
        private int _componentsToDraw;
        private readonly GUIContent _tooltipContent = new();

        #endregion

        public ComponentsColumn()
        {
            this.rect.width  = IconSize;
            this.rect.height = IconSize;

            this._tooltipBG = new Color(0F, 0F, 0F, 0.85F);

            HierarchySettings.Instance.AddListener(HierarchySetting.ComponentsShow, this.OnSettingsChanged);
            this.OnSettingsChanged();
        }

        public override LayoutStatus Layout(GameObject go, Rect selectionRect, ref Rect curRect, float maxWidth)
        {

            this._buffer.Clear();
            go.GetComponents(this._buffer);
            for (int i = this._buffer.Count - 1; i >= 0; i--)
            {
                var c = this._buffer[i];
                if (c is Transform || c is Folder) this._buffer.RemoveAt(i);
            }
            if (this._buffer.Count == 0) return LayoutStatus.Failed;

            int stride = IconSize + IconGap;
            int maxFit = Mathf.FloorToInt((maxWidth - LeftPad) / stride);
            if (maxFit <= 0) return LayoutStatus.Failed;

            this._componentsToDraw = Mathf.Min(maxFit, this._buffer.Count);

            float totalWidth = LeftPad + stride * this._componentsToDraw;
            curRect.x -= totalWidth;

            this.rect.x = Mathf.Floor(curRect.x + LeftPad);
            this.rect.y = Mathf.Floor(curRect.y + (16 - IconSize) / 2);

            return this._componentsToDraw >= this._buffer.Count
                ? LayoutStatus.Success
                : LayoutStatus.Partly;
        }

        public override void Draw(GameObject go, Rect selectionRect)
        {
            int start = this._buffer.Count - this._componentsToDraw;
            float x = this.rect.x;
            int hoveredIndex = -1;

            for (int i = start; i < this._buffer.Count; i++)
            {
                var component = this._buffer[i];
                var iconRect = new Rect(x, this.rect.y, IconSize, IconSize);

                Texture2D icon = Icons.GetIconForComponent(component);

                bool componentEnabled = GetEnabled(component);
                Color tint = Color.white;
                tint.a = componentEnabled ? 1F : 0.35F;

                if (icon != null)
                {
                    ColorUtils.SetColor(tint);
                    GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
                    ColorUtils.ClearColor();
                }

                if (iconRect.Contains(Event.current.mousePosition))
                    hoveredIndex = i;

                x += IconSize + IconGap;
            }

            if (hoveredIndex >= 0) this.DrawTooltip(hoveredIndex, selectionRect);
        }

        #region PRIVATE

        private void OnSettingsChanged()
        {
            this._enabled = HierarchySettings.Instance.GetBool(HierarchySetting.ComponentsShow);
        }

        private void EnsureTooltipStyle()
        {
            if (this._tooltipStyle != null) return;
            this._tooltipStyle = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = Color.white },
                fontSize = 11,
                clipping = TextClipping.Clip,
                padding = new RectOffset(4, 4, 1, 1),
            };
        }

        private void DrawTooltip(int componentIndex, Rect selectionRect)
        {
            this.EnsureTooltipStyle();

            var component = this._buffer[componentIndex];
            this._tooltipContent.text = component != null ? component.GetType().Name : "Missing Script";

            float width = this._tooltipStyle.CalcSize(this._tooltipContent).x;
            int stride = IconSize + IconGap;

            float iconCenterX = this.rect.x + stride * (componentIndex - (this._buffer.Count - this._componentsToDraw)) + IconSize * 0.5F;
            var tipRect = new Rect(iconCenterX - width / 2F - 4F, selectionRect.y, width + 8F, selectionRect.height - 1F);

            if (selectionRect.y > 16) tipRect.y -= 16;
            else                      tipRect.x += width / 2F + 18F;

            EditorGUI.DrawRect(tipRect, this._tooltipBG);
            var labelRect = new Rect(
                tipRect.x + 4F,
                tipRect.y + (tipRect.height - EditorGUIUtility.singleLineHeight) * 0.5F,
                tipRect.width - 8F,
                EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(labelRect, this._tooltipContent, this._tooltipStyle);
        }

        private static bool GetEnabled(Component c)
        {
            if (c == null) return false;
            if (c is Behaviour b) return b.enabled;
            if (c is Renderer r)  return r.enabled;
            if (c is Collider col) return col.enabled;
            return true;
        }

        #endregion
    }
}
