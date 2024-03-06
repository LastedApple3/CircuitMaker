using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CircuitMaker.Basics;
using CircuitMaker.Components;
using CircuitMaker.GUI;
using CircuitMaker.GUI.Settings;
using CircuitMaker.GUI.ExtApp;
using System.IO;
using System.Runtime.CompilerServices;
using System.Data.SqlClient;
using System.Security.Cryptography.X509Certificates;

namespace CircuitMaker
{
    readonly struct WirePointInfo
    {
        public enum WirePointType
        {
            OrdInfo, SkipBack
        }

        [Flags]
        public enum OrdInfo
        {
            Stored = 1, Prev = 2, Next = 4, Exact = 8, Offset = 16,

            ExactStored = Exact | Stored,
            ExactPrev = Exact | Prev,
            ExactNext = Exact | Next,
            OffsetPrev = Offset | Prev,
            OffsetNext = Offset | Next,
        }

        public readonly WirePointType PointType;

        public readonly int? SkipBackBy, StoredX, StoredY;

        public readonly OrdInfo XInfo, YInfo;

        public WirePointInfo(WirePointType type = WirePointType.OrdInfo, int? skipBack = null, OrdInfo xInfo = OrdInfo.ExactStored, OrdInfo yInfo = OrdInfo.ExactStored, int? xStore = null, int? yStore = null)
        {
            PointType = type;
            SkipBackBy = skipBack;
            XInfo = xInfo;
            YInfo = yInfo;
            StoredX = xStore;
            StoredY = yStore;
        }

        private static string FormatOrdinate(OrdInfo info, int? stored)
        {
            string relTo;

            if (info.HasFlag(OrdInfo.Prev))
            {
                relTo = "previous";
            }
            else if (info.HasFlag(OrdInfo.Next))
            {
                relTo = "next";
            }
            else
            {
                relTo = null;
            }

            int? dispInt = (info.HasFlag(OrdInfo.Offset) || info.HasFlag(OrdInfo.Stored)) ? stored : null;

            string formatted = relTo ?? "";

            if (relTo != null && dispInt.HasValue)
            {
                formatted += " ";
                formatted += dispInt.Value < 0 ? "-" : "+";
                formatted += " ";
            }

            if (dispInt.HasValue)
            {
                formatted += Math.Abs(dispInt.Value);
            }

            return formatted;
        }

        public override string ToString()
        {
            switch (PointType)
            {
                case WirePointType.OrdInfo:
                    return $"{FormatOrdinate(XInfo, StoredX)}, {FormatOrdinate(YInfo, StoredY)}";
                case WirePointType.SkipBack:
                    return $"Skip Back {SkipBackBy} places";
                default:
                    return "WirePointInfo";
            }
            
        }

        public static implicit operator WirePointInfo(Pos pos)
        {
            return new WirePointInfo(xStore: pos.X, yStore: pos.Y);
        }

        public static implicit operator WirePointInfo((int xStore, int yStore) tup)
        {
            return new WirePointInfo(xStore: tup.xStore, yStore: tup.yStore);
        }

        public static implicit operator WirePointInfo((int xStore, OrdInfo yInfo) tup)
        {
            return new WirePointInfo(xStore: tup.xStore, yInfo: tup.yInfo);
        }

        public static implicit operator WirePointInfo((int xStore, OrdInfo yInfo, int yStore) tup)
        {
            return new WirePointInfo(xStore: tup.xStore, yInfo: tup.yInfo, yStore: tup.yStore);
        }

        public static implicit operator WirePointInfo((OrdInfo xInfo, int yStore) tup)
        {
            return new WirePointInfo(xInfo: tup.xInfo, yStore: tup.yStore);
        }

        public static implicit operator WirePointInfo((OrdInfo xInfo, OrdInfo yInfo) tup)
        {
            return new WirePointInfo(xInfo: tup.xInfo, yInfo: tup.yInfo);
        }

        public static implicit operator WirePointInfo((OrdInfo xInfo, OrdInfo yInfo, int yStore) tup)
        {
            return new WirePointInfo(xInfo: tup.xInfo, yInfo: tup.yInfo, yStore: tup.yStore);
        }

        public static implicit operator WirePointInfo((OrdInfo xInfo, int xStore, int yStore) tup)
        {
            return new WirePointInfo(xInfo: tup.xInfo, xStore: tup.xStore, yStore: tup.yStore);
        }

        public static implicit operator WirePointInfo((OrdInfo xInfo, int xStore, OrdInfo yInfo) tup)
        {
            return new WirePointInfo(xInfo: tup.xInfo, xStore: tup.xStore, yInfo: tup.yInfo);
        }

        public static implicit operator WirePointInfo((OrdInfo xInfo, int xStore, OrdInfo yInfo, int yStore) tup)
        {
            return new WirePointInfo(xInfo: tup.xInfo, xStore: tup.xStore, yInfo: tup.yInfo, yStore: tup.yStore);
        }

        public static implicit operator WirePointInfo((WirePointType type, int xStore, int yStore) tup)
        {
            return new WirePointInfo(type: tup.type, xStore: tup.xStore, yStore: tup.yStore);
        }

        public static implicit operator WirePointInfo((WirePointType type, int xStore, OrdInfo yInfo) tup)
        {
            return new WirePointInfo(type: tup.type, xStore: tup.xStore, yInfo: tup.yInfo);
        }

        public static implicit operator WirePointInfo((WirePointType type, int xStore, OrdInfo yInfo, int yStore) tup)
        {
            return new WirePointInfo(type: tup.type, xStore: tup.xStore, yInfo: tup.yInfo, yStore: tup.yStore);
        }

        public static implicit operator WirePointInfo((WirePointType type, OrdInfo xInfo, int yStore) tup)
        {
            return new WirePointInfo(type: tup.type, xInfo: tup.xInfo, yStore: tup.yStore);
        }

        public static implicit operator WirePointInfo((WirePointType type, OrdInfo xInfo, OrdInfo yInfo) tup)
        {
            return new WirePointInfo(type: tup.type, xInfo: tup.xInfo, yInfo: tup.yInfo);
        }

        public static implicit operator WirePointInfo((WirePointType type, OrdInfo xInfo, OrdInfo yInfo, int yStore) tup)
        {
            return new WirePointInfo(type: tup.type, xInfo: tup.xInfo, yInfo: tup.yInfo, yStore: tup.yStore);
        }

        public static implicit operator WirePointInfo((WirePointType type, OrdInfo xInfo, int xStore, int yStore) tup)
        {
            return new WirePointInfo(type: tup.type, xInfo: tup.xInfo, xStore: tup.xStore, yStore: tup.yStore);
        }

        public static implicit operator WirePointInfo((WirePointType type, OrdInfo xInfo, int xStore, OrdInfo yInfo) tup)
        {
            return new WirePointInfo(type: tup.type, xInfo: tup.xInfo, xStore: tup.xStore, yInfo: tup.yInfo);
        }

        public static implicit operator WirePointInfo((WirePointType type, OrdInfo xInfo, int xStore, OrdInfo yInfo, int yStore) tup)
        {
            return new WirePointInfo(type: tup.type, xInfo: tup.xInfo, xStore: tup.xStore, yInfo: tup.yInfo, yStore: tup.yStore);
        }

        public static implicit operator WirePointInfo((WirePointType type, int skipBack) tup)
        {
            return new WirePointInfo(type: tup.type, skipBack: tup.skipBack);
        }

    }

    static class WirePointInfoExtensions
    {
        private static int[] ResolveOrds((WirePointInfo.WirePointType type, int? skipBack, WirePointInfo.OrdInfo info, int? stored)[] ordDatas)
        {
            int?[] rawData = new int?[ordDatas.Length];
            (WirePointInfo.WirePointType type, int? skipBack, WirePointInfo.OrdInfo info, int? stored) ordData;

            for (int i = 0; i < ordDatas.Length; i++)
            {
                ordData = ordDatas[i];

                if (ordData.type == WirePointInfo.WirePointType.OrdInfo)
                {
                    if (i == 0 && (ordData.info.HasFlag(WirePointInfo.OrdInfo.Prev)))
                    {
                        throw new ArgumentException("the first element cannot contain a reference to the previous element");
                    }

                    if (i == ordDatas.Length - 1 && (ordData.info.HasFlag(WirePointInfo.OrdInfo.Next)))
                    {
                        throw new ArgumentException("the last element cannot contain a reference to the next element");
                    }

                    if (ordData.info.HasFlag(WirePointInfo.OrdInfo.ExactStored))
                    {
                        if (ordData.stored.HasValue)
                        {
                            rawData[i] = ordData.stored.Value;
                        }
                        else
                        {
                            throw new ArgumentException("no element can reference a stored ordinate it doesn't have");
                        }
                    }

                    if (ordData.info.HasFlag(WirePointInfo.OrdInfo.Offset) && !ordData.stored.HasValue)
                    {
                        throw new ArgumentException("no element can reference a stored ordinate it doesn't have");
                    }
                }
            }

            int?[] prev = new int?[ordDatas.Length];
            int? refInt;

            do
            {
                rawData.CopyTo(prev, 0);

                for (int i = 0; i < ordDatas.Count(); i++)
                {
                    ordData = ordDatas[i];

                    refInt = null;

                    if (!rawData[i].HasValue)
                    {
                        if (ordData.type == WirePointInfo.WirePointType.OrdInfo)
                        {
                            if (ordData.info.HasFlag(WirePointInfo.OrdInfo.Prev) && rawData[i - 1].HasValue)
                            {
                                refInt = rawData[i - 1].Value;
                            }

                            if (ordData.info.HasFlag(WirePointInfo.OrdInfo.Next) && rawData[i + 1].HasValue)
                            {
                                refInt = rawData[i + 1].Value;
                            }

                            if (refInt.HasValue)
                            {
                                if (ordData.info.HasFlag(WirePointInfo.OrdInfo.Exact))
                                {
                                    rawData[i] = refInt;
                                }

                                if (ordData.info.HasFlag(WirePointInfo.OrdInfo.Offset))
                                {
                                    rawData[i] = refInt + ordData.stored;
                                }
                            }
                        } else if (ordData.type == WirePointInfo.WirePointType.SkipBack)
                        {
                            if (!ordData.skipBack.HasValue)
                            {
                                throw new ArgumentException("Cannot skip back without an amount to skip back by.");
                            } else if (i < ordData.skipBack)
                            {
                                throw new ArgumentException("Cannot skip back to a point that is before the start of the list.");
                            } else if (ordData.skipBack <= 0)
                            {
                                throw new ArgumentException("Cannot skip forward or to the same point.");
                            } else if (rawData[i - ordData.skipBack.Value].HasValue)
                            {
                                rawData[i] = rawData[i - ordData.skipBack.Value];
                            }
                        }
                    }
                }
            } while (rawData.Select((num, idx) => num != prev[idx]).Aggregate((b1, b2) => b1 || b2));

            if (rawData.Select(el => el.HasValue).Aggregate((b1, b2) => b1 && b2))
            {
                return rawData.Select(el => el.Value).ToArray();
            }

            throw new ArgumentException("the WirePointInfo[] could not resolve at least one ordinate");
        }

        private static ImmutableList<ImmutableList<Pos>> WirePointSplittingAggregator(ImmutableList<ImmutableList<Pos>> agg, (Pos pos, bool skipBack) next)
        {
            if (next.skipBack)
            {
                return agg.Add(ImmutableList.Create(next.pos));
            }

            return agg.Replace(agg.Last(), agg.Last().Add(next.pos));
        }

        public static Pos[][] ResolveWirePoints(this WirePointInfo[] wirePointInfos)
        {
            return ResolveOrds(wirePointInfos.Select(pointInfo => (pointInfo.PointType, pointInfo.SkipBackBy, pointInfo.XInfo, pointInfo.StoredX)).ToArray())
                .Zip(ResolveOrds(wirePointInfos.Select(pointInfo => (pointInfo.PointType, pointInfo.SkipBackBy, pointInfo.YInfo, pointInfo.StoredY)).ToArray())
                , (x, y) => new Pos(x, y))
                .Zip(wirePointInfos.Select(wirePointInfo => wirePointInfo.PointType == WirePointInfo.WirePointType.SkipBack)
                , (pos, skipBack) => (pos, skipBack))
                .Aggregate(ImmutableList.Create(ImmutableList.Create<Pos>()), WirePointSplittingAggregator)
                .Select(list => list.ToArray())
                .ToArray();
        }
    }

    internal class Program
    {
        static void PlaceWire(Pos[] wire, Board board)
        {
            for (int i = 0; i < wire.Length - 1; i++)
            {
                new Wire(wire[i], wire[i + 1], board);
            }
        }

        static void PlaceWires(Pos[][] wires, Board board)
        {
            foreach (Pos[] wire in wires)
            {
                PlaceWire(wire, board);
            }
        }

        static void PlaceWires(WirePointInfo[][] wires, Board board)
        {
            foreach (WirePointInfo[] wirePointInfoArr in wires)
            {
                foreach (Pos[] wire in wirePointInfoArr.ResolveWirePoints())
                {
                    PlaceWire(wire, board);
                }
            }
        }

        //*
        static Board BuildSRNorLatch()
        {
            Board SRNorLatch = new Board("SR Latch", new System.Drawing.Size(4, 4));

            IBoardInputComponent SInp = new BoardContainerComponents.BoardInputComponent("S", Pin.State.HIGH, new Board.InterfaceLocation(Board.InterfaceLocation.SideEnum.Left, 1));
            IBoardInputComponent RInp = new BoardContainerComponents.BoardInputComponent("R", Pin.State.LOW, new Board.InterfaceLocation(Board.InterfaceLocation.SideEnum.Left, 3));

            IComponent SExtBuffer = new BufferComponents.BufferComponent();
            IComponent RExtBuffer = new BufferComponents.BufferComponent();

            IComponent QNor = new VarInpComponents.VarInpNorComponent(3);
            IComponent QBarNor = new VarInpComponents.VarInpNorComponent(3);

            IComponent QSyncNot = new BufferComponents.NotComponent();
            IComponent QBarSyncNot = new BufferComponents.NotComponent();

            IComponent QSyncBuffer1 = new BufferComponents.BufferComponent();
            IComponent QBarSyncBuffer1 = new BufferComponents.BufferComponent();

            IComponent QSyncBuffer2 = new BufferComponents.BufferComponent();
            IComponent QBarSyncBuffer2 = new BufferComponents.BufferComponent();

            IComponent QSyncBuffer3 = new BufferComponents.BufferComponent();
            IComponent QBarSyncBuffer3 = new BufferComponents.BufferComponent();

            IComponent QSyncBuffer4 = new BufferComponents.BufferComponent();
            IComponent QBarSyncBuffer4 = new BufferComponents.BufferComponent();

            IComponent QSync2And = new VarInpComponents.VarInpAndComponent(2);
            IComponent QBarSync2And = new VarInpComponents.VarInpAndComponent(2);

            IComponent QSync3And = new VarInpComponents.VarInpAndComponent(3);
            IComponent QBarSync3And = new VarInpComponents.VarInpAndComponent(3);

            IComponent QSyncOr = new VarInpComponents.VarInpOrComponent(3);
            IComponent QBarSyncOr = new VarInpComponents.VarInpOrComponent(3);

            IBoardOutputComponent QOutp = new BoardContainerComponents.BoardOutputComponent("Q", new Board.InterfaceLocation(Board.InterfaceLocation.SideEnum.Right, 1));
            IBoardOutputComponent QBarOutp = new BoardContainerComponents.BoardOutputComponent("QBAR", new Board.InterfaceLocation(Board.InterfaceLocation.SideEnum.Right, 3));

            SInp.Place(new Pos(-2, 0), SRNorLatch);
            RInp.Place(new Pos(-2, 7), SRNorLatch);

            SExtBuffer.Place(new Pos(1, -2), SRNorLatch);
            RExtBuffer.Place(new Pos(1, 9), SRNorLatch);

            QNor.Place(new Pos(4, 0), SRNorLatch);
            QBarNor.Place(new Pos(4, 7), SRNorLatch);

            QSyncNot.Place(new Pos(7, 0), SRNorLatch);
            QBarSyncNot.Place(new Pos(8, 7), SRNorLatch);

            QSyncBuffer1.Place(new Pos(9, 3), SRNorLatch);
            QBarSyncBuffer1.Place(new Pos(9, 4), SRNorLatch);

            QSyncBuffer2.Place(new Pos(11, 3), SRNorLatch);
            QBarSyncBuffer2.Place(new Pos(11, 4), SRNorLatch);

            QSyncBuffer3.Place(new Pos(13, -1), Rotation.ANTICLOCKWISE, SRNorLatch);
            QBarSyncBuffer3.Place(new Pos(13, 8), Rotation.CLOCKWISE, SRNorLatch);

            QSyncBuffer4.Place(new Pos(13, -4), SRNorLatch);
            QBarSyncBuffer4.Place(new Pos(13, 11), SRNorLatch);

            QSync2And.Place(new Pos(16, 2), SRNorLatch);
            QBarSync2And.Place(new Pos(16, 5), SRNorLatch);

            QSync3And.Place(new Pos(16, -2), SRNorLatch);
            QBarSync3And.Place(new Pos(16, 9), SRNorLatch);

            QSyncOr.Place(new Pos(20, -2), SRNorLatch);
            QBarSyncOr.Place(new Pos(20, 9), SRNorLatch);

            QOutp.Place(new Pos(24, -2), SRNorLatch);
            QBarOutp.Place(new Pos(24, 9), SRNorLatch);

            PlaceWires(new WirePointInfo[][]
            {
                new WirePointInfo[]
                {
                    SExtBuffer.GetAllPinPositions()[0],
                    SInp.GetAllPinPositions()[0],
                    QNor.GetAllPinPositions()[1],
                },
                new WirePointInfo[]
                {
                    RExtBuffer.GetAllPinPositions()[0],
                    RInp.GetAllPinPositions()[0],
                    QBarNor.GetAllPinPositions()[1]
                },
                new WirePointInfo[]
                {
                    QNor.GetAllPinPositions()[3],
                    (WirePointInfo.OrdInfo.ExactPrev, WirePointInfo.OrdInfo.ExactNext),
                    QBarSyncBuffer1.GetAllPinPositions()[0],
                    (WirePointInfo.WirePointType.SkipBack, 2),
                    (WirePointInfo.OrdInfo.ExactNext, WirePointInfo.OrdInfo.ExactPrev),
                    QBarNor.GetAllPinPositions()[0]
                },
                new WirePointInfo[]
                {
                    QBarNor.GetAllPinPositions()[3],
                    QBarSyncNot.GetAllPinPositions()[0],
                    (WirePointInfo.OrdInfo.ExactPrev, WirePointInfo.OrdInfo.ExactNext),
                    QSyncBuffer1.GetAllPinPositions()[0],
                    (WirePointInfo.WirePointType.SkipBack, 2),
                    (WirePointInfo.OrdInfo.ExactNext, WirePointInfo.OrdInfo.ExactPrev),
                    QNor.GetAllPinPositions()[2]
                },
                new WirePointInfo[]
                {
                    QSyncNot.GetAllPinPositions()[1],
                    QSyncBuffer3.GetAllPinPositions()[0],
                    QSync3And.GetAllPinPositions()[2],
                    QSync2And.GetAllPinPositions()[0]
                },
                new WirePointInfo[]
                {
                    QBarSyncNot.GetAllPinPositions()[1],
                    QBarSyncBuffer3.GetAllPinPositions()[0],
                    QBarSync3And.GetAllPinPositions()[0],
                    QBarSync2And.GetAllPinPositions()[1]
                },
                new WirePointInfo[] { QSyncBuffer3.GetAllPinPositions()[1], QSync3And.GetAllPinPositions()[1] },
                new WirePointInfo[] { QBarSyncBuffer3.GetAllPinPositions()[1], QBarSync3And.GetAllPinPositions()[1] },
                new WirePointInfo[]
                {
                    QSync2And.GetAllPinPositions()[1],
                    QSyncBuffer2.GetAllPinPositions()[1],
                    QSyncBuffer4.GetAllPinPositions()[0],
                    (WirePointInfo.OrdInfo.ExactPrev, WirePointInfo.OrdInfo.OffsetPrev, -1),
                    (WirePointInfo.OrdInfo.ExactNext, WirePointInfo.OrdInfo.ExactPrev),
                    QSyncOr.GetAllPinPositions()[0]
                },
                new WirePointInfo[]
                {
                    QBarSync2And.GetAllPinPositions()[0],
                    QBarSyncBuffer2.GetAllPinPositions()[1],
                    QBarSyncBuffer4.GetAllPinPositions()[0],
                    (WirePointInfo.OrdInfo.ExactPrev, WirePointInfo.OrdInfo.OffsetPrev, 1),
                    (WirePointInfo.OrdInfo.ExactNext, WirePointInfo.OrdInfo.ExactPrev),
                    QBarSyncOr.GetAllPinPositions()[2]
                },
                new WirePointInfo[] { QSync2And.GetAllPinPositions()[2], QSyncOr.GetAllPinPositions()[2] },
                new WirePointInfo[] { QBarSync2And.GetAllPinPositions()[2], QBarSyncOr.GetAllPinPositions()[0] }
                /*
                new WirePointInfo[] {
                    QSyncBuffer.GetAllPinPositions()[0],
                    QNor.GetAllPinPositions()[2],
                    QSyncOr.GetAllPinPositions()[0],
                    (WirePointInfo.OrdInfo.ExactPrev, WirePointInfo.OrdInfo.ExactNext),
                    (WirePointInfo.OrdInfo.ExactNext, WirePointInfo.OrdInfo.OffsetNext, 1),
                    QBarNor.GetAllPinPositions()[1]
                },
                new WirePointInfo[]
                {
                    QBarSyncOr.GetAllPinPositions()[0],
                    (WirePointInfo.OrdInfo.ExactNext, WirePointInfo.OrdInfo.ExactPrev),
                    QBarSyncBuffer.GetAllPinPositions()[0],
                    (WirePointInfo.OrdInfo.ExactPrev, WirePointInfo.OrdInfo.ExactNext),
                    (WirePointInfo.OrdInfo.ExactNext, WirePointInfo.OrdInfo.OffsetNext, -1),
                    QNor.GetAllPinPositions()[0]
                }
                //*/
            }, SRNorLatch);

            SRNorLatch.Save("Boards/SR Latch.brd");

            return SRNorLatch;
        }

        static Board BuildDLatch(Board SRNorLatchBoard)
        {
            Board DLatch = new Board("D Latch", new System.Drawing.Size(4, 4));

            IBoardInputComponent DInp = new BoardContainerComponents.BoardInputComponent("D", Pin.State.LOW, new Board.InterfaceLocation(Board.InterfaceLocation.SideEnum.Left, 1));
            IBoardInputComponent EInp = new BoardContainerComponents.BoardInputComponent("E", Pin.State.LOW, new Board.InterfaceLocation(Board.InterfaceLocation.SideEnum.Left, 3));

            IComponent DBuffer = new BufferComponents.BufferComponent();
            IComponent DNot = new BufferComponents.NotComponent();

            IComponent SAnd = new VarInpComponents.VarInpAndComponent(2);
            IComponent RAnd = new VarInpComponents.VarInpAndComponent(2);

            IBoardInputComponent SInp = new BoardContainerComponents.BoardInputComponent("S", Pin.State.FLOATING, new Board.InterfaceLocation(Board.InterfaceLocation.SideEnum.Top, 2));
            IBoardInputComponent RInp = new BoardContainerComponents.BoardInputComponent("R", Pin.State.FLOATING, new Board.InterfaceLocation(Board.InterfaceLocation.SideEnum.Bottom, 2));

            IComponent SOr = new VarInpComponents.VarInpOrComponent(2);
            IComponent ROr = new VarInpComponents.VarInpOrComponent(2);

            IBoardContainerComponent SRNorLatch = new BoardContainerComponents.BoardContainerComponent(SRNorLatchBoard);

            IBoardOutputComponent QOutp = new BoardContainerComponents.BoardOutputComponent("Q", new Board.InterfaceLocation(Board.InterfaceLocation.SideEnum.Right, 1));
            IBoardOutputComponent QBarOutp = new BoardContainerComponents.BoardOutputComponent("QBAR", new Board.InterfaceLocation(Board.InterfaceLocation.SideEnum.Right, 3));

            DInp.Place(new Pos(0, 0), DLatch); // 0
            EInp.Place(new Pos(4, 3), DLatch); // 1

            DBuffer.Place(new Pos(5, 0), DLatch);
            DNot.Place(new Pos(5, 6), DLatch);

            SAnd.Place(new Pos(8, 1), DLatch);
            RAnd.Place(new Pos(8, 5), DLatch);

            SInp.Place(new Pos(8, -1), DLatch); // 2
            RInp.Place(new Pos(8, 7), DLatch); // 3

            SOr.Place(new Pos(12, 0), DLatch);
            ROr.Place(new Pos(12, 6), DLatch);

            SRNorLatch.Place(new Pos(17, 3), DLatch);

            QOutp.Place(new Pos(22, 2), DLatch); // 4
            QBarOutp.Place(new Pos(22, 4), DLatch); // 5

            /*
            Pos[] intermediaries = new Pos[]
            {
                DInp.GetAllPinPositions()[0],
                DNot.GetAllPinPositions()[0],
            };

            PlaceWires(new Pos[][]
            {
                new Pos[]
                {
                    DBuffer.GetAllPinPositions()[0],
                    intermediaries[0],
                    new Pos(intermediaries[0].X, intermediaries[1].Y),
                    intermediaries[1]
                },
                new Pos[]
                {
                    SAnd.GetAllPinPositions()[1],
                    EInp.GetAllPinPositions()[0],
                    RAnd.GetAllPinPositions()[0]
                },
                new Pos[] { SOr.GetAllPinPositions()[2], SRNorLatch.GetAllPinPositions()[0] },
                new Pos[] { ROr.GetAllPinPositions()[2], SRNorLatch.GetAllPinPositions()[1] }
            }, DLatch);
            //*/

            //*
            PlaceWires(new WirePointInfo[][]
            {
                new WirePointInfo[]
                {
                    DBuffer.GetAllPinPositions()[0],
                    DInp.GetAllPinPositions()[0],
                    (WirePointInfo.OrdInfo.ExactPrev, WirePointInfo.OrdInfo.ExactNext),
                    DNot.GetAllPinPositions()[0]
                },
                new WirePointInfo[]
                {
                    SAnd.GetAllPinPositions()[1],
                    EInp.GetAllPinPositions()[0],
                    RAnd.GetAllPinPositions()[0]
                },
                new WirePointInfo[] { SOr.GetAllPinPositions()[2], SRNorLatch.GetAllPinPositions()[0] },
                new WirePointInfo[] { ROr.GetAllPinPositions()[2], SRNorLatch.GetAllPinPositions()[1] }
            }, DLatch);
            //*/

            DLatch.Save("Boards/D Latch.brd");

            return DLatch;
        }

        static Board BuildDFlipFlop(Board DLatchBoard)
        {
            Board DFlipFlop = new Board("D Flip Flop", new System.Drawing.Size(4, 4));
            const int timingBuffers = 8;

            IBoardInputComponent DInp = new BoardContainerComponents.BoardInputComponent("D", Pin.State.HIGH, new Board.InterfaceLocation(Board.InterfaceLocation.SideEnum.Left, 1));
            IBoardInputComponent ClkInp = new BoardContainerComponents.BoardInputComponent("CLK", Pin.State.LOW, new Board.InterfaceLocation(Board.InterfaceLocation.SideEnum.Left, 3));

            IBoardInputComponent SInp = new BoardContainerComponents.BoardInputComponent("S", Pin.State.FLOATING, new Board.InterfaceLocation(Board.InterfaceLocation.SideEnum.Top, 2));
            IBoardInputComponent RInp = new BoardContainerComponents.BoardInputComponent("R", Pin.State.FLOATING, new Board.InterfaceLocation(Board.InterfaceLocation.SideEnum.Bottom, 2));

            IComponent ClkNot1 = new BufferComponents.NotComponent();

            IComponent[] ClkBuffers = new IComponent[timingBuffers];
            for (int i = 0; i < timingBuffers; i++)
            {
                ClkBuffers[i] = new BufferComponents.BufferComponent();
            }

            IComponent ClkNot2 = new BufferComponents.NotComponent();

            IBoardContainerComponent MasterDLatch = new BoardContainerComponents.BoardContainerComponent(DLatchBoard);
            IBoardContainerComponent SlaveDLatch = new BoardContainerComponents.BoardContainerComponent(DLatchBoard);

            IBoardOutputComponent QOutp = new BoardContainerComponents.BoardOutputComponent("Q", new Board.InterfaceLocation(Board.InterfaceLocation.SideEnum.Right, 1));
            IBoardOutputComponent QBarOutp = new BoardContainerComponents.BoardOutputComponent("QBAR", new Board.InterfaceLocation(Board.InterfaceLocation.SideEnum.Right, 3));

            DInp.Place(new Pos(0, 0), DFlipFlop); // 0
            ClkInp.Place(new Pos(-2, 5), DFlipFlop); // 1

            SInp.Place(new Pos(3, -3), DFlipFlop); // 2
            RInp.Place(new Pos(-1, 4), DFlipFlop); // 3

            ClkNot1.Place(new Pos(1, 5), DFlipFlop);

            for (int i = 0; i < timingBuffers; i++)
            {
                ClkBuffers[i].Place(new Pos(3 + (2 * i), 5), DFlipFlop);
            }

            ClkNot2.Place(new Pos(Math.Max(8, 3 + (2 * timingBuffers)), 5), DFlipFlop);

            MasterDLatch.Place(new Pos(5, 1), DFlipFlop);
            SlaveDLatch.Place(new Pos(Math.Max(12, 7 + (2 * timingBuffers)), 1), DFlipFlop);

            QOutp.Place(new Pos(Math.Max(17, 12 + (2 * timingBuffers)), 0), DFlipFlop); // 4
            QBarOutp.Place(new Pos(Math.Max(17, 12 + (2 * timingBuffers)), 2), DFlipFlop); // 5

            PlaceWires(new Pos[][]
            {
                new Pos[] { MasterDLatch.GetAllPinPositions()[1], ClkNot1.GetAllPinPositions()[1] },
                new Pos[] { ClkNot2.GetAllPinPositions()[1], SlaveDLatch.GetAllPinPositions()[1] },
                new Pos[] { MasterDLatch.GetAllPinPositions()[4], SlaveDLatch.GetAllPinPositions()[0] },
                new Pos[]
                {
                    SInp.GetAllPinPositions()[0],
                    MasterDLatch.GetAllPinPositions()[2],
                    SlaveDLatch.GetAllPinPositions()[2],
                },
                new Pos[]
                {
                    RInp.GetAllPinPositions()[0],
                    MasterDLatch.GetAllPinPositions()[3],
                    SlaveDLatch.GetAllPinPositions()[3],
                }
            }, DFlipFlop);

            DFlipFlop.Save("Boards/D Flip Flop.brd");

            return DFlipFlop;
        }

        static Board BuildSingleCounterElement(Board DFlipFlopBoard)
        {
            Board SingleCounterElement = new Board("Single Counter Element", new System.Drawing.Size(4, 4));

            //CarryInp, DXor, ClkInp, RInp, DFlipFlop, ClkOutp, DOutp, CarryAnd, CarryOutp

            IBoardInputComponent CarryInp = new BoardContainerComponents.BoardInputComponent("Cin", Pin.State.LOW, new Board.InterfaceLocation(Board.InterfaceLocation.SideEnum.Left, 1));

            IComponent DXor = new VarInpComponents.VarInpXorComponent(2);

            IBoardInputComponent ClkInp = new BoardContainerComponents.BoardInputComponent("CLKin", Pin.State.LOW, new Board.InterfaceLocation(Board.InterfaceLocation.SideEnum.Left, 2));
            IBoardInputComponent RInp = new BoardContainerComponents.BoardInputComponent("Rin", Pin.State.HIGH, new Board.InterfaceLocation(Board.InterfaceLocation.SideEnum.Left, 3));

            IBoardContainerComponent DFlipFlop = new BoardContainerComponents.BoardContainerComponent(DFlipFlopBoard);

            IBoardOutputComponent ClkOutp = new BoardContainerComponents.BoardOutputComponent("CLKout", new Board.InterfaceLocation(Board.InterfaceLocation.SideEnum.Right, 2)); 
            IBoardOutputComponent ROutp = new BoardContainerComponents.BoardOutputComponent("Rout", new Board.InterfaceLocation(Board.InterfaceLocation.SideEnum.Right, 3));
            IBoardOutputComponent DOutp = new BoardContainerComponents.BoardOutputComponent("D", new Board.InterfaceLocation(Board.InterfaceLocation.SideEnum.Top, 2));

            IComponent CarryPullUp = new FixedStateComponent(Pin.State.PULLEDHIGH);
            IComponent CarryAnd = new VarInpComponents.VarInpAndComponent(2);

            IBoardOutputComponent CarryOutp = new BoardContainerComponents.BoardOutputComponent("Cout", new Board.InterfaceLocation(Board.InterfaceLocation.SideEnum.Right, 1));

            CarryInp.Place(new Pos(0, 0), SingleCounterElement);

            DXor.Place(new Pos(3, 4), Rotation.CLOCKWISE, SingleCounterElement);

            ClkInp.Place(new Pos(3, 11), SingleCounterElement);
            RInp.Place(new Pos(6, 12), SingleCounterElement);

            DFlipFlop.Place(new Pos(8, 7), SingleCounterElement);

            ClkOutp.Place(new Pos(13, 11), SingleCounterElement);
            ROutp.Place(new Pos(10, 12), SingleCounterElement);
            DOutp.Place(new Pos(13, 6), SingleCounterElement);

            CarryPullUp.Place(new Pos(9, 1), SingleCounterElement);
            CarryAnd.Place(new Pos(13, 1), SingleCounterElement);

            CarryOutp.Place(new Pos(17, 1), SingleCounterElement);

            PlaceWires(new WirePointInfo[][]
            {
                new WirePointInfo[]
                {
                    DXor.GetAllPinPositions()[1],
                    CarryInp.GetAllPinPositions()[0],
                    CarryAnd.GetAllPinPositions()[0],
                    CarryPullUp.GetAllPinPositions()[0]
                },
                new WirePointInfo[]
                {
                    DXor.GetAllPinPositions()[0],
                    CarryAnd.GetAllPinPositions()[1],
                    DOutp.GetAllPinPositions()[0]
                },
                new WirePointInfo[] { DXor.GetAllPinPositions()[2], DFlipFlop.GetAllPinPositions()[0] },
                new WirePointInfo[]
                {
                    DFlipFlop.GetAllPinPositions()[1],
                    ClkInp.GetAllPinPositions()[0],
                    ClkOutp.GetAllPinPositions()[0]
                },
                new WirePointInfo[] { RInp.GetAllPinPositions()[0], DFlipFlop.GetAllPinPositions()[3] }
            }, SingleCounterElement);

            SingleCounterElement.Save("Boards/Single Counter Element.brd");

            return SingleCounterElement;
        }

        static Board BuildBCDDigitCounter(Board SingleCounterElement)
        {
            Board BCDDigitCounter = new Board("BCD Digit Counter", new System.Drawing.Size(2, 6));

            IBoardInputComponent IncInp = new BoardContainerComponents.BoardInputComponent("INC", Pin.State.LOW, new Board.InterfaceLocation(Board.InterfaceLocation.SideEnum.Bottom, 1));
            IBoardInputComponent RInp = new BoardContainerComponents.BoardInputComponent("R", Pin.State.LOW, new Board.InterfaceLocation(Board.InterfaceLocation.SideEnum.Left, 5));

            // wires

            BCDDigitCounter.Save("Boards/BCD Digit Counter.brd");

            return BCDDigitCounter;
        }
        //*/

        //*
        static Board BuildMUX(int inpCount, int[][][] decoding, string name)
        {
            int outpCount = decoding.Length;

            Board MUXBoard = new Board(name, new System.Drawing.Size(3, 1 + Math.Max(inpCount, outpCount)));

            int inpOffset = 1, outpOffset = 1;

            if (inpCount != outpCount)
            {
                int diff = Math.Abs(inpCount - outpCount);
                diff = (int)Math.Floor(diff / 2D);

                if (inpCount > outpCount)
                {
                    outpOffset += diff;
                }
                else
                {
                    inpOffset += diff;
                }
            }

            IBoardInputComponent[] inps = new IBoardInputComponent[inpCount];
            IComponent[] inpBuffers = new IComponent[inpCount];
            IComponent[] inpNots = new IComponent[inpCount];
            Wire[] inpNotWires = new Wire[inpCount];
            List<int>[] wirePoints = new List<int>[2 * inpCount];

            for (int i = 0; i < inpCount; i++)
            {
                inps[i] = new BoardContainerComponents.BoardInputComponent($"I{inpCount - 1 - i}", Pin.State.LOW, new Board.InterfaceLocation(Board.InterfaceLocation.SideEnum.Left, i + inpOffset));
                inpBuffers[i] = new BufferComponents.BufferComponent();
                inpNots[i] = new BufferComponents.NotComponent();

                inps[i].Place(new Pos(0, 4 * i), MUXBoard);
                inpBuffers[i].Place(new Pos(3, 4 * i), MUXBoard);
                inpNots[i].Place(new Pos(3, (4 * i) + 2), MUXBoard);
                inpNotWires[i] = new Wire(inps[i].GetAllPinPositions()[0], inpNots[i].GetAllPinPositions()[0], MUXBoard);

                wirePoints[i] = new List<int>();
                wirePoints[i + inpCount] = new List<int>();
            }

            int andX = 5;
            IBoardOutputComponent[] outps = new IBoardOutputComponent[outpCount];
            IComponent[][] outpAnds = new IComponent[outpCount][];
            IComponent[] outpOrs = new IComponent[outpCount];
            Wire[][] outpAndInpWires = new Wire[outpCount][];
            Wire[][][] outpOrInpWires = new Wire[outpCount][][];

            int decodingiLength, decodingijLength, wireY, decodingijk;
            Pos[] pinPositions;
            Pos andOutp, inter1, inter2, orInp;

            for (int i = 0; i < outps.Length; i++)
            {
                decodingiLength = decoding[i].Length;

                outps[i] = new BoardContainerComponents.BoardOutputComponent($"{(char)((int)'A' + i)}", new Board.InterfaceLocation(Board.InterfaceLocation.SideEnum.Right, i + outpOffset));
                outpAnds[i] = new IComponent[decodingiLength];
                outpAndInpWires[i] = new Wire[decodingiLength];
                outpOrInpWires[i] = new Wire[decodingiLength][];

                for (int j = 0; j < decodingiLength; j++)
                {
                    decodingijLength = decoding[i][j].Length;

                    outpAnds[i][j] = new VarInpComponents.VarInpAndComponent(decodingijLength);
                    andX += decodingijLength;
                    outpAnds[i][j].Place(new Pos(andX, (4 * inpCount) + 2), Rotation.CLOCKWISE, MUXBoard);
                    andX += decodingijLength;

                    for (int k = 0; k < decodingijLength; k++)
                    {
                        pinPositions = outpAnds[i][j].GetAllPinPositions();
                        decodingijk = decoding[i][j][k];

                        wireY = 4 * (decodingijk - 1);

                        if (wireY < 0)
                        {
                            wireY += 6;
                            wireY *= -1;
                        }

                        outpAndInpWires[i][j] = new Wire(pinPositions[k], new Pos(pinPositions[k].X, wireY), MUXBoard);

                        if (decodingijk < 0)
                        {
                            decodingijk *= -1;
                            decodingijk += inpCount;
                        }

                        decodingijk--;

                        wirePoints[decodingijk].Add(pinPositions[k].X);
                    }
                }

                andX++;

                outpOrs[i] = new VarInpComponents.VarInpOrComponent(decodingiLength);
                outpOrs[i].Place(new Pos(outpAnds[i][0].GetAllPinPositions().Last().X - 1 + decodingiLength, ((inpCount + 1) * 4) + decodingiLength), Rotation.CLOCKWISE, MUXBoard);

                for (int j = 0; j < decodingiLength; j++)
                {
                    andOutp = outpAnds[i][j].GetAllPinPositions().Last();
                    pinPositions = outpOrs[i].GetAllPinPositions();
                    orInp = pinPositions[pinPositions.Length - 2 - j];

                    wireY = andOutp.Y + Math.Max(j - 1, 0);

                    inter1 = new Pos(andOutp.X, wireY);
                    inter2 = new Pos(orInp.X, wireY);

                    outpOrInpWires[i][j] = new Wire[]
                    {
                            new Wire(andOutp, inter1, MUXBoard),
                            new Wire(inter1, inter2, MUXBoard),
                            new Wire(inter2, orInp, MUXBoard)
                    };
                }

                outps[i].Place(outpOrs[i].GetComponentPos().Add(0, 4), Rotation.CLOCKWISE, MUXBoard);
            }

            Wire[][] rails = new Wire[2 * inpCount][];

            for (int i = 0; i < wirePoints.Length; i++)
            {
                wireY = 4 * i;

                if (wireY >= inpCount * 4)
                {
                    wireY -= (inpCount * 4) - 2;
                }

                rails[i] = new Wire[wirePoints[i].Count];

                wirePoints[i].Add(4);
                wirePoints[i].Sort();

                for (int j = 0; j < wirePoints[i].Count - 1; j++)
                {
                    rails[i][j] = new Wire(new Pos(wirePoints[i][j], wireY), new Pos(wirePoints[i][j + 1], wireY), MUXBoard);
                }
            }

            MUXBoard.Save($"Boards/{name}.brd");

            return MUXBoard;
        }
        //*/

        [STAThread]
        static void Main(string[] args)
        {
            ComponentRegisterer.RegisterComponents();

            //*
            Board 
                SRNorLatch = BuildSRNorLatch(),
                DLatch = BuildDLatch(SRNorLatch), 
                DFlipFlop = BuildDFlipFlop(DLatch), 
                SingleCounterElement = BuildSingleCounterElement(DFlipFlop),
                //BCDDigitCounter = BuildBCDDigitCounter(SingleCounterElement),

                SevenSegDecoder = BuildMUX(4, new int[][][]
                {
                    new int[][] { new int[]{ 1, -2, -3 }, new int[] { -1, 2, 4 }, new int[] { -2, -4 }, new int[] { -1, 3 }, new int[] { 1, -4 }, new int[] { 2, 3 } },
                    new int[][] { new int[]{ -1, -3, -4 }, new int[] { -1, 3, 4 }, new int[] { 1, -3, 4 }, new int[] { -2, -4 }, new int[] { -1, -2 } },
                    new int[][] { new int[]{ -3, 4 }, new int[] { -1, 2 }, new int[] { 1, -2 }, new int[] { -1, 4 }, new int[] { -1, -3 } },
                    new int[][] { new int[]{ 2, -3, 4 }, new int[] { 1, -3 }, new int[] { 2, 3, -4 }, new int[] { -2, 3, 4 }, new int[] { -1, -2, -4 } },
                    new int[][] { new int[]{ -2, -4 }, new int[] { 3, -4 }, new int[] { 1, 3 }, new int[] { 1, 2 } },
                    new int[][] { new int[]{ -1, 2, -3 }, new int[] { -3, -4 }, new int[] { 2, -4 }, new int[] { 1, -2 }, new int[] { 1, 3 } },
                    new int[][] { new int[]{ -2, 3 }, new int[] { 1, -2 }, new int[] { 1, 4 }, new int[] { 3, -4 }, new int[] { -1, 2, -3, } }
                }, "7seg decoder");
            //*/

            //*
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new GUIForm());
            //*/
        }
    }
}
