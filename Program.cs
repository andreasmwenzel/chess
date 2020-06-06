using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess
{
    class Program
    {
        //PossibleMoves Moves = new PossibleMoves();
        static void Main(string[] args)
        {
            //GameControl control = new GameControl();
            //Gamestate game = control.gamestate;

            //game.PutPiece(new Rook(PieceColor.White), Square.A1);
            //game.PutPiece(new King(PieceColor.White), Square.E1);
            //game.PutPiece(new Rook(PieceColor.White), Square.H1);

            //game.PutPiece(new Rook(PieceColor.Black), Square.A8);
            //game.PutPiece(new King(PieceColor.Black), Square.E8);
            //game.PutPiece(new Rook(PieceColor.Black), Square.H8);
            //DebugUtil.graphGamestate(game);
            //control.PlayGame();

            //PossibleMoves possibleMoves = new PossibleMoves(game, Square.C2);
            //DebugUtil.graphPossibleMoves(possibleMoves.EndPositionBitMask);

            GameControl trialgame = new GameControl();
            trialgame.computerPlayer = PieceColor.White;
            trialgame.StartNewGame();
            DebugUtil.graphGamestate(trialgame.gamestate);
            trialgame.PlayGame();

        }
    }
}
