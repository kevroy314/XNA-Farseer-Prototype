using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using KevinsDemo.ScreenSystem;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace KevinsDemo.UIElements
{
    #region Public Enums

    /// <summary>
    /// A simple enum representing if we're dealing with a horizontal or vertical slider.
    /// </summary>
    public enum SliderType
    {
        HorizontalSlider,
        VerticalSlider
    }

    #endregion

    /// <summary>
    /// A basic slider example which allows us to contruct a slider control which locks to low or high and
    /// handles mouse drags as expected. Not completely tested and no good graphics in play.
    /// </summary>
    public class Slider
    {
        #region Variables

        //The drawing properties of the slider
        private Vector2 _position;
        private Vector2 _sizeInPx;
        private float _value;
        private Color _color;
        private SliderType _type;

        //The bounds of the various slider components
        private Rectangle _bounds;
        private Rectangle _zeroZoneBounds;
        private Rectangle _maxZoneBounds;
        private Rectangle _normalZoneBounds;
        private Rectangle _markerZoneBounds;

        //Some helper values for the min and max zones
        private Vector2 _zeroZoneMax;
        private Vector2 _maxZoneMin;

        //The textures representing the slider parts
        private Texture2D _sliderMiddle;
        private Texture2D _sliderEnd;
        private Texture2D _sliderMarker;

        //The state of the mouse on the mouse down call
        private MouseState? clickState;

        #endregion

        #region Constructors

        public Slider(ContentManager content, Vector2 position, Vector2 sizeInPx, float value, Color color, SliderType type)
        {
            //Save the properties of the slider
            _position = position;
            _sizeInPx = sizeInPx;
            _value = value;
            _color = color;
            _type = type;

            //Load some basic content
            content.Load<Texture2D>("UIElements/sliderMiddle");
            content.Load<Texture2D>("UIElements/sliderEnd");
            content.Load<Texture2D>("UIElements/sliderMarker");

            //Set the zero zone and max zone values
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

            //Initialize all of the necessary boundries
            _bounds = new Rectangle((int)position.X, (int)position.Y, (int)sizeInPx.X, (int)sizeInPx.Y);
            _zeroZoneBounds = new Rectangle((int)position.X, (int)position.Y, (int)(sizeInPx.X * 0.1f), (int)sizeInPx.Y);
            _normalZoneBounds = new Rectangle((int)(position.X + sizeInPx.X * 0.1f), (int)position.Y, (int)(sizeInPx.X * 0.8f), (int)sizeInPx.Y);
            _maxZoneBounds = new Rectangle((int)(position.X + sizeInPx.X * 0.9f), (int)position.Y, (int)(sizeInPx.X * 0.1f), (int)sizeInPx.Y);
            _markerZoneBounds = new Rectangle((int)(_normalZoneBounds.Width * _value + position.X), (int)position.Y, _sliderMarker.Width, (int)sizeInPx.Y);
        }

        #endregion

        #region Update and Draw Functions

        public bool Update(MouseState state)
        {
            //If the click is new, store it's state
            if (!clickState.HasValue && state.LeftButton == ButtonState.Pressed)
                clickState = state;
            //If there is no click, clear the click state and end the update
            if (state.LeftButton == ButtonState.Released)
            {
                clickState = null;
                //We did not update the slider
                return false;
            }

            //Get the x and y  value (y value is only dependent on first click state)
            int x = (int)state.X;
            int y = (int)clickState.Value.Y;
            //Are we within the overall bounds of the slider?
            if (_bounds.Contains(x,y))
            {
                if (_zeroZoneBounds.Contains(x, y)) //If we're in the zero zone, lock to zero
                {
                    _value = 0.0f;
                    _markerZoneBounds = new Rectangle((int)(_normalZoneBounds.X-_sliderMarker.Width), (int)_position.Y, _sliderMarker.Width, (int)_sizeInPx.Y);
                }
                else if (_maxZoneBounds.Contains(x, y)) //If we're in the max zone, lock to max
                {
                    _value = 1.0f;
                    _markerZoneBounds = new Rectangle((int)_maxZoneBounds.X, (int)_position.Y, _sliderMarker.Width, (int)_sizeInPx.Y);
                }
                else if (_normalZoneBounds.Contains(x, y)) //If we're in the normal zone, scale normally
                {
                    _value = (x - _zeroZoneBounds.X) / _normalZoneBounds.Width;
                    _markerZoneBounds = new Rectangle((int)(x - _sliderMarker.Width / 2), (int)_position.Y, _sliderMarker.Width, (int)_sizeInPx.Y);
                }
                //Return true telling we successfully updated the slider
                return true;
            }
            //We did not update the slider
            return false;
        }

        public void Draw(SpriteBatch batch)
        {
            batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
            batch.Draw(_sliderEnd, _zeroZoneBounds, null, Color.Blue, 0f, Vector2.Zero, SpriteEffects.None, 0f);
            batch.Draw(_sliderMiddle, _normalZoneBounds, null, _color, 0f, Vector2.Zero, SpriteEffects.None, 0f);
            batch.Draw(_sliderEnd, _maxZoneBounds, null, Color.Blue, 0f, Vector2.Zero, SpriteEffects.FlipHorizontally, 0f);
            batch.Draw(_sliderMarker, _markerZoneBounds, null, Color.Green, 0f, Vector2.Zero, SpriteEffects.None, 0f);
            batch.End();
        }

        #endregion

        #region Properties

        //The value of the slider control
        public float Value
        {
            get { return Value; }
            set { _value = value; }
        }

        #endregion
    }
}
