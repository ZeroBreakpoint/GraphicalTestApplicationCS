using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Numerics;          
using CustomDataTypesCS;       


using Rectangle = Raylib_cs.Rectangle;

namespace GraphicalTestApplicationCS
{
    public class Tank(Texture2D body, Texture2D turret, Texture2D bullet)
    {
        // Textures for body, turret and bullet.
        private Texture2D bodyTexture = body;
        private Texture2D turretTexture = turret;
        private Texture2D bulletTexture = bullet;

        // World position (initialised at centre-ish).
        public CustomDataTypesCS.Vector3 position = new(400, 300, 0);

        // Body and turret rotations in degrees.
        private float rotation = 0f;
        private float turretRotation = 0f;

        // Uniform scale for drawing (modify to resize).
        private const float scale = 1.25f;

        // Updates movement, rotation and track-dropping each frame.
        public void Update(List<(CustomDataTypesCS.Vector3 position, float timestamp, float rotation)> trackPoints)
        {
            // Convert current body rotation to radians
            float bodyRad = rotation * (float)Math.PI / 180f;

            // Build a 2D rotation matrix from that angle
            var rotMat = new Matrix3();
            rotMat.SetRotateZ(bodyRad);

            bool leaveTrack = false;

            // — Body rotation (tracks) —
            if (Raylib.IsKeyDown(KeyboardKey.KEY_A))
            {
                rotation -= 2f;
                leaveTrack = true;
            }
            if (Raylib.IsKeyDown(KeyboardKey.KEY_D))
            {
                rotation += 2f;
                leaveTrack = true;
            }

            // — Movement (tracks) —
            // Define local forward/back step as a Vector3 (z=0 for pure 2D)
            if (Raylib.IsKeyDown(KeyboardKey.KEY_W))
            {
                var localStep = new CustomDataTypesCS.Vector3(2f, 0, 0);
                // Rotate that step into world-space
                var worldStep = rotMat.Multiply(localStep);
                position += worldStep;    // uses your Vector3 +
                leaveTrack = true;
            }
            if (Raylib.IsKeyDown(KeyboardKey.KEY_S))
            {
                var localStep = new CustomDataTypesCS.Vector3(-2f, 0, 0);
                var worldStep = rotMat.Multiply(localStep);
                position += worldStep;
                leaveTrack = true;
            }

            // If moved or rotated, record a track mark
            if (leaveTrack)
            {
                trackPoints.Add((
                    new CustomDataTypesCS.Vector3(position.x, position.y, 0),
                    (float)Raylib.GetTime(),
                    rotation
                ));
            }

            // — Turret only (no tracks) —
            if (Raylib.IsKeyDown(KeyboardKey.KEY_Q))
                turretRotation -= 2f;
            if (Raylib.IsKeyDown(KeyboardKey.KEY_E))
                turretRotation += 2f;
        }

        // Draws the tank body and turret.
        public void Draw()
        {
            // --- Draw body ---
            var destBody = new Vector2(position.x, position.y);
            var originBody = new Vector2(bodyTexture.width / 2f, bodyTexture.height / 2f) * scale;

            Raylib.DrawTexturePro(
                bodyTexture,
                new Rectangle(0, 0, bodyTexture.width, bodyTexture.height),
                new Rectangle(destBody.X, destBody.Y, bodyTexture.width * scale, bodyTexture.height * scale),
                originBody,
                rotation - 90f,   // Align sprite forward.
                Color.WHITE
            );

            // --- Draw turret ---
            float turretOffset = (bodyTexture.width * 0.05f) * scale;
            float bodyRad = rotation * (float)Math.PI / 180f;

            // Position turret at front of body.
            var destTurret = new Vector2(
                position.x + (float)Math.Cos(bodyRad) * turretOffset,
                position.y + (float)Math.Sin(bodyRad) * turretOffset
            );
            var originTurret = new Vector2(turretTexture.width / 2f, 0) * scale;

            Raylib.DrawTexturePro(
                turretTexture,
                new Rectangle(0, 0, turretTexture.width, turretTexture.height),
                new Rectangle(destTurret.X, destTurret.Y, turretTexture.width * scale, turretTexture.height * scale),
                originTurret,
                rotation + turretRotation - 90f,
                Color.WHITE
            );
        }

        // Fires a bullet from the turret tip.
        public Bullet Fire()
        {
            // Compute combined angle in radians.
            float totalRad = (rotation + turretRotation) * (float)Math.PI / 180f;

            // Determine pivot in world space.
            float pivotOffset = (bodyTexture.width * 0.05f) * scale;
            var bodyMat = new Matrix3();
            bodyMat.SetRotateZ(rotation * (float)Math.PI / 180f);
            var pivotLocal = new CustomDataTypesCS.Vector3(pivotOffset, 0, 1);
            var pivotWorld = bodyMat.Multiply(pivotLocal) + position;

            // Direction vector for bullet.
            var dir = new CustomDataTypesCS.Vector3((float)Math.Cos(totalRad), (float)Math.Sin(totalRad), 0);

            // Compute spawn distance: turret length + half bullet height, minus tweak.
            float dist = (turretTexture.height * scale) + (bulletTexture.height * 0.5f * scale) - 10f;

            // Final spawn position.
            var spawnPos = pivotWorld + dir * dist;
            return new Bullet(spawnPos, totalRad, bulletTexture);
        }
    }
}