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
using KevinsDemo.ScreenSystem;
using KevinsDemo.CharacterSystem;
using KevinsDemo.EnvironmentSystem;
using KevinsEffects.FullScreenEffects;

namespace KevinsDemo.LevelSystem
{
    class SimpleLevel : PhysicsGameScreen, IDemoScreen
    {
        //Render target for the scene
        private RenderTarget2D _RT;
        //Render target for the post-processed blur effect
        private RenderTarget2D _blurredRT;

        //The character
        private Character _pc;
        //The test building
        private Building _building;

        //The blur effect
        private VariableBlurEffect _blur;

        #region IDemoScreen Members

        public string GetTitle()
        {
            return "Kevin's Game Demo";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Sample of a basic leve for test purposes.");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  - Movement: W, A, S, D");
            sb.AppendLine("  - Exit to menu: Escape");
            return sb.ToString();
        }

        #endregion

        public override void LoadContent()
        {
            base.LoadContent();

            //Initialize the render targets to the viewport size
            _RT = new RenderTarget2D(ScreenManager.GraphicsDevice, ScreenManager.GraphicsDevice.Viewport.Bounds.Width, ScreenManager.GraphicsDevice.Viewport.Bounds.Height, false, ScreenManager.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);
            _blurredRT = new RenderTarget2D(ScreenManager.GraphicsDevice, ScreenManager.GraphicsDevice.Viewport.Bounds.Width, ScreenManager.GraphicsDevice.Viewport.Bounds.Height, false, ScreenManager.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);

            //Create the test building
            _building = new Building(World, ScreenManager.Content);

            //Create the character
            _pc = new Character(World, ScreenManager.Content);

            //Create the blur effect (make it slow so it's not distracting)
            _blur = new VariableBlurEffect(ScreenManager.Content, ScreenManager.GraphicsDevice, ScreenManager.GraphicsDevice.Viewport.Bounds, 30, 300, 6);

            //Make the camera track the character
            Camera.EnableTracking = true;
            Camera.EnablePositionTracking = true;
            Camera.TrackingBody = _pc.Body;

            //There is no gravity
            World.Gravity = Vector2.Zero;
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.GraphicsDevice.SetRenderTarget(_RT);
            ScreenManager.GraphicsDevice.Clear(Color.Black);
            ScreenManager.SpriteBatch.Begin(0, null, null, null, null, null, Camera.View);
            _building.Draw(ScreenManager.SpriteBatch);
            _pc.Draw(ScreenManager.SpriteBatch);
            ScreenManager.SpriteBatch.End();
            _blurredRT = _blur.RenderFrame(ScreenManager.GraphicsDevice,ScreenManager.SpriteBatch,_RT,gameTime);
            //Set the render target to the screen and draw the blurred frame
            ScreenManager.GraphicsDevice.SetRenderTarget(null);
            ScreenManager.SpriteBatch.Begin();
            ScreenManager.SpriteBatch.Draw(_blurredRT, ScreenManager.GraphicsDevice.Viewport.Bounds, Color.White);
            ScreenManager.SpriteBatch.End();
            base.Draw(gameTime);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            _pc.Update(gameTime);
            _blur.Update(gameTime);
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void HandleInput(InputHelper input, GameTime gameTime)
        {
            _pc.HandleInput(input, gameTime);
            base.HandleInput(input, gameTime);
        }
    }
}