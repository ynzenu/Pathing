using BhModule.Community.Pathing.Behavior.Filter;
using BhModule.Community.Pathing.Entity;
using Blish_HUD;
using Blish_HUD.Common.UI.Views;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using TmfLib.Pathable;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.UI.Views {
    internal class EntityDetailsTooltip : View, ITooltipView {

        private readonly PathingCategory _category;

        private string _achievementId;
        private string _mapType;
        private string _mount;
        private string _profession;

        public EntityDetailsTooltip(PathingCategory category) {
            _category = category;

            if (_category.TryGetAggregatedAttributeValue(AchievementFilter.ATTR_ID, out var achievementAttr))
            {
                _achievementId = achievementAttr;
            };

            _category.TryGetAggregatedAttributeValue(MapTypeFilter.PRIMARY_ATTR_NAME, out _mapType);
            _category.TryGetAggregatedAttributeValue(MountFilter.PRIMARY_ATTR_NAME, out _mount);
            _category.TryGetAggregatedAttributeValue(ProfessionFilter.PRIMARY_ATTR_NAME, out _profession);
        }

        protected override void Build(Container buildPanel) {
            var panel = new FlowPanel() {
                FlowDirection = ControlFlowDirection.TopToBottom,
                Location      = new Point(0, 0),
                Size = new Point(300, 200),
                Parent        = buildPanel
            };

            if (!string.IsNullOrWhiteSpace(_achievementId)) {
                _ = new Label()
                {
                    Text          = $"Achievement ID: {_achievementId}",
                    Font          = GameService.Content.DefaultFont16,
                    Height = 25,
                    AutoSizeWidth = true,
                    Parent        = panel
                };
            }
            
            if(!string.IsNullOrWhiteSpace(_mapType))
            {
                _ = new Label()
                {
                    Height        = 25,
                    Text          = $"Map type: {_mapType}",
                    Font          = GameService.Content.DefaultFont16,
                    AutoSizeWidth = true,
                    Parent        = panel
                };
            }

            if (!string.IsNullOrWhiteSpace(_mount))
            {
                _ = new Label()
                {
                    Height        = 25,
                    Text          = $"Required mount: {_mount}",
                    Font          = GameService.Content.DefaultFont16,
                    AutoSizeWidth = true,
                    Parent        = panel
                };
            }

            if (!string.IsNullOrWhiteSpace(_profession))
            {
                _ = new Label()
                {
                    Height        = 25,
                    Text          = $"Required profession: {_profession}",
                    Font          = GameService.Content.DefaultFont16,
                    AutoSizeWidth = true,
                    Parent        = panel
                };
            }

        }

    }
}
