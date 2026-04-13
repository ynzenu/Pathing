using System.Collections.Generic;
using System.Linq;
using BhModule.Community.Pathing.State;
using BhModule.Community.Pathing.UI.Controls.TreeNodes;
using BhModule.Community.Pathing.Utility;
using Blish_HUD;
using TmfLib.Pathable;

namespace BhModule.Community.Pathing.UI.Controls.TreeView
{
    public class DistanceTreeView : TreeViewBase
    {
        public DistanceTreeView(PackInitiator packInitiator) : base(packInitiator) {
        }

        public void SetNearMeResults(IPackState packState, int maxResults, bool activeOnly = false) {
            InvokeNodeLoadingStarted();

            ClearChildNodes();
            AllBaseNodes.Clear();

            this.EntityLookup = packState.Entities.ToArray().ToLookup(e => e.Category);

            if (PackInitiator == null) {
                InvokeNodesLoadedFinished();
                return;
            }

            var rootCategory = PackInitiator.GetAllMarkersCategories();
            if (rootCategory == null) {
                InvokeNodesLoadedFinished();
                return;
            }

            var mapId = GameService.Gw2Mumble.CurrentMap.Id;

            // 1. Compute min distance per category using EntityLookup (O(1) per category)
            var categoryDistances = new Dictionary<PathingCategory, float>();
            var allCategories = CategoryUtil.FlattenCategories(rootCategory);

            foreach (var c in allCategories) {
                if (c.IsSeparator || c.IsHidden || !c.LoadedFromPack) continue;

                // Filter to active categories only
                if (activeOnly && packState.CategoryStates.GetNamespaceInactive(c.Namespace)) continue;

                float minDist = float.MaxValue;
                foreach (var e in EntityLookup[c]) {
                    if (e.MapId == mapId && e.DistanceToPlayer > 0 && e.DistanceToPlayer < minDist) {
                        minDist = e.DistanceToPlayer;
                    }
                }

                if (minDist < float.MaxValue) {
                    categoryDistances[c] = minDist;
                }
            }

            // 2. Take the top N nearest categories, sorted by distance
            var nearMeLeaves = categoryDistances
                              .OrderBy(x => x.Value)
                              .Take(maxResults)
                              .ToList();

            if (nearMeLeaves.Count == 0) {
                InvokeNodesLoadedFinished();
                return;
            }

            // 3. Build a flat list (like search results)
            var distanceLookup = nearMeLeaves.ToDictionary(x => x.Key, x => x.Value);

            foreach (var kvp in nearMeLeaves) {
                var node = new PathingCategoryNode(packState, kvp.Key, false) {
                    IsSearchResult = true,
                    ShowDistance    = true,
                    DistanceLookup = distanceLookup
                };

                node.Width  = this.Width - 30;
                node.Parent = this;

                node.Active = !packState.CategoryStates.GetNamespaceInactive(kvp.Key.Namespace);
            }

            InvokeNodesLoadedFinished();
        }
    }
}
