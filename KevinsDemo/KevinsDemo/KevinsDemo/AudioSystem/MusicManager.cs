using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Media;
using KevinsDemo.ScreenSystem;

namespace KevinsDemo.AudioSystem
{
    public class MusicManager
    {
        #region Variables

        private static Song _backgroundMusic;

        #endregion

        #region Initialization

        public static void InitializeMusicManager()
        {
            setBackgroundMusicToRandomChrono();
            MediaPlayer.MediaStateChanged += new EventHandler<EventArgs>(MediaPlayer_MediaStateChanged);
        }

        #endregion

        #region Private Functions

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

        private static void MediaPlayer_MediaStateChanged(object sender, EventArgs e)
        {
            if (MediaPlayer.State == MediaState.Stopped)
                setBackgroundMusicToRandomChrono();
        }

        #endregion

        #region Accessor and Mutator Methods

        public static void Mute()
        {
            MediaPlayer.IsMuted = true;
        }

        public static void Unmute()
        {
            MediaPlayer.IsMuted = false;
        }

        public static void ToggleMute()
        {
            MediaPlayer.IsMuted = !MediaPlayer.IsMuted;
        }

        #endregion

        #region Properties

        public static bool IsMuted
        {
            get { return MediaPlayer.IsMuted; }
            set { MediaPlayer.IsMuted = value; }
        }

        #endregion
    }
}
