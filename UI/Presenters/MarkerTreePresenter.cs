using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BhModule.Community.Pathing.UI.Controls.TreeView;
using BhModule.Community.Pathing.UI.Views;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using TmfLib.Pathable;

namespace BhModule.Community.Pathing.UI.Presenter {

    public class MarkerTreePresenter : Presenter<MarkerTreeView, PackInitiator> {

        private readonly PathingModule _module;

        public MarkerTreePresenter(MarkerTreeView view, PathingModule module) : base(view, module.PackInitiator) {
            _module = module;
        }

        protected override Task<bool> Load(IProgress<string> progress) {
            _module.ModuleLoaded += (sender, args) => {
                UpdateView();
            };

            return Task.FromResult(true);
        }

        public IEnumerable<PathingCategory> FlattenCategories(PathingCategory category)
        {
            var categories = new List<PathingCategory> { category };

            foreach (var subCategory in category)
            {
                categories.AddRange(FlattenCategories(subCategory));
            }

            return categories;
        }

        public IEnumerable<PathingCategory> Search(string input) {
            if(input.Length < 3) return new List<PathingCategory>();

            var categories = FlattenCategories(_module.PackInitiator.GetAllMarkersCategories());

            string normalizedInput = input.ToLower().Replace(" ", "");

            return categories.Where(c => !string.IsNullOrWhiteSpace(c.DisplayName) && c.DisplayName.ToLower().Replace(" ", "").Contains(normalizedInput));
        }

        protected override void UpdateView() {
            if(this.View.TreeView == null) return;

            this.View.TreeView.ClearChildren();

            if (_module.PackInitiator == null) return;

            var root = _module.PackInitiator.GetAllMarkersCategories();

            var packs = _module.PackInitiator.GetSubCategories(root, true);

            foreach (var pack in packs.SubCategories) {
                var packNode = new MarkerNode(_module.PackInitiator.PackState, pack, false)
                {
                    Parent = this.View.TreeView,
                    Width  = this.View.TreeView.Width - 30
                };

                packNode.Build();
            }
        }

        public void ResetView() {
            UpdateView();
        }

        public void SetSearchResults(IList<PathingCategory> categories) {
            
            this.View.TreeView.ClearChildNodes();

            foreach (var category in categories) {
                var categoryNode = new MarkerNode(_module.PackInitiator.PackState, category, false)
                {
                    Parent                = this.View.TreeView,
                    Width                 = this.View.TreeView.Width - 14,
                    BackgroundOpaqueColor = Color.Yellow,
                    BackgroundOpacity     = 0.2f
                };

                categoryNode.Build();
            }
        }

    }
}
