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
using FarseerPhysics.Collision.Shapes;

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
        //The texture for the hit box
        private Texture2D[] _agentHitBoxTextures;

        //The origin of the body
        private Vector2 _origin;
        //The scale of the body
        private Vector2 _scale;
        //The speed of the users motion
        private float _speed;

        //Sprite animations for each direction
        private SpriteAnimation[] _spriteAnimations;

        //The current animation index
        private int _currentDrawSpriteIndex;

        private Portal _inPortal;
        private Portal _outPortal;
        private World _myWorld;
        private ContentManager _myContentManager;
        
        #endregion

        #region Constructor

        public Character(World world, ContentManager content, Vector2 position)
        {

            _myWorld = world;
            _myContentManager = content;

            //Load agent and collision textures
            //_agentTextures = content.Load<Texture2D>("CharacterSprites/chrono");
            _agentCollisionTextures = content.Load<Texture2D>("CharacterSprites/chrono_collisionBox_Small");
            //_agentHitBoxTextures = new Texture2D[4];
            //_agentHitBoxTextures[0] = content.Load<Texture2D>("CharacterSprites/Attacks/SimpleHit/chrono_hitBoxTowardsScreen");
            //_agentHitBoxTextures[1] = content.Load<Texture2D>("CharacterSprites/Attacks/SimpleHit/chrono_hitBoxLeft");
            //_agentHitBoxTextures[2] = content.Load<Texture2D>("CharacterSprites/Attacks/SimpleHit/chrono_hitBoxRight");
            //_agentHitBoxTextures[3] = content.Load<Texture2D>("CharacterSprites/Attacks/SimpleHit/chrono_hitBoxAwayFromScreen");
            //_bounds = _agentTextures.Bounds;

            //Default scale is 1f
            _scale = new Vector2(1f, 1f);

            List<Vertices> list = HelperFunctions.GetVerticiesListFromTexture(_agentCollisionTextures, _scale, ref _origin);

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
            //Vector2 dummyOrigin = new Vector2();
            //PolygonShape[] hitboxes = new PolygonShape[4];
            //hitboxes[0] = new PolygonShape(HelperFunctions.GetConvexHullFromTexture(_agentHitBoxTextures[0], _scale, ref dummyOrigin), 1f);
            //hitboxes[1] = new PolygonShape(HelperFunctions.GetConvexHullFromTexture(_agentHitBoxTextures[1], _scale, ref dummyOrigin), 1f);
            //hitboxes[2] = new PolygonShape(HelperFunctions.GetConvexHullFromTexture(_agentHitBoxTextures[2], _scale, ref dummyOrigin), 1f);
            //hitboxes[3] = new PolygonShape(HelperFunctions.GetConvexHullFromTexture(_agentHitBoxTextures[3], _scale, ref dummyOrigin), 1f);

            //_body.CreateFixture(hitboxes[0]);
            //_body.CreateFixture(hitboxes[1]);
            //_body.CreateFixture(hitboxes[2]);
            //_body.CreateFixture(hitboxes[3]);
            
            //Simple example of the chrono walking animations
            _spriteAnimations = new SpriteAnimation[8]; //Magic#
            _spriteAnimations[0] = new SpriteAnimation("towards screen", content.Load<Texture2D>("CharacterSprites/Movement/Walking/chrono_towardsScreen"), _origin, 4, 1, 5f, true);
            _spriteAnimations[1] = new SpriteAnimation("left", content.Load<Texture2D>("CharacterSprites/Movement/Walking/chrono_left"), _origin, 4, 1, 5f, true);
            _spriteAnimations[2] = new SpriteAnimation("right", content.Load<Texture2D>("CharacterSprites/Movement/Walking/chrono_right"), _origin, 4, 1, 5f, true);
            _spriteAnimations[3] = new SpriteAnimation("away from screen", content.Load<Texture2D>("CharacterSprites/Movement/Walking/chrono_awayFromScreen"), _origin, 4, 1, 5f, true);
            _spriteAnimations[4] = new SpriteAnimation("hit towards screen", content.Load<Texture2D>("CharacterSprites/Attacks/SimpleHit/chrono_hitTowardsScreen"), _origin + new Vector2(0f, 20f), 4, 1, 5f, false);
            _spriteAnimations[5] = new SpriteAnimation("hit left", content.Load<Texture2D>("CharacterSprites/Attacks/SimpleHit/chrono_hitLeft"), _origin + new Vector2(0f, 20f), 4, 1, 5f, false);
            _spriteAnimations[6] = new SpriteAnimation("hit right", content.Load<Texture2D>("CharacterSprites/Attacks/SimpleHit/chrono_hitRight"), _origin + new Vector2(0f, 20f), 4, 1, 5f, false);
            _spriteAnimations[7] = new SpriteAnimation("hit away from screen", content.Load<Texture2D>("CharacterSprites/Attacks/SimpleHit/chrono_hitAwayFromScreen"), _origin + new Vector2(0f, 20f), 4, 1, 5f, false);

            //Start facing towards the screen
            _currentDrawSpriteIndex = 0;

            _inPortal = null;
            _outPortal = null;
        }

        #endregion

        #region Draw, Update and Input

        public void Draw(SpriteBatch batch)
        {
            //Draw the texture at its current position
            if (_inPortal != null) _inPortal.Draw(batch);
            if (_outPortal != null) _outPortal.Draw(batch);
            _spriteAnimations[_currentDrawSpriteIndex].Draw(batch, _body.Position);
        }

        public void Update(GameTime gameTime)
        {
            //Update the current sprite being drawn
            if (_inPortal != null) _inPortal.Update(gameTime);
            if (_outPortal != null) _outPortal.Update(gameTime);
            _spriteAnimations[_currentDrawSpriteIndex].Update(gameTime);
        }

        public void HandleInput(InputHelper input, GameTime gameTime)
        {
            //Create flags for each movement
            bool up = false;
            bool down = false;
            bool left = false;
            bool right = false;
            bool inPortal = false;
            bool outPortal = false;
            bool hit = false;
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
            if ((input.MouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed && input.PreviousMouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released) ||
                (input.GamePadState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.A) && input.PreviousGamePadState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.A)))
                hit = true;
            if ((input.MouseState.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed && input.PreviousMouseState.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Released) ||
                (input.GamePadState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.B) && input.PreviousGamePadState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.B)))
                inPortal = true;
            if ((input.MouseState.MiddleButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed && input.PreviousMouseState.MiddleButton == Microsoft.Xna.Framework.Input.ButtonState.Released) ||
                (input.GamePadState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.X) && input.PreviousGamePadState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.X)))
                outPortal = true;
            //Given the buttons that were pressed, we save the old sprite animation index
            int oldDrawSpriteIndex = _currentDrawSpriteIndex;

            if (!(oldDrawSpriteIndex > 3 && _spriteAnimations[oldDrawSpriteIndex].AnimationState == SpriteAnimationState.Running))
            {

                if (left) //If we pressed left, it takes priority
                {
                    _currentDrawSpriteIndex = 1;
                    //If we pressed something and we were stopped, start the animation again
                    if (_spriteAnimations[_currentDrawSpriteIndex].AnimationState == SpriteAnimationState.Stopped)
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
            }
            if (oldDrawSpriteIndex > 3 && _spriteAnimations[oldDrawSpriteIndex].AnimationState == SpriteAnimationState.Stopped && oldDrawSpriteIndex == _currentDrawSpriteIndex)
                _currentDrawSpriteIndex -= 4;
            if (hit && _currentDrawSpriteIndex <= 3)
                _currentDrawSpriteIndex += 4;

            if (inPortal)
            {
                _inPortal = new Portal(this._myWorld, this._myContentManager, this._body.Position+new Vector2(this._origin.Y,-this._origin.X), true);
                _outPortal = null;
            }
            if (outPortal && _inPortal != null)
            {
                _outPortal = new Portal(this._myWorld, this._myContentManager, this._body.Position + new Vector2(this._origin.Y, -this._origin.X), false);
            }

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
        public Vector2 Scale
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
