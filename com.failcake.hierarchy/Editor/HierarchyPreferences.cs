using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FailCake.Hierarchy.Editor
{
    internal static class HierarchyPreferences
    {
        #region PRIVATE

        private static Vector2 _scroll;
        private static readonly GUIContent _colorLabel = new();

        #endregion

        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            return new SettingsProvider("Preferences/FailCake/Hierarchy", SettingsScope.User)
            {
                label = "Hierarchy",
                guiHandler = _ => DrawGUI(),
                keywords = new HashSet<string>(new[]
                {
                    "FailCake", "hierarchy", "folder", "visibility", "static", "components", "icon"
                }),
            };
        }

        #region PRIVATE

        private static void DrawGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            EditorGUILayout.Space(4);
            Section("Components");
            ToggleRow(HierarchySetting.VisibilityShow,  "Visibility icon");
            ToggleRow(HierarchySetting.StaticShow,      "Static flags indicator");
            ToggleRow(HierarchySetting.ComponentsShow,  "Components row");

            EditorGUILayout.Space(12);
            Section("Row separator & shading");
            ToggleRow(HierarchySetting.SeparatorShow,          "Separator line");
            ColorRow (HierarchySetting.SeparatorColor,         "Separator color");
            ToggleRow(HierarchySetting.SeparatorShowRowShading,"Zebra row shading");
            ColorRow (HierarchySetting.SeparatorEvenRowColor,  "Even row color");
            ColorRow (HierarchySetting.SeparatorOddRowColor,   "Odd row color");

            EditorGUILayout.Space(12);
            Section("Folders");
            ToggleRow(HierarchySetting.FolderShowRowBackground, "Tint folder row background");
            IntRow   (HierarchySetting.FolderChildTintPercent,  "Descendant tint strength (%)", 0, 100);
            ColorRow (HierarchySetting.FolderDefaultColor,      "Default folder color");

            EditorGUILayout.Space(12);
            Section("Smart GameObject icon");
            ToggleRow(HierarchySetting.SmartIconShow,           "Replace default cube icon");
            EditorGUILayout.HelpBox(
                "Transform only → Transform icon\n" +
                "Transform + 1 component → that component's icon\n" +
                "Transform + 2+ components → default cube",
                MessageType.None);

            EditorGUILayout.Space(12);
            Section("Additional");
            IntRow   (HierarchySetting.AdditionalRightIndent,           "Right-side indent (px)", 0, 64);
            ToggleRow(HierarchySetting.AdditionalShowModifierWarning,   "Confirm Shift/Alt bulk actions");
            ColorRow (HierarchySetting.AdditionalActiveColor,           "Active color");
            ColorRow (HierarchySetting.AdditionalInactiveColor,         "Inactive color");

            EditorGUILayout.Space(16);
            if (GUILayout.Button("Restore All Defaults", GUILayout.Height(24)))
            {
                if (EditorUtility.DisplayDialog("Restore FailCake.Hierarchy defaults",
                    "Reset every FailCake.Hierarchy setting to its default value?", "Reset", "Cancel"))
                {
                    foreach (HierarchySetting s in System.Enum.GetValues(typeof(HierarchySetting)))
                        HierarchySettings.Instance.Restore(s);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private static void Section(string title) => EditorGUILayout.LabelField(title, EditorStyles.boldLabel);

        private static void ToggleRow(HierarchySetting s, string label)
        {
            bool current = HierarchySettings.Instance.GetBool(s);
            bool updated = EditorGUILayout.Toggle(label, current);
            if (updated != current) HierarchySettings.Instance.Set(s, updated);
        }

        private static void IntRow(HierarchySetting s, string label, int min, int max)
        {
            int current = HierarchySettings.Instance.GetInt(s);
            int updated = EditorGUILayout.IntSlider(label, current, min, max);
            if (updated != current) HierarchySettings.Instance.Set(s, updated);
        }

        private static void ColorRow(HierarchySetting s, string label)
        {
            _colorLabel.text = label;
            Color current = HierarchySettings.Instance.GetColor(s);
            Color updated = EditorGUILayout.ColorField(_colorLabel, current, true, true, false);
            if (updated != current) HierarchySettings.Instance.SetColor(s, updated);
        }

        #endregion
    }
}
