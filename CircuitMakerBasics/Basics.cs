using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace CircuitMaker.Basics
{
    public class ByteEncoding : Encoding
    {
        public override int GetByteCount(char[] chars, int index, int count)
        {
            return count;
        }

        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            for (int i = 0; i < charCount; i++)
            {
                bytes[byteIndex + i] = (byte)chars[charIndex + i];
            }

            return charCount;
        }

        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            return count;
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            for (int i = 0; i < byteCount; i++)
            {
                chars[charIndex + i] = (char)bytes[byteIndex + i];
            }

            return byteCount;
        }

        public override int GetMaxByteCount(int charCount)
        {
            return charCount;
        }

        public override int GetMaxCharCount(int byteCount)
        {
            return byteCount;
        }

        private static Encoding byteEncoding = null;

        public static Encoding Byte
        {
            get
            {
                if (byteEncoding == null)
                {
                    byteEncoding = new ByteEncoding();
                }

                return byteEncoding;
            }
        }
    }

    //*
    public static class ReadWriteImplementation
    {
        public static void Write<T>(this BinaryWriter bw, T enumVal) where T : Enum
        {
            bw.Write(enumVal.ToString());
        }

        public static T ReadEnum<T>(this BinaryReader br) where T : Enum
        {
            return (T)Enum.Parse(typeof(T), br.ReadString());
        }

        public static void Write(this BinaryWriter bw, Pos pos)
        {
            bw.Write(pos.X);
            bw.Write(pos.Y);
        }

        public static Pos ReadPos(this BinaryReader br)
        {
            return new Pos(br.ReadInt32(), br.ReadInt32());
        }


        public static void Write(this BinaryWriter bw, Wire wire)
        {
            bw.Write(wire.Pos1);
            bw.Write(wire.Pos2);
        }

        public static Wire ReadWire(this BinaryReader br, Board board)
        {
            return new Wire(br.ReadPos(), br.ReadPos(), board);
        }


        public static void Write(this BinaryWriter bw, IComponent comp)
        {
            bw.Write(comp.GetComponentID());
            bw.Write(comp.GetComponentDetails());
            bw.Write(comp.GetComponentPos());
            bw.Write(comp.GetComponentRotation());
        }

        public static IComponent ReadComponent(this BinaryReader br, Board board)
        {
            if (Constructors.TryGetValue(br.ReadString(), out Func<string, IComponent> compFunc))
            {
                IComponent comp = compFunc(br.ReadString());
                comp.Place(br.ReadPos(), br.ReadEnum<Rotation>(), board);
                return comp;
            }

            br.ReadString(); // if couldn't find the chip, just ignore it.
            br.ReadPos();
            br.ReadEnum<Rotation>();

            return null;
        }


        public static void Write(this BinaryWriter bw, Board board)
        {
            bw.Write(board.Name);
            bw.Write(board.ExternalSize.Width);
            bw.Write(board.ExternalSize.Height);

            IComponent[] comps = board.GetComponents();
            Wire[] wires = board.GetAllWires();

            bw.Write(comps.Length);
            foreach (IComponent comp in comps)
            {
                bw.Write(comp);
            }

            bw.Write(wires.Length);
            foreach (Wire wire in wires)
            {
                bw.Write(wire);
            }
        }

        public static Board ReadBoard(this BinaryReader br)
        {
            Board board = new Board(br.ReadString(), new Size(br.ReadInt32(), br.ReadInt32()));

            int compCount = br.ReadInt32();

            for (int i = 0; i < compCount; i++)
            {
                br.ReadComponent(board);
            }

            int wireCount = br.ReadInt32();

            for (int i = 0; i < wireCount; i++)
            {
                br.ReadWire(board);
            }

            return board;
        }


        public static Dictionary<string, Func<string, IComponent>> Constructors = new Dictionary<string, Func<string, IComponent>>();
        public static Dictionary<string, string> DefaultDetails = new Dictionary<string, string>();
    }//*/

    class DefaultDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        private Func<TValue> gen_func;

        public DefaultDictionary(Func<TValue> gen_func)
        {
            this.gen_func = gen_func;
        }

        public new TValue this[TKey k]
        {
            get
            {
                if (!ContainsKey(k))
                {
                    Add(k, gen_func());
                }

                return base[k];
            }
            set => base[k] = value;
        }
    }

    public readonly struct Pos : IEquatable<Pos>
    {
        public readonly int X;
        public readonly int Y;

        public Pos(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public bool Equals(Pos other)
        {
            return X == other.X && Y == other.Y;
        }

        public Pos Add(int X, int Y)
        {
            return new Pos(this.X + X, this.Y + Y);
        }

        public Pos Add(Pos pos)
        {
            return new Pos(X + pos.X, Y + pos.Y);
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }

        public Pos Rotate(Rotation rotation)
        {
            if (rotation == Rotation.CLOCKWISE)
            {
                return new Pos(-Y, X);
            }
            else if (rotation == Rotation.HALF)
            {
                return new Pos(-X, -Y);
            }
            else if (rotation == Rotation.ANTICLOCKWISE)
            {
                return new Pos(Y, -X);
            }
            else
            {
                return this;
            }
        }

        public Point ToPoint()
        {
            return new Point(X, Y);
        }

        public static Pos FromPoint(Point point)
        {
            return new Pos(point.X, point.Y);
        }
    }

    public enum Rotation
    {
        ZERO = 0, CLOCKWISE = 90, HALF = 180, ANTICLOCKWISE = 270
    }

    //*
    static class RotationExtensions
    {
        public static Rotation AddRotation(this Rotation rot1, Rotation rot2)
        {
            return (Rotation)(((int)rot1 + (int)rot2) % 360);
        }
    }//*/

    public class Wire
    {
        public readonly Pos Pos1;
        public readonly Pos Pos2;

        private readonly Board board;

        public Pin Pin1
        {
            get { return board[Pos1]; }
        }

        public Pin Pin2
        {
            get { return board[Pos2]; }
        }

        public Wire(Pos pos1, Pos pos2, Board board)
        {
            Pos1 = pos1;
            Pos2 = pos2;

            this.board = board;

            Pin1.WireUpdate += OnWireUpdate;
            Pin2.WireUpdate += OnWireUpdate;

            board.AddWire(this);
        }

        public void OnWireUpdate()
        {
            Pin.State new_state = Pin1.GetStateForWire().WireJoin(Pin2.GetStateForWire());

            Pin1.SetState(new_state);
            Pin2.SetState(new_state);
        }

        public void Remove()
        {
            Pin1.WireUpdate -= OnWireUpdate;
            Pin2.WireUpdate -= OnWireUpdate;

            board.RemoveWire(this);
        }

        public override string ToString()
        {
            return $"[{Pos1}, {Pos2}]";
        }

        public Rectangle Bounds()
        {
            return Rectangle.FromLTRB(
                Math.Min(Pos1.X, Pos2.X), 
                Math.Min(Pos1.Y, Pos2.Y),
                Math.Max(Pos1.X, Pos2.X),
                Math.Max(Pos1.Y, Pos2.Y));
        }

        public RectangleF InflatedBounds()
        {
            RectangleF bounds = Bounds();

            bounds.Inflate(0.25F, 0.25F);

            return bounds;
        }

        public bool IsVert()
        {
            return Pos1.X == Pos2.X;
        }

        public bool IsHori()
        {
            return Pos1.Y == Pos2.Y;
        }

        public bool Collision(Pos pos)
        {
            Rectangle bounds = Bounds();

            if (IsVert())
            {
                return bounds.Top < pos.Y && pos.Y < bounds.Bottom && pos.X == Pos1.X;
            }

            if (IsHori())
            {
                return bounds.Left < pos.X && pos.X < bounds.Right && pos.Y == Pos1.Y;
            }
            
            // do angled lines?

            return false;
        }
    }

    public struct ColourScheme
    {
        public Color Background, ComponentBackground, ComponentEdge, Wire, WireFloating, WireLow, WireHigh, WireIllegal, Grid;

        public Color GetWireColour(Pin.State state)
        {
            if (state == Pin.State.FLOATING)
            {
                return WireFloating;
            }

            if (state == Pin.State.LOW)
            {
                return WireLow;
            }

            if (state == Pin.State.HIGH)
            {
                return WireHigh;
            }

            if (state == Pin.State.ILLEGAL)
            {
                return WireIllegal;
            }

            return Wire;
        }
    }

    public interface IComponent
    {
        void Place(Pos pos, Board board);
        void Place(Pos pos, Rotation rotation, Board board);
        void Remove();
        bool IsPlaced();

        void Tick();
        void ResetToDefault();


        Pos[] GetAllPinOffsets();
        Pos[] GetAllPinPositions();

        Pos GetComponentPos();
        Rotation GetComponentRotation();
        Matrix GetRenderMatrix();
        Board GetComponentBoard();

        IComponent Copy();

        string GetComponentID();
        string GetComponentDetails();

        RectangleF GetComponentBounds();
        RectangleF GetOffsetComponentBounds();

        /*
        bool HasSettings();
        void OpenSettings();
        //*/

        void Render(Graphics graphics, bool simulating, ColourScheme colourScheme);
        void RenderMainShape(Graphics graphics, bool simulating, ColourScheme colourScheme);
    }

    public interface IInteractibleComponent : IComponent
    {
        void Interact();
    }

    public interface IBoardInterfaceComponent : IComponent
    {
        string GetComponentName();
        void SetComponentName(string compName);

        void SetExternalPin(Pin pin);
        void RemoveExternalPin();

        Board.InterfaceLocation GetInterfaceLocation();
        void SetInterfaceLocation(Board.InterfaceLocation location);
    }

    public interface IBoardInputComponent : IBoardInterfaceComponent { }
    public interface IBoardOutputComponent : IBoardInterfaceComponent { }

    public interface IGraphicalComponent : IComponent
    {
        bool HasGraphics();
        void RenderGraphicalElement(Graphics graphics, bool simulating, ColourScheme colourScheme);
        RectangleF GetGraphicalElementBounds();
        Point? GetGraphicalElementLocation();
        void SetGraphicalElementLocation(Point? location);
    }

    public static class GraphicalComponentExtensions
    {
        public static RectangleF? GetOffsetGraphicalElementBounds<T>(this T comp) where T : IGraphicalComponent
        {
            RectangleF rect = comp.GetGraphicalElementBounds();
            Point? pos = comp.GetGraphicalElementLocation();

            if (pos.HasValue)
            {
                rect.Offset(pos.Value);

                return rect;
            }

            return null;
        }
    }

    public interface IBoardContainerComponent : IGraphicalComponent
    {
        Rectangle GetShape();
        void ResetShape();
        //void SetShape(RectangleF shape);

        Board GetInternalBoard();
    }

    static class StateExtensions
    {
        private static Pin.State[] NotOpTable = new Pin.State[]
        {
            Pin.State.FLOATING, Pin.State.HIGH, Pin.State.LOW, Pin.State.ILLEGAL
        };

        private static Pin.State[] AndDef = new Pin.State[] { Pin.State.LOW, Pin.State.LOW, Pin.State.HIGH };
        private static Pin.State[] OrDef = new Pin.State[] { Pin.State.LOW, Pin.State.HIGH, Pin.State.HIGH };
        private static Pin.State[] XorDef = new Pin.State[] { Pin.State.LOW, Pin.State.HIGH, Pin.State.LOW };

        private static Pin.State CalculateBinOp(Pin.State state1, Pin.State state2, Pin.State[] binOpDef)
        {
            if (state1 > state2)
            {
                return CalculateBinOp(state2, state1, binOpDef);
            } // state2 now higher than state1

            if (state2 == Pin.State.ILLEGAL)
            {
                return Pin.State.ILLEGAL;
            }

            if (state1 == Pin.State.FLOATING)
            {
                return state2;
            }

            if (state1 == state2)
            {
                if (state1 == Pin.State.LOW)
                {
                    return binOpDef[0];
                } else
                {
                    return binOpDef[2];
                }
            } else
            {
                return binOpDef[1];
            }
        }

        public static Pin.State WireJoin(this Pin.State state1, Pin.State state2)
        {
            return state1 | state2;
        }

        public static Pin.State Not(this Pin.State state)
        {
            return NotOpTable[(int)state];
        }

        public static Pin.State And(this Pin.State state1, Pin.State state2)
        {
            return CalculateBinOp(state1, state2, AndDef);
        }

        public static Pin.State Or(this Pin.State state1, Pin.State state2)
        {
            return CalculateBinOp(state1, state2, OrDef);
        }

        public static Pin.State Xor(this Pin.State state1, Pin.State state2)
        {
            return CalculateBinOp(state1, state2, XorDef);
        }
    }

    public class Pin
    {
        [Flags]
        public enum State : byte
        {
            FLOATING = 0b00, LOW = 0b01, HIGH = 0b10, ILLEGAL = 0b11
        }

        private State CurrentState = State.FLOATING;
        private State PrevState = State.FLOATING;

        public State GetStateForComponent()
        {
            return CurrentState;
        }

        public State GetStateForWire()
        {
            if (StateChanged)
            {
                return CurrentState;
            }
            return State.FLOATING;
        }

        public State GetStateForDisplay()
        {
            return CurrentState;
        }

        public void SetState(State state)
        {
            StateChanged = true;
            CurrentState = state;
        }

        private bool StateChanged;

        public void ResetStateChanged()
        {
            StateChanged = false;
        }

        public event Action WireUpdate;

        public bool EmitWireUpdate()
        {
            if (PrevState != CurrentState)
            {
                PrevState = CurrentState;
                if (WireUpdate != null)
                {
                    WireUpdate.Invoke();
                }
            }
            return PrevState != CurrentState;
        }

        public bool HasWires()
        {
            if (WireUpdate == null)
            {
                return false;
            }

            return WireUpdate.GetInvocationList().Length != 0;
        }

        public Wire[] GetWires()
        {
            if (WireUpdate == null)
            {
                return new Wire[0];
            }

            return WireUpdate.GetInvocationList().Select(del => (Wire)del.Target).ToArray();
        }
    }

    public static class SideEnumExtensions
    {
        public static bool IsLeftRight(this Board.InterfaceLocation.SideEnum side)
        {
            return side.HasFlag(Board.InterfaceLocation.SideEnum.LeftRight);
        }
        public static bool IsBottomRight(this Board.InterfaceLocation.SideEnum side)
        {
            return side.HasFlag(Board.InterfaceLocation.SideEnum.BottomRight);
        }
    }

    public class Board
    {
        public struct InterfaceLocation
        {
            [Flags]
            public enum SideEnum : byte { 
                Nothing = 0b000,
                LeftRight = 0b010, 
                BottomRight = 0b001,
                IsSide = 0b100,

                Top = IsSide,
                Bottom = IsSide | BottomRight,
                Left = IsSide | LeftRight,
                Right = IsSide | LeftRight | BottomRight,
            }

            public SideEnum Side;
            public int Distance;

            public InterfaceLocation(SideEnum side, int distance)
            {
                Side = side;
                Distance = distance;
            }

            public static bool operator ==(InterfaceLocation loc1, InterfaceLocation loc2)
            {
                return loc1.Side == loc2.Side && loc1.Distance == loc2.Distance;
            }

            public static bool operator !=(InterfaceLocation loc1, InterfaceLocation loc2)
            {
                return loc1.Side != loc2.Side || loc1.Distance != loc2.Distance;
            }

            public override bool Equals(object other)
            {
                if (other is InterfaceLocation otherLoc)
                {
                    return this == otherLoc;
                }

                return false;
            }

            public override int GetHashCode()
            {
                //return base.GetHashCode();

                return (int)Side ^ Distance;
            }

            public override string ToString()
            {
                return $"({Side},{Distance})";
            }
        }

        /*
        private class InterfaceComponentStore<T> : HashSet<T> where T : IBoardInterfaceComponent
        {
            public void Add(T component)
            {
                Add(component.GetComponentName(), component);
            }

            public bool Remove(T component)
            {
                return Remove(component.GetComponentName());
            }

            public new bool ContainsValue(T component)
            {
                return ContainsKey(component.GetComponentName());
            }
        }
        //*/

        private DefaultDictionary<Pos, Pin> Pins = new DefaultDictionary<Pos, Pin>(() => new Pin());

        private HashSet<Wire> Wires = new HashSet<Wire>();

        private HashSet<IComponent> Components = new HashSet<IComponent>();
        private HashSet<IBoardInterfaceComponent> InterfaceComponents = new HashSet<IBoardInterfaceComponent>();
        private HashSet<IBoardInputComponent> InputComponents = new HashSet<IBoardInputComponent>();
        private HashSet<IBoardOutputComponent> OutputComponents = new HashSet<IBoardOutputComponent>();
        private List<IGraphicalComponent> GraphicalComponents = new List<IGraphicalComponent>();
        private HashSet<IBoardContainerComponent> ContainerComponents = new HashSet<IBoardContainerComponent>();
        private HashSet<Board> InternalBoards = new HashSet<Board>();

        private Size? externalSize;
        public Size ExternalSize
        {
            get
            {
                if (!externalSize.HasValue)
                {
                    int bidirCount = InputComponents.Count() + OutputComponents.Count() - InterfaceComponents.Count();

                    int vertLimit = Math.Max(Math.Max(InputComponents.Count(), OutputComponents.Count()) - bidirCount, 1),
                        horiLimit = Math.Max(Math.Max(bidirCount, vertLimit / 2), 1);

                    externalSize = new Size(vertLimit * 2, horiLimit * 2);

                    SizeChanged?.Invoke();
                }

                return externalSize.Value;
            }
            set
            {
                externalSize = value;
                SizeChanged?.Invoke();
            }
        }

        public event Action SizeChanged;

        public string Name;

        public Board(string name, Size? externalSize = null)
        {
            Name = name;
            if (externalSize.HasValue)
            {
                ExternalSize = externalSize.Value;
            }
        }

        public bool EmitWireUpdate()
        {
            bool returnVal = false;

            ClearUnusedPins();

            foreach (Pin pin in Pins.Values)
            {
                returnVal |= pin.EmitWireUpdate();
            }

            return returnVal;
        }

        public void AddWire(Wire wire)
        {
            Wires.Add(wire);
        }

        public void RemoveWire(Wire wire)
        {
            Wires.Remove(wire);
        }

        public Wire[] GetAllWires()
        {
            return Wires.ToArray();
        }

        public IComponent[] GetComponents()
        {
            return Components.ToArray();
        }

        public IBoardInterfaceComponent[] GetInterfaceComponents()
        {
            return InterfaceComponents.ToArray();
        }

        public IBoardInterfaceComponent GetInterfaceComponent(string name)
        {
            foreach (IBoardInterfaceComponent comp in InterfaceComponents)
            {
                if (comp.GetComponentName() == name)
                {
                    return comp;
                }
            }

            return null;
        }

        public IGraphicalComponent[] GetGraphicalComponents()
        {
            return GraphicalComponents.ToArray();
        }

        public IGraphicalComponent GetGraphicalComponent(int index)
        {
            return GraphicalComponents[index];
        }

        public IBoardInputComponent[] GetInputComponents()
        {
            return InputComponents.ToArray();
        }

        public IBoardInputComponent GetInputComponent(string name)
        {
            foreach (IBoardInputComponent comp in InputComponents)
            {
                if (comp.GetComponentName() == name)
                {
                    return comp;
                }
            }

            return null;
        }

        public IBoardOutputComponent[] GetOutputComponents()
        {
            return OutputComponents.ToArray();
        }

        public IBoardOutputComponent GetOutputComponent(string name)
        {
            foreach (IBoardOutputComponent comp in OutputComponents)
            {
                if (comp.GetComponentName() == name)
                {
                    return comp;
                }
            }

            return null;
        }

        public IBoardContainerComponent[] GetContainerComponents()
        {
            return ContainerComponents.ToArray();
        }

        public void Tick()
        {

            foreach (Pin pin in Pins.Values)
            {
                pin.ResetStateChanged();
            }

            foreach (IComponent comp in Components)
            {
                comp.Tick();
            }

            while (EmitWireUpdate()) { }
        }

        public Pin this[Pos pos] => Pins[pos];

        private string GuaranteeUniqueName(string current, string[] existing)
        {
            string baseName = current.TrimEnd("0123456789".ToArray());
            string baseNumberString = current.Substring(baseName.Length);

            int baseNumber = 0;
            int.TryParse(baseNumberString, out baseNumber);

            while (existing.Contains(current))
            {
                baseNumber++;
                current = baseName + baseNumber.ToString();
            }

            return current;
        }

        internal void AddComponent(IComponent comp)
        {
            RectangleF bounds = comp.GetOffsetComponentBounds();

            foreach (IComponent otherComp in Components)
            {
                if (otherComp.GetOffsetComponentBounds().IntersectsWith(bounds))
                {
                    throw new Exception("cant place on another component. this error shouldn't be in the final product");
                }
            }

            Components.Add(comp);

            if (comp is IGraphicalComponent graphicalComp)
            {
                GraphicalComponents.Add(graphicalComp);
            }

            if (comp is IBoardInterfaceComponent interfaceComp)
            {
                InterfaceLocation interfaceLoc = interfaceComp.GetInterfaceLocation();

                if (interfaceLoc.Distance == 0)
                {
                    int[] existingLocs = GetInterfaceComponents().Where(thisComp => thisComp.GetInterfaceLocation().Side == interfaceLoc.Side).Select(thisComp => thisComp.GetInterfaceLocation().Distance).ToArray();

                    //Console.WriteLine(existingLocs.Select(num => num.ToString()).Prepend("").Aggregate((str1, str2) => str1 + ", " + str2));

                    for (int newDist = 1; true; newDist += 2)
                    {
                        if (!existingLocs.Contains(newDist))
                        {
                            interfaceLoc.Distance = newDist;

                            interfaceComp.SetInterfaceLocation(interfaceLoc);

                            break;
                        }
                    }
                }

                interfaceComp.SetComponentName(GuaranteeUniqueName(interfaceComp.GetComponentName(), 
                    InputComponents.Select(thisComp => thisComp.GetComponentName()).Concat(OutputComponents.Select(thisComp => thisComp.GetComponentName())).ToArray()));

                //InterfaceComponents.Add(interfaceComp.GetComponentName(), interfaceComp);
                InterfaceComponents.Add(interfaceComp);

                /*
                InterfaceLocation? interfaceLocation;

                do
                {
                    interfaceLocation = NextEmptyInterfaceLocation();

                    if (!interfaceLocation.HasValue)
                    {
                        ExternalSize.Width++;
                    }
                } while (!interfaceLocation.HasValue);

                InterfaceLocations.Add(interfaceComp.GetComponentName(), interfaceLocation.Value);
                */

                if (comp is IBoardInputComponent inpComp)
                {
                    //InputComponents.Add(inpComp.GetComponentName(), inpComp);
                    InputComponents.Add(inpComp);
                }

                if (comp is IBoardOutputComponent outpComp)
                {
                    //OutputComponents.Add(outpComp.GetComponentName(), outpComp);
                    OutputComponents.Add(outpComp);
                }

                if (comp is IBoardContainerComponent contComp)
                {
                    ContainerComponents.Add(contComp);

                    if (InternalBoards.Select(board => board.Name).Contains(contComp.GetInternalBoard().Name))
                    {
                        // check <-------------------------------------------------------------------------------------------------------------------------------------------------
                    } else
                    {
                        InternalBoards.Add(contComp.GetInternalBoard());
                    }
                }

                bool isSide = (interfaceLoc.Side & InterfaceLocation.SideEnum.LeftRight) != InterfaceLocation.SideEnum.Nothing;

                while (interfaceLoc.Distance >= (isSide ? ExternalSize.Height : ExternalSize.Width))
                {
                    ExternalSize = new Size(ExternalSize.Width + (isSide ? 0 : 1), ExternalSize.Height + (isSide ? 1 : 0));
                }
            }
        }

        internal void RemoveComponent(IComponent comp)
        {
            Components.Remove(comp);

            /*
            if (comp is IBoardInterfaceComponent interfaceComponent) {
                InterfaceComponents.Remove(interfaceComponent.GetComponentName());
            }
            //*/

            //*
            if (comp is IGraphicalComponent grapicalComp)
            {
                GraphicalComponents.Remove(grapicalComp);
            }

            if (comp is IBoardInterfaceComponent interfaceComp)
            {
                InterfaceComponents.Remove(interfaceComp);
            }

            if (comp is IBoardInputComponent inpComp)
            {
                InputComponents.Remove(inpComp);
            }

            if (comp is IBoardOutputComponent outpComp)
            {
                OutputComponents.Remove(outpComp);
            }

            if (comp is IBoardContainerComponent contComp)
            {
                ContainerComponents.Remove(contComp);

                if (!ContainerComponents.Select(selComp => selComp.GetInternalBoard().Name).Contains(contComp.GetInternalBoard().Name))
                {
                    InternalBoards.RemoveWhere(board => board.Name == contComp.GetInternalBoard().Name);
                }
            }
            //*/
        }

        protected void ClearUnusedPins()
        {
            HashSet<Pos> keepPinPositions = new HashSet<Pos>();

            foreach (IComponent comp in Components)
            {
                keepPinPositions.UnionWith(comp.GetAllPinPositions());
            }

            foreach (Wire wire in Wires)
            {
                keepPinPositions.Add(wire.Pos1);
                keepPinPositions.Add(wire.Pos2);
            }

            //*
            HashSet<Pos> removePinPositions = new HashSet<Pos>();

            removePinPositions.UnionWith(Pins.Keys);
            removePinPositions.ExceptWith(keepPinPositions);

            foreach (Pos pinPos in removePinPositions)
            {
                Pins.Remove(pinPos);
            }
            //*/

            /*
            foreach (Pos pinPos in Pins.Keys)
            {
                if (!Pins[pinPos].HasWires() && !keepPinPositions.Contains(pinPos))
                {
                    Pins.Remove(pinPos);
                }
            }
            //*/
        }

        public void Render(Graphics graphics, bool simulating, Rectangle bounds, ColourScheme colourScheme)
        {
            /*
            for (int x = bounds.Left; x < bounds.Right; x++)
            {
                for (int y = bounds.Top; y < bounds.Bottom; y++)
                {
                    /graphics.FillEllipse(Brushes.Black, x - 0.005F, y - 0.005F, 0.01F, 0.01F); // should make this better
                }
            }//*/

            //*
            for (int x = bounds.Left; x <= bounds.Right; x++)
            {
                graphics.DrawLine(new Pen(colourScheme.Grid, 0.005F), x, bounds.Top, x, bounds.Bottom);
            }

            for (int y = bounds.Top; y <= bounds.Bottom; y++)
            {
                graphics.DrawLine(new Pen(colourScheme.Grid, 0.005F), bounds.Left, y, bounds.Right, y);
            }
            //*/

            Pin pin;
            int connectionCount;
            List<Pos> compPins = new List<Pos>();

            foreach (IComponent comp in Components)
            {
                compPins.AddRange(comp.GetAllPinPositions());
            }

            foreach (Pos pinPos in Pins.Keys)
            {
                if (bounds.Contains(pinPos.X, pinPos.Y))
                {
                    pin = Pins[pinPos];

                    connectionCount = pin.GetWires().Length + compPins.Where(pinPos.Equals).Count();

                    if (connectionCount == 0)
                    {
                        continue;
                    }

                    if (connectionCount != 2)
                    {
                        graphics.FillEllipse(new SolidBrush(colourScheme.GetWireColour(pin.GetStateForDisplay())), pinPos.X - 0.05F, pinPos.Y - 0.05F, 0.1F, 0.1F);
                    }

                    if (simulating)
                    {
                        graphics.DrawString(pin.GetStateForDisplay().ToString(), new Font("arial", 0.1F), Brushes.Black, pinPos.X, pinPos.Y);
                    }
                }
            }

            graphics.FillEllipse(Brushes.Black, -0.5F, -0.5F, 1, 1);

            Matrix matrix = new Matrix();

            foreach (IComponent comp in Components)
            {
                if (comp.GetOffsetComponentBounds().IntersectsWith(bounds))
                {
                    matrix = comp.GetRenderMatrix();
                    graphics.MultiplyTransform(matrix);

                    comp.Render(graphics, simulating, colourScheme);

                    if (comp is IGraphicalComponent graphicalComp)
                    {
                        graphicalComp.RenderGraphicalElement(graphics, simulating, colourScheme);
                    }

                    RectangleF compBounds = comp.GetComponentBounds();

                    //graphics.DrawRectangle(new Pen(Color.Red, 0.05F), compBounds.X, compBounds.Y, compBounds.Width, compBounds.Height);
                    //graphics.DrawRectangle(new Pen(Color.Red, 0.05F), comp.GetComponentBounds());
                    //graphics.FillEllipse(Brushes.Red, -0.05F, -0.05F, 0.1F, 0.1F);

                    matrix.Invert();
                    graphics.MultiplyTransform(matrix);
                }
            }

            //*
            foreach (Wire wire in Wires)
            {
                graphics.DrawLine(new Pen(simulating ? colourScheme.GetWireColour(wire.Pin1.GetStateForDisplay()) : colourScheme.Wire, 0.01F), new Point(wire.Pos1.X, wire.Pos1.Y), new Point(wire.Pos2.X, wire.Pos2.Y));
            }
            //*/
        }

        public bool CheckAllowed(RectangleF bounds) // needs to consider wires too. also, doesn't exactly work <--------------------
        {
            foreach (IComponent comp in Components)
            {
                if (comp.GetOffsetComponentBounds().IntersectsWith(bounds))
                {
                    return false;
                }
            }

            return true;
        }

        public void ResetToFloating()
        {
            foreach (Pin pin in Pins.Values)
            {
                pin.SetState(Pin.State.FLOATING);
            }
        }

        public void ResetForSimulation()
        {
            ResetToFloating();

            foreach (IComponent comp in Components)
            {
                comp.ResetToDefault();
            }
        }

        public void SimplifyWires()
        {
            Wire[] wires;

            foreach (Pin pin in Pins.Values)
            {
                wires = pin.GetWires();

                if (wires.Length == 2 && wires[0].IsHori() == wires[1].IsHori() && wires[0].IsVert() == wires[1].IsVert() && (wires[0].IsHori() || wires[0].IsVert()))
                {
                    if (wires[0].Pos1.Equals(wires[1].Pos1) && !wires[0].Pos2.Equals(wires[1].Pos2))
                    {
                        new Wire(wires[0].Pos2, wires[1].Pos2, this);
                    } else if (wires[0].Pos2.Equals(wires[1].Pos1) && !wires[0].Pos1.Equals(wires[1].Pos2))
                    {
                        new Wire(wires[0].Pos1, wires[1].Pos2, this);
                    } else if (wires[0].Pos1.Equals(wires[1].Pos2) && !wires[0].Pos2.Equals(wires[1].Pos1))
                    {
                        new Wire(wires[0].Pos2, wires[1].Pos1, this);
                    } else if (wires[0].Pos2.Equals(wires[1].Pos2) && !wires[0].Pos1.Equals(wires[1].Pos1))
                    {
                        new Wire(wires[0].Pos1, wires[1].Pos1, this);
                    }

                    wires[0].Remove();
                    wires[1].Remove();
                }
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("components: ");

            string listBuilder(string first, string second) => first + ", " + second;

            sb.Append(Components.Select(comp => comp.ToString()).Aggregate(listBuilder));
            sb.Append(", wires: ");
            sb.Append(Wires.Select(wire => wire.ToString()).Aggregate(listBuilder));

            return sb.ToString();
        }

        private static string GetFilename(string boardname)
        {
            return $"Boards/{boardname}.brd";
        }

        public void Save(string filename)
        {
            using (FileStream file = File.Open(filename, FileMode.Create))
            {
                using (BinaryWriter bw = new BinaryWriter(file))
                {
                    bw.Write(this);
                }
            }
        }

        public static Board Load(string filename)
        {
            using (FileStream file = File.Open(filename, FileMode.Open))
            {
                using (BinaryReader br = new BinaryReader(file))
                {
                    return br.ReadBoard();
                }
            }
        }

        public Board Copy(string copyName = null)
        {
            Board copy = new Board(copyName ?? Name + " - Copy", new Size(ExternalSize.Width, ExternalSize.Height));

            foreach (IComponent comp in Components)
            {
                comp.Copy().Place(comp.GetComponentPos(), copy);
            }

            foreach (Wire wire in Wires)
            {
                new Wire(wire.Pos1, wire.Pos2, copy);
            }

            return copy;
        }
    }//*/
}
