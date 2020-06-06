using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess
{

    public class GameControl
    {
        public Gamestate gamestate { get; private set; }
        public PieceColor computerPlayer = PieceColor.PieceColor_NB;

        private enum Optimize { Lowest = -1, Highest = +1 }

        public GameControl()
        {
            gamestate = new Gamestate();
        }
        public GameControl(Gamestate gameState)
        {
            gamestate = gameState;
        }
        public Gamestate StartNewGame()
        {
            gamestate = new Gamestate();
            gamestate.PutPiece(new Rook(PieceColor.White), Square.A1);
            gamestate.PutPiece(new Knight(PieceColor.White), Square.B1);
            gamestate.PutPiece(new Bishop(PieceColor.White), Square.C1);
            gamestate.PutPiece(new Queen(PieceColor.White), Square.D1);
            gamestate.PutPiece(new King(PieceColor.White), Square.E1);
            gamestate.PutPiece(new Bishop(PieceColor.White), Square.F1);
            gamestate.PutPiece(new Knight(PieceColor.White), Square.G1);
            gamestate.PutPiece(new Rook(PieceColor.White), Square.H1);
            for (Square pos = Square.A2; pos <= Square.H2; pos++ )
            {
                gamestate.PutPiece(new Pawn(PieceColor.White), pos);
            }
            gamestate.PutPiece(new Rook(PieceColor.Black), Square.A8);
            gamestate.PutPiece(new Knight(PieceColor.Black), Square.B8);
            gamestate.PutPiece(new Bishop(PieceColor.Black), Square.C8);
            gamestate.PutPiece(new Queen(PieceColor.Black), Square.D8);
            gamestate.PutPiece(new King(PieceColor.Black), Square.E8);
            gamestate.PutPiece(new Bishop(PieceColor.Black), Square.F8);
            gamestate.PutPiece(new Knight(PieceColor.Black), Square.G8);
            gamestate.PutPiece(new Rook(PieceColor.Black), Square.H8);
            for (Square pos = Square.A7; pos <= Square.H7; pos++)
            {
                gamestate.PutPiece(new Pawn(PieceColor.Black), pos);
            }

            return gamestate;
        }
        public PieceColor PlayGame()
        {
            PieceColor winner = PieceColor.PieceColor_NB;
            do
            {
                IEnumerable<Move> possibleMoves = selectPieceMoves();

                Move move = selectLegalMove(possibleMoves);

                // Make the move
                makeMove(move);

                ControlledPositions controlledPositions = new ControlledPositions(gamestate);
                DebugUtil.graphGamestate(gamestate, null, controlledPositions.ControlledPositionsBitMask, controlledPositions.Check);

                gamestate.flipTurnColor();

                gamestate.setEnPassantPosition(gamestate.turnColor, null);

                controlledPositions = new ControlledPositions(gamestate);
                DebugUtil.graphGamestate(gamestate, null, controlledPositions.ControlledPositionsBitMask, controlledPositions.Check);
            } 
            while (!gameOver(ref winner));

            outputGameOver(winner);

            return winner;
        }

        private bool gameOver(ref PieceColor winner)
        {
            foreach (Square pos in gamestate.getPiecePositions())
            {
                PossibleMoves moves = new PossibleMoves(gamestate, pos);
                if (moves.MoveCount != 0)
                {
                    return false;
                }
            }
            ControlledPositions control = new ControlledPositions(gamestate, (PieceColor)(1 - gamestate.turnColor));
            if (control.Check == null)
            {
                winner = PieceColor.PieceColor_NB;
            }
            else
            {
                winner = (PieceColor)(1 - gamestate.turnColor);
            }
            return true;
        }

        private static void outputGameOver(PieceColor winner)
        {
            switch (winner)
            {
                case PieceColor.White:
                    Console.WriteLine("Game over. White wins!");
                    break;
                case PieceColor.Black:
                    Console.WriteLine("Game over. Black wins!");
                    break;
                default:
                    Console.WriteLine("Game over. It's a draw :-(");
                    break;
            }
        }

        private void makeMove(Move move)
        {
            Piece piece = movePiece(move);

            if (move.isCastle)
            {
                moveRookForCastle(move, piece);
            } 
            
            if (move.setsEnPassant)
            {
                gamestate.setEnPassantPosition(piece.Color, move.MovePosition);
            }
        }

        private Piece movePiece(Move move)
        {
            if (gamestate.GetPiece(move.CapturePosition) != null)
            {
                gamestate.RemovePiece(move.CapturePosition);
            }
            Piece piece = gamestate.GetPiece(move.StartPosition);
            gamestate.RemovePiece(move.StartPosition);
            if (move.Flags.HasFlag(MoveFlags.Conversion))
            {
                piece = Chess.makePieceOfType(move.ConversionType, piece.Color);
            }
            gamestate.PutPiece(piece, move.MovePosition);
            return piece;
        }

        private void moveRookForCastle(Move move, Piece piece)
        {
            int row = (piece.Color == PieceColor.White) ? 0 : 7;
            int startCol = (move.Direction == MoveDirections.Move_W) ? 0 : 7;
            int endCol = (move.Direction == MoveDirections.Move_W) ? 3 : 5;
            Square startPosition = (Square)(8 * row + startCol);
            Square endPosition = (Square)(8 * row + endCol);

            Piece rook = gamestate.RemovePiece(startPosition);
            gamestate.PutPiece(rook, endPosition);
        }

        private IEnumerable<Move> selectPieceMoves()
        {
            if (gamestate.turnColor == computerPlayer)
            {
                IEnumerable<Move> allMoves = getAllMoves();
                return allMoves;
            }
            do
            {
                Console.Write("Enter start position: ");
                string s = Console.ReadLine();
                if (s.Length!=2 || s[0]<'A' || s[0]>'H' || s[1] < '1' || s[1]>'8')
                {
                    Console.WriteLine("Invalid Position Format");
                    continue;
                }
                Square pos = (Square)((s[0]-'A') + 8 * (s[1]-'1'));
                Piece piece = gamestate.GetPiece(pos);
                if(piece == null || piece.Color != gamestate.turnColor)
                {
                    Console.WriteLine("Please select a square with a piece of your color");
                    continue;
                }
                PossibleMoves possibleMoves = new PossibleMoves(gamestate, pos);
                DebugUtil.graphGamestate(gamestate, pos, possibleMoves.EndPositionBitMask);
                if (possibleMoves.MoveCount == 0)
                {
                    Console.WriteLine("This piece has no possible moves");
                    continue;
                }
                return possibleMoves;
            } 
            while (true);
        }

        private IEnumerable<Move> getAllMoves()
        {
            MoveList moves = new MoveList();
            List<Square> positions = new List<Square>(gamestate.getPiecePositions());
            foreach (Square pos in positions)
            {
                Piece piece = gamestate.GetPiece(pos);
                PossibleMoves possibleMoves = new PossibleMoves(gamestate, pos);
                foreach (Move move in possibleMoves)
                {
                    moves.Add(move);
                }
            }
            return moves;
        }

        private Move selectLegalMove(IEnumerable<Move> possibleMoves)
        {
            if (gamestate.turnColor == computerPlayer)
            {
                UInt64 candidateCount = 0;
                Move bestMove = calculateBestMove(possibleMoves, 3, ref candidateCount);
                Piece piece = gamestate.GetPiece(bestMove.StartPosition);
                Console.WriteLine();
                return bestMove;
            }
            do
            {
                Console.Write("Enter end position: ");
                string s = Console.ReadLine();
                if (s.Length != 2 || s[0] < 'A' || s[0] > 'H' || s[1] < '1' || s[1] > '8')
                {
                    Console.WriteLine("Invalid Position Format");
                    continue;
                }
                Square pos = (Square)((s[0] - 'A') + 8 * (s[1] - '1'));
                IEnumerable<Move> moves = PossibleMoves.getMovesToPosition(possibleMoves, pos);
                if (moves.Count() == 0)
                {
                    Console.WriteLine("Please select a legal end position");
                    continue;
                }
                if (moves.Count() >1)
                {
                    Char c;
                    do
                    {
                        Console.WriteLine("Please select conversion type: Q R B N");
                        c = Console.ReadKey().KeyChar;
                    } 
                    while (c != 'Q' && c != 'R' && c != 'B' && c != 'N');
                    PieceType conversionType = Chess.getPieceType(c);

                    moves = moves.Where(m => (m.ConversionType == conversionType));
                }
                return moves.First();
            }
            while (true);
        }

        private Move calculateBestMove(IEnumerable<Move> possibleMoves, uint depth, ref UInt64 candidateCount)
        {
            Console.WriteLine("Please wait. Computer is thinking... ");
            Stack<Move> moveStack = new Stack<Move>();
            MoveList bestMoves = null;
            Optimize optimize = ((depth & 1) == 1) ? Optimize.Lowest : Optimize.Highest;
            int bestScore = (optimize == Optimize.Highest) ? int.MinValue : int.MaxValue;
            foreach (Move move in possibleMoves)
            {
                Console.Write(".");
                Gamestate testState = new Gamestate(gamestate);
                GameControl testControl = new GameControl(testState);
                testControl.makeMove(move);
                Piece piece = gamestate.GetPiece(move.StartPosition);
                //for (int d = 0; d <= 4 - depth; d++) Console.Write("  ");
                //Console.WriteLine(" considering move " + gamestate.turnColor + " " + piece.Type + " from " + move.StartPosition + " to " + move.MovePosition);
                int score = testControl.calculateScore(depth, ref candidateCount, (optimize == Optimize.Highest) ? Optimize.Lowest : Optimize.Highest);
                //for (int d = 0; d <= 4 - depth; d++) Console.Write("  ");
                //Console.WriteLine(" -> score " + score);
                if (optimize == Optimize.Highest && score > bestScore || optimize == Optimize.Lowest && score < bestScore)
                {
                    bestScore = score;
                    bestMoves = new MoveList();
                    bestMoves.Add(move);
                }
                else if (score == bestScore)
                {
                    bestMoves.Add(move);
                }
            }
            Random rand = new Random();
            int i = rand.Next(bestMoves.Count);
            Move bestMove = bestMoves[i];
            Piece bestPiece = gamestate.GetPiece(bestMove.StartPosition);
            Console.WriteLine();
            Console.WriteLine("Best of " + candidateCount + " moves is " + bestPiece.Color + " " + bestPiece.Type + " from " + bestMove.StartPosition + " to " + bestMove.MovePosition + " with score " + bestScore);
            return bestMove;
        }

        private int calculateScore(uint depth, ref UInt64 candidateCount, Optimize optimize)
        {
            if (depth > 0)
            {
                gamestate.flipTurnColor();
                gamestate.setEnPassantPosition(gamestate.turnColor, null);
                IEnumerable<Move> allMoves = getAllMoves();
                int bestScore = (optimize == Optimize.Highest) ? int.MinValue : int.MaxValue;
                Move bestMove = null;
                foreach (Move move in allMoves)
                {
                    Gamestate testState = new Gamestate(gamestate);
                    GameControl testControl = new GameControl(testState);
                    testControl.makeMove(move);
                    Piece piece = gamestate.GetPiece(move.StartPosition);
                    //for (int d = 0; d <= 5 - depth; d++) Console.Write("  ");
                    //Console.WriteLine(" considering move " + gamestate.turnColor + " " + piece.Type +" from " + move.StartPosition + " to " + move.MovePosition);
                    int score = testControl.calculateScore(depth - 1, ref candidateCount, (optimize == Optimize.Highest) ? Optimize.Lowest : Optimize.Highest);
                    //for (int d = 0; d <= 5 - depth; d++) Console.Write("  ");
                    //Console.WriteLine(" -> score " + score);
                    if (optimize == Optimize.Highest && score > bestScore || optimize == Optimize.Lowest && score < bestScore)
                    {
                        bestScore = score;
                        bestMove = move;
                    }
                }
                //Piece bestPiece = gamestate.GetPiece(bestMove.StartPosition);
                //for (int d = 0; d <= 5 - depth; d++) Console.Write("  ");
                //Console.WriteLine(" selected move " + gamestate.turnColor + " " + bestPiece.Type + " from " + bestMove.StartPosition + " to " + bestMove.MovePosition + " with score " + bestScore);
                return bestScore;
            }
            else
            {
                ControlledPositions cp = new ControlledPositions(gamestate);
                int score = 0;
                for (Square pos = Square.A1; pos <= Square.H8; pos++)
                {
                    Piece piece = gamestate.GetPiece(pos);
                    if (piece != null && piece.Color == gamestate.turnColor)
                    {
                            score += 3 * (int)piece.Value;
                    }
                    if ((cp.ControlledPositionsBitMask & ((UInt64)1 << (byte)pos)) != 0)
                    {
                        score += 8;
                        if (piece != null && (piece.Type != PieceType.King || piece.Color != gamestate.turnColor))
                            score += (int)piece.Value;
                    }
                }
                cp = new ControlledPositions(gamestate, (gamestate.turnColor == PieceColor.White) ? PieceColor.Black : PieceColor.White);
                for (Square pos = Square.A1; pos <= Square.H8; pos++)
                {
                    Piece piece = gamestate.GetPiece(pos);
                    if (piece != null && piece.Color != gamestate.turnColor)
                    {
                        score -= 3 * (int)piece.Value;
                    }
                    if ((cp.ControlledPositionsBitMask & ((UInt64)1 << (byte)pos)) != 0)
                    {
                        score -= 8;
                        if (piece != null && (piece.Type != PieceType.King || piece.Color != gamestate.turnColor))
                            score -= (int)piece.Value;
                    }
                }
                candidateCount++;
                return score;
            }
        }        
    }
}
