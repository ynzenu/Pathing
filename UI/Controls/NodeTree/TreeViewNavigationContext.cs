using System;
using TmfLib.Pathable;

namespace BhModule.Community.Pathing.UI.Controls.TreeView
{
    /// <summary>
    /// Provides navigation capabilities from any TreeViewBase instance
    /// back to the main category explorer.
    /// </summary>
    public class TreeViewNavigationContext
    {
        private readonly Action<PathingCategory> _navigateToCategory;

        public TreeViewNavigationContext(Action<PathingCategory> navigateToCategory)
        {
            _navigateToCategory = navigateToCategory;
        }

        /// <summary>
        /// Navigate to the given category in the main "All Categories" explorer tab.
        /// </summary>
        public void NavigateToCategory(PathingCategory category)
        {
            _navigateToCategory?.Invoke(category);
        }
    }
}
