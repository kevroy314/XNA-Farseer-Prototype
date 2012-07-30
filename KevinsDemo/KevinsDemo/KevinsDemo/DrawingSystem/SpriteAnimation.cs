using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KevinsDemo.DrawingSystem
{
    public enum SpriteAnimationState
    {
        Running,
        Paused,
        Stopped
    }

    class SpriteAnimation : Sprite
    {
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

        public SpriteAnimation(string name, Texture2D texture, int textureWidthInFrames, int textureHeightInFrames, float framesPerSecond, bool isLooping)
            : base(name, texture)
        {
            _state = SpriteAnimationState.Stopped;

            _textureWidthInFrames = textureWidthInFrames;
            _textureHeightInFrames = textureHeightInFrames;

            _frameWidthInPx = texture.Bounds.Width / _textureWidthInFrames;
            _frameHeightInPx = texture.Bounds.Height / _textureHeightInFrames;

            _frameCount = textureWidthInFrames * textureHeightInFrames;

            _frameBounds = new Rectangle[_frameCount];
            for (int i = 0; i < _frameBounds.Length; i++)
                _frameBounds[i] = new Rectangle(_frameWidthInPx * (i % _textureWidthInFrames), _frameHeightInPx * ((int)Math.Floor((double)(i / _textureWidthInFrames))), _frameWidthInPx, _frameHeightInPx);

            _framesPerSecond = framesPerSecond;
            _frameInterval = 1 / _framesPerSecond;

            _isLooping = isLooping;

            _visible = true;

            _tintColor = Color.White;
            _rotation = 0f;
            _scale = 1f;
            _spriteEffects = Microsoft.Xna.Framework.Graphics.SpriteEffects.None;
            _layerDepth = 0;
        }

        public SpriteAnimation(string name, Texture2D texture, Vector2 origin, int textureWidthInFrames, int textureHeightInFrames, float framesPerSecond, bool isLooping)
            : base(name, texture, origin)
        {
            _state = SpriteAnimationState.Stopped;

            _textureWidthInFrames = textureWidthInFrames;
            _textureHeightInFrames = textureHeightInFrames;

            _frameWidthInPx = texture.Bounds.Width / _textureWidthInFrames;
            _frameHeightInPx = texture.Bounds.Height / _textureHeightInFrames;

            _frameCount = textureWidthInFrames * textureHeightInFrames;

            _frameBounds = new Rectangle[_frameCount];
            for (int i = 0; i < _frameBounds.Length; i++)
                _frameBounds[i] = new Rectangle(_frameWidthInPx * (i % _textureWidthInFrames), _frameHeightInPx * ((int)Math.Floor((double)(i / _textureWidthInFrames))), _frameWidthInPx, _frameHeightInPx);

            _framesPerSecond = framesPerSecond;
            _frameInterval = 1 / _framesPerSecond;

            _isLooping = isLooping;

            _visible = true;

            _tintColor = Color.White;
            _rotation = 0f;
            _scale = 1f;
            _spriteEffects = Microsoft.Xna.Framework.Graphics.SpriteEffects.None;
            _layerDepth = 0;
        }

        public void Play()
        {
            resetAnimation();
            _state = SpriteAnimationState.Running;
        }
        public void Pause()
        {
            _state = SpriteAnimationState.Paused;
        }
        public void Resume()
        {
            _state = SpriteAnimationState.Running;
        }
        public void Stop()
        {
           _state = SpriteAnimationState.Stopped;
           resetAnimation();
        }

        private void resetAnimation()
        {
            _currentFrame = 0;
        }

        public void Update(GameTime gameTime)
        {
            if (_state == SpriteAnimationState.Running)
            {
                runAnimation(gameTime);
            }
        }

        public void Draw(SpriteBatch batch, Vector2 position)
        {
            if(_visible)
                batch.Draw(_texture, position, _frameBounds[_currentFrame], _tintColor, _rotation, _origin, _scale, _spriteEffects, _layerDepth);
        }

        private void runAnimation(GameTime gameTime)
        {
            if (isTimeToAdvanceFrame(gameTime))
                advanceToNextValidFrame();
        }

        private void advanceToNextValidFrame()
        {
            _currentFrame++;
            if (_isLooping)
                _currentFrame %= _frameCount;
            else if (_currentFrame >= _frameCount)
                _currentFrame = _frameCount - 1;
        }

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

        public float FramesPerSecond
        {
            get { return _framesPerSecond; }
            set 
            {
                _framesPerSecond = value;
                _frameInterval = 1 / _framesPerSecond;
            }
        }

        public float FrameInterval
        {
            get { return _frameInterval; }
            set
            {
                _frameInterval = value;
                _framesPerSecond = 1 / _frameInterval;
            }
        }

        public bool IsLooping
        {
            get { return _isLooping; }
            set { _isLooping = value; }
        }

        public SpriteAnimationState AnimationState
        {
            get { return _state; }
        }

        public Color TintColor
        {
            get { return _tintColor; }
            set { _tintColor = value; }
        }

        public float Rotation
        {
            get { return _rotation; }
            set { _rotation = value; }
        }

        public float Scale
        {
            get { return _scale; }
            set { _scale = value; }
        }

        public SpriteEffects SpriteEffects
        {
            get { return _spriteEffects; }
            set { _spriteEffects = value; }
        }

        public float LayerDepth
        {
            get { return _layerDepth; }
            set { _layerDepth = value; }
        }

        public bool Visible
        {
            get { return _visible; }
            set { _visible = value; }
        }
    }
}
