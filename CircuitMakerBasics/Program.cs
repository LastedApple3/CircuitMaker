﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CircuitMaker.Basics;
using CircuitMaker.Components;
using System.Windows.Forms;
using CircuitMaker.GUI;
using System.IO;

namespace CircuitMaker
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            ComponentRegisterer.RegisterComponents();

            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new GUIForm());

            //Board board = Board.Load("SR-Latch.brd");

            Action<string> textInteract = (str) => _ = str;

            /*
            Board board = new Board("SR-Latch");

            IBoardInputComponent Set = new BoardContainerComponents.BoardInputComponent("S");
            IBoardInputComponent Reset = new BoardContainerComponents.BoardInputComponent("R");

            IComponent QNor = new VarInpComponents.VarInpNorComponent(2);
            IComponent QBarNor = new VarInpComponents.VarInpNorComponent(2);

            IBoardOutputComponent Q = new BoardContainerComponents.BoardOutputComponent("Q");
            IBoardOutputComponent QBar = new BoardContainerComponents.BoardOutputComponent("QBAR");

            Reset.Place(new Pos(4, 0), board);
            Set.Place(new Pos(0, 0), board);

            QNor.Place(new Pos(4, 4), board);
            QBarNor.Place(new Pos(0, 4), board);

            Q.Place(new Pos(4, 8), board);
            QBar.Place(new Pos(0, 8), board);

            Wire[] wires = new Wire[]
            {
                new Wire(Reset.GetAllPinPositions()[0], QNor.GetAllPinPositions()[0], board), // reset to top nor
                new Wire(Set.GetAllPinPositions()[0], QBarNor.GetAllPinPositions()[1], board), // set to bottom nor
                new Wire(QNor.GetAllPinPositions()[2], QBarNor.GetAllPinPositions()[0], board), // top nor out to bottom nor in
                new Wire(QBarNor.GetAllPinPositions()[2], QNor.GetAllPinPositions()[1], board), // bottom bor out to top nor in
                new Wire(QNor.GetAllPinPositions()[2], Q.GetAllPinPositions()[0], board), // top nor to q
                new Wire(QBarNor.GetAllPinPositions()[2], QBar.GetAllPinPositions()[0], board), // bottom nor to qbar
            };

            board.Save();

            Set.SetInputState(Pin.State.LOW);
            Reset.SetInputState(Pin.State.LOW);
            //*/

            /*
            Board board = Board.Load("SR-Latch");
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

            //*
            Board board = new Board("test");

            IInteractibleComponent Set = new UserToggleInpComponent(Pin.State.LOW);
            IInteractibleComponent Reset = new UserToggleInpComponent(Pin.State.LOW);

            IComponent srLatch = new BoardContainerComponents.BoardContainerComponent(Board.Load("SR-Latch")); // make this another constructor? it is the Constructor function.

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

            //*
            string inp;
            while (true)
            {
                board.Tick();

                foreach (IComponent comp in board.GetComponents()) {
                    Console.WriteLine(comp);

                    foreach (Pos pos in comp.GetAllPinPositions())
                    {
                        Console.WriteLine(board[pos].GetStateForDisplay());
                    }

                    Console.WriteLine();
                }

                textInteract(Console.ReadLine());

                Console.Clear();
            }//*/
        }
    }
}

//TODO:

// 1) Create Component that contains Board.
// 2) Construct GUI.
