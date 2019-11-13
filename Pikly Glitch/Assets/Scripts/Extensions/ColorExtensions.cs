using UnityEngine;

namespace Pikl.Extensions {
    /**
		Provides some utility functions for Colors.
	*/
    public static class ColorExtensions {
        private const float LightOffset = 0.125f;
        private const float DarkerFactor = 0.9f;

        /**
			Returns a color lighter than the given color.
		*/
        public static Color Lighter(this Color color) {
            return new Color(
                color.r + LightOffset,
                color.g + LightOffset,
                color.b + LightOffset,
                color.a);
        }

        /**
			Returns a color darker than the given color.
		*/
        public static Color Darker(this Color color, float offset = LightOffset) {
            return new Color(
                color.r - offset,
                color.g - offset,
                color.b - offset,
                color.a);
        }

        /**
			Returns the brightness of the color, 
			defined as the average off the three color channels.
		*/
        public static float Brightness(this Color color) {
            return (color.r + color.g + color.b) / 3;
        }

        /**
			Returns a new color with the RGB values scaled so that the color 
			has the given brightness. 

			If the color is too dark, a grey is returned with the right brighness.

			The alpha is left uncanged.
		*/
        static float wbFactor, wbR, wbG, wbB, wbA;
        public static Color WithBrightness(this Color color, float brightness) {
            if (color.IsApproximatelyBlack()) {
                return new Color(brightness, brightness, brightness, color.a);
            }

            wbFactor = brightness / color.Brightness();

            float wbR = color.r * wbFactor;
            float wbG = color.g * wbFactor;
            float wbB = color.b * wbFactor;

            float wbA = color.a;

            return new Color(wbR, wbG, wbB, wbA);
        }

        /**
			Returns whether the color is black or almost black.
		*/
        public static bool IsApproximatelyBlack(this Color color) {
            return color.r + color.g + color.b <= Mathf.Epsilon;
        }

        /**
			Returns whether the color is white or almost white.
		*/
        public static bool IsApproximatelyWhite(this Color color) {
            return color.r + color.g + color.b >= 1 - Mathf.Epsilon;
        }

        /**
			Returns an opaque version of the given color.
		*/
        public static Color Opaque(this Color color) {
            return new Color(color.r, color.g, color.b);
        }

        /// <summary>
        /// Returns the colour, but with specified Alpha.
        /// </summary>
        public static Color Alpha(this Color color, float alpha = 0) {
            color.a = Mathf.Clamp01(alpha);
            return color;
        }
    }
}
