using UnityEditor;
using UnityEngine;

namespace FailCake.Hierarchy.Editor
{
    internal sealed class FolderIconComponent : HierarchyComponent
    {
        public override int Priority => 10;

        public override bool DrawIcon(HierarchyRowContext ctx)
        {
            if (!ctx.IsFolder) return false;

            var sel = ctx.SelectionRect;
            var iconArea = new Rect(
                Mathf.Floor(sel.x),
                Mathf.Floor(sel.y),
                16F, sel.height);
            EditorGUI.DrawRect(iconArea, ctx.GetCompositeRowBG());

            bool expanded = FolderExpansion.IsFolderExpanded(ctx.InstanceId);
            var icon = expanded ? Icons.FolderOpenIcon : Icons.FolderClosedIcon;
            if (icon != null)
                GUI.DrawTexture(new Rect(iconArea.x, iconArea.y, 16F, 16F), icon, ScaleMode.ScaleToFit);
            return true;
        }
    }
}
