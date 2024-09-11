using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace CustomControls
{
    public class GraphXYDblProc : GraphProcBase
    {
        int constReadIndx;
        List<double[]> ySetList = new List<double[]>();

        double[] Yarr;
        double[] Xarr;
        internal GraphXYDblProc(double[] Yarr, double[] Xarr, Pen drawingPen, string curveName)
        {
            curveNames.Add(curveName);
            this.drawingPen = drawingPen;
            this.Yarr = Yarr;
            this.Xarr = Xarr;
            ySetList.Add(Yarr);
            penList.Add(drawingPen);
            drawIndx = 0;
        }
        internal override object initDrawGraph()
        {
            Yarr = ySetList[drawIndx = 0];
            drawingPen = penList[drawIndx];
            return Yarr;
        }
        public void AddYarray(double[] Yarr, string curveName)
        {
            curveNames.Add(curveName);
            ySetList.Add(Yarr);
            Pen drawingPen2;
            if (constReadIndx < constPenList.Count)
            {
                drawingPen2 = constPenList[constReadIndx++];
                penList.Add(drawingPen2);
            }
            else
                throw new NotImplementedException($"graph Number>={constReadIndx} is not supported!");
        }
        internal override object NextGraphToDraw()
        {
            drawingPen = ++drawIndx < ySetList.Count ? penList[drawIndx]:null;
            Yarr = drawIndx < ySetList.Count ? ySetList[drawIndx] : null;
            return Yarr;
        }
        internal override Point GetStartPxlPoint(in graphXYControl.axisDefsDbl xDefs, in graphXYControl.axisDefsDbl yDefs)
        {
            int indx = 0;
            double x = xDefs.margin + (Xarr[indx] - xDefs.n0 * xDefs.lblStepD) * xDefs.toPxlK;
            double y = yDefs.margin + ((yDefs.n0 + yDefs.n1) * yDefs.lblStepD - Yarr[indx]) * yDefs.toPxlK;
            return new Point(x, y);
        }
        public override void resetAxisLimits(graphXYControl.DrawAreaRect rect)
        {
            rect.ymin = double.MaxValue;
            rect.ymax = double.MinValue;
            foreach (var y in ySetList)
            {
                double tmp = y.Where(x => x > double.NegativeInfinity && x<double.PositiveInfinity).Min();
                if (tmp < rect.ymin)
                    rect.ymin = tmp;
                tmp = y.Max();
                if(tmp> rect.ymax)
                rect.ymax = tmp;
            }
            rect.xmin = Xarr.Min();
            rect.xmax = Xarr.Max();
        }

        internal override void drawGraph2(DrawingContext dc, Point StartLinePt, in graphXYControl.axisDefsDbl xDefs, in graphXYControl.axisDefsDbl yDefs)
        {
            double Xstart = xDefs.margin - xDefs.n0 * xDefs.lblStepD * xDefs.toPxlK;
            double Xstep = xDefs.toPxlK;
            double yStart = yDefs.margin + (yDefs.n0 + yDefs.n1) * yDefs.lblStepD * yDefs.toPxlK;
            double Ystep = yDefs.toPxlK;

            Point nexPt = StartLinePt;
            for (int indx = 1; indx < Yarr.Length; indx++)
            {
                nexPt.X = Xstart + Xarr[indx] * Xstep;
                nexPt.Y = yStart - Yarr[indx] * Ystep;
                dc.DrawLine(drawingPen, StartLinePt, nexPt);
                //DrawLine(visual, nexPt, false);
                StartLinePt = nexPt;
            }
        }
    }
}
