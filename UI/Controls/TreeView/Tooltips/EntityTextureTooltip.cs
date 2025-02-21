using BhModule.Community.Pathing.Entity;
using Blish_HUD.Common.UI.Views;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;

namespace BhModule.Community.Pathing.UI.Views {
    internal class EntityTextureTooltip : View, ITooltipView {

        private readonly IPathingEntity _entity;

        private AsyncTexture2D _texture;
        
        private Image _imgIcon;

        public EntityTextureTooltip(IPathingEntity entity) {
            _entity = entity;

            if (_entity is StandardMarker marker) {
                _texture = marker.Texture;
            }

            if(_entity is StandardTrail trail) {
                _texture = trail.Texture;
            }
        }

        protected override void Build(Container buildPanel) {
            if (_texture == null) return;
            
            _imgIcon = new Image(_texture)
            {
                Size = new Point(_texture.Width, _texture.Height),
                Location = new Point(0, 0),
                Parent = buildPanel
            };
        }

    }
}
