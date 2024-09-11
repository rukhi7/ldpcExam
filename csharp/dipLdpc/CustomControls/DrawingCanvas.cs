using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CustomControls
{
    public class DrawingCanvasLib : Panel
    {
        private List<Visual> visuals = new List<Visual>();
        protected override Visual GetVisualChild(int index)
        {
            return visuals[index];
        }
        protected override int VisualChildrenCount
        {
            get
            {
                return visuals.Count;
            }
        }
        public void AddVisual(Visual visual)
        {
            visuals.Add(visual);

            base.AddVisualChild(visual);
            base.AddLogicalChild(visual);
        }
        public void RemoveVisual(Visual visual)
        {
            visuals.Remove(visual);

            base.RemoveVisualChild(visual);
            base.RemoveLogicalChild(visual);
        }
        internal void ClearSurface()
        {
            foreach (Visual visual in visuals)
            {
                base.RemoveVisualChild(visual);
                base.RemoveLogicalChild(visual);
            }
            visuals.Clear();
        }
        public DrawingVisual GetVisual(Point point)
        {
            HitTestResult hitResult = VisualTreeHelper.HitTest(this, point);

            return hitResult.VisualHit as DrawingVisual;
        }
    }
}
