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
    /// <summary>
    /// This is the basic character class. It manages the sprite animations for it's self,
    /// and draws and updates according to user input. It also interfaces with the collision
    /// system and manages the collisions.
    /// </summary>
    class Character
    {
        #region Variables

        //The body for collision calculations
        private Body _body;
        
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
            _body = BodyFactory.CreateCompoundPolygon(world, list, 1f, BodyType.Dynamic);
            _body.BodyType = BodyType.Dynamic;
            //Collides with everything
            _body.CollidesWith = Category.All;
            //Do not allow rotation
            _body.FixedRotation = true;
            //Apply some friction
            _body.LinearDamping = 10f;
            //Offset the initial position from center
            _body.Position = position;

            //Speed of the player movement
            _speed = 10000f;

            //Simple example of the chrono walking animations
            _spriteAnimations = new SpriteAnimation[4]; //Magic#
            _spriteAnimations[0] = new SpriteAnimation("towards screen", content.Load<Texture2D>("CharacterSprites/chrono_towardsScreen"), _body.Position, 4, 1, 5f, true);
            _spriteAnimations[1] = new SpriteAnimation("left", content.Load<Texture2D>("CharacterSprites/chrono_left"), _body.Position, 4, 1, 5f, true);
            _spriteAnimations[2] = new SpriteAnimation("right", content.Load<Texture2D>("CharacterSprites/chrono_right"), _body.Position, 4, 1, 5f, true);
            _spriteAnimations[3] = new SpriteAnimation("away from screen", content.Load<Texture2D>("CharacterSprites/chrono_awayFromScreen"), _body.Position, 4, 1, 5f, true);

            //Start facing towards the screen
            _currentDrawSpriteIndex = 0;
        }

        #endregion

        #region Draw, Update and Input

        public void Draw(SpriteBatch batch)
        {
            //Draw the texture at its current position
            _spriteAnimations[_currentDrawSpriteIndex].Draw(batch, ConvertUnits.ToDisplayUnits(_body.Position));
        }

        public void Update(GameTime gameTime)
        {
            //Update the current sprite being drawn
            _spriteAnimations[_currentDrawSpriteIndex].Update(gameTime);
        }

        public void HandleInput(InputHelper input, GameTime gameTime)
        {
            //Create flags for each movement
            bool up = false;
            bool down = false;
            bool left = false;
            bool right = false;
            if (input.KeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.W)||input.GamePadState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.LeftThumbstickUp))
            {
                //If we press up, apply a linear impulse up (do not change position as it will not map collisions)
                up = true;
                _body.ApplyLinearImpulse(new Vector2(0.0f, -_speed));
            }
            if (input.KeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.S) || input.GamePadState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.LeftThumbstickDown))
            {
                //If we press down, apply a linear impulse down (do not change position as it will not map collisions)
                down = true;
                _body.ApplyLinearImpulse(new Vector2(0.0f, _speed));
            }
            if (input.KeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.A) || input.GamePadState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.LeftThumbstickLeft))
            {
                //If we press left, apply a linear impulse left (do not change position as it will not map collisions)
                left = true;
                _body.ApplyLinearImpulse(new Vector2(-_speed, 0.0f));
            }
            if (input.KeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D) || input.GamePadState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.LeftThumbstickRight))
            {
                //If we press right, apply a linear impulse right (do not change position as it will not map collisions)
                right = true;
                _body.ApplyLinearImpulse(new Vector2(_speed, 0.0f));
            }

            //Given the buttons that were pressed, we save the old sprite animation index
            int oldDrawSpriteIndex = _currentDrawSpriteIndex;

            if (left) //If we pressed left, it takes priority
            {
                _currentDrawSpriteIndex = 1;
                //If we pressed something and we were stopped, start the animation again
                if(_spriteAnimations[_currentDrawSpriteIndex].AnimationState == SpriteAnimationState.Stopped)
                    _spriteAnimations[_currentDrawSpriteIndex].Play();
            }
            else if (right) //If we pressed right, it takes priority above up and down
            {
                _currentDrawSpriteIndex = 2;
                //If we pressed something and we were stopped, start the animation again
                if (_spriteAnimations[_currentDrawSpriteIndex].AnimationState == SpriteAnimationState.Stopped)
                    _spriteAnimations[_currentDrawSpriteIndex].Play();
            }
            else if (up) //If we pressed up it takes priority over down
            {
                _currentDrawSpriteIndex = 3;
                //If we pressed something and we were stopped, start the animation again
                if (_spriteAnimations[_currentDrawSpriteIndex].AnimationState == SpriteAnimationState.Stopped)
                    _spriteAnimations[_currentDrawSpriteIndex].Play();
            }
            else if (down) //If we pressed down, do down animation
            {
                _currentDrawSpriteIndex = 0;
                //If we pressed something and we were stopped, start the animation again
                if (_spriteAnimations[_currentDrawSpriteIndex].AnimationState == SpriteAnimationState.Stopped)
                    _spriteAnimations[_currentDrawSpriteIndex].Play();
            }
            else //If nothing was pressed, stop the current animation
                _spriteAnimations[_currentDrawSpriteIndex].Stop();

            //If something new was pressed, make sure to restart the animation
            if (oldDrawSpriteIndex != _currentDrawSpriteIndex)
                _spriteAnimations[_currentDrawSpriteIndex].Play();
        }

        #endregion

        #region Properties

        //The body for the character
        public Body Body
        {
            get { return _body; }
        }

        //The bounds for the character texture
        public Rectangle Bounds
        {
            get { return _bounds; }
        }

        //The origin for the character (for rotation)
        public Vector2 Origin
        {
            get { return _origin; }
            set { _origin = value; }
        }

        //The scale of the character to be drawn
        public float Scale
        {
            get { return _scale; }
            set { _scale = value; }
        }

        //The walking speed of the character
        public float WalkSpeed
        {
            get { return _speed; }
            set { _speed = value; }
        }

        #endregion
    }
}
