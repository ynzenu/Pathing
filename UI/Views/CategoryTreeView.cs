using System;
using System.Threading.Tasks;
using System.Threading;
using BhModule.Community.Pathing.UI.Controls.TreeView;
using BhModule.Community.Pathing.UI.Presenter;
using BhModule.Community.Pathing.Utility;
using BhModule.Community.Pathing.UI.Controls;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using Blish_HUD;
using TmfLib.Pathable;
using System.Linq;

namespace BhModule.Community.Pathing.UI.Views
{
    public class CategoryTreeView : View
    {
        private static readonly Logger _logger = Logger.GetLogger<CategoryTreeView>();
        private FlowPanel RepoFlowPanel { get; set; }
        private TabbedRegion _tabbedRegion;
        private FlowPanel _nearMeFlowPanel;
        private FlowPanel _searchFlowPanel;

        public CategoryExplorerTreeView ExplorerTreeView { get; private set; }
        public SearchResultsTreeView SearchTreeView { get; private set; }
        public DistanceTreeView NearMeTreeView { get; private set; }

        private StandardButton _refreshNearMeButton;
        private Checkbox _activeOnlyCheckbox;
        private TextBox _searchBox;

        private Label _searchEmptyStateLabel;

        private Label _packsNotInitializedLabel;

        private Label _packsNotLoadedLabel;

        private Label _helpTextLabel;

        private LoadingSpinner _loadingSpinner;

        private readonly PathingModule _module;

        private CancellationTokenSource _cancellationTokenSource;

        private TabbedRegionTab _allCategoriesTab;
        private TabbedRegionTab _searchResultsTab;
        private TabbedRegionTab _previousTab;

        public PathingCategory TargetCategory { get; set; }

        public CategoryTreeView(PathingModule module)
        {
            _module = module;

            this.WithPresenter(new CategoryTreePresenter(this, module));
        }

        protected override void Build(Container buildPanel)
        {
            this._searchBox = new TextBox
            {
                PlaceholderText = "Search markers or insert path...",
                Parent = buildPanel,
                Location = new Point(0, 10),
                Width = buildPanel.ContentRegion.Width - 25,
            };

            this._searchBox.TextChanged += SearchBoxTextChanged;

            this._packsNotInitializedLabel = new Label
            {
                Text = "Please enter the game to load the marker packs.",
                Font = GameService.Content.DefaultFont18,
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                Location = new Point(buildPanel.Width / 2 - 200, buildPanel.Height / 2 - 80),
                Parent = buildPanel,
                Visible = !PacksAreInitialized(),
            };

            this._packsNotLoadedLabel = new Label
            {
                Text = "No marker packs have been loaded.",
                Font = GameService.Content.DefaultFont18,
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                Location = new Point(buildPanel.Width / 2 - 160, buildPanel.Height / 2 - 80),
                Parent = buildPanel,
                Visible = PacksAreInitialized() && !PacksAreLoaded(),
            };

            this._helpTextLabel = new Label()
            {
                Parent = buildPanel,
                Text = "Right click on categories for options.",
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                StrokeText = true,
                TextColor = Color.LightYellow,
                Font = GameService.Content.DefaultFont16,
            };

            this._tabbedRegion = new TabbedRegion
            {
                Parent = buildPanel,
                Location = new Point(0, _searchBox.Bottom + 5),
                Size = new Point(buildPanel.ContentRegion.Width, buildPanel.ContentRegion.Height - _searchBox.Bottom - this._helpTextLabel.Height - 10),
            };

            // --- All Categories Tab ---
            this.RepoFlowPanel = new CustomFlowPanel
            {
                Size = new Point(buildPanel.ContentRegion.Width, buildPanel.ContentRegion.Height - _searchBox.Bottom - this._helpTextLabel.Height - 10),
                CanScroll = true,
                ShowBorder = true,
            };

            _allCategoriesTab = new TabbedRegionTab(this.RepoFlowPanel) { Header = () => "Explorer" };
            this._tabbedRegion.AddTab(_allCategoriesTab);

            // --- Favorites & Recent Tabs (placeholder) ---
            this._tabbedRegion.AddTab(new TabbedRegionTab(new Panel()) { Header = () => "Favorites" });
            this._tabbedRegion.AddTab(new TabbedRegionTab(new Panel()) { Header = () => "Recent" });

            // --- Near Me Tab ---
            this._nearMeFlowPanel = new CustomFlowPanel
            {
                Size = new Point(buildPanel.ContentRegion.Width, buildPanel.ContentRegion.Height - _searchBox.Bottom - this._helpTextLabel.Height - 10),
                CanScroll = true,
                ShowBorder = true,
            };

            // Toolbar: Active Only checkbox + Refresh button (both right-aligned)
            var nearMeToolbar = new Panel
            {
                Parent = this._nearMeFlowPanel,
                Width = this._nearMeFlowPanel.Width,
                Height = 35,
            };

            this._refreshNearMeButton = new StandardButton
            {
                Parent = nearMeToolbar,
                Text = "Refresh",
                Width = 100,
                Location = new Point(nearMeToolbar.Width - 130, 3),
            };

            this._activeOnlyCheckbox = new Checkbox
            {
                Parent = nearMeToolbar,
                Text = "Active only",
                Checked = true,
                Location = new Point(this._refreshNearMeButton.Left - 115, 7),
                Width = 110,
            };

            this._refreshNearMeButton.Click += (_, _) =>
            {
                RefreshNearMe();
            };

            this._activeOnlyCheckbox.CheckedChanged += (_, _) =>
            {
                RefreshNearMe();
            };

            this.NearMeTreeView = new DistanceTreeView(_module.PackInitiator)
            {
                HeightSizingMode = SizingMode.AutoSize,
                Width = _nearMeFlowPanel.Width,
                Parent = _nearMeFlowPanel
            };

            var nearMeTab = new TabbedRegionTab(this._nearMeFlowPanel) { Header = () => "Near Me" };
            this._tabbedRegion.AddTab(nearMeTab);

            // --- Search Results Tab ---
            this._searchFlowPanel = new CustomFlowPanel
            {
                Size = new Point(buildPanel.ContentRegion.Width, buildPanel.ContentRegion.Height - _searchBox.Bottom - this._helpTextLabel.Height - 10),
                CanScroll = true,
                ShowBorder = true,
            };

            this._searchEmptyStateLabel = new Label
            {
                Parent = _searchFlowPanel,
                Text = "Use the search bar to find categories.",
                Font = GameService.Content.DefaultFont18,
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                TextColor = Color.LightGray,
                Visible = true,
            };

            this.SearchTreeView = new SearchResultsTreeView(_module.PackInitiator)
            {
                HeightSizingMode = SizingMode.AutoSize,
                Width = _searchFlowPanel.Width,
                Parent = _searchFlowPanel
            };

            _searchResultsTab = new TabbedRegionTab(this._searchFlowPanel) { Header = () => "Search Results" };
            this._tabbedRegion.AddTab(_searchResultsTab);

            this._tabbedRegion.OnTabSwitched = () =>
            {
                if (this._tabbedRegion.ActiveTab == nearMeTab)
                {
                    RefreshNearMe();
                }
            };

            this._helpTextLabel.Location = new Point(15, this._tabbedRegion.Bottom);

            // --- Explorer TreeView (All Categories) ---
            this.ExplorerTreeView = new CategoryExplorerTreeView(_module.PackInitiator)
            {
                HeightSizingMode = SizingMode.AutoSize,
                Width = RepoFlowPanel.Width,
                Parent = RepoFlowPanel
            };

            // --- Wire NavigationContext ---
            var navigationContext = new TreeViewNavigationContext(NavigateToCategory);
            this.ExplorerTreeView.NavigationContext = navigationContext;
            this.SearchTreeView.NavigationContext = navigationContext;
            this.NearMeTreeView.NavigationContext = navigationContext;

            this._loadingSpinner = new LoadingSpinner
            {
                Parent = buildPanel,
                Location = new Point(buildPanel.Width / 2 - 75, buildPanel.Height / 2 - 75),
                Size = new Point(55, 55),
                Visible = false // Hide by default
            };

            this.ExplorerTreeView.NodeLoadingStarted += (_, _) =>
            {
                _cancellationTokenSource?.Cancel();

                SetLoading(true);
            };

            this.ExplorerTreeView.NodesLoadedFinished += (_, _) =>
            {
                SetLoading(false);
            };

            // Center the empty state label after layout is established
            CenterSearchEmptyStateLabel();
        }

        private void CenterSearchEmptyStateLabel()
        {
            if (_searchEmptyStateLabel == null || _searchFlowPanel == null) return;

            _searchEmptyStateLabel.Location = new Point(
                _searchFlowPanel.Width / 2 - 150,
                _searchFlowPanel.Height / 2 - 40
            );
        }

        private void SwitchToTab(TabbedRegionTab tab)
        {
            if (tab != null)
                _tabbedRegion.ActiveTab = tab;
        }

        private void RefreshNearMe()
        {
            if (_module.PackInitiator?.PackState == null) return;

            this.NearMeTreeView.SetNearMeResults(
                _module.PackInitiator.PackState,
                200,
                _activeOnlyCheckbox?.Checked ?? false
            );
        }

        public bool ValidateMarkerPacksState()
        {
            var packsAreInitialized = PacksAreInitialized();
            var packsAreLoaded = PacksAreLoaded();

            if (_packsNotInitializedLabel != null)
                _packsNotInitializedLabel.Visible = !packsAreInitialized;

            if (_packsNotLoadedLabel != null)
                _packsNotLoadedLabel.Visible = packsAreInitialized && !packsAreLoaded;

            return packsAreLoaded;
        }

        public void SetLoading(bool loading)
        {
            if (loading)
            {
                if (_packsNotInitializedLabel != null)
                    _packsNotInitializedLabel.Visible = false;

                if (_packsNotLoadedLabel != null)
                    _packsNotLoadedLabel.Visible = false;
            }

            this._loadingSpinner.Visible = loading;
        }

        private void SearchBoxTextChanged(object sender, EventArgs e)
        {
            if (Presenter is CategoryTreePresenter presenter)
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource = new CancellationTokenSource();

                ExplorerTreeView.RemoveNodeHighlights();

                if (_searchBox.Text.StartsWith("."))
                {
                    // Path navigation — switch to All Categories tab
                    SwitchToTab(_allCategoriesTab);
                    ExplorerTreeView.NavigateToPath(_searchBox.Text);
                    return;
                }

                if (string.IsNullOrWhiteSpace(_searchBox.Text))
                {
                    // Search cleared — clear results and switch back to previous tab
                    SearchTreeView.ClearChildNodes();
                    _searchEmptyStateLabel.Text = "Use the search bar to find categories.";
                    _searchEmptyStateLabel.Visible = true;
                    CenterSearchEmptyStateLabel();

                    SwitchToTab(_previousTab ?? _allCategoriesTab);

                    presenter.DoUpdateView();
                    return;
                }

                // Active search — switch to Search Results tab
                if (_tabbedRegion.ActiveTab != _searchResultsTab)
                    _previousTab = _tabbedRegion.ActiveTab;

                _searchEmptyStateLabel.Visible = false;
                SwitchToTab(_searchResultsTab);
                SearchTreeView.ClearChildNodes();
                SetLoading(true);

                Task.Run(async () =>
                {
                    await ExecuteSearch(_searchBox.Text, _cancellationTokenSource.Token);
                }, _cancellationTokenSource.Token);
            }
        }

        public void NavigateToCategory(PathingCategory category)
        {
            SwitchToTab(_allCategoriesTab);
            this.ExplorerTreeView?.LoadNodes();
            this.ExplorerTreeView?.NavigateToPath(category.GetPath());
        }

        private static readonly SemaphoreSlim _searchSemaphore = new SemaphoreSlim(1, 1); // Limit to 1 concurrent search

        private async Task ExecuteSearch(string input, CancellationToken cancellationToken, bool forceShowAll = false)
        {
            await _searchSemaphore.WaitAsync(cancellationToken);

            try
            {

                cancellationToken.ThrowIfCancellationRequested();

                await Task.Delay(200, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                var searchResult = await ExplorerTreeView.SearchAsync(input, cancellationToken, forceShowAll);

                cancellationToken.ThrowIfCancellationRequested();

                var showAllSkippedNode = SearchTreeView.SetSearchResults(searchResult.categories, _module.PackInitiator.PackState, searchResult.skipped);

                cancellationToken.ThrowIfCancellationRequested();

                if (showAllSkippedNode != null)
                {
                    showAllSkippedNode.LeftMouseButtonReleased += async (_, _) =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        await ExecuteSearch(input, cancellationToken, true);
                    };
                }

                var count = searchResult.categories.Count;

                if (count <= 0)
                {
                    _searchEmptyStateLabel.Text = "No categories found...";
                    _searchEmptyStateLabel.Visible = true;
                    CenterSearchEmptyStateLabel();
                }
                else
                {
                    _searchEmptyStateLabel.Visible = false;
                }

                SetLoading(false);
            }
            catch (OperationCanceledException _)
            {
                //Triggered when a new search is executed
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Category search failed.");
            }
            finally
            {
                _searchSemaphore.Release();
            }
        }

        public void ResetSearch()
        {
            _searchBox.Text = string.Empty;
        }

        private bool PacksAreInitialized()
        {
            return _module.PackInitiator?.GetAllMarkersCategories() != null;
        }

        private bool PacksAreLoaded()
        {
            var rootCategory = _module.PackInitiator.GetAllMarkersCategories();

            return rootCategory != null && !(rootCategory.Count(c => c.LoadedFromPack) <= 0);
        }
    }
}
