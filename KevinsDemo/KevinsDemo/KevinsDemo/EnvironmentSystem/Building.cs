using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using FarseerPhysics.Factories;
using KevinsDemo.ScreenSystem;
using FarseerPhysics.Common;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Common.PolygonManipulation;

namespace KevinsDemo.EnvironmentSystem
{
    class Building
    {
        //The physical body of the building
        private Body _body;

        public Body Body
        {
            get { return _body; }
            set { _body = value; }
        }

        //The polygon texture for collisions
        private Texture2D _polygonTexture;
        //The draw texture for rendering
        private Texture2D _drawTexture;

        //The tint color to draw the sprite
        private Color _tintColor;
        //The origin of the building
        private Vector2 _origin;
        //The scale of the building
        private float _scale;
        //The sprite effects for flipped drawing
        private SpriteEffects _spriteEffects;
        //The depth layer for rendering
        private float _layerDepth;

        public Building(World world, ContentManager content, Vector2 position, string drawTextureContentItem, string collisionTextureContentItem)
        {
            //load texture that will represent the physics body
            try
            {
                _polygonTexture = content.Load<Texture2D>(collisionTextureContentItem);
            }
            catch (Exception) 
            {
                _polygonTexture = content.Load<Texture2D>(drawTextureContentItem); 
            }
            _drawTexture = content.Load<Texture2D>(drawTextureContentItem);

            //Create an array to hold the data from the texture
            uint[] data = new uint[_polygonTexture.Width * _polygonTexture.Height];

            //Transfer the texture data to the array
            _polygonTexture.GetData(data);

            //Find the vertices that makes up the outline of the shape in the texture
            Vertices textureVertices = PolygonTools.CreatePolygon(data, _polygonTexture.Width, false);

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
            _scale = 2f;

            //scale the vertices from graphics space to sim space
            Vector2 vertScale = new Vector2(ConvertUnits.ToSimUnits(1)) * _scale;
            foreach (Vertices vertices in list)
            {
                vertices.Scale(ref vertScale);
            }

            //Create a single body with multiple fixtures
            _body = BodyFactory.CreateCompoundPolygon(world, list, 1f, BodyType.Dynamic);
            _body.BodyType = BodyType.Static;
            _body.CollidesWith = Category.All;
            _body.Position = position;
        }

        public void Draw(SpriteBatch batch)
        {
            batch.Draw(_drawTexture, ConvertUnits.ToDisplayUnits(_body.Position), null, _tintColor, _body.Rotation, _origin, _scale, _spriteEffects, _layerDepth);
        }

        public float Rotation
        {
            get { return _body.Rotation; }
            set { _body.Rotation = value; }
        }

        public Color TintColor
        {
            get { return _tintColor; }
            set { _tintColor = value; }
        }

        public Vector2 Origin
        {
            get { return _origin; }
            set { _origin = value; }
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
    }
}
