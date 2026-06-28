using UnityEditor;
using UnityEngine;

namespace FailCake.Hierarchy.Editor
{
    internal sealed class FolderTintComponent : HierarchyComponent
    {
        #region PRIVATE

        private const float SelfAlpha = 0.18F;

        private bool _enabled;
        private int  _childPercent;

        #endregion

        public override bool IsEnabled => this._enabled;

        public FolderTintComponent()
        {
            HierarchySettings.Instance.AddListener(HierarchySetting.FolderShowRowBackground, this.Reload);
            HierarchySettings.Instance.AddListener(HierarchySetting.FolderChildTintPercent,  this.Reload);
            this.Reload();
        }

        public override void DrawBackground(HierarchyRowContext ctx)
        {
            var chain = FolderChain.GetFolderChain(ctx.Go);
            if (chain == null || chain.Count == 0) return;

            var sel = ctx.SelectionRect;
            var full = new Rect(0F, sel.y, sel.x + sel.width, sel.height);

            for (int i = chain.Count - 1; i >= 0; i--)
            {
                var f = chain[i];
                if (f == null) continue;

                bool isSelf = (f.gameObject == ctx.Go);
                if (isSelf && !f.DrawRowBackground) continue;

                Color tint = f.Tint;
                tint.a *= isSelf
                    ? SelfAlpha
                    : (this._childPercent / 100F) * SelfAlpha;
                EditorGUI.DrawRect(full, tint);
            }
        }

        /// <summary>
        /// Merges folder tint chain onto <paramref name="baseBG"/> and returns the
        /// opaque composite. Called by other components that need to redraw the
        /// row background locally (icon area, bold label, etc).
        /// </summary>
        public Color Compose(GameObject go, Color baseBG)
        {
            var chain = FolderChain.GetFolderChain(go);
            if (chain == null || chain.Count == 0) return baseBG;

            float aR = 0F, aG = 0F, aB = 0F, aA = 0F;

            for (int i = chain.Count - 1; i >= 0; i--)
            {
                var f = chain[i];
                if (f == null) continue;

                bool isSelf = (f.gameObject == go);
                if (isSelf && !f.DrawRowBackground) continue;

                Color c = f.Tint;
                float alpha = c.a * (isSelf
                    ? SelfAlpha
                    : (this._childPercent / 100F) * SelfAlpha);

                float invA = 1F - alpha;
                aR = c.r * alpha + aR * invA;
                aG = c.g * alpha + aG * invA;
                aB = c.b * alpha + aB * invA;
                aA = alpha + aA * invA;
            }

            if (aA <= 0F) return baseBG;

            float bgMul = 1F - aA;
            return new Color(
                aR + baseBG.r * bgMul,
                aG + baseBG.g * bgMul,
                aB + baseBG.b * bgMul,
                1F);
        }

        #region PRIVATE

        private void Reload()
        {
            this._enabled      = HierarchySettings.Instance.GetBool(HierarchySetting.FolderShowRowBackground);
            this._childPercent = HierarchySettings.Instance.GetInt (HierarchySetting.FolderChildTintPercent);
        }

        #endregion
    }
}
