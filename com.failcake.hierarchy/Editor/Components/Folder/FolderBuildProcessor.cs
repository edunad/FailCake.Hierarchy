using System.Collections.Generic;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FailCake.Hierarchy.Editor
{

    public sealed class FolderBuildProcessor : IProcessSceneWithReport
    {

        public int callbackOrder => 0;

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            var folders = new List<Folder>();
            foreach (var root in scene.GetRootGameObjects())
            {
                folders.AddRange(root.GetComponentsInChildren<Folder>(includeInactive: true));
            }
            if (folders.Count == 0) return;

            folders.Sort((a, b) => Depth(b.transform).CompareTo(Depth(a.transform)));

            foreach (var folder in folders)
            {
                if (folder == null || folder.gameObject == null) continue;
                FlattenInto(folder);
            }
        }

        #region PRIVATE

        private static int Depth(Transform t)
        {
            int d = 0;
            while (t.parent != null) { d++; t = t.parent; }
            return d;
        }

        private static void FlattenInto(Folder folder)
        {
            var folderTransform = folder.transform;
            var parent = folderTransform.parent;

            int insertAt = folderTransform.GetSiblingIndex();

            int count = folderTransform.childCount;
            var children = new Transform[count];
            for (int i = 0; i < count; i++)
            {
                children[i] = folderTransform.GetChild(i);
            }

            for (int i = 0; i < count; i++)
            {
                var child = children[i];
                if (child == null) continue;

                child.SetParent(parent, worldPositionStays: true);
                child.SetSiblingIndex(insertAt + i);
            }

            Object.DestroyImmediate(folder.gameObject);
        }

        #endregion
    }
}
