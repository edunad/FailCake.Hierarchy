using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FailCake.Hierarchy
{
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    public sealed class Folder : MonoBehaviour
    {
        [SerializeField]
        private Color _tint = new Color(1F, 0.82F, 0.33F, 1F);

        [SerializeField]
        private bool _drawRowBackground = true;

        public Color Tint
        {
            get => this._tint;
            set => this._tint = value;
        }

        public bool DrawRowBackground
        {
            get => this._drawRowBackground;
            set => this._drawRowBackground = value;
        }

#if UNITY_EDITOR

        #region PRIVATE

        private bool _iconClearPending;

        #endregion

        private void OnValidate()
        {
            if (Application.isPlaying) return;

            if (this.transform.localPosition != Vector3.zero) this.transform.localPosition = Vector3.zero;
            if (this.transform.localRotation != Quaternion.identity) this.transform.localRotation = Quaternion.identity;
            if (this.transform.localScale != Vector3.one) this.transform.localScale = Vector3.one;

            if (this._iconClearPending) return;
            this._iconClearPending = true;

            var self = this;
            EditorApplication.delayCall += () =>
            {
                if (self == null) return;
                self._iconClearPending = false;
                if (Application.isPlaying) return;
                if (EditorGUIUtility.GetIconForObject(self.gameObject) != null)
                    EditorGUIUtility.SetIconForObject(self.gameObject, null);
            };
        }

#endif
    }
}
