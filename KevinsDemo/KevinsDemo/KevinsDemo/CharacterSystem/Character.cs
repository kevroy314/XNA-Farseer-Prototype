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

namespace KevinsDemo.CharacterSystem
{
    class Character
    {
        //The body for collision calculations
        private Body _agentBody;

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
        //Locations in the overall texture of unique character sprites
        private Rectangle[,] _characterSpriteLocations;
        //The index of the current character sprite
        private int _currentCharacterSpriteX;
        private int _currentCharacterSpriteY;
        //Sprite sizes
        private int _numSpritesWidth = 4;
        private int _numSpritesHeight = 4;
        //Variables for slowing down the sprite animation
        private int _spriteTransitionDelay;
        private float _spriteTransitionCounter;

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

            _characterSpriteLocations = new Rectangle[_numSpritesWidth, _numSpritesHeight];

            for (int y = 0; y < _numSpritesHeight; y++)
                for (int x = 0; x < _numSpritesWidth; x++)
                    _characterSpriteLocations[x, y] = new Rectangle(_agentTextures.Bounds.Width / _numSpritesWidth * x, _agentTextures.Bounds.Height / _numSpritesHeight * y + 1, _agentTextures.Bounds.Width / _numSpritesWidth, _agentTextures.Bounds.Height / _numSpritesHeight);

            _currentCharacterSpriteX = 0;
            _currentCharacterSpriteY = 0;

            _spriteTransitionDelay = (int)(_speed * _numSpritesWidth * 4f);
            _spriteTransitionCounter = 0;
        }

        public void Draw(SpriteBatch batch)
        {
            //Draw the texture at its current position
            batch.Draw(_agentTextures, ConvertUnits.ToDisplayUnits(_agentBody.Position),
                                           _characterSpriteLocations[_currentCharacterSpriteX, _currentCharacterSpriteY], Color.White, _agentBody.Rotation, _origin, _scale, SpriteEffects.None,
                                           0f);
        }

        public void Update(GameTime gameTime)
        {
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
            if (left)
            {
                _currentCharacterSpriteY = 1;
                _spriteTransitionCounter = (_spriteTransitionCounter + 1f) % _spriteTransitionDelay;
                if (_spriteTransitionCounter == 0)
                    _currentCharacterSpriteX = (_currentCharacterSpriteX + 1) % _numSpritesWidth;
            }
            else if (right)
            {
                _currentCharacterSpriteY = 2;
                _spriteTransitionCounter = (_spriteTransitionCounter + 1f) % _spriteTransitionDelay;
                if (_spriteTransitionCounter == 0)
                    _currentCharacterSpriteX = (_currentCharacterSpriteX + 1) % _numSpritesWidth;
            }
            else if (up)
            {
                _currentCharacterSpriteY = 3;
                _spriteTransitionCounter = (_spriteTransitionCounter + 1f) % _spriteTransitionDelay;
                if (_spriteTransitionCounter == 0)
                    _currentCharacterSpriteX = (_currentCharacterSpriteX + 1) % _numSpritesWidth;
            }
            else if (down)
            {
                _currentCharacterSpriteY = 0;
                _spriteTransitionCounter = (_spriteTransitionCounter + 1f) % _spriteTransitionDelay;
                if (_spriteTransitionCounter == 0)
                    _currentCharacterSpriteX = (_currentCharacterSpriteX + 1) % _numSpritesWidth;
            }
            else
            {
                _spriteTransitionCounter = 0;
                _currentCharacterSpriteX = 0;
            }
        }

        public Body Body
        {
            get { return _agentBody; }
        }

        public Rectangle Bounds
        {
            get { return _bounds; }
        }
    }
}
