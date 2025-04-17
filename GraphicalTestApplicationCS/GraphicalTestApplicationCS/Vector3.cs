using System;

namespace CustomDataTypesCS
{
    public struct Vector3(float x = 0, float y = 0, float z = 0)
    {
        // Public fields represent the x, y and z components.
        public float x = x, y = y, z = z;

        // Operator overloads to enable intuitive vector arithmetic.
        public static Vector3 operator +(Vector3 a, Vector3 b) =>
            new(a.x + b.x, a.y + b.y, a.z + b.z);

        public static Vector3 operator -(Vector3 a, Vector3 b) =>
            new(a.x - b.x, a.y - b.y, a.z - b.z);

        public static Vector3 operator *(Vector3 a, float f) =>
            new(a.x * f, a.y * f, a.z * f);

        public static Vector3 operator *(float f, Vector3 a) => a * f;

        // Returns the dot product of this vector with another.
        public readonly float Dot(Vector3 other)
        {
            return x * other.x + y * other.y + z * other.z;
        }

        // Returns the cross product (result is perpendicular to both vectors).
        public readonly Vector3 Cross(Vector3 other)
        {
            return new Vector3(
                y * other.z - z * other.y,
                z * other.x - x * other.z,
                x * other.y - y * other.x
            );
        }

        // Returns the magnitude (length) of the vector.
        public readonly float Magnitude()
        {
            return (float)Math.Sqrt(x * x + y * y + z * z);
        }

        // Normalises the vector to unit length, if possible.
        public void Normalise()
        {
            float mag = Magnitude();
            if (mag > 0f)
            {
                x /= mag;
                y /= mag;
                z /= mag;
            }
        }
    }
}
