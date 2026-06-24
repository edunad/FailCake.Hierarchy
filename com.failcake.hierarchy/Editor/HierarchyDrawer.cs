using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FailCake.Hierarchy.Editor
{
    internal sealed class HierarchyDrawer
    {
        #region PRIVATE

        private const int IconPadding = 2;
        private const float FolderTintSelfAlpha = 0.18F;

        private readonly HashSet<int> _errorHandled = new();

        private readonly List<HierarchyColumn> _preComponents = new();
        private readonly List<HierarchyColumn> _orderedComponents = new();

        private int _rightIndent;
        private bool _folderTintEnabled;
        private int  _folderChildTintPercent;
        private bool _smartIconEnabled;

        private readonly List<Component> _componentBuffer = new(8);

        private GUIStyle _boldLabelStyle;
        private readonly GUIContent _labelContent = new();

        #endregion

        public HierarchyDrawer()
        {
            this._preComponents.Add(new SeparatorColumn());

            this._orderedComponents.Add(new VisibilityColumn());
            this._orderedComponents.Add(new StaticColumn());
            this._orderedComponents.Add(new ComponentsColumn());

            HierarchySettings.Instance.AddListener(HierarchySetting.AdditionalRightIndent,   this.OnSettingsChanged);
            HierarchySettings.Instance.AddListener(HierarchySetting.FolderShowRowBackground, this.OnSettingsChanged);
            HierarchySettings.Instance.AddListener(HierarchySetting.FolderChildTintPercent,  this.OnSettingsChanged);
            HierarchySettings.Instance.AddListener(HierarchySetting.SmartIconShow,           this.OnSettingsChanged);
            this.OnSettingsChanged();
        }

        public void InvalidateCaches()
        {
            FolderChain.Invalidate();
        }

        public void HierarchyWindowItemOnGUI(int instanceId, Rect selectionRect)
        {
            try
            {
                ColorUtils.SetDefaultColor(GUI.color);

                var go = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
                if (go == null) return;

                FolderExpansion.RecordVisibleRow(go);

                bool isRepaint = Event.current.type == EventType.Repaint;
                bool isFolder = FolderChain.IsFolder(go);

                if (isRepaint)
                {
                    for (int i = 0; i < this._preComponents.Count; i++)
                        if (this._preComponents[i].IsEnabled())
                            this._preComponents[i].Draw(go, selectionRect);
                }

                if (isRepaint && this._folderTintEnabled)
                    this.DrawFolderTintBG(go, selectionRect);

                if (isRepaint && isFolder)
                {
                    var iconArea = new Rect(
                        Mathf.Floor(selectionRect.x),
                        Mathf.Floor(selectionRect.y),
                        16, selectionRect.height);
                    EditorGUI.DrawRect(iconArea, this.GetCompositeRowBG(go));

                    bool expanded = FolderExpansion.IsFolderExpanded(instanceId);
                    var folderIcon = expanded ? Icons.FolderOpenIcon : Icons.FolderClosedIcon;
                    if (folderIcon != null)
                    {
                        var drawRect = new Rect(iconArea.x, iconArea.y, 16, 16);
                        GUI.DrawTexture(drawRect, folderIcon, ScaleMode.ScaleToFit);
                    }
                }

                var curRect = new Rect(selectionRect)
                {
                    width = 16,
                    x = selectionRect.x + selectionRect.width - this._rightIndent,
                };
                this.DrawRightColumn(selectionRect, ref curRect, go);

                if (isRepaint && this._smartIconEnabled && !isFolder)
                    this.DrawSmartIcon(go, selectionRect);

                if (isRepaint && isFolder)
                    this.DrawBoldFolderName(go, selectionRect);

                this._errorHandled.Remove(instanceId);
            }
            catch (Exception ex)
            {
                if (this._errorHandled.Add(instanceId))
                    Debug.LogError($"[FailCake.Hierarchy] Error drawing row: {ex}");
            }
        }

        #region PRIVATE

        private void OnSettingsChanged()
        {
            this._rightIndent            = HierarchySettings.Instance.GetInt (HierarchySetting.AdditionalRightIndent);
            this._folderTintEnabled      = HierarchySettings.Instance.GetBool(HierarchySetting.FolderShowRowBackground);
            this._folderChildTintPercent = HierarchySettings.Instance.GetInt (HierarchySetting.FolderChildTintPercent);
            this._smartIconEnabled       = HierarchySettings.Instance.GetBool(HierarchySetting.SmartIconShow);
        }

        private void DrawRightColumn(Rect selectionRect, ref Rect curRect, GameObject go)
        {
            float minX = selectionRect.x + 16;
            var evt = Event.current;

            if (evt.type == EventType.Repaint)
            {
                int drawnUpTo = this._orderedComponents.Count;
                for (int i = 0; i < this._orderedComponents.Count; i++)
                {
                    var c = this._orderedComponents[i];
                    if (c.IsEnabled())
                    {
                        if (c.Layout(go, selectionRect, ref curRect, curRect.x - minX) == LayoutStatus.Failed)
                        {
                            drawnUpTo = i;
                            break;
                        }
                        curRect.x -= IconPadding;
                    }
                    else
                    {
                        c.DisabledHandler(go);
                    }
                }

                for (int i = 0; i < drawnUpTo; i++)
                    if (this._orderedComponents[i].IsEnabled())
                        this._orderedComponents[i].Draw(go, selectionRect);
            }
            else if (evt.type == EventType.MouseDown
                  || evt.type == EventType.MouseUp
                  || evt.type == EventType.MouseDrag)
            {
                for (int i = 0; i < this._orderedComponents.Count; i++)
                {
                    var c = this._orderedComponents[i];
                    if (!c.IsEnabled()) continue;
                    if (c.Layout(go, selectionRect, ref curRect, curRect.x - minX) != LayoutStatus.Failed)
                    {
                        c.EventHandler(go, evt);
                        curRect.x -= IconPadding;
                    }
                }
            }
        }

        private void DrawFolderTintBG(GameObject go, Rect selectionRect)
        {
            var chain = FolderChain.GetFolderChain(go);
            if (chain == null || chain.Count == 0) return;

            var full = new Rect(0, selectionRect.y,
                                selectionRect.x + selectionRect.width, selectionRect.height);

            for (int i = chain.Count - 1; i >= 0; i--)
            {
                var f = chain[i];
                if (f == null) continue;

                bool isSelf = (f.gameObject == go);
                if (isSelf && !f.DrawRowBackground) continue;

                Color tint = f.Tint;
                tint.a *= isSelf
                    ? FolderTintSelfAlpha
                    : (this._folderChildTintPercent / 100F) * FolderTintSelfAlpha;
                EditorGUI.DrawRect(full, tint);
            }
        }

        private Color GetCompositeRowBG(GameObject go)
        {
            Color rowBG = GetRowBG(go);
            if (!this._folderTintEnabled) return rowBG;

            var chain = FolderChain.GetFolderChain(go);
            if (chain == null || chain.Count == 0) return rowBG;

            float aR = 0F, aG = 0F, aB = 0F, aA = 0F;

            for (int i = chain.Count - 1; i >= 0; i--)
            {
                var f = chain[i];
                if (f == null) continue;

                bool isSelf = (f.gameObject == go);
                if (isSelf && !f.DrawRowBackground) continue;

                Color c = f.Tint;
                float alpha = c.a * (isSelf
                    ? FolderTintSelfAlpha
                    : (this._folderChildTintPercent / 100F) * FolderTintSelfAlpha);

                float invA = 1F - alpha;
                aR = c.r * alpha + aR * invA;
                aG = c.g * alpha + aG * invA;
                aB = c.b * alpha + aB * invA;
                aA = alpha + aA * invA;
            }

            if (aA <= 0F) return rowBG;

            float bgMul = 1F - aA;
            return new Color(
                aR + rowBG.r * bgMul,
                aG + rowBG.g * bgMul,
                aB + rowBG.b * bgMul,
                1F);
        }

        private void DrawSmartIcon(GameObject go, Rect selectionRect)
        {
            var icon = this.ResolveSmartIcon(go);
            if (icon == null) return;

            var iconRect = new Rect(
                Mathf.Floor(selectionRect.x),
                Mathf.Floor(selectionRect.y),
                16, 16);
            EditorGUI.DrawRect(iconRect, this.GetCompositeRowBG(go));
            GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
        }

        private Texture2D ResolveSmartIcon(GameObject go)
        {
            this._componentBuffer.Clear();
            go.GetComponents(this._componentBuffer);

            int nonTransformCount = 0;
            Component primary = null;
            for (int i = 0; i < this._componentBuffer.Count; i++)
            {
                var c = this._componentBuffer[i];
                if (c == null) return null;
                if (c is Transform || c is Folder) continue;
                nonTransformCount++;
                if (primary == null) primary = c;
                if (nonTransformCount > 1) return null;
            }

            if (nonTransformCount == 0) return Icons.TransformIcon;
            return Icons.GetIconForComponent(primary);
        }

        private void DrawBoldFolderName(GameObject go, Rect selectionRect)
        {
            if (GUIUtility.keyboardControl != 0
                && EditorGUIUtility.editingTextField
                && Selection.activeGameObject == go)
            {
                return;
            }

            this.EnsureBoldLabelStyle();
            this._labelContent.text = go.name;

            float textWidth = this._boldLabelStyle.CalcSize(this._labelContent).x;
            float labelX = Mathf.Floor(selectionRect.x + 18);
            float maxWidth = Mathf.Max(0F, selectionRect.width - 18F);
            float labelWidth = Mathf.Min(Mathf.Ceil(textWidth) + 6F, maxWidth);
            var labelRect = new Rect(
                labelX,
                Mathf.Floor(selectionRect.y),
                labelWidth,
                selectionRect.height);

            EditorGUI.DrawRect(labelRect, this.GetCompositeRowBG(go));

            bool selected = Selection.Contains(go);
            this._boldLabelStyle.normal.textColor = selected
                ? Color.white
                : EditorStyles.label.normal.textColor;

            GUI.Label(labelRect, this._labelContent, this._boldLabelStyle);
        }

        private void EnsureBoldLabelStyle()
        {
            if (this._boldLabelStyle != null) return;
            this._boldLabelStyle = new GUIStyle(EditorStyles.label)
            {
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(0, 0, 0, 0),
                margin  = new RectOffset(0, 0, 0, 0),
                alignment = TextAnchor.MiddleLeft,
            };
        }

        private static Color GetRowBG(GameObject go)
        {
            bool selected = Selection.Contains(go);
            if (selected)
            {
                bool focused = IsHierarchyFocused();
                return EditorGUIUtility.isProSkin
                    ? (focused ? new Color32( 44,  93, 135, 255) : new Color32( 77,  77,  77, 255))
                    : (focused ? new Color32( 58, 114, 176, 255) : new Color32(143, 143, 143, 255));
            }
            return EditorGUIUtility.isProSkin
                ? new Color32( 56,  56,  56, 255)
                : new Color32(200, 200, 200, 255);
        }

        private static bool IsHierarchyFocused()
        {
            var focused = EditorWindow.focusedWindow;
            if (focused == null) return false;
            return focused.GetType().Name == "SceneHierarchyWindow";
        }

        #endregion
    }
}
