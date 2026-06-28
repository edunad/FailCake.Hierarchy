using UnityEditor;
using UnityEngine;

namespace FailCake.Hierarchy.Editor
{

    internal sealed class StaticColumn : HierarchyColumn
    {
        #region PRIVATE

        private const int GridW = 11;
        private const int GridH = 10;
        private const int Reserve = 13;

        private Color _activeColor;
        private Color _inactiveColor;
        private StaticEditorFlags _flags;
        private GameObject[] _targets;

        #endregion

        public StaticColumn()
        {
            this.rect.width  = GridW;
            this.rect.height = GridH;

            HierarchySettings.Instance.AddListener(HierarchySetting.StaticShow,              this.OnSettingsChanged);
            HierarchySettings.Instance.AddListener(HierarchySetting.AdditionalActiveColor,   this.OnSettingsChanged);
            HierarchySettings.Instance.AddListener(HierarchySetting.AdditionalInactiveColor, this.OnSettingsChanged);
            this.OnSettingsChanged();
        }

        public override LayoutStatus Layout(GameObject go, Rect selectionRect, ref Rect curRect, float maxWidth)
        {
            if (maxWidth < Reserve) return LayoutStatus.Failed;
            curRect.x -= Reserve;
            this.rect.x = Mathf.Floor(curRect.x + 1);
            this.rect.y = Mathf.Floor(curRect.y + 3);
            this._flags = GameObjectUtility.GetStaticEditorFlags(go);
            return LayoutStatus.Success;
        }

        public override void Draw(GameObject go, Rect selectionRect)
        {

            this.Quad(0, 3, 3, 4, this.HasFlag(StaticEditorFlags.BatchingStatic));
            this.Quad(4, 3, 3, 4, this.HasFlag(StaticEditorFlags.ContributeGI));
            this.Quad(8, 3, 3, 4, this.HasFlag(StaticEditorFlags.ReflectionProbeStatic));

            this.Quad(0, 0, 5, 2, this.HasFlag(StaticEditorFlags.OccludeeStatic));
            this.Quad(6, 0, 5, 2, this.HasFlag(StaticEditorFlags.OccluderStatic));

            this.Quad(0, 8, 5, 2, this.HasFlag(StaticEditorFlags.NavigationStatic));
            this.Quad(6, 8, 5, 2, this.HasFlag(StaticEditorFlags.OffMeshLinkGeneration));
        }

        public override void EventHandler(GameObject go, Event evt)
        {
            if (!evt.isMouse || evt.type != EventType.MouseDown || evt.button != 0) return;
            if (!this.rect.Contains(evt.mousePosition)) return;

            evt.Use();
            this._targets = Selection.Contains(go) ? Selection.gameObjects : new[] { go };
            int intFlags = (int)this._flags;

            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Nothing"),                intFlags ==  0, this.ApplyFlags, 0);
            menu.AddItem(new GUIContent("Everything"),             intFlags == -1, this.ApplyFlags, -1);
            menu.AddSeparator(string.Empty);
            this.AddToggle(menu, "Contribute GI",            StaticEditorFlags.ContributeGI);
            this.AddToggle(menu, "Occluder Static",          StaticEditorFlags.OccluderStatic);
            this.AddToggle(menu, "Occludee Static",          StaticEditorFlags.OccludeeStatic);
            this.AddToggle(menu, "Batching Static",          StaticEditorFlags.BatchingStatic);
            this.AddToggle(menu, "Navigation Static",        StaticEditorFlags.NavigationStatic);
            this.AddToggle(menu, "Off Mesh Link Generation", StaticEditorFlags.OffMeshLinkGeneration);
            this.AddToggle(menu, "Reflection Probe Static",  StaticEditorFlags.ReflectionProbeStatic);
            menu.ShowAsContext();
        }

        #region PRIVATE

        private void OnSettingsChanged()
        {
            this._enabled       = HierarchySettings.Instance.GetBool (HierarchySetting.StaticShow);
            this._activeColor   = HierarchySettings.Instance.GetColor(HierarchySetting.AdditionalActiveColor);
            this._inactiveColor = HierarchySettings.Instance.GetColor(HierarchySetting.AdditionalInactiveColor);
        }

        private void AddToggle(GenericMenu menu, string label, StaticEditorFlags flag)
        {
            bool on = ((int)this._flags & (int)flag) != 0;
            menu.AddItem(new GUIContent(label), on, this.ApplyFlags, (int)flag);
        }

        private void ApplyFlags(object arg)
        {
            int val = (int)arg;
            if (this._targets == null) return;

            for (int i = this._targets.Length - 1; i >= 0; i--)
            {
                var g = this._targets[i];
                if (g == null) continue;

                StaticEditorFlags current = GameObjectUtility.GetStaticEditorFlags(g);
                StaticEditorFlags next = (StaticEditorFlags)val;
                if (val != 0 && val != -1) next = current ^ next;

                Undo.RecordObject(g, "Change Static Flags");
                GameObjectUtility.SetStaticEditorFlags(g, next);
                EditorUtility.SetDirty(g);
            }
        }

        private bool HasFlag(StaticEditorFlags flag) => ((int)this._flags & (int)flag) != 0;

        private void Quad(int x, int y, int w, int h, bool active)
        {
            EditorGUI.DrawRect(
                new Rect(this.rect.x + x, this.rect.y + y, w, h),
                active ? this._activeColor : this._inactiveColor);
        }

        #endregion
    }
}
