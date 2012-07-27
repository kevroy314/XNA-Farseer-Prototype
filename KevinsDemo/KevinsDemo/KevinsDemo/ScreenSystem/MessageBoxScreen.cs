using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace KevinsDemo.ScreenSystem
{

    public enum MessageBoxViewportAlignment
    {
        BottomLeft,
        BottomRight,
        Center,
        CenterBottom,
        CenterLeft,
        CenterRight,
        CenterTop,
        Manual,
        TopLeft,
        TopRight
    }

    /// <summary>
    /// A popup message box screen, used to display "are you sure?"
    /// confirmation messages.
    /// </summary>
    public class MessageBoxScreen : GameScreen
    {
        private Rectangle _backgroundRectangle;
        private Texture2D _gradientTexture;
        private string _message;
        private Vector2 _textPosition;
        private MessageBoxViewportAlignment _alignment;

        public MessageBoxScreen(string message, Vector2 position)
        {
            _message = message;

            _textPosition = position;

            _alignment = MessageBoxViewportAlignment.Manual;

            IsPopup = true;
            HasCursor = true;

            TransitionOnTime = TimeSpan.FromSeconds(0.4);
            TransitionOffTime = TimeSpan.FromSeconds(0.4);
        }

        public MessageBoxScreen(string message, MessageBoxViewportAlignment alignment)
        {
            _message = message;

            _alignment = alignment;

            IsPopup = true;
            HasCursor = true;

            TransitionOnTime = TimeSpan.FromSeconds(0.4);
            TransitionOffTime = TimeSpan.FromSeconds(0.4);
        }

        /// <summary>
        /// Loads graphics content for this screen. This uses the shared ContentManager
        /// provided by the Game class, so the content will remain loaded forever.
        /// Whenever a subsequent MessageBoxScreen tries to load this same content,
        /// it will just get back another reference to the already loaded data.
        /// </summary>
        public override void LoadContent()
        {
            SpriteFont font = ScreenManager.Fonts.DetailsFont;
            ContentManager content = ScreenManager.Game.Content;
            _gradientTexture = content.Load<Texture2D>("Common/popup");

            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            Vector2 viewportSize = new Vector2(viewport.Width, viewport.Height);
            Vector2 textSize = font.MeasureString(_message);
            
            // The background includes a border somewhat larger than the text itself.
            const int hPad = 32;
            const int vPad = 16;

            switch (_alignment)
            {
                case MessageBoxViewportAlignment.BottomLeft:
                    _textPosition.X = hPad * 2; //Left X
                    _textPosition.Y = (viewportSize.Y - textSize.Y) - vPad * 2; //Bottom Y
                    break;
                case MessageBoxViewportAlignment.BottomRight:
                    _textPosition.X = (viewportSize.X - textSize.X) - hPad * 2; //Right X
                    _textPosition.Y = (viewportSize.Y - textSize.Y) - vPad * 2; //Bottom Y
                    break;
                case MessageBoxViewportAlignment.Center:
                    _textPosition = (viewportSize - textSize) / 2; //Center X and Y
                    break;
                case MessageBoxViewportAlignment.CenterBottom:
                    _textPosition.X = (viewportSize.X - textSize.X) / 2; //Center X
                    _textPosition.Y = (viewportSize.Y - textSize.Y) - vPad * 2; //Bottom Y
                    break;
                case MessageBoxViewportAlignment.CenterLeft:
                    _textPosition.X = hPad * 2; //Left X
                    _textPosition.Y = (viewportSize.Y - textSize.Y) / 2; //Center Y
                    break;
                case MessageBoxViewportAlignment.CenterRight:
                    _textPosition.X = (viewportSize.X - textSize.X) - hPad * 2; //Right X
                    _textPosition.Y = (viewportSize.Y - textSize.Y) / 2; //Center Y
                    break;
                case MessageBoxViewportAlignment.CenterTop:
                    _textPosition.X = (viewportSize.X - textSize.X) / 2; //Center X
                    _textPosition.Y = vPad * 2; //Top Y
                    break;
                case MessageBoxViewportAlignment.TopLeft:
                    _textPosition.X = hPad * 2; //Left X
                    _textPosition.Y = vPad * 2; //Top Y
                    break;
                case MessageBoxViewportAlignment.TopRight:
                    _textPosition.X = (viewportSize.X - textSize.X) - hPad * 2; //Right X
                    _textPosition.Y = vPad * 2; //Top Y
                    break;
            }

            _backgroundRectangle = new Rectangle((int)_textPosition.X - hPad,
                                                 (int)_textPosition.Y - vPad,
                                                 (int)textSize.X + hPad * 2,
                                                 (int)textSize.Y + vPad * 2);
        }

        /// <summary>
        /// Responds to user input, accepting or cancelling the message box.
        /// </summary>
        public override void HandleInput(InputHelper input, GameTime gameTime)
        {
            if (input.IsMenuSelect() || input.IsMenuCancel() ||
                input.IsNewMouseButtonPress(MouseButtons.LeftButton))
            {
                ExitScreen();
            }
        }

        /// <summary>
        /// Draws the message box.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            SpriteFont font = ScreenManager.Fonts.DetailsFont;

            // Fade the popup alpha during transitions.
            Color color = Color.White * TransitionAlpha * (2f / 3f);

            spriteBatch.Begin();

            // Draw the background rectangle.
            spriteBatch.Draw(_gradientTexture, _backgroundRectangle, color);

            // Draw the message box text.
            spriteBatch.DrawString(font, _message, _textPosition + Vector2.One, Color.Black);
            spriteBatch.DrawString(font, _message, _textPosition, Color.White);

            spriteBatch.End();
        }

        public Rectangle Bounds
        {
            get { return _backgroundRectangle; }
        }
    }
}