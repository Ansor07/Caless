using System;
using System.Collections.Generic;
using UnityEngine;
 
/// <summary>
/// Структура хода. Содержит всю информацию для выполнения и отката.
/// </summary>
public struct Move
{
    public Vector2Int from;
    public Vector2Int to;
    public int piece;
    public int captured;
 
    // Спецходы
    public bool isCastling;
    public Vector2Int castlePieceFrom;
    public Vector2Int castlePieceTo;
 
    public bool isTeleport;
 
    public bool isRevive;
    public int revivedPiece;
    public Vector2Int revivePos;
 
    public bool isDragonRanged;
 
    public Move(int fr, int fc, int tr, int tc, int piece, int captured)
    {
        this.from = new Vector2Int(fr, fc);
        this.to = new Vector2Int(tr, tc);
        this.piece = piece;
        this.captured = captured;
        isCastling = false;
        castlePieceFrom = Vector2Int.zero;
        castlePieceTo = Vector2Int.zero;
        isTeleport = false;
        isRevive = false;
        revivedPiece = 0;
        revivePos = Vector2Int.zero;
        isDragonRanged = false;
    }
}
 
/// <summary>
/// Ядро движка Caless. 10×10 доска, 12 типов фигур, оригинальные правила.
/// Положительные = белые, отрицательные = чёрные.
/// </summary>
public class CalessEngine
{
    // Типы фигур
    public const int EMPTY    = 0;
    public const int RAT      = 1;
    public const int OX       = 2;
    public const int TIGER    = 3;
    public const int RABBIT   = 4;
    public const int DRAGON   = 5;
    public const int SNAKE    = 6;
    public const int HORSE    = 7;
    public const int GOAT     = 8;
    public const int MONKEY   = 9;
    public const int ROOSTER  = 10;
    public const int DOG      = 11;
    public const int PIG      = 12;
 
    public const int BOARD_SIZE = 10;
 
    // Доска
    public int[,] board = new int[BOARD_SIZE, BOARD_SIZE];
 
    // Текущий ход
    public bool whiteTurn = true;
 
    // Королевские фигуры
    public Vector2Int whiteDragonPos;
    public Vector2Int blackDragonPos;
    public Vector2Int whiteTigerPos;
    public Vector2Int blackTigerPos;
    public bool whiteDragonAlive = true;
    public bool blackDragonAlive = true;
    public bool whiteTigerAlive = true;
    public bool blackTigerAlive = true;
 
    // Флаги спецходов
    public bool whiteDragonMoved = false;
    public bool blackDragonMoved = false;
    public bool whiteCastleUsed = false;
    public bool blackCastleUsed = false;
    public bool whiteTempleUsed = false;
    public bool blackTempleUsed = false;
 
    // Храм — клетка i5 = (4, 8)
    public static readonly Vector2Int TEMPLE_SQUARE = new Vector2Int(4, 8);
 
    // Отслеживание первых ходов (для Rabbit и Rooster)
    public HashSet<long> movedPieces = new HashSet<long>();
 
    // Отслеживание оживлённых фигур (Goat revive)
    public HashSet<long> revivedPieces = new HashSet<long>();
 
    // Отслеживание последовательных ходов одной фигурой (бонус +1)
    public Vector2Int whiteLastMoveDest = new Vector2Int(-1, -1);
    public int whiteConsecutiveCount = 0;
    public Vector2Int blackLastMoveDest = new Vector2Int(-1, -1);
    public int blackConsecutiveCount = 0;
 
    // Ожидание оживления (Goat)
    public bool pendingRevive = false;
    public int pendingRevivePiece = 0;
    public Vector2Int pendingReviveGoatPos;
    public Vector2Int pendingReviveCapturePos;
 
    // История ходов
    public List<MoveRecord> moveHistory = new List<MoveRecord>();
 
    public struct MoveRecord
    {
        public Move move;
        public bool silent;
        public bool prevWhiteTurn;
        public bool prevMovedAtFrom;
        public bool prevMovedAtTo;
        public bool prevWhiteDragonMoved;
        public bool prevBlackDragonMoved;
        public bool prevWhiteCastleUsed;
        public bool prevBlackCastleUsed;
        public bool prevWhiteTempleUsed;
        public bool prevBlackTempleUsed;
        public bool prevWhiteDragonAlive;
        public bool prevBlackDragonAlive;
        public bool prevWhiteTigerAlive;
        public bool prevBlackTigerAlive;
        public Vector2Int prevWhiteDragonPos;
        public Vector2Int prevBlackDragonPos;
        public Vector2Int prevWhiteTigerPos;
        public Vector2Int prevBlackTigerPos;
        public Vector2Int prevWhiteLastMoveDest;
        public int prevWhiteConsecutiveCount;
        public Vector2Int prevBlackLastMoveDest;
        public int prevBlackConsecutiveCount;
    }
    // Названия фигур
    public static readonly Dictionary<int, string> PieceNames = new Dictionary<int, string>
    {
        { RAT, "Крыса" },
        { OX, "Бык" },
        { TIGER, "Тигр" },
        { RABBIT, "Кролик" },
        { DRAGON, "Дракон" },
        { SNAKE, "Змея" },
        { HORSE, "Лошадь" },
        { GOAT, "Козёл" },
        { MONKEY, "Обезьяна" },
        { ROOSTER, "Петух" },
        { DOG, "Собака" },
        { PIG, "Свинья" }
    };
 
    // Символы фигур (для UI)
    public static readonly Dictionary<int, string> PieceSymbols = new Dictionary<int, string>
    {
        { RAT, "🐀" }, { OX, "🐂" }, { TIGER, "🐅" }, { RABBIT, "🐇" },
        { DRAGON, "🐉" }, { SNAKE, "🐍" }, { HORSE, "🐎" }, { GOAT, "🐐" },
        { MONKEY, "🐒" }, { ROOSTER, "🐓" }, { DOG, "🐕" }, { PIG, "🐖" }
    };
 
    // Ценность фигур (для ИИ)
    public static readonly Dictionary<int, int> PieceValues = new Dictionary<int, int>
    {
        { EMPTY, 0 }, { RAT, 30 }, { OX, 70 }, { TIGER, 5000 }, { RABBIT, 15 },
        { DRAGON, 5000 }, { SNAKE, 50 }, { HORSE, 40 }, { GOAT, 35 },
        { MONKEY, 120 }, { ROOSTER, 15 }, { DOG, 90 }, { PIG, 70 }
    };
 
    public static bool IsWhite(int piece) => piece > 0;
    public static bool IsBlack(int piece) => piece < 0;
    public static int PieceType(int piece) => Mathf.Abs(piece);
    public static int PieceColor(int piece) => piece > 0 ? 1 : (piece < 0 ? -1 : 0);
 
    public static string GetPieceName(int piece)
    {
        int type = PieceType(piece);
        return PieceNames.ContainsKey(type) ? PieceNames[type] : "";
    }
 
    public static bool InBounds(int row, int col)
    {
        return row >= 0 && row < BOARD_SIZE && col >= 0 && col < BOARD_SIZE;
    }
 
    private static long PosKey(int row, int col)
    {
        return (long)row * BOARD_SIZE + col;
    }
 
    public bool HasPieceMoved(int row, int col)
    {
        return movedPieces.Contains(PosKey(row, col));
    }
 
    // ==================== ИНИЦИАЛИЗАЦИЯ ====================
 
    public void InitializeBoard() { InitStartPosition(); }
 
    public void InitStartPosition()
    {
        board = new int[BOARD_SIZE, BOARD_SIZE];
 
        // Белые (ряд 0 — задний ряд)
        int[] backRow = { GOAT, HORSE, MONKEY, OX, TIGER, DRAGON, SNAKE, RAT, DOG, PIG };
        for (int c = 0; c < BOARD_SIZE; c++)
            board[0, c] = backRow[c];
 
        // Белые — второй ряд (чередование Петух/Кролик)
        for (int c = 0; c < BOARD_SIZE; c++)
            board[1, c] = (c % 2 == 0) ? ROOSTER : RABBIT;
 
        // Чёрные — предпоследний ряд
        for (int c = 0; c < BOARD_SIZE; c++)
            board[8, c] = -((c % 2 == 0) ? ROOSTER : RABBIT);
 
        // Чёрные (ряд 9 — задний ряд)
        for (int c = 0; c < BOARD_SIZE; c++)
            board[9, c] = -backRow[c];
 
        // Позиции королевских фигур
        whiteDragonPos = new Vector2Int(0, 5);
        blackDragonPos = new Vector2Int(9, 5);
        whiteTigerPos = new Vector2Int(0, 4);
        blackTigerPos = new Vector2Int(9, 4);
 
        whiteDragonAlive = true;
        blackDragonAlive = true;
        whiteTigerAlive = true;
        blackTigerAlive = true;
 
        whiteDragonMoved = false;
        blackDragonMoved = false;
        whiteCastleUsed = false;
        blackCastleUsed = false;
        whiteTempleUsed = false;
        blackTempleUsed = false;
 
        whiteTurn = true;
        movedPieces.Clear();
        revivedPieces.Clear();
        moveHistory.Clear();
        pendingRevive = false;
 
        whiteLastMoveDest = new Vector2Int(-1, -1);
        whiteConsecutiveCount = 0;
        blackLastMoveDest = new Vector2Int(-1, -1);
        blackConsecutiveCount = 0;
    }
 
    // ==================== ГЕНЕРАЦИЯ ХОДОВ ====================
 
    public List<Move> GetAllLegalMoves(bool forWhite)
    {
        List<Move> legalMoves = new List<Move>();
        int colorSign = forWhite ? 1 : -1;
 
        for (int r = 0; r < BOARD_SIZE; r++)
        {
            for (int c = 0; c < BOARD_SIZE; c++)
            {
                if (board[r, c] * colorSign > 0)
                {
                    List<Move> pieceMoves = GetLegalMovesForPiece(r, c);
                    legalMoves.AddRange(pieceMoves);
                }
            }
        }
        return legalMoves;
    }
 
    public List<Move> GetLegalMovesForPiece(int row, int col)
    {
        List<Move> pseudoMoves = GetPseudoLegalMoves(row, col);
        List<Move> legalMoves = new List<Move>();
        bool isWhitePiece = board[row, col] > 0;
 
        foreach (Move m in pseudoMoves)
        {
            MakeMove(m, true);
            bool invalid = IsRoyalUnderThreat(isWhitePiece);
            UndoLastMove();
 
            if (!invalid)
                legalMoves.Add(m);
        }
        return legalMoves;
    }
 
    /// <summary>
    /// Проверка: уязвимая королевская фигура под атакой?
    /// Kal возможен только когда одна из двух королевских фигур уже уничтожена.
    /// </summary>
    public bool IsRoyalUnderThreat(bool forWhite)
    {
        if (forWhite)
        {
            if (!whiteDragonAlive && !whiteTigerAlive) return false;
 
            // Обе фигуры живы — нет Kal
            if (whiteDragonAlive && whiteTigerAlive) return false;
 
            // Только Дракон жив — он уязвим
            if (whiteDragonAlive && !whiteTigerAlive)
                return IsSquareAttacked(whiteDragonPos.x, whiteDragonPos.y, false);
 
            // Только Тигр жив — он уязвим (движется как Дракон)
            if (!whiteDragonAlive && whiteTigerAlive)
                return IsSquareAttacked(whiteTigerPos.x, whiteTigerPos.y, false);
        }
        else
        {
            if (!blackDragonAlive && !blackTigerAlive) return false;
            if (blackDragonAlive && blackTigerAlive) return false;
 
            if (blackDragonAlive && !blackTigerAlive)
                return IsSquareAttacked(blackDragonPos.x, blackDragonPos.y, true);
 
            if (!blackDragonAlive && blackTigerAlive)
                return IsSquareAttacked(blackTigerPos.x, blackTigerPos.y, true);
        }
        return false;
    }
 
    /// <summary>
    /// Проверка: может ли атакующая сторона атаковать эту клетку?
    /// </summary>
    public bool IsSquareAttacked(int row, int col, bool byWhite)
    {
        int attackerSign = byWhite ? 1 : -1;
 
        for (int r = 0; r < BOARD_SIZE; r++)
        {
            for (int c = 0; c < BOARD_SIZE; c++)
            {
                int piece = board[r, c];
                if (piece * attackerSign <= 0) continue;
 
                List<Move> moves = GetPseudoLegalMoves(r, c);
                foreach (Move m in moves)
                {
                    if (m.to.x == row && m.to.y == col)
                        return true;
                }
            }
        }
        return false;
    }
 
    /// <summary>
    /// Проверка на Kal (шах) для текущей стороны
    /// </summary>
    public bool IsInCheck(bool forWhite)
    {
        return IsRoyalUnderThreat(forWhite);
    }
 
    /// <summary>
    /// Проверка: текущая сторона в Kal и нет легальных ходов = Рас (мат)
    /// </summary>
    public bool IsCheckmate(bool forWhite)
    {
        if (!IsInCheck(forWhite)) return false;
        return GetAllLegalMoves(forWhite).Count == 0;
    }
 
    /// <summary>
    /// Проверка победы: обе королевские фигуры противника уничтожены или Рас
    /// </summary>
    public int CheckGameOver()
    {
        // Белые проиграли
        if (!whiteDragonAlive && !whiteTigerAlive) return -1;
        // Чёрные проиграли
        if (!blackDragonAlive && !blackTigerAlive) return 1;
 
        // Рас (мат)
        if (IsCheckmate(true)) return -1;  // белым мат
        if (IsCheckmate(false)) return 1;  // чёрным мат
 
        return 0; // Игра продолжается
    }
 
    // ==================== ПСЕВДО-ЛЕГАЛЬНЫЕ ХОДЫ ====================
 
    public List<Move> GetPseudoLegalMoves(int row, int col)
    {
        List<Move> moves = new List<Move>();
        int piece = board[row, col];
        if (piece == EMPTY) return moves;
 
        int type = PieceType(piece);
        bool isWhite = IsWhite(piece);
        int colorSign = isWhite ? 1 : -1;
 
        int bonus = GetConsecutiveBonus(row, col, isWhite);
 
        switch (type)
        {
            case RAT:
                AddRatMoves(moves, row, col, colorSign, bonus);
                break;
            case OX:
                AddOxMoves(moves, row, col, colorSign, bonus);
                break;
            case TIGER:
                AddTigerMoves(moves, row, col, isWhite, colorSign, bonus);
                break;
            case RABBIT:
                AddRabbitMoves(moves, row, col, isWhite, colorSign);
                break;
            case DRAGON:
                AddDragonMoves(moves, row, col, isWhite, colorSign, bonus);
                break;
            case SNAKE:
                AddSnakeMoves(moves, row, col, colorSign, bonus);
                break;
            case HORSE:
                AddHorseMoves(moves, row, col, colorSign);
                break;
            case GOAT:
                AddGoatMoves(moves, row, col, colorSign, bonus);
                break;
            case MONKEY:
                AddMonkeyMoves(moves, row, col, colorSign, bonus);
                break;
            case ROOSTER:
                AddRoosterMoves(moves, row, col, isWhite, colorSign);
                break;
            case DOG:
                AddDogMoves(moves, row, col, colorSign, bonus);
                break;
            case PIG:
                AddPigMoves(moves, row, col, colorSign, bonus);
                break;
        }
 
        return moves;
    }
 
    private int GetConsecutiveBonus(int row, int col, bool isWhite)
    {
        if (isWhite)
        {
            if (whiteLastMoveDest.x == row && whiteLastMoveDest.y == col && whiteConsecutiveCount >= 3)
                return 1;
        }
        else
        {
            if (blackLastMoveDest.x == row && blackLastMoveDest.y == col && blackConsecutiveCount >= 3)
                return 1;
        }
        return 0;
    }
 
    // --- Крыса: ровно 3 клетки в любом из 8 направлений ---
    private void AddRatMoves(List<Move> moves, int row, int col, int colorSign, int bonus)
    {
        int dist = 3 + bonus;
        int[,] dirs = { {1,0},{-1,0},{0,1},{0,-1},{1,1},{1,-1},{-1,1},{-1,-1} };
 
        for (int d = 0; d < dirs.GetLength(0); d++)
        {
            int dr = dirs[d, 0], dc = dirs[d, 1];
            bool blocked = false;
 
            for (int step = 1; step <= dist; step++)
            {
                int nr = row + dr * step;
                int nc = col + dc * step;
                if (!InBounds(nr, nc)) { blocked = true; break; }
 
                if (step < dist)
                {
                    if (board[nr, nc] != EMPTY) { blocked = true; break; }
                }
                else
                {
                    if (!blocked)
                    {
                        int target = board[nr, nc];
                        if (target * colorSign <= 0)
                            moves.Add(new Move(row, col, nr, nc, board[row, col], target));
                    }
                }
            }
        }
    }
 
    // --- Бык: до 4 клеток в любом из 8 направлений ---
    private void AddOxMoves(List<Move> moves, int row, int col, int colorSign, int bonus)
    {
        int maxDist = 4 + bonus;
        AddSlidingMoves(moves, row, col, colorSign,
            new int[,] { {1,0},{-1,0},{0,1},{0,-1},{1,1},{1,-1},{-1,1},{-1,-1} }, maxDist);
    }
 
    // --- Тигр: как ферзь пока Дракон жив, иначе 1 клетка ---
    private void AddTigerMoves(List<Move> moves, int row, int col, bool isWhite, int colorSign, int bonus)
    {
        bool dragonAlive = isWhite ? whiteDragonAlive : blackDragonAlive;
        int[,] dirs = { {1,0},{-1,0},{0,1},{0,-1},{1,1},{1,-1},{-1,1},{-1,-1} };
 
        if (dragonAlive)
        {
            int maxDist = BOARD_SIZE + bonus;
            AddSlidingMoves(moves, row, col, colorSign, dirs, maxDist);
        }
        else
        {
            int range = 1 + bonus;
            for (int d = 0; d < dirs.GetLength(0); d++)
            {
                for (int step = 1; step <= range; step++)
                {
                    int nr = row + dirs[d, 0] * step;
                    int nc = col + dirs[d, 1] * step;
                    if (!InBounds(nr, nc)) break;
                    int target = board[nr, nc];
                    if (target * colorSign > 0) break;
                    moves.Add(new Move(row, col, nr, nc, board[row, col], target));
                    if (target != EMPTY) break;
                }
            }
        }
    }
 
    // --- Кролик: 2 вперёд (первый ход), затем 1 вперёд. Атака: вертикально вперёд ---
    private void AddRabbitMoves(List<Move> moves, int row, int col, bool isWhite, int colorSign)
    {
        int forward = isWhite ? 1 : -1;
        bool firstMove = !HasPieceMoved(row, col);
        bool atBackRank = isWhite ? (row == BOARD_SIZE - 1) : (row == 0);  // ✅ Дошли до конца!

        if (atBackRank)
        {
            // ✅ ДОШЛИ ДО КОНЦА → 2 клетки ЛЮБОМ направлении!
            int[,] allDirs = { {1,0},{-1,0},{0,1},{0,-1},{1,1},{1,-1},{-1,1},{-1,-1} };
            for (int d = 0; d < allDirs.GetLength(0); d++)
            {
                for (int step = 1; step <= 2; step++)  // 1 или 2 клетки
                {
                    int nr = row + allDirs[d, 0] * step;
                    int nc = col + allDirs[d, 1] * step;
                    if (!InBounds(nr, nc)) break;
                    int target = board[nr, nc];
                    if (target * colorSign > 0) break;
                    moves.Add(new Move(row, col, nr, nc, board[row, col], target));
                    if (target != EMPTY) break;
                }
            }
            return;
        }

        // Обычные ходы
        int nr1 = row + forward;
        if (InBounds(nr1, col) && board[nr1, col] == EMPTY)
        {
            moves.Add(new Move(row, col, nr1, col, board[row, col], EMPTY));
            
            // ✅ ПЕРВЫЙ ХОД - 2 вперёд
            if (firstMove)
            {
                int nr2 = row + forward * 2;
                if (InBounds(nr2, col) && board[nr2, col] == EMPTY)
                    moves.Add(new Move(row, col, nr2, col, board[row, col], EMPTY));
            }
        }

        // Атака вертикально вперёд (только если в пределах доски)
        if (InBounds(nr1, col))
        {
            int target1 = board[nr1, col];
            if (target1 * colorSign < 0)
                moves.Add(new Move(row, col, nr1, col, board[row, col], target1));
        }
    }
 
    // --- Дракон: 1 клетка в любом направлении + дальняя атака по вертикали/горизонтали ---
    private void AddDragonMoves(List<Move> moves, int row, int col, bool isWhite, int colorSign, int bonus)
    {
        // 1. ХОДЫ: ТОЛЬКО орто 1 клетка
        int[,] orthoDirs = { {1,0},{-1,0},{0,1},{0,-1} };
        for (int d = 0; d < orthoDirs.GetLength(0); d++)
        {
            int dr = orthoDirs[d, 0], dc = orthoDirs[d, 1];
            int nr = row + dr, nc = col + dc;
            if (InBounds(nr, nc) && board[nr, nc] * colorSign <= 0)
                moves.Add(new Move(row, col, nr, nc, board[row, col], board[nr, nc]));
        }

        // 2. ЗАХВАТ ВРАГОВ РЯДОМ: ЛЮБОЕ направление
        int[,] allDirs = { 
            {1,0},{-1,0},{0,1},{0,-1},
            {1,1},{1,-1},{-1,1},{-1,-1}
        };
        
        for (int d = 0; d < allDirs.GetLength(0); d++)  // ✅ ФИКС!
        {
            int dr = allDirs[d, 0];
            int dc = allDirs[d, 1];
            int nr = row + dr;
            int nc = col + dc;
            
            if (InBounds(nr, nc))
            {
                int target = board[nr, nc];
                if (target * colorSign < 0)  // ВРАГ РЯДОМ
                {
                    Move captureMove = new Move(row, col, nr, nc, board[row, col], target);
                    captureMove.isDragonRanged = true;
                    moves.Add(captureMove);
                }
            }
        }
    }
 
    // --- Змея: до 6 клеток по диагонали ---
    private void AddSnakeMoves(List<Move> moves, int row, int col, int colorSign, int bonus)
    {
        int maxDist = 6 + bonus;
        AddSlidingMoves(moves, row, col, colorSign,
            new int[,] { {1,1},{1,-1},{-1,1},{-1,-1} }, maxDist);
    }
 
    // --- Лошадь: паттерн V-H-V (прыгает) ---
    private void AddHorseMoves(List<Move> moves, int row, int col, int colorSign)
    {
        int[,] offsets = { {2,1},{2,-1},{-2,1},{-2,-1},{0,1},{0,-1} };
 
        for (int i = 0; i < offsets.GetLength(0); i++)
        {
            int nr = row + offsets[i, 0];
            int nc = col + offsets[i, 1];
            if (!InBounds(nr, nc)) continue;
            if (offsets[i, 0] == 0 && offsets[i, 1] != 0) continue; // Пропускаем тривиальные (0,±1)
            int target = board[nr, nc];
            if (target * colorSign <= 0)
                moves.Add(new Move(row, col, nr, nc, board[row, col], target));
        }
    }
 
    // --- Козёл: 1 клетка в любом направлении ---
    private void AddGoatMoves(List<Move> moves, int row, int col, int colorSign, int bonus)
    {
        int range = 1 + bonus;
        int[,] dirs = { {1,0},{-1,0},{0,1},{0,-1},{1,1},{1,-1},{-1,1},{-1,-1} };
 
        for (int d = 0; d < dirs.GetLength(0); d++)
        {
            for (int step = 1; step <= range; step++)
            {
                int nr = row + dirs[d, 0] * step;
                int nc = col + dirs[d, 1] * step;
                if (!InBounds(nr, nc)) break;
                int target = board[nr, nc];
                if (target * colorSign > 0) break;
                moves.Add(new Move(row, col, nr, nc, board[row, col], target));
                if (target != EMPTY) break;
            }
        }
    }
 
    // --- Обезьяна: шахматный конь (8 направлений, прыжок) + змея (до 6 по диагонали) ---
    private void AddMonkeyMoves(List<Move> moves, int row, int col, int colorSign, int bonus)
    {
        // Ходы коня (стандартные L-shape)
        int[,] knightOffsets = { {2,1},{2,-1},{-2,1},{-2,-1},{1,2},{1,-2},{-1,2},{-1,-2} };
        for (int i = 0; i < knightOffsets.GetLength(0); i++)
        {
            int nr = row + knightOffsets[i, 0];
            int nc = col + knightOffsets[i, 1];
            if (!InBounds(nr, nc)) continue;
            int target = board[nr, nc];
            if (target * colorSign <= 0)
                moves.Add(new Move(row, col, nr, nc, board[row, col], target));
        }
 
        // Ходы змеи (до 6 по диагонали)
        int maxDist = 6 + bonus;
        AddSlidingMoves(moves, row, col, colorSign,
            new int[,] { {1,1},{1,-1},{-1,1},{-1,-1} }, maxDist);
    }
 
    // --- Петух: 2 вперёд (первый ход), затем 1 вперёд. Атака: диагонально вперёд ---
    private void AddRoosterMoves(List<Move> moves, int row, int col, bool isWhite, int colorSign)
    {
        int forward = isWhite ? 1 : -1;
        bool firstMove = !HasPieceMoved(row, col);
        bool atBackRank = isWhite ? (row == BOARD_SIZE - 1) : (row == 0);  // ✅ Дошли до конца!

        if (atBackRank)
        {
            // ✅ ДОШЛИ ДО КОНЦА → 2 клетки ЛЮБОМ направлении!
            int[,] allDirs = { {1,0},{-1,0},{0,1},{0,-1},{1,1},{1,-1},{-1,1},{-1,-1} };
            for (int d = 0; d < allDirs.GetLength(0); d++)
            {
                for (int step = 1; step <= 2; step++)  // 1 или 2 клетки
                {
                    int nr = row + allDirs[d, 0] * step;
                    int nc = col + allDirs[d, 1] * step;
                    if (!InBounds(nr, nc)) break;
                    int target = board[nr, nc];
                    if (target * colorSign > 0) break;
                    moves.Add(new Move(row, col, nr, nc, board[row, col], target));
                    if (target != EMPTY) break;
                }
            }
            return;
        }

        // Обычные ходы
        int nr1 = row + forward;
        if (InBounds(nr1, col) && board[nr1, col] == EMPTY)
        {
            moves.Add(new Move(row, col, nr1, col, board[row, col], EMPTY));
            
            // ✅ ПЕРВЫЙ ХОД - 2 вперёд
            if (firstMove)
            {
                int nr2 = row + forward * 2;
                if (InBounds(nr2, col) && board[nr2, col] == EMPTY)
                    moves.Add(new Move(row, col, nr2, col, board[row, col], EMPTY));
            }
        }

        // ✅ АТАКА - диагональ ВПЕРЁД
        int[] dcArr = { -1, 1 };
        foreach (int dc in dcArr)
        {
            int nc = col + dc;
            if (InBounds(nr1, nc))
            {
                int target = board[nr1, nc];
                if (target * colorSign < 0)
                    moves.Add(new Move(row, col, nr1, nc, board[row, col], target));
            }
        }
    }
 
    // --- Собака: 1 клетка по диагонали + ходы Свиньи (ладья) ---
    private void AddDogMoves(List<Move> moves, int row, int col, int colorSign, int bonus)
    {
        // Диагональ 1 клетка
        int diagRange = 1 + bonus;
        int[,] diagDirs = { {1,1},{1,-1},{-1,1},{-1,-1} };
        for (int d = 0; d < diagDirs.GetLength(0); d++)
        {
            for (int step = 1; step <= diagRange; step++)
            {
                int nr = row + diagDirs[d, 0] * step;
                int nc = col + diagDirs[d, 1] * step;
                if (!InBounds(nr, nc)) break;
                int target = board[nr, nc];
                if (target * colorSign > 0) break;
                moves.Add(new Move(row, col, nr, nc, board[row, col], target));
                if (target != EMPTY) break;
            }
        }
 
        // Ходы Свиньи (ладья — ортогональное скольжение)
        int maxRook = BOARD_SIZE + bonus;
        AddSlidingMoves(moves, row, col, colorSign,
            new int[,] { {1,0},{-1,0},{0,1},{0,-1} }, maxRook);
    }
 
    // --- Свинья: ладья (ортогональное скольжение) ---
    private void AddPigMoves(List<Move> moves, int row, int col, int colorSign, int bonus)
    {
        int maxDist = BOARD_SIZE + bonus;
        AddSlidingMoves(moves, row, col, colorSign,
            new int[,] { {1,0},{-1,0},{0,1},{0,-1} }, maxDist);
    }
 
    // --- Общий метод скольжения ---
    private void AddSlidingMoves(List<Move> moves, int row, int col, int colorSign, int[,] dirs, int maxDist)
    {
        for (int d = 0; d < dirs.GetLength(0); d++)
        {
            int dr = dirs[d, 0], dc = dirs[d, 1];
            for (int step = 1; step <= maxDist; step++)
            {
                int nr = row + dr * step;
                int nc = col + dc * step;
                if (!InBounds(nr, nc)) break;
                int target = board[nr, nc];
                if (target * colorSign > 0) break;
                moves.Add(new Move(row, col, nr, nc, board[row, col], target));
                if (target != EMPTY) break;
            }
        }
    }
 
    // ==================== РОКИРОВКА (Вертикальный Королевский Сдвиг) ====================
 
    public List<Move> GetCastlingMoves(bool forWhite)
    {
        List<Move> castleMoves = new List<Move>();

        // ✅ Только если Дракон жив и НЕ двигался
        if (forWhite && (!whiteDragonAlive || whiteDragonMoved)) return castleMoves;
        if (!forWhite && (!blackDragonAlive || blackDragonMoved)) return castleMoves;

        // ✅ Разрешенные фигуры
        int[] castleTypes = { PIG, GOAT, HORSE, DOG, RABBIT, TIGER };

        Vector2Int dragonPos = forWhite ? whiteDragonPos : blackDragonPos;
        int dragonCol = dragonPos.y;

        // ✅ Ищем фигуры на ТОЙ ЖЕ СТРОКЕ (горизонталь)
        int dragonRow = dragonPos.x;
        
        for (int c = 0; c < BOARD_SIZE; c++)
        {
            if (c == dragonCol) continue;  // Не сам Дракон
            
            int piece = board[dragonRow, c];  // ✅ ТОЛЬКО горизонталь!
            if (piece == EMPTY) continue;
            
            // ✅ Цвет проверка
            if ((forWhite && piece <= 0) || (!forWhite && piece >= 0)) continue;

            int type = PieceType(piece);
            bool canCastle = false;
            foreach (int ct in castleTypes)
            {
                if (type == ct) { canCastle = true; break; }
            }
            if (!canCastle) continue;

            // ✅ Проверяем РЯДОМ (соседние колонки)
            if (Mathf.Abs(c - dragonCol) == 1)  // ✅ ТОЛЬКО РЯДОМ!
            {
                // ✅ Создаём рокировку: меняем местами
                Move castleMove = new Move();
                castleMove.from = dragonPos;
                castleMove.to = new Vector2Int(dragonRow, c);  // ✅ Дракон идёт к фигуре
                castleMove.piece = board[dragonPos.x, dragonPos.y];
                castleMove.captured = EMPTY;
                castleMove.isCastling = true;
                castleMove.castlePieceFrom = new Vector2Int(dragonRow, c);
                castleMove.castlePieceTo = dragonPos;  // Фигура идёт к Дракону
                castleMoves.Add(castleMove);
            }
        }

        return castleMoves;
    }
 
    // ==================== ТЕЛЕПОРТАЦИЯ ХРАМА ====================
 
    public List<Move> GetTempleTeleportMoves(bool forWhite)
    {
        List<Move> teleportMoves = new List<Move>();
 
        if (forWhite && whiteTempleUsed) return teleportMoves;
        if (!forWhite && blackTempleUsed) return teleportMoves;
 
        int colorSign = forWhite ? 1 : -1;

        // Телепортация доступна ТОЛЬКО для фигуры, стоящей на клетке Храма
        int tr0 = TEMPLE_SQUARE.x;
        int tc0 = TEMPLE_SQUARE.y;
        if (!InBounds(tr0, tc0)) return teleportMoves;

        int piece = board[tr0, tc0];
        if (piece * colorSign <= 0) return teleportMoves;

        // Телепортируем эту фигуру на любую пустую клетку
        for (int tr = 0; tr < BOARD_SIZE; tr++)
        {
            for (int tc = 0; tc < BOARD_SIZE; tc++)
            {
                if (board[tr, tc] == EMPTY && (tr != tr0 || tc != tc0))
                {
                    Move teleport = new Move(tr0, tc0, tr, tc, piece, EMPTY);
                    teleport.isTeleport = true;
                    teleportMoves.Add(teleport);
                }
            }
        }
 
        return teleportMoves;
    }

    public bool CanUseTemple(bool forWhite)
    {
        if (forWhite && whiteTempleUsed) return false;
        if (!forWhite && blackTempleUsed) return false;
        if (!InBounds(TEMPLE_SQUARE.x, TEMPLE_SQUARE.y)) return false;
        int piece = board[TEMPLE_SQUARE.x, TEMPLE_SQUARE.y];
        int colorSign = forWhite ? 1 : -1;
        return piece * colorSign > 0;
    }
 
    // ==================== ОЖИВЛЕНИЕ КОЗЛА ====================
 
    public bool CheckGoatRevive(Move lastMove, bool forWhite)
    {
        if (lastMove.captured == EMPTY) return false;
        int capturedType = PieceType(lastMove.captured);
        if (capturedType == DRAGON || capturedType == TIGER) return false;
        if (revivedPieces.Contains(PosKey(lastMove.to.x, lastMove.to.y))) return false;
 
        int goatVal = forWhite ? GOAT : -GOAT;
        int[,] dirs = { {1,0},{-1,0},{0,1},{0,-1},{1,1},{1,-1},{-1,1},{-1,-1} };
 
        for (int d = 0; d < dirs.GetLength(0); d++)
        {
            int gr = lastMove.to.x + dirs[d, 0];
            int gc = lastMove.to.y + dirs[d, 1];
            if (InBounds(gr, gc) && board[gr, gc] == goatVal)
            {
                pendingRevive = true;
                pendingRevivePiece = lastMove.captured;
                pendingReviveGoatPos = new Vector2Int(gr, gc);
                pendingReviveCapturePos = lastMove.to;
                return true;
            }
        }
        return false;
    }
 
    public List<Vector2Int> GetRevivePositions()
    {
        List<Vector2Int> positions = new List<Vector2Int>();
        if (!pendingRevive) return positions;
 
        int[,] dirs = { {1,0},{-1,0},{0,1},{0,-1},{1,1},{1,-1},{-1,1},{-1,-1} };
        for (int d = 0; d < dirs.GetLength(0); d++)
        {
            int nr = pendingReviveGoatPos.x + dirs[d, 0];
            int nc = pendingReviveGoatPos.y + dirs[d, 1];
            if (InBounds(nr, nc) && board[nr, nc] == EMPTY)
                positions.Add(new Vector2Int(nr, nc));
        }
        return positions;
    }
 
    // ==================== ВЫПОЛНЕНИЕ И ОТКАТ ХОДА ====================
 
    public void MakeMove(Move move, bool silent = false)
    {
        MoveRecord record = new MoveRecord();
        record.move = move;
        record.silent = silent;
        record.prevWhiteTurn = whiteTurn;
        record.prevMovedAtFrom = movedPieces.Contains(PosKey(move.from.x, move.from.y));
        record.prevMovedAtTo = movedPieces.Contains(PosKey(move.to.x, move.to.y));
        record.prevWhiteDragonMoved = whiteDragonMoved;
        record.prevBlackDragonMoved = blackDragonMoved;
        record.prevWhiteCastleUsed = whiteCastleUsed;
        record.prevBlackCastleUsed = blackCastleUsed;
        record.prevWhiteTempleUsed = whiteTempleUsed;
        record.prevBlackTempleUsed = blackTempleUsed;
        record.prevWhiteDragonAlive = whiteDragonAlive;
        record.prevBlackDragonAlive = blackDragonAlive;
        record.prevWhiteTigerAlive = whiteTigerAlive;
        record.prevBlackTigerAlive = blackTigerAlive;
        record.prevWhiteDragonPos = whiteDragonPos;
        record.prevBlackDragonPos = blackDragonPos;
        record.prevWhiteTigerPos = whiteTigerPos;
        record.prevBlackTigerPos = blackTigerPos;
        record.prevWhiteLastMoveDest = whiteLastMoveDest;
        record.prevWhiteConsecutiveCount = whiteConsecutiveCount;
        record.prevBlackLastMoveDest = blackLastMoveDest;
        record.prevBlackConsecutiveCount = blackConsecutiveCount;
 
        bool isWhite = IsWhite(move.piece);
        int type = PieceType(move.piece);
 
        // Обработка захвата — обновление состояния королевских фигур
        if (move.captured != EMPTY)
        {
            int capturedType = PieceType(move.captured);
            bool capturedIsWhite = IsWhite(move.captured);
 
            if (capturedType == DRAGON)
            {
                if (capturedIsWhite) whiteDragonAlive = false;
                else blackDragonAlive = false;
            }
            else if (capturedType == TIGER)
            {
                if (capturedIsWhite) whiteTigerAlive = false;
                else blackTigerAlive = false;
            }
        }
 
        if (move.isCastling)
        {
            // Рокировка: Дракон и фигура меняются местами
            int castlePiece = board[move.castlePieceFrom.x, move.castlePieceFrom.y];
            board[move.to.x, move.to.y] = move.piece;
            board[move.from.x, move.from.y] = EMPTY;
            board[move.castlePieceTo.x, move.castlePieceTo.y] = castlePiece;
            board[move.castlePieceFrom.x, move.castlePieceFrom.y] = EMPTY;
 
            if (isWhite)
            {
                whiteCastleUsed = true;
                whiteDragonPos = move.to;
                whiteDragonMoved = true;
                if (PieceType(castlePiece) == TIGER) whiteTigerPos = move.castlePieceTo;
            }
            else
            {
                blackCastleUsed = true;
                blackDragonPos = move.to;
                blackDragonMoved = true;
                if (PieceType(castlePiece) == TIGER) blackTigerPos = move.castlePieceTo;
            }
        }
        else if (move.isRevive)
        {
            board[move.to.x, move.to.y] = move.revivedPiece;
            revivedPieces.Add(PosKey(move.to.x, move.to.y));
        }
        else
        {
            // Обычный ход
            board[move.to.x, move.to.y] = move.piece;
            board[move.from.x, move.from.y] = EMPTY;
 
            if (move.isTeleport)
            {
                if (isWhite) whiteTempleUsed = true;
                else blackTempleUsed = true;
            }
        }
 
        // Обновление позиций королевских фигур
        if (type == DRAGON && !move.isRevive)
        {
            if (isWhite) { whiteDragonPos = move.to; whiteDragonMoved = true; }
            else { blackDragonPos = move.to; blackDragonMoved = true; }
        }
        else if (type == TIGER && !move.isRevive)
        {
            if (isWhite) whiteTigerPos = move.to;
            else blackTigerPos = move.to;
        }
 
        // Отметка «фигура ходила» (для первого хода Кролика/Петуха)
        // Отслеживаем по ТЕКУЩЕЙ позиции фигуры: убираем из старой клетки,
        // добавляем в новую (для рокировки достаточно отметить Дракона).
        if (!silent)
        {
            movedPieces.Remove(PosKey(move.from.x, move.from.y));
            if (!move.isRevive)
                movedPieces.Add(PosKey(move.to.x, move.to.y));
 
            // Обновление последовательных ходов
            if (isWhite)
            {
                if (whiteLastMoveDest.x == move.from.x && whiteLastMoveDest.y == move.from.y)
                    whiteConsecutiveCount++;
                else
                    whiteConsecutiveCount = 1;
                whiteLastMoveDest = move.to;
            }
            else
            {
                if (blackLastMoveDest.x == move.from.x && blackLastMoveDest.y == move.from.y)
                    blackConsecutiveCount++;
                else
                    blackConsecutiveCount = 1;
                blackLastMoveDest = move.to;
            }
        }
 
        moveHistory.Add(record);
 
        if (!silent)
            whiteTurn = !whiteTurn;
    }
 
    public void UndoLastMove()
    {
        if (moveHistory.Count == 0) return;
 
        MoveRecord record = moveHistory[moveHistory.Count - 1];
        moveHistory.RemoveAt(moveHistory.Count - 1);
        Move move = record.move;
 
        if (move.isCastling)
        {
            int castlePiece = board[record.move.castlePieceTo.x, record.move.castlePieceTo.y];
            board[move.from.x, move.from.y] = move.piece;
            board[move.to.x, move.to.y] = EMPTY;
            board[move.castlePieceFrom.x, move.castlePieceFrom.y] = castlePiece;
            board[move.castlePieceTo.x, move.castlePieceTo.y] = EMPTY;
        }
        else if (move.isRevive)
        {
            board[move.to.x, move.to.y] = EMPTY;
            revivedPieces.Remove(PosKey(move.to.x, move.to.y));
        }
        else
        {
            board[move.from.x, move.from.y] = move.piece;
            board[move.to.x, move.to.y] = move.captured;
        }
 
        whiteDragonMoved = record.prevWhiteDragonMoved;
        blackDragonMoved = record.prevBlackDragonMoved;
        whiteCastleUsed = record.prevWhiteCastleUsed;
        blackCastleUsed = record.prevBlackCastleUsed;
        whiteTempleUsed = record.prevWhiteTempleUsed;
        blackTempleUsed = record.prevBlackTempleUsed;
        whiteDragonAlive = record.prevWhiteDragonAlive;
        blackDragonAlive = record.prevBlackDragonAlive;
        whiteTigerAlive = record.prevWhiteTigerAlive;
        blackTigerAlive = record.prevBlackTigerAlive;
        whiteDragonPos = record.prevWhiteDragonPos;
        blackDragonPos = record.prevBlackDragonPos;
        whiteTigerPos = record.prevWhiteTigerPos;
        blackTigerPos = record.prevBlackTigerPos;
        whiteLastMoveDest = record.prevWhiteLastMoveDest;
        whiteConsecutiveCount = record.prevWhiteConsecutiveCount;
        blackLastMoveDest = record.prevBlackLastMoveDest;
        blackConsecutiveCount = record.prevBlackConsecutiveCount;

        // Восстанавливаем «фигура ходила»
        if (record.prevMovedAtFrom)
            movedPieces.Add(PosKey(move.from.x, move.from.y));
        else
            movedPieces.Remove(PosKey(move.from.x, move.from.y));

        if (record.prevMovedAtTo)
            movedPieces.Add(PosKey(move.to.x, move.to.y));
        else
            movedPieces.Remove(PosKey(move.to.x, move.to.y));

        // Восстанавливаем чей ход
        whiteTurn = record.prevWhiteTurn;
    }
 
    // ==================== ОЦЕНКА ПОЗИЦИИ ====================
 
    public int Evaluate()
    {
        int score = 0;
 
        for (int r = 0; r < BOARD_SIZE; r++)
        {
            for (int c = 0; c < BOARD_SIZE; c++)
            {
                int piece = board[r, c];
                if (piece == EMPTY) continue;
 
                int type = PieceType(piece);
                int value = PieceValues.ContainsKey(type) ? PieceValues[type] : 0;
                int sign = IsWhite(piece) ? 1 : -1;
 
                score += sign * value;
 
                // Позиционный бонус — центр доски
                float centerDist = Mathf.Abs(r - 4.5f) + Mathf.Abs(c - 4.5f);
                score += sign * Mathf.RoundToInt((9 - centerDist) * 0.5f);
 
                // Бонус за продвижение пешек
                if (type == ROOSTER || type == RABBIT)
                {
                    int advancement = IsWhite(piece) ? r : (BOARD_SIZE - 1 - r);
                    score += sign * advancement * 2;
                }
            }
        }
 
        // Штрафы за потерю королевских фигур
        if (!whiteDragonAlive) score -= 3000;
        if (!whiteTigerAlive) score -= 3000;
        if (!blackDragonAlive) score += 3000;
        if (!blackTigerAlive) score += 3000;
 
        // Бонус за Kal
        if (IsInCheck(true)) score -= 50;
        if (IsInCheck(false)) score += 50;
 
        return score;
    }
 
    // ==================== КЛОНИРОВАНИЕ ====================
 
    public CalessEngine Clone()
    {
        CalessEngine clone = new CalessEngine();
        clone.board = (int[,])board.Clone();
        clone.whiteTurn = whiteTurn;
        clone.whiteDragonPos = whiteDragonPos;
        clone.blackDragonPos = blackDragonPos;
        clone.whiteTigerPos = whiteTigerPos;
        clone.blackTigerPos = blackTigerPos;
        clone.whiteDragonAlive = whiteDragonAlive;
        clone.blackDragonAlive = blackDragonAlive;
        clone.whiteTigerAlive = whiteTigerAlive;
        clone.blackTigerAlive = blackTigerAlive;
        clone.whiteDragonMoved = whiteDragonMoved;
        clone.blackDragonMoved = blackDragonMoved;
        clone.whiteCastleUsed = whiteCastleUsed;
        clone.blackCastleUsed = blackCastleUsed;
        clone.whiteTempleUsed = whiteTempleUsed;
        clone.blackTempleUsed = blackTempleUsed;
        clone.movedPieces = new HashSet<long>(movedPieces);
        clone.revivedPieces = new HashSet<long>(revivedPieces);
        clone.whiteLastMoveDest = whiteLastMoveDest;
        clone.whiteConsecutiveCount = whiteConsecutiveCount;
        clone.blackLastMoveDest = blackLastMoveDest;
        clone.blackConsecutiveCount = blackConsecutiveCount;
        return clone;
    }
}