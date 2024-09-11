using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace CustomControls
{
    /// <summary>
    /// Interaction logic for graphXYControl.xaml
    /// </summary>
    public partial class graphXYControl : UserControl
    {
        public graphXYControl()
        {
            InitializeComponent();
            drawArray = drawArrayMetod2;
        }
        public class DrawAreaRect
        {
            internal double xmin;
            internal double xmax;
            internal double ymin;
            internal double ymax;
        }
        DrawAreaRect XYlimits = new DrawAreaRect();
        private Pen redPen = new Pen(Brushes.Red, 4);
        static private Pen drawingPen = new Pen(Brushes.SteelBlue, 4);
        private Pen drawingPen2 = new Pen(Brushes.LightGreen, 3);
        private Pen axePen = new Pen(Brushes.LightGray, 2);
        private Pen gridPen = new Pen(Brushes.LightGray, 1);

        public int[] arr2;
        Point StartLinePt;
        RadioButton[] clstSwitch = new RadioButton[5];
        GraphProcBase gProc;

        internal graphXYControl(GraphProcBase gpb)
        {
            InitializeComponent();

            drawArray = drawArrayMetod2;
            gProc = gpb;
            gProc.resetAxisLimits(XYlimits);
        }
        public void setArray(GraphProcBase gpb)
        {
            gProc = gpb;
            gProc.resetAxisLimits(XYlimits);
        }
        public static GraphProc getIntGraph(int[] parr, double v1, int v2, string curveName)
        {
            GraphProc gProc = new GraphProc(parr, drawingPen, v1, v2, curveName);
            return gProc;
        }
        public static GraphXYDblProc getDblXYGraph(double[] Yarr, double[] Xarr, string curveName)
        {
            GraphXYDblProc gProc = new GraphXYDblProc(Yarr, Xarr, drawingPen, curveName);
            //throw new NotImplementedException("No getDblGraph");

            return gProc;
        }

        delegate void drawArrayDel();

        readonly drawArrayDel drawArray;

        bool YrangeFlag = true;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            gProc.resetAxisLimits(XYlimits);
            if (YrangeFlag)
            {
                if (XYlimits.ymin > 0) XYlimits.ymin = 0;
                else
                    if (XYlimits.ymax < 0) XYlimits.ymax = 0;
            }
            YrangeFlag ^= true;
            visualGraph = null;
            drawArray();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            drawArray();
        }

        int clusterN;
        private void ClusterBtn_Click(object sender, RoutedEventArgs e)
        {
            //D:\tmpG8\ken\OTA\DC0924153556-LBb.bin
            RadioButton btn = e.Source as RadioButton;
            if (btn == null) return;
            string str = (string)btn.Content;
            int clN = str.Split(' ')[1][0] - '1';
            if (clusterN != clN)
            {
                //                arr = ensElmnts.getClusterArr(clN);
                gProc.resetAxisLimits(XYlimits);
                visualGraph = null;
                YrangeFlag = true;
                clusterN = clN;
            }
            drawArray();
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            drawArray();
        }

        internal struct axisDefsDbl
        {
            public int margin;
            public int pxlSz;//graph area size in pixels
                             //            public int lblSz;
            public double toPxlK;//ratio(koeficient) for conversion number to pixels count
            public double lblStepD;//the number choosen interval to mark with labels
            public int n0;//count of lblSteps till the bottom of graph
            public int n1;//count of lblSteps in the graph (height of graph)
            public int minLblSz;//Label interval size in pixels
            public int power10;
        }

        FormattedText dblToTxt(double number)
        {
            return new FormattedText(
                number.ToString(),
                CultureInfo.GetCultureInfo("en-us"),
                FlowDirection.LeftToRight,
                new Typeface("Verdana"),
                12,
                Brushes.Black);
        }
        //void DrawAxeY(Point lnPt1, Point lnPt2, double yStep, int ystart, int valIncr, Point xLinePt1, Point xLinePt2)
        void DrawAxisY(in axisDefsDbl x, in axisDefsDbl y, double x_toPxlK, double y_toPxlK)
        {
            DrawingVisual visual = new DrawingVisual();
            double valIncr = y.lblStepD;
            double ystart = y.n0 * y.lblStepD;
            double yStep = y.lblStepD * y_toPxlK;
            Point lnPt1 = new Point { X = x.margin, Y = y.margin + y.n1 * yStep };//vertical
            Point lnPt2 = new Point { X = x.margin, Y = y.margin };
            Point xLinePt1 = lnPt1;//horizontal
            Point xLinePt2 = new Point { X = x.margin + x.n1 * x.lblStepD * x_toPxlK, Y = lnPt1.Y };
            double yPos = lnPt1.Y;
            using (DrawingContext dc = visual.RenderOpen())
            {
                //               dc.DrawLine(axePen, lnPt1, lnPt2);//vertical
                lnPt1.X = 5;
                double minLblSz = y.minLblSz;
                for (; yPos >= lnPt2.Y; yPos -= yStep)
                {
                    lnPt1.Y = yPos - 12;
                    // Create the initial formatted text string.
                    if (minLblSz >= y.minLblSz)
                    {
                        if (y.power10 < 0)
                            ystart = Math.Round(ystart, -y.power10 + 1);
                        dc.DrawText(dblToTxt(ystart), lnPt1);
                        minLblSz = 0;
                    }
                    dc.DrawLine(gridPen, xLinePt1, xLinePt2);//horizontal
                    xLinePt1.Y -= yStep; xLinePt2.Y -= yStep;
                    minLblSz += yStep;
                    ystart += valIncr;
                }
            }
            drawingSurface.AddVisual(visual);
        }
        void DrawAxisX(in axisDefsDbl x, in axisDefsDbl y, double x_toPxlK, double y_toPxlK)
        {
            DrawingVisual visual = new DrawingVisual();
            double valIncr = x.lblStepD;
            //            int ystart = y.n0 * y.lblStep;
            double yStep = y.lblStepD * y_toPxlK;
            double xStep = x_toPxlK * x.lblStepD;
            double xstart = x.n0 * x.lblStepD;
            Point lnPt1 = new Point { X = x.margin, Y = y.margin + y.n1 * yStep };//horizontal
            Point lnPt2 = new Point { X = x.margin + x.n1 * x.lblStepD * x_toPxlK, Y = lnPt1.Y };
            Point yLinePt1 = lnPt1;//vertical
            Point yLinePt2 = new Point { X = x.margin, Y = y.margin };
            using (DrawingContext dc = visual.RenderOpen())
            {
                double xPos = lnPt1.X;
                //              dc.DrawLine(axePen, lnPt1, lnPt2);//horizontal
                double minLblSz = x.minLblSz;
                for (; xPos < lnPt2.X; xPos += xStep)
                {
                    lnPt1.X = xPos;
                    if (minLblSz >= y.minLblSz)
                    {
                        if (x.power10 < 0)
                            xstart = Math.Round(xstart, -x.power10 + 1);
                        // Create the initial formatted text string.
                        dc.DrawText(dblToTxt(xstart), lnPt1);
                        minLblSz = 0;
                    }
                    dc.DrawLine(gridPen, yLinePt1, yLinePt2);//vertical
                    yLinePt1.X += xStep; yLinePt2.X += xStep;
                    minLblSz += xStep;
                    xstart += valIncr;
                }
            }
            drawingSurface.AddVisual(visual);
        }
        DrawingVisual legends;
        bool isDragging;
        Vector clickOffset;
        void DrawLegends(Point legendPnt)
        {
            using (DrawingContext dc = legends.RenderOpen())
            {
//                StartLinePt = gProc.GetStartPxlPoint(xDefs, yDefs);

//                legendPnt.Y += legendPnt.Y * 1.25;
                do
                {
                    gProc.drawLegend(dc, legendPnt, xDefs, yDefs);
                    legendPnt.Y += yDefs.margin / 2;
                } while (gProc.NextGraphToDraw() != null);
            }
            gProc.initDrawGraph();
        }
        DrawingVisual visualGraph;
        axisDefsDbl xDefs;
        //        axisDefs xDefsOld;
        axisDefsDbl yDefs;

        double ratioX = 1;
        double ratioY = 1;
        private void drawArrayMetod2()
        {
            if (gProc == null) return;
            drawingSurface.ClearSurface();
            int xMargin = 40;
            int yMargin = 50;
            int w = (int)drawingSurface.ActualWidth - xMargin * 2;
            int h = (int)drawingSurface.ActualHeight - yMargin * 2;

            if (visualGraph == null)
            {
                //                xDefs = calcAxisDefs(xMargin, 50, w, xTransK*arrMinValX, xTransK * arrMaxValX);
                xDefs = calcAxisDefs(xMargin, 50, w, XYlimits.xmin, XYlimits.xmax);
                yDefs = calcAxisDefs(yMargin, 35, h, XYlimits.ymin, XYlimits.ymax);
                if (xDefs.n1 < 0 || xDefs.toPxlK < 0)
                    return;
                if (yDefs.n1 < 0 || yDefs.toPxlK < 0)
                    return;
                ratioX = 1;
                ratioY = 1;
            }
            else
            {
                ratioX = (double)w / xDefs.pxlSz;
                ratioY = (double)h / yDefs.pxlSz;
            }
            double x_toPxlK = xDefs.toPxlK * ratioX;
            double y_toPxlK = yDefs.toPxlK * ratioY;
            DrawAxisY(xDefs, yDefs, x_toPxlK, y_toPxlK);
            DrawAxisX(xDefs, yDefs, x_toPxlK, y_toPxlK);
            gProc.initDrawGraph();
            Point legendPnt = new Point(xDefs.pxlSz * ratioX - 100, yDefs.margin);
            legends = new DrawingVisual();
            DrawLegends(legendPnt);
            drawingSurface.AddVisual(legends);

            if (visualGraph != null)
            {
                visualGraph.Transform = new ScaleTransform(ratioX, ratioY, xMargin, yMargin);
                drawingSurface.AddVisual(visualGraph);
                return;
            }
            Point lnPt1 = new Point { X = xDefs.margin, Y = yDefs.margin + yDefs.n1 * yDefs.lblStepD * yDefs.toPxlK };
            Point lnPt2 = new Point { X = xDefs.margin, Y = yDefs.margin };

            visualGraph = new DrawingVisual();
            
            using (DrawingContext dc = visualGraph.RenderOpen())
            {
                StartLinePt = gProc.GetStartPxlPoint(xDefs, yDefs);
                do
                {
                    //                    dc.DrawLine(redPen, lnPt1, lnPt2);
                    //drawGraph2(dc, StartLinePt, xDefs.toPxlK * xTransK, yDefs.margin + (yDefs.n0 + yDefs.n1) * yDefs.lblStepD* yDefs.toPxlK, yDefs.toPxlK);

                    gProc.drawGraph2(dc, StartLinePt, xDefs, yDefs);
                } while (gProc.NextGraphToDraw() != null);
            }
            gProc.initDrawGraph();
            drawingSurface.AddVisual(visualGraph);
        }

        private bool isMultiSelecting = false;
        private DrawingVisual selectionSquare;
        private Point selectionSquareTopLeft;
        private void drawingSurface_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pointClicked = e.GetPosition(drawingSurface);

            DrawingVisual dv = drawingSurface.GetVisual(pointClicked);
            if(dv == legends)
            {
                Point topLeftCorner = new Point(
                    dv.ContentBounds.TopLeft.X + drawingPen.Thickness / 2,
                    dv.ContentBounds.TopLeft.Y + drawingPen.Thickness / 2);
                clickOffset = topLeftCorner - pointClicked;
                isDragging = true;
                return;
            }

            selectionSquare = new DrawingVisual();

            drawingSurface.AddVisual(selectionSquare);

            selectionSquareTopLeft = pointClicked;
            isMultiSelecting = true;
            // Make sure we get the MouseLeftButtonUp event even if the user
            // moves off the Canvas. Otherwise, two selection squares could be drawn at once.
            drawingSurface.CaptureMouse();
            //            Xval.Text = pointClicked.X.ToString("F2");
            //           Yval.Text = pointClicked.Y.ToString("F2");

            pointClicked = pixelToValue(pointClicked);
            Xval.Text = pointClicked.X.ToString("F2");
            Yval.Text = pointClicked.Y.ToString("F2");
        }
        Point pixelToValue(Point pt)
        {
            pt.X = (xDefs.n0 * xDefs.lblStepD) + (pt.X - xDefs.margin) / xDefs.toPxlK / ratioX;
            pt.Y = ((yDefs.n0 + yDefs.n1) * yDefs.lblStepD) - (pt.Y - yDefs.margin) / yDefs.toPxlK / ratioY;
            return pt;
        }
        private void drawingSurface_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isDragging) isDragging = false;
            if (isMultiSelecting)
            {
                Point pointClicked = e.GetPosition(drawingSurface);
                // Display all the squares in this region.
                RectangleGeometry geometry = new RectangleGeometry(
                    new Rect(selectionSquareTopLeft, pointClicked));
                //               MessageBox.Show(String.Format("You selected {0} square(s).", 55));
                isMultiSelecting = false;
                drawingSurface.RemoveVisual(selectionSquare);
                drawingSurface.ReleaseMouseCapture();
                selectionSquareTopLeft = pixelToValue(selectionSquareTopLeft);
                pointClicked = pixelToValue(pointClicked);
                Xval.Text = pointClicked.X.ToString("F2");
                Yval.Text = pointClicked.Y.ToString("F2");

                if (pointClicked.Y < selectionSquareTopLeft.Y)
                {
                    XYlimits.ymin = pointClicked.Y;
                    XYlimits.ymax = selectionSquareTopLeft.Y;
                }
                else
                {
                    XYlimits.ymin = selectionSquareTopLeft.Y;
                    XYlimits.ymax = pointClicked.Y;
                }
                const double xTransK = 1;
                if (pointClicked.X < selectionSquareTopLeft.X)
                {
                    XYlimits.xmin = (pointClicked.X / xTransK);
                    XYlimits.xmax = (selectionSquareTopLeft.X / xTransK);
                }
                else
                {
                    XYlimits.xmin = (selectionSquareTopLeft.X / xTransK);
                    XYlimits.xmax = (pointClicked.X / xTransK);
                }
                visualGraph = null;
                //                    YrangeFlag = true;
                drawArray();
            }
        }

        private void drawingSurface_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                gProc.initDrawGraph();
                Point pointDragged = e.GetPosition(drawingSurface) + clickOffset;
                DrawLegends(pointDragged);
            }else
            if (isMultiSelecting)
            {
                Point pointDragged = e.GetPosition(drawingSurface);
                DrawSelectionSquare(selectionSquareTopLeft, pointDragged);
            }
            else
            {
                Point pointClicked = e.GetPosition(drawingSurface);
                pointClicked = pixelToValue(pointClicked);
                Xval.Text = pointClicked.X.ToString("F2");
                Yval.Text = pointClicked.Y.ToString("F2");

            }
        }
        private Brush selectionSquareBrush = Brushes.Transparent;
        private Pen selectionSquarePen = new Pen(Brushes.Black, 2);
        private void DrawSelectionSquare(Point point1, Point point2)
        {
            selectionSquarePen.DashStyle = DashStyles.Dash;

            using (DrawingContext dc = selectionSquare.RenderOpen())
            {
                dc.DrawRectangle(selectionSquareBrush, selectionSquarePen,
                    new Rect(point1, point2));
            }
        }

        private axisDefsDbl calcAxisDefs(int margin, int minLblSz, int dimentionSize, double bottomLimit, double topLimit)
        {
            axisDefsDbl xy = new axisDefsDbl();
            if (topLimit == bottomLimit) { topLimit++; bottomLimit--; }
            xy.minLblSz = minLblSz;
            xy.margin = margin;
            xy.pxlSz = dimentionSize;
            xy.n1 = dimentionSize / minLblSz;
            xy.lblStepD = findStep(ref xy, topLimit, bottomLimit);
            xy.lblStepD = Math.Round(xy.lblStepD, 15);
            xy.n1 = (int)Math.Ceiling(topLimit / xy.lblStepD);

            xy.n0 = (int)Math.Floor(bottomLimit / xy.lblStepD);
            xy.n1 -= xy.n0;
            xy.toPxlK = (double)xy.pxlSz / xy.n1 / xy.lblStepD;
            return xy;
        }
        private double findStep(ref axisDefsDbl xy, double maxVp, double minVp)
        {
            int n = xy.n1;
            if (maxVp == minVp) { minVp--; maxVp++; }
            double vStep = (maxVp - minVp) / n;
            xy.power10 = (int)Math.Floor(Math.Log10(vStep));
            double indx = Math.Pow(10, xy.power10);
            double val = Math.Floor(vStep / indx);
            {//make rem local variable
                double rem = vStep % indx;
                if (rem != 0)
                {
                    val = val + 1;
                    if (val > 9d)
                    {
                        val = 1;
                        xy.power10++;
                        indx *= 10;
                    }
                }
            }
            val *= indx;
            double minV = Math.Floor(minVp / val);
            minV *= val;
            double maxV = Math.Ceiling(maxVp / val);
            maxV *= val;
            while ((maxV - minV) / val > n)
            {
                /*                if (indx > 1)
                                    val += indx / 2;
                                else*/
                val += indx;
                minV = Math.Floor(minVp / val);
                minV *= val;
                maxV = Math.Ceiling(maxVp / val);
                maxV *= val;
            }
            return val;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
