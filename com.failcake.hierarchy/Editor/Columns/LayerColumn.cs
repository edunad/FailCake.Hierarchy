using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FailCake.Hierarchy.Editor
{
    internal sealed class LayerColumn : HierarchyColumn
    {
        #region PRIVATE

        private const int HPadding     = 4;
        private const int MinRowHeight = 16;

        private static readonly string[] _fallbackNames = BuildFallbackNames();

        private GUIStyle _style;
        private readonly GUIContent _content = new();

        private readonly string[] _nameCache  = new string[32];
        private readonly float[]  _widthCache = new float[32];

        private GameObject[] _targets;
        private bool _applyToChildren;

        private readonly List<GameObject> _collectBuffer = new();

        #endregion

        public LayerColumn()
        {
            this.InvalidateCache();
            HierarchySettings.Instance.AddListener(HierarchySetting.LayerShow, this.OnSettingsChanged);
            this.OnSettingsChanged();
        }

        public void InvalidateCache()
        {
            for (int i = 0; i < 32; i++)
            {
                this._nameCache[i] = null;
                this._widthCache[i] = -1F;
            }
        }

        public override LayoutStatus Layout(GameObject go, Rect selectionRect, ref Rect curRect, float maxWidth)
        {
            int layer = go.layer;
            if (layer == 0) return LayoutStatus.Skip;

            this.EnsureStyle();
            float width = this.GetCachedWidth(layer);
            float needed = width + HPadding * 2F;

            if (maxWidth < needed) return LayoutStatus.Failed;

            curRect.x -= needed;
            this.rect = new Rect(
                Mathf.Floor(curRect.x),
                Mathf.Floor(curRect.y),
                needed,
                selectionRect.height > 0 ? selectionRect.height : MinRowHeight);

            return LayoutStatus.Success;
        }

        public override void Draw(GameObject go, Rect selectionRect)
        {
            this.EnsureStyle();
            this._content.text = this.GetCachedName(go.layer);
            GUI.Label(this.rect, this._content, this._style);
        }

        public override void EventHandler(GameObject go, Event evt)
        {
            if (!evt.isMouse || evt.type != EventType.MouseDown || evt.button != 0) return;
            if (!this.rect.Contains(evt.mousePosition)) return;

            evt.Use();

            this._targets = Selection.Contains(go) ? Selection.gameObjects : new[] { go };
            this._applyToChildren = evt.shift;

            var menu = new GenericMenu();
            int currentLayer = go.layer;
            for (int i = 0; i < 32; i++)
            {
                string name = LayerMask.LayerToName(i);
                if (string.IsNullOrEmpty(name)) continue;
                menu.AddItem(new GUIContent(name), currentLayer == i, this.ApplyLayer, i);
            }
            menu.ShowAsContext();
        }

        #region PRIVATE

        private void OnSettingsChanged()
        {
            this._enabled = HierarchySettings.Instance.GetBool(HierarchySetting.LayerShow);
        }

        private string GetCachedName(int layer)
        {
            if ((uint)layer >= 32) return _fallbackNames[0];
            var cached = this._nameCache[layer];
            if (cached != null) return cached;

            string name = LayerMask.LayerToName(layer);
            if (string.IsNullOrEmpty(name)) name = _fallbackNames[layer];
            this._nameCache[layer] = name;
            return name;
        }

        private float GetCachedWidth(int layer)
        {
            if ((uint)layer >= 32) return 0F;
            float cached = this._widthCache[layer];
            if (cached >= 0F) return cached;

            this._content.text = this.GetCachedName(layer);
            float w = this._style.CalcSize(this._content).x;
            this._widthCache[layer] = w;
            return w;
        }

        private void ApplyLayer(object arg)
        {
            int layer = (int)arg;
            if (this._targets == null || this._targets.Length == 0) return;

            bool applyChildren = this._applyToChildren;

            Undo.SetCurrentGroupName(applyChildren ? "Change Layer (recursive)" : "Change Layer");
            int group = Undo.GetCurrentGroup();

            for (int i = this._targets.Length - 1; i >= 0; i--)
            {
                var g = this._targets[i];
                if (g == null) continue;

                if (applyChildren)
                {
                    this._collectBuffer.Clear();
                    CollectRecursive(g, this._collectBuffer);
                    for (int j = 0; j < this._collectBuffer.Count; j++)
                        ApplyLayerSingle(this._collectBuffer[j], layer);
                }
                else
                {
                    ApplyLayerSingle(g, layer);
                }
            }

            Undo.CollapseUndoOperations(group);
        }

        private static void ApplyLayerSingle(GameObject g, int layer)
        {
            if (g == null) return;
            Undo.RecordObject(g, "Change Layer");
            g.layer = layer;
            EditorUtility.SetDirty(g);
        }

        private void EnsureStyle()
        {
            if (this._style != null) return;
            this._style = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleRight,
                padding = new RectOffset(HPadding, HPadding, 0, 0),
                margin  = new RectOffset(0, 0, 0, 0),
                clipping = TextClipping.Clip,
            };
        }

        private static string[] BuildFallbackNames()
        {
            var names = new string[32];
            for (int i = 0; i < 32; i++) names[i] = "Layer " + i;
            return names;
        }

        #endregion
    }
}
