using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FailCake.Hierarchy.Editor
{
    public enum HierarchySetting
    {

        VisibilityShow,
        StaticShow,
        ComponentsShow,
        LayerShow,
        ComponentsOrder,

        SeparatorShow,
        SeparatorColor,
        SeparatorShowRowShading,
        SeparatorEvenRowColor,
        SeparatorOddRowColor,

        FolderShowRowBackground,
        FolderDefaultColor,
        FolderChildTintPercent,

        SmartIconShow,
        DividerShow,

        MissingScriptsShow,
        MissingScriptsTintLabel,
        MissingScriptsColor,

        AdditionalRightIndent,
        AdditionalActiveColor,
        AdditionalInactiveColor,
    }

    public sealed class HierarchySettings
    {
        #region PRIVATE

        private const string PrefsPrefix = "FailCake.Hierarchy.";
        private const string PrefsDark   = "Dark_";
        private const string PrefsLight  = "Light_";

        private static HierarchySettings _instance;

        private readonly HashSet<HierarchySetting> _skinDependent = new();
        private readonly Dictionary<HierarchySetting, object> _defaults = new();
        private readonly Dictionary<HierarchySetting, Action> _listeners = new();

        #endregion

        public static HierarchySettings Instance => _instance ??= new HierarchySettings();

        private HierarchySettings()
        {
            this.Init(HierarchySetting.VisibilityShow,                          true);
            this.Init(HierarchySetting.StaticShow,                              true);
            this.Init(HierarchySetting.ComponentsShow,                          true);
            this.Init(HierarchySetting.LayerShow,                               true);
            this.Init(HierarchySetting.ComponentsOrder,                         "Components,Layer,Static,Visibility");

            this.Init(HierarchySetting.SeparatorShow,                           true);
            this.InitColor(HierarchySetting.SeparatorColor,                     "FF303030", "48666666");
            this.Init(HierarchySetting.SeparatorShowRowShading,                 true);
            this.InitColor(HierarchySetting.SeparatorEvenRowColor,              "13000000", "08000000");
            this.InitColor(HierarchySetting.SeparatorOddRowColor,               "00000000", "00FFFFFF");

            this.Init(HierarchySetting.FolderShowRowBackground,                 true);
            this.InitColor(HierarchySetting.FolderDefaultColor,                 "FFFFD354", "FFFFD354");
            this.Init(HierarchySetting.FolderChildTintPercent,                  35);

            this.Init(HierarchySetting.SmartIconShow,                           true);
            this.Init(HierarchySetting.DividerShow,                             true);

            this.Init(HierarchySetting.MissingScriptsShow,                      true);
            this.Init(HierarchySetting.MissingScriptsTintLabel,                 true);
            this.InitColor(HierarchySetting.MissingScriptsColor,                "4DFF3030", "4DFF3030");

            this.Init(HierarchySetting.AdditionalRightIndent,                   0);
            this.InitColor(HierarchySetting.AdditionalActiveColor,              "FFFFFF80", "CF363636");
            this.InitColor(HierarchySetting.AdditionalInactiveColor,            "FF4F4F4F", "1E000000");
        }

        public bool   GetBool  (HierarchySetting s) => EditorPrefs.GetBool   (this.Key(s), (bool)  this._defaults[s]);
        public int    GetInt   (HierarchySetting s) => EditorPrefs.GetInt    (this.Key(s), (int)   this._defaults[s]);
        public string GetString(HierarchySetting s) => EditorPrefs.GetString (this.Key(s), (string)this._defaults[s]);
        public Color  GetColor (HierarchySetting s) => ColorUtils.FromHex(this.GetString(s));

        public void Set(HierarchySetting s, bool   v, bool fire = true) { EditorPrefs.SetBool  (this.Key(s), v); this.Fire(s, fire); }
        public void Set(HierarchySetting s, int    v, bool fire = true) { EditorPrefs.SetInt   (this.Key(s), v); this.Fire(s, fire); }
        public void Set(HierarchySetting s, string v, bool fire = true) { EditorPrefs.SetString(this.Key(s), v); this.Fire(s, fire); }
        public void SetColor(HierarchySetting s, Color c, bool fire = true)
        {
            EditorPrefs.SetString(this.Key(s), ColorUtils.ToHex(c));
            this.Fire(s, fire);
        }

        public void Restore(HierarchySetting s)
        {
            if (!this._defaults.TryGetValue(s, out var d)) return;
            switch (d)
            {
                case bool   b: this.Set(s, b);   break;
                case int    i: this.Set(s, i);   break;
                case string str: this.Set(s, str); break;
            }
        }

        public void AddListener(HierarchySetting s, Action cb)
        {
            if (!this._listeners.TryAdd(s, cb)) this._listeners[s] += cb;
        }

        public void RemoveListener(HierarchySetting s, Action cb)
        {
            if (this._listeners.TryGetValue(s, out var cur)) this._listeners[s] = cur - cb;
        }

        #region PRIVATE

        private string Key(HierarchySetting s)
        {
            string name = PrefsPrefix;
            if (this._skinDependent.Contains(s)) name += EditorGUIUtility.isProSkin ? PrefsDark : PrefsLight;
            return name + s.ToString("G");
        }

        private void Fire(HierarchySetting s, bool fire)
        {
            if (fire && this._listeners.TryGetValue(s, out var cb)) cb?.Invoke();
            EditorApplication.RepaintHierarchyWindow();
        }

        private void Init(HierarchySetting s, object defaultValue)
        {
            this._defaults[s] = defaultValue;
            if (!EditorPrefs.HasKey(this.Key(s)))
            {
                switch (defaultValue)
                {
                    case bool   b: EditorPrefs.SetBool  (this.Key(s), b); break;
                    case int    i: EditorPrefs.SetInt   (this.Key(s), i); break;
                    case string str: EditorPrefs.SetString(this.Key(s), str); break;
                }
            }
        }

        private void InitColor(HierarchySetting s, string darkHex, string lightHex)
        {
            this._skinDependent.Add(s);
            this.Init(s, EditorGUIUtility.isProSkin ? darkHex : lightHex);
        }

        #endregion
    }
}
