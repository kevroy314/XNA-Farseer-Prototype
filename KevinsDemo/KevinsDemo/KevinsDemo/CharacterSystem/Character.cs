using System;
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
using KevinsDemo.DrawingSystem;

namespace KevinsDemo.CharacterSystem
{
    class Character
    {
        #region Variables

        //The body for collision calculations
        private Body _agentBody;
        
        //The bounds of the texture
        private Rectangle _bounds;

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

        //Sprite animations for each direction
        private SpriteAnimation[] _spriteAnimations;

        //The current animation index
        private int _currentDrawSpriteIndex;

        #endregion

        #region Constructor

        public Character(World world, ContentManager content, Vector2 position)
        {
            //Load agent and collision textures
            _agentTextures = content.Load<Texture2D>("CharacterSprites/chrono");
            _agentCollisionTextures = content.Load<Texture2D>("CharacterSprites/chrono_collisionBox_Small");
            _bounds = _agentTextures.Bounds;
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
            _agentBody.Position = position;

            //Speed of the player movement
            _speed = 0.6f;

            _spriteAnimations = new SpriteAnimation[4]; //Magic#
            _spriteAnimations[0] = new SpriteAnimation("towards screen", content.Load<Texture2D>("CharacterSprites/chrono_towardsScreen"), _agentBody.Position, 4, 1, 5f, true);
            _spriteAnimations[1] = new SpriteAnimation("left", content.Load<Texture2D>("CharacterSprites/chrono_left"), _agentBody.Position, 4, 1, 5f, true);
            _spriteAnimations[2] = new SpriteAnimation("right", content.Load<Texture2D>("CharacterSprites/chrono_right"), _agentBody.Position, 4, 1, 5f, true);
            _spriteAnimations[3] = new SpriteAnimation("away from screen", content.Load<Texture2D>("CharacterSprites/chrono_awayFromScreen"), _agentBody.Position, 4, 1, 5f, true);

            _currentDrawSpriteIndex = 0;
        }

        #endregion

        #region Draw, Update and Input

        public void Draw(SpriteBatch batch)
        {
            //Draw the texture at its current position
            _spriteAnimations[_currentDrawSpriteIndex].Draw(batch,ConvertUnits.ToDisplayUnits(_agentBody.Position));
        }

        public void Update(GameTime gameTime)
        {
            for (int i = 0; i < _spriteAnimations.Length; i++)
                _spriteAnimations[i].Update(gameTime);
        }

        public void HandleInput(InputHelper input, GameTime gameTime)
        {
            bool up = false;
            bool down = false;
            bool left = false;
            bool right = false;
            if (input.KeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.W)||input.GamePadState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.LeftThumbstickUp))
            {
                up = true;
                _agentBody.ApplyLinearImpulse(new Vector2(0.0f, -_speed));
            }
            if (input.KeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.S) || input.GamePadState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.LeftThumbstickDown))
            {
                down = true;
                _agentBody.ApplyLinearImpulse(new Vector2(0.0f, _speed));
            }
            if (input.KeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.A) || input.GamePadState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.LeftThumbstickLeft))
            {
                left = true;
                _agentBody.ApplyLinearImpulse(new Vector2(-_speed, 0.0f));
            }
            if (input.KeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D) || input.GamePadState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.LeftThumbstickRight))
            {
                right = true;
                _agentBody.ApplyLinearImpulse(new Vector2(_speed, 0.0f));
            }

            int oldDrawSpriteIndex = _currentDrawSpriteIndex;

            if (left)
            {
                _currentDrawSpriteIndex = 1;
                if(_spriteAnimations[_currentDrawSpriteIndex].AnimationState == SpriteAnimationState.Stopped)
                    _spriteAnimations[_currentDrawSpriteIndex].Play();
            }
            else if (right)
            {
                _currentDrawSpriteIndex = 2;
                if (_spriteAnimations[_currentDrawSpriteIndex].AnimationState == SpriteAnimationState.Stopped)
                    _spriteAnimations[_currentDrawSpriteIndex].Play();
            }
            else if (up)
            {
                _currentDrawSpriteIndex = 3;
                if (_spriteAnimations[_currentDrawSpriteIndex].AnimationState == SpriteAnimationState.Stopped)
                    _spriteAnimations[_currentDrawSpriteIndex].Play();
            }
            else if (down)
            {
                _currentDrawSpriteIndex = 0;
                if (_spriteAnimations[_currentDrawSpriteIndex].AnimationState == SpriteAnimationState.Stopped)
                    _spriteAnimations[_currentDrawSpriteIndex].Play();
            }
            else
                _spriteAnimations[_currentDrawSpriteIndex].Stop();

            if (oldDrawSpriteIndex != _currentDrawSpriteIndex)
                _spriteAnimations[_currentDrawSpriteIndex].Play();
        }

        #endregion

        #region Properties

        public Body Body
        {
            get { return _agentBody; }
        }

        public Rectangle Bounds
        {
            get { return _bounds; }
        }

        public Vector2 Origin
        {
            get { return _origin; }
            set { _origin = value; }
        }

        public float Scale
        {
            get { return _scale; }
            set { _scale = value; }
        }

        public float Speed
        {
            get { return _speed; }
            set { _speed = value; }
        }

        #endregion
    }
}
