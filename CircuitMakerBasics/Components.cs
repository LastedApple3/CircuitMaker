using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CircuitMaker.Basics;
using System.Drawing;
using System.Drawing.Drawing2D;

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

            ReadWriteImplementation.Constructors.Add(BoardContainerComponents.BoardInputComponent.ID, BoardContainerComponents.BoardInputComponent.Constructor);
            ReadWriteImplementation.Constructors.Add(BoardContainerComponents.BoardOutputComponent.ID, BoardContainerComponents.BoardOutputComponent.Constructor);
            ReadWriteImplementation.Constructors.Add(BoardContainerComponents.BoardContainerComponent.ID, BoardContainerComponents.BoardContainerComponent.Constructor);
        }
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

        public Rotation GetRotation()
        {
            return ComponentRotation;
        }

        public Board GetComponentBoard()
        {
            return ComponentBoard;
        }

        public bool IsPlaced { get; private set; } = false;

        public void Place(Pos pos, Board board)
        {
            Place(pos, Rotation.ZERO, board);
        }

        public void Place(Pos pos, Rotation rotation, Board board)
        {
            if (IsPlaced)
            {
                throw new Exception("Already placed, can't place again.");
            }

            ComponentPos = pos;
            ComponentRotation = rotation;
            ComponentBoard = board;

            board.AddComponent(this);

            IsPlaced = true;
        }

        public void Remove() // can be removed if not placed
        {
            if (IsPlaced)
            {
                ComponentBoard.RemoveComponent(this);
            }

            IsPlaced = false;
        }

        public abstract void Tick();

        public abstract Pos[] GetAllPinOffsets();
        public abstract Pos[] GetAllPinPositions();

        public abstract string GetComponentID();
        public abstract string GetComponentDetails();

        public abstract IComponent NonStaticConstructor(string details);

        public abstract void Render(Graphics graphics, ColourScheme colourScheme);

        /*
        public void Render(Graphics graphics)
        {
            graphics.DrawString(GetComponentID() + ":" + GetComponentDetails(), new Font("arial", 0.1F), Brushes.Black, new Point(0, 0));

            foreach (Pos pinPos in GetAllPinOffsets())
            {
                float rad = 0.1F;
                graphics.FillEllipse(Brushes.Black, pinPos.X - rad, pinPos.Y - rad, 2 * rad, 2 * rad);
                graphics.DrawLine(new Pen(Color.FromArgb(128, Color.Black), 0.1F), new PointF(), new PointF(pinPos.X, pinPos.Y));
            }

            graphics.DrawRectangle(new Pen(Color.Black, 0.01F), GetComponentBounds());
        }//*/

        public abstract void RenderMainShape(Graphics graphics, ColourScheme colourScheme);/*
        public void RenderMainShape(Graphics graphics, ColourScheme colourScheme)
        {
            Rectangle rect = GetDefaultComponentBounds();
            rect.Inflate(-1, -1);

            graphics.FillRectangle(new SolidBrush(colourScheme.ComponentBackground), rect);
            graphics.DrawRectangle(new Pen(colourScheme.ComponentEdge), rect);
        }
        //*/

        protected void DrawComponentFromPath(Graphics graphics, GraphicsPath path, ColourScheme colourScheme)
        {
            graphics.FillPath(new SolidBrush(colourScheme.ComponentBackground), path);
            graphics.DrawPath(new Pen(colourScheme.ComponentEdge, 0.01F), path);
        }

        public abstract Rectangle GetComponentBounds();

        //*
        protected Rectangle GetDefaultComponentBounds()
        {
            Pos[] offsets = GetAllPinOffsets().Append(new Pos(0, 0)).ToArray();

            return Rectangle.FromLTRB(
                offsets.Select(offset => offset.X).Aggregate(Math.Min),
                offsets.Select(offset => offset.Y).Aggregate(Math.Min),
                offsets.Select(offset => offset.X).Aggregate(Math.Max),
                offsets.Select(offset => offset.Y).Aggregate(Math.Max));
        }//*/

        public Rectangle GetOffsetComponentBounds()
        {
            Rectangle rect = GetComponentBounds();
            Pos pos = GetComponentPos();

            rect.Offset(pos.X, pos.Y);

            return rect;
        }

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
        private abstract class InpOutpTools
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


            public static void DrawInpLine(Graphics graphics, Pos inpOffset, Color wireColour)
            {
                graphics.DrawLine(new Pen(wireColour, 0.01F), inpOffset.X, inpOffset.Y, inpOffset.X + 1, inpOffset.Y);
            }

            public static void DrawOutpLine(Graphics graphics, Pos outpOffset, Color wireColour)
            {
                graphics.DrawLine(new Pen(wireColour, 0.01F), outpOffset.X, outpOffset.Y, outpOffset.X - 1, outpOffset.Y);
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
                return InpOutpTools.ApplySingRotation(GetRotation(), GetInpOffset());
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



            public override void Render(Graphics graphics, ColourScheme colourScheme)
            {
                InpOutpTools.DrawInpLine(graphics, GetInpOffset(), colourScheme.Wire);

                RenderMainShape(graphics, colourScheme);
            }
        }

        public abstract class MultInpNoneOutpBaseComponent : BaseComponent, IMultInpComponent
        {
            public abstract Pos[] GetInpOffsets();

            public Pos[] GetRotatedInpOffsets()
            {
                return InpOutpTools.ApplyMultRotation(GetRotation(), GetInpOffsets());
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



            public override void Render(Graphics graphics, ColourScheme colourScheme)
            {
                foreach (Pos inpOffset in GetInpOffsets())
                {
                    InpOutpTools.DrawInpLine(graphics, inpOffset, colourScheme.Wire);
                }

                RenderMainShape(graphics, colourScheme);
            }
        }

        public abstract class NoneInpSingOutpBaseComponent : BaseComponent, ISingOutpComponent
        {
            public abstract Pos GetOutpOffset();

            public Pos GetRotatedOutpOffset()
            {
                return InpOutpTools.ApplySingRotation(GetRotation(), GetOutpOffset());
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



            public override void Render(Graphics graphics, ColourScheme colourScheme)
            {
                InpOutpTools.DrawOutpLine(graphics, GetOutpOffset(), colourScheme.Wire);

                RenderMainShape(graphics, colourScheme);
            }
        }

        public abstract class NoneInpMultOutpBaseComponent : BaseComponent, IMultOutpComponent
        {
            public abstract Pos[] GetOutpOffsets();

            public Pos[] GetRotatedOutpOffsets()
            {
                return InpOutpTools.ApplyMultRotation(GetRotation(), GetOutpOffsets());
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



            public override void Render(Graphics graphics, ColourScheme colourScheme)
            {
                foreach (Pos outpOffset in GetOutpOffsets())
                {
                    InpOutpTools.DrawOutpLine(graphics, outpOffset, colourScheme.Wire);
                }

                RenderMainShape(graphics, colourScheme);
            }
        }


        public abstract class SingInpSingOutpBaseComponent : BaseComponent, ISingInpSingOutpComponent
        {
            public abstract Pos GetInpOffset();

            public Pos GetRotatedInpOffset()
            {
                return InpOutpTools.ApplySingRotation(GetRotation(), GetInpOffset());
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
                return InpOutpTools.ApplySingRotation(GetRotation(), GetOutpOffset());
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



            public override void Render(Graphics graphics, ColourScheme colourScheme)
            {
                InpOutpTools.DrawInpLine(graphics, GetInpOffset(), colourScheme.Wire);

                InpOutpTools.DrawOutpLine(graphics, GetOutpOffset(), colourScheme.Wire);

                RenderMainShape(graphics, colourScheme);
            }
        }

        public abstract class SingInpMultOutpBaseComponent : BaseComponent, ISingInpMultOutpComponent
        {
            public abstract Pos GetInpOffset();

            public Pos GetRotatedInpOffset()
            {
                return InpOutpTools.ApplySingRotation(GetRotation(), GetInpOffset());
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
                return InpOutpTools.ApplyMultRotation(GetRotation(), GetOutpOffsets());
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



            public override void Render(Graphics graphics, ColourScheme colourScheme)
            {
                InpOutpTools.DrawInpLine(graphics, GetInpOffset(), colourScheme.Wire);

                foreach (Pos outpOffset in GetOutpOffsets())
                {
                    InpOutpTools.DrawOutpLine(graphics, outpOffset, colourScheme.Wire);
                }

                RenderMainShape(graphics, colourScheme);
            }
        }

        public abstract class MultInpSingOutpBaseComponent : BaseComponent, IMultInpSingOutpComponent
        {
            public abstract Pos[] GetInpOffsets();

            public Pos[] GetRotatedInpOffsets()
            {
                return InpOutpTools.ApplyMultRotation(GetRotation(), GetInpOffsets());
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
                return InpOutpTools.ApplySingRotation(GetRotation(), GetOutpOffset());
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



            public override void Render(Graphics graphics, ColourScheme colourScheme)
            {
                foreach (Pos inpOffset in GetInpOffsets())
                {
                    InpOutpTools.DrawInpLine(graphics, inpOffset, colourScheme.Wire);
                }

                InpOutpTools.DrawOutpLine(graphics, GetOutpOffset(), colourScheme.Wire);

                RenderMainShape(graphics, colourScheme);
            }
        }

        public abstract class MultInpMultOutpBaseComponent : BaseComponent, IMultInpMultOutpComponent
        {
            public abstract Pos[] GetInpOffsets();

            public Pos[] GetRotatedInpOffsets()
            {
                return InpOutpTools.ApplyMultRotation(GetRotation(), GetInpOffsets());
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
                return InpOutpTools.ApplyMultRotation(GetRotation(), GetOutpOffsets());
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



            public override void Render(Graphics graphics, ColourScheme colourScheme)
            {
                foreach (Pos inpOffset in GetInpOffsets())
                {
                    InpOutpTools.DrawInpLine(graphics, inpOffset, colourScheme.Wire);
                }

                foreach (Pos outpOffset in GetOutpOffsets())
                {
                    InpOutpTools.DrawOutpLine(graphics, outpOffset, colourScheme.Wire);
                }

                RenderMainShape(graphics, colourScheme);
            }
        }
    }

    abstract class VarInpComponents
    {
        public abstract class BaseVarInpComponent : InpOutpBaseComponents.MultInpSingOutpBaseComponent
        {
            private Pos[] InpOffsets;
            private Pos OutpOffset;

            private string Details;

            public override Pos[] GetInpOffsets()
            {
                return InpOffsets;
            }

            public override Pos GetOutpOffset()
            {
                return OutpOffset;
            }

            public BaseVarInpComponent(int inpCount)
            {
                OutpOffset = new Pos(2, 0);
                InpOffsets = new Pos[inpCount];

                int evenOffset = 1 - (inpCount % 2);

                for (int inpNum = 0; inpNum < inpCount; inpNum++)
                {
                    InpOffsets[inpNum] = new Pos(-2, (2 * inpNum) - inpCount + evenOffset);
                }

                Details = $"{inpCount}";
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

            public override Rectangle GetComponentBounds()
            {
                Rectangle rect = GetDefaultComponentBounds();
                rect.Inflate(0, 1);
                return rect;
            }

            protected void DrawNotCircle(Graphics graphics, ColourScheme colourScheme)
            {
                graphics.FillEllipse(new SolidBrush(colourScheme.Background), GetOutpOffset().X - 1, GetOutpOffset().Y - 0.1F, 0.2F, 0.2F);
                graphics.DrawEllipse(new Pen(colourScheme.ComponentEdge, 0.01F), GetOutpOffset().X - 1, GetOutpOffset().Y - 0.1F, 0.2F, 0.2F);
            }

            protected void DrawAndComponent(Graphics graphics, ColourScheme colourScheme)
            {
                GraphicsPath path = new GraphicsPath();

                float vertDist = InpOffsets.Length - 0.5F;

                path.AddBeziers(new PointF[] { 
                    new PointF(-1, -vertDist), 
                    new PointF(1, -vertDist),
                    new PointF(1, -vertDist),
                    new PointF(1, 0),
                    new PointF(1, vertDist),
                    new PointF(1, vertDist),
                    new PointF(-1, vertDist)
                });
                path.CloseFigure();

                DrawComponentFromPath(graphics, path, colourScheme);
            }

            protected void DrawOrComponent(Graphics graphics, ColourScheme colourScheme)
            {

            }

            protected void DrawXorComponent(Graphics graphics, ColourScheme colourScheme)
            {

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

            public override string GetComponentID()
            {
                return ID;
            }

            public static IComponent Constructor(string Details)
            {
                int inpCount;

                if (int.TryParse(Details, out inpCount))
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

            public override string GetComponentID()
            {
                return ID;
            }

            public static IComponent Constructor(string Details)
            {
                int inpCount;

                if (int.TryParse(Details, out inpCount))
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

            public override string GetComponentID()
            {
                return ID;
            }

            public static IComponent Constructor(string Details)
            {
                int inpCount;

                if (int.TryParse(Details, out inpCount))
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

            public override string GetComponentID()
            {
                return ID;
            }

            public static IComponent Constructor(string Details)
            {
                int inpCount;

                if (int.TryParse(Details, out inpCount))
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

            public override string GetComponentID()
            {
                return ID;
            }

            public static IComponent Constructor(string Details)
            {
                int inpCount;

                if (int.TryParse(Details, out inpCount))
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

            public override string GetComponentID()
            {
                return ID;
            }

            public static IComponent Constructor(string Details)
            {
                int inpCount;

                if (int.TryParse(Details, out inpCount))
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

    class FixedStateComponent : InpOutpBaseComponents.NoneInpSingOutpBaseComponent
    {
        protected Pin.State OutputState;

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

        public override string GetComponentID()
        {
            return ID;
        }

        public override string GetComponentDetails()
        {
            return $"{(int)OutputState}";
        }

        public static IComponent Constructor(string Details)
        {
            int outputState;

            if (int.TryParse(Details, out outputState))
            {
                return new FixedStateComponent((Pin.State)outputState);
            }

            throw new Exception();
        }

        public override IComponent NonStaticConstructor(string details)
        {
            return Constructor(details);
        }

        public override Rectangle GetComponentBounds()
        {
            Rectangle rect = GetDefaultComponentBounds();
            rect.Inflate(0, 1);
            rect.Offset(-1, 0);
            rect.Width++;
            return rect;
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
        public UserToggleInpComponent(Pin.State startState) : base(startState) { }

        public void Interact()
        {
            OutputState = OutputState.Not();
        }

        public static new string ID = "TOGGLE";

        public override string GetComponentID()
        {
            return ID;
        }

        public static new IComponent Constructor(string Details)
        {
            int outputState;

            if (int.TryParse(Details, out outputState))
            {
                return new UserToggleInpComponent((Pin.State)outputState);
            }

            throw new Exception();
        }

        public override IComponent NonStaticConstructor(string details)
        {
            return Constructor(details);
        }
    }

    abstract class BoardContainerComponents
    {
        public class BoardInputComponent : InpOutpBaseComponents.NoneInpSingOutpBaseComponent, IBoardInputComponent
        {
            private string ComponentName;
            protected Pin.State State;

            public override Pos GetOutpOffset()
            {
                return new Pos(2, 0);
            }

            public string GetComponentName()
            {
                return ComponentName;
            }

            public BoardInputComponent(string name)
            {
                ComponentName = name;
            }

            public static string ID = "INPUT";

            public override string GetComponentID()
            {
                return ID;
            }

            public override string GetComponentDetails()
            {
                return ComponentName;
            }

            public static BoardInputComponent Constructor(string details)
            {
                return new BoardInputComponent(details);
            }

            public override IComponent NonStaticConstructor(string details)
            {
                return Constructor(details);
            }

            public void SetInputState(Pin.State state)
            {
                State = state;
            }

            public override void Tick()
            {
                GetOutpPin().SetState(State);
            }

            public override Rectangle GetComponentBounds()
            {
                Rectangle rect = GetDefaultComponentBounds();
                rect.Inflate(0, 1);
                rect.Offset(-1, 0);
                rect.Width++;
                return rect;
            }

            public override void RenderMainShape(Graphics graphics, ColourScheme colourScheme)
            {
                GraphicsPath path = new GraphicsPath();

                path.AddLines(new PointF[] { new PointF(-1.5F, -0.5F), new PointF(0.5F, -0.5F), new PointF(1, 0), new PointF(0.5F, 0.5F), new PointF(-1.5F, 0.5F) });
                path.CloseFigure();

                DrawComponentFromPath(graphics, path, colourScheme);
            }
        }

        public class BoardOutputComponent : InpOutpBaseComponents.SingInpNoneOutpBaseComponent, IBoardOutputComponent
        {
            private string ComponentName;
            protected Pin.State State;

            public override Pos GetInpOffset()
            {
                return new Pos(-2, 0);
            }

            public string GetComponentName()
            {
                return ComponentName;
            }

            public BoardOutputComponent(string name)
            {
                ComponentName = name;
            }

            public static string ID = "OUTPUT";

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

            public Pin.State GetOutputState()
            {
                return State;
            }

            public override void Tick()
            {
                State = GetInpPin().GetStateForComponent();
            }

            public override Rectangle GetComponentBounds()
            {
                Rectangle rect = GetDefaultComponentBounds();
                rect.Inflate(0, 1);
                rect.Width++;
                return rect;
            }

            public override void RenderMainShape(Graphics graphics, ColourScheme colourScheme)
            {
                GraphicsPath path = new GraphicsPath();

                path.AddLines(new PointF[] { new PointF(1.5F, -0.5F), new PointF(-0.5F, -0.5F), new PointF(-1, 0), new PointF(-0.5F, 0.5F), new PointF(1.5F, 0.5F) });
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
                return InternalBoard.GetBoardName();
            }

            public override void Tick()
            {
                Pin[] inpPins = GetInpPins();
                for (int i = 0; i < InpNames.Length; i++)
                {
                    InternalBoard.GetInputComponent(InpNames[i]).SetInputState(inpPins[i].GetStateForComponent());
                }

                Pin[] outpPins = GetOutpPins();
                for (int i = 0; i < OutpNames.Length; i++)
                {
                    outpPins[i].SetState(InternalBoard.GetOutputComponent(OutpNames[i]).GetOutputState());
                }

                InternalBoard.Tick();
            }

            public static BoardContainerComponent Constructor(string details)
            {
                return new BoardContainerComponent(Board.Load(details));
            }

            public override IComponent NonStaticConstructor(string details)
            {
                return Constructor(details);
            }

            public new IComponent Copy()
            {
                return new BoardContainerComponent(InternalBoard.Copy());
            }

            public override Rectangle GetComponentBounds()
            {
                Rectangle rect = GetDefaultComponentBounds();
                rect.Inflate(0, 1);
                return rect;
            }

            public override void RenderMainShape(Graphics graphics, ColourScheme colourScheme)
            {
                Rectangle rect = GetDefaultComponentBounds();
                rect.Inflate(-1, -1);

                graphics.FillRectangle(new SolidBrush(colourScheme.ComponentBackground), rect);
                graphics.DrawRectangle(new Pen(colourScheme.ComponentEdge), rect);
            }
        }
    } 
}
