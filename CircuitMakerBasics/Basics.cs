using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Remoting.Messaging;
using CircuitMaker.Components;
using System.Threading;
using System.CodeDom;
using System.Security.Permissions;
using System.Runtime.CompilerServices;

namespace CircuitMaker.Basics
{
    static class RectangleFExtensions
    {
        public static int dp = 5;

        private static float Round(float val)
        {
            return (float)Math.Round(val, dp);
        }

        public static RectangleF Round(this RectangleF rect)
        {
            return new RectangleF(Round(rect.X), Round(rect.Y), Round(rect.Width), Round(rect.Height));
        }
    }

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

    public class PlacementException : Exception
    {
        public PlacementException(string desc) : base(desc) { }
    }

    public static class ReadWriteImplementation
    {
        public static void PromiseBoard(string boardName, Action<Board> provider)
        {
            promises.Add(new BoardPromise(boardName, provider));
        }

        private class BoardPromise
        {
            public string BoardName;
            private Action<Board> BoardProvider;

            public BoardPromise(string boardName, Action<Board> provider)
            {
                BoardName = boardName;
                BoardProvider = provider;
            }

            public void FulfillPromise(Board board)
            {
                BoardProvider(board.Copy());
            }
        }

        private static List<BoardPromise> promises = new List<BoardPromise>();

        private static void FulfillPromises(Board[] boards)
        {
            Dictionary<string, Board> namedBoards = new Dictionary<string, Board>();

            foreach (Board board in boards)
            {
                namedBoards.Add(board.Name, board);
            }

            List<BoardPromise> unfulfilled = new List<BoardPromise>();

            while (promises.Count > 0)
            {
                if (namedBoards.ContainsKey(promises[0].BoardName))
                {
                    promises[0].FulfillPromise(namedBoards[promises[0].BoardName]);
                }
                else
                {
                    unfulfilled.Add(promises[0]);
                }

                promises.RemoveAt(0);
            }

            promises = unfulfilled;
        }


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
            string compType = br.ReadString();
            if (Constructors.TryGetValue(compType, out Func<string, IComponent> compFunc))
            {
                IComponent comp = compFunc(br.ReadString());
                comp.Place(br.ReadPos(), br.ReadEnum<Rotation>(), board);
                return comp;
            }

            throw new PlacementException($"Could not find builtin component of type {compType}");
        }


        public static void Write(this BinaryWriter bw, Board board)
        {
            WriteBoardBasic(bw, board);

            Board[] boards = board.GetBoardList().Where(thisBoard => thisBoard.Name != board.Name).ToArray();

            bw.Write(boards.Length);
            foreach (Board thisBoard in boards)
            {
                WriteBoardBasic(bw, thisBoard);
            }
        }

        private static void WriteBoardBasic(BinaryWriter bw, Board board)
        {
            board.SimplifyWires();

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
            Board board = ReadBoardBasic(br);

            int boardCount = br.ReadInt32();
            Board[] boards = new Board[boardCount];
            for (int i = 0; i < boardCount; i++)
            {
                boards[i] = ReadBoardBasic(br);
            }

            FulfillPromises(boards);

            if (promises.Count > 0)
            {
                string[] names = promises.Select(promise => promise.BoardName).ToArray();

                throw new PlacementException($"Board with name{(promises.Count > 1 ? "s" : "")} {names.Select(s => "'" + s + "'").Aggregate((s1, s2) => s1 + ", " + s2)} not found");
            }

            board.SimplifyWires();

            return board;
        }

        private static Board ReadBoardBasic(BinaryReader br)
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
    }

    class DefaultDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        private Func<TValue> BlindGenerator;
        private Func<TKey, TValue> KeyBasedGenerator;

        public DefaultDictionary(Func<TValue> generator)
        {
            BlindGenerator = generator;
        }

        public DefaultDictionary(Func<TKey, TValue> generator)
        {
            KeyBasedGenerator = generator;
        }

        public new TValue this[TKey key]
        {
            get
            {
                if (!ContainsKey(key))
                {
                    if (BlindGenerator != null)
                    {
                        Add(key, BlindGenerator());
                    } else if (KeyBasedGenerator != null)
                    {
                        Add(key, KeyBasedGenerator(key));
                    }
                }

                return base[key];
            }
            set => base[key] = value;
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

    static class RotationExtensions
    {
        public static Rotation AddRotation(this Rotation rot1, Rotation rot2)
        {
            return (Rotation)(((int)rot1 + (int)rot2) % 360);
        }
    }

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

            return bounds.Contains(new Point(pos.X, pos.Y));
        }
    }

    public struct ColourScheme
    {
        public Color Background, ComponentBackground, ComponentEdge, Wire, WireFloating, WireLow, WirePulledLow, WireHigh, WirePulledHigh, WireIllegal, Grid, Selection;

        public Color GetWireColour(Pin.State state)
        {
            switch (state)
            {
                case Pin.State.FLOATING:    return WireFloating;
                case Pin.State.LOW:         return WireLow;
                case Pin.State.PULLEDLOW:   return WirePulledLow;
                case Pin.State.HIGH:        return WireHigh;
                case Pin.State.PULLEDHIGH:  return WirePulledHigh;
                case Pin.State.ILLEGAL:     return WireIllegal;
                default:                    return Wire;
            }
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
        Pos[] GetAllUniquePinPositions();

        Pos GetComponentPos();
        Rotation GetComponentRotation();
        Matrix GetRenderMatrix();
        Board GetComponentBoard();

        IComponent Copy();

        string GetComponentID();
        string GetComponentDetails();

        RectangleF GetComponentBounds();
        RectangleF GetOffsetComponentBounds();

        void Render(Graphics graphics, bool simulating, ColourScheme colourScheme);
        void RenderMainShape(Graphics graphics, bool simulating, ColourScheme colourScheme);
    }

    public interface IInteractibleComponent : IComponent
    {
        void Interact();
    }

    public interface IWireComponent : IComponent { }

    public interface IBoardInterfaceComponent : IWireComponent
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
        float GetGraphicalElementScale();
        void SetGraphicalElementScale(float scale);
        void SetGraphicalElementLocation(Point? location);
    }

    public static class GraphicalComponentExtensions
    {
        private static RectangleF Scale(RectangleF rect, float scale)
        {
            rect.X *= scale;
            rect.Y *= scale;
            rect.Width *= scale;
            rect.Height *= scale;

            return rect;
        }

        private static RectangleF? Offset(RectangleF rect, Point? pos)
        {
            if (pos.HasValue)
            {
                rect.Offset(pos.Value);

                return rect;
            }

            return null;
        }

        public static RectangleF? GetOffsetGraphicalElementBounds<T>(this T comp) where T : IGraphicalComponent
        {
            return Offset(comp.GetGraphicalElementBounds(), comp.GetGraphicalElementLocation());
        }

        public static RectangleF GetScaledGraphicalElementBounds<T>(this T comp) where T : IGraphicalComponent
        {
            return Scale(comp.GetGraphicalElementBounds(), comp.GetGraphicalElementScale());
        }

        public static RectangleF? GetOffsetScaledGraphicalElementBounds<T>(this T comp) where T : IGraphicalComponent
        {
            return Offset(Scale(comp.GetGraphicalElementBounds(), comp.GetGraphicalElementScale()), comp.GetGraphicalElementLocation());
        }
    }

    public interface IBoardContainerComponent : IGraphicalComponent
    {
        Rectangle GetShape();
        void ResetShape();

        Board GetInternalBoard();
    }

    static class StateExtensions
    {
        private struct BinOpInput
        {
            Pin.State State1, State2;

            public BinOpInput((Pin.State state1, Pin.State state2) states)
            {
                State1 = states.state1; State2 = states.state2;
            }

            public BinOpInput(Pin.State state1, Pin.State state2)
            {
                State1 = state1; State2 = state2;
            }

            public override bool Equals(object obj)
            {
                if (obj is BinOpInput otherInp)
                {
                    return (State1 == otherInp.State1 && State2 == otherInp.State2) || (State1 == otherInp.State2 && State2 == otherInp.State1);
                }

                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return (int)State1 + (int)State2;
            }

            public static implicit operator BinOpInput((Pin.State state1, Pin.State state2) states)
            {
                return new BinOpInput(states.state1, states.state2);
            }

            /*
            public static implicit operator ValueTuple<Pin.State, Pin.State>(BinOpInput binOpInput)
            {
                return (binOpInput.State1, binOpInput.State2);
            }
            //*/

            public static bool operator ==(BinOpInput inp1, BinOpInput inp2)
            {
                return inp1.Equals(inp2);
            }

            public static bool operator !=(BinOpInput inp1, BinOpInput inp2)
            {
                return !inp1.Equals(inp2);
            }

            public override string ToString()
            {
                return $"{State1}, {State2}";
            }
        }

        private class BinOpTable : Dictionary<BinOpInput, Pin.State>
        {
            public Pin.State this[Pin.State state1, Pin.State state2] => this[new BinOpInput(state1, state2)];
            public Pin.State this[(Pin.State state1, Pin.State state2) states] => this[new BinOpInput(states)];

            public BinOpTable(IDictionary<BinOpInput, Pin.State> dict) : base(dict) { }

            public BinOpTable() : base() { }
        }

        private static Dictionary<Pin.State, Pin.State> NotOpTable = new Dictionary<Pin.State, Pin.State>
        {
            { Pin.State.FLOATING, Pin.State.FLOATING },
            { Pin.State.LOW, Pin.State.HIGH },
            { Pin.State.HIGH, Pin.State.LOW },
            { Pin.State.ILLEGAL, Pin.State.ILLEGAL }
        };

        private static Dictionary<Pin.State, Pin.State> PullTable = new Dictionary<Pin.State, Pin.State>
        {
            { Pin.State.FLOATING, Pin.State.FLOATING },
            { Pin.State.LOW, Pin.State.LOW },
            { Pin.State.PULLEDLOW, Pin.State.LOW },
            { Pin.State.HIGH, Pin.State.HIGH },
            { Pin.State.PULLEDHIGH, Pin.State.HIGH },
            { Pin.State.ILLEGAL, Pin.State.ILLEGAL }
        };

        private static Dictionary<BinOpInput, Pin.State> GenericOpTable = new Dictionary<BinOpInput, Pin.State>
        {
            { (Pin.State.FLOATING, Pin.State.FLOATING), Pin.State.FLOATING },
            { (Pin.State.FLOATING, Pin.State.LOW), Pin.State.LOW },
            { (Pin.State.FLOATING, Pin.State.HIGH), Pin.State.HIGH },
            { (Pin.State.FLOATING, Pin.State.ILLEGAL), Pin.State.ILLEGAL },
            { (Pin.State.ILLEGAL, Pin.State.LOW), Pin.State.ILLEGAL },
            { (Pin.State.ILLEGAL, Pin.State.HIGH), Pin.State.ILLEGAL },
            { (Pin.State.ILLEGAL, Pin.State.ILLEGAL), Pin.State.ILLEGAL }
        };

        private static BinOpTable AndOpTable = new BinOpTable((new Dictionary<BinOpInput, Pin.State>
        {
            { (Pin.State.LOW, Pin.State.LOW), Pin.State.LOW },
            { (Pin.State.LOW, Pin.State.HIGH), Pin.State.LOW },
            { (Pin.State.HIGH, Pin.State.HIGH), Pin.State.HIGH }
        }.Concat(GenericOpTable)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
        private static BinOpTable OrOpTable = new BinOpTable((new Dictionary<BinOpInput, Pin.State>
        {
            { (Pin.State.LOW, Pin.State.LOW), Pin.State.LOW },
            { (Pin.State.LOW, Pin.State.HIGH), Pin.State.HIGH },
            { (Pin.State.HIGH, Pin.State.HIGH), Pin.State.HIGH }
        }.Concat(GenericOpTable)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
        private static BinOpTable XorOpTable = new BinOpTable((new Dictionary<BinOpInput, Pin.State>
        {
            { (Pin.State.LOW, Pin.State.LOW), Pin.State.LOW },
            { (Pin.State.LOW, Pin.State.HIGH), Pin.State.HIGH },
            { (Pin.State.HIGH, Pin.State.HIGH), Pin.State.LOW }
        }.Concat(GenericOpTable)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value));

        private static BinOpTable WireJoinTable = new BinOpTable
        {
            { (Pin.State.FLOATING,      Pin.State.FLOATING),    Pin.State.FLOATING },
            { (Pin.State.FLOATING,      Pin.State.LOW),         Pin.State.LOW },
            { (Pin.State.FLOATING,      Pin.State.PULLEDLOW),   Pin.State.PULLEDLOW },
            { (Pin.State.FLOATING,      Pin.State.HIGH),        Pin.State.HIGH },
            { (Pin.State.FLOATING,      Pin.State.PULLEDHIGH),  Pin.State.PULLEDHIGH },
            { (Pin.State.FLOATING,      Pin.State.ILLEGAL),     Pin.State.ILLEGAL },
            { (Pin.State.LOW,           Pin.State.LOW),         Pin.State.LOW },
            { (Pin.State.LOW,           Pin.State.PULLEDLOW),   Pin.State.LOW },
            { (Pin.State.LOW,           Pin.State.HIGH),        Pin.State.ILLEGAL },
            { (Pin.State.LOW,           Pin.State.PULLEDHIGH),  Pin.State.LOW },
            { (Pin.State.LOW,           Pin.State.ILLEGAL),     Pin.State.ILLEGAL },
            { (Pin.State.PULLEDLOW,     Pin.State.PULLEDLOW),   Pin.State.PULLEDLOW },
            { (Pin.State.PULLEDLOW,     Pin.State.HIGH),        Pin.State.HIGH },
            { (Pin.State.PULLEDLOW,     Pin.State.PULLEDHIGH),  Pin.State.FLOATING },
            { (Pin.State.PULLEDLOW,     Pin.State.ILLEGAL),     Pin.State.ILLEGAL },
            { (Pin.State.HIGH,          Pin.State.HIGH),        Pin.State.HIGH },
            { (Pin.State.HIGH,          Pin.State.PULLEDHIGH),  Pin.State.HIGH },
            { (Pin.State.HIGH,          Pin.State.ILLEGAL),     Pin.State.ILLEGAL },
            { (Pin.State.PULLEDHIGH,    Pin.State.PULLEDHIGH),  Pin.State.PULLEDHIGH },
            { (Pin.State.PULLEDHIGH,    Pin.State.ILLEGAL),     Pin.State.ILLEGAL },
            { (Pin.State.ILLEGAL,       Pin.State.ILLEGAL),     Pin.State.ILLEGAL }
        };

        public static Pin.State WireJoin(this Pin.State state1, Pin.State state2)
        {
            return WireJoinTable[state1, state2];
        }

        public static Pin.State Not(this Pin.State state)
        {
            return NotOpTable[state];
        }

        public static Pin.State And(this Pin.State state1, Pin.State state2)
        {
            return AndOpTable[state1, state2];
        }

        public static Pin.State Or(this Pin.State state1, Pin.State state2)
        {
            return OrOpTable[state1, state2];
        }

        public static Pin.State Xor(this Pin.State state1, Pin.State state2)
        {
            return XorOpTable[state1, state2];
        }

        public static Pin.State Pulled(this Pin.State state)
        {
            return PullTable[state];
        }
    }

    public class Pin
    {
        public enum State
        {
            FLOATING, LOW, PULLEDLOW, HIGH, PULLEDHIGH, ILLEGAL
        }

        private State CurrentState, OriginalState;

        public State GetStateForComponent()
        {
            return OriginalState.Pulled();
        }

        public State GetStateForWireComponent()
        {
            return CurrentState;
        }

        public State GetStateForWire()
        {
            return CurrentState;
        }

        public State GetStateForDisplay()
        {
            return CurrentState;
        }

        public void SetState(State state)
        {
            //StateChanged = true;
            CurrentState = CurrentState.WireJoin(state);
        }

        //private bool StateChanged;

        public void ResetToFloating()
        {
            CurrentState = State.FLOATING;
            OriginalState = State.FLOATING;
            //StateChanged = false;
        }

        public void SetupForTick()
        {
            OriginalState = CurrentState;
            CurrentState = Pin.State.FLOATING;
            //StateChanged = false;
        }

        public event Action WireUpdate;

        public bool EmitWireUpdate()
        {
            State stateBefore = CurrentState;

            if (OriginalState != CurrentState)
            {
                //PrevState = CurrentState;

                WireUpdate?.Invoke();
            }

            return stateBefore != CurrentState;
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
                return (int)Side ^ Distance;
            }

            public override string ToString()
            {
                return $"({Side},{Distance})";
            }
        }

        private DefaultDictionary<Pos, Pin> Pins = new DefaultDictionary<Pos, Pin>(() => new Pin());

        private HashSet<Wire> Wires = new HashSet<Wire>();

        private HashSet<IComponent> Components = new HashSet<IComponent>();
        private HashSet<IWireComponent> WireComponents = new HashSet<IWireComponent>();
        private HashSet<IComponent> NonWireComponents = new HashSet<IComponent>();
        private HashSet<IBoardInterfaceComponent> InterfaceComponents = new HashSet<IBoardInterfaceComponent>();
        private HashSet<IBoardInputComponent> InputComponents = new HashSet<IBoardInputComponent>();
        private HashSet<IBoardOutputComponent> OutputComponents = new HashSet<IBoardOutputComponent>();
        private List<IGraphicalComponent> GraphicalComponents = new List<IGraphicalComponent>();
        private HashSet<IBoardContainerComponent> ContainerComponents = new HashSet<IBoardContainerComponent>();

        private Board owner;

        public void SetOwnerBoard(Board owner)
        {
            this.owner = owner;
        }

        public void ResetOwnerBoard()
        {
            owner = null;
        }

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

        public IWireComponent[] GetWireComponents()
        {
            return WireComponents.ToArray();
        }

        public IComponent[] GetNonWireComponents()
        {
            return NonWireComponents.ToArray();
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

        public Board[] GetBoardList()
        {
            List<Board> checkedBoardList = new List<Board>(), uncheckedBoardList = new List<Board> { this };

            Func<Board, bool> notSeen = board => !checkedBoardList.Select(checkedBoard => checkedBoard.Name).Concat(uncheckedBoardList.Select(uncheckedBoard => uncheckedBoard.Name)).Contains(board.Name);

            while (uncheckedBoardList.Count > 0)
            {
                foreach (IBoardContainerComponent contComp in uncheckedBoardList[0].GetContainerComponents())
                {
                    if (notSeen(contComp.GetInternalBoard()))
                    {
                        uncheckedBoardList.Add(contComp.GetInternalBoard());
                    }
                }

                checkedBoardList.Add(uncheckedBoardList[0]);
                uncheckedBoardList.RemoveAt(0);
            }

            return checkedBoardList.ToArray();
        }

        public void TickSetup()
        {
            ClearUnusedPins();

            foreach (Pin pin in Pins.Values)
            {
                pin.SetupForTick();
            }

            foreach (IBoardContainerComponent boardContainerComp in ContainerComponents)
            {
                boardContainerComp.GetInternalBoard().TickSetup();
            }
        }

        public void TickComponents()
        {
            foreach (IComponent nonWireComp in NonWireComponents)
            {
                nonWireComp.Tick();
            }
        }

        private Func<T, bool> ActAndCheck<T>(Func<T, bool> action) where T : IComponent
        {
            return (T comp) =>
            {

                Pin.State[] startStates, endStates;

                startStates = comp.GetAllPinPositions().Select(pos => this[pos].GetStateForWire()).ToArray();

                bool returnVal = action(comp);

                endStates = comp.GetAllPinPositions().Select(pos => this[pos].GetStateForWire()).ToArray();

                for (int i = 0; i < startStates.Length; i++)
                {
                    returnVal |= startStates[i] != endStates[i];
                }

                return returnVal;
            };
        }

        /*
        private bool TickWireComp(IWireComponent wireComp)
        {
            Pin.State[] startStates, endStates;

            bool returnVal = false;

            startStates = wireComp.GetAllPinPositions().Select(pos => this[pos].GetStateForWire()).ToArray();

            wireComp.Tick();

            endStates = wireComp.GetAllPinPositions().Select(pos => this[pos].GetStateForWire()).ToArray();

            for (int i = 0; i < startStates.Length; i++)
            {
                returnVal |= startStates[i] != endStates[i];
            }

            return returnVal;
        }
        //*/

        private static bool Or(bool b1, bool b2) { return b1 || b2; }

        private bool SubTickJustWires()
        {
            return Pins.Values.Select(pin => pin.EmitWireUpdate())
                .Concat(ContainerComponents.Select(comp => comp.GetInternalBoard().SubTickJustWires()))
                .Aggregate(false, Or);
        }

        private bool SubTickJustWireComps()
        {
            return WireComponents.Select(ActAndCheck<IWireComponent>(comp => { comp.Tick(); return false; }))
                .Concat(ContainerComponents.Select(ActAndCheck<IBoardContainerComponent>(comp => comp.GetInternalBoard().SubTickJustWireComps())))
                .Aggregate(false, Or);
        }

        public bool SubTickWires()
        {
            //*
            try
            {
                return SubTickJustWires() | SubTickJustWireComps();
            } finally
            {
                
            }
            //*/

            /*
            return ContainerComponents.Select(comp => comp.GetInternalBoard().TickWires())
                .Concat(Pins.Values.Select(pin => pin.EmitWireUpdate()))
                .Concat(WireComponents.Select(TickWireComp))
                .Aggregate(false, (b1, b2) => b1 || b2);
            //*/
        }

        public void TickWires()
        {
            bool repeat;
            do
            {
                repeat = false;

                while (SubTickJustWires()) { repeat = true; }
                while (SubTickJustWireComps()) { repeat = true; }
            } while (repeat);
        }

        public void Tick()
        {
            TickSetup();

            TickComponents();

            TickWires();
            //while (SubTickWires()) { }
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
                    throw new PlacementException("cant place on another component. this error shouldn't be in the final product");
                }
            }

            Components.Add(comp);

            if (comp is IGraphicalComponent graphicalComp)
            {
                GraphicalComponents.Add(graphicalComp);
            }

            if (comp is IWireComponent wireComp)
            {
                WireComponents.Add(wireComp);
            } else
            {
                NonWireComponents.Add(comp);
            }

            if (comp is IBoardContainerComponent contComp)
            {
                ContainerComponents.Add(contComp);
            }

            if (comp is IBoardInterfaceComponent interfaceComp)
            {
                InterfaceLocation interfaceLoc = interfaceComp.GetInterfaceLocation();

                if (interfaceLoc.Distance == 0)
                {
                    int[] existingLocs = GetInterfaceComponents().Where(thisComp => thisComp.GetInterfaceLocation().Side == interfaceLoc.Side).Select(thisComp => thisComp.GetInterfaceLocation().Distance).ToArray();

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

                InterfaceComponents.Add(interfaceComp);

                if (comp is IBoardInputComponent inpComp)
                {
                    InputComponents.Add(inpComp);
                }

                if (comp is IBoardOutputComponent outpComp)
                {
                    OutputComponents.Add(outpComp);
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

            if (comp is IGraphicalComponent graphicalComp)
            {
                GraphicalComponents.Remove(graphicalComp);
            }

            if (comp is IWireComponent wireComp)
            {
                WireComponents.Remove(wireComp);
            } else
            {
                NonWireComponents.Remove(comp);
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
            }
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

            HashSet<Pos> removePinPositions = new HashSet<Pos>();

            removePinPositions.UnionWith(Pins.Keys);
            removePinPositions.ExceptWith(keepPinPositions);

            foreach (Pos pinPos in removePinPositions)
            {
                Pins.Remove(pinPos);
            }
        }

        public void Render(Graphics graphics, bool simulating, Rectangle bounds, ColourScheme colourScheme)
        {
            for (int x = bounds.Left; x <= bounds.Right; x++)
            {
                graphics.DrawLine(new Pen(colourScheme.Grid, 0.005F), x, bounds.Top, x, bounds.Bottom);
            }

            for (int y = bounds.Top; y <= bounds.Bottom; y++)
            {
                graphics.DrawLine(new Pen(colourScheme.Grid, 0.005F), bounds.Left, y, bounds.Right, y);
            }

            Pin pin;
            int connectionCount;
            List<Pos> compPins = new List<Pos>();

            foreach (IComponent comp in Components)
            {
                compPins.AddRange(comp.GetAllUniquePinPositions());
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

                    //*
                    if (simulating)
                    {
                        graphics.DrawString(pin.GetStateForDisplay().ToString(), new Font("arial", 0.1F), Brushes.Black, pinPos.X, pinPos.Y);
                    }
                    //*/
                }
            }

            graphics.DrawEllipse(new Pen(colourScheme.Grid, 0.01F), -0.1F, -0.1F, 0.2F, 0.2F);

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

                    matrix.Invert();
                    graphics.MultiplyTransform(matrix);
                }
            }

            foreach (Wire wire in Wires)
            {
                graphics.DrawLine(new Pen(simulating ? colourScheme.GetWireColour(wire.Pin1.GetStateForDisplay()) : colourScheme.Wire, 0.01F), new Point(wire.Pos1.X, wire.Pos1.Y), new Point(wire.Pos2.X, wire.Pos2.Y));
            }
        }

        public bool CheckAllowed(RectangleF bounds)
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
                pin.ResetToFloating();
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
            Pin pin;
            Wire[] wires;
            List<Pos> compPins;

            foreach (Pos pinPos in Pins.Keys)
            {
                pin = Pins[pinPos];

                foreach (Wire wire in pin.GetWires())
                {
                    if (wire.Pos1.Equals(wire.Pos2))
                    {
                        wire.Remove();
                    }
                }

                wires = pin.GetWires();
                compPins = new List<Pos>();

                foreach (IComponent comp in Components)
                {
                    compPins.AddRange(comp.GetAllUniquePinPositions());
                }

                if (wires.Length == 2 && compPins.Where(pinPos.Equals).Count() == 0 && wires[0].IsHori() == wires[1].IsHori() && wires[0].IsVert() == wires[1].IsVert() && (wires[0].IsHori() || wires[0].IsVert()))
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

            foreach (IBoardContainerComponent boardContainerComp in ContainerComponents)
            {
                boardContainerComp.GetInternalBoard().SimplifyWires();
            }
        }

        public Dictionary<Pos, Pin.State> GetStateToCheckForChanges()
        {
            return Pins.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.GetStateForDisplay());
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
            Board copy = new Board(copyName ?? Name, new Size(ExternalSize.Width, ExternalSize.Height));

            foreach (IComponent comp in Components)
            {
                comp.Copy().Place(comp.GetComponentPos(), comp.GetComponentRotation(), copy);
            }
            
            //private HashSet<IComponent> Components = new HashSet<IComponent>();
            //private HashSet<IWireComponent> WireComponents = new HashSet<IWireComponent>();
            //private HashSet<IComponent> NonWireComponents = new HashSet<IComponent>();
            //private HashSet<IBoardInterfaceComponent> InterfaceComponents = new HashSet<IBoardInterfaceComponent>();
            //private HashSet<IBoardInputComponent> InputComponents = new HashSet<IBoardInputComponent>();
            //private HashSet<IBoardOutputComponent> OutputComponents = new HashSet<IBoardOutputComponent>();
            //private List<IGraphicalComponent> GraphicalComponents = new List<IGraphicalComponent>();
            //private HashSet<IBoardContainerComponent> ContainerComponents = new HashSet<IBoardContainerComponent>();

            foreach (Wire wire in Wires)
            {
                new Wire(wire.Pos1, wire.Pos2, copy);
            }

            return copy;
        }
    }
}
