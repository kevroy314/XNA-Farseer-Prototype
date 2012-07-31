using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using DPSF;
using DPSF.ParticleSystems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using KevinsDemo.ScreenSystem;

namespace KevinsDemo.NPCs.Enemies
{
    /// <summary>
    /// This class demonstrates the most basic enemy.
    /// It flies in circles and has particles.
    /// </summary>
    class SimpleTestEnemy
    {
        #region Variables

        //The enemy texture to draw
        private Texture2D _enemyTexture;

        //The enemy particles system
        private TrailParticleSystem _enemyParticles;

        //The enemy position
        private Vector2 _position;
        //The enemy velocity
        private Vector2 _velocity;

        #endregion

        #region Constructors

        public SimpleTestEnemy(Game owningGame, ContentManager content, Vector2 startPosition, int numberOfParticles)
        {
            _position = startPosition;
            _velocity = Vector2.Zero;
            _enemyTexture = content.Load<Texture2D>("Textures/RedCircle");
            _enemyParticles = new TrailParticleSystem(owningGame);
            _enemyParticles.AutoInitialize(owningGame.GraphicsDevice, owningGame.Content, GameMain.ScreenManager.SpriteBatch);
        }

        #endregion

        #region Update and Draw Functions
        Vector3 cPos = Vector3.Zero;
        public void Update(GameTime gameTime, Camera2D camera)
        {
            //Move in a circle
            //_position.X = 300 * (float)Math.Cos(gameTime.TotalGameTime.TotalSeconds);
            //_position.Y = 300 * (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds);
            _enemyParticles.Emitter.PositionData.Position = new Vector3(_position.X,-_position.Y, 0f);

            // Set up the Camera's View matrix
            cPos = new Vector3(camera.Position.X, camera.Position.Y, 800f);
            Matrix sViewMatrix = Matrix.CreateLookAt(cPos, new Vector3(camera.Position.X, camera.Position.Y, 0f), Vector3.Up);

            // Setup the Camera's Projection matrix by specifying the field of view (1/4 pi), aspect ratio, and the near and far clipping planes
            Matrix sProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, (float)GameMain.ScreenManager.GraphicsDevice.Viewport.Width / (float)GameMain.ScreenManager.GraphicsDevice.Viewport.Height, 1, 10000);


            // Draw the Particle System
            _enemyParticles.SetWorldViewProjectionMatrices(Matrix.Identity, sViewMatrix, sProjectionMatrix);
            _enemyParticles.SetCameraPosition(cPos);
            _enemyParticles.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        public void Draw(SpriteBatch batch)
        {
            batch.Draw(_enemyTexture, _position, Color.White);
            batch.DrawString(GameMain.ScreenManager.Fonts.DetailsFont, cPos.ToString(), new Vector2(100f, 100f), Color.White);
            batch.DrawString(GameMain.ScreenManager.Fonts.DetailsFont, _enemyParticles.Particles.Length.ToString(), new Vector2(100f, 140f), Color.White);
        }

        public void DrawParticles()
        {
            _enemyParticles.Draw();
        }

        #endregion

        #region Content Management Functions

        public void UnloadContent()
        {
            _enemyParticles.Destroy();
        }
        public Vector2 Position
        {
            set { _position = value; }
        }

        #endregion
    }
}
