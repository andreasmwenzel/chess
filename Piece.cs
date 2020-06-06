using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess
{
    abstract public class Piece
    {
        abstract public PieceType Type { get; }
        abstract public PieceValue Value { get; }
        public PieceColor Color {get; private set;}
        public Piece(PieceColor c)
        {
            Color = c;
        }

        public MoveList getPositionMoves(Square position)
        {
            if (getAllMoves() == null)
            {
                initMoves();
            }
            return getAllMoves()[position];
        }

        abstract protected AllMoves getAllMoves();

        abstract protected void initMoves();
        protected AllMoves initMoves(MoveDirections[] moveDirections, bool moveMultiple)
        {
            MoveList[] moveLists = new MoveList[64];
            for (Square startPosition = Square.A1; startPosition <= Square.H8; startPosition++)
            {
                moveLists[(byte)startPosition] = new MoveList();
                foreach(MoveDirections dir in moveDirections)
                {
                    Square position = startPosition;
                    while (movePosition(ref position, dir))
                    {
                        moveLists[(byte)startPosition].Add(new Move(MoveFlags.EitherType, dir, startPosition, position, position));
                        if(!moveMultiple)
                        {
                            break;
                        }
                    }
                }
            }
            return new AllMoves(moveLists);
        }

        protected static bool movePosition(ref Square pos, MoveDirections dir)
        {
            int startRow = (int)pos >> 3;
            int startCol = (int)pos & 7;
            int dir36 = (int)dir + 36;
            int dr = (dir36 >> 3) - 4;
            int dc = (dir36 & 7) - 4;
            int endRow = startRow + dr;
            int endCol = startCol + dc;
            //Console.WriteLine(dir + "," + endRow + "," + endCol);
            if(endRow<0 || endRow>7 || endCol<0 || endCol>7)
            {
                return false;
            }

            pos = (Square)((int)pos + (int)dir);
            return true;
        }
               
    }

    class King : Piece 
    {
        private const Square whiteCastlePosition = Square.E1;
        private const Square blackCastlePosition = Square.E8;
        public override PieceType Type
        {
            get { return PieceType.King; }
        }
        public override PieceValue Value 
        {
            get { return PieceValue.King; }
        }
        public King(PieceColor c)
            : base(c)
        {
        }
        static AllMoves whiteMoves;
        static AllMoves blackMoves;
        static MoveDirections[] KingDirections = {  MoveDirections.Move_N, MoveDirections.Move_E, MoveDirections.Move_S, MoveDirections.Move_W,
                                                        MoveDirections.Move_NE, MoveDirections.Move_SE, MoveDirections.Move_SW, MoveDirections.Move_NW };
        protected override AllMoves getAllMoves()
        {
            return (Color == PieceColor.White) ? whiteMoves : blackMoves;
        }
        protected override void initMoves()
        {
            whiteMoves = initMoves(PieceColor.White);
            blackMoves = initMoves(PieceColor.Black);
        }
        private AllMoves initMoves(PieceColor colorFlags)
        {
            MoveList[] moveLists = new MoveList[64];
            for (Square startPosition = Square.A1; startPosition <= Square.H8; startPosition++)
            {
                moveLists[(byte)startPosition] = new MoveList();
                foreach(MoveDirections dir in KingDirections)
                {
                    Square position = startPosition;
                    MoveFlags moveFlags = MoveFlags.EitherType;
                    
                    while (movePosition(ref position, dir))
                    {
                        moveLists[(byte)startPosition].Add(new Move(moveFlags, dir, startPosition, position, position));

                        if ((dir == MoveDirections.Move_E || dir == MoveDirections.Move_W) && !moveFlags.HasFlag(MoveFlags.Castle))
                        {
                            moveFlags |= MoveFlags.Castle;
                            moveFlags &= ~MoveFlags.Capture;
                            if ((startPosition == whiteCastlePosition && colorFlags == PieceColor.White ||
                                  startPosition == blackCastlePosition && colorFlags == PieceColor.Black))
                            {
                                continue;
                            }

                        }                        
                        break;
                    }
                }
            }
            return new AllMoves(moveLists);
        }
    }

    class Queen : Piece
    {
        public override PieceType Type
        {
            get { return PieceType.Queen; }
        }
        public override PieceValue Value
        {
            get { return PieceValue.Queen; }
        }
        public Queen(PieceColor c)
            : base(c)
        {
        }
        static AllMoves moves;
        static MoveDirections[] QueenDirections = {  MoveDirections.Move_N, MoveDirections.Move_E, MoveDirections.Move_S, MoveDirections.Move_W,
                                                    MoveDirections.Move_NE, MoveDirections.Move_SE, MoveDirections.Move_SW, MoveDirections.Move_NW};
        protected override AllMoves getAllMoves()
        {
            return moves;
        }
        protected override void initMoves()
        {
            moves = initMoves(QueenDirections, true);
        }
    }

    class Rook : Piece
    {
        public override PieceType Type
        {
            get { return PieceType.Rook; }
        }
        public override PieceValue Value
        {
            get { return PieceValue.Rook; }
        }
        public Rook(PieceColor c)
            : base(c)
        {
        }
        static AllMoves moves;
        static MoveDirections[] RookDirections = {  MoveDirections.Move_N, MoveDirections.Move_E, MoveDirections.Move_S, MoveDirections.Move_W};
        protected override AllMoves getAllMoves()
        {
            return moves;
        }
        protected override void initMoves()
        {
            moves = initMoves(RookDirections, true);
        }
    }

    class Bishop : Piece
    {
        public override PieceType Type
        {
            get { return PieceType.Bishop; }
        }
        public override PieceValue Value
        {
            get { return PieceValue.Bishop; }
        }
        public Bishop(PieceColor c)
            : base(c)
        {
        }
        static AllMoves moves;
        static MoveDirections[] BishopDirections = { MoveDirections.Move_NE, MoveDirections.Move_SE, MoveDirections.Move_SW, MoveDirections.Move_NW };
        protected override AllMoves getAllMoves()
        {
            return moves;
        }
        protected override void initMoves()
        {
            moves = initMoves(BishopDirections, true);
        }
    }

    class Knight : Piece
    {
        public override PieceType Type
        {
            get { return PieceType.Knight; }
        }
        public override PieceValue Value
        {
            get { return PieceValue.Knight; }
        }
        public Knight(PieceColor c)
            : base(c)
        {
        }
        static AllMoves moves;
        static MoveDirections[] KnightDirections = {  MoveDirections.Move_NNE, MoveDirections.Move_ENE, MoveDirections.Move_ESE, MoveDirections.Move_SSE,
                                                MoveDirections.Move_NNW, MoveDirections.Move_WNW, MoveDirections.Move_WSW, MoveDirections.Move_SSW};
        protected override AllMoves getAllMoves()
        {
            return moves;
        }
        protected override void initMoves()
        {
            moves = initMoves(KnightDirections, false);
        }
    }

    class Pawn:Piece
    {
        public override PieceType Type
        {
            get { return PieceType.Pawn; }
        }
        public override PieceValue Value
        {
            get { return PieceValue.Pawn; }
        }
        public Pawn(PieceColor c)
            : base(c)
        {
        }
        static AllMoves whiteMoves;
        static AllMoves blackMoves;
        static MoveDirections[] WhitePawnDirections = { MoveDirections.Move_N, MoveDirections.Move_NE, MoveDirections.Move_NW, };
        static MoveDirections[] BlackPawnDirections = { MoveDirections.Move_S, MoveDirections.Move_SE, MoveDirections.Move_SW, };

        protected override AllMoves getAllMoves()
        {
            return (Color == PieceColor.White) ? whiteMoves : blackMoves;
        }
        protected override void initMoves()
        {
            whiteMoves = initMoves(WhitePawnDirections);
            blackMoves = initMoves(BlackPawnDirections);
        }
        private AllMoves initMoves(MoveDirections[] moveDirections)
        {
            MoveList[] moveLists = new MoveList[64];
            for (Square startPosition = Square.A1; startPosition <= Square.H8; startPosition++)
            {
                moveLists[(byte)startPosition] = new MoveList();
                foreach (MoveDirections dir in moveDirections)
                {
                    MoveFlags setEnPassant = 0;
                    Square position = startPosition;
                    while (movePosition(ref position, dir))
                    {
                        MoveFlags moveFlags = ((((int)dir & 7) != 0) ? MoveFlags.Capture : MoveFlags.Move) | setEnPassant;
                        if (isConversionPosition(position))
                        {
                            addConversionMoves(moveLists, startPosition, dir, position, moveFlags);
                        }
                        else
                        {
                            moveLists[(byte)startPosition].Add(new Move(moveFlags, dir, startPosition, position, position));
                        }
                        addEnPassantMoves(moveLists, startPosition, dir, position);
                        if((dir == MoveDirections.Move_N && ((byte)position >> 3) == 2) ||
                            dir == MoveDirections.Move_S && ((byte)position >> 3) == 5)
                        {
                            setEnPassant = MoveFlags.SetsEnPassant;
                            continue;
                        }
                        break;
                    }
                }
            }
            return new AllMoves(moveLists);
        }

        private static void addEnPassantMoves(MoveList[] moveLists, Square startPosition, MoveDirections dir, Square position)
        {
            if ((dir == MoveDirections.Move_NW || dir == MoveDirections.Move_NE) && ((byte)startPosition >> 3) == 4)
            {
                moveLists[(byte)startPosition].Add(new Move(MoveFlags.UseEnPassant | MoveFlags.Capture, dir, startPosition, position, position - 8));
            }
            if ((dir == MoveDirections.Move_SW || dir == MoveDirections.Move_SE) && ((byte)startPosition >> 3) == 3)
            {
                moveLists[(byte)startPosition].Add(new Move(MoveFlags.UseEnPassant | MoveFlags.Capture, dir, startPosition, position, position + 8));
            }
        }

        private static void addConversionMoves(MoveList[] moveLists, Square startPosition, MoveDirections dir, Square position, MoveFlags moveFlags)
        {
            moveFlags |= MoveFlags.Conversion;
            moveLists[(byte)startPosition].Add(new Move(moveFlags, dir, startPosition, position, position, PieceType.Queen));
            moveLists[(byte)startPosition].Add(new Move(moveFlags, dir, startPosition, position, position, PieceType.Rook));
            moveLists[(byte)startPosition].Add(new Move(moveFlags, dir, startPosition, position, position, PieceType.Bishop));
            moveLists[(byte)startPosition].Add(new Move(moveFlags, dir, startPosition, position, position, PieceType.Knight));
        }

        private static bool isConversionPosition(Square position)
        {
            return ((byte)position >> 3) == 0 || ((byte)position >> 3) == 7;
        }
    }
 
}
