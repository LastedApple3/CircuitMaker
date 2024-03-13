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
using System.Security.Policy;
using System.Reflection.Emit;

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

            ReadWriteImplementation.Constructors.Add(BufferComponents.BufferComponent.ID, BufferComponents.BufferComponent.Constructor);
            ReadWriteImplementation.Constructors.Add(BufferComponents.NotComponent.ID, BufferComponents.NotComponent.Constructor);
            ReadWriteImplementation.Constructors.Add(BufferComponents.TristateBufferComponent.ID, BufferComponents.TristateBufferComponent.Constructor);

            ReadWriteImplementation.Constructors.Add(FixedStateComponent.ID, FixedStateComponent.Constructor);
            ReadWriteImplementation.Constructors.Add(UserToggleInpComponent.ID, UserToggleInpComponent.Constructor);
            ReadWriteImplementation.Constructors.Add(UserPulseInpComponent.ID, UserPulseInpComponent.Constructor);

            ReadWriteImplementation.Constructors.Add(LogicProbeComponent.ID, LogicProbeComponent.Constructor);

            ReadWriteImplementation.Constructors.Add(SevenSegmentComponent.ID, SevenSegmentComponent.Constructor);

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

            ReadWriteImplementation.DefaultDetails.Add(BufferComponents.BufferComponent.ID, BufferComponents.BufferComponent.DefaultDetails);
            ReadWriteImplementation.DefaultDetails.Add(BufferComponents.NotComponent.ID, BufferComponents.NotComponent.DefaultDetails);
            ReadWriteImplementation.DefaultDetails.Add(BufferComponents.TristateBufferComponent.ID, BufferComponents.TristateBufferComponent.DefaultDetails);

            ReadWriteImplementation.DefaultDetails.Add(FixedStateComponent.ID, FixedStateComponent.DefaultDetails);
            ReadWriteImplementation.DefaultDetails.Add(UserToggleInpComponent.ID, UserToggleInpComponent.DefaultDetails);
            ReadWriteImplementation.DefaultDetails.Add(UserPulseInpComponent.ID, UserPulseInpComponent.DefaultDetails);

            ReadWriteImplementation.DefaultDetails.Add(LogicProbeComponent.ID, LogicProbeComponent.DefaultDetails);

            ReadWriteImplementation.DefaultDetails.Add(SevenSegmentComponent.ID, SevenSegmentComponent.DefaultDetails);

            ReadWriteImplementation.DefaultDetails.Add(BoardContainerComponents.BoardInputComponent.ID, BoardContainerComponents.BoardInputComponent.DefaultDetails);
            ReadWriteImplementation.DefaultDetails.Add(BoardContainerComponents.BoardOutputComponent.ID, BoardContainerComponents.BoardOutputComponent.DefaultDetails);
            ReadWriteImplementation.DefaultDetails.Add(BoardContainerComponents.BoardBidirComponent.ID, BoardContainerComponents.BoardBidirComponent.DefaultDetails);

        }

        /* More Components:
         * Multiplexer?
         * Specialised 7seg MUX?
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

            matrix.Translate(ComponentPos.X, ComponentPos.Y);
            matrix.Rotate((float)ComponentRotation);

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

        public virtual void Place(Pos pos, Rotation rotation, Board board)
        {
            if (isPlaced)
            {
                throw new PlacementException("Already placed, can't place again.");
            }

            ComponentPos = pos;
            ComponentRotation = rotation;
            ComponentBoard = board;

            board.AddComponent(this);

            isPlaced = true;
        }

        public virtual void Remove() // can be removed if not placed
        {
            if (isPlaced)
            {
                ComponentBoard.RemoveComponent(this);
            }

            isPlaced = false;
        }

        public abstract void Tick();
        public virtual void ResetToDefault() { }

        public abstract Pos[] GetAllPinOffsets();
        public abstract Pos[] GetAllPinPositions();

        public abstract string GetComponentID();
        public abstract string GetComponentDetails();

        public abstract IComponent NonStaticConstructor(string details);

        public abstract void Render(Graphics graphics, bool simulating, ColourScheme colourScheme);

        public abstract void RenderMainShape(Graphics graphics, bool simulating, ColourScheme colourScheme);

        protected void DrawComponentFromPath(Graphics graphics, GraphicsPath path, ColourScheme colourScheme)
        {
            graphics.FillPath(new SolidBrush(colourScheme.ComponentBackground), path);
            graphics.DrawPath(new Pen(colourScheme.ComponentEdge, 0.01F), path);
        }

        protected void DrawInversionCircle(Graphics graphics, PointF point, ColourScheme colourScheme, float rad = 0.2F)
        {
            graphics.FillEllipse(new SolidBrush(colourScheme.Background), point.X, point.Y - rad, 2 * rad, 2 * rad);
            graphics.DrawEllipse(new Pen(colourScheme.ComponentEdge, 0.01F), point.X, point.Y - rad, 2 * rad, 2 * rad);
        }

        public abstract RectangleF GetComponentBounds();

        protected RectangleF CreateSmallestRect(Pos[] containedPoints)
        {
            return RectangleF.FromLTRB(
                containedPoints.Select(offset => offset.X).Aggregate(Math.Min),
                containedPoints.Select(offset => offset.Y).Aggregate(Math.Min),
                containedPoints.Select(offset => offset.X).Aggregate(Math.Max),
                containedPoints.Select(offset => offset.Y).Aggregate(Math.Max));
        }

        protected RectangleF GetDefaultComponentBounds()
        {
            Pos[] offsets = GetAllPinOffsets().Append(new Pos(0, 0)).ToArray();

            return CreateSmallestRect(offsets);
        }

        public RectangleF GetOffsetComponentBounds()
        {
            RectangleF rect = GetComponentBounds();
            Matrix matrix = GetRenderMatrix();

            PointF[] corners = { new PointF(rect.Left, rect.Top), new PointF(rect.Right, rect.Bottom) };
            matrix.TransformPoints(corners);

            return RectangleF.FromLTRB(
                Math.Min(corners[0].X, corners[1].X),
                Math.Min(corners[0].Y, corners[1].Y),
                Math.Max(corners[0].X, corners[1].X),
                Math.Max(corners[0].Y, corners[1].Y)
            ).Round();
        }

        public IComponent Copy()
        {
            return NonStaticConstructor(GetComponentDetails());
        }

        public override string ToString()
        {
            return $"{GetComponentID()}:{GetComponentDetails()}@{ComponentPos}";
        }

        private Pos[] RemoveDuplicates(Pos[] positions)
        {
            List<Pos> posList = new List<Pos>();

            foreach (Pos pos in positions)
            {
                if (!posList.Contains(pos))
                {
                    posList.Add(pos);
                }
            }

            return posList.ToArray();
        }

        public Pos[] GetAllUniquePinPositions()
        {
            return RemoveDuplicates(GetAllPinPositions());
        }
    }

    abstract class InpOutpBaseComponents
    {
        public abstract class InpOutpTools
        {
            public static Pos GetSingRotatedOffset(IComponent comp, Pos SingPinOffset) // returns offset, rotated
            {
                return SingPinOffset.Rotate(comp.GetComponentRotation());
            }

            public static Pos GetSingPosition(IComponent comp, Pos SingPinOffset) // returns pin position
            {
                return comp.GetComponentPos().Add(GetSingRotatedOffset(comp, SingPinOffset));
            }
            
            public static Pin GetSingPin(IComponent comp, Pos SingPinOffset)
            {
                return comp.GetComponentBoard()[GetSingPosition(comp, SingPinOffset)];
            }

            public static Pos[] GetMultRotatedOffsets(IComponent comp, Pos[] MultPinOffset)
            {
                return MultPinOffset.Select(pinOffset => GetSingRotatedOffset(comp, pinOffset)).ToArray();
            }

            public static Pos[] GetMultPositions(IComponent comp, Pos[] MultPinOffset)
            {
                return MultPinOffset.Select(pinOffset => GetSingPosition(comp, pinOffset)).ToArray();
            }

            public static Pin[] GetMultPins(IComponent comp, Pos[] MultPinOffset)
            {
                return MultPinOffset.Select(pinOffset => GetSingPin(comp, pinOffset)).ToArray();
            }


            public static Color GetWireColour(bool simulating, Pos offset, ColourScheme colourScheme, IComponent comp)
            {
                if (simulating && comp.IsPlaced())
                {
                    return colourScheme.GetWireColour(comp.GetComponentBoard()[offset.Rotate(comp.GetComponentRotation()).Add(comp.GetComponentPos())].GetStateForDisplay());
                }

                return colourScheme.Wire;
            }

            public static void DrawInpOutpLine(Graphics graphics, bool simulating, Pos pinOffset, PointF otherOffset, ColourScheme colourScheme, IComponent comp)
            {
                graphics.DrawLine(
                    new Pen(GetWireColour(simulating, pinOffset, colourScheme, comp), 0.01F),
                    pinOffset.X, pinOffset.Y, otherOffset.X, otherOffset.Y);
            }

            public static void DrawInpLine(Graphics graphics, bool simulating, Pos inpOffset, ColourScheme colourScheme, IComponent comp)
            {
                DrawInpOutpLine(graphics, simulating, inpOffset, new PointF(inpOffset.X + 1.5F, inpOffset.Y), colourScheme, comp);
            }

            public static void DrawOutpLine(Graphics graphics, bool simulating, Pos outpOffset, ColourScheme colourScheme, IComponent comp)
            {
                DrawInpOutpLine(graphics, simulating, outpOffset, new PointF(outpOffset.X - 1.5F, outpOffset.Y), colourScheme, comp);
            }
        }


        public interface ISingInpComponent : IComponent
        {
            Pos GetInpOffset();
            Pos GetInpPosition();
            Pin GetInpPin();
        }

        public interface IMultInpComponent : IComponent
        {
            Pos[] GetInpOffsets();
            Pos[] GetInpPositions();
            Pin[] GetInpPins();
        }

        public interface ISingOutpComponent : IComponent
        {
            Pos GetOutpOffset();
            Pos GetOutpPosition();
            Pin GetOutpPin();
        }

        public interface IMultOutpComponent : IComponent
        {
            Pos[] GetOutpOffsets();
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

            public Pos GetInpPosition()
            {
                return InpOutpTools.GetSingPosition(this, GetInpOffset());
            }

            public Pin GetInpPin()
            {
                return InpOutpTools.GetSingPin(this, GetInpOffset());
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

                RenderMainShape(graphics, simulating, colourScheme);
            }
        }

        public abstract class MultInpNoneOutpBaseComponent : BaseComponent, IMultInpComponent
        {
            public abstract Pos[] GetInpOffsets();

            public Pos[] GetInpPositions()
            {
                return InpOutpTools.GetMultPositions(this, GetInpOffsets());
            }

            public Pin[] GetInpPins()
            {
                return InpOutpTools.GetMultPins(this, GetInpOffsets());
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

                RenderMainShape(graphics, simulating, colourScheme);
            }
        }

        public abstract class NoneInpSingOutpBaseComponent : BaseComponent, ISingOutpComponent
        {
            public abstract Pos GetOutpOffset();

            public Pos GetOutpPosition()
            {
                return InpOutpTools.GetSingPosition(this, GetOutpOffset());
            }

            public Pin GetOutpPin()
            {
                return InpOutpTools.GetSingPin(this, GetOutpOffset());
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

                RenderMainShape(graphics, simulating, colourScheme);
            }
        }

        public abstract class NoneInpMultOutpBaseComponent : BaseComponent, IMultOutpComponent
        {
            public abstract Pos[] GetOutpOffsets();

            public Pos[] GetOutpPositions()
            {
                return InpOutpTools.GetMultPositions(this, GetOutpOffsets());
            }

            public Pin[] GetOutpPins()
            {
                return InpOutpTools.GetMultPins(this, GetOutpOffsets());
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

                RenderMainShape(graphics, simulating, colourScheme);
            }
        }


        public abstract class SingInpSingOutpBaseComponent : BaseComponent, ISingInpSingOutpComponent
        {
            public abstract Pos GetInpOffset();

            public Pos GetInpPosition()
            {
                return InpOutpTools.GetSingPosition(this, GetInpOffset());
            }

            public Pin GetInpPin()
            {
                return InpOutpTools.GetSingPin(this, GetInpOffset());
            }



            public abstract Pos GetOutpOffset();

            public Pos GetOutpPosition()
            {
                return InpOutpTools.GetSingPosition(this, GetOutpOffset());
            }

            public Pin GetOutpPin()
            {
                return InpOutpTools.GetSingPin(this, GetOutpOffset());
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

                RenderMainShape(graphics, simulating, colourScheme);
            }
        }

        public abstract class SingInpMultOutpBaseComponent : BaseComponent, ISingInpMultOutpComponent
        {
            public abstract Pos GetInpOffset();

            public Pos GetInpPosition()
            {
                return InpOutpTools.GetSingPosition(this, GetInpOffset());
            }

            public Pin GetInpPin()
            {
                return InpOutpTools.GetSingPin(this, GetInpOffset());
            }



            public abstract Pos[] GetOutpOffsets();

            public Pos[] GetOutpPositions()
            {
                return InpOutpTools.GetMultPositions(this, GetOutpOffsets());
            }

            public Pin[] GetOutpPins()
            {
                return InpOutpTools.GetMultPins(this, GetOutpOffsets());
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

                RenderMainShape(graphics, simulating, colourScheme);
            }
        }

        public abstract class MultInpSingOutpBaseComponent : BaseComponent, IMultInpSingOutpComponent
        {
            public abstract Pos[] GetInpOffsets();

            public Pos[] GetInpPositions()
            {
                return InpOutpTools.GetMultPositions(this, GetInpOffsets());
            }

            public Pin[] GetInpPins()
            {
                return InpOutpTools.GetMultPins(this, GetInpOffsets());
            }



            public abstract Pos GetOutpOffset();

            public Pos GetOutpPosition()
            {
                return InpOutpTools.GetSingPosition(this, GetOutpOffset());
            }

            public Pin GetOutpPin()
            {
                return InpOutpTools.GetSingPin(this, GetOutpOffset());
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

                RenderMainShape(graphics, simulating, colourScheme);
            }
        }

        public abstract class MultInpMultOutpBaseComponent : BaseComponent, IMultInpMultOutpComponent
        {
            public abstract Pos[] GetInpOffsets();

            public Pos[] GetInpPositions()
            {
                return InpOutpTools.GetMultPositions(this, GetInpOffsets());
            }

            public Pin[] GetInpPins()
            {
                return InpOutpTools.GetMultPins(this, GetInpOffsets());
            }



            public abstract Pos[] GetOutpOffsets();

            public Pos[] GetOutpPositions()
            {
                return InpOutpTools.GetMultPositions(this, GetOutpOffsets());
            }

            public Pin[] GetOutpPins()
            {
                return InpOutpTools.GetMultPins(this, GetOutpOffsets());
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

                RenderMainShape(graphics, simulating, colourScheme);
            }
        }
    }

    abstract class BufferComponents
    {
        public abstract class BaseBufferComponent : InpOutpBaseComponents.MultInpSingOutpBaseComponent
        {
            public override Pos GetOutpOffset()
            {
                return new Pos(1, 0);
            }

            public override void RenderMainShape(Graphics graphics, bool simulating, ColourScheme colourScheme)
            {
                GraphicsPath path = new GraphicsPath();

                path.AddLines(new PointF[]
                {
                new PointF(-0.5F, -0.5F),
                new PointF(0.5F, 0),
                new PointF(-0.5F, 0.5F)
                });
                path.CloseFigure();

                DrawComponentFromPath(graphics, path, colourScheme);
            }

            public override RectangleF GetComponentBounds()
            {
                RectangleF rect = GetDefaultComponentBounds();
                rect.Inflate(0, 0.5F);
                return rect;
            }
        }

        public class BufferComponent : BaseBufferComponent
        {
            public override Pos[] GetInpOffsets()
            {
                return new Pos[] { new Pos(-1, 0) };
            }

            public BufferComponent() { }

            public override void Tick()
            {
                GetOutpPin().SetState(GetInpPins()[0].GetStateForComponent());
            }

            public static string ID = "BUFFER";
            public static string DefaultDetails = "";

            public override string GetComponentID()
            {
                return ID;
            }

            public override string GetComponentDetails()
            {
                return "";
            }

            public static BufferComponent Constructor(string details)
            {
                return new BufferComponent();
            }

            public override IComponent NonStaticConstructor(string details)
            {
                return Constructor(details);
            }
        }

        public class NotComponent : BaseBufferComponent
        {
            public override Pos[] GetInpOffsets()
            {
                return new Pos[] { new Pos(-1, 0) };
            }

            public NotComponent() { }

            public override void Tick()
            {
                GetOutpPin().SetState(GetInpPins()[0].GetStateForComponent().Not());
            }

            public static string ID = "NOT";
            public static string DefaultDetails = "";

            public override string GetComponentID()
            {
                return ID;
            }

            public override string GetComponentDetails()
            {
                return "";
            }

            public static NotComponent Constructor(string details)
            {
                return new NotComponent();
            }

            public override IComponent NonStaticConstructor(string details)
            {
                return Constructor(details);
            }

            public override void RenderMainShape(Graphics graphics, bool simulating, ColourScheme colourScheme)
            {
                base.RenderMainShape(graphics, simulating, colourScheme);

                DrawInversionCircle(graphics, new PointF(0.5F, 0), colourScheme, 0.1F);
            }
        }

        public class TristateBufferComponent : BaseBufferComponent
        {
            public override Pos[] GetInpOffsets()
            {
                return new Pos[] {
                    new Pos(-1, 0),
                    new Pos(0, -1)
                };
            }

            public TristateBufferComponent() { }

            public override void Tick()
            {
                Pin.State activationState = GetInpPins()[1].GetStateForComponent();

                if (activationState == Pin.State.HIGH)
                {
                    GetOutpPin().SetState(GetInpPins()[0].GetStateForComponent());
                }
                else if (activationState == Pin.State.LOW || activationState == Pin.State.FLOATING)
                {
                    GetOutpPin().SetState(Pin.State.FLOATING);
                }
                else
                {
                    GetOutpPin().SetState(Pin.State.ILLEGAL);
                }
            }

            public static string ID = "TRISTATE";
            public static string DefaultDetails = "";

            public override string GetComponentID()
            {
                return ID;
            }

            public override string GetComponentDetails()
            {
                return "";
            }

            public static TristateBufferComponent Constructor(string details)
            {
                return new TristateBufferComponent();
            }

            public override IComponent NonStaticConstructor(string details)
            {
                return Constructor(details);
            }

            public override RectangleF GetComponentBounds()
            {
                RectangleF rect = GetDefaultComponentBounds();
                rect.Height += 0.5F;
                return rect;
            }

            public override void Render(Graphics graphics, bool simulating, ColourScheme colourScheme)
            {
                InpOutpBaseComponents.InpOutpTools.DrawInpLine(graphics, simulating, GetInpOffsets()[0], colourScheme, this);

                InpOutpBaseComponents.InpOutpTools.DrawInpOutpLine(graphics, simulating, GetInpOffsets()[1], new PointF(0, 0), colourScheme, this);

                InpOutpBaseComponents.InpOutpTools.DrawOutpLine(graphics, simulating, GetOutpOffset(), colourScheme, this);

                RenderMainShape(graphics, simulating, colourScheme);
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
                    throw new PlacementException("Can't have less than 2 inputs.");
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

            protected abstract bool IsNegated();

            public override void Tick()
            {
                Pin.State state = GetInpPins().Select(inpPin => inpPin.GetStateForComponent()).Aggregate(Accumulator);

                state = IsNegated() ? state.Not() : state;

                GetOutpPin().SetState(state);
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

            protected static int ParseInpCount(string details)
            {
                int inpCount;

                if (int.TryParse(details, out inpCount))
                {
                    return inpCount;
                }

                throw new PlacementException("Did not successfully parse int.");
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

            protected override bool IsNegated()
            {
                return false;
            }

            public static IComponent Constructor(string details)
            {
                return new VarInpAndComponent(ParseInpCount(details));
            }

            public override IComponent NonStaticConstructor(string details)
            {
                return Constructor(details);
            }

            public override void RenderMainShape(Graphics graphics, bool simulating, ColourScheme colourScheme)
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

            protected override bool IsNegated()
            {
                return false;
            }

            public static IComponent Constructor(string details)
            {
                return new VarInpOrComponent(ParseInpCount(details));
            }

            public override IComponent NonStaticConstructor(string details)
            {
                return Constructor(details);
            }

            public override void RenderMainShape(Graphics graphics, bool simulating, ColourScheme colourScheme)
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

            protected override bool IsNegated()
            {
                return false;
            }

            public static IComponent Constructor(string details)
            {
                return new VarInpXorComponent(ParseInpCount(details));
            }

            public override IComponent NonStaticConstructor(string details)
            {
                return Constructor(details);
            }

            public override void RenderMainShape(Graphics graphics, bool simulating, ColourScheme colourScheme)
            {
                DrawXorComponent(graphics, colourScheme);
            }
        }

        public class VarInpNandComponent : BaseVarInpComponent
        {
            public VarInpNandComponent(int inpCount) : base(inpCount) { }

            protected override Pin.State Accumulator(Pin.State state1, Pin.State state2)
            {
                return state1.And(state2);
            }

            public static string ID = "NAND";
            public static string DefaultDetails = "2";

            public override string GetComponentID()
            {
                return ID;
            }

            protected override bool IsNegated()
            {
                return true;
            }

            public static IComponent Constructor(string details)
            {
                return new VarInpNandComponent(ParseInpCount(details));
            }

            public override IComponent NonStaticConstructor(string details)
            {
                return Constructor(details);
            }

            public override void RenderMainShape(Graphics graphics, bool simulating, ColourScheme colourScheme)
            {
                DrawAndComponent(graphics, colourScheme);
                DrawInversionCircle(graphics, new PointF(1, 0), colourScheme);
            }
        }

        public class VarInpNorComponent : BaseVarInpComponent
        {
            public VarInpNorComponent(int inpCount) : base(inpCount) { }

            protected override Pin.State Accumulator(Pin.State state1, Pin.State state2)
            {
                return state1.Or(state2);
            }

            public static string ID = "NOR";
            public static string DefaultDetails = "2";

            public override string GetComponentID()
            {
                return ID;
            }

            protected override bool IsNegated()
            {
                return true;
            }

            public static IComponent Constructor(string details)
            {
                return new VarInpNorComponent(ParseInpCount(details));
            }

            public override IComponent NonStaticConstructor(string details)
            {
                return Constructor(details);
            }

            public override void RenderMainShape(Graphics graphics, bool simulating, ColourScheme colourScheme)
            {
                DrawOrComponent(graphics, colourScheme);
                DrawInversionCircle(graphics, new PointF(1, 0), colourScheme);
            }
        }

        public class VarInpXnorComponent : BaseVarInpComponent
        {
            public VarInpXnorComponent(int inpCount) : base(inpCount) { }

            protected override Pin.State Accumulator(Pin.State state1, Pin.State state2)
            {
                return state1.Xor(state2);
            }

            public static string ID = "XNOR";
            public static string DefaultDetails = "2";

            public override string GetComponentID()
            {
                return ID;
            }

            protected override bool IsNegated()
            {
                return true;
            }

            public static IComponent Constructor(string details)
            {
                return new VarInpXnorComponent(ParseInpCount(details));
            }

            public override IComponent NonStaticConstructor(string details)
            {
                return Constructor(details);
            }

            public override void RenderMainShape(Graphics graphics, bool simulating, ColourScheme colourScheme)
            {
                DrawXorComponent(graphics, colourScheme);
                DrawInversionCircle(graphics, new PointF(1, 0), colourScheme);
            }
        }
    }

    class FixedStateComponent : InpOutpBaseComponents.NoneInpSingOutpBaseComponent, ISettingsComponent
    {
        protected Pin.State OutputState;

        protected virtual string getOutputDescriptor()
        {
            return "output";
        }

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

            throw new PlacementException("Did not successfully parse int.");
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
            stateSettingDesc = new EnumSettingDescription<Pin.State>($"What is the {getOutputDescriptor()} state for this component?", OutputState);

            return new ISettingDescription[] { stateSettingDesc };
        }

        public virtual void ApplySettings()
        {
            OutputState = stateSettingDesc.GetValue();
        }

        public override void RenderMainShape(Graphics graphics, bool simulating, ColourScheme colourScheme)
        {
            GraphicsPath path = new GraphicsPath();

            path.AddLines(new PointF[] { new PointF(-0.5F, -0.5F), new PointF(0.5F, -0.5F), new PointF(1, 0), new PointF(0.5F, 0.5F), new PointF(-0.5F, 0.5F) });
            path.CloseFigure();

            DrawComponentFromPath(graphics, path, colourScheme);
        }
    }

    /*
    class UserToggleInpComponent : FixedStateComponent, IInteractibleComponent
    {

        public override string GetComponentDetails()
        {
            return $"{(int)StartState},{(int)OtherState}";
        }

        public static new IComponent Constructor(string details)
        {
            int startState, otherState;

            string[] states = details.Split(',');

            if (int.TryParse(states[0], out startState) && int.TryParse(states[1], out otherState))
            {
                return new UserToggleInpComponent((Pin.State)startState, (Pin.State)otherState);
            }

            throw new PlacementException("Did not successfully parse int.");
        }

        public override IComponent NonStaticConstructor(string details)
        {
            return Constructor(details);
        }

        EnumSettingDescription<Pin.State> otherStateSettingDesc;

        public override ISettingDescription[] GetSettingDescriptions()
        {
            otherStateSettingDesc = new EnumSettingDescription<Pin.State>($"What is the other {getOutputDescriptor()} state for this component?", OtherState);
            return base.GetSettingDescriptions().Append(otherStateSettingDesc).ToArray();
        }

        public override void ApplySettings()
        {
            StartState = stateSettingDesc.GetValue();
            OtherState = otherStateSettingDesc.GetValue();
        }

        public new void ResetToDefault()
        {
            OutputState = StartState;
        }
    }
    //*/
    class UserToggleInpComponent : InpOutpBaseComponents.NoneInpSingOutpBaseComponent, ISettingsComponent, IInteractibleComponent
    {
        protected Pin.State OutputState, StartState, OtherState;

        protected virtual string getOutputDescriptor()
        {
            return "output";
        }

        protected EnumSettingDescription<Pin.State> startStateSettingDesc;
        protected EnumSettingDescription<Pin.State> otherStateSettingDesc;

        public override Pos GetOutpOffset()
        {
            return new Pos(2, 0);
        }

        public UserToggleInpComponent(Pin.State startState, Pin.State otherState)
        {
            OutputState = startState;

            StartState = startState;
            OtherState = otherState;
        }

        public override void Tick()
        {
            GetOutpPin().SetState(OutputState);
        }

        public void Interact()
        {
            if (OutputState == StartState)
            {
                OutputState = OtherState;
            }
            else
            {
                OutputState = StartState;
            }
        }

        public static string ID = "TOGGLE";
        public static string DefaultDetails = $"{(int)Pin.State.LOW},{(int)Pin.State.HIGH}";

        public override string GetComponentID()
        {
            return ID;
        }

        public override string GetComponentDetails()
        {
            return $"{(int)StartState},{(int)OtherState}";
        }

        public static IComponent Constructor(string details)
        {
            int startState, otherState;

            string[] states = details.Split(',');

            if (int.TryParse(states[0], out startState) && int.TryParse(states[1], out otherState))
            {
                return new UserToggleInpComponent((Pin.State)startState, (Pin.State)otherState);
            }

            throw new PlacementException("Did not successfully parse int.");
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
            startStateSettingDesc = new EnumSettingDescription<Pin.State>($"What is the starting {getOutputDescriptor()} state for this component?", StartState);
            otherStateSettingDesc = new EnumSettingDescription<Pin.State>($"What is the other {getOutputDescriptor()} state for this component?", OtherState);

            return new ISettingDescription[] { startStateSettingDesc, otherStateSettingDesc };
        }

        public virtual void ApplySettings()
        {
            StartState = startStateSettingDesc.GetValue();
            OtherState = otherStateSettingDesc.GetValue();
        }

        public override void ResetToDefault()
        {
            OutputState = StartState;
        }

        public override void RenderMainShape(Graphics graphics, bool simulating, ColourScheme colourScheme)
        {
            GraphicsPath path = new GraphicsPath();

            path.AddLines(new PointF[] { new PointF(-0.5F, -0.5F), new PointF(0.5F, -0.5F), new PointF(1, 0), new PointF(0.5F, 0.5F), new PointF(-0.5F, 0.5F) });
            path.CloseFigure();

            DrawComponentFromPath(graphics, path, colourScheme);
        }
    }

    class UserPulseInpComponent : InpOutpBaseComponents.NoneInpSingOutpBaseComponent, IInteractibleComponent, ISettingsComponent
    {
        public override Pos GetOutpOffset()
        {
            return new Pos(2, 0);
        }

        protected int PulseLength;
        protected Pin.State NormalState, PulseState;

        protected PositiveIntSettingDescription PulseLengthSettingDesc;
        protected EnumSettingDescription<Pin.State> NormalStateSettingDesc, PulseStateSettingDesc;

        public static string ID = "PULSE";
        public static string DefaultDetails = $"{20},{(int)Pin.State.LOW},{(int)Pin.State.HIGH}";

        public override string GetComponentID()
        {
            return ID;
        }

        public override string GetComponentDetails()
        {
            return $"{PulseLength},{(int)NormalState},{(int)PulseState}";
        }

        public UserPulseInpComponent(int pulseLength, Pin.State normalState, Pin.State pulseState)
        {
            PulseLength = pulseLength;
            NormalState = normalState;
            PulseState = pulseState;
        }

        private int ticksLeft = 0;

        public override void Tick()
        {
            if (ticksLeft > 0)
            {
                ticksLeft--;
                GetOutpPin().SetState(PulseState);
            } else
            {
                GetOutpPin().SetState(NormalState);
            }
        }
        public static IComponent Constructor(string details)
        {
            int pulseLength, normalState, pulseState;

            string[] detailArr = details.Split(',');

            if (int.TryParse(detailArr[0], out pulseLength) && int.TryParse(detailArr[1], out normalState) && int.TryParse(detailArr[2], out pulseState))
            {
                return new UserPulseInpComponent(pulseLength, (Pin.State)normalState, (Pin.State)pulseState);
            }

            throw new PlacementException("Did not successfully parse int.");
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
            PulseLengthSettingDesc = new PositiveIntSettingDescription("How many ticks should this component pulse for?", PulseLength);
            NormalStateSettingDesc = new EnumSettingDescription<Pin.State>("What state should this component output while not pulsing?", NormalState);
            PulseStateSettingDesc = new EnumSettingDescription<Pin.State>("What state should this component output while pulsing?", PulseState);

            return new ISettingDescription[] { PulseLengthSettingDesc, NormalStateSettingDesc, PulseStateSettingDesc };
        }

        public virtual void ApplySettings()
        {
            PulseLength = PulseLengthSettingDesc.GetValue();
            NormalState = NormalStateSettingDesc.GetValue();
            PulseState = PulseStateSettingDesc.GetValue();
        }

        public override void RenderMainShape(Graphics graphics, bool simulating, ColourScheme colourScheme)
        {
            GraphicsPath path = new GraphicsPath();

            path.AddLines(new PointF[] { new PointF(-0.5F, -0.5F), new PointF(0.5F, -0.5F), new PointF(1, 0), new PointF(0.5F, 0.5F), new PointF(-0.5F, 0.5F) });
            path.CloseFigure();

            DrawComponentFromPath(graphics, path, colourScheme);
        }

        public void Interact()
        {
            ticksLeft = PulseLength;
        }

        public override void ResetToDefault()
        {
            ticksLeft = 0;
        }
    }

    class LogicProbeComponent : InpOutpBaseComponents.SingInpNoneOutpBaseComponent, IGraphicalComponent
    {
        public override Pos GetInpOffset()
        {
            return new Pos(-2, 0);
        }

        static LogicProbeComponent()
        {
            using (Stream stream = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(stream))
                {
                    bw.Write(1F);
                    bw.Write(false);

                    stream.Position = 0;

                    using (BinaryReader br = new BinaryReader(stream))
                    {
                        DefaultDetails = ByteEncoding.Byte.GetString(br.ReadBytes((int)stream.Length));
                    }
                }
            }
        }

        public LogicProbeComponent()
        {
            DisplayFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
        }

        public override void Tick() { }

        public static string ID = "PROBE";
        public static string DefaultDetails;

        private Point? GraphicalLocation = null;
        private float GraphicalScale = 1;

        private StringFormat DisplayFormat;

        public bool HasGraphics()
        {
            return true;
        }

        public Point? GetGraphicalElementLocation()
        {
            return GraphicalLocation;
        }

        public void SetGraphicalElementLocation(Point? location)
        {
            GraphicalLocation = location;
        }

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
                    bw.Write(GraphicalScale);
                    bw.Write(GraphicalLocation.HasValue);
                    if (GraphicalLocation.HasValue)
                    {
                        bw.Write(GraphicalLocation.Value.X);
                        bw.Write(GraphicalLocation.Value.Y);
                    }

                    stream.Position = 0;

                    using (BinaryReader br = new BinaryReader(stream))
                    {
                        return ByteEncoding.Byte.GetString(br.ReadBytes((int)stream.Length));
                    }
                }
            }
        }

        public static LogicProbeComponent Constructor(string details)
        {
            using (Stream stream = new MemoryStream(ByteEncoding.Byte.GetBytes(details)))
            {
                using (BinaryReader br = new BinaryReader(stream))
                {
                    LogicProbeComponent retVal = new LogicProbeComponent();

                    retVal.GraphicalScale = br.ReadSingle();
                    if (br.ReadBoolean())
                    {
                        retVal.SetGraphicalElementLocation(new Point(br.ReadInt32(), br.ReadInt32()));
                    }

                    return retVal;
                }
            }
        }

        public override IComponent NonStaticConstructor(string details)
        {
            return Constructor(details);
        }

        public override RectangleF GetComponentBounds()
        {
            RectangleF rect = GetDefaultComponentBounds();
            rect.Inflate(1, 0.5F);
            rect.Width -= 1.5F;
            rect.X++;
            return rect;
        }

        public override void RenderMainShape(Graphics graphics, bool simulating, ColourScheme colourScheme)
        {
            GraphicsPath path = new GraphicsPath();

            path.AddLines(new PointF[] { new PointF(0.5F, -0.5F), new PointF(-0.5F, -0.5F), new PointF(-1, 0), new PointF(-0.5F, 0.5F), new PointF(0.5F, 0.5F) });
            path.CloseFigure();

            DrawComponentFromPath(graphics, path, colourScheme);
        }

        public void RenderGraphicalElement(Graphics graphics, bool simulating, ColourScheme colourScheme)
        {
            char display;
            Color colour;

            if (simulating)
            {
                Pin.State state = GetInpPin().GetStateForDisplay();

                display = state.ToString()[0];
                colour = colourScheme.GetWireColour(state);
            } else
            {
                display = '?';
                colour = colourScheme.Wire;
            }

            graphics.DrawString($"{display}", new Font("arial", 0.5F), new SolidBrush(colour), 0, 0, DisplayFormat);
        }

        public RectangleF GetGraphicalElementBounds()
        {
            return new RectangleF(-0.5F, -0.5F, 1, 1);
        }

        public float GetGraphicalElementScale()
        {
            return GraphicalScale;
        }

        public void SetGraphicalElementScale(float scale)
        {
            GraphicalScale = scale;
        }
    }

    class SevenSegmentComponent : InpOutpBaseComponents.MultInpNoneOutpBaseComponent, IGraphicalComponent
    {
        public override RectangleF GetComponentBounds()
        {
            return new Rectangle(-4, -4, 7, 8);
        }

        public override string GetComponentDetails()
        {
            using (Stream stream = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(stream))
                {
                    bw.Write(GraphicalScale);
                    bw.Write(GraphicalLocation.HasValue);
                    if (GraphicalLocation.HasValue)
                    {
                        bw.Write(GraphicalLocation.Value.X);
                        bw.Write(GraphicalLocation.Value.Y);
                    }

                    stream.Position = 0;

                    using (BinaryReader br = new BinaryReader(stream))
                    {
                        return ByteEncoding.Byte.GetString(br.ReadBytes((int)stream.Length));
                    }
                }
            }
        }

        static SevenSegmentComponent()
        {
            using (Stream stream = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(stream))
                {
                    bw.Write(1F);
                    bw.Write(false);

                    stream.Position = 0;

                    using (BinaryReader br = new BinaryReader(stream))
                    {
                        DefaultDetails = ByteEncoding.Byte.GetString(br.ReadBytes((int)stream.Length));
                    }
                }
            }
        }

        public static string DefaultDetails;
        public static string ID = "7SEG";

        public override string GetComponentID()
        {
            return ID;
        }

        public RectangleF GetGraphicalElementBounds()
        {
            return new RectangleF(-2.25F, -3.75F, 4.5F, 7.5F);
        }

        public float GetGraphicalElementScale()
        {
            return GraphicalScale;
        }

        public void SetGraphicalElementScale(float scale)
        {
            GraphicalScale = scale;
        }

        private Point? GraphicalLocation = null;
        private float GraphicalScale = 1;

        public Point? GetGraphicalElementLocation()
        {
            return GraphicalLocation;
        }

        public override Pos[] GetInpOffsets()
        {
            return new Pos[]
            {
                new Pos(-4, -3),
                new Pos(-4, -2),
                new Pos(-4, -1),
                new Pos(-4, 0),
                new Pos(-4, 1),
                new Pos(-4, 2),
                new Pos(-4, 3)
            };
        }

        public bool HasGraphics()
        {
            return true;
        }

        public static SevenSegmentComponent Constructor(string details)
        {
            using (Stream stream = new MemoryStream(ByteEncoding.Byte.GetBytes(details)))
            {
                using (BinaryReader br = new BinaryReader(stream))
                {
                    SevenSegmentComponent retVal = new SevenSegmentComponent();

                    retVal.GraphicalScale = br.ReadSingle();
                    if (br.ReadBoolean())
                    {
                        retVal.SetGraphicalElementLocation(new Point(br.ReadInt32(), br.ReadInt32()));
                    }

                    return retVal;
                }
            }
        }

        public override IComponent NonStaticConstructor(string details)
        {
            return Constructor(details);
        }

        private PointF[] GetDiamond(PointF around)
        {
            float dist = 0.75F;

            return new PointF[]
            {
                new PointF(around.X - dist, around.Y),
                new PointF(around.X, around.Y - dist),
                new PointF(around.X + dist, around.Y),
                new PointF(around.X, around.Y + dist)
            };
        }

        private void DrawSegment(Graphics graphics, bool simulating, ColourScheme colourScheme, PointF point1, PointF point2, Pin.State state)
        {
            GraphicsPath path;

            Brush brush = new SolidBrush(colourScheme.GetWireColour(state));

            PointF[] dia1 = GetDiamond(point1), dia2 = GetDiamond(point2);

            path = new GraphicsPath();
            path.AddLines(dia1);
            path.CloseFigure();
            graphics.FillPath(brush, path);

            path = new GraphicsPath();
            path.AddLines(dia2);
            path.CloseFigure();
            graphics.FillPath(brush, path);

            path = new GraphicsPath();
            path.AddLines(new PointF[]
            {
                dia1[0],
                dia1[2],
                dia2[2],
                dia2[0]
            });
            path.CloseFigure();
            graphics.FillPath(brush, path);

            path = new GraphicsPath();
            path.AddLines(new PointF[]
            {
                dia1[1],
                dia1[3],
                dia2[3],
                dia2[1]
            });
            path.CloseFigure();
            graphics.FillPath(brush, path);
        }

        public void RenderGraphicalElement(Graphics graphics, bool simulating, ColourScheme colourScheme)
        {
            Pin.State[] states;
            if (simulating)
            {
                states = GetInpPins().Select(pin => pin.GetStateForDisplay()).ToArray();
            } else
            {
                states = new Pin.State[7];
            }

            DrawSegment(graphics, simulating, colourScheme, new PointF(-0.5F, -3),  new PointF(0.5F, -3),   states[0]); // a
            DrawSegment(graphics, simulating, colourScheme, new PointF(1.5F, -1),   new PointF(1.5F, -2),   states[1]); // b
            DrawSegment(graphics, simulating, colourScheme, new PointF(1.5F, 1),    new PointF(1.5F, 2),    states[2]); // c
            DrawSegment(graphics, simulating, colourScheme, new PointF(-0.5F, 3),   new PointF(0.5F, 3),    states[3]); // d
            DrawSegment(graphics, simulating, colourScheme, new PointF(-1.5F, 1),   new PointF(-1.5F, 2),   states[4]); // e
            DrawSegment(graphics, simulating, colourScheme, new PointF(-1.5F, -1),  new PointF(-1.5F, -2),  states[5]); // f
            DrawSegment(graphics, simulating, colourScheme, new PointF(-0.5F, 0),   new PointF(0.5F, 0),    states[6]); // g
        }

        public override void RenderMainShape(Graphics graphics, bool simulating, ColourScheme colourScheme)
        {
            RectangleF bounds = GetComponentBounds();

            GraphicsPath path = new GraphicsPath();
            path.AddLines(new PointF[]
            {
                new PointF(bounds.Left + 1, bounds.Top),
                new PointF(bounds.Left + 1, bounds.Bottom),
                new PointF(bounds.Right, bounds.Bottom),
                new PointF(bounds.Right, bounds.Top)
            });
            path.CloseFigure();

            DrawComponentFromPath(graphics, path, colourScheme);
        }

        public void SetGraphicalElementLocation(Point? location)
        {
            GraphicalLocation = location;
        }

        public override void Tick() { }
    }

    abstract class BoardContainerComponents
    {
        public class BoardInputComponent : UserToggleInpComponent, IBoardInputComponent
        {
            private string ComponentName;
            private Pin externalPin = null;

            private NameSettingDescription nameSettingDesc;

            private Board.InterfaceLocation interfaceLocation;

            protected override string getOutputDescriptor()
            {
                return "default output";
            }

            public Board.InterfaceLocation GetInterfaceLocation()
            {
                return interfaceLocation;
            }

            public void SetInterfaceLocation(Board.InterfaceLocation location)
            {
                interfaceLocation = location;
            }

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

            public BoardInputComponent(string name, Pin.State startDefaultState, Pin.State otherDefaultState, Board.InterfaceLocation interfaceLocation) : base(startDefaultState, otherDefaultState)
            {
                ComponentName = name;

                this.interfaceLocation = interfaceLocation;
            }

            public new static string ID = "INPUT";
            public new static string DefaultDetails = $"INPUT,{(int)Pin.State.LOW},{(int)Pin.State.HIGH},{(byte)Board.InterfaceLocation.SideEnum.Left},{0}";

            public override string GetComponentID()
            {
                return ID;
            }

            public override string GetComponentDetails()
            {
                return $"{ComponentName},{(int)StartState},{(int)OtherState},{(byte)interfaceLocation.Side},{interfaceLocation.Distance}";
            }

            public void SetExternalPin(Pin pin)
            {
                externalPin = pin;
                OutputState = StartState;
            }

            public void RemoveExternalPin()
            {
                externalPin = null;
                OutputState = StartState;
            }

            public override void Tick()
            {
                if (externalPin != null)
                {
                    OutputState = externalPin.GetStateForWireComponent();
                }

                base.Tick();
            }

            public new static BoardInputComponent Constructor(string details)
            {
                string[] strings = details.Split(',');

                if (int.TryParse(strings[1], out int startDefaultState) && int.TryParse(strings[2], out int otherDefaultState) && int.TryParse(strings[3], out int sideInt) && int.TryParse(strings[4], out int distInt))
                {
                    return new BoardInputComponent(strings[0], (Pin.State)startDefaultState, (Pin.State)otherDefaultState, new Board.InterfaceLocation((Board.InterfaceLocation.SideEnum)(byte)sideInt, distInt));
                }

                throw new PlacementException("Did not successfully parse int.");
            }

            public override IComponent NonStaticConstructor(string details)
            {
                return Constructor(details);
            }

            public override ISettingDescription[] GetSettingDescriptions()
            {
                nameSettingDesc = new NameSettingDescription("What is this component called?", ComponentName);

                return (new ISettingDescription[] { nameSettingDesc }).Concat(base.GetSettingDescriptions()).ToArray();
            }

            public override void ApplySettings()
            {
                base.ApplySettings();

                ComponentName = nameSettingDesc.GetValue();
            }

            public override RectangleF GetComponentBounds()
            {
                RectangleF rect = base.GetComponentBounds();
                rect.Width++;
                rect.X--;
                return rect;
            }

            public override void Render(Graphics graphics, bool simulating, ColourScheme colourScheme)
            {
                base.Render(graphics, simulating, colourScheme);

                graphics.DrawString(ComponentName, new Font("arial", 0.5F), Brushes.Black, -1, -0.25F);
            }

            public override void RenderMainShape(Graphics graphics, bool simulating, ColourScheme colourScheme)
            {
                GraphicsPath path = new GraphicsPath();

                path.AddLines(new PointF[] { new PointF(-1.5F, -0.5F), new PointF(0.5F, -0.5F), new PointF(1, 0), new PointF(0.5F, 0.5F), new PointF(-1.5F, 0.5F) });
                path.CloseFigure();

                DrawComponentFromPath(graphics, path, colourScheme);
            }
        }

        public class BoardOutputComponent : InpOutpBaseComponents.SingInpNoneOutpBaseComponent, IBoardOutputComponent, ISettingsComponent
        {
            private string ComponentName;
            //protected Pin.State State;
            private Pin externalPin = null;

            private NameSettingDescription nameSettingDesc;

            private Board.InterfaceLocation interfaceLocation;

            public Board.InterfaceLocation GetInterfaceLocation()
            {
                return interfaceLocation;
            }

            public void SetInterfaceLocation(Board.InterfaceLocation location)
            {
                interfaceLocation = location;
            }

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

            public BoardOutputComponent(string name, Board.InterfaceLocation interfaceLocation)
            {
                ComponentName = name;

                this.interfaceLocation = interfaceLocation;
            }

            public static string ID = "OUTPUT";
            public static string DefaultDetails = $"OUTPUT,{(byte)Board.InterfaceLocation.SideEnum.Right},{0}";

            public override string GetComponentID()
            {
                return ID;
            }

            public override string GetComponentDetails()
            {
                return $"{ComponentName},{(byte)interfaceLocation.Side},{interfaceLocation.Distance}";
            }

            public static BoardOutputComponent Constructor(string details)
            {
                string[] strings = details.Split(',');

                if (int.TryParse(strings[1], out int sideInt) && int.TryParse(strings[2], out int distInt))
                {
                    return new BoardOutputComponent(strings[0], new Board.InterfaceLocation((Board.InterfaceLocation.SideEnum)(byte)sideInt, distInt));
                }

                throw new PlacementException("Did not successfully parse int.");
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

            public override void Tick()
            {
                if (externalPin != null)
                {
                    externalPin.SetState(GetInpPin().GetStateForWireComponent());
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

            public override void RenderMainShape(Graphics graphics, bool simulating, ColourScheme colourScheme)
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

            private Board.InterfaceLocation interfaceLocation;

            public Board.InterfaceLocation GetInterfaceLocation()
            {
                return interfaceLocation;
            }

            public void SetInterfaceLocation(Board.InterfaceLocation location)
            {
                interfaceLocation = location;
            }

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

            public BoardBidirComponent(string name, Pin.State defaultState, Board.InterfaceLocation interfaceLocation)
            {
                ComponentName = name;

                DefaultExternalState = defaultState;

                this.interfaceLocation = interfaceLocation;
            }

            public static string ID = "BIDIR";
            public static string DefaultDetails = $"BIDIR,{(int)Pin.State.LOW},{(byte)Board.InterfaceLocation.SideEnum.Top},{0}";

            public override string GetComponentID()
            {
                return ID;
            }

            public override string GetComponentDetails()
            {
                return $"{ComponentName},{(int)DefaultExternalState},{(byte)interfaceLocation.Side},{interfaceLocation.Distance}";
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
                    GetOutpPin().SetState(GetInpPin().GetStateForWireComponent().WireJoin(DefaultExternalState));
                } else
                {
                    Pin.State state = GetInpPin().GetStateForWireComponent().WireJoin(externalPin.GetStateForWireComponent());

                    GetOutpPin().SetState(state);
                    externalPin.SetState(state);
                }
            }

            public static BoardBidirComponent Constructor(string details)
            {
                string[] strings = details.Split(',');

                if (int.TryParse(strings[1], out int stateInt) && int.TryParse(strings[2], out int sideInt) && int.TryParse(strings[3], out int distInt))
                {
                    return new BoardBidirComponent(strings[0], (Pin.State)stateInt, new Board.InterfaceLocation((Board.InterfaceLocation.SideEnum)sideInt, distInt));
                }

                throw new PlacementException("Did not successfully parse int.");
            }

            public override IComponent NonStaticConstructor(string details)
            {
                return Constructor(details);
            }

            public ISettingDescription[] GetSettingDescriptions()
            {
                nameSettingDesc = new NameSettingDescription("What is this component called?", ComponentName);
                defaultStateSettingDesc = new EnumSettingDescription<Pin.State>("What is the simulated external state of this component?", DefaultExternalState);

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
                rect.Height -= 0.5F;
                rect.Y -= 0.5F;
                return rect;
            }

            public override void Render(Graphics graphics, bool simulating, ColourScheme colourScheme)
            {
                InpOutpBaseComponents.InpOutpTools.DrawInpOutpLine(graphics, simulating, offset, new PointF(offset.X, offset.Y - 1.5F), colourScheme, this);

                RenderMainShape(graphics, simulating, colourScheme);

                graphics.DrawString(ComponentName, new Font("arial", 0.5F), Brushes.Black, -1, -0.25F);
            }

            public override void RenderMainShape(Graphics graphics, bool simulating, ColourScheme colourScheme)
            {
                GraphicsPath path = new GraphicsPath();

                path.AddLines(new PointF[] { new PointF(-0.5F, -1.5F), new PointF(-0.5F, 0.5F), new PointF(0, 1), new PointF(0.5F, 0.5F), new PointF(0.5F, -1.5F) });
                path.CloseFigure();

                DrawComponentFromPath(graphics, path, colourScheme);
            }
        }

        public class BoardContainerComponent : InpOutpBaseComponents.MultInpMultOutpBaseComponent, IBoardContainerComponent
        {
            private string InternalBoardName;
            public Board InternalBoard { get; private set; }

            private Rectangle Shape;

            private Pos[] InpOffsets;
            private string[] InpNames;

            private Pos[] OutpOffsets;
            private string[] OutpNames;

            private static StringFormat LeftStringFormat,  RightStringFormat,  TopStringFormat, BottomStringFormat;
            private static Dictionary<Board.InterfaceLocation.SideEnum, StringFormat> StringFormats;

            private Point? GraphicalLocation = null;
            private float GraphicalScale = 1;

            public override Pos[] GetInpOffsets()
            {
                return InpOffsets.ToArray();
            }

            public override Pos[] GetOutpOffsets()
            {
                return OutpOffsets.ToArray();
            }

            static BoardContainerComponent()
            {
                LeftStringFormat = new StringFormat();
                RightStringFormat = new StringFormat();
                TopStringFormat = new StringFormat();
                BottomStringFormat = new StringFormat();

                LeftStringFormat.Alignment = StringAlignment.Near;
                TopStringFormat.Alignment = StringAlignment.Center;
                BottomStringFormat.Alignment = StringAlignment.Center;
                RightStringFormat.Alignment = StringAlignment.Far;

                TopStringFormat.LineAlignment = StringAlignment.Near;
                LeftStringFormat.LineAlignment = StringAlignment.Center;
                RightStringFormat.LineAlignment = StringAlignment.Center;
                BottomStringFormat.LineAlignment = StringAlignment.Far;

                StringFormats = new Dictionary<Board.InterfaceLocation.SideEnum, StringFormat>
                {
                    { Board.InterfaceLocation.SideEnum.Left, LeftStringFormat },
                    { Board.InterfaceLocation.SideEnum.Right, RightStringFormat },
                    { Board.InterfaceLocation.SideEnum.Top, TopStringFormat },
                    { Board.InterfaceLocation.SideEnum.Bottom, BottomStringFormat }
                };
            }

            private void ProvideInternalBoard(Board board)
            {
                if (InternalBoard == null)
                {
                    InternalBoard = board;
                    InternalBoardName = board.Name;

                    Initialize();

                    if (IsPlaced())
                    {
                        PlaceForInternalBoard();
                    }
                }
            }

            public BoardContainerComponent(string boardName)
            {
                InternalBoardName = boardName;
                ReadWriteImplementation.PromiseBoard(boardName, ProvideInternalBoard);
            }

            public BoardContainerComponent(Board internalBoard, bool copy = true)
            {
                InternalBoard = copy ? internalBoard.Copy() : internalBoard;
                InternalBoardName = internalBoard.Name;

                Initialize();
            }

            private List<Action<IComponent>> detailsProviders = new List<Action<IComponent>>();

            public void PromiseDetails(Action<IComponent> detailsProvider)
            {
                detailsProviders.Add(detailsProvider);
            }

            private void Initialize()
            {
                InternalBoard.SizeChanged += InternalBoard_SizeChanged;

                ResetShape();

                IBoardInterfaceComponent[] interfaceComps = InternalBoard.GetInterfaceComponents();

                List<Pos> inpOffsetList = new List<Pos>(), outpOffsetList = new List<Pos>();
                List<string> inpNameList = new List<string>(), outpNameList = new List<string>();

                Board.InterfaceLocation interfaceLocation;
                Pos offset;
                (Pos, Pos) offsetInfo;

                foreach (IBoardInterfaceComponent interfaceComp in interfaceComps)
                {
                    interfaceLocation = interfaceComp.GetInterfaceLocation();

                    offsetInfo = GetOffset(interfaceLocation);
                    offset = offsetInfo.Item1.Add(offsetInfo.Item2);

                    if (interfaceComp is IBoardInputComponent)
                    {
                        inpOffsetList.Add(offset);
                        inpNameList.Add(interfaceComp.GetComponentName());
                    }

                    if (interfaceComp is IBoardOutputComponent)
                    {
                        outpOffsetList.Add(offset);
                        outpNameList.Add(interfaceComp.GetComponentName());
                    }
                }

                InpOffsets = inpOffsetList.ToArray();
                InpNames = inpNameList.ToArray();
                OutpOffsets = outpOffsetList.ToArray();
                OutpNames = outpNameList.ToArray();

                while (detailsProviders.Count > 0)
                {
                    detailsProviders[0](this);

                    detailsProviders.RemoveAt(0);
                }
            }

            private void InternalBoard_SizeChanged()
            {
                ResetShape();
            }

            public void ResetShape()
            {
                Shape = new Rectangle(-InternalBoard.ExternalSize.Width / 2, -InternalBoard.ExternalSize.Height / 2, InternalBoard.ExternalSize.Width, InternalBoard.ExternalSize.Height);
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
                        bw.Write(InternalBoard.Name);
                        bw.Write(GraphicalScale);
                        bw.Write(GraphicalLocation.HasValue);
                        if (GraphicalLocation.HasValue)
                        {
                            bw.Write(GraphicalLocation.Value.X);
                            bw.Write(GraphicalLocation.Value.Y);
                        }

                        stream.Position = 0;

                        using (BinaryReader br = new BinaryReader(stream))
                        {
                            return ByteEncoding.Byte.GetString(br.ReadBytes((int)stream.Length));
                        }
                    }
                }
            }

            private (Pos, Pos) GetOffset(Board.InterfaceLocation interfaceLocation)
            {
                if (interfaceLocation.Side == Board.InterfaceLocation.SideEnum.Top)
                {
                    return (new Pos(Shape.Left + interfaceLocation.Distance, Shape.Top), new Pos(0, -1));
                }
                else if (interfaceLocation.Side == Board.InterfaceLocation.SideEnum.Bottom)
                {
                    return (new Pos(Shape.Left + interfaceLocation.Distance, Shape.Bottom), new Pos(0, 1));
                }
                else if (interfaceLocation.Side == Board.InterfaceLocation.SideEnum.Left)
                {
                    return (new Pos(Shape.Left, Shape.Top + interfaceLocation.Distance), new Pos(-1, 0));
                }
                else if (interfaceLocation.Side == Board.InterfaceLocation.SideEnum.Right)
                {
                    return (new Pos(Shape.Right, Shape.Top + interfaceLocation.Distance), new Pos(1, 0));
                }

                return (new Pos(), new Pos());
            }

            public override void Place(Pos pos, Rotation rotation, Board board)
            {
                base.Place(pos, rotation, board);

                if (InternalBoard != null)
                {
                    PlaceForInternalBoard();
                }
            }

            private void PlaceForInternalBoard()
            {
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

                InternalBoard.SetOwnerBoard(GetComponentBoard());
            }

            public override void Remove()
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

                InternalBoard.ResetOwnerBoard();
            }

            public override void Tick()
            {
                InternalBoard.TickComponents();
            }

            public static BoardContainerComponent Constructor(string details)
            {
                using (Stream stream = new MemoryStream(ByteEncoding.Byte.GetBytes(details)))
                {
                    using (BinaryReader br = new BinaryReader(stream))
                    {
                        BoardContainerComponent retVal = new BoardContainerComponent(br.ReadString());

                        retVal.GraphicalScale = br.ReadSingle();

                        if (br.ReadBoolean())
                        {
                            retVal.SetGraphicalElementLocation(new Point(br.ReadInt32(), br.ReadInt32()));
                        }

                        return retVal;
                    }
                }
            }

            public override IComponent NonStaticConstructor(string details)
            {
                return Constructor(details);
            }

            public new IComponent Copy()
            {
                if (InternalBoard != null)
                {
                    return new BoardContainerComponent(InternalBoard);
                }

                return new BoardContainerComponent(InternalBoardName);
            }

            public override RectangleF GetComponentBounds()
            {
                Pos[] offsets = new Pos[] { new Pos(0, 0), new Pos(Shape.Left, Shape.Top), new Pos(Shape.Right, Shape.Bottom) };
                
                if (InternalBoard != null)
                {
                    offsets = offsets.Concat(GetAllPinOffsets()).ToArray();
                }

                RectangleF rect = CreateSmallestRect(offsets);
                return rect;
            }

            public override void Render(Graphics graphics, bool simulating, ColourScheme colourScheme)
            {
                Pos otherOffset;
                (Pos, Pos) offsetInfo;

                foreach (IBoardInterfaceComponent interfaceComp in InternalBoard.GetInterfaceComponents())
                {
                    offsetInfo = GetOffset(interfaceComp.GetInterfaceLocation());

                    otherOffset = offsetInfo.Item1.Add(new Pos(-offsetInfo.Item2.X, -offsetInfo.Item2.Y));

                    InpOutpBaseComponents.InpOutpTools.DrawInpOutpLine(graphics, simulating, offsetInfo.Item1.Add(offsetInfo.Item2), new PointF(otherOffset.X, otherOffset.Y), colourScheme, this);
                }

                RenderMainShape(graphics, simulating, colourScheme);

                RenderGraphicalElement(graphics, simulating, colourScheme);
            }

            public override void RenderMainShape(Graphics graphics, bool simulating, ColourScheme colourScheme)
            {
                GraphicsPath path = new GraphicsPath();
                path.AddRectangle(Shape);
                path.CloseFigure();

                DrawComponentFromPath(graphics, path, colourScheme);

                (Pos, Pos) offsetInfo;

                foreach (IBoardInterfaceComponent interfaceComp in InternalBoard.GetInterfaceComponents())
                {
                    Board.InterfaceLocation interfaceLocation = interfaceComp.GetInterfaceLocation();
                    offsetInfo = GetOffset(interfaceLocation);

                    graphics.DrawString(interfaceComp.GetComponentName(), new Font("arial", 0.25F), Brushes.Black, new Point(offsetInfo.Item1.X, offsetInfo.Item1.Y), StringFormats[interfaceLocation.Side]);
                }
            }

            public void RenderGraphicalElement(Graphics graphics, bool simulating, ColourScheme colourScheme)
            {
                Matrix matrix;
                PointF? loc;
                float scale;

                foreach (IGraphicalComponent graphicalComp in InternalBoard.GetGraphicalComponents().Where(comp => comp.HasGraphics()))
                {
                    loc = graphicalComp.GetGraphicalElementLocation();
                    scale = graphicalComp.GetGraphicalElementScale();

                    if (loc.HasValue)
                    {
                        using (new TransformRestorer(graphics))
                        {
                            matrix = new Matrix();
                            matrix.Translate(loc.Value.X, loc.Value.Y);
                            matrix.Scale(scale, scale);

                            graphics.MultiplyTransform(matrix);

                            graphicalComp.RenderGraphicalElement(graphics, simulating, colourScheme);
                        }

                        //matrix.Invert();
                        //graphics.MultiplyTransform(matrix);
                    }
                }
            }

            public Rectangle GetShape()
            {
                return Shape;
            }

            public Board GetInternalBoard()
            {
                return InternalBoard;
            }

            public bool HasGraphics()
            {
                return InternalBoard.GetGraphicalComponents().Where(comp => comp.HasGraphics()).Any();
            }

            public Point? GetGraphicalElementLocation()
            {
                return GraphicalLocation;
            }

            public void SetGraphicalElementLocation(Point? point)
            {
                GraphicalLocation = point;
            }

            public RectangleF GetGraphicalElementBounds()
            {
                RectangleF rect = new RectangleF(), compRect;

                foreach (IGraphicalComponent graphicalComp in InternalBoard.GetGraphicalComponents())
                {
                    compRect = graphicalComp.GetGraphicalElementBounds();

                    rect = RectangleF.FromLTRB(
                        Math.Min(rect.Left, compRect.Left),
                        Math.Min(rect.Top, compRect.Top),
                        Math.Max(rect.Right, compRect.Right),
                        Math.Max(rect.Bottom, compRect.Bottom)
                    );
                }

                return rect;
            }

            public float GetGraphicalElementScale()
            {
                return GraphicalScale;
            }

            public void SetGraphicalElementScale(float scale)
            {
                GraphicalScale = scale;
            }

            public override void ResetToDefault()
            {
                InternalBoard.ResetForSimulation();
            }
        }
    } 
}
