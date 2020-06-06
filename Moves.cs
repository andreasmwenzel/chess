using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess
{
    [Flags]
    public enum MoveFlags : byte
    {
        Move = 1, Capture = 1 << 1, EitherType = Move | Capture,
        StartPosition = 1 << 2,
        Castle = 1 << 3,
        UseEnPassant = 1 << 4,
        SetsEnPassant = 1 << 5,
        Conversion = 1 << 6, 

    } 

    public class Move
    {
        public MoveFlags Flags { get; private set; }
        public MoveDirections Direction { get; private set; }
        public Square StartPosition { get; private set; }
        public Square MovePosition { get; private set; }
        public Square CapturePosition { get; private set; }
        public PieceType ConversionType { get; private set; }

        public Move(MoveFlags flags, MoveDirections direction, Square startPosition, Square movePosition, Square capturePosition, PieceType conversionType = PieceType.PieceType_NB)
        {
            Flags = flags;
            Direction = direction;
            StartPosition = startPosition;
            MovePosition = movePosition;
            CapturePosition = capturePosition;
            ConversionType = conversionType;
        }

        public bool isCastle
        {
            get
            {
                return Flags.HasFlag(MoveFlags.Castle);
            }
        }

        public bool setsEnPassant
        {
            get
            {
                return Flags.HasFlag(MoveFlags.SetsEnPassant);
            }
        }
    }

    public class MoveList : List<Move>
    {
        public MoveList()
        {
        }
    }

    public class AllMoves
    {
        public MoveList[] Moves { get; private set; }

        public AllMoves(MoveList[] moves)
        {
            Moves = moves;
        }

        public MoveList this[Square pos]
        {
            get
            {
              return Moves[(byte)pos];
            }
        }
    }

    public class PossibleMoves:IEnumerable<Move>
    {
        public uint MoveCount { get; private set; }
        public UInt64 EndPositionBitMask { get; private set; }
        private MoveList Moves;
        private UInt32 MoveBitMask;
        public PossibleMoves(Gamestate board, Square StartPosition)
        {
            MoveCount = 0;
            MoveBitMask = 0;
            Piece StartPiece = board.GetPiece(StartPosition);
            if (StartPiece == null)
            {
                return;
            }
            Moves = StartPiece.getPositionMoves(StartPosition);
            MoveDirections skipDirection = MoveDirections.Move_0;
            UInt32 mask = 1;
            foreach (Move move in Moves)
            {
                mask <<= 1;
                if (move.Direction == skipDirection)
                {
                    continue;
                }
                else
                {
                    skipDirection = MoveDirections.Move_0;
                }

                if (continueIfCastle(board, StartPiece, move))
                    continue;

                Piece EndPiece = board.GetPiece(move.MovePosition);
                Piece CapturePiece = board.GetPiece(move.CapturePosition);

                if (continueIfEnPassant(board, move, CapturePiece))
                    continue;

                if (endPiecesMatchMoveFlags(StartPiece, move, EndPiece, CapturePiece))
                {
                    // check if move is legal
                    Gamestate testBoard = new Gamestate(board);
                    if (testBoard.GetPiece(move.CapturePosition) != null)
                    {
                        testBoard.RemovePiece(move.CapturePosition);
                    }
                    Piece piece = testBoard.GetPiece(StartPosition);
                    testBoard.RemovePiece(StartPosition);
                    testBoard.PutPiece(piece, move.MovePosition);
                    testBoard.turnColor = (testBoard.turnColor == PieceColor.White) ? PieceColor.Black : PieceColor.White;
                    ControlledPositions controlledMoves = new ControlledPositions(testBoard);
                    if (controlledMoves.Check == null)
                    {
                        if (castleSquaresInCheck(move, piece, controlledMoves))
                            continue;

                        EndPositionBitMask |= ((UInt64)1 << (byte)move.MovePosition);
                        MoveBitMask |= mask;
                        MoveCount++;
                    }
                }
                
                if (EndPiece != null || CapturePiece != null)
                {
                    skipDirection = move.Direction;
                }
            }
            MoveBitMask >>= 1;
        }

        private static bool endPiecesMatchMoveFlags(Piece StartPiece, Move move, Piece EndPiece, Piece CapturePiece)
        {
            return EndPiece == null && move.Flags.HasFlag(MoveFlags.Move) ||
                   CapturePiece != null && move.Flags.HasFlag(MoveFlags.Capture) && CapturePiece.Color != StartPiece.Color;
        }

        private static bool castleSquaresInCheck(Move move, Piece piece, ControlledPositions controlledMoves)
        {
            if (move.isCastle)
            {
                UInt64 testMask = castleSquaresBitMask(move, piece);
                if ((controlledMoves.ControlledPositionsBitMask & testMask) != 0)
                {
                    return true;
                }
            }
            return false;
        }

        private static ulong castleSquaresBitMask(Move move, Piece piece)
        {
            UInt64 testMask = 0;
            if (move.Direction == MoveDirections.Move_E)
            {
                if (piece.Color == PieceColor.White)
                {
                    testMask = ((UInt64)1 << (byte)Square.F1 | (UInt64)1 << (byte)Square.G1);
                }
                else
                {
                    testMask = ((UInt64)1 << (byte)Square.F8 | (UInt64)1 << (byte)Square.G8);
                }
            }
            else
            {
                if (piece.Color == PieceColor.White)
                {
                    testMask = ((UInt64)1 << (byte)Square.D1 | (UInt64)1 << (byte)Square.C1);
                }
                else
                {
                    testMask = ((UInt64)1 << (byte)Square.D8 | (UInt64)1 << (byte)Square.C8);
                }
            }
            return testMask;
        }

        private static bool continueIfEnPassant(Gamestate board, Move move, Piece CapturePiece)
        {
            if (move.Flags.HasFlag(MoveFlags.UseEnPassant))
            {
                if (CapturePiece == null || move.CapturePosition != board.getEnPassantPosition(CapturePiece.Color))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool continueIfCastle(Gamestate board, Piece StartPiece, Move move)
        {
            if (move.isCastle)
            {
                if (move.Direction == MoveDirections.Move_E)
                {
                    if ((StartPiece.Color == PieceColor.White) ? !board.whiteCanCastle_EE : !board.blackCanCastle_EE)
                    {
                        return true;
                    }
                }
                else
                {
                    if ((StartPiece.Color == PieceColor.White) ? !board.whiteCanCastle_WW : !board.blackCanCastle_WW)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        static public MoveList getMovesToPosition(IEnumerable<Move> moves, Square pos)
        {
            MoveList moveList = new MoveList();
            foreach(Move move in moves)
            {
                if (move.MovePosition == pos)
                {
                    moveList.Add(move);
                }
            }
            return moveList;
        }

        public IEnumerator<Move> GetEnumerator()
        {
            UInt32 mask = 1;
            foreach (Move move in Moves)
            {
                if((MoveBitMask & mask) != 0)
                {
                    yield return move;
                }
                mask <<= 1;
            }
        }

        // Must also implement IEnumerable.GetEnumerator, but implement as a private method. 
        private IEnumerator GetEnumerator1()
        {
            return this.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator1();
        }
    }

    public class ControlledPositions
    {
        public UInt64 ControlledPositionsBitMask { get; private set; }
        public Square? Check { get; private set; }
        public ControlledPositions(Gamestate board)
            : this(board, board.turnColor) 
        {
        }
        public ControlledPositions(Gamestate board, PieceColor color)
        {
            foreach (Square StartPosition in board.getPiecePositions(color))
            {
                Piece StartPiece = board.GetPiece(StartPosition);
                MoveList moves = StartPiece.getPositionMoves(StartPosition);
                MoveDirections skipDirection = MoveDirections.Move_0;
                foreach (Move move in moves)
                {
                    if (move.Direction == skipDirection)
                        continue;
                    else
                        skipDirection = MoveDirections.Move_0;

                    if (move.Flags.HasFlag(MoveFlags.Capture))
                    {
                        Piece EndPiece = board.GetPiece(move.CapturePosition);
                        if (!move.Flags.HasFlag(MoveFlags.UseEnPassant) || EndPiece != null && move.CapturePosition == board.getEnPassantPosition(EndPiece.Color))
                        {
                            ControlledPositionsBitMask |= ((UInt64)1 << (byte)move.CapturePosition);
                        }
                        if (EndPiece != null)
                        {
                            if (EndPiece is King && EndPiece.Color != StartPiece.Color)
                            {
                                Check = move.CapturePosition;
                            }
                            skipDirection = move.Direction;
                        }
                    }
                }
            }
        }
    }

}
