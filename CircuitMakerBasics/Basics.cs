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
using System.Windows.Forms;

namespace CircuitMaker.Basics
{
    class TransformRestorer : IDisposable
    {
        private Graphics SavedGraphics;
        private Matrix SavedMatrix;

        public TransformRestorer(Graphics graphics)
        {
            SavedGraphics = graphics;
            SavedMatrix = graphics.Transform;
        }

        public void Dispose()
        {
            SavedGraphics.Transform = SavedMatrix;
        }
    }

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

            if (comp is IGraphicalComponent graphicalComp)
            {
                bw.Write(graphicalComp.GetGraphicalElementScale());

                Point? graphicalLoc = graphicalComp.GetGraphicalElementLocation();
                bw.Write(graphicalLoc.HasValue);
                if (graphicalLoc.HasValue)
                {
                    bw.Write(graphicalLoc.Value.X);
                    bw.Write(graphicalLoc.Value.Y);
                }
            }

            if (comp is IBoardInterfaceComponent interfaceComp)
            {

            }
        }

        public static IComponent ReadComponent(this BinaryReader br, Board board)
        {
            string compType = br.ReadString();
            if (Constructors.TryGetValue(compType, out Func<string, IComponent> compFunc))
            {
                IComponent comp = compFunc(br.ReadString());
                comp.Place(br.ReadPos(), br.ReadEnum<Rotation>(), board);

                if (comp is IGraphicalComponent graphicalComp)
                {
                    graphicalComp.SetGraphicalElementScale(br.ReadSingle());

                    if (br.ReadBoolean())
                    {
                        graphicalComp.SetGraphicalElementLocation(new Point(br.ReadInt32(), br.ReadInt32()));
                    }
                }

                if (comp is IBoardInterfaceComponent interfaceComp)
                {

                }

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

            //board.SimplifyWires();

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
        private readonly Func<TValue> BlindGenerator;
        private readonly Func<TKey, TValue> KeyBasedGenerator;

        private readonly Func<TKey, bool> KeyKeepChecker;
        private readonly Func<TValue, bool> ValKeepChecker;

        public DefaultDictionary()
        {
            ValKeepChecker = val => val.Equals(default(TValue));
        }

        public DefaultDictionary(Func<TValue> generator)
        {
            BlindGenerator = generator;
        }

        public DefaultDictionary(Func<TKey, TValue> generator)
        {
            KeyBasedGenerator = generator;
        }

        public DefaultDictionary(Func<TValue> generator, Func<TValue, bool> valKeepChecker)
        {
            BlindGenerator = generator;
            ValKeepChecker = valKeepChecker;
        }

        public DefaultDictionary(Func<TKey, TValue> generator, Func<TValue, bool> valKeepChecker)
        {
            KeyBasedGenerator = generator;
            ValKeepChecker = valKeepChecker;
        }

        public DefaultDictionary(Func<TValue> generator, Func<TKey, bool> keyKeepChecker)
        {
            BlindGenerator = generator;
            KeyKeepChecker = keyKeepChecker;
        }

        public DefaultDictionary(Func<TKey, TValue> generator, Func<TKey, bool> keyKeepChecker)
        {
            KeyBasedGenerator = generator;
            KeyKeepChecker = keyKeepChecker;
        }

        public void TrimDown()
        {
            HashSet<TKey> removeKeys = new HashSet<TKey>();

            if (ValKeepChecker != null)
            {
                foreach (TKey key in Keys)
                {
                    if (!ValKeepChecker(base[key]))
                    {
                        removeKeys.Add(key);
                    }
                }
            } else if (KeyKeepChecker != null)
            {
                foreach (TKey key in Keys)
                {
                    if (!KeyKeepChecker(key))
                    {
                        removeKeys.Add(key);
                    }
                }
            }

            removeKeys.Select(Remove);
        }

        public new TValue this[TKey key]
        {
            get
            {
                try
                {
                    if (!ContainsKey(key))
                    {
                        if (BlindGenerator != null)
                        {
                            Add(key, BlindGenerator());
                        }
                        else if (KeyBasedGenerator != null)
                        {
                            Add(key, KeyBasedGenerator(key));
                        }
                        else
                        {
                            Add(key, default);
                        }
                    }

                    return base[key];
                } finally
                {
                    //TrimDown();
                }
            }
            set
            {
                base[key] = value;
                TrimDown();
            }
        }
    }

    public readonly struct Pos// : IEquatable<Pos>
    {
        public readonly int X;
        public readonly int Y;

        public Pos(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
        }

        //*
        public bool Equals(Pos other)
        {
            return X == other.X && Y == other.Y;
        }
        //*/

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

        public static bool operator ==(Pos pos1, Pos pos2)
        {
            return pos1.X == pos2.X && pos1.Y == pos2.Y;
        }

        public static bool operator !=(Pos pos1, Pos pos2)
        {
            return pos1.X != pos2.X || pos1.Y != pos2.Y;
        }

        public override bool Equals(object obj)
        {
            if (obj is Pos pos)
            {
                return this == pos;
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
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

        public bool IsPlaced { get; private set; }

        public Pin Pin1
        {
            get
            {
                if (!IsPlaced)
                {
                    throw new ObjectDisposedException("This wire has already been removed.");
                }

                return board[Pos1];
            }
        }

        public Pin Pin2
        {
            get 
            {
                if (!IsPlaced)
                {
                    throw new ObjectDisposedException("This wire has already been removed.");
                }

                return board[Pos2];
            }
        }

        public Wire(Pos pos1, Pos pos2, Board board)
        {
            Pos1 = pos1;
            Pos2 = pos2;

            this.board = board;

            //Pin1.WireUpdate += OnWireUpdate;
            //Pin2.WireUpdate += OnWireUpdate;

            board.AddWire(this);

            IsPlaced = true;
        }

        public void OnWireUpdate()
        {
            if (!IsPlaced)
            {
                throw new ObjectDisposedException("This wire has already been removed.");
            }

            //Pin.State new_state = Pin1.GetStateForWire().WireJoin(Pin2.GetStateForWire());

            //Pin1.SetState(new_state);
            //Pin2.SetState(new_state);

            Pin1.SetState(Pin2.GetStateForWire());
            Pin2.SetState(Pin1.GetStateForWire());
        }

        public bool TrySplitWire(Pos pos)
        {
            if (!IsPlaced)
            {
                throw new ObjectDisposedException("This wire has already been removed.");
            }

            if (Collision(pos))
            {
                new Wire(Pos1, pos, board);
                new Wire(Pos2, pos, board);

                Remove();

                return true;
            }

            return false;
        }

        public void Remove()
        {
            if (!IsPlaced)
            {
                throw new ObjectDisposedException("This wire has already been removed.");
            }

            //Pin1.WireUpdate -= OnWireUpdate;
            //Pin2.WireUpdate -= OnWireUpdate;

            board.RemoveWire(this);

            IsPlaced = false;
        }

        public override string ToString()
        {
            return IsPlaced ? $"[{Pos1}, {Pos2}]" : "Removed Wire";
        }

        public static bool operator ==(Wire wire1, Wire wire2)
        {
            if (wire1 is null && wire2 is null)
            {
                return true;
            }
            else if (wire1 is null || wire2 is null)
            {
                return false;
            }

            if (wire1.board != wire2.board)
            {
                return false;
            }

            return (wire1.Pos1 == wire2.Pos1 && wire1.Pos2 == wire2.Pos2) || (wire1.Pos1 == wire2.Pos2 && wire1.Pos2 == wire2.Pos1);
        }

        public static bool operator !=(Wire wire1, Wire wire2)
        {
            if (wire1 is null && wire2 is null)
            {
                return false;
            } else if (wire1 is null || wire2 is null)
            {
                return true;
            }

            return (wire1.Pos1 != wire2.Pos1 || wire1.Pos2 != wire2.Pos2) && (wire1.Pos1 != wire2.Pos2 || wire1.Pos2 != wire2.Pos1);
        }

        public override bool Equals(object obj)
        {
            if (obj is Wire wire)
            {
                return this == wire;
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Pos1.GetHashCode() + Pos2.GetHashCode();
        }

        public Rectangle Bounds()
        {
            if (!IsPlaced)
            {
                throw new ObjectDisposedException("This wire has already been removed.");
            }

            return Rectangle.FromLTRB(
                Math.Min(Pos1.X, Pos2.X), 
                Math.Min(Pos1.Y, Pos2.Y),
                Math.Max(Pos1.X, Pos2.X),
                Math.Max(Pos1.Y, Pos2.Y));
        }

        public RectangleF InflatedBounds()
        {
            if (!IsPlaced)
            {
                throw new ObjectDisposedException("This wire has already been removed.");
            }

            RectangleF bounds = Bounds();

            bounds.Inflate(0.25F, 0.25F);

            return bounds;
        }

        public bool IsVert()
        {
            if (!IsPlaced)
            {
                throw new ObjectDisposedException("This wire has already been removed.");
            }

            return Pos1.X == Pos2.X;
        }

        public bool IsHori()
        {
            if (!IsPlaced)
            {
                throw new ObjectDisposedException("This wire has already been removed.");
            }

            return Pos1.Y == Pos2.Y;
        }

        public bool Collision(Pos pos)
        {
            if (!IsPlaced)
            {
                throw new ObjectDisposedException("This wire has already been removed.");
            }

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

    public interface IWireComponent : IComponent
    {
        bool TickAgain();
    }
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

    public interface IGraphicalComponent : IComponent // go to the implementers and remove the graphical element saving
    {
        bool HasGraphics();
        void RenderGraphicalElement(Graphics graphics, bool simulating, ColourScheme colourScheme);

        RectangleF GetGraphicalElementBounds();

        Point? GetGraphicalElementLocation();
        void SetGraphicalElementLocation(Point? location);

        float GetGraphicalElementScale();
        void SetGraphicalElementScale(float scale);
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

        void PromiseDetails(Action<IBoardContainerComponent> detailProvider);
    }

    static class StateExtensions
    {
        /*
        private struct (Pin.State state1, Pin.State state2)
        {
            Pin.State State1, State2;

            public (Pin.State state1, Pin.State state2)((Pin.State state1, Pin.State state2) states)
            {
                State1 = states.state1; State2 = states.state2;
            }

            public (Pin.State state1, Pin.State state2)(Pin.State state1, Pin.State state2)
            {
                State1 = state1; State2 = state2;
            }

            public override bool Equals(object obj)
            {
                if (obj is (Pin.State state1, Pin.State state2) otherInp)
                {
                    return (State1 == otherInp.State1 && State2 == otherInp.State2) || (State1 == otherInp.State2 && State2 == otherInp.State1);
                }

                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return (int)State1 + (int)State2;
            }

            public static implicit operator (Pin.State state1, Pin.State state2)((Pin.State state1, Pin.State state2) states)
            {
                return new (Pin.State state1, Pin.State state2)(states.state1, states.state2);
            }

            public static bool operator ==((Pin.State state1, Pin.State state2) inp1, (Pin.State state1, Pin.State state2) inp2)
            {
                return inp1.Equals(inp2);
            }

            public static bool operator !=((Pin.State state1, Pin.State state2) inp1, (Pin.State state1, Pin.State state2) inp2)
            {
                return !inp1.Equals(inp2);
            }

            public override string ToString()
            {
                return $"{State1}, {State2}";
            }
        }
        //*/

        private class BinOpTable : Dictionary<(Pin.State state1, Pin.State state2), Pin.State>
        {
            private static (Pin.State state1, Pin.State state2) simplifyStates((Pin.State state1, Pin.State state2) states)
            {
                if (states.state1 > states.state2)
                {
                    return (states.state2, states.state1);
                }

                return states;
            }

            public new Pin.State this[(Pin.State state1, Pin.State state2) states]
            {
                get => base[simplifyStates(states)];
                set => base[simplifyStates(states)] = value;
            }

            public Pin.State this[Pin.State state1, Pin.State state2]
            {
                get => base[simplifyStates((state1, state2))];
                set => base[simplifyStates((state1, state2))] = value;
            }

            public BinOpTable(IEnumerable<KeyValuePair<(Pin.State state1, Pin.State state2), Pin.State>> kvps) : base(kvps.ToDictionary(kvp => simplifyStates(kvp.Key), kvp => kvp.Value)) { }

            public BinOpTable(IDictionary<(Pin.State state1, Pin.State state2), Pin.State> dict) : base(dict.ToDictionary(kvp => simplifyStates(kvp.Key), kvp => kvp.Value)) { }

            public BinOpTable() : base() { }

            public new void Add((Pin.State state1, Pin.State state2) key, Pin.State val)
            {
                base.Add(simplifyStates(key), val);
            }
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

        private static Dictionary<(Pin.State state1, Pin.State state2), Pin.State> GenericOpTable = new Dictionary<(Pin.State state1, Pin.State state2), Pin.State>
        {
            { (Pin.State.FLOATING, Pin.State.FLOATING), Pin.State.FLOATING },
            { (Pin.State.FLOATING, Pin.State.LOW), Pin.State.LOW },
            { (Pin.State.FLOATING, Pin.State.HIGH), Pin.State.HIGH },
            { (Pin.State.FLOATING, Pin.State.ILLEGAL), Pin.State.ILLEGAL },
            { (Pin.State.ILLEGAL, Pin.State.LOW), Pin.State.ILLEGAL },
            { (Pin.State.ILLEGAL, Pin.State.HIGH), Pin.State.ILLEGAL },
            { (Pin.State.ILLEGAL, Pin.State.ILLEGAL), Pin.State.ILLEGAL }
        };

        private static BinOpTable AndOpTable = new BinOpTable(new Dictionary<(Pin.State state1, Pin.State state2), Pin.State>
        {
            { (Pin.State.LOW, Pin.State.LOW), Pin.State.LOW },
            { (Pin.State.LOW, Pin.State.HIGH), Pin.State.LOW },
            { (Pin.State.HIGH, Pin.State.HIGH), Pin.State.HIGH }
        }.Concat(GenericOpTable));
        private static BinOpTable OrOpTable = new BinOpTable(new Dictionary<(Pin.State state1, Pin.State state2), Pin.State>
        {
            { (Pin.State.LOW, Pin.State.LOW), Pin.State.LOW },
            { (Pin.State.LOW, Pin.State.HIGH), Pin.State.HIGH },
            { (Pin.State.HIGH, Pin.State.HIGH), Pin.State.HIGH }
        }.Concat(GenericOpTable));
        private static BinOpTable XorOpTable = new BinOpTable(new Dictionary<(Pin.State state1, Pin.State state2), Pin.State>
        {
            { (Pin.State.LOW, Pin.State.LOW), Pin.State.LOW },
            { (Pin.State.LOW, Pin.State.HIGH), Pin.State.HIGH },
            { (Pin.State.HIGH, Pin.State.HIGH), Pin.State.LOW }
        }.Concat(GenericOpTable));

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
            { (Pin.State.PULLEDLOW,     Pin.State.PULLEDHIGH),  Pin.State.ILLEGAL },
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
            CurrentState = CurrentState.WireJoin(state);
        }

        public void ResetToFloating()
        {
            CurrentState = State.FLOATING;
            OriginalState = State.FLOATING;
        }

        public void SetupForTick()
        {
            OriginalState = CurrentState;
            CurrentState = Pin.State.FLOATING;
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

        private DefaultDictionary<Pos, Pin> Pins;
        private HashSet<Wire> Wires = new HashSet<Wire>();
        private HashSet<Wire> AllWires = new HashSet<Wire>();
        private DefaultDictionary<Pos, int> ConnectionsToPin = new DefaultDictionary<Pos, int>();

        private HashSet<IComponent> Components = new HashSet<IComponent>();
        private HashSet<IWireComponent> WireComponents = new HashSet<IWireComponent>();
        private HashSet<IWireComponent> AllWireComponents = new HashSet<IWireComponent>();
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

            Pins = new DefaultDictionary<Pos, Pin>(() => new Pin(), (pos) => ConnectionsToPin[pos] != 0);
        }

        public void AddWire(Wire wire)
        {
            Wires.Add(wire);
            AllWires.Add(wire);

            ConnectionsToPin[wire.Pos1]++;
            ConnectionsToPin[wire.Pos2]++;
        }

        public void RemoveWire(Wire wire)
        {
            Wires.Remove(wire);
            AllWires.Remove(wire);

            ConnectionsToPin[wire.Pos1]--;
            ConnectionsToPin[wire.Pos2]--;
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

        private bool SubTickWire(Wire wire)
        {
            Pin pin1 = wire.Pin1, pin2 = wire.Pin2;
            //Pin pin1 = Pins[wire.Pos1], pin2 = Pins[wire.Pos2];
            Pin.State state1 = pin1.GetStateForWire(), state2 = pin2.GetStateForWire();

            if (state1 != state2)
            {
                //return false;

                //wire.Pin1.SetState(wire.Pin2.GetStateForWire());
                //wire.Pin2.SetState(wire.Pin1.GetStateForWire());

                pin1.SetState(state2);
                pin2.SetState(state1);

                return true;
            }

            return false;
        }

        /*
        private bool SubTickJustWires()
        {
            return //Pins.Values.Select(pin => pin.EmitWireUpdate())
                Wires.Select(SubTickWire)
                .Concat(ContainerComponents.Select(comp => comp.GetInternalBoard().SubTickJustWires()))
                .Aggregate(false, Or);
        }

        private bool SubTickJustWireComps()
        {
            return WireComponents.Select(ActAndCheck<IWireComponent>(comp => { comp.Tick(); return false; }))
                .Concat(ContainerComponents.Select(ActAndCheck<IBoardContainerComponent>(comp => comp.GetInternalBoard().SubTickJustWireComps())))
                .Aggregate(false, Or);
        }
        //*/

        public bool SubTickWires()
        {
            /*
            return Wires.Select(SubTickWire)
                //.Concat(ContainerComponents.Select(comp => comp.GetInternalBoard().SubTickJustWires()))
                .Concat(WireComponents.Select(ActAndCheck<IWireComponent>(comp => { comp.Tick(); return false; })))
                .Concat(ContainerComponents.Select(ActAndCheck<IBoardContainerComponent>(comp => comp.GetInternalBoard().SubTickWires())))
                .Aggregate(false, Or);
            //*/

            /*
            return AllWires.Select(SubTickWire)
                //.Concat(ContainerComponents.Select(comp => comp.GetInternalBoard().SubTickJustWires()))
                //.Concat(AllWireComponents.Select(ActAndCheck<IWireComponent>(comp => { comp.Tick(); return false; })))
                .Concat(AllWireComponents.Select(comp => { comp.Tick(); return comp.TickAgain(); }))
                //.Concat(ContainerComponents.Select(comp => comp.GetInternalBoard().SubTickWires()))
                //.Concat(ContainerComponents.Select(ActAndCheck<IBoardContainerComponent>(comp => comp.GetInternalBoard().SubTickWires())))
                .Aggregate(false, Or);
            //*/

            bool retVal = false;

            foreach (Wire wire in AllWires)
            {
                retVal |= SubTickWire(wire);
            }

            foreach (IWireComponent wireComp in AllWireComponents)
            {
                wireComp.Tick();
                retVal |= wireComp.TickAgain();
            }

            return retVal;
        }

        public void TickWires()
        {
            while (SubTickWires()) { }

            /*
            bool repeat;
            do
            {
                repeat = false;

                while (SubTickJustWires()) { repeat = true; }
                while (SubTickJustWireComps()) { repeat = true; }
            } while (repeat);
            //*/
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

        private void CountComponentPins(IComponent comp)
        {
            //comp.GetAllUniquePinPositions().Select(pos => ConnectionsToPin[pos]++).ToArray();

            foreach (Pos pos in comp.GetAllUniquePinPositions())
            {
                ConnectionsToPin[pos]++;
            }
        }

        private void AddContainedBoardWires(IBoardContainerComponent comp)
        {
            AllWires.UnionWith(comp.GetInternalBoard().AllWires);
            AllWireComponents.UnionWith(comp.GetInternalBoard().AllWireComponents);

            if (!(owner is null))
            {
                owner.AddContainedBoardWires(comp);
            }
        }

        internal void AddComponent(IComponent comp)
        {
            foreach (IComponent otherComp in Components)
            {
                if (otherComp.GetOffsetComponentBounds().IntersectsWith(comp.GetOffsetComponentBounds()))
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
                AllWireComponents.Add(wireComp);
            } else
            {
                NonWireComponents.Add(comp);
            }

            if (comp is IBoardContainerComponent contComp)
            {
                ContainerComponents.Add(contComp);

                contComp.PromiseDetails(CountComponentPins);
                contComp.PromiseDetails(AddContainedBoardWires);
            } else
            {
                CountComponentPins(comp);
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
            comp.GetAllUniquePinPositions().Select(pos => ConnectionsToPin[pos]--);

            Components.Remove(comp);

            if (comp is IGraphicalComponent graphicalComp)
            {
                GraphicalComponents.Remove(graphicalComp);
            }

            if (comp is IWireComponent wireComp)
            {
                WireComponents.Remove(wireComp);
                AllWireComponents.Remove(wireComp);
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

                AllWires.ExceptWith(contComp.GetInternalBoard().AllWires);
                AllWireComponents.ExceptWith(contComp.GetInternalBoard().AllWireComponents);
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
            Pen gridPen = new Pen(colourScheme.Grid, 0.005F);

            for (int x = bounds.Left; x <= bounds.Right; x++)
            {
                graphics.DrawLine(gridPen, x, bounds.Top, x, bounds.Bottom);
            }

            for (int y = bounds.Top; y <= bounds.Bottom; y++)
            {
                graphics.DrawLine(gridPen, bounds.Left, y, bounds.Right, y);
            }

            Pin pin;

            foreach (Pos pinPos in Pins.Keys)
            {
                if (bounds.Contains(pinPos.X, pinPos.Y))
                {
                    pin = Pins[pinPos];

                    if (ConnectionsToPin[pinPos] == 0)
                    {
                        continue;
                    }

                    if (ConnectionsToPin[pinPos] != 2)
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

            Matrix matrix;// = new Matrix();

            foreach (IComponent comp in Components)
            {
                if (comp.GetOffsetComponentBounds().IntersectsWith(bounds))
                {
                    using (new TransformRestorer(graphics))
                    {
                        matrix = comp.GetRenderMatrix();
                        graphics.MultiplyTransform(matrix);

                        comp.Render(graphics, simulating, colourScheme);

                        if (comp is IGraphicalComponent graphicalComp)
                        {
                            graphicalComp.RenderGraphicalElement(graphics, simulating, colourScheme);
                        }

                        RectangleF compBounds = comp.GetComponentBounds();
                    }
                    
                    //matrix.Invert();
                    //graphics.MultiplyTransform(matrix);
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

        /*
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
                    if (wire.Pos1 == wire.Pos2)
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
                    if (wires[0].Pos1 == wires[1].Pos1 && wires[0].Pos2 != wires[1].Pos2)
                    {
                        new Wire(wires[0].Pos2, wires[1].Pos2, this);
                    } else if (wires[0].Pos2 == wires[1].Pos1 && wires[0].Pos1 != wires[1].Pos2)
                    {
                        new Wire(wires[0].Pos1, wires[1].Pos2, this);
                    } else if (wires[0].Pos1 == wires[1].Pos2 && wires[0].Pos2 != wires[1].Pos1)
                    {
                        new Wire(wires[0].Pos2, wires[1].Pos1, this);
                    } else if (wires[0].Pos2 == wires[1].Pos2 && wires[0].Pos1 != wires[1].Pos1)
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
        //*/

        private bool TrySimplifyWirePair(Wire wire1, Wire wire2)
        {
            Func<Pos, int> getCompOrd, getOtherOrd;
            Func<int, int, Pos> backToPos;
            int[] compOrd;
            int min, max, otherOrd;

            if (wire1.TrySplitWire(wire2.Pos1) ||
                wire1.TrySplitWire(wire2.Pos2) ||
                wire2.TrySplitWire(wire1.Pos1) ||
                wire2.TrySplitWire(wire1.Pos2))
            {
                return true;
            }

            if ((ConnectionsToPin[wire1.Pos1] == 2 && (wire1.Pos1 == wire2.Pos1 || wire1.Pos1 == wire2.Pos2)) ||
                (ConnectionsToPin[wire1.Pos2] == 2 && (wire1.Pos2 == wire2.Pos1 || wire1.Pos2 == wire2.Pos2)))
            {
                if (wire1.IsHori() && wire2.IsHori())
                {
                    getCompOrd = pos => pos.Y;
                    getOtherOrd = pos => pos.X;
                    backToPos = (comp, other) => new Pos(other, comp);
                }
                else if (wire1.IsHori() && wire2.IsHori())
                {
                    getCompOrd = pos => pos.X;
                    getOtherOrd = pos => pos.Y;
                    backToPos = (comp, other) => new Pos(comp, other);
                }
                else
                {
                    return false;
                }

                compOrd = new int[] { getCompOrd(wire1.Pos1), getCompOrd(wire1.Pos2), getCompOrd(wire2.Pos1), getCompOrd(wire2.Pos2) };
                otherOrd = getOtherOrd(wire1.Pos1);

                min = compOrd.Aggregate(Math.Min);
                max = compOrd.Aggregate(Math.Max);

                new Wire(backToPos(min, otherOrd), backToPos(max, otherOrd), this);

                wire1.Remove();
                wire2.Remove();

                return true;
            }

            return false;
        }

        public void SimplifyWires()
        {
            //DefaultDictionary<Pos, int> wiresOnPoint = new DefaultDictionary<Pos, int>();

            HashSet<Wire> removeWires = new HashSet<Wire>();

            Wire[] wires;

            HashSet<Wire> addWires = new HashSet<Wire>();

            //bool removed;


            //removed = false;

            bool changeMade;

            do
            {
                changeMade = false;

                foreach (Wire wire in Wires)
                {
                    if (wire.Pos1 == wire.Pos2)
                    {
                        removeWires.Add(wire);
                    }
                }

                foreach (Wire removeWire in removeWires)
                {
                    removeWire.Remove();
                }

                removeWires.Clear();

                wires = new Wire[Wires.Count];
                Wires.CopyTo(wires);

                for (int i = 0; i < wires.Length - 1; i++)
                {
                    for (int j = i + 1; j < wires.Length; j++)
                    {
                        changeMade = TrySimplifyWirePair(wires[i], wires[j]);

                        if (changeMade)
                        {
                            break;
                        }
                    }

                    if (changeMade)
                    {
                        break;
                    }
                }
            } while (changeMade);

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
            return $"Board: {Name}";
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
