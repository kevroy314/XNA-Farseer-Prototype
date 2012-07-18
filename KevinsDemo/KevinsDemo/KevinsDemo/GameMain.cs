using System;
using Microsoft.Xna.Framework;
using KevinsDemo.LevelSystem;
using KevinsDemo.ScreenSystem;

namespace KevinsDemo
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class GameMain : Game
    {
        private GraphicsDeviceManager _graphics;

        public GameMain()
        {
            Window.Title = "Kevin's Game Demo";
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferMultiSampling = true;
#if WINDOWS || XBOX
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            ConvertUnits.SetDisplayUnitToSimUnitRatio(24f);
            IsFixedTimeStep = true;
#elif WINDOWS_PHONE
            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 480;
            ConvertUnits.SetDisplayUnitToSimUnitRatio(16f);
            IsFixedTimeStep = false;
#endif
#if WINDOWS
            _graphics.IsFullScreen = false;
#elif XBOX || WINDOWS_PHONE
            _graphics.IsFullScreen = true;
#endif

            Content.RootDirectory = "Content";

            //new-up components and add to Game.Components
            ScreenManager = new ScreenManager(this);
            Components.Add(ScreenManager);

            FrameRateCounter frameRateCounter = new FrameRateCounter(ScreenManager);
            frameRateCounter.DrawOrder = 101;
            Components.Add(frameRateCounter);
        }

        public ScreenManager ScreenManager { get; set; }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            SimpleLevel samplelevel1 = new SimpleLevel();

            MenuScreen menuScreen = new MenuScreen("");

            menuScreen.AddMenuItem("", EntryType.Separator, null);
            menuScreen.AddMenuItem("", EntryType.Separator, null);
            menuScreen.AddMenuItem("", EntryType.Separator, null);

            menuScreen.AddMenuItem("Game Levels", EntryType.Separator, null);
            menuScreen.AddMenuItem(samplelevel1.GetTitle(), EntryType.Screen, samplelevel1);

            menuScreen.AddMenuItem("", EntryType.Separator, null);
            menuScreen.AddMenuItem("", EntryType.Separator, null);
            menuScreen.AddMenuItem("", EntryType.Separator, null);
            menuScreen.AddMenuItem("Exit", EntryType.ExitItem, null);
            
            ScreenManager.AddScreen(new BackgroundScreen());
            ScreenManager.AddScreen(menuScreen);
            ScreenManager.AddScreen(new LogoScreen(TimeSpan.FromSeconds(3.0)));
        }
    }
}