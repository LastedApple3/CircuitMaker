using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using CircuitMaker.Basics;

namespace CircuitMaker.GUI
{
    public partial class GUIForm : Form
    {
        public GUIForm()
        {
            InitializeComponent();
        }
    }

    public class Builder : UserControl
    {
        protected bool panning;
        protected Point panLastMouseLocation;

        protected Matrix transformationMatrix;

        private Board board;

        private ColourScheme colourScheme;

        public Builder()
        {
            DoubleBuffered = true;

            transformationMatrix = new Matrix();
            transformationMatrix.Scale(10, 10);

            colourScheme = new ColourScheme();
            colourScheme.Background = Color.White;
            colourScheme.ComponentEdge = Color.Black;
            colourScheme.ComponentBackground = Color.LightYellow;
            colourScheme.Wire = Color.Black;

            //board = new Board("current build");
            board = Board.Load("SR-Nor-Latch");
        }

        private Matrix GetInvertedTransformationMatrix()
        {
            Matrix invertedMatrix = transformationMatrix.Clone();
            invertedMatrix.Invert();
            return invertedMatrix;
        }

        private Point DetransformPoint(Point point)
        {
            Point[] points = new Point[] { point };
            DetransformPoints(points);
            return points[0];
        }

        private PointF DetransformPoint(PointF point)
        {
            PointF[] points = new PointF[] { point };
            DetransformPoints(points);
            return points[0];
        }

        private void DetransformPoints(Point[] points)
        {
            GetInvertedTransformationMatrix().TransformPoints(points);
        }

        private void DetransformPoints(PointF[] points)
        {
            GetInvertedTransformationMatrix().TransformPoints(points);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics graphics = e.Graphics;

            graphics.ResetTransform();
            graphics.Clear(Color.White);

            graphics.DrawRectangle(new Pen(Color.Red, 1), 0, 0, Width - 1, Height - 1);

            graphics.MultiplyTransform(transformationMatrix);

            Point[] corners = new Point[] { new Point(0, 0), new Point(Width - 1, Height - 1) };
            DetransformPoints(corners);

            /*
            int startX = corners[0].X - 1, startY = corners[0].Y - 1,
                endX = corners[1].X + 1, endY = corners[1].Y + 1;

            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    graphics.FillEllipse(Brushes.DarkGray, x - 0.01F, y - 0.01F, 0.02F, 0.02F);
                }
            }//*/

            //graphics.FillEllipse(Brushes.Green, zoomAndPan.TranslateRect(new Rectangle(-20, -10, 40, 20)));

            board.Render(graphics, Rectangle.FromLTRB(corners[0].X - 1, corners[0].Y - 1, corners[1].X + 1, corners[1].Y + 1), colourScheme);
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);

            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Right)
            {
                panning = true;

                panLastMouseLocation = e.Location;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button == MouseButtons.Right)
            {
                panning = false;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (panning)
            {
                PointF[] locs = new PointF[] { e.Location, panLastMouseLocation };
                DetransformPoints(locs);

                transformationMatrix.Translate(locs[0].X - locs[1].X, locs[0].Y - locs[1].Y);

                panLastMouseLocation = e.Location;

                Invalidate();
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (!panning)
            {
                float scale = (Math.Sign(e.Delta) * 0.1F) + 1;

                Point[] locs = new Point[] { e.Location };

                DetransformPoints(locs);

                transformationMatrix.Translate(locs[0].X, locs[0].Y);
                transformationMatrix.Scale(scale, scale);
                transformationMatrix.Translate(-locs[0].X, -locs[0].Y);

                Invalidate();
            }
        }
    }

    static class RenderTools
    {
        public static (Point, Point) CornersFromRect(Rectangle rect)
        {
            return (new Point(rect.Left, rect.Top), new Point(rect.Right, rect.Bottom));
        }

        public static Rectangle RectFromCorners(Point topLeft, Point bottomRight)
        {
            return new Rectangle(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);
        }
    }

    public struct ZoomAndPan
    {
        public Point PanPoint;
        public double ZoomMult;

        public int PanX
        {
            get { return PanPoint.X; }
            set { PanPoint.X = value; }
        }

        public int PanY
        {
            get { return PanPoint.Y; }
            set { PanPoint.Y = value; }
        }

        private int ApplyZoomMult(double val)
        {
            return (int)Math.Round(val * ZoomMult);
        }

        public Point TranslatePoint(Point point)
        {
            return new Point(
                ApplyZoomMult(point.X + PanX),
                ApplyZoomMult(point.Y + PanY));
        }

        public Rectangle TranslateRect(Rectangle rect)
        {
            //Point topLeft, bottomRight;

            //(topLeft, bottomRight) = RenderTools.CornersFromRect(rect);

            //return RenderTools.RectFromCorners(TranslatePoint(topLeft), TranslatePoint(bottomRight));

            return new Rectangle(TranslatePoint(rect.Location), new Size(ApplyZoomMult(rect.Width), ApplyZoomMult(rect.Height)));
        }
    }

    public struct RenderInstructions
    {

    }
}
