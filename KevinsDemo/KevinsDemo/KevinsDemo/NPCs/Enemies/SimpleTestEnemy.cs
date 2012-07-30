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
        private SpriteParticleSystem _enemyParticles;

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
            _enemyParticles = new SpriteParticleSystem(owningGame, new Vector3(startPosition, 0f), _enemyTexture.Bounds);
            _enemyParticles.AutoInitialize(owningGame.GraphicsDevice, owningGame.Content, GameMain.ScreenManager.SpriteBatch);
        }

        #endregion

        #region Update and Draw Functions

        public void Update(GameTime gameTime, Camera2D camera)
        {
            //Move in a circle
            _position.X = 300 * (float)Math.Cos(gameTime.TotalGameTime.TotalSeconds);
            _position.Y = 300 * (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds);

            //Set the attractor position and update the particle engine
            _enemyParticles.AttractorPosition = new Vector3(_position, 0f) - new Vector3(camera.Position, 0f);
            _enemyParticles.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        public void Draw(SpriteBatch batch)
        {
            batch.Draw(_enemyTexture, _position, Color.White);
            _enemyParticles.Draw();
        }

        #endregion

        #region Content Management Functions

        public void UnloadContent()
        {
            _enemyParticles.Destroy();
        }

        #endregion
    }
}
