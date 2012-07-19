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
        private Body _compound;
        private Vector2 _origin;
        private Texture2D _polygonTexture;
        private float _scale;
        private bool flipHorizontal;

        public Building(World world, ContentManager content, Vector2 position, float rotation, bool flippedHorizontally, string contentItem)
        {
            flipHorizontal = flippedHorizontally;

            //load texture that will represent the physics body
            _polygonTexture = content.Load<Texture2D>(contentItem);

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
            _compound = BodyFactory.CreateCompoundPolygon(world, list, 1f, BodyType.Dynamic);
            _compound.BodyType = BodyType.Static;
            _compound.CollidesWith = Category.All;
            _compound.Position = position;
            _compound.Rotation = rotation;
        }

        public void Draw(SpriteBatch batch)
        {
            batch.Draw(_polygonTexture, ConvertUnits.ToDisplayUnits(_compound.Position),
                null, Color.White, _compound.Rotation, _origin, _scale, flipHorizontal ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                                           0f);
        }
    }
}
