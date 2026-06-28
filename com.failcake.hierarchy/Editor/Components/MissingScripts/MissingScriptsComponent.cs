using UnityEditor;
using UnityEngine;

namespace FailCake.Hierarchy.Editor
{
    internal sealed class MissingScriptsComponent : HierarchyComponent
    {
        #region PRIVATE

        private bool  _enabled;
        private bool  _tintRow;
        private Color _tintColor;

        #endregion

        public override int Priority => 20;
        public override bool IsEnabled => this._enabled;

        public MissingScriptsComponent()
        {
            HierarchySettings.Instance.AddListener(HierarchySetting.MissingScriptsShow,      this.Reload);
            HierarchySettings.Instance.AddListener(HierarchySetting.MissingScriptsTintLabel, this.Reload);
            HierarchySettings.Instance.AddListener(HierarchySetting.MissingScriptsColor,     this.Reload);
            this.Reload();
        }

        public override void InvalidateCaches() => MissingScripts.Invalidate();

        public override void DrawBackground(HierarchyRowContext ctx)
        {
            if (!this._tintRow) return;
            if (!MissingScripts.Has(ctx.Go)) return;

            var sel = ctx.SelectionRect;
            var tintRect = new Rect(sel.x + 16F, sel.y, Mathf.Max(0F, sel.width - 16F), sel.height);
            EditorGUI.DrawRect(tintRect, this._tintColor);
        }

        public override bool DrawIcon(HierarchyRowContext ctx)
        {
            if (!MissingScripts.Has(ctx.Go)) return false;

            var icon = Icons.MissingScriptIcon;
            if (icon == null) return false;

            var sel = ctx.SelectionRect;
            var iconArea = new Rect(Mathf.Floor(sel.x), Mathf.Floor(sel.y), 16F, sel.height);
            EditorGUI.DrawRect(iconArea, ctx.GetCompositeRowBG());

            var drawRect = new Rect(iconArea.x, iconArea.y, 16F, 16F);
            GUI.DrawTexture(drawRect, icon, ScaleMode.ScaleToFit);

            int count = MissingScripts.Count(ctx.Go);
            string tip = count == 1
                ? "1 missing script — right-click to remove"
                : count + " missing scripts — right-click to remove";
            GUI.Label(drawRect, new GUIContent(string.Empty, tip));
            return true;
        }

        public override void HandleEvent(HierarchyRowContext ctx)
        {
            var evt = ctx.Evt;
            if (evt.type != EventType.MouseDown || evt.button != 1) return;
            if (!MissingScripts.Has(ctx.Go)) return;

            var sel = ctx.SelectionRect;
            var iconRect = new Rect(Mathf.Floor(sel.x), Mathf.Floor(sel.y), 16F, 16F);
            if (!iconRect.Contains(evt.mousePosition)) return;

            var target = ctx.Go;
            int count = MissingScripts.Count(target);
            string label = count == 1 ? "Remove Missing Script" : "Remove " + count + " Missing Scripts";

            var menu = new GenericMenu();
            menu.AddItem(new GUIContent(label), false, () =>
            {
                if (target == null) return;
                MissingScripts.RemoveFrom(target);
                EditorApplication.RepaintHierarchyWindow();
            });
            menu.ShowAsContext();
            evt.Use();
        }

        #region PRIVATE

        private void Reload()
        {
            this._enabled   = HierarchySettings.Instance.GetBool (HierarchySetting.MissingScriptsShow);
            this._tintRow   = HierarchySettings.Instance.GetBool (HierarchySetting.MissingScriptsTintLabel);
            this._tintColor = HierarchySettings.Instance.GetColor(HierarchySetting.MissingScriptsColor);
        }

        #endregion
    }
}
