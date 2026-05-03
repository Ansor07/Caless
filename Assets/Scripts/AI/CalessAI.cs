using System.Collections.Generic;
using UnityEngine;
 
/// <summary>
/// ИИ для Caless. Два уровня сложности:
///   Лёгкий — случайный ход с предпочтением взятий
///   Средний — Minimax глубина 2 с альфа-бета отсечением
/// </summary>
public class CalessAI
{
    public enum Difficulty { Easy, Medium }
 
    public Difficulty difficulty = Difficulty.Easy;
 
    private CalessEngine engine;
 
    public CalessAI() { }
 
    public CalessAI(Difficulty diff)
    {
        difficulty = diff;
    }
    public Move GetBestMove(CalessEngine eng)
    {
        engine = eng;
        bool forWhite = engine.whiteTurn;

        // ✅ УПРОЩЁННО - убираем проблемный код рокировки
        List<Move> legalMoves = engine.GetAllLegalMoves(forWhite);
        
        if (legalMoves.Count == 0)
            return default;

        switch (difficulty)
        {
            case Difficulty.Easy:
                return GetEasyMove(legalMoves);
            case Difficulty.Medium:
                return GetMediumMove(legalMoves, forWhite);
            default:
                return legalMoves[Random.Range(0, legalMoves.Count)];
        }
    }
 
    private Move GetEasyMove(List<Move> moves)
    {
        List<Move> captures = new List<Move>();
        List<Move> royalCaptures = new List<Move>();
 
        foreach (Move m in moves)
        {
            if (m.captured != CalessEngine.EMPTY)
            {
                int capturedType = CalessEngine.PieceType(m.captured);
                if (capturedType == CalessEngine.DRAGON || capturedType == CalessEngine.TIGER)
                    royalCaptures.Add(m);
                else
                    captures.Add(m);
            }
        }
 
        if (royalCaptures.Count > 0)
            return royalCaptures[Random.Range(0, royalCaptures.Count)];
 
        if (captures.Count > 0 && Random.value < 0.5f)
            return captures[Random.Range(0, captures.Count)];
 
        return moves[Random.Range(0, moves.Count)];
    }
 
    private Move GetMediumMove(List<Move> moves, bool forWhite)
    {
        Move bestMove = moves[0];
        int bestScore = int.MinValue;
 
        moves.Sort((a, b) =>
        {
            int aScore = 0, bScore = 0;
            if (a.captured != 0)
            {
                int at = CalessEngine.PieceType(a.captured);
                aScore = CalessEngine.PieceValues.ContainsKey(at) ? CalessEngine.PieceValues[at] : 10;
            }
            if (b.captured != 0)
            {
                int bt = CalessEngine.PieceType(b.captured);
                bScore = CalessEngine.PieceValues.ContainsKey(bt) ? CalessEngine.PieceValues[bt] : 10;
            }
            return bScore.CompareTo(aScore);
        });
 
        foreach (Move move in moves)
        {
            engine.MakeMove(move);
            int score = -NegaMax(1, int.MinValue + 1, int.MaxValue, !forWhite);
            engine.UndoLastMove();
 
            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }
 
        return bestMove;
    }
 
    private int NegaMax(int depth, int alpha, int beta, bool forWhite)
    {
        if (depth == 0)
        {
            int eval = engine.Evaluate();
            return forWhite ? eval : -eval;
        }
 
        int gameOver = engine.CheckGameOver();
        if (gameOver != 0)
        {
            int mateScore = 20000 + depth;
            if ((forWhite && gameOver > 0) || (!forWhite && gameOver < 0))
                return mateScore;
            return -mateScore;
        }
 
        List<Move> moves = engine.GetAllLegalMoves(forWhite);
 
        if (moves.Count == 0)
        {
            if (engine.IsInCheck(forWhite))
                return -(20000 + depth);
            return 0;
        }
 
        moves.Sort((a, b) =>
        {
            int aScore = 0, bScore = 0;
            if (a.captured != 0)
                aScore = CalessEngine.PieceType(Mathf.Abs(a.captured)) * 10;
            if (b.captured != 0)
                bScore = CalessEngine.PieceType(Mathf.Abs(b.captured)) * 10;
            return bScore.CompareTo(aScore);
        });
 
        int best = int.MinValue;
        foreach (Move move in moves)
        {
            engine.MakeMove(move);
            int score = -NegaMax(depth - 1, -beta, -alpha, !forWhite);
            engine.UndoLastMove();
 
            if (score > best) best = score;
            if (score > alpha) alpha = score;
            if (alpha >= beta) break;
        }
 
        return best;
    }
}