using System;
using System.Collections.Generic;
using System.Linq;
using BhModule.Community.Pathing.Entity;
using BhModule.Community.Pathing.State;
using BhModule.Community.Pathing.UI.Controls.TreeNodes;
using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using TmfLib.Pathable;

namespace BhModule.Community.Pathing.UI.Controls.TreeView
{
    public class TreeViewBase : Container
    {
        private Control _scrollToChildControl = null;

        private static readonly Logger _logger = Logger.GetLogger<TreeViewBase>();
        private PathingCategoryNode _skipStateCheckNode;

        public PackInitiator PackInitiator { get; private set; }
        public IList<TreeNodeBase> AllBaseNodes { get; } = new List<TreeNodeBase>();
        protected IList<TreeNodeBase> ChildBaseNodes { get; } = new List<TreeNodeBase>();

        public TreeViewNavigationContext NavigationContext { get; set; }

        public event EventHandler<EventArgs> NodeLoadingStarted;
        public event EventHandler<EventArgs> NodesLoadedFinished;

        public ILookup<PathingCategory, IPathingEntity> EntityLookup { get; protected set; }

        public TreeViewBase(PackInitiator packInitiator) {
            PackInitiator = packInitiator;
        }

        public void AddNode(TreeNodeBase node) {
            if(!AllBaseNodes.Contains(node))
                AllBaseNodes.Add(node);
        }

        public void RemoveNode(TreeNodeBase node) {
            if(AllBaseNodes.Contains(node))
                AllBaseNodes.Remove(node);
        }

        public void SetPackInitiator(PackInitiator packInitiator) {
            if (packInitiator == null || PackInitiator == packInitiator) return;

            if(PackInitiator != null)
                PackInitiator.PackState.UserConfiguration.GlobalPathablesEnabled.SettingChanged -= GlobalPathablesEnabledOnSettingChanged;

            PackInitiator = packInitiator;

            PackInitiator.PackState.UserConfiguration.GlobalPathablesEnabled.SettingChanged += GlobalPathablesEnabledOnSettingChanged;
        }

        protected void RefreshEntityLookup() {
            this.EntityLookup = PackInitiator.PackState.Entities.ToArray().ToLookup(e => e.Category);
        }

        protected void InvokeNodeLoadingStarted() {
            NodeLoadingStarted?.Invoke(this, EventArgs.Empty);
        }

        protected void InvokeNodesLoadedFinished() {
            NodesLoadedFinished?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnChildAdded(ChildChangedEventArgs e)
        {
            if (!(e.ChangedChild is TreeNodeBase newChild)) return;

            AddNode(newChild);
            this.ChildBaseNodes.Add(newChild);

            ReflowChildLayout(this.ChildBaseNodes);

            base.OnChildAdded(e);
        }

        protected override void OnChildRemoved(ChildChangedEventArgs e)
        {
            if (e.ChangedChild is TreeNodeBase newChild) {
                RemoveNode(newChild);
                this.ChildBaseNodes.Remove(newChild);
            }

            base.OnChildRemoved(e);
        }

        public override void RecalculateLayout() {
            try {
                ReflowChildLayout(ChildBaseNodes);
            } catch (Exception ex) {
                //Investigate why collection is sometimes modified during reflow
                _logger.Warn($"Could not recalculate TreeView layout: {ex.Message}");
            }
           
            base.RecalculateLayout();
        }

        private int ReflowChildLayout(IList<TreeNodeBase> containerChildren)
        {
            var lastBottom =  0;

            var children = containerChildren.Where(c => c.Visible).ToList();

            foreach (var child in children)
            {
                child.Location = new Point(0, lastBottom);

                lastBottom = child.Bottom;
            }
            
            return lastBottom;
        }

        public void ClearChildNodes()
        {
            var controlsQueue = new Queue<Control>(this.ChildBaseNodes);

            while (controlsQueue.Count > 0)
            {
                var control = controlsQueue.Dequeue();

                control.Parent = null;
                control.Dispose();
            }
        }

        protected virtual void GlobalPathablesEnabledOnSettingChanged(object sender, ValueChangedEventArgs<bool> e) {
            // Override in subclasses that need to respond to global pathables enabled changes
        }

        public void SkipNextStateCheck(PathingCategoryNode node) {
            _skipStateCheckNode = node;
        }

        public void UpdateResultsCheckState(IPackState packState) {
            foreach (var node in AllBaseNodes) {
                if (node is PathingCategoryNode { IsSearchResult: true, Checkable: true } categoryNode && categoryNode != _skipStateCheckNode) {
                    categoryNode.Checked = !packState.CategoryStates.GetCategoryInactive(categoryNode.PathingCategory);
                    categoryNode.Active = !packState.CategoryStates.GetNamespaceInactive(categoryNode.PathingCategory.Namespace);

                    categoryNode.InvalidatePath();
                }
            }

            _skipStateCheckNode = null;
        }

        protected void SetScrollToChild(Control control) {
            _scrollToChildControl = control;

            if (_scrollToChildControl != null && this.Parent is CustomFlowPanel parentPanel)
            {
                parentPanel.ScrollToChild(_scrollToChildControl, _scrollToChildControl.Height);
            }
        }

        protected override void OnResized(ResizedEventArgs e) {
            base.OnResized(e);

            if (_scrollToChildControl != null && this.Parent is CustomFlowPanel parentPanel)
            {
                parentPanel.ScrollToChild(_scrollToChildControl, _scrollToChildControl.Height);
                _scrollToChildControl = null;
            }
        }

        protected override void DisposeControl() {
            if (PackInitiator != null) {
                PackInitiator.PackState.UserConfiguration.GlobalPathablesEnabled.SettingChanged -= GlobalPathablesEnabledOnSettingChanged;
            }

            base.DisposeControl();
        }
    }
}
