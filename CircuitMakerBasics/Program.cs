using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CircuitMaker.Basics;
using CircuitMaker.Components;
using System.Windows.Forms;
using CircuitMaker.GUI;
using CircuitMaker.GUI.Settings;
using CircuitMaker.GUI.ExtApp;
using System.IO;

namespace CircuitMaker
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            ComponentRegisterer.RegisterComponents();

            //*
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
            //*/

            //*
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new GUIForm());
            //*/
        }
    }
}
