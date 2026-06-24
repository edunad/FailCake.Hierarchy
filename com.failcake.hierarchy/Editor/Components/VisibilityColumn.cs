using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FailCake.Hierarchy.Editor
{

    internal sealed class VisibilityColumn : HierarchyColumn
    {
        #region PRIVATE

        private const int IconSize = 16;

        private Color _activeColor;
        private Color _inactiveColor;
        private Texture2D _onTex;
        private Texture2D _offTex;
        private int _targetState = -1;

        #endregion

        public VisibilityColumn()
        {
            this.rect.width  = IconSize;
            this.rect.height = IconSize;
            this._onTex  = Icons.VisibilityOnIcon;
            this._offTex = Icons.VisibilityOffIcon;

            HierarchySettings.Instance.AddListener(HierarchySetting.VisibilityShow,           this.OnSettingsChanged);
            HierarchySettings.Instance.AddListener(HierarchySetting.AdditionalActiveColor,    this.OnSettingsChanged);
            HierarchySettings.Instance.AddListener(HierarchySetting.AdditionalInactiveColor,  this.OnSettingsChanged);
            this.OnSettingsChanged();
        }

        public override LayoutStatus Layout(GameObject go, Rect selectionRect, ref Rect curRect, float maxWidth)
        {
            if (maxWidth < IconSize) return LayoutStatus.Failed;
            curRect.x -= IconSize;

            this.rect.x = Mathf.Floor(curRect.x);
            this.rect.y = Mathf.Floor(curRect.y);
            return LayoutStatus.Success;
        }

        public override void Draw(GameObject go, Rect selectionRect)
        {

            int state = go.activeSelf ? 1 : 0;
            var t = go.transform;
            while (t.parent != null)
            {
                t = t.parent;
                if (!t.gameObject.activeSelf) { state = 2; break; }
            }

            Texture2D icon = state == 0 ? this._offTex : this._onTex;
            if (icon == null) return;

            Color tint;
            if (state == 0)      tint = this._inactiveColor;
            else if (state == 1) tint = this._activeColor;
            else
            {
                tint = this._activeColor;
                tint.r *= 0.65F; tint.g *= 0.65F; tint.b *= 0.65F; tint.a *= 0.65F;
            }

            ColorUtils.SetColor(tint);
            GUI.DrawTexture(this.rect, icon, ScaleMode.ScaleToFit);
            ColorUtils.ClearColor();
        }

        public override void EventHandler(GameObject go, Event evt)
        {
            if (!evt.isMouse || evt.button != 0 || !this.rect.Contains(evt.mousePosition)) return;

            if (evt.type == EventType.MouseDown)
            {
                this._targetState = go.activeSelf ? 0 : 1;
            }
            else if (evt.type == EventType.MouseDrag && this._targetState != -1)
            {
                if (this._targetState == (go.activeSelf ? 1 : 0)) return;
            }
            else
            {
                this._targetState = -1;
                return;
            }

            bool warn = HierarchySettings.Instance.GetBool(HierarchySetting.AdditionalShowModifierWarning);
            var targets = new List<GameObject>();

            if (evt.shift)
            {
                if (!warn || EditorUtility.DisplayDialog(
                        "Change visibility",
                        $"Turn {(go.activeSelf ? "off" : "on")} this GameObject and all its children?",
                        "Yes", "Cancel"))
                {
                    CollectRecursive(go, targets);
                }
            }
            else if (evt.alt)
            {
                if (go.transform.parent == null) return;
                if (!warn || EditorUtility.DisplayDialog(
                        "Change visibility",
                        $"Turn {(go.activeSelf ? "off" : "on")} this GameObject and its siblings?",
                        "Yes", "Cancel"))
                {
                    CollectRecursive(go.transform.parent.gameObject, targets, 1);
                    targets.Remove(go.transform.parent.gameObject);
                }
            }
            else
            {
                if (Selection.Contains(go)) targets.AddRange(Selection.gameObjects);
                else CollectRecursive(go, targets, 0);
            }

            SetVisibility(targets, !go.activeSelf);
            evt.Use();
        }

        #region PRIVATE

        private void OnSettingsChanged()
        {
            this._enabled       = HierarchySettings.Instance.GetBool (HierarchySetting.VisibilityShow);
            this._activeColor   = HierarchySettings.Instance.GetColor(HierarchySetting.AdditionalActiveColor);
            this._inactiveColor = HierarchySettings.Instance.GetColor(HierarchySetting.AdditionalInactiveColor);
        }

        private static void SetVisibility(List<GameObject> targets, bool on)
        {
            if (targets == null || targets.Count == 0) return;
            for (int i = targets.Count - 1; i >= 0; i--)
            {
                var g = targets[i];
                if (g == null) continue;
                Undo.RecordObject(g, on ? "Show GameObject" : "Hide GameObject");
                g.SetActive(on);
                EditorUtility.SetDirty(g);
            }
        }

        #endregion
    }
}
