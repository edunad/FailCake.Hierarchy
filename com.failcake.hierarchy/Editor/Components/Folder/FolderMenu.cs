using UnityEditor;
using UnityEngine;

namespace FailCake.Hierarchy.Editor
{

    internal static class FolderMenu
    {
        #region PRIVATE

        private const string CreateFolderMenu  = "GameObject/Create Folder";
        private const string GroupInFolderMenu = "GameObject/Group in Folder";
        private const int CreatePriority = 0;
        private const int GroupPriority  = 1;

        #endregion

        [MenuItem(CreateFolderMenu + " %#&n", false, CreatePriority)]
        private static void CreateFolder(MenuCommand cmd)
        {

            if (cmd.context != null && cmd.context != Selection.activeGameObject) return;

            GameObject parent = cmd.context as GameObject;
            var folder = CreateFolderGameObject("Folder", parent);
            Selection.activeGameObject = folder;
            EditorGUIUtility.PingObject(folder);

            var evt = EditorWindow.focusedWindow;
            if (evt != null) evt.SendEvent(EditorGUIUtility.CommandEvent("Rename"));
        }

        [MenuItem(GroupInFolderMenu + " %#g", true, GroupPriority)]
        private static bool ValidateGroupInFolder() => Selection.gameObjects.Length > 0;

        [MenuItem(GroupInFolderMenu + " %#g", false, GroupPriority)]
        private static void GroupInFolder(MenuCommand cmd)
        {
            if (cmd.context != null && cmd.context != Selection.activeGameObject) return;

            var selected = Selection.gameObjects;
            if (selected == null || selected.Length == 0) return;

            Transform commonParent = selected[0].transform.parent;
            for (int i = 1; i < selected.Length; i++)
            {
                if (selected[i].transform.parent != commonParent)
                {
                    commonParent = null;
                    break;
                }
            }

            Undo.SetCurrentGroupName("Group in Folder");
            int group = Undo.GetCurrentGroup();

            var folder = CreateFolderGameObject("Folder", commonParent != null ? commonParent.gameObject : null);

            int minSibling = int.MaxValue;
            foreach (var s in selected)
            {
                if (s.transform.parent == commonParent)
                    minSibling = Mathf.Min(minSibling, s.transform.GetSiblingIndex());
            }
            if (minSibling != int.MaxValue) folder.transform.SetSiblingIndex(minSibling);

            foreach (var s in selected)
            {
                if (s == folder) continue;
                Undo.SetTransformParent(s.transform, folder.transform, "Group in Folder");
            }

            Selection.activeGameObject = folder;
            Undo.CollapseUndoOperations(group);

            var evt = EditorWindow.focusedWindow;
            if (evt != null) evt.SendEvent(EditorGUIUtility.CommandEvent("Rename"));
        }

        #region PRIVATE

        private static GameObject CreateFolderGameObject(string name, GameObject parent)
        {
            var go = new GameObject(name);
            go.AddComponent<Folder>();

            if (parent != null)
            {
                go.transform.SetParent(parent.transform, worldPositionStays: false);
            }
            else
            {
                var activeScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
                if (go.scene != activeScene && activeScene.IsValid())
                    UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go, activeScene);
            }

            GameObjectUtility.EnsureUniqueNameForSibling(go);
            Undo.RegisterCreatedObjectUndo(go, "Create Folder");
            return go;
        }

        #endregion
    }
}
