using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FarseerPhysics.Common;
using FarseerPhysics.Common.PolygonManipulation;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Factories;
using VariableBlurEffect;

namespace KevinsDemo
{
    class SimpleLevel : PhysicsGameScreen, IDemoScreen
    {
        private RenderTarget2D RT;
        private RenderTarget2D blurredRT;

        private Body _compound;
        private Vector2 _origin;
        private Texture2D _polygonTexture;
        private float _scale;

        private Body _agentBody;
        private Texture2D _agentTextures;
        private Texture2D _agentCollisionTexture;
        private Vector2 _origin2;

        private VariableBlurEffect.VariableBlurEffect blur;

        #region IDemoScreen Members

        public string GetTitle()
        {
            return "Kevin's Game Demo";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("TODO: Add sample description!");
            sb.AppendLine(string.Empty);
            sb.AppendLine("GamePad:");
            sb.AppendLine("  - Move cursor: left thumbstick");
            sb.AppendLine("  - Grab object (beneath cursor): A button");
            sb.AppendLine("  - Drag grabbed object: left thumbstick");
            sb.AppendLine("  - Exit to menu: Back button");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  - Exit to menu: Escape");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Mouse / Touchscreen");
            sb.AppendLine("  - Grab object (beneath cursor): Left click");
            sb.AppendLine("  - Drag grabbed object: move mouse / finger");
            return sb.ToString();
        }

        #endregion

        public override void LoadContent()
        {
            base.LoadContent();

            RT = new RenderTarget2D(ScreenManager.GraphicsDevice, ScreenManager.GraphicsDevice.Viewport.Bounds.Width, ScreenManager.GraphicsDevice.Viewport.Bounds.Height, false, ScreenManager.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);
            blurredRT = new RenderTarget2D(ScreenManager.GraphicsDevice, ScreenManager.GraphicsDevice.Viewport.Bounds.Width, ScreenManager.GraphicsDevice.Viewport.Bounds.Height, false, ScreenManager.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);

            World.Gravity = Vector2.Zero;
            
            //load texture that will represent the physics body
            _polygonTexture = ScreenManager.Content.Load<Texture2D>("Samples/0");
            _agentTextures = ScreenManager.Content.Load<Texture2D>("chrono");
            _agentCollisionTexture = ScreenManager.Content.Load<Texture2D>("Samples/chrono_collisionBox");



            //Create an array to hold the data from the texture
            uint[] data = new uint[_polygonTexture.Width * _polygonTexture.Height];
            uint[] data2 = new uint[_agentCollisionTexture.Width * _agentCollisionTexture.Height];

            //Transfer the texture data to the array
            _polygonTexture.GetData(data);
            _agentCollisionTexture.GetData(data2);

            //Find the vertices that makes up the outline of the shape in the texture
            Vertices textureVertices = PolygonTools.CreatePolygon(data, _polygonTexture.Width, false);
            Vertices textureVertices2 = PolygonTools.CreatePolygon(data2, _agentCollisionTexture.Width, false);

            //The tool return vertices as they were found in the texture.
            //We need to find the real center (centroid) of the vertices for 2 reasons:

            //1. To translate the vertices so the polygon is centered around the centroid.
            Vector2 centroid = -textureVertices.GetCentroid();
            textureVertices.Translate(ref centroid);
            Vector2 centroid2 = -textureVertices2.GetCentroid();
            textureVertices2.Translate(ref centroid2);

            //2. To draw the texture the correct place.
            _origin = -centroid;
            _origin2 = -centroid2;
            //_origin2 = new Vector2(0.0f, 0.0f);

            //We simplify the vertices found in the texture.
            textureVertices = SimplifyTools.ReduceByDistance(textureVertices, 4f);
            textureVertices2 = SimplifyTools.ReduceByDistance(textureVertices2, 4f);

            //Since it is a concave polygon, we need to partition it into several smaller convex polygons
            List<Vertices> list = BayazitDecomposer.ConvexPartition(textureVertices);
            List<Vertices> list2 = BayazitDecomposer.ConvexPartition(textureVertices2);

            //Adjust the scale of the object
            _scale = 1f;

            //scale the vertices from graphics space to sim space
            Vector2 vertScale = new Vector2(ConvertUnits.ToSimUnits(1)) * _scale;
            foreach (Vertices vertices in list)
            {
                vertices.Scale(ref vertScale);
            }
            Vector2 vertScale2 = new Vector2(ConvertUnits.ToSimUnits(1)) * _scale;
            foreach (Vertices vertices in list2)
            {
                vertices.Scale(ref vertScale2);
            }

            //Create a single body with multiple fixtures
            _compound = BodyFactory.CreateCompoundPolygon(World, list, 1f, BodyType.Dynamic);
            _compound.BodyType = BodyType.Static;
            _compound.CollidesWith = Category.All;

            _agentBody = BodyFactory.CreateCompoundPolygon(World, list2, 1f, BodyType.Dynamic);
            _agentBody.BodyType = BodyType.Dynamic;
            _agentBody.CollidesWith = Category.All;
            _agentBody.FixedRotation = true;
            _agentBody.LinearDamping = 10f;
            _agentBody.Position += new Vector2(10f, 10f);

            blur = new VariableBlurEffect.VariableBlurEffect(ScreenManager.Content, ScreenManager.GraphicsDevice, ScreenManager.GraphicsDevice.Viewport.Bounds, 30, 300, 6);

            Camera.EnableTracking = true;
            Camera.EnablePositionTracking = true;
            Camera.TrackingBody = _agentBody;
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.GraphicsDevice.SetRenderTarget(RT);
            ScreenManager.GraphicsDevice.Clear(Color.Black);
            ScreenManager.SpriteBatch.Begin(0, null, null, null, null, null, Camera.View);
            ScreenManager.SpriteBatch.Draw(_polygonTexture, ConvertUnits.ToDisplayUnits(_compound.Position),
                                           null, Color.White, _compound.Rotation, _origin, _scale, SpriteEffects.None,
                                           0f);
            ScreenManager.SpriteBatch.Draw(_agentTextures, ConvertUnits.ToDisplayUnits(_agentBody.Position),
                                           null, Color.White, _agentBody.Rotation, _origin2, _scale, SpriteEffects.None, 
                                           0f);
            ScreenManager.SpriteBatch.End();
            blurredRT = blur.RenderFrame(ScreenManager.GraphicsDevice,ScreenManager.SpriteBatch,RT,gameTime);
            //Set the render target to the screen and draw the blurred frame
            ScreenManager.GraphicsDevice.SetRenderTarget(null);
            ScreenManager.SpriteBatch.Begin();
            ScreenManager.SpriteBatch.Draw(blurredRT, ScreenManager.GraphicsDevice.Viewport.Bounds, Color.White);
            ScreenManager.SpriteBatch.End();
            base.Draw(gameTime);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            blur.Update(gameTime);
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        static float speed = 0.4f;
        public override void HandleInput(InputHelper input, GameTime gameTime)
        {
            if(input.KeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.W))
                _agentBody.ApplyLinearImpulse(new Vector2(0.0f, -speed));
            if(input.KeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.A))
                _agentBody.ApplyLinearImpulse(new Vector2(-speed, 0.0f));
            if(input.KeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.S))
                _agentBody.ApplyLinearImpulse(new Vector2(0.0f, speed));
            if(input.KeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D))
                _agentBody.ApplyLinearImpulse(new Vector2(speed, 0.0f));

            base.HandleInput(input, gameTime);
        }
    }
}