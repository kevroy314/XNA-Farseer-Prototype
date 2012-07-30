using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KevinsDemo.DrawingSystem
{
    #region Public Enums

    /// <summary>
    /// An enum which can be used to specify the state of a sprite animation.
    /// </summary>
    public enum SpriteAnimationState
    {
        Running,
        Paused,
        Stopped
    }

    #endregion

    /// <summary>
    /// The basic sprite animation class. 
    /// It can animate a sprite sheet given a series of options including speed, number of frames, and draw options.
    /// </summary>
    class SpriteAnimation : Sprite
    {
        #region Variables

        //Read/Write General Properties
        private float _framesPerSecond;
        private bool _isLooping;
        private bool _visible;

        //Read/Write Draw Properties
        private Color _tintColor;
        private float _rotation;
        private float _scale;
        private SpriteEffects _spriteEffects;
        private float _layerDepth;

        //Read Only Properties
        private SpriteAnimationState _state;

        //Sprite Sheet and Dimension Properties
        private Rectangle[] _frameBounds;
        private int _frameWidthInPx, _frameHeightInPx;
        private int _textureWidthInFrames, _textureHeightInFrames;
        private int _frameCount;

        //State Propetiers
        private float _totalElapsedTime;
        private int _currentFrame;
        private float _frameInterval;

        #endregion

        #region Constructors

        public SpriteAnimation(string name, Texture2D texture, int textureWidthInFrames, int textureHeightInFrames, float framesPerSecond, bool isLooping)
            : base(name, texture)
        {
            //The animation starts in a stop state
            _state = SpriteAnimationState.Stopped;

            //Set the member variables
            _textureWidthInFrames = textureWidthInFrames;
            _textureHeightInFrames = textureHeightInFrames;

            //Calculate the size in pixels (assume equal sized frames)
            _frameWidthInPx = texture.Bounds.Width / _textureWidthInFrames;
            _frameHeightInPx = texture.Bounds.Height / _textureHeightInFrames;

            //Calculate the total frame count
            _frameCount = textureWidthInFrames * textureHeightInFrames;

            //Generate the boundries for each frame (used as source Rectangle in draw stage)
            _frameBounds = new Rectangle[_frameCount];
            for (int i = 0; i < _frameBounds.Length; i++)
                _frameBounds[i] = new Rectangle(_frameWidthInPx * (i % _textureWidthInFrames), _frameHeightInPx * ((int)Math.Floor((double)(i / _textureWidthInFrames))), _frameWidthInPx, _frameHeightInPx);

            //Calculate the frame interval based on FPS
            _framesPerSecond = framesPerSecond;
            _frameInterval = 1 / _framesPerSecond;

            //Set member variables
            _isLooping = isLooping;

            //Make it visible
            _visible = true;

            //Set default drawing properties
            _tintColor = Color.White;
            _rotation = 0f;
            _scale = 1f;
            _spriteEffects = Microsoft.Xna.Framework.Graphics.SpriteEffects.None;
            _layerDepth = 0;
        }

        public SpriteAnimation(string name, Texture2D texture, Vector2 origin, int textureWidthInFrames, int textureHeightInFrames, float framesPerSecond, bool isLooping)
            : this(name,texture,textureWidthInFrames,textureHeightInFrames,framesPerSecond,isLooping)
        {
            base._origin = origin;
        }

        #endregion

        #region Accessor and Mutator Methods

        //Start the animation
        public void Play()
        {
            resetAnimation();
            _state = SpriteAnimationState.Running;
        }

        //Pause the animation without resetting it
        public void Pause()
        {
            _state = SpriteAnimationState.Paused;
        }
        
        //Continue playing without resetting the animation
        public void Resume()
        {
            _state = SpriteAnimationState.Running;
        }

        //Stop the animation and reset it
        public void Stop()
        {
           _state = SpriteAnimationState.Stopped;
           resetAnimation();
        }

        #endregion

        #region Private Animation Functions

        //Reset the animation
        private void resetAnimation()
        {
            _currentFrame = 0;
        }

        //Iterate the frame (loop if specified)
        private void advanceToNextValidFrame()
        {
            _currentFrame++;
            if (_isLooping)
                _currentFrame %= _frameCount;
            else if (_currentFrame >= _frameCount)
                _currentFrame = _frameCount - 1;
        }

        //Check the game time to determine if we should advance to the next frame
        private bool isTimeToAdvanceFrame(GameTime gameTime)
        {
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _totalElapsedTime += elapsedTime;
            if (_totalElapsedTime > _frameInterval)
            {
                _totalElapsedTime -= _frameInterval;
                return true;
            }

            return false;
        }

        //Run the animation
        private void runAnimation(GameTime gameTime)
        {
            if (isTimeToAdvanceFrame(gameTime))
                advanceToNextValidFrame();
        }

        #endregion

        #region Update and Draw Functions

        public void Update(GameTime gameTime)
        {
            //Run the animation if our state says to
            if (_state == SpriteAnimationState.Running)
            {
                runAnimation(gameTime);
            }
        }

        public void Draw(SpriteBatch batch, Vector2 position)
        {
            //Only draw if visible
            if(_visible)
                batch.Draw(_texture, position, _frameBounds[_currentFrame], _tintColor, _rotation, _origin, _scale, _spriteEffects, _layerDepth);
        }

        #endregion

        #region Properties

        //The frames per second the animation runs at (also sets the FrameInterval)
        public float FramesPerSecond
        {
            get { return _framesPerSecond; }
            set 
            {
                _framesPerSecond = value;
                _frameInterval = 1 / _framesPerSecond;
            }
        }

        //The frame interval the animation runs at (also sets the Frames Per Second)
        public float FrameInterval
        {
            get { return _frameInterval; }
            set
            {
                _frameInterval = value;
                _framesPerSecond = 1 / _frameInterval;
            }
        }

        //Does the animation loop?
        public bool IsLooping
        {
            get { return _isLooping; }
            set { _isLooping = value; }
        }

        //The state of the animation
        public SpriteAnimationState AnimationState
        {
            get { return _state; }
        }

        //The tint color to draw the sprite (default is it's natural color)
        public Color TintColor
        {
            get { return _tintColor; }
            set { _tintColor = value; }
        }

        //The rotation of the sprite
        public float Rotation
        {
            get { return _rotation; }
            set { _rotation = value; }
        }

        //The draw scale of the sprite
        public float Scale
        {
            get { return _scale; }
            set { _scale = value; }
        }

        //The flip state of the sprite
        public SpriteEffects SpriteEffects
        {
            get { return _spriteEffects; }
            set { _spriteEffects = value; }
        }

        //The draw layer depth of the sprite
        public float LayerDepth
        {
            get { return _layerDepth; }
            set { _layerDepth = value; }
        }

        //The current visibility
        public bool Visible
        {
            get { return _visible; }
            set { _visible = value; }
        }

        #endregion
    }
}
