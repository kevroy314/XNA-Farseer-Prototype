using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KevinsDemo.ScreenSystem
{
    /// <summary>
    /// Base class for screens that contain a menu of options. The user can
    /// move up and down to select an entry, or cancel to back out of the screen.
    /// </summary>
    public class OptionsMenu : MenuScreen
    {

        /// <summary>
        /// Constructor.
        /// </summary>
        public OptionsMenu(string menuTitle, bool isMainMenu)
            : base(menuTitle, isMainMenu) { }

        public void AddMenuItem(MenuEntry.SettingsChangeDisplayUpdate displayChangeUpdate, EntryType type, MenuEntry.SettingsChangeDelegate settingsChange, object settingsChangeParameter, bool optionRequiresRestart)
        {
            MenuEntry entry = new MenuEntry(this, displayChangeUpdate, type, settingsChange, optionRequiresRestart);
            _menuEntries.Add(entry);
        }

        /// <summary>
        /// Responds to user input, changing the selected entry and accepting
        /// or cancelling the menu.
        /// </summary>
        public override void HandleInput(InputHelper input, GameTime gameTime)
        {
            base.HandleInput(input, gameTime);

            if (input.IsMenuSelect() && _selectedEntry != -1)
            {
                if (_menuEntries[_selectedEntry].SettingsChange != null)
                {
                    _menuEntries[_selectedEntry].ModifyOption(null);
                    if (_menuEntries[_selectedEntry].OptionRequiresRestart)
                        ScreenManager.AddScreen(new MessageBoxScreen("Changing this option requires you to restart the game for it to take effect.", MessageBoxViewportAlignment.Center));
                }
            }
        }

        public override void ExitScreen()
        {
            base.ExitScreen();
            GlobalGameOptions.SaveOptionsToFile();
        }
    }
}