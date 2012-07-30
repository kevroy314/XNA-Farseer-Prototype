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
            _enemyParticles = new SpriteParticleSystem(owningGame, ConvertUnits.ToDisplayUnits(new Vector3(startPosition, 0f)), _enemyTexture.Bounds);
            _enemyParticles.AutoInitialize(owningGame.GraphicsDevice, owningGame.Content, GameMain.ScreenManager.SpriteBatch);
        }

        public void Update(GameTime gameTime, Camera2D camera)
        {
            _position.X = 4 * (float)Math.Cos(gameTime.TotalGameTime.TotalSeconds);
            _position.Y = 4 * (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds);
            _enemyParticles.AttractorPosition = ConvertUnits.ToDisplayUnits(new Vector3(_position, 0f)-new Vector3(camera.Position,0f));
            _enemyParticles.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        public void Draw(SpriteBatch batch)
        {
            batch.Draw(_enemyTexture, ConvertUnits.ToDisplayUnits(_position), Color.White);
            _enemyParticles.Draw();
        }

        public void UnloadContent()
        {
            _enemyParticles.Destroy();
        }
    }
}
