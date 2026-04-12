using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace BhModule.Community.Pathing.UI.Controls
{
    public class TabbedRegion : Container
    {
        private readonly AsyncTexture2D _inactiveHeader = AsyncTexture2D.FromAssetId(2200566);
        private readonly AsyncTexture2D _activeHeader = AsyncTexture2D.FromAssetId(2200567);
        private readonly AsyncTexture2D _separator = AsyncTexture2D.FromAssetId(156055);
        private readonly Panel _contentPanel;
        private TabbedRegionTab _activeTab;
        private Rectangle _contentRegion;
        private Rectangle _headerRegion;
        private Thickness _headerPadding = new(8, 0);
        private Thickness _contentPadding = new(4, 0);

        public TabbedRegion()
        {
            Tabs.CollectionChanged += Tab_CollectionChanged;
            _contentPanel = new()
            {
                Parent = this,
            };
        }

        private void Tab_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (_activeTab == null)
            {
                SwitchTab(Tabs.FirstOrDefault());
            }

            RecalculateLayout();
        }

        public TabbedRegionTab ActiveTab
        {
            get => _activeTab;
            set => SwitchTab(value);
        }

        public ObservableCollection<TabbedRegionTab> Tabs { get; } = [];

        public BitmapFont HeaderFont
        {
            get; set
            {
                field = value ?? Content.DefaultFont18;
                foreach (var tab in Tabs)
                {
                    tab.Font = field;
                }
            }
        } = Content.DefaultFont18;

        public Thickness ContentPadding { get => _contentPadding; set => _contentPadding = value; }

        public Action OnTabSwitched { get; set; }

        public void AddTab(TabbedRegionTab tab)
        {
            tab.Container.Visible = false;
            tab.Container.Parent = _contentPanel;
            tab.Container.Size = _contentPanel.Size;

            Tabs.Add(tab);
        }

        public void AddTab(Container tab)
        {
            Tabs.Add(new(tab));
        }

        public void RemoveTab(Container tab)
        {
            _ = Tabs.ToList().RemoveAll(e => e.Container == tab);
        }

        public void SwitchTab(TabbedRegionTab tab)
        {
            if (tab is not null)
            {
                tab.Container.Visible = true;
                _activeTab = tab;
                _activeTab.Container?.Invalidate();
            }

            foreach (var item in Tabs)
            {
                item.IsActive = item == _activeTab;
                item.Container.Visible = item.IsActive;
            }

            OnTabSwitched?.Invoke();
            RecalculateLayout();
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            _headerRegion = new Rectangle(0, 0, Width, HeaderFont.LineHeight + 5 + (int)_headerPadding.Top);
            _contentRegion = new Rectangle((int)_contentPadding.Left, _headerRegion.Bottom + (int)_contentPadding.Top, Width - (int)_contentPadding.Left - (int)_contentPadding.Right, Height - _headerRegion.Bottom - (int)_contentPadding.Top - (int)_contentPadding.Bottom);

            if (_contentPanel is not null)
            {
                _contentPanel.Location = _contentRegion.Location;
                _contentPanel.Size = _contentRegion.Size;

                foreach (var tab in Tabs)
                {
                    if (tab.Container != null)
                    {
                        tab.Container.Size     = _contentPanel.Size;
                        tab.Container.Location = Point.Zero;
                    }
                }
            }

            int currentX = _headerRegion.Left;
            for (int i = 0; i < Tabs.Count; i++)
            {
                int iconSpace = Tabs[i].Icon != null ? (_headerRegion.Height - 5 + 10) : 0;
                int tabWidth = iconSpace + (int)HeaderFont.MeasureString(Tabs[i].Title).Width + 20;

                Tabs[i].Bounds = new(currentX, _headerRegion.Top, tabWidth, _headerRegion.Height);
                currentX += tabWidth;
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);
            for (int i = 0; i < Tabs.Count; i++)
            {
                Tabs[i].DrawHeader(this, spriteBatch, RelativeMousePosition);
            }
            spriteBatch.DrawOnCtrl(this, _separator, new Rectangle(_headerRegion.Left, _headerRegion.Bottom - 9, _headerRegion.Width, 16), Color.Black);
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            for (int i = 0; i < Tabs.Count; i++)
            {
                if (Tabs[i].IsHovered(RelativeMousePosition))
                {
                    SwitchTab(Tabs[i]);
                    break;
                }
            }
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            Tabs.CollectionChanged -= Tab_CollectionChanged;
        }
    }
}
