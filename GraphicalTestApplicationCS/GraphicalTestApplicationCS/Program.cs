using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Numerics;           
using CustomDataTypesCS;         

namespace GraphicalTestApplicationCS
{
    // Simple class to track each crate’s state (position, destruction, explosion timer).
    class CrateState(Vector2 pos)
    {
        public Vector2 Pos = pos;
        public bool Destroyed = false;
        public bool ShowExplosion = false;
        public float Timer = 0f;
    }

    class Program
    {
        static void Main()
        {
            // — Initialise the window to 1280×720 and set the target FPS —  
            Raylib.InitWindow(1280, 720, "Tank Game - C# - Bradley Robertson ");
            Raylib.SetTargetFPS(60);

            // — Load all sprite textures from the assets folder —  
            Texture2D tankBodyTex = Raylib.LoadTexture("assets/tankbody.png");
            Texture2D turretTex = Raylib.LoadTexture("assets/tankturret.png");
            Texture2D bulletTex = Raylib.LoadTexture("assets/bullet.png");
            Texture2D sandTex = Raylib.LoadTexture("assets/sand.png");
            Texture2D trackTex = Raylib.LoadTexture("assets/tracks.png");
            Texture2D crateTex = Raylib.LoadTexture("assets/crate.png");
            Texture2D explosionTex = Raylib.LoadTexture("assets/explosion.png");

            // Global scale for background, crates, explosions, etc.
            const float globalScale = 1.15f;

            // Instantiate player tank and collections for bullets & track marks.
            var playerTank = new Tank(tankBodyTex, turretTex, bulletTex);
            var bullets = new List<Bullet>();
            var trackPoints = new List<(CustomDataTypesCS.Vector3 position, float timestamp, float rotation)>();

            // — Spawn 3 crates at safe distances from tank AND each other —  
            const float crateMinDist = 150f;
            Vector2 tankStart = new(playerTank.position.x, playerTank.position.y);
            var crates = new List<CrateState>();
            var rand = Random.Shared;
            int screenW = Raylib.GetScreenWidth(), screenH = Raylib.GetScreenHeight();
            for (int i = 0; i < 3; i++)
            {
                Vector2 pos;
                bool valid;
                do
                {
                    pos = new Vector2(
                        rand.Next(100, screenW - 100),
                        rand.Next(100, screenH - 100)
                    );
                    // Ensure crate is at least crateMinDist from the tank.
                    valid = Vector2.Distance(pos, tankStart) >= crateMinDist;
                    // Also ensure crates aren’t too close to each other.
                    foreach (var other in crates)
                    {
                        if (Vector2.Distance(pos, other.Pos) < crateMinDist)
                        {
                            valid = false;
                            break;
                        }
                    }
                } while (!valid);
                crates.Add(new CrateState(pos));
            }

            // — Main game loop: runs until window is closed —  
            while (!Raylib.WindowShouldClose())
            {
                // — Update tank state and handle firing input —  
                playerTank.Update(trackPoints);
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_SPACE))
                    bullets.Add(playerTank.Fire());

                // Update screen dimensions in case the window was resized.
                screenW = Raylib.GetScreenWidth();
                screenH = Raylib.GetScreenHeight();

                // — Update bullets, remove off‑screen, detect bullet‑to‑crate collisions —  
                for (int i = bullets.Count - 1; i >= 0; i--)
                {
                    bullets[i].Update();

                    if (bullets[i].IsOffScreen(screenW, screenH))
                    {
                        bullets.RemoveAt(i);
                        continue;
                    }

                    foreach (var crate in crates)
                    {
                        if (!crate.Destroyed &&
                            Raylib.CheckCollisionRecs(
                                new Raylib_cs.Rectangle(bullets[i].X, bullets[i].Y, 10, 10),
                                new Raylib_cs.Rectangle(crate.Pos.X, crate.Pos.Y,
                                                        crateTex.width, crateTex.height)))
                        {
                            crate.Destroyed = true;
                            crate.ShowExplosion = true;
                            crate.Timer = 0.5f;
                            bullets.RemoveAt(i);
                            break;
                        }
                    }
                }

                // — Detect tank-body/turret collisions with crates —  
                Vector2 tankCenter = new(playerTank.position.x, playerTank.position.y);
                float tankRadius = Math.Max(tankBodyTex.width, turretTex.width)
                                   * globalScale * 0.5f;
                foreach (var crate in crates)
                {
                    if (!crate.Destroyed &&
                        Vector2.Distance(
                            crate.Pos + new Vector2(
                                crateTex.width * globalScale * 0.5f,
                                crateTex.height * globalScale * 0.5f
                            ),
                            tankCenter
                        ) < tankRadius)
                    {
                        crate.Destroyed = true;
                        crate.ShowExplosion = true;
                        crate.Timer = 0.5f;
                    }
                }

                // — Update explosion timers and hide explosions when time’s up —  
                foreach (var crate in crates)
                {
                    if (crate.ShowExplosion)
                    {
                        crate.Timer -= Raylib.GetFrameTime();
                        if (crate.Timer <= 0f)
                            crate.ShowExplosion = false;
                    }
                }

                // — Begin drawing this frame —  
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Raylib_cs.Color.BLACK);

                // 1) Draw tiled sand background across the screen.
                for (int y = 0; y < screenH; y += sandTex.height)
                    for (int x = 0; x < screenW; x += sandTex.width)
                        Raylib.DrawTextureEx(
                            sandTex,
                            new Vector2(x, y),
                            0,
                            globalScale,
                            Raylib_cs.Color.WHITE
                        );

                // 2) Draw fading tracks at 20% intensity to show tank path.
                double now = Raylib.GetTime();
                foreach (var (pt, time, rot) in trackPoints.ToArray())
                {
                    if (now - time > 10.0)
                    {
                        trackPoints.Remove((pt, time, rot));
                        continue;
                    }
                    float rawAlpha = 1f - (float)((now - time) / 10.0f);
                    float adjAlpha = rawAlpha * 0.20f;          // 20% opacity
                    byte alphaByte = (byte)(adjAlpha * 255f);

                    var fade = new Raylib_cs.Color(
                        (byte)255, (byte)255, (byte)255,
                        alphaByte
                    );

                    var srcRect = new Raylib_cs.Rectangle(0, 0,
                                                           trackTex.width,
                                                           trackTex.height);
                    var destRect = new Raylib_cs.Rectangle(pt.x, pt.y,
                                                           trackTex.width,
                                                           trackTex.height);
                    var origin = new Vector2(
                        trackTex.width / 2f,
                        trackTex.height / 2f
                    );
                    float drawRot = rot - 90f;

                    Raylib.DrawTexturePro(trackTex, srcRect, destRect, origin, drawRot, fade);
                }

                // 3) Draw crates and any active explosion animations.
                foreach (var crate in crates)
                {
                    if (!crate.Destroyed)
                        Raylib.DrawTextureEx(
                            crateTex,
                            crate.Pos,
                            0,
                            globalScale,
                            Raylib_cs.Color.WHITE
                        );

                    if (crate.ShowExplosion)
                    {
                        Vector2 ePos = new(
                            crate.Pos.X + (crateTex.width * globalScale
                                           - explosionTex.width * globalScale) * 0.5f,
                            crate.Pos.Y + (crateTex.height * globalScale
                                           - explosionTex.height * globalScale) * 0.5f
                        );
                        Raylib.DrawTextureEx(
                            explosionTex,
                            ePos,
                            0,
                            globalScale,
                            Raylib_cs.Color.WHITE
                        );
                    }
                }

                // 4) Draw the player’s tank and all active bullets.
                playerTank.Draw();
                foreach (var b in bullets)
                    b.Draw();

                // End drawing for this frame.
                Raylib.EndDrawing();
            }

            // — Clean up all textures and close the window on exit —  
            Raylib.UnloadTexture(tankBodyTex);
            Raylib.UnloadTexture(turretTex);
            Raylib.UnloadTexture(bulletTex);
            Raylib.UnloadTexture(sandTex);
            Raylib.UnloadTexture(trackTex);
            Raylib.UnloadTexture(crateTex);
            Raylib.UnloadTexture(explosionTex);
            Raylib.CloseWindow();
        }
    }
}