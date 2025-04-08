using System.Collections.Generic;
using UnityEngine;

public class MiniMax
{
    private int evaluatedPositions;
    private const int MaxScore = 1000000;
    private const int MinScore = -MaxScore;
    private static readonly int[] PieceValues = { 100, 320, 330, 500, 900, 20000 };

    public (int, Move) MinMax(int depth, bool maximisingPlayer, int alpha = -1000000, int beta = 1000000)
    {//depth is how many moves ahead to search; maximizingplayer  is whether current player is maximizing or minimizing; alpha/beta: best already explored for max/min
        evaluatedPositions++;
        if (depth == 0) return ((int)Evaluate(), null);

        List<Move> moves = GameManager.instance.GenerateAllMoves(maximisingPlayer ? 
            GameManager.instance.black : GameManager.instance.white);
        moves = SortMoves(moves, maximisingPlayer);

        int bestMoveVal = maximisingPlayer ? MinScore : MaxScore;
        Move bestMove = null;

        foreach (Move move in moves)
        {
            bool pieceCaptured = false;
            GameObject captured = null;
            bool pawnMoved = false;

            if (move.piece.name.Contains("Pawn") && !GameManager.instance.movedPawns.Contains(move.piece))
                pawnMoved = true;

            if (GameManager.instance.PieceAtGrid(move.dest) == null)
                GameManager.instance.Move(move.piece, move.dest, false);
            else
            {
                captured = GameManager.instance.CapturePieceAt(move.dest, false, maximisingPlayer ? 
                    GameManager.instance.black : GameManager.instance.white);
                GameManager.instance.Move(move.piece, move.dest, false);
                pieceCaptured = true;
            }

            int value = MinMax(depth - 1, !maximisingPlayer, alpha, beta).Item1;

            if (maximisingPlayer)
            {
                if (value > bestMoveVal)
                {
                    bestMoveVal = value;
                    bestMove = move;
                }
                alpha = Math.Max(alpha, bestMoveVal);
            }
            else
            {
                if (value < bestMoveVal)
                {
                    bestMoveVal = value;
                    bestMove = move;
                }
                beta = Math.Min(beta, bestMoveVal);
            }

            GameManager.instance.Move(move.piece, move.source, false);
            if (pieceCaptured) GameManager.instance.UndoDelete(captured, move.dest, 
                maximisingPlayer ? GameManager.instance.black : GameManager.instance.white);
            if (pawnMoved) GameManager.instance.movedPawns.Remove(move.piece);

            if (beta <= alpha) break;
        }

        return (bestMoveVal, bestMove);
    }

    private float Evaluate()
    {// calculates the board state's numberical score; 
        float pieceScore = 0; //initializes total score to 0 
        for (int i = 0; i < 8; i++)// scans all board positions 
        {
            for (int j = 0; j < 8; j++)// gets base value of each piece, adds positions bonus from piecesquaretable; if black it adds score, if white it subtracts. 
            {
                GameObject piece = GameManager.instance.pieces[j, i];
                if (piece != null)
                {
                    Piece pieceScript = piece.GetComponent<Piece>();
                    bool isBlack = piece.name.Contains("Black");
                    int value = PieceValues[(int)pieceScript.type];
                    float posScore = GetPositionScore(pieceScript, j, i, isBlack);
                    
                    pieceScore += isBlack ? value + posScore : -value - posScore;
                }
            }
        }
        return pieceScore;// returns a final score; positive favors black negative favors white 
    }

    private float GetPositionScore(Piece piece, int row, int col, bool isBlack)
    {// gets positional bonus for a piece 
        switch (piece.type)// checks piece type and returns the corresponding value from piecesquarertable 
        {
            case PieceType.Pawn: return isBlack ? PieceSquareTable.pawnScoreBlack[row, col] : PieceSquareTable.pawnScoreWhite[row, col];
            case PieceType.Bishop: return isBlack ? PieceSquareTable.bishopScoreBlack[row, col] : PieceSquareTable.bishopScoreWhite[row, col];
            case PieceType.Knight: return PieceSquareTable.knightScore[row, col];
            case PieceType.Rook: return isBlack ? PieceSquareTable.rookScoreBlack[row, col] : PieceSquareTable.rookScoreWhite[row, col];
            case PieceType.Queen: return PieceSquareTable.queenScore[row, col];
            case PieceType.King: return isBlack ? PieceSquareTable.kingScoreBlack[row, col] : PieceSquareTable.kingScoreWhite[row, col];
            default: return 0;
        }
    }

    private List<Move> SortMoves(List<Move> moves, bool maximisingPlayer)
    {//improves purning efficiency; prioritizes captures then non captures; uses piece values to order captures 
        moves.Sort((a, b) => {
            bool aCapture = GameManager.instance.PieceAtGrid(a.dest) != null;
            bool bCapture = GameManager.instance.PieceAtGrid(b.dest) != null;
            
            if (aCapture != bCapture) return bCapture.CompareTo(aCapture);
            if (aCapture) return GetPieceValue(GameManager.instance.PieceAtGrid(b.dest))
                .CompareTo(GetPieceValue(GameManager.instance.PieceAtGrid(a.dest)));
            return 0;
        });
        return moves;
    }

    private int GetPieceValue(GameObject piece)
    {
        return piece != null ? PieceValues[(int)piece.GetComponent<Piece>().type] : 0;
    }

    public void EvaluatedMovesCount()
    {
        Debug.Log("Moves evaluated: " + evaluatedPositions);
        evaluatedPositions = 0;
    }
}