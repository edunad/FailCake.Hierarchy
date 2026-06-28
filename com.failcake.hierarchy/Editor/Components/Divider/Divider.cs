using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FailCake.Hierarchy.Editor
{
    internal static class Divider
    {
        #region PRIVATE

        private const int MinDashes = 3;
        private const int MaxCacheEntries = 64;
        private const float LabelPadding = 6F;
        private const float LineInset = 4F;

        private static GUIStyle _labelStyle;
        private static readonly GUIContent _labelContent = new();

        private struct Cached
        {
            public string Label;
            public float Width;
        }

        private static readonly Dictionary<string, Cached> _cache = new(32);

        #endregion

        public static bool IsDivider(GameObject go)
        {
            if (go == null) return false;
            string n = go.name;
            if (n == null || n.Length < MinDashes) return false;
            if (n[0] != '-' || n[1] != '-' || n[2] != '-') return false;
            return true;
        }

        public static void Draw(GameObject go, Rect selectionRect, Color rowBG, Color lineColor)
        {
            EnsureStyle();

            var cached = GetCached(go.name);

            var full = new Rect(
                0F,
                selectionRect.y,
                selectionRect.x + selectionRect.width,
                selectionRect.height);
            EditorGUI.DrawRect(full, rowBG);

            float cy = Mathf.Floor(selectionRect.y + selectionRect.height * 0.5F);
            var lineRect = new Rect(full.x + LineInset, cy, full.width - LineInset * 2F, 1F);

            if (string.IsNullOrEmpty(cached.Label))
            {
                EditorGUI.DrawRect(lineRect, lineColor);
                return;
            }

            float labelX = full.x + (full.width - cached.Width) * 0.5F;

            var leftLine = new Rect(
                lineRect.x,
                lineRect.y,
                Mathf.Max(0F, labelX - LabelPadding - lineRect.x),
                1F);
            float rightLineStart = labelX + cached.Width + LabelPadding;
            var rightLine = new Rect(
                rightLineStart,
                lineRect.y,
                Mathf.Max(0F, (lineRect.x + lineRect.width) - rightLineStart),
                1F);

            if (leftLine.width  > 0F) EditorGUI.DrawRect(leftLine,  lineColor);
            if (rightLine.width > 0F) EditorGUI.DrawRect(rightLine, lineColor);

            _labelContent.text = cached.Label;
            var labelRect = new Rect(labelX, full.y, cached.Width, full.height);
            GUI.Label(labelRect, _labelContent, _labelStyle);
        }

        #region PRIVATE

        private static Cached GetCached(string source)
        {
            if (source == null) source = string.Empty;
            if (_cache.TryGetValue(source, out var hit)) return hit;

            string label = ExtractLabel(source);
            float width = 0F;
            if (!string.IsNullOrEmpty(label))
            {
                _labelContent.text = label;
                width = _labelStyle.CalcSize(_labelContent).x;
            }

            if (_cache.Count >= MaxCacheEntries) _cache.Clear();

            var entry = new Cached { Label = label, Width = width };
            _cache[source] = entry;
            return entry;
        }

        private static string ExtractLabel(string name)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;

            int start = 0;
            int len = name.Length;
            while (start < len && (name[start] == '-' || name[start] == ' ' || name[start] == '\t'))
                start++;

            int end = len - 1;
            while (end >= start && (name[end] == '-' || name[end] == ' ' || name[end] == '\t'))
                end--;

            if (end < start) return string.Empty;
            if (start == 0 && end == len - 1) return name;
            return name.Substring(start, end - start + 1);
        }

        private static void EnsureStyle()
        {
            if (_labelStyle != null) return;
            _labelStyle = new GUIStyle(EditorStyles.miniBoldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(0, 0, 0, 0),
                margin  = new RectOffset(0, 0, 0, 0),
                clipping = TextClipping.Clip,
            };
        }

        #endregion
    }
}
