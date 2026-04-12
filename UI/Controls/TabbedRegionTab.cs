using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Gw2Sharp.WebApi;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System;


namespace BhModule.Community.Pathing.UI.Controls
{
    public class TabbedRegionTab
    {
        private readonly DetailedTexture _inactiveHeader = new(2200567);
        private readonly DetailedTexture _activeHeader = new(2200566);

        private Rectangle _bounds;
        private Rectangle _iconBounds;
        private Rectangle _textBounds;
        private string _title;

        public TabbedRegionTab(Container container)
        {
            Container = container;

            //LocalizingService.LocaleChanged += UserLocale_SettingChanged;
            UserLocale_SettingChanged(this, null);
        }

        private void UserLocale_SettingChanged(object sender, ValueChangedEventArgs<Locale> e)
        {
            _title = Header?.Invoke();
        }

        public Container Container { get; set; }

        public bool IsActive { get; set; }

        public Func<string> Header { get; set { field = value; _title = value?.Invoke(); } }

        public string Title => _title ?? Header?.Invoke() ?? string.Empty;

        public Rectangle Bounds
        {
            get => _bounds;
            set
            {
                _bounds = value;
                _inactiveHeader.Bounds = value;
                _activeHeader.Bounds = value;
                RecalculateLayout();
            }
        }

        public bool IsHovered(Point p)
        {
            return Bounds.Contains(p);
        }

        public AsyncTexture2D Icon { get; set; }
        public BitmapFont Font { get; set; } = GameService.Content.DefaultFont18;

        public void DrawHeader(Control ctrl, SpriteBatch spriteBatch, Point mousePos)
        {
            Color color = IsActive ? Color.White : Color.White * (IsHovered(mousePos) ? 0.9F : 0.6F);
            (!IsActive ? _activeHeader : _inactiveHeader).Draw(ctrl, spriteBatch, mousePos, color);

            if (!string.IsNullOrEmpty(_title))
            {
                spriteBatch.DrawStringOnCtrl(ctrl, _title, Font, _textBounds, Color.White, false, true, 2, HorizontalAlignment.Center, VerticalAlignment.Middle);
            }

            if (Icon is not null)
            {
                spriteBatch.DrawOnCtrl(ctrl, Icon, _iconBounds, Color.White);
            }
        }

        private void RecalculateLayout()
        {
            if (Icon != null)
            {
                _iconBounds = new(Bounds.Left + 5, Bounds.Top + 5, Bounds.Height - 10, Bounds.Height - 10);
                _textBounds = new(_iconBounds.Right + 5, Bounds.Top, Bounds.Width - _iconBounds.Width - 15, Bounds.Height);
            }
            else
            {
                _iconBounds = Rectangle.Empty;
                _textBounds = new(Bounds.Left + 5, Bounds.Top, Bounds.Width - 10, Bounds.Height);
            }
        }
    }
}
