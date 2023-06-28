using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CircuitMaker.Basics
{
    //*
    static class ReadWriteImplementation
    {
        static void Write(this BinaryWriter bw, Pos pos)
        {
            bw.Write(pos.X);
            bw.Write(pos.Y);
        }

        static Pos ReadPos(this BinaryReader br)
        {
            return new Pos(br.ReadInt32(), br.ReadInt32());
        }


        static void Write(this BinaryWriter bw, Wire wire)
        {
            bw.Write(wire.Pos1);
            bw.Write(wire.Pos2);
        }

        static Wire ReadWire(this BinaryReader br, Board board)
        {
            return new Wire(br.ReadPos(), br.ReadPos(), board);
        }


        static void Write(this BinaryWriter bw, IComponent comp)
        {
            bw.Write(comp.GetComponentID());
            bw.Write(comp.GetComponentDetails());
            bw.Write(comp.GetComponentPos());
        }

        static IComponent ReadComponent(this BinaryReader br, Board board)
        {
            Func<string, IComponent> compFunc;

            if (Constructors.TryGetValue(br.ReadString(), out compFunc))
            {
                IComponent comp = compFunc(br.ReadString());
                comp.Place(br.ReadPos(), board);
                return comp;
            }

            br.ReadString(); // if couldn't find the chip, just ignore it.
            br.ReadPos();

            return null;
        }


        public static void Write(this BinaryWriter bw, Board board)
        {
            bw.Write(board.GetBoardName());

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
            Board board = new Board(br.ReadString());

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

    readonly struct Pos : IEquatable<Pos>
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

        /*
        void Write(this BinaryWriter bw)
        {
            bw.Write(X);
            bw.Write(Y);
        }

        static Pos ReadPos(this BinaryReader br)
        {
            return new Pos(br.ReadInt32(), br.ReadInt32());
        }//*/
    }

    readonly struct Wire
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
        }

        public override string ToString()
        {
            return $"[{Pos1}, {Pos2}]";
        }
    }

    interface IComponent // implement way to define clearance?
    {
        void Place(Pos pos, Board board);
        void Remove();

        void Tick();

        Pos[] GetAllPinPositions();

        Pos GetComponentPos();
        Board GetComponentBoard();

        string GetComponentID();
        string GetComponentDetails();
    }

    interface IInteractibleComponent : IComponent
    {
        void Interact();
    }

    interface IBoardInputComponent : IComponent
    {
        string GetComponentName();
        void SetInputState(Pin.State state);
    }

    interface IBoardOutputComponent : IComponent
    {
        string GetComponentName();
        Pin.State GetOutputState();
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

        /*
        public static Pin.State Not(this Pin.State state)
        {
            switch (state)
            {
                case Pin.State.FLOATING: return Pin.State.FLOATING;
                case Pin.State.LOW: return Pin.State.HIGH;
                case Pin.State.HIGH: return Pin.State.LOW;
                case Pin.State.ILLEGAL: return Pin.State.ILLEGAL;
                default: return Pin.State.ILLEGAL; // shouldn't be possible, so it's illegal anyway
            }
        }

        public static Pin.State Or(this Pin.State state1, Pin.State state2)
        {
            if (state1 == Pin.State.ILLEGAL || state2 == Pin.State.ILLEGAL)
            {
                return Pin.State.ILLEGAL;
            }

            if (state1 == Pin.State.FLOATING || state2 == Pin.State.FLOATING)
            {
                return Pin.State.FLOATING;
            }

            if (state1 == Pin.State.LOW && state2 == Pin.State.LOW)
            {
                return Pin.State.LOW;
            }

            return Pin.State.HIGH;
        }

        public static Pin.State And(this Pin.State state1, Pin.State state2)
        {
            if (state1 == Pin.State.ILLEGAL || state2 == Pin.State.ILLEGAL)
            {
                return Pin.State.ILLEGAL;
            }

            if (state1 == Pin.State.FLOATING || state2 == Pin.State.FLOATING)
            {
                return Pin.State.FLOATING;
            }

            if (state1 == Pin.State.HIGH && state2 == Pin.State.HIGH)
            {
                return Pin.State.HIGH;
            }

            return Pin.State.LOW;
        }

        public static Pin.State Xor(this Pin.State state1, Pin.State state2)
        {
            if (state1 == Pin.State.ILLEGAL || state2 == Pin.State.ILLEGAL)
            {
                return Pin.State.ILLEGAL;
            }

            if (state1 == Pin.State.FLOATING || state2 == Pin.State.FLOATING)
            {
                return Pin.State.FLOATING;
            }

            if (state1 == state2)
            {
                return Pin.State.LOW;
            }

            return Pin.State.HIGH;
        }//*/
    }

    class Pin
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

    class Board
    {
        private DefaultDictionary<Pos, Pin> Pins = new DefaultDictionary<Pos, Pin>(() => new Pin());

        private HashSet<IComponent> Components = new HashSet<IComponent>();

        private Dictionary<string, IBoardInputComponent> InputComponents = new Dictionary<string, IBoardInputComponent>();
        private Dictionary<string, IBoardOutputComponent> OutputComponents = new Dictionary<string, IBoardOutputComponent>();

        private string Name;

        public string GetBoardName()
        {
            return Name;
        }

        public Board(string name)
        {
            Name = name;
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

        public Wire[] GetAllWires()
        {
            HashSet<Wire> wires = new HashSet<Wire>();

            ClearUnusedPins();

            foreach (Pin pin in Pins.Values)
            {
                wires.UnionWith(pin.GetWires());
            }

            return wires.ToArray();
        }

        public IComponent[] GetComponents()
        {
            return Components.ToArray();
        }

        public IBoardInputComponent[] GetInputComponents()
        {
            return InputComponents.Values.ToArray();
        }

        public IBoardInputComponent GetInputComponent(string name)
        {
            return InputComponents.ContainsKey(name) ? InputComponents[name] : null;
            //return InputComponents.TryGetValue(name, out IBoardInputComponent comp) ? comp : null;
        }

        public IBoardOutputComponent[] GetOutputComponents()
        {
            return OutputComponents.Values.ToArray();
        }

        public IBoardOutputComponent GetOutputComponent(string name)
        {
            return OutputComponents.ContainsKey(name) ? OutputComponents[name] : null;
            //return OutputComponents.TryGetValue(name, out IBoardOutputComponent comp) ? comp : null;
        }

        //public bool EmitComponentUpdate()
        //{
        //    bool returnVal = false;
        //
        //    foreach (Component comp in components)
        //    {
        //        returnVal |= comp.Tick();
        //    }
        //
        //    return returnVal;
        //}

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

            //while (EmitComponentUpdate()) { }
            while (EmitWireUpdate()) { }
        }

        public Pin this[Pos pos] => Pins[pos];

        internal void AddComponent(IComponent comp)
        {
            Components.Add(comp);

            if (comp is IBoardInputComponent inpComponent)
            {
                InputComponents.Add(inpComponent.GetComponentName(), inpComponent);
            }

            if (comp is IBoardOutputComponent outpComponent)
            {
                OutputComponents.Add(outpComponent.GetComponentName(), outpComponent);
            }
        }

        internal void RemoveComponent(IComponent comp)
        {
            Components.Remove(comp);

            if (comp is IBoardInputComponent inpComponent)
            {
                InputComponents.Remove(inpComponent.GetComponentName());
            }

            if (comp is IBoardOutputComponent outpComponent)
            {
                OutputComponents.Remove(outpComponent.GetComponentName());
            }
        }

        protected void ClearUnusedPins()
        {
            HashSet<Pos> keepPinPositions = new HashSet<Pos>();

            foreach (IComponent comp in Components)
            {
                keepPinPositions.UnionWith(comp.GetAllPinPositions());
            }

            foreach (Pos pinPos in Pins.Keys)
            {
                if (!Pins[pinPos].HasWires() && !keepPinPositions.Contains(pinPos))
                {
                    Pins.Remove(pinPos);
                }
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("components: ");

            Func<string, string, string> listBuilder = (first, second) => first + ", " + second;

            sb.Append(Components.Select(comp => comp.ToString()).Aggregate(listBuilder));
            sb.Append(", wires: ");
            sb.Append(GetAllWires().Select(wire => wire.ToString()).Aggregate(listBuilder));

            return sb.ToString();
        }

        private static string GetFilename(string boardname)
        {
            return $"Boards/{boardname}.brd";
        }

        public void Save()
        {
            using (FileStream file = File.Open(GetFilename(Name), FileMode.Create))
            {
                using (BinaryWriter bw = new BinaryWriter(file))
                {
                    //ReadWriteImplementation.Write(bw, this);
                    bw.Write(this);
                }
            }
        }

        public static Board Load(string boardname)
        {
            using (FileStream file = File.Open(GetFilename(boardname), FileMode.Open))
            {
                using (BinaryReader br = new BinaryReader(file))
                {
                    return br.ReadBoard();
                }
            }
        }
    }//*/
}
