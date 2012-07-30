using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Media;
using KevinsDemo.ScreenSystem;

namespace KevinsDemo.AudioSystem
{
    /// <summary>
    /// This class manages the music in the game. 
    /// It only needs access to the content manager to function properly.
    /// </summary>
    public class MusicManager
    {
        #region Variables

        //The current song playing (or paused/stopped)
        private static Song _backgroundMusic;

        #endregion

        #region Initialization

        //Called to initialize autoplaying music in the background amongst a playlist
        public static void InitializeMusicManager()
        {
            setBackgroundMusicToRandomChrono();
            MediaPlayer.MediaStateChanged += new EventHandler<EventArgs>(MediaPlayer_MediaStateChanged);
        }

        #endregion

        #region Private Functions

        //Called to set the background music to one of the chrono songs
        private static void setBackgroundMusicToRandomChrono()
        {
            Random rand = new Random();
            int songNum = rand.Next(1, 63);
            bool worked = true;
            do
            {
                try
                {
                    _backgroundMusic = GameMain.ScreenManager.Content.Load<Song>("Music/" + songNum);
                    worked = true;
                }
                catch (Exception) { worked = false; }
            } while (!worked);
            MediaPlayer.Play(_backgroundMusic);
        }

        #endregion

        #region Events

        //Called to iterate to a new song when the current song ends
        private static void MediaPlayer_MediaStateChanged(object sender, EventArgs e)
        {
            if (MediaPlayer.State == MediaState.Stopped)
                setBackgroundMusicToRandomChrono();
        }

        #endregion

        #region Accessor and Mutator Methods

        //Mutes the music
        public static void Mute()
        {
            MediaPlayer.IsMuted = true;
        }

        //Unmutes the music
        public static void Unmute()
        {
            MediaPlayer.IsMuted = false;
        }

        //Toggles the mute of the music
        public static void ToggleMute()
        {
            MediaPlayer.IsMuted = !MediaPlayer.IsMuted;
        }

        #endregion

        #region Properties

        //Gets or sets the mute of the music
        public static bool IsMuted
        {
            get { return MediaPlayer.IsMuted; }
            set { MediaPlayer.IsMuted = value; }
        }

        //Gets or sets the volume of the media player
        public static float Volume
        {
            get { return MediaPlayer.Volume; }
            set { MediaPlayer.Volume = value; }
        }

        //Gets the state of the background music
        public static MediaState State
        {
            get { return MediaPlayer.State; }
        }

        #endregion
    }
}
