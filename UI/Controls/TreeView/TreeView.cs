using System;
using System.Collections.Generic;
using System.Linq;
using BhModule.Community.Pathing.State;
using BhModule.Community.Pathing.Utility;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using TmfLib.Pathable;

namespace BhModule.Community.Pathing.UI.Controls.TreeView
{
    public class TreeView : Container
    {
        public IList<TreeNodeBase> ChildBaseNodes { get; } = new List<TreeNodeBase>();

        protected override void OnChildAdded(ChildChangedEventArgs e)
        {
            if (!(e.ChangedChild is TreeNodeBase newChild)) return;

            this.ChildBaseNodes.Add(newChild);

            ReflowChildLayout(this.ChildBaseNodes);
        }

        protected override void OnChildRemoved(ChildChangedEventArgs e)
        {
            if (e.ChangedChild is TreeNodeBase newChild)
                this.ChildBaseNodes.Remove(newChild);

            base.OnChildRemoved(e);
        }

        public override void RecalculateLayout() {
            UpdateContentRegion();

            base.RecalculateLayout();
        }

        private int ReflowChildLayout(IEnumerable<TreeNodeBase> containerChildren)
        {
            var lastBottom =  0;

            foreach (var child in containerChildren)
            {
                child.Location = new Point(0, lastBottom);

                lastBottom = child.Bottom;
            }

            return lastBottom;
        }

        private void UpdateContentRegion()
        {
            int bottomChild = ReflowChildLayout(this.ChildBaseNodes);

            this.ContentRegion = new Rectangle(0, 0, this.Width, bottomChild);

            this.Height = this.ContentRegion.Bottom;
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
    }
}
