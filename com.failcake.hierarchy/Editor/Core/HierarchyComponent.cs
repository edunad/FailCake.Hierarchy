using UnityEngine;

namespace FailCake.Hierarchy.Editor
{
    /// <summary>
    /// Per-row state passed to HierarchyComponent phases. This instance is reused
    /// across every row, so do not retain references to it from outside the callbacks.
    /// </summary>
    public sealed class HierarchyRowContext
    {
        public GameObject Go;
        public int        InstanceId;
        public Rect       SelectionRect;
        public bool       IsRepaint;
        public Event      Evt;
        public bool       IsFolder;

        /// <summary>Set to true once a component has claimed the GameObject icon slot.</summary>
        public bool IconClaimed;

        /// <summary>Set to true once a component has fully consumed the row.</summary>
        public bool RowConsumed;

        internal HierarchyDrawer Drawer;

        public Color GetRowBG()          => HierarchyRowUtil.GetRowBG(this.Go);
        public Color GetCompositeRowBG() => this.Drawer.ComposeRowBG(this.Go);
        public bool  IsRenaming()        => HierarchyRowUtil.IsRenamingRow(this.Go);
    }

    /// <summary>
    /// Plugin base for hierarchy row decorations. Each component can participate in
    /// any subset of the phases below. Phases run in this order per row:
    ///
    ///   1. TryConsumeRow   — short-circuits all other phases (e.g. Divider).
    ///   2. DrawBackground  — full-row fills (separator, folder tint, row tints).
    ///   3. DrawIcon        — GameObject icon slot; first claim wins (priority-ordered).
    ///   4. (right-side columns — not a component phase.)
    ///   5. DrawLabel       — label overlays (e.g. bold folder name).
    ///   6. DrawOverlay     — final decorations.
    ///   7. HandleEvent     — non-repaint events.
    ///
    /// Higher <see cref="Priority"/> runs first within a phase. In practice only
    /// <see cref="DrawIcon"/> uses the priority ordering; the other phases iterate in
    /// registration order.
    /// </summary>
    public abstract class HierarchyComponent
    {
        public virtual int  Priority  => 0;
        public virtual bool IsEnabled => true;

        /// <summary>Called when the hierarchy changes or undo/redo runs.</summary>
        public virtual void InvalidateCaches() { }

        public virtual bool TryConsumeRow (HierarchyRowContext ctx) => false;
        public virtual void DrawBackground(HierarchyRowContext ctx) { }

        /// <summary>Return true to claim the GameObject icon slot.</summary>
        public virtual bool DrawIcon      (HierarchyRowContext ctx) => false;

        public virtual void DrawLabel    (HierarchyRowContext ctx) { }
        public virtual void DrawOverlay  (HierarchyRowContext ctx) { }
        public virtual void HandleEvent  (HierarchyRowContext ctx) { }
    }
}
