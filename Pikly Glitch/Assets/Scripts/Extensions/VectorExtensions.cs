using UnityEngine;

namespace Pikl.Extensions {
    /**
		Contains useful extensions for vectors.
		
		@author Herman Tulleken

		@version1_0
	*/
    public static class VectorExtensions {
        public static Vector3 To3DXZ(this Vector2 vector, float y) {
            return new Vector3(vector.x, y, vector.y);
        }

        public static Vector3 To3DXZ(this Vector2 vector) {
            return vector.To3DXZ(0);
        }

        public static Vector3 To3DXY(this Vector2 vector, float z) {
            return new Vector3(vector.x, vector.y, z);
        }

        public static Vector3 To3DXY(this Vector2 vector) {
            return vector.To3DXY(0);
        }

        public static Vector3 To3DYZ(this Vector2 vector, float x) {
            return new Vector3(x, vector.x, vector.y);
        }

        public static Vector3 To3DYZ(this Vector2 vector) {
            return vector.To3DYZ(0);
        }

        public static Vector2 To2DXZ(this Vector3 vector) {
            return new Vector2(vector.x, vector.z);
        }

        public static Vector2 To2DXY(this Vector3 vector) {
            return new Vector2(vector.x, vector.y);
        }

        public static Vector2 To2DYZ(this Vector3 vector) {
            return new Vector2(vector.y, vector.z);
        }

        /**
			Returns the vector rotated 90 degrees counter-clockwise. This vector is
			always perpendicular to the given vector.

			The perp dot product can be caluclted using this:
				var perpDotPorpduct = Vector2.Dot(v1.Perp(), v2);
		*/
        public static Vector2 Perp(this Vector2 vector) {
            return new Vector2(-vector.y, vector.x);
        }

        /**
			Returns the projection of this vector onto the given base.
		*/
        public static Vector2 Proj(this Vector2 vector, Vector2 baseVector) {
            var direction = baseVector.normalized;
            var magnitude = Vector2.Dot(vector, direction);

            return direction * magnitude;
        }

        /**
			Returns the rejection of this vector onto the given base.

			The sum of a vector's projection and rejection on a base is
			equal to the original vector.
		*/
        public static Vector2 Rej(this Vector2 vector, Vector2 baseVector) {
            return vector - vector.Proj(baseVector);
        }

        /**
			Returns the projection of this vector onto the given base.
		*/
        public static Vector3 Proj(this Vector3 vector, Vector3 baseVector) {
            var direction = baseVector.normalized;
            var magnitude = Vector2.Dot(vector, direction);

            return direction * magnitude;
        }

        /**
			Returns the rejection of this vector onto the given base.

			The sum of a vector's projection and rejection on a base is
			equal to the original vector.
		*/
        public static Vector3 Rej(this Vector3 vector, Vector3 baseVector) {
            return vector - vector.Proj(baseVector);
        }

        /**
			Returns the projection of this vector onto the given base.
		*/
        public static Vector4 Proj(this Vector4 vector, Vector4 baseVector) {
            var direction = baseVector.normalized;
            var magnitude = Vector2.Dot(vector, direction);

            return direction * magnitude;
        }

        /**
			Returns the rejection of this vector onto the given base.

			The sum of a vector's projection and rejection on a base is
			equal to the original vector.
		*/
        public static Vector4 Rej(this Vector4 vector, Vector4 baseVector) {
            return vector - vector.Proj(baseVector);
        }

        public static Vector3 PerpXZ(this Vector3 v) {
            return new Vector3(-v.z, v.y, v.x);
        }

        public static Vector3 PerpXY(this Vector3 v) {
            return new Vector3(-v.y, v.x, v.z);
        }

        public static Vector2 Rotate(this Vector2 v, float degrees) {
            degrees = Mathf.Deg2Rad * degrees;
            return new Vector2(v.x * Mathf.Cos(degrees) - v.y * Mathf.Sin(degrees), v.x * Mathf.Sin(degrees) + v.y * Mathf.Cos(degrees));
        }
        public static Vector3 Rotate(this Vector3 v, float degrees) {
            degrees = Mathf.Deg2Rad * degrees;
            return new Vector2(v.x * Mathf.Cos(degrees) - v.y * Mathf.Sin(degrees), v.x * Mathf.Sin(degrees) + v.y * Mathf.Cos(degrees));
        }

        /// <summary>
        /// Converts a vector2 into an angle in degrees
        /// </summary>
        public static float ToAngle(this Vector2 v) {
            v = v.normalized;
            float angleRadians = Mathf.Atan2(v.y, v.x);
            float angle = angleRadians * Mathf.Rad2Deg;
            if (angle < 0)
                angle += 360;

            return angle;
        }

        /// <summary>
        /// Converts a vector3 into an angle in degrees - for 2D
        /// </summary>
        public static float ToAngle(this Vector3 v) {
            v = v.normalized;
            float angleRadians = Mathf.Atan2(v.y, v.x);
            float angle = angleRadians * Mathf.Rad2Deg;
            if (angle < 0)
                angle += 360;

            return angle;
        }

        /// <summary>
        /// Returns the angle from one z to another.
        /// </summary>
        public static float AngleTo(this Vector3 from, Vector3 to) {
            float phi = Mathf.Abs(to.z - from.z) % 360;       // This is either the distance or 360 - distance
            float distance = phi > 180 ? 360 - phi : phi;
            return distance;
        }

        ///// <summary>
        ///// Returns a proper angle from 0-360 between two vectors using z axis, none of that clamped-at-180 bullshit.
        ///// </summary>
        //public static float Angle360(Vector3 from, Vector3 to) {
        //    float angle = from.z - to.z;
        //    angle = angle < 0 ? 360 - Mathf.Abs(angle) : angle;
        //    //Debug.HBDebug.Log("Angle : " + angle);
        //    return angle;
        //}

        //public static float SignedAngle(Vector3 a, Vector3 b, Vector3 n) {
        //    // angle in [0,180]
        //    float angle = Vector3.Angle(a, b);
        //    float sign = Mathf.Sign(Vector3.Dot(n, Vector3.Cross(a, b)));

        //    // angle in [-179,180]
        //    float signed_angle = angle * sign;

        //    // angle in [0,360] (not used but included here for completeness)
        //    float angle360 = (signed_angle + 360) % 360;

        //    Debug.HBDebug.Log("Angle : " + angle360);

        //    return angle360;
        //}
        
        public static Vector2 Abs (this Vector2 v) {
            return new Vector2(Mathf.Abs(v.x), Mathf.Abs(v.y));
        }

    }
}