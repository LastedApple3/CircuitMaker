﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using CircuitMaker.Basics;
using CircuitMaker.Components;
using CircuitMaker.GUI.Settings;
using CircuitMaker.GUI.ExtApp;

namespace CircuitMaker.GUI
{
    public partial class GUIForm : Form
    {
        public GUIForm()
        {
            InitializeComponent();

            UpdateFormText();
        }

        private void newBoardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            builder.OpenNewBoard("untitled");

            UpdateFormText();

            Invalidate();
        }

        private void openBoardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Boards (*.brd)|*.brd|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 0;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    builder.OpenLoadBoard(openFileDialog.FileName);
                    UpdateFormText();
                }
            }

            Invalidate();
        }

        private void saveBoardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            builder.SaveBoard();
        }

        private void saveBoardAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Boards (*.brd)|*.brd|All files (*.*)|*.*";
                saveFileDialog.FilterIndex = 0;

                saveFileDialog.FileName = builder.GetBoard().Name;
                saveFileDialog.DefaultExt = "brd";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string name = saveFileDialog.FileName.Split('\\').LastOrDefault();

                    if (name.EndsWith(".brd"))
                    {
                        name = name.Remove(name.Length - 4);
                    }

                    builder.GetBoard().Name = name;
                    UpdateFormText();

                    builder.SaveBoard(saveFileDialog.FileName);
                }
            }

            Invalidate();
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RenameBoardForm dialog = new RenameBoardForm(builder.GetBoard().Name);

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                builder.GetBoard().Name = dialog.NewBoardName();
                UpdateFormText();
            }
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            builder.CutSelection();
            Invalidate();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            builder.CopySelection();
            Invalidate();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            builder.PasteSelection();
            Invalidate();
        }

        private void editExternalAppearanceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExtAppEditorForm dialog = new ExtAppEditorForm(new BoardContainerComponents.BoardContainerComponent(builder.GetBoard(), false), builder.colourScheme);

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                dialog.ResetChanges();
            }
        }

        private void insertBuiltinComponentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                builder.CreateBuiltinComponent();
            }
            catch (PlacementException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void insertBoardComponentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                builder.CreateBoardComponent();
            } catch (PlacementException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSimulate_Click(object sender, EventArgs e)
        {
            builder.SetSimulation(!builder.Simulating);
        }

        private void BtnSimulate_UpdateText(bool simulating)
        {
            UpdateFormText();

            if (simulating)
            {
                btnSimulate.Text = "Stop Simulation";
            }
            else
            {
                btnSimulate.Text = "Start Simulation";
            }
        }

        public void UpdateFormText()
        {
            Text = $"{builder.GetBoard().Name} - {(builder.Simulating ? "Simulating" : "Editing")}";
        }

        private void Builder_SimulatingChange(bool simulating)
        {
            insertToolStripMenuItem.Enabled = !simulating;
            editToolStripMenuItem.Enabled = !simulating;

            //insertBoardComponentToolStripMenuItem.Enabled = !simulating;
            //insertBuiltinComponentToolStripMenuItem.Enabled = !simulating;
        }
    }

    public class Builder : UserControl
    {
        private enum DragType
        {
            None, MoveComponent, DrawWire
        }

        private struct Selection
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

            public void Remove()
            {
                if (SelectedObject.IsComp())
                {
                    SelectedObject.Comp().Remove();
                } else if (SelectedObject.IsWire())
                {
                    SelectedObject.Wire().Remove();
                }

                SelectedObject.Reset();
            }
        }

        private ContextMenuStrip genericMenu, componentMenu;
        private ToolStripLabel compNameMenuLabel;
        private ToolStripMenuItem startSimMenuItem, pasteCompMenuItem, createBuiltinCompMenuItem, createBoardCompMenuItem, openSettingsMenuItem, moveCompMenuItem, copyCompMenuItem, deleteCompMenuItem;
        private ToolStripSeparator compNameMenuSep, openSettingsMenuSep;

        private DragType dragType = DragType.None;

        private bool panning;
        private Point panLastMouseLocation;

        private Selection selection;

        private IComponent rightClickedComp;
        private Point rightClickMouseLoc;

        private IComponent clipboardComp;

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
        private string storedFilename;

        public ColourScheme colourScheme { get; private set; }

        public bool Simulating { get; private set; } = false;

        private int tps = 100;
        public int TPS
        {
            get
            {
                return tps;
            }
            set
            {
                tps = value;
                simulationTimer.Interval = (int)Math.Round((double)(1000 / tps));
            }
        }

        private Timer simulationTimer;

        public Board GetBoard()
        {
            return board;
        }

        private static string ConstructDefaultFilename(string boardName)
        {
            return $"Boards/{boardName}.brd";
        }

        public static Builder LoadBoard(string filename)
        {
            return new Builder(Board.Load(filename), filename);
        }

        public static Builder NewBoard(string name) {
            return new Builder(new Board(name));
        }

        public void OpenLoadBoard(string filename)
        {
            board = Board.Load(filename);
            storedFilename = filename;

            selection.Deselect();
            SetSimulation(false);

            Invalidate();
        }

        public void OpenNewBoard(string name)
        {
            board = new Board(name);
            storedFilename = null;

            selection.Deselect();
            SetSimulation(false);

            Invalidate();
        }

        public void SaveBoard(string filename = null)
        {
            storedFilename = filename ?? storedFilename ?? ConstructDefaultFilename(board.Name);
            board.Save(storedFilename);
        }

        private Builder(Board board, string filename = null)
        {
            storedFilename = filename;

            DoubleBuffered = true;

            clipboardComp = null;

            transformationMatrix = new Matrix();
            transformationMatrix.Scale(20, 20);

            colourScheme = new ColourScheme
            {
                Background = Color.White,
                ComponentEdge = Color.Black,
                ComponentBackground = Color.LightYellow,
                Wire = Color.Black,
                WireFloating = Color.Gray,
                WireLow = Color.SkyBlue,
                WirePulledLow = Color.LightSkyBlue,
                WireHigh = Color.Blue,
                WirePulledHigh = Color.LightBlue,
                WireIllegal = Color.Red,
                Grid = Color.FromArgb(63, Color.Black),
                Selection = Color.Red
            };

            simulationTimer = new Timer
            {
                Interval = (int)Math.Round((double)(1000 / tps)),
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

            createBuiltinCompMenuItem = new ToolStripMenuItem
            {
                Name = "tsmiCreateBuiltinComp",
                Text = "Create Builtin Component"
            };
            createBuiltinCompMenuItem.Click += CreateBuiltinCompMenuItem_Click;

            createBoardCompMenuItem = new ToolStripMenuItem
            {
                Name = "tsmiCreateBoardComp",
                Text = "Create Board Component"
            };
            createBoardCompMenuItem.Click += CreateBoardCompMenuItem_Click;

            genericMenu = new ContextMenuStrip
            {
                Name = "cmsGeneric"
            };
            genericMenu.Items.AddRange(new ToolStripItem[] { startSimMenuItem, pasteCompMenuItem, createBuiltinCompMenuItem, createBoardCompMenuItem });

            compNameMenuLabel = new ToolStripLabel
            {
                Name = "tslCompName"
            };

            compNameMenuSep = new ToolStripSeparator
            {
                Name = "tssCompName"
            };

            openSettingsMenuItem = new ToolStripMenuItem
            {
                Name = "tsmiOpenSettings",
                Text = "Open Settings"
            };
            openSettingsMenuItem.Click += OpenSettingsMenuItem_Click;

            openSettingsMenuSep = new ToolStripSeparator
            {
                Name = "tssOpenSettings"
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
            if (clipboardComp != null)
            {
                if (dragType == DragType.None)
                {
                    StartDraggingComponent(clipboardComp.Copy(), rightClickMouseLoc, true, true);

                    dragType = DragType.MoveComponent;
                }
            }
        }

        private void CreateBuiltinCompMenuItem_Click(object sender, EventArgs e)
        {
            CreateBuiltinComponent();
            Invalidate();
        }

        private void CreateBoardCompMenuItem_Click(object sender, EventArgs e)
        {
            CreateBoardComponent();
            Invalidate();
        }

        private void OpenSettingsMenuItem_Click(object sender, EventArgs e)
        {
            if (rightClickedComp is ISettingsComponent settingsComp) {
                settingsComp.OpenSettings();

                Invalidate();
            }
        }

        private void MoveCompMenuItem_Click(object sender, EventArgs e)
        {
            if (rightClickedComp != null)
            {
                StartDraggingComponent(rightClickedComp, rightClickMouseLoc, true);

                dragType = DragType.MoveComponent;
            }
        }

        private void CopyCompMenuItem_Click(object sender, EventArgs e)
        {
            if (rightClickedComp != null)
            {
                clipboardComp = rightClickedComp.Copy();
            }
        }

        private void DeleteCompMenuItem_Click(object sender, EventArgs e)
        {
            if (rightClickedComp != null)
            {
                rightClickedComp.Remove();

                Invalidate();
            }
        }

        public void CreateBuiltinComponent()
        {
            ComponentSelectionForm form = new ComponentSelectionForm();

            if (form.ShowDialog() == DialogResult.OK)
            {
                StartDraggingComponent(form.GetComponent(), new Point(), true, true);

                dragType = DragType.MoveComponent;
            }
        }

        public void CreateBoardComponent()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Boards (*.brd)|*.brd|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 0;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    Board intBoard = Board.Load(openFileDialog.FileName);

                    string[] overlap = intBoard.GetBoardList().Select(board => board.Name).Intersect(board.GetBoardList().Select(board => board.Name)).ToArray();

                    if (overlap.Count() > 0)
                    {
                        if (MessageBox.Show(
                            overlap.Prepend("The following boards are found within both the board you just selected and the board you are working on:").Aggregate((s1, s2) => s1 + "\n\t - " + s2) + "\nIf this might cause any issues, cancel now and double check. If this is intended, continue.",
                            "Overlap Detected",
                            MessageBoxButtons.OKCancel
                        ) == DialogResult.Cancel)
                        {
                            return;
                        }
                    }

                    StartDraggingComponent(new BoardContainerComponents.BoardContainerComponent(intBoard, false), new Point(), true, true);

                    dragType = DragType.MoveComponent;
                }
            }
        }

        public void CopySelection()
        {
            if (selection.HasComp())
            {
                clipboardComp = selection.SelectedComp.Copy();
            }
        }

        public void CutSelection()
        {
            if (selection.HasComp())
            {
                clipboardComp = selection.SelectedComp;
                clipboardComp.Remove();
            }
        }

        public void PasteSelection()
        {
            if (clipboardComp != null)
            {
                if (dragType == DragType.None)
                {
                    StartDraggingComponent(clipboardComp.Copy(), new Point(), true, true);

                    dragType = DragType.MoveComponent;
                }
            }
        }

        public void SetSimulation(bool simulate)
        {
            if (dragType == DragType.None)
            {
                Simulating = simulate;
                simulationTimer.Enabled = simulate;

                if (Simulating)
                {
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
            Dictionary<Pos, Pin.State> before = board.GetStateToCheckForChanges();

            board.Tick();

            Dictionary<Pos, Pin.State> after = board.GetStateToCheckForChanges();

            foreach (Pos pos in before.Keys)
            {
                if (!after.ContainsKey(pos) || before[pos] != after[pos])
                {
                    Invalidate();
                    break;
                }
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

                graphics.DrawRectangle(new Pen(colourScheme.Selection, 0.05F), bounds.X, bounds.Y, bounds.Width, bounds.Height);
            }

            Matrix matrix = new Matrix();

            if (dragType == DragType.MoveComponent)
            {
                using (new TransformRestorer(graphics))
                {
                    matrix.Reset();
                    Point newPoint = DetransformPoint(dragNewPoint);
                    matrix.Translate(newPoint.X, newPoint.Y);
                    matrix.Rotate((float)dragNewRot);

                    graphics.MultiplyTransform(matrix);

                    dragComp.Render(graphics, Simulating, colourScheme);
                }

                //matrix.Invert();
                //graphics.MultiplyTransform(matrix);
            }

            if (dragType == DragType.DrawWire)
            {
                graphics.DrawLine(new Pen(colourScheme.Wire, 0.05F), wireStart.ToPoint(), wireEnd.ToPoint());
            }

            graphics.ResetTransform();

            graphics.DrawRectangle(new Pen(colourScheme.Selection, 1), 0, 0, Width - 1, Height - 1);
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
                    if (e.KeyCode == Keys.Delete)
                    {
                        selection.Remove();

                        board.SimplifyWires();

                        Invalidate();
                    }

                    if (selection.HasComp())
                    {
                        if (e.KeyCode == (Keys.Control | Keys.C))
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


        private void StartDraggingComponent(IComponent comp, Point mouseLoc, bool offsetIsZero = false, bool resetIsDel = false)
        {
            dragComp = comp;

            dragResetIsDel = resetIsDel;

            dragResetPos = comp.GetComponentPos();
            dragResetRot = comp.GetComponentRotation();

            Point[] point = { dragResetPos.ToPoint() };

            transformationMatrix.TransformPoints(point);

            dragOffset = offsetIsZero ? new Point(0, 0) : new Point(point[0].X - mouseLoc.X, point[0].Y - mouseLoc.Y);
            dragNewRot = comp.GetComponentRotation();
            dragNewPoint = new Point(mouseLoc.X + dragOffset.X, mouseLoc.Y + dragOffset.Y);

            comp.Remove();

            Invalidate();
        }

        private void PutDownDraggedComponent()
        {
            Point newPoint = DetransformPoint(dragNewPoint);
            Pos newPos = Pos.FromPoint(newPoint);

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
            board.SimplifyWires();

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
                        if (wireStart != wireEnd)
                        {
                            new Wire(wireStart, wireEnd, board);
                        }

                        StartWire(wireEnd);
                    } else
                    {
                        IComponent clickedComp = GetClickedComponent(e.Location);
                        Wire clickedWire = GetClickedWire(e.Location);

                        Pos clickedPos = Pos.FromPoint(DetransformPoint(e.Location));

                        if (board.GetComponents().Length != 0 && board.GetComponents().Select(comp => comp.GetAllPinPositions()).Aggregate((posArr1, posArr2) => posArr1.Concat(posArr2).ToArray()).Contains(clickedPos))
                        {
                            StartWire(clickedPos);

                            dragType = DragType.DrawWire;
                        } else if (clickedComp != null)
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
                                StartWire(clickedPos);

                                dragType = DragType.DrawWire;

                                selection.Deselect();
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
                            contextMenuStrip.Items.Insert(1, openSettingsMenuSep);
                        } else
                        {
                            contextMenuStrip.Items.Remove(openSettingsMenuItem);
                            contextMenuStrip.Items.Remove(openSettingsMenuSep);
                        }

                        if (clickedComp is IBoardInterfaceComponent interfaceComp) 
                        {
                            compNameMenuLabel.Text = interfaceComp.GetComponentName();
                            contextMenuStrip.Items.Insert(0, compNameMenuLabel);
                            contextMenuStrip.Items.Insert(1, compNameMenuSep);
                        } else if (clickedComp is IBoardContainerComponent contComp)
                        {
                            compNameMenuLabel.Text = contComp.GetInternalBoard().Name;
                            contextMenuStrip.Items.Insert(0, compNameMenuLabel);
                            contextMenuStrip.Items.Insert(1, compNameMenuSep);
                        } else
                        {
                            contextMenuStrip.Items.Remove(compNameMenuLabel);
                            contextMenuStrip.Items.Remove(compNameMenuSep);
                        }
                    }
                    else
                    {
                        contextMenuStrip = genericMenu;
                    }

                    rightClickedComp = clickedComp;
                    rightClickMouseLoc = e.Location;

                    contextMenuStrip.Show(Cursor.Position);
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

            if (panning)
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
                dragNewRot = dragNewRot.AddRotation(e.Delta > 0 ? Rotation.CLOCKWISE : Rotation.ANTICLOCKWISE);

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
