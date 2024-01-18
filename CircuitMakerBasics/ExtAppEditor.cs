﻿using System;
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
        private class DragState
        {
            private bool isInterfaceLoc;
            private bool isGraphicalComp;
            private bool isSize;

            private string interfaceLocName;
            private IGraphicalComponent graphicalComp;

            private Board.InterfaceLocation interfaceLocReset;
            private PointF? graphicalCompReset;
            private Size sizeReset;

            public DragState()
            {
                Reset();
            }

            public bool IsAnything() { return isInterfaceLoc || isGraphicalComp || isSize; }

            public bool IsInterfaceLoc() { return isInterfaceLoc; }
            public bool IsGraphicalComp() { return isGraphicalComp; }
            public bool IsSize() { return isSize; }

            public string GetInterfaceLocName() { return interfaceLocName; }
            public IGraphicalComponent GetGraphicalComp() { return graphicalComp; }

            public Board.InterfaceLocation GetInterfaceLocReset() { return interfaceLocReset; }
            public PointF? GetGraphicalCompReset() { return graphicalCompReset; }
            public Size GetSizeReset() { return sizeReset; }

            public void SelectInterfaceLoc(string name, Board.InterfaceLocation reset) { Reset(); isInterfaceLoc = true; interfaceLocName = name; interfaceLocReset = reset; }
            public void SelectGraphicalComp(IGraphicalComponent comp, PointF? reset) { Reset(); isGraphicalComp = true; graphicalComp = comp; graphicalCompReset = reset; }
            public void SelectSize(Size reset) { Reset(); isSize = true; sizeReset = reset; }

            public void Reset() { isInterfaceLoc = false; isGraphicalComp = false; isSize = false; /* interfaceLocName = null; graphicalComp = null; interfaceLocReset = null; graphicalCompReset = null; */ }
        }

        private IBoardContainerComponent boardContainerComp;
        public ColourScheme colourScheme;

        private Dictionary<string, Board.InterfaceLocation> interfaceLocSave;
        private Dictionary<IGraphicalComponent, Point?> graphicalLocSave;
        private Size sizeSave;

        private int scale = 40;
        private int resizeStartRange = 10;

        private Matrix compTransformationMatrix = new Matrix();
        private Matrix graphicalsTransformationMatrix = new Matrix();

        private DragState dragState;

        private Point mouseDragLoc;

        Rectangle compDisplayBounds;
        Rectangle graphicalsDisplayBounds;

        public ExtAppEditor(IBoardContainerComponent boardContainerComp, ColourScheme colourScheme)
        {
            DoubleBuffered = true;

            InitializeComponent();

            dragState = new DragState();

            this.colourScheme = colourScheme;
            this.boardContainerComp = boardContainerComp;

            ResetSize();

            SaveChanges();
        }

        private void ResetSize()
        {
            //boardContainerComp.ResetShape();

            Size size = boardContainerComp.GetInternalBoard().ExternalSize;
            size.Width += 2;
            size.Height += 2;

            float graphicalsStart = size.Width;

            size.Width *= scale;
            size.Height *= scale;

            compDisplayBounds = new Rectangle(Point.Empty, size);

            size.Width += scale / 2;

            graphicalsStart += 0.5F;

            int graphicalsWidth = GetUnplacedGraphicalComponents().Select(comp => (int)Math.Ceiling(comp.GetGraphicalElementBounds().Width)).Append(scale).Aggregate(Math.Max);

            graphicalsDisplayBounds = new Rectangle(size.Width, 0, graphicalsWidth, size.Height);

            size.Width += graphicalsWidth;

            Size = size;

            Rectangle compRect = boardContainerComp.GetShape();
            compTransformationMatrix.Reset();
            compTransformationMatrix.Scale(scale, scale);
            compTransformationMatrix.Translate(-compRect.X, -compRect.Y);
            compTransformationMatrix.Translate(1, 1);

            graphicalsTransformationMatrix.Reset();
            graphicalsTransformationMatrix.Scale(scale, scale);
            graphicalsTransformationMatrix.Translate(graphicalsStart /*boardContainerComp.GetInternalBoard().ExternalSize.Width + 2.5F*/ /*graphicalsDisplayBounds.X*/ , 0);
        }

        public void SaveChanges()
        {
            Board internalBoard = boardContainerComp.GetInternalBoard();

            interfaceLocSave = new Dictionary<string, Board.InterfaceLocation>();

            foreach (IBoardInterfaceComponent interfaceComp in internalBoard.GetInterfaceComponents())
            {
                interfaceLocSave.Add(interfaceComp.GetComponentName(), interfaceComp.GetInterfaceLocation());
            }

            graphicalLocSave = new Dictionary<IGraphicalComponent, Point?>();

            foreach (IGraphicalComponent graphicalComp in internalBoard.GetGraphicalComponents())
            {
                graphicalLocSave.Add(graphicalComp, graphicalComp.GetGraphicalElementLocation());
            }

            sizeSave = internalBoard.ExternalSize;
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

            internalBoard.ExternalSize = sizeSave;
        }

        private Matrix GetInvertedCompTransformationMatrix()
        {
            Matrix invertedMatrix = compTransformationMatrix.Clone();
            invertedMatrix.Invert();
            return invertedMatrix;
        }

        private Point CompDetransformPoint(Point point)
        {
            Point[] points = new Point[] { point };
            CompDetransformPoints(points);
            return points[0];
        }

        private PointF CompDetransformPointF(PointF point)
        {
            PointF[] points = new PointF[] { point };
            CompDetransformPointFs(points);
            return points[0];
        }

        private void CompDetransformPoints(Point[] points)
        {
            GetInvertedCompTransformationMatrix().TransformPoints(points);
        }

        private void CompDetransformPointFs(PointF[] points)
        {
            GetInvertedCompTransformationMatrix().TransformPoints(points);
        }

        private IGraphicalComponent DetectUnplacedGraphicalClick(Point point)
        {
            int offset = 0;
            Matrix matrix = new Matrix();
            RectangleF boundsF;
            Rectangle bounds;
            PointF[] points;

            foreach (IGraphicalComponent graphicalComp in GetUnplacedGraphicalComponents())
            {
                matrix.Reset();

                matrix.Translate(0, offset);

                boundsF = graphicalComp.GetGraphicalElementBounds();
                bounds = new Rectangle((int)Math.Floor(boundsF.X * scale), (int)Math.Floor(boundsF.Y * scale), (int)Math.Ceiling(boundsF.Width * scale), (int)Math.Ceiling(boundsF.Height * scale));

                //Console.WriteLine(bounds);

                offset += bounds.Height;

                matrix.Translate(-bounds.X, -bounds.Y);
                matrix.Multiply(graphicalsTransformationMatrix);
                matrix.Invert();

                points = new PointF[] { point };

                matrix.TransformPoints(points);

                Console.WriteLine(points[0]);

                if (((RectangleF)bounds).Contains(points[0]))
                {
                    return graphicalComp;
                }
            }

            return null;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics graphics = e.Graphics;

            graphics.ResetTransform();

            graphics.Clear(DefaultBackColor);
            //graphics.Clear(Color.White);

            Rectangle compRect = compDisplayBounds;
            compRect.Width -= 1;
            compRect.Height -= 1;
            graphics.FillRectangle(new SolidBrush(Color.White), compRect);
            graphics.DrawRectangle(new Pen(Brushes.Black, 0.5F), compRect);

            Rectangle graphicalsRect = graphicalsDisplayBounds;
            graphicalsRect.Width -= 1;
            graphicalsRect.Height -= 1;
            graphics.FillRectangle(new SolidBrush(Color.White), graphicalsRect);
            graphics.DrawRectangle(new Pen(Brushes.Black, 0.5F), graphicalsRect);

            graphics.MultiplyTransform(compTransformationMatrix);

            boardContainerComp.Render(graphics, false, colourScheme);

            graphics.ResetTransform();
            //graphics.MultiplyTransform(GetInvertedCompTransformationMatrix());

            graphics.MultiplyTransform(graphicalsTransformationMatrix);

            //int rad = 1; //194;
            //graphics.FillEllipse(new SolidBrush(Color.Orange), -rad, -rad, 2 * rad, 2 * rad);

            int offset = 0;
            Matrix matrix = new Matrix();
            RectangleF boundsF;
            Rectangle bounds;

            foreach (IGraphicalComponent graphicalComp in GetUnplacedGraphicalComponents())
            {
                //Console.WriteLine(graphicalComp.GetComponentID());

                matrix.Reset();

                matrix.Translate(0, offset);

                boundsF = graphicalComp.GetGraphicalElementBounds();
                bounds = new Rectangle((int)Math.Floor(boundsF.X * scale), (int)Math.Floor(boundsF.Y * scale), (int)Math.Ceiling(boundsF.Width * scale), (int)Math.Ceiling(boundsF.Height * scale));

                //Console.WriteLine(bounds);

                offset += bounds.Height;

                matrix.Translate(-bounds.X, -bounds.Y);

                graphics.MultiplyTransform(matrix, MatrixOrder.Append);

                graphicalComp.RenderGraphicalElement(graphics, false, colourScheme);

                matrix.Invert();
                graphics.MultiplyTransform(matrix, MatrixOrder.Append);
            }
        }

        private IGraphicalComponent[] GetGraphicalComponents()
        {
            return boardContainerComp.GetInternalBoard().GetGraphicalComponents();
        }

        private IGraphicalComponent[] GetUnplacedGraphicalComponents()
        {
            return GetGraphicalComponents().Where(comp => !comp.GetGraphicalElementLocation().HasValue).ToArray();
        }

        private IBoardInterfaceComponent[] GetInterfaceComponents()
        {
            return boardContainerComp.GetInternalBoard().GetInterfaceComponents();
        }

        private IBoardInterfaceComponent GetInterfaceComponent(string name)
        {
            return boardContainerComp.GetInternalBoard().GetInterfaceComponent(name);
        }

        private IBoardInterfaceComponent GetInterfaceComponent(Board.InterfaceLocation loc)
        {
            IBoardInterfaceComponent[] comps = GetInterfaceComponents().Where(comp => comp.GetInterfaceLocation() == loc).ToArray();

            if (comps.Length == 0)
            {
                return null;
            }

            return comps[0];
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            Point farCorner = new Point(compDisplayBounds.Width - scale, compDisplayBounds.Height - scale);

            if (e.Location.X > compDisplayBounds.Width)
            {
                Console.WriteLine(DetectUnplacedGraphicalClick(e.Location).GetComponentID());
            } else if (e.Location.X > farCorner.X - resizeStartRange && e.Location.X < farCorner.X + resizeStartRange && e.Location.Y > farCorner.Y - resizeStartRange && e.Location.Y < farCorner.Y + resizeStartRange)
            {
                dragState.SelectSize(boardContainerComp.GetInternalBoard().ExternalSize);
            } else
            {
                bool onLeft = e.Location.X < scale, onRight = e.Location.X > farCorner.X,
                    onTop = e.Location.Y < scale, onBottom = e.Location.Y > farCorner.Y,
                    onLeftRight = onLeft || onRight, onTopBottom = onTop || onBottom;

                if (!(onLeftRight || onTopBottom))
                {
                    RectangleF? bounds;

                    foreach (IGraphicalComponent graphicalComp in GetGraphicalComponents())
                    {
                        bounds = graphicalComp.GetOffsetGraphicalElementBounds();

                        if (bounds.HasValue)
                        {
                            Console.WriteLine(bounds.Value);
                            Console.WriteLine(e.Location);

                            if (true) // test collision
                            {
                                dragState.SelectGraphicalComp(graphicalComp, graphicalComp.GetGraphicalElementLocation());

                                break;
                            }
                        }
                    }
                }
                else if (onLeftRight ^ onTopBottom)
                {
                    Board.InterfaceLocation.SideEnum side = Board.InterfaceLocation.SideEnum.IsSide |
                        (onLeftRight ? Board.InterfaceLocation.SideEnum.LeftRight : Board.InterfaceLocation.SideEnum.Nothing) |
                        ((onBottom || onRight) ? Board.InterfaceLocation.SideEnum.BottomRight : Board.InterfaceLocation.SideEnum.Nothing);

                    Point clickedPoint = CompDetransformPoint(e.Location);
                    Point offset = boardContainerComp.GetShape().Location;

                    foreach (IBoardInterfaceComponent interfaceComp in GetInterfaceComponents().Where(interfaceComp => interfaceComp.GetInterfaceLocation().Side == side))
                    {
                        if ((onLeftRight ? clickedPoint.Y - offset.Y : clickedPoint.X - offset.X) == interfaceComp.GetInterfaceLocation().Distance)
                        {
                            dragState.SelectInterfaceLoc(interfaceComp.GetComponentName(), interfaceComp.GetInterfaceLocation());
                        }
                    }
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (dragState.IsAnything())
            {
                dragState.Reset();

                //Invalidate();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (dragState.IsAnything())
            {
                mouseDragLoc = e.Location;

                Point mousePoint = CompDetransformPoint(mouseDragLoc);
                Board internalBoard = boardContainerComp.GetInternalBoard();

                //Console.WriteLine(mousePoint);
                //Console.WriteLine(boardContainerComp.GetShape());
                //Console.WriteLine();

                if (dragState.IsSize())
                {
                    float[] bounds = new float[] { 1, 1 };

                    int idx;

                    foreach (IBoardInterfaceComponent comp in GetInterfaceComponents())
                    {
                        idx = comp.GetInterfaceLocation().Side.IsLeftRight() ? 1 : 0;

                        bounds[idx] = Math.Max(comp.GetInterfaceLocation().Distance + 1, bounds[idx]);
                    }

                    /*
                    IBoardInterfaceComponent[] interfaceComps = GetInterfaceComponents();
                    Func<Board.InterfaceLocation.SideEnum, int> interfacesOnSide = side => interfaceComps.Where(comp => comp.GetInterfaceLocation().Side == side).Count();

                    float[] bounds = new float[] {
                        Math.Max(1, Math.Max(interfacesOnSide(Board.InterfaceLocation.SideEnum.Top), interfacesOnSide(Board.InterfaceLocation.SideEnum.Bottom)) + 1),
                        Math.Max(1, Math.Max(interfacesOnSide(Board.InterfaceLocation.SideEnum.Left), interfacesOnSide(Board.InterfaceLocation.SideEnum.Right)) + 1)
                    };
                    //*/

                    RectangleF? possCompBounds;
                    RectangleF compBounds;

                    foreach (IGraphicalComponent graphicalComp in internalBoard.GetGraphicalComponents())
                    {
                        possCompBounds = graphicalComp.GetOffsetGraphicalElementBounds();

                        if (possCompBounds.HasValue)
                        {
                            compBounds = possCompBounds.Value;

                            bounds = new float[]
                            {
                               Math.Max(bounds[0], compBounds.Left),
                               Math.Max(bounds[1], compBounds.Bottom),
                            };
                        }
                    }

                    Rectangle shape = boardContainerComp.GetShape();

                    internalBoard.ExternalSize = new Size(Math.Max(mousePoint.X - shape.X, (int)Math.Ceiling(bounds[0])), Math.Max(mousePoint.Y - shape.Y, (int)Math.Ceiling(bounds[1])));

                    ResetSize();

                    //Console.WriteLine(internalBoard.ExternalSize);
                }
                else if (dragState.IsInterfaceLoc())
                {
                    Rectangle shape = boardContainerComp.GetShape();

                    Point relToCorner = new Point(mousePoint.X - shape.X, mousePoint.Y - shape.Y);

                    Dictionary<Board.InterfaceLocation.SideEnum, int> dists = new Dictionary<Board.InterfaceLocation.SideEnum, int> {
                        { Board.InterfaceLocation.SideEnum.Left, relToCorner.X },
                        { Board.InterfaceLocation.SideEnum.Top, relToCorner.Y },
                        { Board.InterfaceLocation.SideEnum.Right, shape.Width - relToCorner.X },
                        { Board.InterfaceLocation.SideEnum.Bottom, shape.Height - relToCorner.Y }
                    };

                    Board.InterfaceLocation.SideEnum closestSide = dists.Aggregate((kvp1, kvp2) => kvp1.Value < kvp2.Value ? kvp1 : kvp2).Key;
                    int closestDist = Math.Max(1, closestSide.IsLeftRight() ? Math.Min(relToCorner.Y, shape.Height - 1) : Math.Min(relToCorner.X, shape.Width - 1));

                    Board.InterfaceLocation.SideEnum actualSide = closestSide;
                    int actualDist = closestDist;

                    int distProg = 0, sideProg = 0;

                    bool lastOutside = false, thisOutside;

                    while (lastOutside || 
                           !(GetInterfaceComponent(new Board.InterfaceLocation(actualSide, actualDist)) == null ||
                             GetInterfaceComponent(new Board.InterfaceLocation(actualSide, actualDist)).GetComponentName() == dragState.GetInterfaceLocName()))
                    {
                        actualDist += distProg * ((2 * (distProg % 2)) - 1);

                        distProg++;

                        thisOutside = actualDist <= 0 || actualDist >= (actualSide.IsLeftRight() ? shape.Height : shape.Width);

                        if (thisOutside && lastOutside)
                        {
                            if (sideProg == 0 || sideProg == 2)
                            {
                                actualSide ^= Board.InterfaceLocation.SideEnum.LeftRight;
                            } else if (sideProg == 1)
                            {
                                actualSide ^= Board.InterfaceLocation.SideEnum.BottomRight;
                            } else if (sideProg == 3)
                            {
                                throw new Exception("no space");
                            }

                            if (actualSide.IsLeftRight() == closestSide.IsLeftRight())
                            {
                                actualDist = closestDist;
                            } else if (closestSide.IsBottomRight())
                            {
                                actualDist = (actualSide.IsLeftRight() ? shape.Height : shape.Width) - 1;
                            } else
                            {
                                actualDist = 1;
                            }

                            distProg = 0;
                            sideProg++;
                        }

                        lastOutside = thisOutside;
                    }
                    
                    GetInterfaceComponent(dragState.GetInterfaceLocName()).SetInterfaceLocation(new Board.InterfaceLocation(actualSide, actualDist));

                } else if (dragState.IsGraphicalComp())
                {

                }

                Invalidate();
            }
        }
    }
}
