using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BhModule.Community.Pathing.UI.Controls.TreeNodes;
using BhModule.Community.Pathing.Utility;
using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using TmfLib.Pathable;

namespace BhModule.Community.Pathing.UI.Controls.TreeView
{
    public class CategoryExplorerTreeView : TreeViewBase
    {
        private PathingCategoryNode _rootNode;
        private List<CategorySearchRecord> _searchRecords = new List<CategorySearchRecord>();

        private class CategorySearchRecord {
            public PathingCategory Category              { get; }
            public string          NormalizedDisplayName { get; }
            public string          NormalizedName        { get; }

            public CategorySearchRecord(PathingCategory category) {
                this.Category              = category;
                this.NormalizedDisplayName = category.DisplayName?.Replace(" ", "") ?? "";
                this.NormalizedName        = category.Name?.Replace(" ", "") ?? "";
            }
        }

        public CategoryExplorerTreeView(PackInitiator packInitiator) : base(packInitiator) {
        }

        public void LoadNodes() {
            InvokeNodeLoadingStarted();

            ClearChildNodes();
            AllBaseNodes.Clear();

            RefreshEntityLookup();

            var rootCategory = PackInitiator.GetAllMarkersCategories();

            if (rootCategory == null) return;

            _rootNode?.Dispose();

            if (rootCategory.Count(c => c.LoadedFromPack) <= 0) return; //No packs installed

            _rootNode = new PathingCategoryNode(PackInitiator.PackState, rootCategory, false)
            {
                Name   = "All Markers",
                Width  = this.Width - 30,
                Parent = this
            };

            _rootNode.Checked = PackInitiator.PackState.UserConfiguration.GlobalPathablesEnabled.Value;

            _rootNode.CheckedChanged += (_, e) => {
                if(PackInitiator?.PackState != null)
                    PackInitiator.PackState.UserConfiguration.GlobalPathablesEnabled.Value = e.Checked;
            };

            _rootNode.Expand();

            _searchRecords = CategoryUtil.FlattenCategories(rootCategory).Select(c => new CategorySearchRecord(c)).ToList();

            InvokeNodesLoadedFinished();
        }

        protected override void GlobalPathablesEnabledOnSettingChanged(object sender, ValueChangedEventArgs<bool> e) {
            if (_rootNode == null || PackInitiator?.PackState?.UserConfiguration == null) return;

            _rootNode.Checked = PackInitiator.PackState.UserConfiguration.GlobalPathablesEnabled.Value;
        }

        public async Task<(List<PathingCategory> categories, int skipped)> SearchAsync(string input, CancellationToken cancellationToken = default, bool forceShowAll = false)
        {
            if (string.IsNullOrWhiteSpace(input)) {
                await Task.CompletedTask;
                return (new List<PathingCategory>(), 0);
            }

            IEnumerable<PathingCategory> filteredResults = null;

            var skipped = 0;

            cancellationToken.ThrowIfCancellationRequested();

            string normalizedInput = input.Replace(" ", "");

            var results = _searchRecords
                         .Where(c =>
            {
                if (string.IsNullOrWhiteSpace(c.Category.DisplayName) || string.IsNullOrWhiteSpace(c.Category.Name)) return false;

                return (c.NormalizedDisplayName.IndexOf(normalizedInput, StringComparison.OrdinalIgnoreCase) >= 0) ||
                       c.NormalizedName.IndexOf(normalizedInput, StringComparison.OrdinalIgnoreCase) >= 0;
            }).Select(c => c.Category).ToList();

            (filteredResults, skipped) = results.FilterCategories(PackInitiator.PackState, forceShowAll, this.EntityLookup);

            return (filteredResults.ToList(), skipped);
        }

        public void RemoveNodeHighlights() {
            foreach (var node in AllBaseNodes) {
                if (node.Highlighted) node.Highlighted = false;
            }
        }

        public void NavigateToPath(string path) {
            if(_rootNode == null) return;

            PathingCategoryNode currentNode = _rootNode;

            RemoveNodeHighlights();
            var splitPath = path.Split('.');

            var pathItemsWithIndex = splitPath
               .Select((pathItem, index) => new { pathItem, index, isLast = index == splitPath.Length - 1 });

            foreach (var item in pathItemsWithIndex) {
                if (string.IsNullOrWhiteSpace(item.pathItem)) continue;

                var categoryResult = currentNode
                                     .PathingCategory?
                                     .FirstOrDefault(n => !string.IsNullOrWhiteSpace(n?.Name) && n.Name.Equals(item.pathItem));

                if (categoryResult == null || !categoryResult.LoadedFromPack) return;

                var baseNodes = currentNode.ChildBaseNodes ?? _rootNode?.ChildBaseNodes;

                if (baseNodes == null) 
                    return;

                var tempCurrentNode = baseNodes.OfType<PathingCategoryNode>().FirstOrDefault(n => n.PathingCategory == categoryResult);

                tempCurrentNode ??= new PathingCategoryNode(PackInitiator.PackState, categoryResult, false)
                {
                    Width  = currentNode.Width - 30,
                    Parent = currentNode
                };
                
                currentNode = tempCurrentNode;

                if (!item.isLast) {
                    currentNode.Expand();
                }
            }

            if (currentNode == null) return;

            currentNode.Highlighted = true;
            SetScrollToChild(currentNode);
        }

        public void UpdateCheckedState(PathingCategory category, bool active) {
            var node = AllBaseNodes
                      .OfType<PathingCategoryNode>()
                      .FirstOrDefault(n => n.PathingCategory == category);

            if (node != null) {
                node.Checked = active;
            }
        }
    }
}
