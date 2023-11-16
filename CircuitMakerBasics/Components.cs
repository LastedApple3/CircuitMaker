using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CircuitMaker.Basics;
using CircuitMaker.GUI.Settings;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

namespace CircuitMaker.Components
{
    public static class ComponentRegisterer
    {
        public static void RegisterComponents()
        {
            ReadWriteImplementation.Constructors.Add(VarInpComponents.VarInpAndComponent.ID, VarInpComponents.VarInpAndComponent.Constructor);
            ReadWriteImplementation.Constructors.Add(VarInpComponents.VarInpOrComponent.ID, VarInpComponents.VarInpOrComponent.Constructor);
            ReadWriteImplementation.Constructors.Add(VarInpComponents.VarInpXorComponent.ID, VarInpComponents.VarInpXorComponent.Constructor);
            ReadWriteImplementation.Constructors.Add(VarInpComponents.VarInpNandComponent.ID, VarInpComponents.VarInpNandComponent.Constructor);
            ReadWriteImplementation.Constructors.Add(VarInpComponents.VarInpNorComponent.ID, VarInpComponents.VarInpNorComponent.Constructor);
            ReadWriteImplementation.Constructors.Add(VarInpComponents.VarInpXnorComponent.ID, VarInpComponents.VarInpXnorComponent.Constructor);

            ReadWriteImplementation.Constructors.Add(FixedStateComponent.ID, FixedStateComponent.Constructor);
            ReadWriteImplementation.Constructors.Add(UserToggleInpComponent.ID, UserToggleInpComponent.Constructor);

            ReadWriteImplementation.Constructors.Add(LogicProbeComponent.ID, LogicProbeComponent.Constructor);

            ReadWriteImplementation.Constructors.Add(BoardContainerComponents.BoardInputComponent.ID, BoardContainerComponents.BoardInputComponent.Constructor);
            ReadWriteImplementation.Constructors.Add(BoardContainerComponents.BoardOutputComponent.ID, BoardContainerComponents.BoardOutputComponent.Constructor);
            ReadWriteImplementation.Constructors.Add(BoardContainerComponents.BoardBidirComponent.ID, BoardContainerComponents.BoardBidirComponent.Constructor);
            ReadWriteImplementation.Constructors.Add(BoardContainerComponents.BoardContainerComponent.ID, BoardContainerComponents.BoardContainerComponent.Constructor);


            ReadWriteImplementation.DefaultDetails.Add(VarInpComponents.VarInpAndComponent.ID, VarInpComponents.VarInpAndComponent.DefaultDetails);
            ReadWriteImplementation.DefaultDetails.Add(VarInpComponents.VarInpOrComponent.ID, VarInpComponents.VarInpOrComponent.DefaultDetails);
            ReadWriteImplementation.DefaultDetails.Add(VarInpComponents.VarInpXorComponent.ID, VarInpComponents.VarInpXorComponent.DefaultDetails);
            ReadWriteImplementation.DefaultDetails.Add(VarInpComponents.VarInpNandComponent.ID, VarInpComponents.VarInpNandComponent.DefaultDetails);
            ReadWriteImplementation.DefaultDetails.Add(VarInpComponents.VarInpNorComponent.ID, VarInpComponents.VarInpNorComponent.DefaultDetails);
            ReadWriteImplementation.DefaultDetails.Add(VarInpComponents.VarInpXnorComponent.ID, VarInpComponents.VarInpXnorComponent.DefaultDetails);

            ReadWriteImplementation.DefaultDetails.Add(FixedStateComponent.ID, FixedStateComponent.DefaultDetails);
            ReadWriteImplementation.DefaultDetails.Add(UserToggleInpComponent.ID, UserToggleInpComponent.DefaultDetails);

            ReadWriteImplementation.DefaultDetails.Add(LogicProbeComponent.ID, LogicProbeComponent.DefaultDetails);

            ReadWriteImplementation.DefaultDetails.Add(BoardContainerComponents.BoardInputComponent.ID, BoardContainerComponents.BoardInputComponent.DefaultDetails);
            ReadWriteImplementation.DefaultDetails.Add(BoardContainerComponents.BoardOutputComponent.ID, BoardContainerComponents.BoardOutputComponent.DefaultDetails);
            ReadWriteImplementation.DefaultDetails.Add(BoardContainerComponents.BoardBidirComponent.ID, BoardContainerComponents.BoardBidirComponent.DefaultDetails);

        }

        /* More Components: 
         * TristateBuffer
         * Not
         * VisualDisplay (need better name. the most basic form of graphical interaction that boards will inherit)
         * SevenSeg
         */
    }

    abstract class BaseComponent : IComponent
    {
        private Pos ComponentPos;
        private Rotation ComponentRotation;
        private Board ComponentBoard;

        public Pos GetComponentPos()
        {
            return ComponentPos;
        }

        public Rotation GetComponentRotation()
        {
            return ComponentRotation;
        }

        public Matrix GetRenderMatrix()
        {
            Matrix matrix = new Matrix();

            matrix.Rotate((float)ComponentRotation);
            matrix.Translate(ComponentPos.X, ComponentPos.Y);

            return matrix;
        }

        public Board GetComponentBoard()
        {
            return ComponentBoard;
        }

        private bool isPlaced = false;

        public bool IsPlaced() { return isPlaced; }

        public void Place(Pos pos, Board board)
        {
            Place(pos, Rotation.ZERO, board);
        }

        public void Place(Pos pos, Rotation rotation, Board board)
        {
            if (isPlaced)
            {
                throw new Exception("Already placed, can't place again. This error shouldn't raise when finished.");
            }

            ComponentPos = pos;
            ComponentRotation = rotation;
            ComponentBoard = board;

            board.AddComponent(this);

            isPlaced = true;
        }

        public void Remove() // can be removed if not placed
        {
            if (isPlaced)
            {
                ComponentBoard.RemoveComponent(this);
            }

            isPlaced = false;
        }

        public abstract void Tick();
        public void ResetToDefault() { }

        public abstract Pos[] GetAllPinOffsets();
        public abstract Pos[] GetAllPinPositions();

        public abstract string GetComponentID();
        public abstract string GetComponentDetails();

        public abstract IComponent NonStaticConstructor(string details);

        public abstract void Render(Graphics graphics, bool simulating, ColourScheme colourScheme);

        public abstract void RenderMainShape(Graphics graphics, ColourScheme colourScheme);

        protected void DrawComponentFromPath(Graphics graphics, GraphicsPath path, ColourScheme colourScheme)
        {
            graphics.FillPath(new SolidBrush(colourScheme.ComponentBackground), path);
            graphics.DrawPath(new Pen(colourScheme.ComponentEdge, 0.01F), path);
        }

        public abstract RectangleF GetComponentBounds();

        //*
        protected RectangleF GetDefaultComponentBounds()
        {
            Pos[] offsets = GetAllPinOffsets().Append(new Pos(0, 0)).ToArray();

            return RectangleF.FromLTRB(
                offsets.Select(offset => offset.X).Aggregate(Math.Min),
                offsets.Select(offset => offset.Y).Aggregate(Math.Min),
                offsets.Select(offset => offset.X).Aggregate(Math.Max),
                offsets.Select(offset => offset.Y).Aggregate(Math.Max));
        }//*/

        public RectangleF GetOffsetComponentBounds()
        {
            RectangleF rect = GetComponentBounds();
            Matrix matrix = GetRenderMatrix();

            PointF[] corners = { new PointF(rect.Left, rect.Top), new PointF(rect.Right, rect.Bottom) };
            matrix.TransformPoints(corners);

            return RectangleF.FromLTRB(corners[0].X, corners[0].Y, corners[1].X, corners[1].Y);
        }

        /*
        public virtual bool HasSettings() { return false; }

        public void OpenSettings() // this function is calling BaseComponent's version of the surrounding functions, when it should call its current class's version
        {
            Console.WriteLine("opening settings");
            Console.WriteLine(this);

            if (!HasSettings())
            {
                Console.WriteLine("didn't have settings, canceling");
                return;
            }

            Console.WriteLine(GetSettingDescriptions().Length);

            SettingsDialog settingsDialog = new SettingsDialog($"{GetComponentID()} Settings", GetSettingDescriptions());
            settingsDialog.ShowDialog();

            ApplySettings();
        }

        public virtual ISettingDescription[] GetSettingDescriptions() { return new ISettingDescription[0]; }
        public virtual void ApplySettings() { }
        //*/

        public IComponent Copy()
        {
            return NonStaticConstructor(GetComponentDetails());
        }

        public override string ToString()
        {
            return $"{GetComponentID()}:{GetComponentDetails()}@{ComponentPos}";
        }
    }

    abstract class InpOutpBaseComponents
    {
        public abstract class InpOutpTools
        {
            public static Pos[] GetMultPositions(IComponent comp, Pos[] MultOffsets)
            {
                return MultOffsets.Select(comp.GetComponentPos().Add).ToArray();
            }

            public static Pin[] GetMultPins(IComponent comp, Pos[] MultOffsets)
            {
                return GetMultPositions(comp, MultOffsets).Select((Pos pos) => comp.GetComponentBoard()[pos]).ToArray();
            }

            public static Pos GetSingPosition(IComponent comp, Pos SingOffset)
            {
                return comp.GetComponentPos().Add(SingOffset);
            }
            
            public static Pin GetSingPin(IComponent comp, Pos SingOffset)
            {
                return comp.GetComponentBoard()[GetSingPosition(comp, SingOffset)];
            }

            public static Pos ApplySingRotation(Rotation rotation, Pos SingOffsets)
            {
                return SingOffsets.Rotate(rotation);
            }

            public static Pos[] ApplyMultRotation(Rotation rotation, Pos[] MultOffsets)
            {
                return MultOffsets.Select((pos) => pos.Rotate(rotation)).ToArray();
            }


            public static Color GetWireColour(bool simulating, Pos offset, ColourScheme colourScheme, IComponent comp)
            {
                if (!simulating)
                {
                    return colourScheme.Wire;
                }

                if (comp.IsPlaced())
                {
                    return colourScheme.GetWireColour(comp.GetComponentBoard()[offset.Add(comp.GetComponentPos())].GetStateForDisplay());
                }
                return colourScheme.Wire;
            }

            public static void DrawInpOutpLine(Graphics graphics, bool simulating, Pos offset1, PointF offset2, ColourScheme colourScheme, IComponent comp)
            {
                graphics.DrawLine(
                    new Pen(GetWireColour(simulating, offset1, colourScheme, comp), 0.01F),
                    offset1.X, offset1.Y, offset2.X, offset2.Y);
                    //offset1.X, offset1.Y, offset2.X, offset2.Y);
            }

            public static void DrawInpLine(Graphics graphics, bool simulating, Pos inpOffset, ColourScheme colourScheme, IComponent comp)
            {
                DrawInpOutpLine(graphics, simulating, inpOffset, new PointF(inpOffset.X + 1.5F, inpOffset.Y), colourScheme, comp);
                //graphics.DrawLine(
                //    new Pen(GetWireColour(inpOffset, colourScheme, comp), 0.01F), 
                //    inpOffset.X, inpOffset.Y, inpOffset.X + 1.5F, inpOffset.Y);
            }

            public static void DrawOutpLine(Graphics graphics, bool simulating, Pos outpOffset, ColourScheme colourScheme, IComponent comp)
            {
                DrawInpOutpLine(graphics, simulating, outpOffset, new PointF(outpOffset.X - 1.5F, outpOffset.Y), colourScheme, comp);
                //graphics.DrawLine(
                //    new Pen(GetWireColour(outpOffset, colourScheme, comp), 0.01F),
                //    outpOffset.X, outpOffset.Y, outpOffset.X - 1.5F, outpOffset.Y);
            }
        }

        public interface ISingInpComponent : IComponent
        {
            Pos GetInpOffset();
            Pos GetRotatedInpOffset();
            Pos GetInpPosition();
            Pin GetInpPin();
        }

        public interface IMultInpComponent : IComponent
        {
            Pos[] GetInpOffsets();
            Pos[] GetRotatedInpOffsets();
            Pos[] GetInpPositions();
            Pin[] GetInpPins();
        }

        public interface ISingOutpComponent : IComponent
        {
            Pos GetOutpOffset();
            Pos GetRotatedOutpOffset();
            Pos GetOutpPosition();
            Pin GetOutpPin();
        }

        public interface IMultOutpComponent : IComponent
        {
            Pos[] GetOutpOffsets();
            Pos[] GetRotatedOutpOffsets();
            Pos[] GetOutpPositions();
            Pin[] GetOutpPins();
        }


        public interface ISingInpSingOutpComponent : ISingInpComponent, ISingOutpComponent { }
        public interface ISingInpMultOutpComponent : ISingInpComponent, IMultOutpComponent { }
        public interface IMultInpSingOutpComponent : IMultInpComponent, ISingOutpComponent { }
        public interface IMultInpMultOutpComponent : IMultInpComponent, IMultOutpComponent { }


        public abstract class SingInpNoneOutpBaseComponent : BaseComponent, ISingInpComponent
        {
            public abstract Pos GetInpOffset();

            public Pos GetRotatedInpOffset()
            {
                return InpOutpTools.ApplySingRotation(GetComponentRotation(), GetInpOffset());
            }

            public Pos GetInpPosition()
            {
                return InpOutpTools.GetSingPosition(this, GetRotatedInpOffset());
            }

            public Pin GetInpPin()
            {
                return InpOutpTools.GetSingPin(this, GetRotatedInpOffset());
            }


            public override Pos[] GetAllPinOffsets()
            {
                return new Pos[] { GetInpOffset() };
            }

            public override Pos[] GetAllPinPositions()
            {
                return new Pos[] { GetInpPosition() };
            }



            public override void Render(Graphics graphics, bool simulating, ColourScheme colourScheme)
            {
                InpOutpTools.DrawInpLine(graphics, simulating, GetInpOffset(), colourScheme, this);

                RenderMainShape(graphics, colourScheme);
            }
        }

        public abstract class MultInpNoneOutpBaseComponent : BaseComponent, IMultInpComponent
        {
            public abstract Pos[] GetInpOffsets();

            public Pos[] GetRotatedInpOffsets()
            {
                return InpOutpTools.ApplyMultRotation(GetComponentRotation(), GetInpOffsets());
            }

            public Pos[] GetInpPositions()
            {
                return InpOutpTools.GetMultPositions(this, GetRotatedInpOffsets());
            }

            public Pin[] GetInpPins()
            {
                return InpOutpTools.GetMultPins(this, GetRotatedInpOffsets());
            }



            public override Pos[] GetAllPinOffsets()
            {
                return GetInpOffsets();
            }

            public override Pos[] GetAllPinPositions()
            {
                return GetInpPositions();
            }



            public override void Render(Graphics graphics, bool simulating, ColourScheme colourScheme)
            {
                foreach (Pos inpOffset in GetInpOffsets())
                {
                    InpOutpTools.DrawInpLine(graphics, simulating, inpOffset, colourScheme, this);
                }

                RenderMainShape(graphics, colourScheme);
            }
        }

        public abstract class NoneInpSingOutpBaseComponent : BaseComponent, ISingOutpComponent
        {
            public abstract Pos GetOutpOffset();

            public Pos GetRotatedOutpOffset()
            {
                return InpOutpTools.ApplySingRotation(GetComponentRotation(), GetOutpOffset());
            }

            public Pos GetOutpPosition()
            {
                return InpOutpTools.GetSingPosition(this, GetRotatedOutpOffset());
            }

            public Pin GetOutpPin()
            {
                return InpOutpTools.GetSingPin(this, GetRotatedOutpOffset());
            }



            public override Pos[] GetAllPinOffsets()
            {
                return new Pos[] { GetOutpOffset() };
            }

            public override Pos[] GetAllPinPositions()
            {
                return new Pos[] { GetOutpPosition() };
            }



            public override void Render(Graphics graphics, bool simulating, ColourScheme colourScheme)
            {
                InpOutpTools.DrawOutpLine(graphics, simulating, GetOutpOffset(), colourScheme, this);

                RenderMainShape(graphics, colourScheme);
            }
        }

        public abstract class NoneInpMultOutpBaseComponent : BaseComponent, IMultOutpComponent
        {
            public abstract Pos[] GetOutpOffsets();

            public Pos[] GetRotatedOutpOffsets()
            {
                return InpOutpTools.ApplyMultRotation(GetComponentRotation(), GetOutpOffsets());
            }

            public Pos[] GetOutpPositions()
            {
                return InpOutpTools.GetMultPositions(this, GetRotatedOutpOffsets());
            }

            public Pin[] GetOutpPins()
            {
                return InpOutpTools.GetMultPins(this, GetRotatedOutpOffsets());
            }



            public override Pos[] GetAllPinOffsets()
            {
                return GetOutpOffsets();
            }

            public override Pos[] GetAllPinPositions()
            {
                return GetOutpPositions();
            }



            public override void Render(Graphics graphics, bool simulating, ColourScheme colourScheme)
            {
                foreach (Pos outpOffset in GetOutpOffsets())
                {
                    InpOutpTools.DrawOutpLine(graphics, simulating, outpOffset, colourScheme, this);
                }

                RenderMainShape(graphics, colourScheme);
            }
        }


        public abstract class SingInpSingOutpBaseComponent : BaseComponent, ISingInpSingOutpComponent
        {
            public abstract Pos GetInpOffset();

            public Pos GetRotatedInpOffset()
            {
                return InpOutpTools.ApplySingRotation(GetComponentRotation(), GetInpOffset());
            }

            public Pos GetInpPosition()
            {
                return InpOutpTools.GetSingPosition(this, GetRotatedInpOffset());
            }

            public Pin GetInpPin()
            {
                return InpOutpTools.GetSingPin(this, GetRotatedInpOffset());
            }



            public abstract Pos GetOutpOffset();

            public Pos GetRotatedOutpOffset()
            {
                return InpOutpTools.ApplySingRotation(GetComponentRotation(), GetOutpOffset());
            }

            public Pos GetOutpPosition()
            {
                return InpOutpTools.GetSingPosition(this, GetRotatedOutpOffset());
            }

            public Pin GetOutpPin()
            {
                return InpOutpTools.GetSingPin(this, GetRotatedOutpOffset());
            }



            public override Pos[] GetAllPinOffsets()
            {
                return new Pos[] { GetInpOffset(), GetOutpOffset() };
            }

            public override Pos[] GetAllPinPositions()
            {
                return new Pos[] { GetInpPosition(), GetOutpPosition() };
            }



            public override void Render(Graphics graphics, bool simulating, ColourScheme colourScheme)
            {
                InpOutpTools.DrawInpLine(graphics, simulating, GetInpOffset(), colourScheme, this);

                InpOutpTools.DrawOutpLine(graphics, simulating, GetOutpOffset(), colourScheme, this);

                RenderMainShape(graphics, colourScheme);
            }
        }

        public abstract class SingInpMultOutpBaseComponent : BaseComponent, ISingInpMultOutpComponent
        {
            public abstract Pos GetInpOffset();

            public Pos GetRotatedInpOffset()
            {
                return InpOutpTools.ApplySingRotation(GetComponentRotation(), GetInpOffset());
            }

            public Pos GetInpPosition()
            {
                return InpOutpTools.GetSingPosition(this, GetRotatedInpOffset());
            }

            public Pin GetInpPin()
            {
                return InpOutpTools.GetSingPin(this, GetRotatedInpOffset());
            }



            public abstract Pos[] GetOutpOffsets();

            public Pos[] GetRotatedOutpOffsets()
            {
                return InpOutpTools.ApplyMultRotation(GetComponentRotation(), GetOutpOffsets());
            }

            public Pos[] GetOutpPositions()
            {
                return InpOutpTools.GetMultPositions(this, GetRotatedOutpOffsets());
            }

            public Pin[] GetOutpPins()
            {
                return InpOutpTools.GetMultPins(this, GetRotatedOutpOffsets());
            }



            public override Pos[] GetAllPinOffsets()
            {
                return (new Pos[] { GetInpOffset() }).Concat(GetOutpOffsets()).ToArray();
            }

            public override Pos[] GetAllPinPositions()
            {
                return (new Pos[] { GetInpPosition() }).Concat(GetOutpPositions()).ToArray();
            }



            public override void Render(Graphics graphics, bool simulating, ColourScheme colourScheme)
            {
                InpOutpTools.DrawInpLine(graphics, simulating, GetInpOffset(), colourScheme, this);

                foreach (Pos outpOffset in GetOutpOffsets())
                {
                    InpOutpTools.DrawOutpLine(graphics, simulating, outpOffset, colourScheme, this);
                }

                RenderMainShape(graphics, colourScheme);
            }
        }

        public abstract class MultInpSingOutpBaseComponent : BaseComponent, IMultInpSingOutpComponent
        {
            public abstract Pos[] GetInpOffsets();

            public Pos[] GetRotatedInpOffsets()
            {
                return InpOutpTools.ApplyMultRotation(GetComponentRotation(), GetInpOffsets());
            }

            public Pos[] GetInpPositions()
            {
                return InpOutpTools.GetMultPositions(this, GetRotatedInpOffsets());
            }

            public Pin[] GetInpPins()
            {
                return InpOutpTools.GetMultPins(this, GetRotatedInpOffsets());
            }



            public abstract Pos GetOutpOffset();

            public Pos GetRotatedOutpOffset()
            {
                return InpOutpTools.ApplySingRotation(GetComponentRotation(), GetOutpOffset());
            }

            public Pos GetOutpPosition()
            {
                return InpOutpTools.GetSingPosition(this, GetRotatedOutpOffset());
            }

            public Pin GetOutpPin()
            {
                return InpOutpTools.GetSingPin(this, GetRotatedOutpOffset());
            }



            public override Pos[] GetAllPinOffsets()
            {
                return GetInpOffsets().Append(GetOutpOffset()).ToArray();
            }

            public override Pos[] GetAllPinPositions()
            {
                return GetInpPositions().Append(GetOutpPosition()).ToArray();
            }



            public override void Render(Graphics graphics, bool simulating, ColourScheme colourScheme)
            {
                foreach (Pos inpOffset in GetInpOffsets())
                {
                    InpOutpTools.DrawInpLine(graphics, simulating, inpOffset, colourScheme, this);
                }

                InpOutpTools.DrawOutpLine(graphics, simulating, GetOutpOffset(), colourScheme, this);

                RenderMainShape(graphics, colourScheme);
            }
        }

        public abstract class MultInpMultOutpBaseComponent : BaseComponent, IMultInpMultOutpComponent
        {
            public abstract Pos[] GetInpOffsets();

            public Pos[] GetRotatedInpOffsets()
            {
                return InpOutpTools.ApplyMultRotation(GetComponentRotation(), GetInpOffsets());
            }

            public Pos[] GetInpPositions()
            {
                return InpOutpTools.GetMultPositions(this, GetRotatedInpOffsets());
            }

            public Pin[] GetInpPins()
            {
                return InpOutpTools.GetMultPins(this, GetRotatedInpOffsets());
            }



            public abstract Pos[] GetOutpOffsets();

            public Pos[] GetRotatedOutpOffsets()
            {
                return InpOutpTools.ApplyMultRotation(GetComponentRotation(), GetOutpOffsets());
            }

            public Pos[] GetOutpPositions()
            {
                return InpOutpTools.GetMultPositions(this, GetRotatedOutpOffsets());
            }

            public Pin[] GetOutpPins()
            {
                return InpOutpTools.GetMultPins(this, GetRotatedOutpOffsets());
            }



            public override Pos[] GetAllPinOffsets()
            {
                return GetInpOffsets().Concat(GetOutpOffsets()).ToArray();
            }

            public override Pos[] GetAllPinPositions()
            {
                return GetInpPositions().Concat(GetOutpPositions()).ToArray();
            }



            public override void Render(Graphics graphics, bool simulating, ColourScheme colourScheme)
            {
                foreach (Pos inpOffset in GetInpOffsets())
                {
                    InpOutpTools.DrawInpLine(graphics, simulating, inpOffset, colourScheme, this);
                }

                foreach (Pos outpOffset in GetOutpOffsets())
                {
                    InpOutpTools.DrawOutpLine(graphics, simulating, outpOffset, colourScheme, this);
                }

                RenderMainShape(graphics, colourScheme);
            }
        }
    }

    abstract class VarInpComponents
    {
        public abstract class BaseVarInpComponent : InpOutpBaseComponents.MultInpSingOutpBaseComponent, ISettingsComponent
        {
            private int InpCount;

            private Pos[] InpOffsets;
            private Pos OutpOffset;

            private string Details;

            private PositiveIntSettingDescription inputSettingDesc;

            public override Pos[] GetInpOffsets()
            {
                return InpOffsets;
            }

            public override Pos GetOutpOffset()
            {
                return OutpOffset;
            }

            private void DefineInpOffsets()
            {
                if (InpCount < 2)
                {
                    throw new Exception("Can't have less than 2 inputs.");
                }

                InpOffsets = new Pos[InpCount];

                for (int inpNum = 0; inpNum < InpCount; inpNum++)
                {
                    InpOffsets[inpNum] = new Pos(-2, (2 * inpNum) - InpCount + 1);
                }

                Details = $"{InpCount}";
            }

            public BaseVarInpComponent(int inpCount)
            {
                OutpOffset = new Pos(2, 0);

                InpCount = inpCount;

                DefineInpOffsets();
            }

            public override void Tick()
            {
                GetOutpPin().SetState(GetInpPins().Select(inpPin => inpPin.GetStateForComponent()).Aggregate(Accumulator));
            }

            protected abstract Pin.State Accumulator(Pin.State state1, Pin.State state2);

            public override string GetComponentDetails()
            {
                return Details;
            }

            public override RectangleF GetComponentBounds()
            {
                RectangleF rect = GetDefaultComponentBounds();
                rect.Inflate(0, 0.5F);
                return rect;
            }

            public ISettingDescription[] GetSettingDescriptions()
            {
                inputSettingDesc = new PositiveIntSettingDescription("How many inputs should this component have?", InpCount);

                return new ISettingDescription[] { inputSettingDesc };
            }

            public void ApplySettings()
            {
                InpCount = inputSettingDesc.GetValue();

                DefineInpOffsets();
            }

            protected void DrawNotCircle(Graphics graphics, ColourScheme colourScheme)
            {
                float rad = 0.2F;
                graphics.FillEllipse(new SolidBrush(colourScheme.Background), GetOutpOffset().X - 1, GetOutpOffset().Y - rad, 2 * rad, 2 * rad); // need to make this transparent
                graphics.DrawEllipse(new Pen(colourScheme.ComponentEdge, 0.01F), GetOutpOffset().X - 1, GetOutpOffset().Y - rad, 2 * rad, 2 * rad);
            }

            protected GraphicsPath AddAndShape(GraphicsPath path)
            {
                float vertDist = InpOffsets.Length - 0.5F;

                path.AddBeziers(new PointF[] {
                    new PointF(-1, -vertDist),
                    new PointF(0, -vertDist),
                    new PointF(1, -vertDist),
                    new PointF(1, 0),
                    new PointF(1, vertDist),
                    new PointF(0, vertDist),
                    new PointF(-1, vertDist)
                });
                path.CloseFigure();

                return path;
            }

            protected GraphicsPath AddOrShape(GraphicsPath path)
            {
                float vertDist = InpOffsets.Length - 0.5F;

                path.AddBezier(
                    new PointF(-1.5F, -vertDist),
                    new PointF(-0.5F, -vertDist),
                    new PointF(0.5F, -vertDist),
                    new PointF(1, 0)
                );
                path.AddBezier(
                    new PointF(1, 0),
                    new PointF(0.5F, vertDist),
                    new PointF(-0.5F, vertDist),
                    new PointF(-1.5F, vertDist)
                );
                path.AddBezier(
                    new PointF(-1.5F, vertDist),
                    new PointF(-0.5F, 0.5F),
                    new PointF(-0.5F, -0.5F),
                    new PointF(-1.5F, -vertDist)
                );
                path.CloseFigure();

                return path;
            }

            protected GraphicsPath AddXorShape(GraphicsPath path)
            {
                float vertDist = InpOffsets.Length - 0.5F;

                AddOrShape(path);

                path.AddBezier(
                    new PointF(-1.75F, vertDist),
                    new PointF(-0.75F, 0.5F),
                    new PointF(-0.75F, -0.5F),
                    new PointF(-1.75F, -vertDist)
                );
                path.AddBezier(
                    new PointF(-1.75F, -vertDist),
                    new PointF(-0.75F, -0.5F),
                    new PointF(-0.75F, 0.5F),
                    new PointF(-1.75F, vertDist)
                );

                return path;
            }

            protected void DrawAndComponent(Graphics graphics, ColourScheme colourScheme)
            {
                GraphicsPath path = new GraphicsPath();

                AddAndShape(path);

                DrawComponentFromPath(graphics, path, colourScheme);
            }

            protected void DrawOrComponent(Graphics graphics, ColourScheme colourScheme)
            {
                GraphicsPath path = new GraphicsPath();

                AddOrShape(path);

                DrawComponentFromPath(graphics, path, colourScheme);
            }

            protected void DrawXorComponent(Graphics graphics, ColourScheme colourScheme)
            {
                GraphicsPath path = new GraphicsPath();

                AddXorShape(path);

                DrawComponentFromPath(graphics, path, colourScheme);
            }
        }

        public class VarInpAndComponent : BaseVarInpComponent
        {
            public VarInpAndComponent(int inpCount) : base(inpCount) { }

            protected override Pin.State Accumulator(Pin.State state1, Pin.State state2)
            {
                return state1.And(state2);
            }

            public static string ID = "AND";
            public static string DefaultDetails = "2";

            public override string GetComponentID()
            {
                return ID;
            }

            public static IComponent Constructor(string details)
            {
                int inpCount;

                if (int.TryParse(details, out inpCount))
                {
                    return new VarInpAndComponent(inpCount);
                }

                throw new Exception();
            }

            public override IComponent NonStaticConstructor(string details)
            {
                return Constructor(details);
            }

            public override void RenderMainShape(Graphics graphics, ColourScheme colourScheme)
            {
                DrawAndComponent(graphics, colourScheme);
            }
        }

        public class VarInpOrComponent : BaseVarInpComponent
        {
            public VarInpOrComponent(int inpCount) : base(inpCount) { }

            protected override Pin.State Accumulator(Pin.State state1, Pin.State state2)
            {
                return state1.Or(state2);
            }

            public static string ID = "OR";
            public static string DefaultDetails = "2";

            public override string GetComponentID()
            {
                return ID;
            }

            public static IComponent Constructor(string details)
            {
                int inpCount;

                if (int.TryParse(details, out inpCount))
                {
                    return new VarInpOrComponent(inpCount);
                }

                throw new Exception();
            }

            public override IComponent NonStaticConstructor(string details)
            {
                return Constructor(details);
            }

            public override void RenderMainShape(Graphics graphics, ColourScheme colourScheme)
            {
                DrawOrComponent(graphics, colourScheme);
            }
        }

        public class VarInpXorComponent : BaseVarInpComponent
        {
            public VarInpXorComponent(int inpCount) : base(inpCount) { }

            protected override Pin.State Accumulator(Pin.State state1, Pin.State state2)
            {
                return state1.Xor(state2);
            }

            public static string ID = "XOR";
            public static string DefaultDetails = "2";

            public override string GetComponentID()
            {
                return ID;
            }

            public static IComponent Constructor(string details)
            {
                int inpCount;

                if (int.TryParse(details, out inpCount))
                {
                    return new VarInpXorComponent(inpCount);
                }

                throw new Exception();
            }

            public override IComponent NonStaticConstructor(string details)
            {
                return Constructor(details);
            }

            public override void RenderMainShape(Graphics graphics, ColourScheme colourScheme)
            {
                DrawXorComponent(graphics, colourScheme);
            }
        }

        public class VarInpNandComponent : BaseVarInpComponent
        {
            public VarInpNandComponent(int inpCount) : base(inpCount) { }

            protected override Pin.State Accumulator(Pin.State state1, Pin.State state2)
            {
                return state1.And(state2).Not();
            }

            public static string ID = "NAND";
            public static string DefaultDetails = "2";

            public override string GetComponentID()
            {
                return ID;
            }

            public static IComponent Constructor(string details)
            {
                int inpCount;

                if (int.TryParse(details, out inpCount))
                {
                    return new VarInpNandComponent(inpCount);
                }

                throw new Exception();
            }

            public override IComponent NonStaticConstructor(string details)
            {
                return Constructor(details);
            }

            public override void RenderMainShape(Graphics graphics, ColourScheme colourScheme)
            {
                DrawAndComponent(graphics, colourScheme);
                DrawNotCircle(graphics, colourScheme);
            }
        }

        public class VarInpNorComponent : BaseVarInpComponent
        {
            public VarInpNorComponent(int inpCount) : base(inpCount) { }

            protected override Pin.State Accumulator(Pin.State state1, Pin.State state2)
            {
                return state1.Or(state2).Not();
            }

            public static string ID = "NOR";
            public static string DefaultDetails = "2";

            public override string GetComponentID()
            {
                return ID;
            }

            public static IComponent Constructor(string details)
            {
                int inpCount;

                if (int.TryParse(details, out inpCount))
                {
                    return new VarInpNorComponent(inpCount);
                }

                throw new Exception();
            }

            public override IComponent NonStaticConstructor(string details)
            {
                return Constructor(details);
            }

            public override void RenderMainShape(Graphics graphics, ColourScheme colourScheme)
            {
                DrawOrComponent(graphics, colourScheme);
                DrawNotCircle(graphics, colourScheme);
            }
        }

        public class VarInpXnorComponent : BaseVarInpComponent
        {
            public VarInpXnorComponent(int inpCount) : base(inpCount) { }

            protected override Pin.State Accumulator(Pin.State state1, Pin.State state2)
            {
                return state1.Xor(state2).Not();
            }

            public static string ID = "XNOR";
            public static string DefaultDetails = "2";

            public override string GetComponentID()
            {
                return ID;
            }

            public static IComponent Constructor(string details)
            {
                int inpCount;

                if (int.TryParse(details, out inpCount))
                {
                    return new VarInpXnorComponent(inpCount);
                }

                throw new Exception();
            }

            public override IComponent NonStaticConstructor(string details)
            {
                return Constructor(details);
            }

            public override void RenderMainShape(Graphics graphics, ColourScheme colourScheme)
            {
                DrawXorComponent(graphics, colourScheme);
                DrawNotCircle(graphics, colourScheme);
            }
        }
    }

    class FixedStateComponent : InpOutpBaseComponents.NoneInpSingOutpBaseComponent, ISettingsComponent
    {
        protected Pin.State OutputState;

        protected string stateDescriptor = "output";
        protected EnumSettingDescription<Pin.State> stateSettingDesc;

        public override Pos GetOutpOffset()
        {
            return new Pos(2, 0);
        }

        public FixedStateComponent(Pin.State state)
        {
            OutputState = state;
        }

        public override void Tick()
        {
            GetOutpPin().SetState(OutputState);
        }

        public static string ID = "FIXED";
        public static string DefaultDetails = $"{(int)Pin.State.LOW}";

        public override string GetComponentID()
        {
            return ID;
        }

        public override string GetComponentDetails()
        {
            return $"{(int)OutputState}";
        }

        public static IComponent Constructor(string details)
        {
            int outputState;

            if (int.TryParse(details, out outputState))
            {
                return new FixedStateComponent((Pin.State)outputState);
            }

            throw new Exception();
        }

        public override IComponent NonStaticConstructor(string details)
        {
            return Constructor(details);
        }

        public override RectangleF GetComponentBounds()
        {
            RectangleF rect = GetDefaultComponentBounds();
            rect.Inflate(0, 0.5F);
            rect.X -= 0.5F;
            rect.Width += 0.5F;
            return rect;
        }

        public virtual ISettingDescription[] GetSettingDescriptions()
        {
            stateSettingDesc = new EnumSettingDescription<Pin.State>("What is the output state for this component?", OutputState);

            return new ISettingDescription[] { stateSettingDesc };
        }

        public virtual void ApplySettings()
        {
            OutputState = stateSettingDesc.GetValue();
        }

        public override void RenderMainShape(Graphics graphics, ColourScheme colourScheme)
        {
            GraphicsPath path = new GraphicsPath();

            path.AddLines(new PointF[] { new PointF(-0.5F, -0.5F), new PointF(0.5F, -0.5F), new PointF(1, 0), new PointF(0.5F, 0.5F), new PointF(-0.5F, 0.5F) });
            path.CloseFigure();

            DrawComponentFromPath(graphics, path, colourScheme);
        }
    }

    class UserToggleInpComponent : FixedStateComponent, IInteractibleComponent
    {
        protected Pin.State DefaultState;

        protected new string stateDescriptor = "default";

        public UserToggleInpComponent(Pin.State startState) : base(startState)
        {
            DefaultState = startState;
        }

        public void Interact()
        {
            OutputState = OutputState.Not();
        }

        public static new string ID = "TOGGLE";

        public override string GetComponentID()
        {
            return ID;
        }

        public override string GetComponentDetails()
        {
            return $"{(int)DefaultState}";
        }

        public static new IComponent Constructor(string details)
        {
            int outputState;

            if (int.TryParse(details, out outputState))
            {
                return new UserToggleInpComponent((Pin.State)outputState);
            }

            throw new Exception();
        }

        public override IComponent NonStaticConstructor(string details)
        {
            return Constructor(details);
        }

        public override void ApplySettings()
        {
            DefaultState = stateSettingDesc.GetValue();
        }

        public new void ResetToDefault()
        {
            OutputState = DefaultState;
        }
    }

    class LogicProbeComponent : InpOutpBaseComponents.SingInpNoneOutpBaseComponent, IGraphicalComponent
    {
        public override Pos GetInpOffset()
        {
            return new Pos(-2, 0);
        }

        public LogicProbeComponent() { }

        public override void Tick() { }

        public static string ID = "PROBE";
        public static string DefaultDetails = "";

        public override string GetComponentID()
        {
            return ID;
        }

        public override string GetComponentDetails()
        {
            return "";
        }

        public static LogicProbeComponent Constructor(string details)
        {
            return new LogicProbeComponent();
        }

        public override IComponent NonStaticConstructor(string details)
        {
            return new LogicProbeComponent();
        }

        public override RectangleF GetComponentBounds()
        {
            RectangleF rect = GetDefaultComponentBounds();
            rect.Inflate(1, 0.5F);
            rect.Width -= 0.5F;
            rect.X++;
            return rect;
        }

        public override void RenderMainShape(Graphics graphics, ColourScheme colourScheme)
        {
            GraphicsPath path = new GraphicsPath();

            path.AddLines(new PointF[] { new PointF(1.5F, -0.5F), new PointF(-0.5F, -0.5F), new PointF(-1, 0), new PointF(-0.5F, 0.5F), new PointF(1.5F, 0.5F) });
            path.CloseFigure();

            DrawComponentFromPath(graphics, path, colourScheme);
        }

        public void RenderGraphicalElement(Graphics graphics, bool simulating, ColourScheme colourScheme)
        {
            if (simulating)
            {
                Pin.State state = GetInpPin().GetStateForDisplay();
                graphics.DrawString($"{state.ToString()[0]}", new Font("arial", 0.75F), new SolidBrush(colourScheme.GetWireColour(state)), 0, -0.5F);
            } else
            {
                graphics.DrawString("?", new Font("arial", 0.75F), new SolidBrush(colourScheme.Wire), 0, -0.5F);
            }
        }

        public SizeF GetGraphicalElementBounds()
        {
            return new SizeF(1, 1);
        }
    }

    abstract class BoardContainerComponents // io components should also contain (and save) a pos and direction for its pin
    {
        public class BoardInputComponent : UserToggleInpComponent, IBoardInputComponent
        {
            private string ComponentName;
            private Pin externalPin = null;

            private NameSettingDescription nameSettingDesc;

            public override Pos GetOutpOffset()
            {
                return new Pos(2, 0);
            }

            public string GetComponentName()
            {
                return ComponentName;
            }

            public void SetComponentName(string compName)
            {
                ComponentName = compName;
            }

            public BoardInputComponent(string name, Pin.State defaultState) : base(defaultState)
            {
                ComponentName = name;
            }

            public new static string ID = "INPUT";
            public new static string DefaultDetails = "INPUT,L";

            public override string GetComponentID()
            {
                return ID;
            }

            public override string GetComponentDetails()
            {
                char stateChar;

                if (DefaultState == Pin.State.LOW)
                {
                    stateChar = 'L';
                }
                else if (DefaultState == Pin.State.HIGH)
                {
                    stateChar = 'H';
                }
                else if (DefaultState == Pin.State.ILLEGAL)
                {
                    stateChar = 'I';
                }
                else
                {
                    stateChar = 'F';
                }

                return ComponentName + ',' + stateChar;
            }

            public void SetExternalPin(Pin pin)
            {
                externalPin = pin;
            }

            public void RemoveExternalPin()
            {
                externalPin = null;
            }

            public override void Tick()
            {
                if (externalPin != null)
                {
                    OutputState = externalPin.GetStateForComponent();
                }

                base.Tick();
            }

            public new static BoardInputComponent Constructor(string details)
            {
                char stateChar = details[details.Length - 1];
                Pin.State state;

                if (stateChar == 'L')
                {
                    state = Pin.State.LOW;
                } else if (stateChar == 'H')
                {
                    state = Pin.State.HIGH;
                } else if (stateChar == 'I')
                {
                    state = Pin.State.ILLEGAL;
                } else
                {
                    state = Pin.State.FLOATING;
                }

                return new BoardInputComponent(details.Substring(0, details.Length - 2), state);
            }

            public override IComponent NonStaticConstructor(string details)
            {
                return Constructor(details);
            }

            /*
            public new void ResetToDefault()
            {
                OutputState = DefaultState;
            }
            //*/

            public override ISettingDescription[] GetSettingDescriptions() // this is not being called, instead the base is being called.
            {
                nameSettingDesc = new NameSettingDescription("What is this component called?", ComponentName);

                //Console.WriteLine((new ISettingDescription[] { nameSettingDesc }).Concat(base.GetSettingDescriptions()).ToArray());

                return (new ISettingDescription[] { nameSettingDesc }).Concat(base.GetSettingDescriptions()).ToArray();
            }

            public override void ApplySettings()
            {
                base.ApplySettings();

                ComponentName = nameSettingDesc.GetValue();
            }

            //*
            public override RectangleF GetComponentBounds()
            {
                RectangleF rect = base.GetComponentBounds();
                rect.Width++;
                rect.X--;
                return rect;
            }//*/

            public override void Render(Graphics graphics, bool simulating, ColourScheme colourScheme)
            {
                base.Render(graphics, simulating, colourScheme);

                graphics.DrawString(ComponentName, new Font("arial", 0.5F), Brushes.Black, -1, -0.25F);
            }

            public override void RenderMainShape(Graphics graphics, ColourScheme colourScheme)
            {
                GraphicsPath path = new GraphicsPath();

                path.AddLines(new PointF[] { new PointF(-1.5F, -0.5F), new PointF(0.5F, -0.5F), new PointF(1, 0), new PointF(0.5F, 0.5F), new PointF(-1.5F, 0.5F) });
                path.CloseFigure();

                DrawComponentFromPath(graphics, path, colourScheme);
            }
        }

        public class BoardOutputComponent : InpOutpBaseComponents.SingInpNoneOutpBaseComponent, IBoardOutputComponent, ISettingsComponent // create a probe component and inherit it here.
        {
            private string ComponentName;
            protected Pin.State State;
            private Pin externalPin = null;

            private NameSettingDescription nameSettingDesc;

            public override Pos GetInpOffset()
            {
                return new Pos(-2, 0);
            }

            public string GetComponentName()
            {
                return ComponentName;
            }

            public void SetComponentName(string compName)
            {
                ComponentName = compName;
            }

            public BoardOutputComponent(string name)
            {
                ComponentName = name;
            }

            public static string ID = "OUTPUT";
            public static string DefaultDetails = "OUTPUT";

            public override string GetComponentID()
            {
                return ID;
            }

            public override string GetComponentDetails()
            {
                return ComponentName;
            }

            public static BoardOutputComponent Constructor(string details)
            {
                return new BoardOutputComponent(details);
            }

            public override IComponent NonStaticConstructor(string details)
            {
                return Constructor(details);
            }

            public void SetExternalPin(Pin pin)
            {
                externalPin = pin;
            }

            public void RemoveExternalPin()
            {
                externalPin = null;
            }

            public Pin.State GetOutputState()
            {
                return State;
            }

            public override void Tick()
            {
                State = GetInpPin().GetStateForComponent();

                if (externalPin != null)
                {
                    externalPin.SetState(State);
                }
            }

            public ISettingDescription[] GetSettingDescriptions()
            {
                nameSettingDesc = new NameSettingDescription("What is this component called?", ComponentName);

                return new ISettingDescription[] { nameSettingDesc };
            }

            public void ApplySettings()
            {
                ComponentName = nameSettingDesc.GetValue();
            }

            public override RectangleF GetComponentBounds()
            {
                RectangleF rect = GetDefaultComponentBounds();
                rect.Inflate(1, 0.5F);
                rect.Width -= 0.5F;
                rect.X++;
                return rect;
            }

            public override void Render(Graphics graphics, bool simulating, ColourScheme colourScheme)
            {
                base.Render(graphics, simulating, colourScheme);

                graphics.DrawString(ComponentName, new Font("arial", 0.5F), Brushes.Black, -0.5F, -0.25F);
            }

            public override void RenderMainShape(Graphics graphics, ColourScheme colourScheme)
            {
                GraphicsPath path = new GraphicsPath();

                path.AddLines(new PointF[] { new PointF(1.5F, -0.5F), new PointF(-0.5F, -0.5F), new PointF(-1, 0), new PointF(-0.5F, 0.5F), new PointF(1.5F, 0.5F) });
                path.CloseFigure();

                DrawComponentFromPath(graphics, path, colourScheme);
            }
        }

        public class BoardBidirComponent : InpOutpBaseComponents.SingInpSingOutpBaseComponent, IBoardInputComponent, IBoardOutputComponent, ISettingsComponent
        {
            private string ComponentName;
            private Pin.State DefaultExternalState;
            private Pin externalPin = null;

            private NameSettingDescription nameSettingDesc;
            private EnumSettingDescription<Pin.State> defaultStateSettingDesc;

            private readonly Pos offset = new Pos(0, 2);

            public override Pos GetInpOffset()
            {
                return offset;
            }

            public override Pos GetOutpOffset()
            {
                return offset;
            }

            public string GetComponentName()
            {
                return ComponentName;
            }

            public void SetComponentName(string compName)
            {
                ComponentName = compName;
            }

            public BoardBidirComponent(string name, Pin.State defaultState)
            {
                ComponentName = name;

                DefaultExternalState = defaultState;
            }

            public static string ID = "BIDIR";
            public static string DefaultDetails = $"BIDIR,{(int)Pin.State.LOW}";

            public override string GetComponentID()
            {
                return ID;
            }

            public override string GetComponentDetails()
            {
                return $"{ComponentName},{(int)DefaultExternalState}";
            }

            public void SetExternalPin(Pin pin)
            {
                externalPin = pin;
            }

            public void RemoveExternalPin()
            {
                externalPin = null;
            }

            public override void Tick()
            {
                if (externalPin == null)
                {
                    GetOutpPin().SetState(GetInpPin().GetStateForWire().WireJoin(DefaultExternalState));
                } else
                {
                    Pin.State state = GetInpPin().GetStateForWire().WireJoin(externalPin.GetStateForWire());

                    GetOutpPin().SetState(state);
                    externalPin.SetState(state);
                }
            }

            public static BoardBidirComponent Constructor(string details)
            {
                string[] strings = details.Split(',');

                if (int.TryParse(strings[1], out int stateInt))
                {
                    return new BoardBidirComponent(strings[0], (Pin.State)stateInt);
                }

                throw new Exception();
            }

            public override IComponent NonStaticConstructor(string details)
            {
                return Constructor(details);
            }

            public ISettingDescription[] GetSettingDescriptions()
            {
                nameSettingDesc = new NameSettingDescription("What is this component called?", ComponentName);
                defaultStateSettingDesc = new EnumSettingDescription<Pin.State>("What is the default state of this component?", DefaultExternalState);

                return new ISettingDescription[] { nameSettingDesc, defaultStateSettingDesc };
            }

            public void ApplySettings()
            {
                ComponentName = nameSettingDesc.GetValue();
                DefaultExternalState = defaultStateSettingDesc.GetValue();
            }

            public override RectangleF GetComponentBounds()
            {
                RectangleF rect = GetDefaultComponentBounds();
                rect.Inflate(0.5F, 1);
                //rect.Width++;
                //rect.X -= 0.5F;
                //rect.Y -= 0.5F;
                rect.Height -= 0.5F;
                rect.Y -= 0.5F;
                return rect;
            }

            public override void Render(Graphics graphics, bool simulating, ColourScheme colourScheme)
            {
                InpOutpBaseComponents.InpOutpTools.DrawInpOutpLine(graphics, simulating, offset, new PointF(offset.X, offset.Y - 1.5F), colourScheme, this);

                RenderMainShape(graphics, colourScheme);

                graphics.DrawString(ComponentName, new Font("arial", 0.5F), Brushes.Black, -1, -0.25F);
            }

            public override void RenderMainShape(Graphics graphics, ColourScheme colourScheme)
            {
                GraphicsPath path = new GraphicsPath();

                path.AddLines(new PointF[] { new PointF(-0.5F, -1.5F), new PointF(-0.5F, 0.5F), new PointF(0, 1), new PointF(0.5F, 0.5F), new PointF(0.5F, -1.5F) });
                path.CloseFigure();

                DrawComponentFromPath(graphics, path, colourScheme);
            }
        }

        public class BoardContainerComponent : InpOutpBaseComponents.MultInpMultOutpBaseComponent
        {
            public Board InternalBoard;

            private Pos[] InpOffsets;
            private string[] InpNames;

            private Pos[] OutpOffsets;
            private string[] OutpNames;

            public override Pos[] GetInpOffsets()
            {
                return InpOffsets.ToArray();
            }

            public override Pos[] GetOutpOffsets()
            {
                return OutpOffsets.ToArray();
            }

            public BoardContainerComponent(Board internalBoard)
            {
                InternalBoard = internalBoard;

                IBoardInputComponent[] inpComps = internalBoard.GetInputComponents();
                int inputCount = inpComps.Length;
                InpOffsets = new Pos[inputCount];
                InpNames = new string[inputCount];

                for (int i = 0; i < inputCount; i++)
                {
                    InpOffsets[i] = new Pos(-5, (2 * i) - inputCount);
                    InpNames[i] = inpComps[i].GetComponentName();
                }

                IBoardOutputComponent[] outpComps = internalBoard.GetOutputComponents();
                int outputCount = inpComps.Length;
                OutpOffsets = new Pos[outputCount];
                OutpNames = new string[outputCount];

                for (int i = 0; i < outputCount; i++)
                {
                    OutpOffsets[i] = new Pos(5, (2 * i) - outputCount);
                    OutpNames[i] = outpComps[i].GetComponentName();
                }
            }

            public static string ID = "BOARD";

            public override string GetComponentID()
            {
                return ID;
            }

            public override string GetComponentDetails()
            {
                using (Stream stream = new MemoryStream())
                {
                    using (BinaryWriter bw = new BinaryWriter(stream))
                    {
                        bw.Write(InternalBoard);
                    }

                    using (StreamReader sr = new StreamReader(stream))
                    {
                        return sr.ReadToEnd();
                    }
                }

                //return InternalBoard.Name; // return the file form of the board instead
            }

            public new void Place(Pos pos, Rotation rotation, Board board)
            {
                base.Place(pos, rotation, board);

                Pin[] inpPins = GetInpPins();
                for (int i = 0; i < InpNames.Length; i++)
                {
                    InternalBoard.GetInputComponent(InpNames[i]).SetExternalPin(inpPins[i]);
                }

                Pin[] outpPins = GetOutpPins();
                for (int i = 0; i < OutpNames.Length; i++)
                {
                    InternalBoard.GetOutputComponent(OutpNames[i]).SetExternalPin(outpPins[i]);
                }
            }

            public new void Remove()
            {
                base.Remove();

                foreach (IBoardInputComponent inpComp in InternalBoard.GetInputComponents())
                {
                    inpComp.RemoveExternalPin();
                }

                foreach (IBoardOutputComponent outpComp in InternalBoard.GetOutputComponents())
                {
                    outpComp.RemoveExternalPin();
                }
            }

            public override void Tick()
            {
                //Pin[] inpPins = GetInpPins();
                //for (int i = 0; i < InpNames.Length; i++)
                //{
                //    InternalBoard.GetInputComponent(InpNames[i]).SetInputState(inpPins[i].GetStateForComponent());
                //}

                //Pin[] outpPins = GetOutpPins();
                //for (int i = 0; i < OutpNames.Length; i++)
                //{
                //    outpPins[i].SetState(InternalBoard.GetOutputComponent(OutpNames[i]).GetOutputState());
                //}

                InternalBoard.Tick();
            }

            public static BoardContainerComponent Constructor(string details)
            {
                using (Stream stream = new MemoryStream(details.Cast<byte>().ToArray()))
                {
                    using (BinaryReader br = new BinaryReader(stream))
                    {
                        return new BoardContainerComponent(br.ReadBoard());
                    }
                }

                //return new BoardContainerComponent(Board.Load(details));
            }

            public override IComponent NonStaticConstructor(string details)
            {
                return Constructor(details);
            }

            public new IComponent Copy()
            {
                return new BoardContainerComponent(InternalBoard.Copy());
            }

            public override RectangleF GetComponentBounds()
            {
                RectangleF rect = GetDefaultComponentBounds();
                rect.Inflate(0, 1);
                return rect;
            }

            public override void RenderMainShape(Graphics graphics, ColourScheme colourScheme)
            {
                //RectangleF rect = GetDefaultComponentBounds();
                //rect.Inflate(-1, -1);

                //graphics.FillRectangle(new SolidBrush(colourScheme.ComponentBackground), rect);
                //graphics.DrawRectangle(new Pen(Color.Red, 0.05F), rect.X, rect.Y, rect.Width, rect.Height);
            }
        }
    } 
}
