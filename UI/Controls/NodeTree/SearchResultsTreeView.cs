using System.Collections.Generic;
using BhModule.Community.Pathing.State;
using BhModule.Community.Pathing.UI.Controls.TreeNodes;
using Blish_HUD.Content;
using Microsoft.Xna.Framework;
using TmfLib.Pathable;

namespace BhModule.Community.Pathing.UI.Controls.TreeView
{
    public class SearchResultsTreeView : TreeViewBase
    {
        public SearchResultsTreeView(PackInitiator packInitiator) : base(packInitiator) {
        }

        public LabelNode SetSearchResults(IList<PathingCategory> categories, IPackState packState, int skipped = 0)
        {
            ClearChildNodes();

            LabelNode showAllSkippedCategories = null;

            if (skipped > 0 && packState.UserConfiguration.PackShowWhenCategoriesAreFiltered.Value) {
                showAllSkippedCategories = new LabelNode($"{skipped} hidden (click to show)", AsyncTexture2D.FromAssetId(358463))
                {
                    Clickable        = true,
                    Width            = this.Parent.Width - 14,
                    TextColor        = Color.LightYellow,
                    BasicTooltipText = string.Format(Strings.Info_HiddenCategories, PackInitiator.PackState.UserConfiguration.PackEnableSmartCategoryFilter.DisplayName),
                    Parent           = this
                };
            }
            
            foreach (var category in categories)
            {
                var node = new PathingCategoryNode(packState, category, false)
                {
                    Width       = this.Width - 30,
                    IsSearchResult = true,
                    Parent      = this
                };

                node.Active = !packState.CategoryStates.GetNamespaceInactive(category.Namespace);
            }

            return showAllSkippedCategories;
        }
    }
}
