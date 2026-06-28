using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace FailCake.Hierarchy.Editor
{
    internal static class HierarchyPreferences
    {
        #region PRIVATE

        private static Vector2 _scroll;
        private static readonly GUIContent _colorLabel = new();

        private static ReorderableList _columnOrderList;
        private static List<ColumnKind> _columnOrderBuffer;

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
                    "FailCake", "hierarchy", "folder", "visibility", "static", "components", "icon", "layer", "order", "missing"
                }),
            };
        }

        #region PRIVATE

        private static void DrawGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            EditorGUILayout.Space(4);
            Section("Columns");
            EditorGUILayout.LabelField("Toggle and reorder row columns (top = leftmost in hierarchy).", EditorStyles.miniLabel);
            EditorGUILayout.Space(2);
            EnsureColumnOrderList();
            _columnOrderList.DoLayoutList();

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
            Section("Divider rows");
            ToggleRow(HierarchySetting.DividerShow,             "Render \"---\" names as dividers");
            EditorGUILayout.HelpBox(
                "GameObjects whose name starts with 3+ dashes render as a horizontal divider.\n" +
                "Examples: \"--- Enemies ---\", \"----Lighting\", \"-----\"",
                MessageType.None);

            EditorGUILayout.Space(12);
            Section("Missing scripts");
            ToggleRow(HierarchySetting.MissingScriptsShow,      "Highlight missing scripts");
            ToggleRow(HierarchySetting.MissingScriptsTintLabel, "Tint row background");
            ColorRow (HierarchySetting.MissingScriptsColor,     "Tint color");
            EditorGUILayout.HelpBox(
                "Replaces the GameObject icon with an error icon and tints the row when a MonoBehaviour reference is missing.\n" +
                "Right-click the error icon in the hierarchy to remove the missing scripts.",
                MessageType.None);

            EditorGUILayout.Space(12);
            Section("Additional");
            IntRow   (HierarchySetting.AdditionalRightIndent,           "Right-side indent (px)", 0, 64);
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
                    InvalidateColumnOrderList();
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private static void EnsureColumnOrderList()
        {
            if (_columnOrderList != null) return;

            _columnOrderBuffer = ParseColumnOrder(HierarchySettings.Instance.GetString(HierarchySetting.ComponentsOrder));

            _columnOrderList = new ReorderableList(_columnOrderBuffer, typeof(ColumnKind), true, false, false, false)
            {
                headerHeight = 0,
                elementHeight = EditorGUIUtility.singleLineHeight + 4,
                drawElementCallback = DrawColumnOrderElement,
                onReorderCallback = _ => SaveColumnOrder(),
            };
        }

        private static void InvalidateColumnOrderList()
        {
            _columnOrderList = null;
            _columnOrderBuffer = null;
        }

        private static void DrawColumnOrderElement(Rect rect, int index, bool active, bool focused)
        {
            if (index < 0 || index >= _columnOrderBuffer.Count) return;
            var kind = _columnOrderBuffer[index];

            rect.y += 2;
            rect.height = EditorGUIUtility.singleLineHeight;

            const float toggleWidth = 20F;
            var toggleRect = new Rect(rect.x, rect.y, toggleWidth, rect.height);
            var labelRect  = new Rect(rect.x + toggleWidth, rect.y, rect.width - toggleWidth, rect.height);

            var setting = KindToToggleSetting(kind);
            bool current = HierarchySettings.Instance.GetBool(setting);
            bool updated = EditorGUI.Toggle(toggleRect, current);
            if (updated != current) HierarchySettings.Instance.Set(setting, updated);

            EditorGUI.LabelField(labelRect, DisplayName(kind));
        }

        private static void SaveColumnOrder()
        {
            if (_columnOrderBuffer == null || _columnOrderBuffer.Count == 0) return;
            var parts = new string[_columnOrderBuffer.Count];
            for (int i = 0; i < _columnOrderBuffer.Count; i++)
                parts[i] = _columnOrderBuffer[i].ToString();
            HierarchySettings.Instance.Set(HierarchySetting.ComponentsOrder, string.Join(",", parts));
        }

        private static List<ColumnKind> ParseColumnOrder(string raw)
        {
            var result = new List<ColumnKind>(4);
            var seen = new HashSet<ColumnKind>();

            if (!string.IsNullOrEmpty(raw))
            {
                foreach (var token in raw.Split(','))
                {
                    if (System.Enum.TryParse<ColumnKind>(token.Trim(), out var kind) && seen.Add(kind))
                        result.Add(kind);
                }
            }

            foreach (ColumnKind kind in System.Enum.GetValues(typeof(ColumnKind)))
                if (seen.Add(kind)) result.Add(kind);

            return result;
        }

        private static HierarchySetting KindToToggleSetting(ColumnKind kind) => kind switch
        {
            ColumnKind.Visibility => HierarchySetting.VisibilityShow,
            ColumnKind.Static     => HierarchySetting.StaticShow,
            ColumnKind.Layer      => HierarchySetting.LayerShow,
            ColumnKind.Components => HierarchySetting.ComponentsShow,
            _                     => HierarchySetting.ComponentsShow,
        };

        private static string DisplayName(ColumnKind kind) => kind switch
        {
            ColumnKind.Visibility => "Visibility icon",
            ColumnKind.Static     => "Static flags indicator",
            ColumnKind.Layer      => "Layer name",
            ColumnKind.Components => "Components row",
            _                     => kind.ToString(),
        };

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
