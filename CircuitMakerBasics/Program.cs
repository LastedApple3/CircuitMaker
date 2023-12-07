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
            Board board = new Board("SR-Nor-Latch", new System.Drawing.Size(4, 4));

            IBoardInputComponent Set = new BoardContainerComponents.BoardInputComponent("S", Pin.State.HIGH, new Board.InterfaceLocation(Board.InterfaceLocation.SideEnum.Left, 1));
            IBoardInputComponent Reset = new BoardContainerComponents.BoardInputComponent("R", Pin.State.LOW, new Board.InterfaceLocation(Board.InterfaceLocation.SideEnum.Left, 3));

            IComponent QNor = new VarInpComponents.VarInpNorComponent(2);
            IComponent QBarNor = new VarInpComponents.VarInpNorComponent(2);

            IBoardOutputComponent Q = new BoardContainerComponents.BoardOutputComponent("Q", new Board.InterfaceLocation(Board.InterfaceLocation.SideEnum.Right, 1));
            IBoardOutputComponent QBar = new BoardContainerComponents.BoardOutputComponent("QBAR", new Board.InterfaceLocation(Board.InterfaceLocation.SideEnum.Right, 3));

            Set.Place(new Pos(4, 2), board);
            Reset.Place(new Pos(4, 9), board);

            QNor.Place(new Pos(10, 8), board);
            QBarNor.Place(new Pos(10, 3), board);

            Q.Place(new Pos(16, 8), board);
            QBar.Place(new Pos(16, 3), board);

            Wire[] wires = new Wire[]
            {
                new Wire(Reset.GetAllPinPositions()[0], QNor.GetAllPinPositions()[1], board),
                new Wire(Set.GetAllPinPositions()[0], QBarNor.GetAllPinPositions()[0], board),

                new Wire(QNor.GetAllPinPositions()[2], QNor.GetAllPinPositions()[2].Add(1, 0), board),
                new Wire(QNor.GetAllPinPositions()[2].Add(1, 0), QNor.GetAllPinPositions()[2].Add(1, -3), board),
                new Wire(QNor.GetAllPinPositions()[2].Add(1, -3), QBarNor.GetAllPinPositions()[1].Add(0, 1), board),
                new Wire(QBarNor.GetAllPinPositions()[1].Add(0, 1), QBarNor.GetAllPinPositions()[1], board),

                new Wire(QBarNor.GetAllPinPositions()[2], QBarNor.GetAllPinPositions()[2].Add(0, 3), board),
                new Wire(QNor.GetAllPinPositions()[0], QNor.GetAllPinPositions()[0].Add(0, -1), board),
                new Wire(QNor.GetAllPinPositions()[0].Add(0, -1), QBarNor.GetAllPinPositions()[2].Add(0, 3), board),

                new Wire(QNor.GetAllPinPositions()[2].Add(1, 0), Q.GetAllPinPositions()[0], board),
                new Wire(QBarNor.GetAllPinPositions()[2], QBar.GetAllPinPositions()[0], board),
            };

            board.Save("Boards/SR-Nor-Latch.brd");
            //*/

            //*
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new GUIForm());
            //Application.Run(new ExtAppEditorForm());
            //*/
        }
    }
}
