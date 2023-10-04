using System;
using System.Collections.Generic;
//using System.ComponentModel;
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
        private enum DragType
        {
            None, Pan, MoveComponent, DrawWire
        }

        private DragType dragType = DragType.None;

        private Point panLastMouseLocation;

        //private IComponent selectedComp;

        private IComponent dragComp;
        private PointF dragOffset;
        private Pos dragResetPos;
        private Rotation dragResetRot;
        private Point dragNewPoint;
        private Rotation dragNewRot;

        protected Matrix transformationMatrix;

        private Board board;

        private ColourScheme colourScheme;

        public bool Simulating { get; private set; } = false;

        private Timer simulationTimer;

        public Builder()
        {
            DoubleBuffered = true;

            transformationMatrix = new Matrix();
            transformationMatrix.Scale(20, 20);

            /*
            colourScheme = new ColourScheme();

            colourScheme.Background = Color.White;
            colourScheme.ComponentEdge = Color.Black;
            colourScheme.ComponentBackground = Color.LightYellow;
            colourScheme.Wire = Color.Black;
            colourScheme.WireFloating = Color.Gray;
            colourScheme.WireLow = Color.DarkBlue;
            colourScheme.WireHigh = Color.Blue;
            colourScheme.WireIllegal = Color.Red;
            //*/

            //*
            colourScheme = new ColourScheme
            {
                Background = Color.White,
                ComponentEdge = Color.Black,
                ComponentBackground = Color.LightYellow,
                Wire = Color.Black,
                WireFloating = Color.Gray,
                WireLow = Color.DarkBlue,
                WireHigh = Color.Blue,
                WireIllegal = Color.Red
            };
            //*/

            simulationTimer = new Timer();
            simulationTimer.Interval = 100;
            simulationTimer.Enabled = false;
            simulationTimer.Tick += SimulationTick;

            //board = new Board("current build");
            board = Board.Load("SR-Nor-Latch");
        }

        public void SetSimulation(bool simulate)
        {
            if (dragType == DragType.None)
            {
                Simulating = simulate;
                simulationTimer.Enabled = simulate;

                if (Simulating)
                {
                    board.ResetForSimulation();
                } else {
                    board.ResetToFloating();
                }

                Invalidate();
            }
        }

        private void SimulationTick(object sender, EventArgs e)
        {
            board.Tick();

            Invalidate();
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

        private PointF DetransformPointF(PointF point)
        {
            PointF[] points = new PointF[] { point };
            DetransformPointFs(points);
            return points[0];
        }

        private void DetransformPoints(Point[] points)
        {
            GetInvertedTransformationMatrix().TransformPoints(points);
        }

        private void DetransformPointFs(PointF[] points)
        {
            GetInvertedTransformationMatrix().TransformPoints(points);
        }

        private IComponent GetClickedComponent(Point mouseLoc)
        {
            PointF mousePos = DetransformPointF(mouseLoc);

            foreach (IComponent comp in board.GetComponents())
            {
                if (((RectangleF)comp.GetOffsetComponentBounds()).Contains(mousePos))
                {
                    return comp;
                }
            }

            return null;
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
            if (dragType == DragType.MoveComponent)
            {
                Matrix matrix = new Matrix();

                matrix.Rotate((float)dragNewRot);
                matrix.Translate(dragNewPoint.X, dragNewPoint.Y);

                dragComp.Render(graphics, colourScheme);
            }
            //*/

            board.Render(graphics, Rectangle.FromLTRB(corners[0].X - 1, corners[0].Y - 1, corners[1].X + 1, corners[1].Y + 1), colourScheme);
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);

            Invalidate();
        }


        private void StartDraggingComponent(IComponent comp)
        {
            dragComp = comp;

            dragResetPos = comp.GetComponentPos();
            dragResetRot = comp.GetComponentRotation();

            comp.Remove();
        }

        private void PutDownDraggedComponent()
        {
            Point newPoint = DetransformPoint(dragNewPoint);
            Pos newPos = new Pos(newPoint.X, newPoint.Y);

            Matrix matrix = new Matrix();

            matrix.Rotate((float)dragComp.GetComponentRotation());
            matrix.Translate(dragComp.GetComponentPos().X, dragComp.GetComponentPos().Y);

            RectangleF bounds = dragComp.GetComponentBounds();
            PointF[] corners = { new PointF(bounds.Left, bounds.Top), new PointF(bounds.Right, bounds.Bottom) };

            matrix.TransformPoints(corners);

            bounds = RectangleF.FromLTRB(corners[0].X, corners[0].Y, corners[1].X, corners[1].Y);

            if (board.CheckAllowed(bounds))
            {
                dragComp.Place(newPos, dragNewRot, board);

                dragComp = null;
            } else
            {
                ResetDraggedComponent();
            }

            //dragComp.Place(newPos, dragNewRot, board);
        }

        private void ResetDraggedComponent()
        {
            dragComp.Place(dragResetPos, dragResetRot, board);

            dragComp = null;
        }


        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Right)
            {
                if (dragType == DragType.None)
                {
                    dragType = DragType.Pan;

                    panLastMouseLocation = e.Location;
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button == MouseButtons.Right)
            {
                if (dragType == DragType.Pan)
                {
                    dragType = DragType.None;
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (dragType == DragType.Pan)
            {
                PointF[] locs = new PointF[] { e.Location, panLastMouseLocation };
                DetransformPointFs(locs);

                transformationMatrix.Translate(locs[0].X - locs[1].X, locs[0].Y - locs[1].Y);

                panLastMouseLocation = e.Location;

                Invalidate();
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (dragType == DragType.None)
            {
                float scale = (Math.Sign(e.Delta) * 0.1F) + 1;

                PointF loc = DetransformPointF(e.Location);

                transformationMatrix.Translate(loc.X, loc.Y);

                transformationMatrix.Scale(scale, scale);

                float[] elements = transformationMatrix.Elements;

                transformationMatrix.Scale(
                    Math.Min(elements[0], 100F) / elements[0],
                    Math.Min(elements[3], 100F) / elements[3]
                );

                transformationMatrix.Translate(-loc.X, -loc.Y);

                Invalidate();
            }
        }

        /*
        protected override void OnMouseClick(MouseEventArgs e) // see notes.txt <------------------------------------------------------------------- next thing to do
        {
            base.OnMouseClick(e);

            if (e.Button == MouseButtons.Left)
            {
                if (Simulating)
                {
                    IComponent comp = GetClickedComponent(e.Location);

                    Console.WriteLine(comp == null ? "NONE" : comp.ToString());

                    if (comp != null)
                    {
                        if (comp is IInteractibleComponent intComp)
                        {
                            Console.WriteLine("interacting");

                            intComp.Interact();

                            Invalidate();
                        }
                    }
                } else
                {
                    if (dragType == DragType.DrawWire)
                    {
                        // finish current wire
                        // start new wire
                    } else if (dragType == DragType.None && GetClickedComponent(e.Location) == null)
                    {
                        dragType = DragType.DrawWire;
                        // start new wire
                    }
                }
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            if (e.Button == MouseButtons.Left && !Simulating)
            {
                if (dragType == DragType.DrawWire)
                {
                    dragType = DragType.None;
                }
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (dragType == DragType.None)
            {
                if (e.Button == MouseButtons.Left && !Simulating)
                {
                    dragType = DragType.MoveComponent;

                    IComponent comp = GetClickedComponent(e.Location);

                    if (comp != null)
                    {
                        dragComp = comp;

                        PointF detransformedMouse = DetransformPointF(e.Location);
                        dragOffset = new PointF(-detransformedMouse.X + comp.GetComponentPos().X, -detransformedMouse.Y + comp.GetComponentPos().Y);

                        dragResetPos = comp.GetComponentPos();
                        dragResetRot = comp.GetComponentRotation();

                        comp.Remove();

                        Invalidate();
                    }
                } else if (e.Button == MouseButtons.Right)
                {
                    dragType = DragType.Pan;

                    panLastMouseLocation = e.Location;
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButtons.Left && dragType == DragType.MoveComponent && !Simulating)
            {
                dragType = DragType.None;

                Point newPos = DetransformPoint(dragNewPoint);

                Rectangle newBounds = dragComp.GetComponentBounds();
                Matrix matrix = new Matrix();

                matrix.Rotate((float)dragNewRot);
                matrix.Translate(newPos.X, newPos.Y);

                Point[] corners = new Point[] { new Point(newBounds.Left, newBounds.Top), new Point(newBounds.Right, newBounds.Bottom) };
                matrix.TransformPoints(corners);

                newBounds = Rectangle.FromLTRB(corners[0].X, corners[0].Y, corners[1].X, corners[1].Y);

                if (board.CheckAllowed(newBounds))
                {
                    dragComp.Place(new Pos(newPos.X, newPos.Y), dragNewRot, board);
                } else
                {
                    dragComp.Place(dragResetPos, dragResetRot, board);
                }

                Invalidate();
            } else if (e.Button == MouseButtons.Right && dragType == DragType.Pan)
            {
                dragType = DragType.None;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (dragType == DragType.Pan)
            {
                PointF[] locs = new PointF[] { e.Location, panLastMouseLocation };
                DetransformPointFs(locs);

                transformationMatrix.Translate(locs[0].X - locs[1].X, locs[0].Y - locs[1].Y);

                panLastMouseLocation = e.Location;

                Invalidate();
            } else if (!Simulating && dragType == DragType.MoveComponent)
            {
                dragNewPoint = new Point((int)Math.Round(e.X + dragOffset.X), (int)Math.Round(e.Y + dragOffset.Y));
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (dragType == DragType.None)
            {
                float scale = (Math.Sign(e.Delta) * 0.1F) + 1;

                PointF loc = DetransformPointF(e.Location);

                transformationMatrix.Translate(loc.X, loc.Y);

                transformationMatrix.Scale(scale, scale);

                float[] elements = transformationMatrix.Elements;

                transformationMatrix.Scale(
                    Math.Min(elements[0], 100F) / elements[0],
                    Math.Min(elements[3], 100F) / elements[3]
                );

                transformationMatrix.Translate(-loc.X, -loc.Y);

                Invalidate();
            } else if (dragType == DragType.MoveComponent)
            {
                dragNewRot = (Rotation)(((Math.Sign(e.Delta) * 90) + ((int)dragNewRot)) % 360);
            }
        }
        //*/
    }
}
