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
using KevinsDemo;

namespace ParticleObjects
{
    /// <summary>
    /// The basic campfire object to demo particle objects.
    /// </summary>
    class Campfire
    {
        #region Variables

        //The texture representing the logs
        private Texture2D _logs;

        //The fire particle system
        private FireParticleSystem _particleSystem = null;

        //The parent game (required by particle system)
        private Game _parentGame;

        //The body for the logs (for collisions)
        private Body _body;

        //The origin of the logs (for draw rotation)
        private Vector2 _origin;
        //The scale of the logs for drawing
        private Vector2 _scale;
        //The rotation for drawing
        private float _rotation;
        //The flip options for drawing
        private SpriteEffects _spriteEffects;
        //The layer depth for drawing
        private float _layerDepth;

        #endregion

        #region Constructors

        public Campfire(Game game, World world, SpriteBatch batch, Vector2 position)
        {
            //Set the parent game
            _parentGame = game;

            //Initialize the logs
            _logs = _parentGame.Content.Load<Texture2D>("EnvironmentObjects/logs");
            //Initialize the scale (we're scaling down the logs when we draw instead of in the original image to show off that capability)
            _scale = new Vector2(0.5f, 0.45f);
            //No rotation
            _rotation = 0f;
            //No sprite effects
            _spriteEffects = SpriteEffects.None;
            //Layer depth to front layer
            _layerDepth = 0f;

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

        #endregion

        #region Content Management Functions

        public void UnloadContent()
        {
            // Destroy the Particle System
            _particleSystem.Destroy();
        }

        #endregion

        #region Update and Draw Functions

        public void Update(GameTime gameTime, Camera2D camera)
        {
            // Set up the Camera's View matrix
            Vector3 cPos = new Vector3(camera.Position.X / 4, -camera.Position.Y / 4, 200f);
            Matrix sViewMatrix = Matrix.CreateLookAt(cPos, new Vector3(camera.Position.X / 4, -camera.Position.Y / 4, 0f), Vector3.Up);

            // Setup the Camera's Projection matrix by specifying the field of view (1/4 pi), aspect ratio, and the near and far clipping planes
            Matrix sProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, (float)_parentGame.GraphicsDevice.Viewport.Width / (float)_parentGame.GraphicsDevice.Viewport.Height, 1, 10000);

            // Draw the Particle System
            _particleSystem.SetWorldViewProjectionMatrices(Matrix.Identity, sViewMatrix, sProjectionMatrix);
            _particleSystem.SetCameraPosition(cPos);
            _particleSystem.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        //Draws just the logs
        public void Draw(SpriteBatch batch)
        {
            batch.Draw(_logs, _body.Position, null, Color.White, _rotation, _origin, _scale, _spriteEffects, _layerDepth);
        }

        //Draws just the particles
        public void DrawParticles()
        {
            _particleSystem.Draw();
        }

        #endregion

        #region Properties

        //The body of the campfire logs (for collisions)
        public Body Body
        {
            get { return _body; }
        }

        //The particle system for the fire
        public FireParticleSystem ParticleSystem
        {
            get { return _particleSystem; }
            set { _particleSystem = value; }
        }

        //The rotation for drawing
        public float Rotation
        {
            get { return _rotation; }
            set { _rotation = value; }
        }

        //The flip effects for drawing
        public SpriteEffects SpriteEffects
        {
            get { return _spriteEffects; }
            set { _spriteEffects = value; }
        }

        //The layer depth for drawing
        public float LayerDepth
        {
            get { return _layerDepth; }
            set { _layerDepth = value; }
        }

        //The origin for rotation
        public Vector2 Origin
        {
            get { return _origin; }
            set { _origin = value; }
        }

        //The scale for drawing
        public Vector2 Scale
        {
            get { return _scale; }
            set { _scale = value; }
        }

        #endregion
    }
}
