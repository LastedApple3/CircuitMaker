﻿using System;
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
using CircuitMaker.GUI.Settings;

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
            None, /* Pan, */ MoveComponent, DrawWire
        }

        private struct Selection // at some point, allow for multiple selections, containing a list of ComponentOrWire instead of just one.
        {
            public struct ComponentOrWire
            {
                private IComponent comp;
                private Wire wire;

                public bool Exists() { return IsComp() || IsWire(); }

                public bool IsComp() { return comp != null; }

                public bool IsWire() { return wire != null; }

                public IComponent Comp() { return comp; }
                public Wire Wire() { return wire; }

                public void Set(IComponent comp) { this.comp = comp; wire = null; }
                public void Set(Wire wire) { comp = null; this.wire = wire; }
                public void Reset() { comp = null; wire = null; }
            }

            public ComponentOrWire SelectedObject;

            public IComponent SelectedComp { get { return SelectedObject.Comp(); } }
            public Wire SelectedWire { get { return SelectedObject.Wire(); } }

            public bool HasObject() { return SelectedObject.Exists(); }

            public bool HasComp() { return SelectedObject.IsComp(); }

            public bool HasWire() { return SelectedObject.IsWire(); }

            public void Deselect() { SelectedObject.Reset(); }

            public void Select(IComponent comp) { SelectedObject.Set(comp); }
            public void Select(Wire wire) { SelectedObject.Set(wire); }
        }

        private ContextMenuStrip genericMenu, componentMenu;
        private ToolStripMenuItem startSimMenuItem, pasteCompMenuItem, createCompMenuItem, openSettingsMenuItem, moveCompMenuItem, copyCompMenuItem, deleteCompMenuItem;
        private ToolStripSeparator componentMenuSep;

        private DragType dragType = DragType.None;

        private bool panning;
        private Point panLastMouseLocation;

        private Selection selection;

        private IComponent dragComp;
        private Point dragOffset;
        private bool dragResetIsDel;
        private Pos dragResetPos;
        private Rotation dragResetRot;
        private Point dragNewPoint;
        private Rotation dragNewRot;

        private Pos wireStart;
        private Pos wireEnd;

        protected Matrix transformationMatrix;

        private Board board;

        private ColourScheme colourScheme;

        public bool Simulating { get; private set; } = false;

        private Timer simulationTimer;

        public static Builder LoadBoard(string name)
        {
            return new Builder(Board.Load(name));
        }

        public static Builder NewBoard(string name) {
            return new Builder(new Board(name));
        }

        private Builder(Board board)
        {
            DoubleBuffered = true;

            transformationMatrix = new Matrix();
            transformationMatrix.Scale(20, 20);

            colourScheme = new ColourScheme
            {
                Background = Color.White,
                ComponentEdge = Color.Black,
                ComponentBackground = Color.LightYellow,
                Wire = Color.Black,
                WireFloating = Color.Gray,
                WireLow = Color.DarkBlue,
                WireHigh = Color.Blue,
                WireIllegal = Color.Red,
                Grid = Color.FromArgb(63, Color.Black)
            };

            simulationTimer = new Timer
            {
                Interval = (int)Math.Round((double)(1000 / 10)), // 10 t/s
                Enabled = false
            };
            simulationTimer.Tick += SimulationTick;

            this.board = board;

            startSimMenuItem = new ToolStripMenuItem
            {
                Name = "tsmiStartSim",
                Text = "Start Simulation"
            };
            startSimMenuItem.Click += StartSimMenuItem_Click;

            pasteCompMenuItem = new ToolStripMenuItem
            {
                Name = "tsmiPasteComp",
                Text = "Paste Component"
            };
            pasteCompMenuItem.Click += PasteCompMenuItem_Click;

            createCompMenuItem = new ToolStripMenuItem
            {
                Name = "tsmiCreateComp",
                Text = "Create Component"
            };
            createCompMenuItem.Click += CreateCompMenuItem_Click;

            genericMenu = new ContextMenuStrip
            {
                Name = "cmsGeneric"
            };
            genericMenu.Items.AddRange(new ToolStripItem[] { startSimMenuItem, pasteCompMenuItem, createCompMenuItem });

            openSettingsMenuItem = new ToolStripMenuItem
            {
                Name = "tsmiOpenSettings",
                Text = "Open Settings"
            };
            openSettingsMenuItem.Click += OpenSettingsMenuItem_Click;

            componentMenuSep = new ToolStripSeparator
            {
                Name = "tssCompMenu"
            };

            moveCompMenuItem = new ToolStripMenuItem
            {
                Name = "tsmiMoveComp",
                Text = "Move Component"
            };
            moveCompMenuItem.Click += MoveCompMenuItem_Click;

            copyCompMenuItem = new ToolStripMenuItem
            {
                Name = "tsmiCopyComp",
                Text = "Copy Component"
            };
            copyCompMenuItem.Click += CopyCompMenuItem_Click;

            deleteCompMenuItem = new ToolStripMenuItem
            {
                Name = "tsmiDeleteComp",
                Text = "Delete Component"
            };
            deleteCompMenuItem.Click += DeleteCompMenuItem_Click;

            componentMenu = new ContextMenuStrip
            {
                Name = "cmsSettingsComponent"
            };
            componentMenu.Items.AddRange(new ToolStripItem[] { moveCompMenuItem, copyCompMenuItem, deleteCompMenuItem });
        }

        private void StartSimMenuItem_Click(object sender, EventArgs e)
        {
            SetSimulation(true);
        }

        private void PasteCompMenuItem_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void CreateCompMenuItem_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OpenSettingsMenuItem_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void MoveCompMenuItem_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void CopyCompMenuItem_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void DeleteCompMenuItem_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public void SetSimulation(bool simulate)
        {
            if (dragType == DragType.None)
            {
                Simulating = simulate;
                simulationTimer.Enabled = simulate;

                if (Simulating)
                {
                    //selectedComp = null;
                    selection.Deselect();

                    board.ResetForSimulation();
                } else {
                    board.ResetToFloating();
                }

                SimulatingChange.Invoke(simulate);

                Invalidate();
            }
        }

        public event Action<bool> SimulatingChange;

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
                if (comp.GetOffsetComponentBounds().Contains(mousePos))
                {
                    return comp;
                }
            }

            return null;
        }

        private Wire GetClickedWire(Point mouseLoc)
        {
            PointF mousePos = DetransformPointF(mouseLoc);

            foreach (Wire wire in board.GetAllWires())
            {
                if (wire.InflatedBounds().Contains(mousePos))
                {
                    return wire;
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

            graphics.MultiplyTransform(transformationMatrix);

            Point[] corners = new Point[] { new Point(0, 0), new Point(Width - 1, Height - 1) };
            DetransformPoints(corners);

            board.Render(graphics, Simulating, Rectangle.FromLTRB(corners[0].X - 1, corners[0].Y - 1, corners[1].X + 1, corners[1].Y + 1), colourScheme);

            if (selection.HasObject())
            {
                RectangleF bounds = new RectangleF();
                
                if (selection.HasComp())
                {
                    bounds = selection.SelectedComp.GetOffsetComponentBounds();
                }
                else if (selection.HasWire())
                {
                    bounds = selection.SelectedWire.InflatedBounds();
                }

                graphics.DrawRectangle(new Pen(Color.Red, 0.05F), bounds.X, bounds.Y, bounds.Width, bounds.Height);
            }

            Matrix matrix = new Matrix();

            if (dragType == DragType.MoveComponent)
            {
                matrix.Reset();
                Point newPoint = DetransformPoint(dragNewPoint);
                matrix.Translate(newPoint.X, newPoint.Y);
                matrix.Rotate((float)dragNewRot);

                graphics.MultiplyTransform(matrix);

                dragComp.Render(graphics, Simulating, colourScheme);

                matrix.Invert();
                graphics.MultiplyTransform(matrix);
            }

            if (dragType == DragType.DrawWire)
            {
                graphics.DrawLine(new Pen(colourScheme.Wire, 0.05F), wireStart.ToPoint(), wireEnd.ToPoint());
            }

            graphics.ResetTransform();

            graphics.DrawRectangle(new Pen(Color.Red, 1), 0, 0, Width - 1, Height - 1);
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);

            Invalidate();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (!Simulating)
            {
                if (selection.HasObject())
                {
                    if (selection.HasComp()) // <---------------------------------------------------------------------- selected wire not dealt with
                    {
                        if (e.KeyCode == Keys.Delete)
                        {
                            selection.SelectedComp.Remove();
                            selection.Deselect();
                        }

                        else if (e.KeyCode == (Keys.Control | Keys.C))
                        {
                            if (dragType == DragType.None)
                            {
                                selection.Select(selection.SelectedComp.Copy());

                                dragType = DragType.MoveComponent;

                                StartDraggingComponent(selection.SelectedComp, DetransformPoint(MousePosition));
                            }
                        }
                    }
                } else
                {
                    if (e.KeyCode == Keys.Delete)
                    {
                        foreach (Wire wire in board.GetAllWires())
                        {
                            if (wire.Collision(Pos.FromPoint(DetransformPoint(MousePosition))))
                            {
                                wire.Remove();
                            }
                        }
                    }
                }
            }
        }


        private void StartDraggingComponent(IComponent comp, Point mouseLoc, bool resetIsDel = false)
        {
            dragComp = comp;

            dragResetIsDel = resetIsDel;

            dragResetPos = comp.GetComponentPos();
            dragResetRot = comp.GetComponentRotation();

            Point[] point = { /* new Point(dragResetPos.X, dragResetPos.Y) */ dragResetPos.ToPoint() };

            transformationMatrix.TransformPoints(point);

            dragOffset = new Point(point[0].X - mouseLoc.X, point[0].Y - mouseLoc.Y);
            dragNewRot = comp.GetComponentRotation();
            dragNewPoint = new Point(mouseLoc.X + dragOffset.X, mouseLoc.Y + dragOffset.Y);

            comp.Remove();

            Invalidate();
        }

        private void PutDownDraggedComponent()
        {
            Point newPoint = DetransformPoint(dragNewPoint);
            Pos newPos = Pos.FromPoint(newPoint); //new Pos(newPoint.X, newPoint.Y);

            Matrix matrix = new Matrix();

            matrix.Rotate((float)dragComp.GetComponentRotation());
            matrix.Translate(newPos.X, newPos.Y);

            RectangleF bounds = dragComp.GetComponentBounds();
            PointF[] corners = { new PointF(bounds.Left, bounds.Top), new PointF(bounds.Right, bounds.Bottom) };

            matrix.TransformPoints(corners);

            bounds = RectangleF.FromLTRB(corners[0].X, corners[0].Y, corners[1].X, corners[1].Y);

            if (board.CheckAllowed(bounds))
            {
                dragComp.Place(newPos, dragNewRot, board);

                dragComp = null;

                Invalidate();
            } else
            {
                ResetDraggedComponent();
            }
        }

        private void ResetDraggedComponent()
        {
            if (!dragResetIsDel)
            {
                dragComp.Place(dragResetPos, dragResetRot, board);
            }

            dragComp = null;

            Invalidate();
        }


        private void StartWire(Pos pos)
        {
            Rectangle bounds;

            HashSet<Wire> removeWires = new HashSet<Wire>();
            HashSet<(Pos, Pos)> addWires = new HashSet<(Pos, Pos)>();

            foreach (Wire wire in board.GetAllWires())
            {
                bounds = wire.Bounds();

                if (wire.Collision(pos))
                {
                    removeWires.Add(wire);

                    addWires.Add((wire.Pos1, pos));
                    addWires.Add((wire.Pos2, pos));
                }
            }

            foreach (Wire wire in removeWires)
            {
                wire.Remove();
            }

            foreach ((Pos, Pos) wire in addWires)
            {
                new Wire(wire.Item1, wire.Item2, board);
            }

            board.SimplifyWires();

            wireStart = pos;
        }


        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (e.Button == MouseButtons.Left)
            {
                if (Simulating)
                {
                    IComponent clickedComp = GetClickedComponent(e.Location);

                    if (clickedComp != null)
                    {
                        if (clickedComp is IInteractibleComponent interactComp)
                        {
                            interactComp.Interact();
                        }
                    }
                } else {
                    if (dragType == DragType.MoveComponent)
                    {
                        PutDownDraggedComponent();

                        dragType = DragType.None;
                    } else if (dragType == DragType.DrawWire)
                    {
                        if (!wireStart.Equals(wireEnd))
                        {
                            new Wire(wireStart, wireEnd, board);
                        }

                        StartWire(wireEnd);
                    } else
                    {
                        IComponent clickedComp = GetClickedComponent(e.Location);
                        Wire clickedWire = GetClickedWire(e.Location);

                        if (clickedComp != null)
                        {
                            if (clickedComp == selection.SelectedComp)
                            {
                                StartDraggingComponent(clickedComp, e.Location);

                                dragType = DragType.MoveComponent;
                            } else
                            {
                                selection.Select(clickedComp);
                            }
                        } else if (clickedWire != null)
                        {
                            if (clickedWire == selection.SelectedWire)
                            {
                                StartWire(Pos.FromPoint(DetransformPoint(e.Location)));

                                dragType = DragType.DrawWire;
                            } else
                            {
                                selection.Select(clickedWire);
                            }
                        } else
                        {
                            selection.Deselect();
                        }
                    }
                }
            } else if (e.Button == MouseButtons.Right)
            {
                if (!Simulating)
                {
                    IComponent clickedComp = GetClickedComponent(e.Location);

                    ContextMenuStrip contextMenuStrip;

                    if (clickedComp != null)
                    {
                        selection.Select(clickedComp);

                        contextMenuStrip = componentMenu;

                        if (clickedComp is ISettingsComponent)
                        {
                            contextMenuStrip.Items.Insert(0, openSettingsMenuItem);
                            contextMenuStrip.Items.Insert(1, componentMenuSep);
                        } else
                        {
                            contextMenuStrip.Items.Remove(openSettingsMenuItem);
                            contextMenuStrip.Items.Remove(componentMenuSep);
                        }
                    }
                    else
                    {
                        contextMenuStrip = genericMenu;
                    }

                    contextMenuStrip.Show(Cursor.Position);

                    //contextMenuStrip.Items.Remove(openSettingsMenuItem);
                    //contextMenuStrip.Items.Remove(componentMenuSep);
                }
            }

            Invalidate();
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            if (e.Button == MouseButtons.Left)
            {
                if (!Simulating)
                {
                    if (dragType == DragType.MoveComponent)
                    {
                        ResetDraggedComponent();

                        dragType = DragType.None;
                    }

                    if (dragType == DragType.DrawWire)
                    {
                        dragType = DragType.None;
                    } else
                    {
                        IComponent clickedComp = GetClickedComponent(e.Location);

                        if (clickedComp != null && clickedComp == selection.SelectedComp)
                        {
                            if (clickedComp is ISettingsComponent settingsComp)
                            {
                                settingsComp.OpenSettings();
                            }
                            else
                            {
                                StartDraggingComponent(clickedComp, e.Location);

                                dragType = DragType.MoveComponent;
                            }
                        }
                        else
                        {
                            selection.Select(clickedComp);
                        }
                    }
                }
            }

            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Middle)
            {
                if (!panning)
                {
                    panning = true;

                    panLastMouseLocation = e.Location;
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button == MouseButtons.Middle)
            {
                if (panning)
                {
                    panning = false;
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (panning /* dragType == DragType.Pan */)
            {
                PointF[] locs = new PointF[] { e.Location, panLastMouseLocation };
                DetransformPointFs(locs);

                transformationMatrix.Translate(locs[0].X - locs[1].X, locs[0].Y - locs[1].Y);

                panLastMouseLocation = e.Location;

                Invalidate();
            }

            if (dragType == DragType.MoveComponent)
            {
                dragNewPoint = new Point(e.Location.X + dragOffset.X, e.Location.Y + dragOffset.Y);
                //Console.WriteLine(dragNewPoint);

                Invalidate();
            }

            if (dragType == DragType.DrawWire)
            {
                wireEnd = Pos.FromPoint(DetransformPoint(e.Location));

                if (wireStart.X != wireEnd.X && wireStart.Y != wireEnd.Y)
                {
                    if (Math.Abs(wireEnd.X - wireStart.X) > Math.Abs(wireEnd.Y - wireStart.Y))
                    {
                        wireEnd = new Pos(wireEnd.X, wireStart.Y);
                    } else
                    {
                        wireEnd = new Pos(wireStart.X, wireEnd.Y);
                    }
                }

                Invalidate();
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (dragType == DragType.MoveComponent) 
            {
                dragNewRot += (90 * Math.Sign(e.Delta));

                Invalidate();
            } else 
            {
                float scale = (Math.Sign(e.Delta) * 0.1F) + 1;

                PointF loc = DetransformPointF(e.Location);

                transformationMatrix.Translate(loc.X, loc.Y);

                transformationMatrix.Scale(scale, scale);

                float[] elements = transformationMatrix.Elements;

                transformationMatrix.Scale(
                    Math.Max(Math.Min(elements[0], 100F), 10F) / elements[0],
                    Math.Max(Math.Min(elements[3], 100F), 10F) / elements[3]
                );

                transformationMatrix.Translate(-loc.X, -loc.Y);

                Invalidate();
            }
        }
    }
}
