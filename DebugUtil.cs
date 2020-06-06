using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess
{
    class DebugUtil
    {
        static public void graphPossibleMoves(UInt64 moves)
        {
            for (int row = 7; row >= 0; row--)
            {
                Console.Write((row + 1));
                for (int col = 0; col < 8; col++)
                {
                    if ((moves & ((UInt64)1 << (8 * row + col))) != 0)
                    {
                        Console.Write(" X");
                    }
                    else
                    {
                        Console.Write(" .");
                    }
                }
                Console.WriteLine();
            }
            Console.WriteLine("  A B C D E F G H");
            Console.WriteLine();
        }

        static private Char getPieceSymbolByType(PieceType type)
        {
            switch (type)
            {
                case PieceType.King: return 'K';
                case PieceType.Queen: return 'Q';
                case PieceType.Rook: return 'R';
                case PieceType.Bishop: return 'B';
                case PieceType.Knight: return 'N';
                case PieceType.Pawn: return 'P';
                default : return 'X';
            }
        }

        static public void graphGamestate(Gamestate gamestate, Square? startPosition = null, UInt64? moves = null, Square? checkPos = null)
        {
            for (int row = 7; row >= 0; row--)
            {
                Console.Write((row + 1));
                for (int col = 0; col < 8; col++)
                {
                    Square pos = (Square)(8 * row + col);
                    if (moves != null)
                    {
                        if ((moves & ((UInt64)1 << (byte)pos)) != 0)
                        {
                            Console.BackgroundColor = (checkPos != null && checkPos == pos) ? ConsoleColor.Red : ConsoleColor.DarkGreen;
                        }
                    }
                    Piece piece = gamestate.GetPiece(pos);
                    
                    if (piece != null)
                    {
                        if(pos == startPosition)
                        {
                            Console.BackgroundColor = ConsoleColor.Blue;
                        }
                        Console.ForegroundColor = (piece.Color == PieceColor.White) ? ConsoleColor.White : (Console.BackgroundColor == ConsoleColor.DarkGreen ? ConsoleColor.Black : ConsoleColor.DarkGray);
                        Console.Write(" " + getPieceSymbolByType(piece.Type));

                    }
                    else
                    {
                        Console.Write(" .");
                    }
                    Console.ResetColor();
                }
                Console.WriteLine();
            }
            Console.WriteLine("  A B C D E F G H");
            Console.WriteLine("EPW:" + gamestate.getEnPassantPosition(PieceColor.White) + " EPB:" + gamestate.getEnPassantPosition(PieceColor.Black));
            Console.WriteLine("WCW:" + gamestate.whiteCanCastle_WW + " WCE:" + gamestate.whiteCanCastle_EE + " BCW:" + gamestate.blackCanCastle_WW + " BCE:" + gamestate.blackCanCastle_EE);
            Console.WriteLine();
        }
    }
}
