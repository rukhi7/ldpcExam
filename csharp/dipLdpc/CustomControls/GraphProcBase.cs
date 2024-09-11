using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace CustomControls
{
    public abstract class GraphProcBase
    {
        protected List<string> curveNames = new List<string>();
//        protected static Pen drawingPen2 = new Pen(Brushes.LightGreen, 3);
        protected List<Pen> constPenList = new List<Pen>{ new Pen(Brushes.LightGreen, 3), new Pen(Brushes.Red, 3),
                                                           new Pen(Brushes.Orange, 3), new Pen(Brushes.BlueViolet, 3) };
        SolidColorBrush rectBr = new SolidColorBrush();
        protected Pen drawingPen;
        protected List<Pen> penList = new List<Pen>();
        protected int drawIndx;
        internal abstract Point GetStartPxlPoint(in graphXYControl.axisDefsDbl xDefs, in graphXYControl.axisDefsDbl yDefs);
        public abstract void resetAxisLimits(graphXYControl.DrawAreaRect rect);

        internal abstract void drawGraph2(DrawingContext dc, Point StartLinePt, in graphXYControl.axisDefsDbl xDefs, in graphXYControl.axisDefsDbl yDefs);
        FormattedText dblToTxt()
        {
            return new FormattedText(
                curveNames[drawIndx],
                CultureInfo.GetCultureInfo("en-us"),
                FlowDirection.LeftToRight,
                new Typeface("Verdana"),
                12,
                Brushes.Black);
        }
        internal void drawLegend(DrawingContext dc, Point StartLinePt, in graphXYControl.axisDefsDbl xDefs, in graphXYControl.axisDefsDbl yDefs)
        {
            Point nexPt = new Point(StartLinePt.X, StartLinePt.Y - yDefs.margin / 4);
//            nexPt.X += 50;
//            dc.DrawLine(drawingPen, StartLinePt, nexPt);
            FormattedText ft = dblToTxt();
            dc.DrawText(ft, nexPt);
            int marg = 2;
            dc.DrawRectangle(rectBr, drawingPen, new Rect(nexPt.X- marg, nexPt.Y- marg, ft.Width+2* marg, ft.Height+2* marg));
        }
        internal abstract object initDrawGraph();
//        public abstract void AddYarray(double[] Yarr);
        internal abstract object NextGraphToDraw();
    }
}
