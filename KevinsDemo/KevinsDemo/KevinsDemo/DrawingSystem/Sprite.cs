using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KevinsDemo.DrawingSystem
{
    /// <summary>
    /// The basic sprite object for drawing. It stores an origin (for rotation), Texture2D, and name.
    /// </summary>
    public class Sprite
    {
        #region Variables

        //The name of the sprite
        protected string _name;
        //The texture to be drawn
        protected Texture2D _texture;
        //The origin of the sprite (for rotation)
        protected Vector2 _origin;

        #endregion

        #region Constructors

        public Sprite(string name, Texture2D texture, Vector2 origin)
        {
            _name = name;
            _texture = texture;
            _origin = origin;
        }

        public Sprite(string name, Texture2D texture)
        {
            _name = name;
            _texture = texture;
            _origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
        }

        #endregion

        #region Properties

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public Texture2D Texture
        {
            get { return _texture; }
            set { _texture = value; }
        }

        public Vector2 Origin
        {
            get { return _origin; }
            set { _origin = value; }
        }

        #endregion
    }
}