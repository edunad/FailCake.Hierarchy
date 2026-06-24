using System;
using System.Globalization;
using UnityEngine;

namespace FailCake.Hierarchy.Editor
{
    internal static class ColorUtils
    {
        #region PRIVATE

        private static Color _defaultColor = Color.white;

        #endregion

        public static void SetDefaultColor(Color c) => _defaultColor = c;

        public static void SetColor(Color c) => GUI.color = c;

        public static void ClearColor() => GUI.color = _defaultColor;

        public static Color FromHex(string hex)
        {
            if (string.IsNullOrEmpty(hex)) return Color.white;
            try
            {
                uint value = Convert.ToUInt32(hex, 16);
                return new Color(
                    ((value >> 16) & 0xFF) / 255F,
                    ((value >>  8) & 0xFF) / 255F,
                    ((value >>  0) & 0xFF) / 255F,
                    ((value >> 24) & 0xFF) / 255F);
            }
            catch
            {
                return Color.white;
            }
        }

        public static string ToHex(Color c)
        {
            uint r = (uint)Mathf.Clamp(Mathf.RoundToInt(c.r * 255F), 0, 255);
            uint g = (uint)Mathf.Clamp(Mathf.RoundToInt(c.g * 255F), 0, 255);
            uint b = (uint)Mathf.Clamp(Mathf.RoundToInt(c.b * 255F), 0, 255);
            uint a = (uint)Mathf.Clamp(Mathf.RoundToInt(c.a * 255F), 0, 255);
            uint value = (a << 24) | (r << 16) | (g << 8) | b;
            return value.ToString("X8", CultureInfo.InvariantCulture);
        }
    }
}
