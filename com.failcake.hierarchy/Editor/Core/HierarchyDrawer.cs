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

        private readonly List<HierarchyComponent> _components = new();
        private readonly List<HierarchyComponent> _componentsByPriority = new();
        private readonly HashSet<int> _errorHandled = new();

        private readonly List<HierarchyColumn> _orderedColumns = new();
        private readonly VisibilityColumn _visibilityColumn;
        private readonly StaticColumn     _staticColumn;
        private readonly LayerColumn      _layerColumn;
        private readonly ComponentsColumn _componentsColumn;

        private readonly FolderTintComponent _folderTintComponent;

        private int _rightIndent;
        private readonly HierarchyRowContext _ctx = new();

        #endregion

        public HierarchyDrawer()
        {
            // Right-side columns (existing layout system — kept separate from the
            // HierarchyComponent plugin phases because they have their own protocol).
            this._visibilityColumn = new VisibilityColumn();
            this._staticColumn     = new StaticColumn();
            this._layerColumn      = new LayerColumn();
            this._componentsColumn = new ComponentsColumn();

            // Plugin components. Registration order drives background/label/overlay
            // draw order (later = on top). Icon slot is resolved by Priority instead.
            this.Register(new SeparatorComponent());
            this.Register(this._folderTintComponent = new FolderTintComponent());
            this.Register(new DividerComponent());
            this.Register(new FolderIconComponent());
            this.Register(new SmartIconComponent());
            this.Register(new BoldFolderNameComponent());
            this.Register(new MissingScriptsComponent());

            HierarchySettings.Instance.AddListener(HierarchySetting.AdditionalRightIndent, this.OnSettingsChanged);
            HierarchySettings.Instance.AddListener(HierarchySetting.ComponentsOrder,       this.RebuildColumnOrder);

            this.OnSettingsChanged();
            this.RebuildColumnOrder();
        }

        public void Register(HierarchyComponent component)
        {
            this._components.Add(component);
            this._componentsByPriority.Add(component);
            this._componentsByPriority.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        }

        public void InvalidateCaches()
        {
            FolderChain.Invalidate();
            this._layerColumn.InvalidateCache();
            for (int i = 0; i < this._components.Count; i++)
                this._components[i].InvalidateCaches();
        }

        /// <summary>Compose the row background including any folder tint. Exposed for components.</summary>
        public Color ComposeRowBG(GameObject go)
        {
            Color baseBG = HierarchyRowUtil.GetRowBG(go);
            return this._folderTintComponent != null && this._folderTintComponent.IsEnabled
                ? this._folderTintComponent.Compose(go, baseBG)
                : baseBG;
        }

        public void HierarchyWindowItemOnGUI(int instanceId, Rect selectionRect)
        {
            try
            {
                ColorUtils.SetDefaultColor(GUI.color);

                var go = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
                if (go == null) return;

                FolderExpansion.RecordVisibleRow(go);

                var ctx = this._ctx;
                ctx.Go            = go;
                ctx.InstanceId    = instanceId;
                ctx.SelectionRect = selectionRect;
                ctx.Evt           = Event.current;
                ctx.IsRepaint     = ctx.Evt.type == EventType.Repaint;
                ctx.IsFolder      = FolderChain.IsFolder(go);
                ctx.IconClaimed   = false;
                ctx.RowConsumed   = false;
                ctx.Drawer        = this;

                // Phase 1: row consumers (dividers, etc.).
                for (int i = 0; i < this._components.Count; i++)
                {
                    var c = this._components[i];
                    if (!c.IsEnabled) continue;
                    if (c.TryConsumeRow(ctx)) { ctx.RowConsumed = true; break; }
                }
                if (ctx.RowConsumed) { this._errorHandled.Remove(instanceId); return; }

                // Phase 2: backgrounds (repaint only).
                if (ctx.IsRepaint)
                {
                    for (int i = 0; i < this._components.Count; i++)
                    {
                        var c = this._components[i];
                        if (c.IsEnabled) c.DrawBackground(ctx);
                    }
                }

                // Phase 3: icon slot — priority-ordered, first claim wins.
                if (ctx.IsRepaint)
                {
                    for (int i = 0; i < this._componentsByPriority.Count; i++)
                    {
                        var c = this._componentsByPriority[i];
                        if (!c.IsEnabled) continue;
                        if (c.DrawIcon(ctx)) { ctx.IconClaimed = true; break; }
                    }
                }

                // Phase 4: right-side column grid (existing system, unchanged).
                var curRect = new Rect(selectionRect)
                {
                    width = 16,
                    x = selectionRect.x + selectionRect.width - this._rightIndent,
                };
                this.DrawRightColumn(selectionRect, ref curRect, go);

                // Phase 5: labels (repaint only).
                if (ctx.IsRepaint)
                {
                    for (int i = 0; i < this._components.Count; i++)
                    {
                        var c = this._components[i];
                        if (c.IsEnabled) c.DrawLabel(ctx);
                    }
                }

                // Phase 6: overlays (repaint only).
                if (ctx.IsRepaint)
                {
                    for (int i = 0; i < this._components.Count; i++)
                    {
                        var c = this._components[i];
                        if (c.IsEnabled) c.DrawOverlay(ctx);
                    }
                }

                // Phase 7: events (non-repaint only).
                if (!ctx.IsRepaint)
                {
                    for (int i = 0; i < this._components.Count; i++)
                    {
                        var c = this._components[i];
                        if (c.IsEnabled) c.HandleEvent(ctx);
                    }
                }

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
            this._rightIndent = HierarchySettings.Instance.GetInt(HierarchySetting.AdditionalRightIndent);
        }

        private void RebuildColumnOrder()
        {
            this._orderedColumns.Clear();

            var raw = HierarchySettings.Instance.GetString(HierarchySetting.ComponentsOrder);
            var order = new List<ColumnKind>(4);
            var seen  = new HashSet<ColumnKind>();

            if (!string.IsNullOrEmpty(raw))
            {
                foreach (var token in raw.Split(','))
                {
                    if (Enum.TryParse<ColumnKind>(token.Trim(), out var kind) && seen.Add(kind))
                        order.Add(kind);
                }
            }

            foreach (ColumnKind kind in Enum.GetValues(typeof(ColumnKind)))
                if (seen.Add(kind)) order.Add(kind);

            for (int i = order.Count - 1; i >= 0; i--)
            {
                var column = this.ResolveColumn(order[i]);
                if (column != null) this._orderedColumns.Add(column);
            }

            EditorApplication.RepaintHierarchyWindow();
        }

        private HierarchyColumn ResolveColumn(ColumnKind kind) => kind switch
        {
            ColumnKind.Visibility => this._visibilityColumn,
            ColumnKind.Static     => this._staticColumn,
            ColumnKind.Layer      => this._layerColumn,
            ColumnKind.Components => this._componentsColumn,
            _                     => null,
        };

        private void DrawRightColumn(Rect selectionRect, ref Rect curRect, GameObject go)
        {
            float minX = selectionRect.x + 16F;
            var evt = Event.current;

            if (evt.type == EventType.Repaint)
            {
                int drawnUpTo = this._orderedColumns.Count;
                int skipMask = 0;
                for (int i = 0; i < this._orderedColumns.Count; i++)
                {
                    var c = this._orderedColumns[i];
                    if (c.IsEnabled())
                    {
                        var status = c.Layout(go, selectionRect, ref curRect, curRect.x - minX);
                        if (status == LayoutStatus.Failed)
                        {
                            drawnUpTo = i;
                            break;
                        }
                        if (status == LayoutStatus.Skip)
                        {
                            skipMask |= (1 << i);
                            continue;
                        }
                        curRect.x -= IconPadding;
                    }
                    else
                    {
                        c.DisabledHandler(go);
                    }
                }

                for (int i = 0; i < drawnUpTo; i++)
                {
                    if ((skipMask & (1 << i)) != 0) continue;
                    if (this._orderedColumns[i].IsEnabled())
                        this._orderedColumns[i].Draw(go, selectionRect);
                }
            }
            else if (evt.type == EventType.MouseDown
                  || evt.type == EventType.MouseUp
                  || evt.type == EventType.MouseDrag)
            {
                for (int i = 0; i < this._orderedColumns.Count; i++)
                {
                    var c = this._orderedColumns[i];
                    if (!c.IsEnabled()) continue;
                    var status = c.Layout(go, selectionRect, ref curRect, curRect.x - minX);
                    if (status == LayoutStatus.Failed) break;
                    if (status == LayoutStatus.Skip) continue;
                    c.EventHandler(go, evt);
                    curRect.x -= IconPadding;
                }
            }
        }

        #endregion
    }
}
