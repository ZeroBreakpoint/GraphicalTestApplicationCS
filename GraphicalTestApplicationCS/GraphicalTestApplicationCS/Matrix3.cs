using System;

namespace CustomDataTypesCS
{
    public struct Matrix3
    {
        // Flat array to hold 9 elements: row-major order.
        public float[] m;

        // Constructor initialises matrix elements directly.
        public Matrix3(
            float m00, float m01, float m02,
            float m10, float m11, float m12,
            float m20, float m21, float m22)
        {
            m = new float[9];
            m[0] = m00; m[1] = m01; m[2] = m02;
            m[3] = m10; m[4] = m11; m[5] = m12;
            m[6] = m20; m[7] = m21; m[8] = m22;
        }

        // Matrix multiplication operator: a * b.
        public static Matrix3 operator *(Matrix3 a, Matrix3 b)
        {
            var result = new Matrix3
            {
                m = new float[9]
            };

            // Compute each cell via dot product of row and column.
            for (int row = 0; row < 3; row++)
                for (int col = 0; col < 3; col++)
                    for (int k = 0; k < 3; k++)
                        result.m[row * 3 + col] += a.m[row * 3 + k] * b.m[k * 3 + col];

            return result;
        }

        // Multiplies this matrix by a Vector3 (treating z as 1 for 2D transforms).
        public readonly Vector3 Multiply(Vector3 v) =>
            new(
                m[0] * v.x + m[1] * v.y + m[2] * v.z,
                m[3] * v.x + m[4] * v.y + m[5] * v.z,
                m[6] * v.x + m[7] * v.y + m[8] * v.z
            );

        // Sets this matrix to a rotation about the Z axis (2D rotation).
        public void SetRotateZ(float radians)
        {
            // Initialise array if needed.
            if (m == null || m.Length != 9)
                m = new float[9];

            float cos = (float)Math.Cos(radians);
            float sin = (float)Math.Sin(radians);

            // Row-major assignment for rotation matrix.
            m[0] = cos; m[1] = -sin; m[2] = 0f;
            m[3] = sin; m[4] = cos; m[5] = 0f;
            m[6] = 0f; m[7] = 0f; m[8] = 1f;
        }
    }
}

