﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using KevinsDemo.ScreenSystem;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace KevinsDemo.UIElements
{
    public enum SliderType
    {
        HorizontalSlider,
        VerticalSlider
    }

    public class Slider
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

        private MouseState? clickState;

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
            _zeroZoneBounds = new Rectangle((int)position.X, (int)position.Y, (int)(sizeInPx.X * 0.1f), (int)sizeInPx.Y);
            _normalZoneBounds = new Rectangle((int)(position.X + sizeInPx.X * 0.1f), (int)position.Y, (int)(sizeInPx.X * 0.8f), (int)sizeInPx.Y);
            _maxZoneBounds = new Rectangle((int)(position.X + sizeInPx.X * 0.9f), (int)position.Y, (int)(sizeInPx.X * 0.1f), (int)sizeInPx.Y);
            _markerZoneBounds = new Rectangle((int)(_normalZoneBounds.Width * _value + position.X), (int)position.Y, _sliderMarker.Width, (int)sizeInPx.Y);
        }

        public bool Update(MouseState state)
        {
            if (!clickState.HasValue && state.LeftButton == ButtonState.Pressed)
                clickState = state;
            if (state.LeftButton == ButtonState.Released)
            {
                clickState = null;
                return false;
            }

            int x = (int)state.X;
            int y = (int)clickState.Value.Y;
            if (_bounds.Contains(x,y))
            {
                if (_zeroZoneBounds.Contains(x, y))
                {
                    _value = 0.0f;
                    _markerZoneBounds = new Rectangle((int)(_normalZoneBounds.X-_sliderMarker.Width), (int)_position.Y, _sliderMarker.Width, (int)_sizeInPx.Y);
                }
                else if (_maxZoneBounds.Contains(x, y))
                {
                    _value = 1.0f;
                    _markerZoneBounds = new Rectangle((int)_maxZoneBounds.X, (int)_position.Y, _sliderMarker.Width, (int)_sizeInPx.Y);
                }
                else if (_normalZoneBounds.Contains(x, y))
                {
                    _value = (x - _zeroZoneBounds.X) / _normalZoneBounds.Width;
                    _markerZoneBounds = new Rectangle((int)(x - _sliderMarker.Width / 2), (int)_position.Y, _sliderMarker.Width, (int)_sizeInPx.Y);
                }
                return true;
            }
            return false;
        }

        public void Draw()
        {
            _screenManager.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
            _screenManager.SpriteBatch.Draw(_sliderEnd, _zeroZoneBounds, null, Color.Blue, 0f, Vector2.Zero, SpriteEffects.None, 0f);
            _screenManager.SpriteBatch.Draw(_sliderMiddle, _normalZoneBounds, null, _color, 0f, Vector2.Zero, SpriteEffects.None, 0f);
            _screenManager.SpriteBatch.Draw(_sliderEnd, _maxZoneBounds, null, Color.Blue, 0f, Vector2.Zero, SpriteEffects.FlipHorizontally, 0f);
            _screenManager.SpriteBatch.Draw(_sliderMarker, _markerZoneBounds, null, Color.Green, 0f, Vector2.Zero, SpriteEffects.None, 0f);
            _screenManager.SpriteBatch.End();
        }

        public float Value
        {
            get { return Value; }
            set { _value = value; }
        }
    }
}
