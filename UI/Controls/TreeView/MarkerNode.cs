using System;
using System.Collections.Generic;
using System.Linq;
using BhModule.Community.Pathing.Behavior;
using BhModule.Community.Pathing.Behavior.Filter;
using BhModule.Community.Pathing.Content;
using BhModule.Community.Pathing.Entity;
using BhModule.Community.Pathing.Scripting.Lib;
using BhModule.Community.Pathing.State;
using BhModule.Community.Pathing.UI.Tooltips;
using BhModule.Community.Pathing.UI.Views;
using BhModule.Community.Pathing.Utility;
using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Controls.Effects;
using Blish_HUD.Entities;
using Blish_HUD.Input;
using Gw2Sharp.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Timers;
using TmfLib.Pathable;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.UI.Controls.TreeView
{
    public class MarkerNode : TreeNodeBase
    {
        private Image _iconControl;
        private Label _categoryCountLabel;
        private Label _labelControl;

        private Color _textColor = Color.White;

        public Color TextColor
        {
            get => _textColor;
            set {
                if (SetProperty(ref _textColor, value) || _labelControl == null) return;
               
                _labelControl.TextColor = value;

                if (_iconControl != null)
                    _iconControl.Tint = value;
                
            }
        }

        private readonly string         _text;
        private readonly AsyncTexture2D _icon;

        private readonly IPackState _packState;
        private readonly PathingCategory _pathingCategory;
        private bool _forceShowAll;

        public bool ForceShowAll {
            get => _forceShowAll;
            set {
                if (!SetProperty(ref _forceShowAll, value)) return;

                if(Visible) //sub nodes are also built when node is shown
                    BuildSubNodes();

                foreach (var childNode in ChildBaseNodes.OfType<MarkerNode>()) 
                    childNode.ForceShowAll = value;
            }
        }

        private bool _selectable;
        public bool Selectable {
            get => _selectable;
            set {
                if (!SetProperty(ref _selectable, value)) return;

                if(value)
                    this._checkbox?.Show();
                else {
                    this._checkbox?.Hide();
                }
            }
        } 

        private bool _selected;

        public bool Selected
        {
            get => _selected;
            private set {
                SetProperty(ref _selected, value);
            }
        }

        private Checkbox _checkbox { get; set; }

        private IPathingEntity _entity;

        private IList<IPathingEntity> _entities;

        public MarkerNode(IPackState packState, PathingCategory pathingCategory, bool forceShowAll, string text = null)
        {
            _packState       = packState;
            _pathingCategory = pathingCategory;
            _forceShowAll    = forceShowAll;

            this._text = text ?? pathingCategory.DisplayName;
            this.Name  = this._text;

            if (pathingCategory.IsSeparator) {
                this.Selectable   = false;
                this.TextColor    = Color.LightYellow;
                BackgroundOpacity = 0.3f;

            } else {
                this.Selectable            = true;

                if (GetEntity() == null) {
                    BackgroundOpacity = 0.3f;
                } else {
                    BackgroundOpacity     = 0.05f;
                    BackgroundOpaqueColor = Color.LightYellow;
                }

          
            }

            this.EffectBehind = new ScrollingHighlightEffect(this);

            if (this.Selectable)
                this.Selected = !_packState.CategoryStates.GetCategoryInactive(_pathingCategory);

            //TODO: Get icon?
            _icon = GetIconFile();
            //_icon = pathingCategory.;

            this.ShowBackground = true;
            this.PanelHeight    = 40;
        }

        public virtual void Build() {
            _entity   = GetEntity();
            _entities = CategoryUtil.GetAssociatedPathingEntities(_pathingCategory, _packState.Entities).ToList();
            //if(_pathingCategory != null)
            //    this.Tooltip = new Tooltip(new EntityDetailsTooltip(_pathingCategory));

            if (_entity != null) {
                 var details = _packState.MapStates.GetMapDetails(_entity.MapId);

                if (_entity.BehaviorFiltered)
                {
                    var shiz = 0;
                }

                if (_entity.IsFiltered(EntityRenderTarget.World))
                {
                    var shiz = 0;
                }
            }
            
            
            if (_entity is StandardMarker standardMarker) {
                //var filter = StandardBehaviorFilter.BuildFromAttributes(_pathingCategory.GetAggregatedAttributes(), standardMarker, _packState) as StandardBehaviorFilter;



                var filterReasons = standardMarker.Behaviors
                                                  .Where(b => b is ICanFilter filter && filter.IsFiltered())
                                                  .Select(b => (b as ICanFilter)?.FilterReason());

                //foreach (var behavior in standardMarker.Behaviors)
                //{
                //    if (behavior is ICanFilter filter)
                //    {
                //        if(filter.IsFiltered())
                //            filterReasons.Add(filter.FilterReason());
                //        filtered |= filter.FilterReason();
                //    }
                //}

                if (filterReasons != null && filterReasons.Any())
                    this.BasicTooltipText = string.Join(", ", filterReasons);

            }

            BuildDetailsPanel();
            BuildCheckbox();
            
            BuildIcon();

            BuildLabel();

            BuildCategoryNumber();

            BuildAchievemenTexture();
            BuildFestivalsLabel();
            
            BuildMapId();
            //BuildEntityCount();

            //BuildPropertiesPanel();
            //BuildDistance();
        }


        private FlowPanel _detailsPanel;
        private void BuildDetailsPanel()
        {
            if (_detailsPanel != null) throw new InvalidOperationException("Requirements panel already exists.");

            _detailsPanel = new FlowPanel()
            {
                Parent         = this,
                FlowDirection  = ControlFlowDirection.LeftToRight,
                Size           = new Point(this.ContentRegion.Width - 100, this.PanelHeight),
                Location       = new Point(28,                       1),
                ControlPadding = new Vector2(5, 0),
                CanScroll      = false,
                ShowTint       = DevMode
            };
        }

        private FlowPanel _propertiesPanel;
        private void BuildPropertiesPanel()
        {
            if (_propertiesPanel != null) throw new InvalidOperationException("Requirements panel already exists.");

            _propertiesPanel = new FlowPanel()
            {
                Parent         = this,
                FlowDirection  = ControlFlowDirection.RightToLeft,
                Size           = new Point(100, this.PanelHeight),
                Location       = new Point(Width - 135,                       1),
                ControlPadding = new Vector2(5, 0),
                CanScroll      = false,
                ShowTint       = DevMode
            };
        }

        private void BuildDistance() {

            var distance = GetEntity()?.DistanceToPlayer;

            if(distance == null) return;

            _ = new Label
            {
                Parent        = _propertiesPanel,
                Text          = distance.ToString(),
                Height        = this.PanelHeight,
                AutoSizeWidth = true,
                Font          = GameService.Content.DefaultFont16,
                TextColor     = this.TextColor,
                StrokeText    = true
            };

        }

        private void BuildIcon() {
            if (_icon == null) return;

            var tooltip = new Tooltip(new EntityTextureTooltip(_entity));

            var iconContainer = new Panel() {
                Parent = _detailsPanel,
                Size   = new Point(35, Height),
                Tooltip = tooltip
            };

            _iconControl = new Image(_icon)
            {
                Parent  = iconContainer,
                Top     = 4,
                Size    = new Point(30, 30),
                Tooltip = tooltip
            };
        }

        private void BuildCategoryNumber()
        {
            if (_categoryCountLabel != null) {
                _categoryCountLabel.Text    = $"({ChildBaseNodes.Count} markers)";
                _categoryCountLabel.Visible = ChildBaseNodes.Count != 0;
                return;
            }

            _categoryCountLabel = new Label
            {
                Parent        = _detailsPanel,
                Text          = $"({ChildBaseNodes.Count} markers)",
                Height        = this.PanelHeight,
                AutoSizeWidth = true,
                Font          = GameService.Content.DefaultFont16,
                TextColor     = Color.LightBlue,
                StrokeText    = true,
                Visible       = true
            };
        }

        private void BuildLabel()
        {
            _labelControl?.Dispose();

            _labelControl = new Label
            {
                Parent        = _detailsPanel,
                Text          = this._text,
                Height        = this.PanelHeight,
                AutoSizeWidth = true,
                Font          = GameService.Content.DefaultFont16,
                TextColor     = this.TextColor,
                StrokeText    = true,
                BasicTooltipText = this.BasicTooltipText
            };
        }


        public void BuildCheckbox()
        {
            if (!this.Selectable) return;

            var checkboxContainer = new Panel
            {
                Parent = _detailsPanel,
                Size   = new Point(this.PanelHeight / 2 + 5, this.PanelHeight),
            };

            this._checkbox = new Checkbox
            {
                Parent  = checkboxContainer,
                Left    = 5,
                Size    = new Point(this.PanelHeight, this.PanelHeight),
                Checked = this.Selected
            };

            this._checkbox.CheckedChanged += CheckboxOnCheckedChanged;
        }

        private void BuildAchievemenTexture() {
            var id = GetAchievementId();

            if (id == null) return;

            _ = new Image(AsyncTexture2D.FromAssetId(155062))
            {
                Parent = _detailsPanel,
                Size   = new Point(this.Height, this.Height),
            };
        }

        private void BuildMapId() {
            _pathingCategory.TryGetAggregatedAttributeValue("mapId", out var mapId);

            if (_entity is StandardMarker standardMarker) {
                var mappId = standardMarker.PointOfInterest.MapId;

                //var map = _packState.MapStates.FindMapDetails(standardMarker.Position.X, standardMarker.Position.Y);

                if (mappId == null) return;

                _ = new Label
                {
                    Parent        = _detailsPanel,
                    Text          = mappId.ToString(),
                    Height        = this.PanelHeight,
                    AutoSizeWidth = true,
                    Font          = GameService.Content.DefaultFont16,
                    TextColor     = Color.LightBlue,
                    StrokeText    = true
                };
            }

           
        }

        private void BuildEntityCount() {
            
            var textures = new List<AsyncTexture2D>();

            if (_entities == null || !_entities.Any()) return;

            foreach (var entity in _entities) {
                if(entity is StandardMarker marker)
                    textures.Add(marker.Texture);

                if(entity is StandardTrail trail)
                    textures.Add(trail.Texture);
            }

            var count = textures.Select(t => t.Texture.Name).Distinct().Count();

            if (count == null || count == 0) return;

            _ = new Label
            {
                Parent        = _detailsPanel,
                Text          = $"({count} textures)",
                Height        = this.PanelHeight,
                AutoSizeWidth = true,
                Font          = GameService.Content.DefaultFont16,
                TextColor     = Color.LightBlue,
                StrokeText    = true
            };
        }


        private void BuildFestivalsLabel()
        {
            var festivals    = GetFestivals();

            if (string.IsNullOrWhiteSpace(festivals)) return;

            _ = new Label
            {
                Parent        = _detailsPanel,
                Text          = festivals,
                Height        = this.PanelHeight,
                AutoSizeWidth = true,
                Font          = GameService.Content.DefaultFont16,
                TextColor     = Color.LightGreen,
                StrokeText    = true
            };
        }

        public void BuildSubNodes() {
            this.ClearChildNodes();

            var skipped = AddSubNodes(_forceShowAll);

            if (skipped > 0 && _packState.UserConfiguration.PackShowWhenCategoriesAreFiltered.Value)
            {
                var showAllSkippedCategories = new LabelNode($"{skipped} hidden (click to show)", AsyncTexture2D.FromAssetId(358463))
                {
                    Width = this.Parent.Width - 14,
                    // LOCALIZE: Skipped categories menu item
                    //Enabled          = false,
                    TextColor = Color.LightYellow,
                    //CanCheck         = true, //TODO Make clickable
                    BasicTooltipText = string.Format(Strings.Info_HiddenCategories, _packState.UserConfiguration.PackEnableSmartCategoryFilter.DisplayName),
                    Parent           = this
                };

                showAllSkippedCategories.Build();

                //TODO Make clickable
                // The control is disabled, so the .Click event won't fire.  We cheat by just doing LeftMouseButtonReleased.
                showAllSkippedCategories.LeftMouseButtonReleased += ShowAllSkippedCategories_LeftMouseButtonReleased;
            }

            if (skipped == 0 && !this.ChildBaseNodes.Any())
            {
                //Selectable = false;

                //this.AddChild(new LabelNode("No marker packs loaded...")
                //{
                //    Enabled = false,
                //});
            }

        }

        private void CheckboxOnCheckedChanged(object sender, CheckChangedEvent e) {
            if (this.Enabled && !_pathingCategory.IsSeparator)
            {
                _packState.CategoryStates.SetInactive(_pathingCategory, !e.Checked);
            }
        }

        protected override void OnShown(EventArgs e)
        {
            if (this.ChildBaseNodes.Any()) {
                ShowChildren();
                
                return; //TODO: Any reason to not return here?
            }

            BuildSubNodes();

            base.OnShown(e);
        }

        private int AddSubNodes(bool forceAll) {

            (IEnumerable<PathingCategory> subCategories, int skipped) = GetSubCategories(forceAll);

            foreach (var subCategory in subCategories)
            {
                var subNode = new MarkerNode(_packState, subCategory, _forceShowAll)
                {
                    Width  = this.Parent.Width - 14,
                    Parent = this
                };

                subNode.Build();
            }

            BuildCategoryNumber();

            return skipped;
        }

        private void ShowAllSkippedCategories_LeftMouseButtonReleased(object sender, MouseEventArgs e)
        {
            this.ClearChildNodes();

            AddSubNodes(true);
        }

        protected override void OnHidden(EventArgs e)
        {
            HideChildren();
            
            base.OnHidden(e);
        }

        private (IEnumerable<PathingCategory> SubCategories, int Skipped) GetSubCategories(bool forceShowAll = false)
        {
            // We only show subcategories with a non-empty DisplayName (explicitly setting it to "" will hide it) and
            // was loaded by one of the packs (since those still around from unloaded packs will remain).
            var subCategories = _pathingCategory.Where(cat => cat.LoadedFromPack && cat.DisplayName != "" && !cat.IsHidden);

            if (!_packState.UserConfiguration.PackEnableSmartCategoryFilter.Value || forceShowAll)
            {
                return (subCategories, 0);
            }

            var filteredSubCategories = new List<PathingCategory>();

            PathingCategory lastCategory = null;

            bool lastIsSeparator = false;

            int skipped = 0;

            // We go bottom to top to check if the categories are potentially relevant to categories below.
            foreach (var subCategory in subCategories.Reverse())
            {
                if (subCategory.IsSeparator && ((!lastCategory?.IsSeparator ?? false) || lastIsSeparator))
                {
                    // If separator was relevant to this category, we include it.
                    filteredSubCategories.Add(subCategory);
                    lastIsSeparator = true;
                }
                else if (CategoryUtil.UiCategoryIsNotFiltered(subCategory, _packState))
                {
                    // If category was not filtered, we include it.
                    filteredSubCategories.Add(subCategory);
                    lastIsSeparator = false;
                }
                else
                {
                    lastIsSeparator = false;
                    if (!subCategory.IsSeparator) skipped++;
                    continue;
                }

                lastCategory = subCategory;
            }

            return (Enumerable.Reverse(filteredSubCategories), skipped);
        }

        private IPathingEntity GetEntity() {
            return _packState.Entities.FirstOrDefault(e => e.Category == _pathingCategory);
        }

        private AsyncTexture2D GetIconFile() {
            var entity = GetEntity();

            //TODO: Texture still empty for hidden categories because they are not unpacked?
            if (_pathingCategory.ExplicitAttributes.Contains("texture")) {
                var iconFile = _pathingCategory.ExplicitAttributes["texture"].Value;

                if (!string.IsNullOrWhiteSpace(iconFile)) {
                    var textureManager = TextureResourceManager.GetTextureResourceManager(_pathingCategory.ResourceManager);
                    return textureManager.LoadTextureAsync(iconFile).Result.Texture;
                }
            }
         
            if (entity is StandardMarker marker) {
                return marker.Texture;
            }

            if (entity is StandardTrail trail) {
                return trail.Texture;
            }

            return null;
        }

        private string GetFestivals() {
            if (_pathingCategory.TryGetAggregatedAttributeValue(FestivalFilter.PRIMARY_ATTR_NAME, out var festivalAttr)) {
                return festivalAttr;
            }

            return null;
        }

        private int? GetAchievementId() {
            if (_pathingCategory.TryGetAggregatedAttributeValue(AchievementFilter.ATTR_ID, out var achievementAttr)) {

                var achievementBit = -1;

                if (_pathingCategory.TryGetAggregatedAttributeValue(AchievementFilter.ATTR_BIT, out var achievementBitAttr)) {
                    if (InvariantUtil.TryParseInt(achievementBitAttr, out int achievementBitParsed)) {
                        achievementBit = achievementBitParsed;
                    }
                }

                // TODO: Add as a context so that multiple characteristics can be accounted for.

                if (!InvariantUtil.TryParseInt(achievementAttr, out int achievementId)) 
                    return null;

                return achievementId;
            }

            return null;
        }

        private void DetectAndBuildContexts()
        {
            if (_pathingCategory.TryGetAggregatedAttributeValue(AchievementFilter.ATTR_ID, out var achievementAttr))
            {

                var achievementBit = -1;
                if (_pathingCategory.TryGetAggregatedAttributeValue(AchievementFilter.ATTR_BIT, out var achievementBitAttr))
                {
                    if (InvariantUtil.TryParseInt(achievementBitAttr, out int achievementBitParsed))
                    {
                        achievementBit = achievementBitParsed;
                    }
                }

                // TODO: Add as a context so that multiple characteristics can be accounted for.

                if (!InvariantUtil.TryParseInt(achievementAttr, out int achievementId)) return;

                if (achievementId < 0) return;

                if (_packState.UserConfiguration.PackShowTooltipsOnAchievements.Value)
                {
                    this.Tooltip = new Tooltip(new AchievementTooltipView(achievementId, achievementBit));
                }

                if (_packState.UserConfiguration.PackAllowMarkersToAutomaticallyHide.Value)
                {
                    this.Enabled = !_packState.AchievementStates.IsAchievementHidden(achievementId, achievementBit);

                    if (!this.Enabled && this._checkbox != null)
                    {
                        this._checkbox.Checked = false;
                    }
                }
            }
            else if (_pathingCategory.ExplicitAttributes.TryGetAttribute("tip-description", out var descriptionAttr))
            {
                this.Tooltip = new Tooltip(new DescriptionTooltipView(null, descriptionAttr.Value));
            }
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            if(_entity?.DistanceToPlayer != null)
                spriteBatch.DrawStringOnCtrl(this, 
                                       Math.Round(_entity.DistanceToPlayer, 0).ToString(),
                                       GameService.Content.DefaultFont16,
                                       new Rectangle(Width - 100, 5, 100, 30),
                                       Color.White);

            base.PaintAfterChildren(spriteBatch, bounds);
        }

        protected override void OnClick(MouseEventArgs e)
        {
            //Mouse is over checkbox
            if (this._checkbox != null && this._checkbox.AbsoluteBounds.Contains(Control.Input.Mouse.Position))
                return;

            base.OnClick(e);
        }

        protected override void DisposeControl()
        {
            _iconControl?.Dispose();
            _labelControl?.Dispose();

            base.DisposeControl();
        }
    }
}
