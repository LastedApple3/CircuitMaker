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
using System.IO;

namespace CircuitMaker
{

    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            ComponentRegisterer.RegisterComponents();

            //Board board = Board.Load("SR-Latch.brd");

            //Action<string> textInteract = (str) => _ = str;

            /*
            Board board = new Board("VarInpTests");

            int inpCount = 5;

            IComponent and = new VarInpComponents.VarInpAndComponent(inpCount);
            IComponent or = new VarInpComponents.VarInpOrComponent(inpCount);
            IComponent xor = new VarInpComponents.VarInpXorComponent(inpCount);
            IComponent nand = new VarInpComponents.VarInpNandComponent(inpCount);
            IComponent nor = new VarInpComponents.VarInpNorComponent(inpCount);
            IComponent xnor = new VarInpComponents.VarInpXnorComponent(inpCount);

            and.Place(new Pos(0, 0), board);
            or.Place(new Pos(10, 0), board);
            xor.Place(new Pos(20, 0), board);
            nand.Place(new Pos(30, 0), board);
            nor.Place(new Pos(40, 0), board);
            xnor.Place(new Pos(50, 0), board);

            //ReadWriteImplementation.Constructors["AND"]("5");

            board.Save();
            //*/

            //* <-------------------------------------------------------------------------------------------------------------------------------------
            Board board = new Board("SR-Nor-Latch");

            IBoardInterfaceComponent Bidir = new BoardContainerComponents.BoardBidirComponent("B", Pin.State.FLOATING);

            IBoardInputComponent Set = new BoardContainerComponents.BoardInputComponent("S", Pin.State.HIGH);
            IBoardInputComponent Reset = new BoardContainerComponents.BoardInputComponent("R", Pin.State.LOW);

            IComponent QNor = new VarInpComponents.VarInpNorComponent(2);
            IComponent QBarNor = new VarInpComponents.VarInpNorComponent(2);

            IBoardOutputComponent Q = new BoardContainerComponents.BoardOutputComponent("Q");
            IBoardOutputComponent QBar = new BoardContainerComponents.BoardOutputComponent("QBAR");

            Bidir.Place(new Pos(-10, -10), board);

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

            board.Save("Boards/test.brd");
            //*/

            /*
            Board board = Board.Load("SR-Nor-Latch");
            //*/

            /*
            Pin.State setState = Pin.State.LOW;
            Pin.State resetState = Pin.State.LOW;

            textInteract = str => { 
                if (str == "S") { setState = setState.Not(); } else if (str == "R") { resetState = resetState.Not(); };

                board.GetInputComponent("S").SetInputState(setState);
                board.GetInputComponent("R").SetInputState(resetState);
            };
            //*/

            /*
            Board board = new Board("test");

            IInteractibleComponent Set = new UserToggleInpComponent(Pin.State.LOW);
            IInteractibleComponent Reset = (IInteractibleComponent)Set.Copy();
            //IInteractibleComponent Reset = new UserToggleInpComponent(Pin.State.LOW);

            IComponent srLatch = new BoardContainerComponents.BoardContainerComponent(Board.Load("SR-Nor-Latch")); // make this another constructor? it is the Constructor function.

            Set.Place(new Pos(0, 5), board);
            Reset.Place(new Pos(0, 0), board);

            srLatch.Place(new Pos(5, 2), board);

            Wire[] wires = new Wire[]
            {
                new Wire(Set.GetAllPinPositions()[0], srLatch.GetAllPinPositions()[0], board),
                new Wire(Reset.GetAllPinPositions()[0], srLatch.GetAllPinPositions()[1], board)
            };

            textInteract = str => { if (str == "S") { Set.Interact(); } else if (str == "R") { Reset.Interact(); } };
            //*/

            /*
            string inp;
            while (true)
            {
                board.Tick();

                RenderComponents(board);

                textInteract(Console.ReadLine());

                Console.Clear();
            }//*/

            /*
            EnumSettingDescription<Pin.State> enumSettingDescription = new EnumSettingDescription<Pin.State>("state:");
            IntSettingDescription intSettingDescription = new IntSettingDescription("int:");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new SettingsDialog("test", new ISettingDescription[] { }));

            SettingsDialog dialog = new SettingsDialog("test", new ISettingDescription[] { enumSettingDescription, intSettingDescription });
            dialog.ShowDialog();
            //*/

            /*
            ComponentSelectionForm form = new ComponentSelectionForm();

            if (form.ShowDialog() == DialogResult.OK)
            {
                Console.WriteLine(form.GetConstructor());
                Console.ReadLine();
            }
            //*/

            //*
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new GUIForm());
            //Application.Run(new ComponentSelectionForm());
            //Application.Run(new MenuTestForm());
            //*/
        }
    }
}

//TODO:

// 1) Construct GUI.
