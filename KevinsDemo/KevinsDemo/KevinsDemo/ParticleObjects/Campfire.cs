using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using DPSF.ParticleSystems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Common.PolygonManipulation;
using FarseerPhysics.Common.Decomposition;
using KevinsDemo.ScreenSystem;
using FarseerPhysics.Factories;

namespace ParticleObjects
{
    class Campfire
    {
        private Texture2D _logs;

        // Declare our Particle System variable
        private FireParticleSystem _particleSystem = null;

        private Game _parentGame;

        private Body _body;

        private Vector2 _origin;

        Vector2 _scale;

        public Campfire(Game game, World world, SpriteBatch batch, Vector2 position)
        {
            _parentGame = game;
            // TODO: use this.Content to load your game content here
            _logs = _parentGame.Content.Load<Texture2D>("EnvironmentObjects/logs");
            _scale = new Vector2(0.5f, 0.45f);
            //Create an array to hold the data from the texture
            uint[] data = new uint[_logs.Width * _logs.Height];


            //Transfer the texture data to the array
            _logs.GetData(data);

            //Find the vertices that makes up the outline of the shape in the texture
            Vertices textureVertices = PolygonTools.CreatePolygon(data, _logs.Width, false);

            //The tool return vertices as they were found in the texture.
            //We need to find the real center (centroid) of the vertices for 2 reasons:

            //1. To translate the vertices so the polygon is centered around the centroid.
            Vector2 centroid = -textureVertices.GetCentroid();
            textureVertices.Translate(ref centroid);

            //2. To draw the texture the correct place.
            _origin = -centroid;

            //We simplify the vertices found in the texture.
            textureVertices = SimplifyTools.ReduceByDistance(textureVertices, 4f);

            //Since it is a concave polygon, we need to partition it into several smaller convex polygons
            List<Vertices> list = BayazitDecomposer.ConvexPartition(textureVertices);

            //scale the vertices from graphics space to sim space
            Vector2 vertScale = new Vector2(ConvertUnits.ToSimUnits(1)) * _scale;
            foreach (Vertices vertices in list)
            {
                vertices.Scale(ref vertScale);
            }

            //Create a single body with multiple fixtures
            _body = BodyFactory.CreateCompoundPolygon(world, list, 1f, BodyType.Dynamic);
            _body.BodyType = BodyType.Static;
            _body.CollidesWith = Category.All;
            _body.Position = position;

            // Declare a new Particle System instance and Initialize it
            _particleSystem = new FireParticleSystem(_parentGame);
            _particleSystem.AutoInitialize(_parentGame.GraphicsDevice, _parentGame.Content, batch);
        }

        public void UnloadContent()
        {
            // Destroy the Particle System
            _particleSystem.Destroy();
        }

        public void Update(GameTime gameTime, Camera2D camera)
        {
            // Set up the Camera's View matrix
            Matrix sViewMatrix = Matrix.CreateLookAt(new Vector3(camera.Position.X / 4, -camera.Position.Y / 4, 200f), new Vector3(camera.Position.X / 4, -camera.Position.Y / 4, 0f), Vector3.Up);

            // Setup the Camera's Projection matrix by specifying the field of view (1/4 pi), aspect ratio, and the near and far clipping planes
            Matrix sProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, (float)_parentGame.GraphicsDevice.Viewport.Width / (float)_parentGame.GraphicsDevice.Viewport.Height, 1, 10000);

            //sProjectionMatrix = camera.SimProjection;

            // Draw the Particle System
            _particleSystem.SetWorldViewProjectionMatrices(Matrix.Identity, sViewMatrix, sProjectionMatrix);
            _particleSystem.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        public void Draw(SpriteBatch batch)
        {
            batch.Draw(_logs, _body.Position, null, Color.White, 0f, _origin, _scale, SpriteEffects.None, 0);
        }

        public void DrawParticles()
        {
            _particleSystem.Draw();
        }
    }
}
