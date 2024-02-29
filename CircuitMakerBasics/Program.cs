using System;
using System.Collections.Generic;
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

namespace CircuitMaker
{
    internal class Program
    {
        //*
        static void SaveSRNorLatch()
        {
            Board SRNorLatch = new Board("SR Latch", new System.Drawing.Size(4, 4));

            IBoardInputComponent Set = new BoardContainerComponents.BoardInputComponent("S", Pin.State.HIGH, new Board.InterfaceLocation(Board.InterfaceLocation.SideEnum.Left, 1));
            IBoardInputComponent Reset = new BoardContainerComponents.BoardInputComponent("R", Pin.State.LOW, new Board.InterfaceLocation(Board.InterfaceLocation.SideEnum.Left, 3));

            IComponent QNor = new VarInpComponents.VarInpNorComponent(2);
            IComponent QBarNor = new VarInpComponents.VarInpNorComponent(2);

            IBoardOutputComponent Q = new BoardContainerComponents.BoardOutputComponent("Q", new Board.InterfaceLocation(Board.InterfaceLocation.SideEnum.Right, 1));
            IBoardOutputComponent QBar = new BoardContainerComponents.BoardOutputComponent("QBAR", new Board.InterfaceLocation(Board.InterfaceLocation.SideEnum.Right, 3));

            Set.Place(new Pos(4, 2), SRNorLatch);
            Reset.Place(new Pos(4, 9), SRNorLatch);

            QNor.Place(new Pos(10, 8), SRNorLatch);
            QBarNor.Place(new Pos(10, 3), SRNorLatch);

            Q.Place(new Pos(16, 8), SRNorLatch);
            QBar.Place(new Pos(16, 3), SRNorLatch);

            _ = new Wire[]
            {
                new Wire(Reset.GetAllPinPositions()[0], QNor.GetAllPinPositions()[1], SRNorLatch),
                new Wire(Set.GetAllPinPositions()[0], QBarNor.GetAllPinPositions()[0], SRNorLatch),

                new Wire(QNor.GetAllPinPositions()[2], QNor.GetAllPinPositions()[2].Add(1, 0), SRNorLatch),
                new Wire(QNor.GetAllPinPositions()[2].Add(1, 0), QNor.GetAllPinPositions()[2].Add(1, -3), SRNorLatch),
                new Wire(QNor.GetAllPinPositions()[2].Add(1, -3), QBarNor.GetAllPinPositions()[1].Add(0, 1), SRNorLatch),
                new Wire(QBarNor.GetAllPinPositions()[1].Add(0, 1), QBarNor.GetAllPinPositions()[1], SRNorLatch),

                new Wire(QBarNor.GetAllPinPositions()[2], QBarNor.GetAllPinPositions()[2].Add(0, 3), SRNorLatch),
                new Wire(QNor.GetAllPinPositions()[0], QNor.GetAllPinPositions()[0].Add(0, -1), SRNorLatch),
                new Wire(QNor.GetAllPinPositions()[0].Add(0, -1), QBarNor.GetAllPinPositions()[2].Add(0, 3), SRNorLatch),

                new Wire(QNor.GetAllPinPositions()[2].Add(1, 0), Q.GetAllPinPositions()[0], SRNorLatch),
                new Wire(QBarNor.GetAllPinPositions()[2], QBar.GetAllPinPositions()[0], SRNorLatch),
            };

            SRNorLatch.Save("Boards/SR Latch.brd");
        }
        //*/

        //*
        static void SaveMUX(int inpCount, int[][][] decoding, string name)
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
            IComponent[] inpNots = new IComponent[inpCount];
            Wire[] inpNotWires = new Wire[inpCount];
            List<int>[] wirePoints = new List<int>[2 * inpCount];

            for (int i = 0; i < inpCount; i++)
            {
                inps[i] = new BoardContainerComponents.BoardInputComponent($"I{inpCount - 1 - i}", Pin.State.LOW, new Board.InterfaceLocation(Board.InterfaceLocation.SideEnum.Left, i + inpOffset));
                inpNots[i] = new NotComponent();

                inps[i].Place(new Pos(0, 4 * i), MUXBoard);
                inpNots[i].Place(new Pos(4, (4 * i) + 2), MUXBoard);
                inpNotWires[i] = new Wire(inps[i].GetAllPinPositions()[0], inpNots[i].GetAllPinPositions()[0], MUXBoard);

                wirePoints[i] = new List<int>();
                wirePoints[i + inpCount] = new List<int>();
            }

            int andX = 7;
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

                wirePoints[i].Add(i < inpCount ? 2 : 6);
                wirePoints[i].Sort();

                for (int j = 0; j < wirePoints[i].Count - 1; j++)
                {
                    rails[i][j] = new Wire(new Pos(wirePoints[i][j], wireY), new Pos(wirePoints[i][j + 1], wireY), MUXBoard);
                }
            }

            MUXBoard.Save($"Boards/{name}.brd");
        }
        //*/

        [STAThread]
        static void Main(string[] args)
        {
            ComponentRegisterer.RegisterComponents();

            SaveSRNorLatch();

            //*
            SaveMUX(4, new int[][][]
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
