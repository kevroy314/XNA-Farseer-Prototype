using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using DPSF;
using DPSF.ParticleSystems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace KevinsDemo.NPCs.Enemies
{
    class SimpleTestEnemy
    {
        private Texture2D _enemyTexture;
        private SpriteParticleSystem _enemyParticles;
        private Vector2 _position;
        private Vector2 _velocity;

        public SimpleTestEnemy(Game owningGame, ContentManager content, Vector2 startPosition, int numberOfParticles)
        {
            _position = startPosition;
            _velocity = Vector2.Zero;
            _enemyTexture = content.Load<Texture2D>("Textures/RedCircle");
            _enemyParticles = new SpriteParticleSystem(owningGame, new Vector3(startPosition, 0f), _enemyTexture.Bounds);
            _enemyParticles.AutoInitialize(owningGame.GraphicsDevice, owningGame.Content, GameMain.ScreenManager.SpriteBatch);
        }

        public void Update(GameTime gameTime)
        {
            _enemyParticles.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        public void Draw(SpriteBatch batch)
        {
            batch.Draw(_enemyTexture, _position, Color.White);
            _enemyParticles.Draw();
        }

        public void UnloadContent()
        {
            _enemyParticles.Destroy();
        }
    }
}
