﻿using System;
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
using KevinsDemo.FullScreenEffects;
using DPSF;
using DPSF.ParticleSystems;
using ParticleObjects;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using KevinsDemo.NPCs.Enemies;
using KevinsDemo.AudioSystem;

namespace KevinsDemo.LevelSystem
{
    class SimpleLevel : PhysicsGameScreen, IDemoScreen
    {
        //Render target for the scene
        private RenderTarget2D _RT;
        //Render target for the post-processed blur effect
        private RenderTarget2D _blurredRT;

        private SoundEffect _heartBeat;

        //The character
        private Character _pc;
        //The test building
        private Building[] _buildings;

        //The blur effect
        private VariableBlurEffect _blur;

        //The campfire
        private Campfire _fire;
        //private SpriteParticleSystem _spriteParticles;

        private SimpleTestEnemy _enemy;

        private Game _parentGame;

        private SnowParticleSystem _snow;

        public SimpleLevel(Game g)
        {
            _parentGame = g;
        }

        #region IDemoScreen Members

        public string GetTitle()
        {
            return "Kevin's Game Demo";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Sample of a basic level for test purposes.");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  - Movement: W, A, S, D");
            sb.AppendLine("  - Exit to menu: Escape");
            return sb.ToString();
        }

        #endregion

        private string[] songs = { };

        public override void LoadContent()
        {
            base.LoadContent();
            
            //Initialize the render targets to the viewport size
            _RT = new RenderTarget2D(ScreenManager.GraphicsDevice, ScreenManager.GraphicsDevice.Viewport.Bounds.Width, ScreenManager.GraphicsDevice.Viewport.Bounds.Height, false, ScreenManager.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);
            _blurredRT = new RenderTarget2D(ScreenManager.GraphicsDevice, ScreenManager.GraphicsDevice.Viewport.Bounds.Width, ScreenManager.GraphicsDevice.Viewport.Bounds.Height, false, ScreenManager.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);

            _heartBeat = ScreenManager.Content.Load<SoundEffect>("SoundEffects/doublebeat");

            //Create the test buildings
            _buildings = new Building[6];
            string[] buildingTypes = { "pavilion_45", "hunter_315", "hunter_225", "storagetent_135", "lumberjack_45", "hunter_45" };
            for (int i = 0; i < _buildings.Length; i++)
            {
                float cos = (float)Math.Cos(((float)i / (float)_buildings.Length) * Math.PI * 2);
                float sin = (float)Math.Sin(((float)i / (float)_buildings.Length) * Math.PI * 2);
                float r = 500f;
                float xpos = cos * r;
                float ypos = sin * r;
                _buildings[i] = new Building(World, ScreenManager.Content, new Vector2(xpos, ypos), "EnvironmentObjects/Tents/" + buildingTypes[i], "EnvironmentObjects/Tents/" + buildingTypes[i]+"_collisions");
            }

            _fire = new Campfire(_parentGame, World, ScreenManager.SpriteBatch, Vector2.Zero);
            
            //Create the character
            _pc = new Character(World, ScreenManager.Content, Vector2.Zero);
            _enemy = new SimpleTestEnemy(_parentGame, ScreenManager.Content, Camera.ConvertWorldToScreen(new Vector2(10f, 10f)), 1500);

            //Create the blur effect (make it slow so it's not distracting)
            _blur = new VariableBlurEffect(ScreenManager.Content, ScreenManager.GraphicsDevice, ScreenManager.GraphicsDevice.Viewport.Bounds, 30, 300, 6);

            //Make the camera track the character
            Camera.EnableTracking = true;
            Camera.EnablePositionTracking = true;
            Camera.TrackingBody = _pc.Body;

            //There is no gravity
            World.Gravity = Vector2.Zero;

            _snow = new SnowParticleSystem(_parentGame);
            _snow.AutoInitialize(ScreenManager.GraphicsDevice, ScreenManager.Content, ScreenManager.SpriteBatch);

            MusicManager.InitializeMusicManager();
        }

        public override void UnloadContent()
        {
            _blur.UnloadContent();
            _fire.UnloadContent();
            _enemy.UnloadContent();
            base.UnloadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.GraphicsDevice.SetRenderTarget(_RT);

            ScreenManager.GraphicsDevice.Clear(Color.Black);
            
            ScreenManager.SpriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Camera.View);
            for(int i = 0; i < _buildings.Length;i++)
                _buildings[i].Draw(ScreenManager.SpriteBatch);
            _fire.Draw(ScreenManager.SpriteBatch);
            ScreenManager.SpriteBatch.End();

            ScreenManager.SpriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Camera.View);
            _pc.Draw(ScreenManager.SpriteBatch);
            _enemy.Draw(ScreenManager.SpriteBatch);
            ScreenManager.SpriteBatch.End();

            _fire.DrawParticles();
            _enemy.DrawParticles();


            // Set up the Camera's View matrix
            Vector3 cPos = new Vector3(Camera.Position.X / 4, -Camera.Position.Y / 4, 200f);
            Matrix sViewMatrix = Matrix.CreateLookAt(cPos, new Vector3(Camera.Position.X / 4, -Camera.Position.Y / 4, 0f), Vector3.Up);

            // Setup the Camera's Projection matrix by specifying the field of view (1/4 pi), aspect ratio, and the near and far clipping planes
            Matrix sProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, (float)_parentGame.GraphicsDevice.Viewport.Width / (float)_parentGame.GraphicsDevice.Viewport.Height, 1, 10000);

            // Draw the Particle System
            _snow.SetWorldViewProjectionMatrices(Matrix.Identity, sViewMatrix, sProjectionMatrix);
            _snow.SetCameraPosition(cPos);
            _snow.Draw();

            _blurredRT = _blur.RenderFrame(ScreenManager.GraphicsDevice,ScreenManager.SpriteBatch,_RT,gameTime);

            //Set the render target to the screen and draw the blurred frame
            ScreenManager.GraphicsDevice.SetRenderTarget(null);
            ScreenManager.SpriteBatch.Begin();
            ScreenManager.SpriteBatch.Draw(_blurredRT, ScreenManager.GraphicsDevice.Viewport.Bounds, Color.White);
            ScreenManager.SpriteBatch.End();

            base.Draw(gameTime);
            ScreenManager.SpriteBatch.Begin(SpriteSortMode.Deferred,null,null,null,null,null,Camera.View);
            ScreenManager.SpriteBatch.DrawString(ScreenManager.Fonts.DetailsFont, "(" + mouseX + ", " + mouseY + ")", new Vector2(mouseX, mouseY), Color.White);
            ScreenManager.SpriteBatch.End();
        }
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            _pc.Update(gameTime);
            float blurProgress = _blur.Update(gameTime);
            if (MathHelper.Distance(blurProgress, 0.25f) < 0.05f)
                SoundEffectsManager.Play(_heartBeat);
            _fire.Update(gameTime, Camera);
            Vector2 screenPos = Camera.ConvertWorldToScreen(_pc.Body.Position);
            if(moveEnemy)
                _enemy.Update(gameTime, Camera);
            _snow.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            _snow.Emitter.PositionData.Position = new Vector3(Camera.Position, 0f);
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }
        private int mouseX, mouseY;
        private bool moveEnemy = true;
        public override void HandleInput(InputHelper input, GameTime gameTime)
        {
            if (input.MouseState.MiddleButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                moveEnemy = false;
            else
                moveEnemy = true;
            Vector2 loc = Camera.ConvertScreenToWorld(new Vector2(input.MouseState.X, input.MouseState.Y));
            mouseX = (int)loc.X;
            mouseY = (int)loc.Y;
            _enemy.Position = loc;
            _pc.HandleInput(input, gameTime);
            base.HandleInput(input, gameTime);
        }
    }
}