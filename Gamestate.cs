using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess
{
    public class Gamestate
    {
        Piece[] board;
        List<Square> whitePiecePositions;
        List<Square> blackPiecePositions;
        private Square?[] EnPassantPosition;
        public PieceColor turnColor { get; set; }
        public bool whiteCanCastle_EE{get; private set;}
        public bool whiteCanCastle_WW{get; private set;}
        public bool blackCanCastle_EE{get; private set;}
        public bool blackCanCastle_WW{get; private set;}

        public Gamestate()
        {
            board = new Piece[64];
            turnColor = PieceColor.White;
            whitePiecePositions = new List<Square>();
            blackPiecePositions = new List<Square>();
            EnPassantPosition = new Square?[(byte)PieceColor.PieceColor_NB];
            EnPassantPosition[(byte)PieceColor.White] = EnPassantPosition[(byte)PieceColor.Black] = null;
            whiteCanCastle_EE = whiteCanCastle_WW = blackCanCastle_EE = blackCanCastle_WW = true;
        }
        public Gamestate(Gamestate source)
        {
            board = (Piece[])source.board.Clone();
            turnColor = source.turnColor;
            whitePiecePositions = new List<Square>(source.whitePiecePositions);
            blackPiecePositions = new List<Square>(source.blackPiecePositions);
            EnPassantPosition = (Square?[])source.EnPassantPosition.Clone();
            whiteCanCastle_EE = source.whiteCanCastle_EE;
            whiteCanCastle_WW = source.whiteCanCastle_WW;
            blackCanCastle_WW = source.blackCanCastle_WW;
            blackCanCastle_EE = source.blackCanCastle_EE;
        }

        public PieceColor flipTurnColor()
        {
            turnColor = (turnColor == PieceColor.White) ? PieceColor.Black : PieceColor.White;
            return turnColor;
        }

        public Square? getEnPassantPosition(PieceColor color)
        {
            return EnPassantPosition[(byte)color];
        }
        public void setEnPassantPosition(PieceColor color, Square? pos)
        {
            EnPassantPosition[(byte)color] = pos;
        }

        public bool PutPiece(Piece piece, Square position)
        {
            if (board[(byte)position]!=null)
            {
                throw new ArgumentException();
            }
            board[(byte)position] = piece;
            List<Square> positionList = getPiecePositionList(piece.Color);
            positionList.Add(position);
            return true;
        }
        public Piece RemovePiece(Square position)
        {
            Piece piece = GetPiece(position);
            if(piece!=null)
            {
                board[(byte)position] = null;
                List<Square> positionList = getPiecePositionList(piece.Color);
                positionList.Remove(position);
            }
            updateCastleFlags(position);
            return piece;
        }

        private void updateCastleFlags(Square position)
        {
            if (position == Square.A1 || position == Square.E1)
            {
                whiteCanCastle_WW = false;
            }
            if (position == Square.E1 || position == Square.H1)
            {
                whiteCanCastle_EE = false;
            }
            if (position == Square.A8 || position == Square.E8)
            {
                blackCanCastle_WW = false;
            }
            if (position == Square.E8 || position == Square.H8)
            {
                blackCanCastle_EE = false;
            }
        }
        public Piece GetPiece(Square position)
        {
            if (board[(byte)position] != null)
            {
                return board[(byte)position];
            }
            return null;
        }
        public IEnumerable<Square> getPiecePositions(PieceColor? color = null)
        {
            if (color == null)
            {
                color = turnColor;
            }
            return (color == PieceColor.White) ? whitePiecePositions : blackPiecePositions;
        }
        private List<Square> getPiecePositionList(PieceColor color)
        {
            return (color == PieceColor.White) ? whitePiecePositions : blackPiecePositions;
        }
    }
}
