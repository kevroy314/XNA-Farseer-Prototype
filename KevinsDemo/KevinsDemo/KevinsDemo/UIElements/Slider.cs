using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using KevinsDemo.ScreenSystem;
using Microsoft.Xna.Framework.Graphics;

namespace KevinsDemo.UIElements
{
    class Slider
    {
        private ScreenManager _screenManager;

        private Vector2 _position;
        private Vector2 _sizeInPx;
        private float _value;
        private Color _color;
        private SliderType _type;

        private Rectangle _bounds;
        private Rectangle _zeroZoneBounds;
        private Rectangle _maxZoneBounds;
        private Rectangle _normalZoneBounds;
        private Rectangle _markerZoneBounds;

        private Vector2 _zeroZoneMax;
        private Vector2 _maxZoneMin;

        private Texture2D _sliderMiddle;
        private Texture2D _sliderEnd;
        private Texture2D _sliderMarker;

        public Slider(ScreenManager screenManager, Vector2 position, Vector2 sizeInPx, float value, Color color, SliderType type)
        {
            _screenManager = screenManager;
            _position = position;
            _sizeInPx = sizeInPx;
            _value = value;
            _color = color;
            _type = type;

            _sliderMiddle = screenManager.Content.Load<Texture2D>("UIElements/sliderMiddle");
            _sliderEnd = screenManager.Content.Load<Texture2D>("UIElements/sliderEnd");
            _sliderMarker = screenManager.Content.Load<Texture2D>("UIElements/sliderMarker");

            if (type == SliderType.HorizontalSlider)
            {
                _zeroZoneMax = Vector2.Multiply(sizeInPx, new Vector2(0.1f, 1.0f)) + position;
                _maxZoneMin = Vector2.Multiply(sizeInPx, new Vector2(0.9f, 1.0f)) + position;
            }
            else if (type == SliderType.VerticalSlider)
            {
                _zeroZoneMax = Vector2.Multiply(sizeInPx, new Vector2(1.0f, 0.1f)) + position;
                _maxZoneMin = Vector2.Multiply(sizeInPx, new Vector2(1.0f, 0.9f)) + position;
            }

            _bounds = new Rectangle((int)position.X, (int)position.Y, (int)sizeInPx.X, (int)sizeInPx.Y);
            _zeroZoneBounds = new Rectangle((int)position.X, (int)position.Y, (int)_zeroZoneMax.X, (int)_zeroZoneMax.Y);
            _zeroZoneBounds = new Rectangle((int)_zeroZoneMax.X, (int)_zeroZoneMax.Y, (int)_maxZoneMin.X, (int)_maxZoneMin.Y);
            _maxZoneBounds = new Rectangle((int)_maxZoneMin.X, (int)_maxZoneMin.Y, (int)sizeInPx.X, (int)sizeInPx.Y);
            _markerZoneBounds = new Rectangle((int)(_normalZoneBounds.Width * _value + position.X), (int)position.Y, _sliderMarker.Width, _sliderMarker.Height);
        }

        public enum SliderType
        {
            HorizontalSlider,
            VerticalSlider
        }

        public bool Update(Vector2 mousePosition)
        {
            int x = (int)mousePosition.X;
            int y = (int)mousePosition.Y;
            if (_bounds.Contains(x,y))
            {
                if (_zeroZoneBounds.Contains(x, y))
                {
                    _value = 0.0f;
                    _markerZoneBounds = new Rectangle((int)(mousePosition.X + _sliderMarker.Width / 2), (int)_position.Y, _sliderMarker.Width, _sliderMarker.Height);
                    return true;
                }
                else if (_maxZoneBounds.Contains(x, y))
                {
                    _value = 1.0f;
                    _markerZoneBounds = new Rectangle((int)(mousePosition.X + _sliderMarker.Width / 2), (int)_position.Y, _sliderMarker.Width, _sliderMarker.Height);
                    return true;
                }
                else if (_normalZoneBounds.Contains(x, y))
                {
                    _value = (x - _zeroZoneBounds.X) / _normalZoneBounds.Width;
                    _markerZoneBounds = new Rectangle((int)(mousePosition.X + _sliderMarker.Width / 2), (int)_position.Y, _sliderMarker.Width, _sliderMarker.Height);
                    return true;
                }
            }
            return false;
        }

        public void Draw()
        {
            _screenManager.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
            _screenManager.SpriteBatch.Draw(_sliderEnd, _zeroZoneBounds, null, _color, 0f, Vector2.Zero, SpriteEffects.None, 0f);
            _screenManager.SpriteBatch.Draw(_sliderMiddle, _normalZoneBounds, null, _color, 0f, Vector2.Zero, SpriteEffects.None, 0f);
            _screenManager.SpriteBatch.Draw(_sliderEnd, _maxZoneBounds, null, _color, 0f, Vector2.Zero, SpriteEffects.FlipHorizontally, 0f);
            _screenManager.SpriteBatch.Draw(_sliderMarker, _markerZoneBounds, null, _color, 0f, Vector2.Zero, SpriteEffects.None, 0f);
            _screenManager.SpriteBatch.End();
        }

        public float Value
        {
            get { return Value; }
            set { _value = value; }
        }
    }
}
