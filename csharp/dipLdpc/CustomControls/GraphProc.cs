using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace CustomControls
{
    public class GraphProc : GraphProcBase
    {
        private int[] arr;
        double xTransK;
        int xZero;
        List<int[]> ySetList = new List<int[]>();
        int constReadIndx;

        public GraphProc(int[] parr, Pen drawingPen, double v1, int v2, string curveName)
        {
            curveNames.Add(curveName);
            xTransK = v1;
            xZero = v2;
            this.drawingPen = drawingPen;
            this.arr = parr;
            ySetList.Add(arr);
            penList.Add(drawingPen);
            drawIndx = 0;
        }

        internal override object initDrawGraph()
        {
            arr = ySetList[drawIndx = 0];
            drawingPen = penList[drawIndx];
            return arr;
        }
        public void AddYarray(int[] Yarr, string curveNm)
        {
            curveNames.Add(curveNm);
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
            drawingPen = ++drawIndx < ySetList.Count ? penList[drawIndx] : null;
            arr = drawIndx < ySetList.Count ? ySetList[drawIndx] : null;
            return arr;
        }
        public override void resetAxisLimits(graphXYControl.DrawAreaRect rect)
        {
            rect.ymin = arr.Min();
            rect.xmin = -xZero;
            rect.xmax = rect.xmin + arr.Length - 1;
            rect.xmin *= xTransK;
            rect.xmax *= xTransK;
            rect.ymax = arr.Max();
        }

        internal override Point GetStartPxlPoint(in graphXYControl.axisDefsDbl xDefs, in graphXYControl.axisDefsDbl yDefs)
        {
            int indx = 0;
            double x = xDefs.margin - (xTransK * (xZero - 0.5) + xDefs.n0 * xDefs.lblStepD) * xDefs.toPxlK;
            double y = yDefs.margin + ((yDefs.n0 + yDefs.n1) * yDefs.lblStepD - arr[indx]) * yDefs.toPxlK;
            return new Point(x, y);
        }

        internal override void drawGraph2(DrawingContext dc, Point StartLinePt, in graphXYControl.axisDefsDbl xDefs, in graphXYControl.axisDefsDbl yDefs)
        {
            double Xstep = xDefs.toPxlK * xTransK;
            double yStart = yDefs.margin + (yDefs.n0 + yDefs.n1) * yDefs.lblStepD * yDefs.toPxlK;
            double Ystep = yDefs.toPxlK;

           Point nexPt = StartLinePt;
            for (int indx = 1; indx < arr.Length; indx++)
            {
                nexPt.X += Xstep;
                nexPt.Y = yStart - arr[indx] * Ystep;
                dc.DrawLine(drawingPen, StartLinePt, nexPt);
                //DrawLine(visual, nexPt, false);
                StartLinePt = nexPt;
            }
        }
    }
}
