using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using KevinsDemo.DrawingSystem;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework.Content;
using FarseerPhysics.Common;

namespace KevinsDemo.CharacterSystem
{
    class Portal
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

        private bool _inPortal;

        #endregion

        #region Constructor

        public Portal(World world, ContentManager content, Vector2 position, bool inPortal)
        {
            _inPortal = inPortal;
            //Load agent and collision textures
            //_agentTextures = content.Load<Texture2D>("CharacterSprites/chrono");
            if(_inPortal)
                _agentCollisionTextures = content.Load<Texture2D>("CharacterSprites/Portal/InPortal0");
            else
                _agentCollisionTextures = content.Load<Texture2D>("CharacterSprites/Portal/OutPortal0");
            //_agentHitBoxTextures = new Texture2D[4];
            //_agentHitBoxTextures[0] = content.Load<Texture2D>("CharacterSprites/Attacks/SimpleHit/chrono_hitBoxTowardsScreen");
            //_agentHitBoxTextures[1] = content.Load<Texture2D>("CharacterSprites/Attacks/SimpleHit/chrono_hitBoxLeft");
            //_agentHitBoxTextures[2] = content.Load<Texture2D>("CharacterSprites/Attacks/SimpleHit/chrono_hitBoxRight");
            //_agentHitBoxTextures[3] = content.Load<Texture2D>("CharacterSprites/Attacks/SimpleHit/chrono_hitBoxAwayFromScreen");
            //_bounds = _agentTextures.Bounds;

            //Default scale is 1f
            _scale = new Vector2(0.25f, 0.25f);

            List<Vertices> list = HelperFunctions.GetVerticiesListFromTexture(_agentCollisionTextures, _scale, ref _origin);

            //Set up the agents body
            //Create dynamic body
            _body = BodyFactory.CreateCompoundPolygon(world, list, 1f);
            _body.CollidesWith = Category.None;
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
            _spriteAnimations = new SpriteAnimation[1]; //Magic#
            if(_inPortal)
                _spriteAnimations[0] = new SpriteAnimation("f0", content.Load<Texture2D>("CharacterSprites/Portal/InPortal0"), _origin, 4, 2, 10f, true);
            else
                _spriteAnimations[0] = new SpriteAnimation("f0", content.Load<Texture2D>("CharacterSprites/Portal/OutPortal0"), _origin, 4, 2, 10f, true);
            _spriteAnimations[0].Scale = 0.25f;
            _spriteAnimations[0].Play();
            //Start facing towards the screen
            _currentDrawSpriteIndex = 0;
        }

        #endregion

        #region Draw, Update and Input

        public void Draw(SpriteBatch batch)
        {
            //Draw the texture at its current position
            _spriteAnimations[_currentDrawSpriteIndex].Draw(batch, _body.Position);
        }

        public void Update(GameTime gameTime)
        {
            //Update the current sprite being drawn
            _spriteAnimations[_currentDrawSpriteIndex].Update(gameTime);
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
