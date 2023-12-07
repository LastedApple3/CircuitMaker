using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CircuitMaker.Basics;

namespace CircuitMaker.GUI.ExtApp
{
    public partial class ExtAppEditor : UserControl
    {
        private IBoardContainerComponent boardContainerComp;
        public ColourScheme colourScheme;

        private Dictionary<string, Board.InterfaceLocation> interfaceLocSave;
        private Dictionary<IGraphicalComponent, PointF?> graphicalLocSave;

        private int scale = 40;

        private Matrix transformationMatrix;

        public ExtAppEditor(IBoardContainerComponent boardContainerComp, ColourScheme colourScheme)
        {
            InitializeComponent();

            transformationMatrix = new Matrix();

            RectangleF compRect = boardContainerComp.GetShape();
            transformationMatrix.Scale(scale, scale);
            transformationMatrix.Translate(-compRect.X, -compRect.Y);
            transformationMatrix.Translate(1, 1);

            this.colourScheme = colourScheme;
            this.boardContainerComp = boardContainerComp;

            ResetSize();

            SaveChanges();
        }

        private void ResetSize()
        {
            Size = boardContainerComp.GetInternalBoard().ExternalSize;
            Width += 2;
            Height += 2;
            Width *= scale;
            Height *= scale;
        }

        public void SaveChanges()
        {
            Board internalBoard = boardContainerComp.GetInternalBoard();

            interfaceLocSave = new Dictionary<string, Board.InterfaceLocation>();

            foreach (IBoardInterfaceComponent interfaceComp in internalBoard.GetInterfaceComponents())
            {
                interfaceLocSave.Add(interfaceComp.GetComponentName(), interfaceComp.GetInterfaceLocation());
            }

            graphicalLocSave = new Dictionary<IGraphicalComponent, PointF?>();

            foreach (IGraphicalComponent graphicalComp in internalBoard.GetGraphicalComponents())
            {
                graphicalLocSave.Add(graphicalComp, graphicalComp.GetGraphicalElementLocation());
            }
        }

        public void ResetChanges()
        {
            Board internalBoard = boardContainerComp.GetInternalBoard();

            foreach (string compName in interfaceLocSave.Keys)
            {
                internalBoard.GetInterfaceComponent(compName).SetInterfaceLocation(interfaceLocSave[compName]);
            }

            foreach (IGraphicalComponent graphicalComp in graphicalLocSave.Keys)
            {
                graphicalComp.SetGraphicalElementLocation(graphicalLocSave[graphicalComp]);
            }
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

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics graphics = e.Graphics;

            graphics.ResetTransform();

            graphics.Clear(Color.White);

            Rectangle clipRect = e.ClipRectangle;
            clipRect.Width -= 1;
            clipRect.Height -= 1;
            graphics.DrawRectangle(new Pen(Brushes.Black, 0.5F), clipRect);

            graphics.MultiplyTransform(transformationMatrix);

            boardContainerComp.Render(graphics, false, colourScheme);

            Matrix inverseTransformationMatrix = transformationMatrix.Clone();
            inverseTransformationMatrix.Invert();
            graphics.MultiplyTransform(inverseTransformationMatrix);
        }

        private IGraphicalComponent[] GetGraphicalComponents()
        {
            return boardContainerComp.GetInternalBoard().GetGraphicalComponents();
        }

        private IBoardInterfaceComponent[] GetInterfaceComponents()
        {
            return boardContainerComp.GetInternalBoard().GetInterfaceComponents();
        }

        private PointF PositionNewGraphicalElement(RectangleF bounds)
        {
            return new PointF();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            bool onLeft = e.Location.X < scale, onRight = e.Location.X > Size.Width - scale,
                onTop = e.Location.Y < scale, onBottom = e.Location.Y > Size.Height - scale,
                onLeftRight = onLeft || onRight, onTopBottom = onTop || onBottom;

            Console.WriteLine($"{e.Location}, {new PointF(scale, scale)}, {new PointF(Size.Width - scale, Size.Height - scale)}");
            Console.WriteLine($"onLeft: {onLeft}, onRight: {onRight}, onTop: {onTop}, onBottom: {onBottom}");
            Console.WriteLine(!(onLeftRight || onTopBottom));

            if (!(onLeftRight || onTopBottom))
            {
                RectangleF? possibleBounds;
                RectangleF bounds;

                foreach (IGraphicalComponent graphicalComp in GetGraphicalComponents())
                {
                    possibleBounds = graphicalComp.GetOffsetGraphicalElementBounds();

                    if (possibleBounds.HasValue)
                    {
                        bounds = possibleBounds.Value;
                    } else
                    {
                        graphicalComp.SetGraphicalElementLocation(PositionNewGraphicalElement(graphicalComp.GetGraphicalElementBounds()));

                        Invalidate();

                        bounds = graphicalComp.GetOffsetGraphicalElementBounds().Value;
                    }

                    Console.WriteLine($"{bounds.Location}, {new PointF(bounds.X + bounds.Width, bounds.Y + bounds.Height)}");

                    if (bounds.Contains(DetransformPointF(e.Location)))
                    {
                        Console.WriteLine("clicked on graphical element");
                    }
                }
            } else if (onLeftRight ^ onTopBottom)
            {
                /*
                Board.InterfaceLocation.SideEnum side = 0b000;

                if (!(onTop || onBottom))
                {
                    if (onLeft)
                    {
                        side = Board.InterfaceLocation.SideEnum.Left;
                    }
                    else if (onRight)
                    {
                        side = Board.InterfaceLocation.SideEnum.Right;
                    }
                }
                else if (!(onLeft || onRight))
                {
                    if (onTop)
                    {
                        side = Board.InterfaceLocation.SideEnum.Top;
                    }
                    else if (onBottom)
                    {
                        side = Board.InterfaceLocation.SideEnum.Bottom;
                    }
                }
                //*/

                Board.InterfaceLocation.SideEnum side = Board.InterfaceLocation.SideEnum.IsSide |
                    (onLeftRight ? Board.InterfaceLocation.SideEnum.LeftRight : Board.InterfaceLocation.SideEnum.Nothing) |
                    ((onBottom || onRight) ? Board.InterfaceLocation.SideEnum.BottomRight : Board.InterfaceLocation.SideEnum.Nothing);

                foreach (IBoardInterfaceComponent interfaceComp in GetInterfaceComponents().Where(interfaceComp => interfaceComp.GetInterfaceLocation().Side == side))
                {

                }
            }

            Console.WriteLine();
        }
    }
}
