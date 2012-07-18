﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using KevinsDemo.ScreenSystem;
using Microsoft.Xna.Framework.Content;
using FarseerPhysics.Common;
using FarseerPhysics.Common.PolygonManipulation;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Factories;

namespace KevinsDemo.CharacterSystem
{
    class Character
    {
        //The body for collision calculations
        private Body _agentBody;
        //The texture for drawing
        private Texture2D _agentTextures;
        //The texture for determining the collision polygon
        private Texture2D _agentCollisionTextures;
        //The origin of the body
        private Vector2 _origin;
        //The scale of the body
        private float _scale;
        //The speed of the users motion
        private float _speed;

        public Character(World world, ContentManager content)
        {
            //Load agent and collision textures
            _agentTextures = content.Load<Texture2D>("chrono");
            _agentCollisionTextures = content.Load<Texture2D>("chrono_collisionBox");

            //Create an array to hold the data from the texture
            uint[] data = new uint[_agentCollisionTextures.Width * _agentCollisionTextures.Height];

            //Transfer the texture data to the array
            _agentCollisionTextures.GetData(data);

            //Find the vertices that makes up the outline of the shape in the texture
            Vertices textureVertices = PolygonTools.CreatePolygon(data, _agentCollisionTextures.Width, false);

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

            //Adjust the scale of the object
            _scale = 1f;

            //Scale the vertices from graphics space to sim space
            Vector2 vertScale = new Vector2(ConvertUnits.ToSimUnits(1)) * _scale;
            foreach (Vertices vertices in list)
            {
                vertices.Scale(ref vertScale);
            }

            //Set up the agents body
            //Create dynamic body
            _agentBody = BodyFactory.CreateCompoundPolygon(world, list, 1f, BodyType.Dynamic);
            _agentBody.BodyType = BodyType.Dynamic;
            //Collides with everything
            _agentBody.CollidesWith = Category.All;
            //Do not allow rotation
            _agentBody.FixedRotation = true;
            //Apply some friction
            _agentBody.LinearDamping = 10f;
            //Offset the initial position from center
            _agentBody.Position += new Vector2(10f, 10f);

            //Speed of the player movement
            _speed = 0.4f;
        }

        public void Draw(SpriteBatch batch)
        {
            //Draw the texture at its current position
            batch.Draw(_agentTextures, ConvertUnits.ToDisplayUnits(_agentBody.Position),
                                           null, Color.White, _agentBody.Rotation, _origin, _scale, SpriteEffects.None,
                                           0f);
        }

        public void Update(GameTime gameTime)
        {
        }

        public void HandleInput(InputHelper input, GameTime gameTime)
        {
            if (input.KeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.W))
                _agentBody.ApplyLinearImpulse(new Vector2(0.0f, -_speed));
            if (input.KeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.A))
                _agentBody.ApplyLinearImpulse(new Vector2(-_speed, 0.0f));
            if (input.KeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.S))
                _agentBody.ApplyLinearImpulse(new Vector2(0.0f, _speed));
            if (input.KeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D))
                _agentBody.ApplyLinearImpulse(new Vector2(_speed, 0.0f));
        }

        public Body Body
        {
            get { return _agentBody; }
        }
    }
}