using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Numerics;          
using CustomDataTypesCS;       

using Rectangle = Raylib_cs.Rectangle;

namespace GraphicalTestApplicationCS
{
    public class Bullet(CustomDataTypesCS.Vector3 pos, float angleRad, Texture2D tex)
    {
        // Current position in world space.
        private CustomDataTypesCS.Vector3 position = pos;

        // Angle of travel in radians.
        private readonly float angle = angleRad;

        // Movement speed (units per frame).
        private readonly float speed = 6f;

        // Texture for rendering the bullet sprite.
        private Texture2D texture = tex;

        // Convenience properties for collision checks and drawing.
        public float X => position.x;
        public float Y => position.y;

        // Updates position along the angle at constant speed.
        public void Update()
        {
            // Build a rotation matrix from the bullet’s angle
            var rotMat = new Matrix3();
            rotMat.SetRotateZ(angle);

            // Local forward step is along +X by 'speed' units
            var localStep = new CustomDataTypesCS.Vector3(speed, 0, 0);

            // Rotate into world-space, then add to position
            var worldStep = rotMat.Multiply(localStep);
            position += worldStep;
        }

        // Draws the bullet rotated to match its trajectory.
        public void Draw()
        {
            // Convert radians to degrees.
            float degrees = angle * 180f / (float)Math.PI;

            // Scale factor for sprite.
            const float scale = 1.15f;
            float w = texture.width * scale;
            float h = texture.height * scale;

            // Origin at sprite centre for correct rotation.
            var origin = new Vector2(texture.width / 2f, texture.height / 2f) * scale;

            // Raylib’s DrawTexturePro requires source & destination rectangles.
            Raylib.DrawTexturePro(
                texture,
                new Rectangle(0, 0, texture.width, texture.height),
                new Rectangle(position.x, position.y, w, h),
                origin,
                degrees + 90f,    // Adjust so sprite points along its path.
                Color.WHITE       // Uses built-in Color struct.
            );
        }

        // Returns true if bullet has exited the visible screen area.
        public bool IsOffScreen(int screenWidth, int screenHeight)
        {
            return position.x < 0 || position.x > screenWidth ||
                   position.y < 0 || position.y > screenHeight;
        }
    }
}