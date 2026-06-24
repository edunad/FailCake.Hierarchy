using UnityEditor;
using UnityEngine;

namespace FailCake.Hierarchy.Editor
{

    [CustomEditor(typeof(Folder))]
    [CanEditMultipleObjects]
    internal sealed class FolderInspector : UnityEditor.Editor
    {
        #region PRIVATE

        private SerializedProperty _tintProp;
        private SerializedProperty _drawBgProp;

        #endregion

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            EditorGUILayout.HelpBox(
                "This GameObject is a FailCake.Hierarchy folder. It will be stripped on build — its children will be reparented up one level. Folders keep an identity transform.",
                MessageType.Info);

            EditorGUILayout.PropertyField(this._tintProp,   new GUIContent("Tint"));
            EditorGUILayout.PropertyField(this._drawBgProp, new GUIContent("Row Background"));

            if (GUILayout.Button("Flatten Now"))
            {
                foreach (var t in this.targets)
                {
                    if (t is Folder f && f != null) Flatten(f);
                }
            }

            this.serializedObject.ApplyModifiedProperties();
        }

        #region PRIVATE

        private void OnEnable()
        {
            this._tintProp   = this.serializedObject.FindProperty("_tint");
            this._drawBgProp = this.serializedObject.FindProperty("_drawRowBackground");

            this.HideTransforms(true);
        }

        private void OnDisable()
        {
            this.HideTransforms(false);
        }

        private static void Flatten(Folder folder)
        {
            var parent = folder.transform.parent;
            Undo.SetCurrentGroupName("Flatten Folder");
            int group = Undo.GetCurrentGroup();

            var children = new Transform[folder.transform.childCount];
            for (int i = 0; i < children.Length; i++) children[i] = folder.transform.GetChild(i);

            int baseSibling = folder.transform.GetSiblingIndex();
            for (int i = 0; i < children.Length; i++)
            {
                Undo.SetTransformParent(children[i], parent, "Flatten Folder");
                children[i].SetSiblingIndex(baseSibling + i);
            }
            Undo.DestroyObjectImmediate(folder.gameObject);
            Undo.CollapseUndoOperations(group);
        }

        private void HideTransforms(bool hide)
        {
            foreach (var t in this.targets)
            {
                if (t is Folder folder && folder != null && folder.transform != null)
                {
                    if (hide)  folder.transform.hideFlags |= HideFlags.HideInInspector;
                    else       folder.transform.hideFlags &= ~HideFlags.HideInInspector;
                }
            }
        }

        #endregion
    }
}
