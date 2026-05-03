using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Главный менеджер игры Caless.
/// Управляет всеми режимами: vs AI, локальный, Bluetooth, обучение.
/// Координирует движок, рендеринг, UI и ввод.
/// </summary>
public class GameManager : MonoBehaviour
{
    public enum GameMode { None, VsComputer, LocalMultiplayer, Bluetooth, Training }
    public enum PlayerSide { White, Black }

    [HideInInspector] public CalessEngine engine;
    [HideInInspector] public CalessAI ai;
    [HideInInspector] public BoardRenderer boardRenderer;
    [HideInInspector] public UIManager uiManager;
    [HideInInspector] public SettingsManager settings;
    [HideInInspector] public TrainingManager trainingManager;
    [HideInInspector] public BluetoothManager bluetoothManager;

    public GameMode currentMode = GameMode.None;
    public bool isPlayerTurn = true;
    public int moveCount = 0;
    public List<int> playerCapturedPieces = new List<int>();
    public List<int> aiCapturedPieces = new List<int>();
    public Sprite[] pieceSprites;

    // ==================== ТАЙМЕР ====================
    public float whiteTimeLeft = 600f;
    public float blackTimeLeft = 600f;
    public bool timerEnabled = true;

    private PlayerSide playerSide = PlayerSide.White;
    private CalessAI.Difficulty aiDifficulty = CalessAI.Difficulty.Easy;
    private Camera mainCam;

    private Vector2Int selectedSquare = new Vector2Int(-1, -1);
    private List<Move> selectedMoves = new List<Move>();
    private bool gameActive = false;

    // Специальные режимы
    private bool templeMode = false;
    private Vector2Int templePiece = new Vector2Int(-1, -1);
    private bool castlingMode = false;
    private bool pendingRevive = false;
    private Move lastMoveForRevive;

    void Start()
    {
        Application.targetFrameRate = 60;
        Screen.orientation = ScreenOrientation.Portrait;

        InitializeComponents();
        uiManager.ShowMainMenu();

        PieceSpritesHolder spriteHolder = gameObject.AddComponent<PieceSpritesHolder>();
        if (pieceSprites != null && pieceSprites.Length > 0)
            PieceSpritesHolder.SetSprites(pieceSprites);
    }

    private void InitializeComponents()
    {
        settings = gameObject.AddComponent<SettingsManager>();
        settings.Load();

        boardRenderer = gameObject.AddComponent<BoardRenderer>();
        boardRenderer.SetSettings(settings);

        // Если UIManager уже есть в сцене (с назначенными спрайтами) — используем его
        uiManager = GetComponent<UIManager>();
        if (uiManager == null)
            uiManager = FindObjectOfType<UIManager>();
        if (uiManager == null)
            uiManager = gameObject.AddComponent<UIManager>();
        uiManager.gameManager = this;

        trainingManager = gameObject.AddComponent<TrainingManager>();
        trainingManager.gameManager = this;

        bluetoothManager = gameObject.AddComponent<BluetoothManager>();
        bluetoothManager.gameManager = this;

        mainCam = Camera.main;
        if (mainCam == null)
        {
            GameObject camObj = new GameObject("MainCamera");
            camObj.tag = "MainCamera";
            mainCam = camObj.AddComponent<Camera>();
            mainCam.orthographic = true;
            mainCam.orthographicSize = 5.5f;
            mainCam.backgroundColor = new Color(0.06f, 0.05f, 0.04f);
            mainCam.transform.position = new Vector3(0, 0f, -10);
        }
    }

    // ==================== ЗАПУСК ИГР ====================

    public void StartGame(CalessAI.Difficulty difficulty, PlayerSide side)
    {
        currentMode = GameMode.VsComputer;
        aiDifficulty = difficulty;
        playerSide = side;

        engine = new CalessEngine();
        engine.InitializeBoard();

        ai = new CalessAI();
        ai.difficulty = difficulty;

        ResetGameState();
        InitTimers();

        boardRenderer.isBoardFlipped = (side == PlayerSide.Black);
        boardRenderer.Initialize(engine);

        isPlayerTurn = (side == PlayerSide.White);
        gameActive = true;

        uiManager.ShowGameUI();

        if (!isPlayerTurn)
            StartCoroutine(AITurn());
    }

    public void StartLocalMultiplayer()
    {
        currentMode = GameMode.LocalMultiplayer;

        engine = new CalessEngine();
        engine.InitializeBoard();

        ResetGameState();
        InitTimers();

        boardRenderer.isBoardFlipped = false;
        boardRenderer.Initialize(engine);

        isPlayerTurn = true;
        gameActive = true;

        uiManager.ShowGameUI();
    }

    public void StartBluetoothGame()
    {
        currentMode = GameMode.Bluetooth;
        uiManager.ShowBluetoothUI(OnBluetoothHost, OnBluetoothJoin, () => ShowMenu());
    }

    public void StartTraining()
    {
        currentMode = GameMode.Training;
        trainingManager.StartTraining();
    }

    public void ShowMenu()
    {
        gameActive = false;
        currentMode = GameMode.None;
        StopAllCoroutines();
        uiManager.ShowMainMenu();
    }

    public void RestartGame()
    {
        if (currentMode == GameMode.VsComputer)
            StartGame(aiDifficulty, playerSide);
        else if (currentMode == GameMode.LocalMultiplayer)
            StartLocalMultiplayer();
    }

    // ==================== ТАЙМЕР ====================

    private void InitTimers()
    {
        float time = (settings != null) ? settings.GetMoveTimeMinutes() * 60f : 600f;
        whiteTimeLeft = time;
        blackTimeLeft = time;
        timerEnabled = true;
    }

    private void UpdateTimer()
    {
        if (!gameActive || !timerEnabled || engine == null) return;

        float delta = Time.deltaTime;

        if (engine.whiteTurn)
            whiteTimeLeft = Mathf.Max(0, whiteTimeLeft - delta);
        else
            blackTimeLeft = Mathf.Max(0, blackTimeLeft - delta);

        uiManager.UpdateTimers(whiteTimeLeft, blackTimeLeft, engine.whiteTurn);

        // Время истекло
        if (engine.whiteTurn && whiteTimeLeft <= 0)
        {
            gameActive = false;
            string winner;
            if (currentMode == GameMode.LocalMultiplayer)
                winner = "Чёрные победили!";
            else
                winner = (playerSide == PlayerSide.Black) ? "Вы выиграли!" : "Вы проиграли!";
            uiManager.ShowGameOver("Время вышло!", winner);
        }
        else if (!engine.whiteTurn && blackTimeLeft <= 0)
        {
            gameActive = false;
            string winner;
            if (currentMode == GameMode.LocalMultiplayer)
                winner = "Белые победили!";
            else
                winner = (playerSide == PlayerSide.White) ? "Вы выиграли!" : "Вы проиграли!";
            uiManager.ShowGameOver("Время вышло!", winner);
        }
    }

    // ==================== ХОД НАЗАД ====================

    public void UndoMove()
    {
        if (!gameActive) return;
        if (engine == null || engine.moveHistory.Count == 0) return;
        if (currentMode == GameMode.Bluetooth) return; // В Bluetooth нельзя

        StopAllCoroutines();

        if (currentMode == GameMode.VsComputer)
        {
            // Откатываем ход ИИ + ход игрока
            if (engine.moveHistory.Count >= 2)
            {
                Move lastMove = engine.moveHistory[engine.moveHistory.Count - 1].move;
                engine.UndoLastMove();
                if (lastMove.captured != CalessEngine.EMPTY && aiCapturedPieces.Count > 0)
                    aiCapturedPieces.RemoveAt(aiCapturedPieces.Count - 1);

                lastMove = engine.moveHistory[engine.moveHistory.Count - 1].move;
                engine.UndoLastMove();
                if (lastMove.captured != CalessEngine.EMPTY && playerCapturedPieces.Count > 0)
                    playerCapturedPieces.RemoveAt(playerCapturedPieces.Count - 1);

                moveCount = Mathf.Max(0, moveCount - 2);
                isPlayerTurn = true;
            }
        }
        else if (currentMode == GameMode.LocalMultiplayer)
        {
            if (engine.moveHistory.Count >= 1)
            {
                Move lastMove = engine.moveHistory[engine.moveHistory.Count - 1].move;
                engine.UndoLastMove();
                if (lastMove.captured != CalessEngine.EMPTY)
                {
                    bool wasWhiteTurn = !engine.whiteTurn; // restored
                    if (wasWhiteTurn && playerCapturedPieces.Count > 0)
                        playerCapturedPieces.RemoveAt(playerCapturedPieces.Count - 1);
                    else if (!wasWhiteTurn && aiCapturedPieces.Count > 0)
                        aiCapturedPieces.RemoveAt(aiCapturedPieces.Count - 1);
                }
                moveCount = Mathf.Max(0, moveCount - 1);
            }
        }

        boardRenderer.RefreshPieces();
        boardRenderer.ClearHighlights();
        selectedSquare = new Vector2Int(-1, -1);
        selectedMoves.Clear();
        castlingMode = false;
        pendingRevive = false;

        uiManager.UpdateGameInfo();
        uiManager.RefreshMoveHistory(engine.moveHistory);
    }

    // ==================== ПРИМЕНИТЬ ТЕМУ ДОСКИ ====================

    public void ApplyBoardTheme()
    {
        if (boardRenderer != null && engine != null && gameActive)
        {
            boardRenderer.SetSettings(settings);
            boardRenderer.Initialize(engine);
        }
    }

    private void ResetGameState()
    {
        moveCount = 0;
        playerCapturedPieces.Clear();
        aiCapturedPieces.Clear();
        selectedSquare = new Vector2Int(-1, -1);
        selectedMoves.Clear();
        templeMode = false;
        castlingMode = false;
        pendingRevive = false;
    }

    // ==================== ВВОД ====================

    void Update()
    {
        if (gameActive)
            UpdateTimer();

        if (!gameActive) return;

        if (currentMode == GameMode.Training)
        {
            trainingManager.HandleInput();
            return;
        }

        if (!isPlayerTurn && currentMode != GameMode.LocalMultiplayer) return;
        if (boardRenderer.IsAnimating) return;

        HandleGameInput();
    }

    private void HandleGameInput()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (mainCam == null)
        {
            mainCam = Camera.main;
            if (mainCam == null) return;
        }

        Vector3 mouseWorld = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;
        Vector2Int boardPos = boardRenderer.GetBoardPosition(mouseWorld);

        if (!CalessEngine.InBounds(boardPos.x, boardPos.y)) return;

        if (templeMode)
        {
            HandleTempleInput(boardPos);
            return;
        }

        if (selectedSquare.x >= 0)
        {
            HandleMoveInput(boardPos);
            return;
        }

        HandlePieceSelection(boardPos);
    }

    private void HandlePieceSelection(Vector2Int pos)
    {
        int piece = engine.board[pos.x, pos.y];
        if (piece == CalessEngine.EMPTY) return;

        bool pieceIsWhite = CalessEngine.IsWhite(piece);
        if (currentMode == GameMode.LocalMultiplayer)
        {
            if (pieceIsWhite != engine.whiteTurn) return;
        }
        else
        {
            bool playerIsWhite = (playerSide == PlayerSide.White);
            if (pieceIsWhite != playerIsWhite) return;
        }

        SelectPiece(pos);
    }

    private void SelectPiece(Vector2Int pos)
    {
        selectedSquare = pos;
        selectedMoves = engine.GetLegalMovesForPiece(pos.x, pos.y);

        if (castlingMode)
        {
            int type = CalessEngine.PieceType(engine.board[pos.x, pos.y]);
            if (type == CalessEngine.DRAGON)
            {
                List<Move> castleMoves = engine.GetCastlingMoves(engine.whiteTurn);
                selectedMoves.AddRange(castleMoves);
            }
        }

        boardRenderer.ClearHighlights();
        boardRenderer.ShowSelectedHighlight(pos.x, pos.y);

        foreach (Move m in selectedMoves)
        {
            if (m.isCastling)
                boardRenderer.ShowCastleHint(m.to.x, m.to.y);
            else
            {
                bool isCapture = m.captured != CalessEngine.EMPTY || m.isDragonRanged;
                boardRenderer.ShowMoveHint(m.to.x, m.to.y, isCapture);
            }
        }

        if (engine.IsInCheck(engine.whiteTurn))
        {
            Vector2Int royalPos = GetVulnerableRoyalPos(engine.whiteTurn);
            if (royalPos.x >= 0)
                boardRenderer.ShowKalHighlight(royalPos.x, royalPos.y);
        }
    }

    private void HandleMoveInput(Vector2Int targetPos)
    {
        Move? targetMove = null;
        foreach (Move m in selectedMoves)
        {
            if (m.to == targetPos)
            {
                targetMove = m;
                break;
            }
        }

        if (targetMove.HasValue)
        {
            ExecutePlayerMove(targetMove.Value);
            return;
        }

        int clicked = engine.board[targetPos.x, targetPos.y];
        if (clicked != CalessEngine.EMPTY)
        {
            bool clickedIsWhite = CalessEngine.IsWhite(clicked);
            bool turnIsWhite = (currentMode == GameMode.LocalMultiplayer) ?
                engine.whiteTurn : (playerSide == PlayerSide.White);

            if (clickedIsWhite == turnIsWhite)
            {
                SelectPiece(targetPos);
                return;
            }
        }
        DeselectPiece();
    }

    private void DeselectPiece()
    {
        selectedSquare = new Vector2Int(-1, -1);
        selectedMoves.Clear();
        castlingMode = false;
        boardRenderer.ClearHighlights();
    }

    // ==================== ВЫПОЛНЕНИЕ ХОДА ====================

    private void ExecutePlayerMove(Move move)
    {
        boardRenderer.ClearHighlights();

        if (move.captured != CalessEngine.EMPTY)
        {
            if (currentMode == GameManager.GameMode.LocalMultiplayer)
            {
                if (engine.whiteTurn)
                    playerCapturedPieces.Add(move.captured);
                else
                    aiCapturedPieces.Add(move.captured);
            }
            else
            {
                playerCapturedPieces.Add(move.captured);
            }
        }

        engine.MakeMove(move);
        moveCount++;

        boardRenderer.SetLastMove(move.from, move.to);
        selectedSquare = new Vector2Int(-1, -1);
        selectedMoves.Clear();
        castlingMode = false;

        bool animate = (settings != null && settings.AnimationsEnabled);

        if (animate)
        {
            if (move.isCastling)
            {
                boardRenderer.AnimateCastling(move, () => {
                    boardRenderer.RefreshPieces();
                    OnMoveComplete(move);
                });
            }
            else
            {
                boardRenderer.AnimateMove(move, () => {
                    boardRenderer.RefreshPieces();
                    OnMoveComplete(move);
                });
            }
        }
        else
        {
            boardRenderer.RefreshPieces();
            OnMoveComplete(move);
        }

        uiManager.RefreshMoveHistory(engine.moveHistory);
    }

    private void OnMoveComplete(Move move)
    {
        if (TryHandleGoatRevive(move, fromAI: false, onContinue: () => ContinueAfterMove(move)))
            return;

        ContinueAfterMove(move);
    }

    private void ContinueAfterMove(Move move)
    {
        if (CheckGameOver()) return;

        if (currentMode == GameManager.GameMode.Bluetooth && bluetoothManager.IsConnected)
            bluetoothManager.SendMove(move);

        if (currentMode == GameManager.GameMode.VsComputer)
        {
            isPlayerTurn = false;
            uiManager.UpdateGameInfo();
            StartCoroutine(AITurn());
        }
        else if (currentMode == GameManager.GameMode.LocalMultiplayer)
        {
            uiManager.UpdateGameInfo();
        }
    }

    // ==================== AI ====================

    private IEnumerator AITurn()
    {
        uiManager.ShowThinking(true);
        yield return new WaitForSeconds(0.3f);

        Move aiMove = ai.GetBestMove(engine);

        uiManager.ShowThinking(false);

        if (aiMove.captured != CalessEngine.EMPTY)
            aiCapturedPieces.Add(aiMove.captured);

        engine.MakeMove(aiMove);
        moveCount++;

        boardRenderer.SetLastMove(aiMove.from, aiMove.to);

        bool animate = (settings != null && settings.AnimationsEnabled);
        if (animate)
        {
            if (aiMove.isCastling)
                boardRenderer.AnimateCastling(aiMove, () => OnAIMoveComplete());
            else
                boardRenderer.AnimateMove(aiMove, () => OnAIMoveComplete());
        }
        else
        {
            boardRenderer.RefreshPieces();
            OnAIMoveComplete();
        }

        uiManager.RefreshMoveHistory(engine.moveHistory);
    }

    private void OnAIMoveComplete()
    {
        boardRenderer.RefreshPieces();

        Move lastAiMove = (engine.moveHistory.Count > 0)
            ? engine.moveHistory[engine.moveHistory.Count - 1].move
            : default;

        if (TryHandleGoatRevive(lastAiMove, fromAI: true, onContinue: () => {
                if (CheckGameOver()) return;
                isPlayerTurn = true;
                uiManager.UpdateGameInfo();
            }))
            return;

        if (CheckGameOver()) return;

        isPlayerTurn = true;
        uiManager.UpdateGameInfo();
    }

    // ==================== ПРОВЕРКА КОНЦА ИГРЫ ====================

    private bool CheckGameOver()
    {
        // Обе королевские фигуры белых уничтожены
        if (!engine.whiteDragonAlive && !engine.whiteTigerAlive)
        {
            gameActive = false;
            string winner;
            if (currentMode == GameMode.LocalMultiplayer)
                winner = "Чёрные победили!";
            else
                winner = (playerSide == PlayerSide.Black) ? "Вы выиграли!" : "Вы проиграли!";
            uiManager.ShowGameOver("РАС!", winner);
            return true;
        }

        // Обе королевские фигуры чёрных уничтожены
        if (!engine.blackDragonAlive && !engine.blackTigerAlive)
        {
            gameActive = false;
            string winner;
            if (currentMode == GameMode.LocalMultiplayer)
                winner = "Белые победили!";
            else
                winner = (playerSide == PlayerSide.White) ? "Вы выиграли!" : "Вы проиграли!";
            uiManager.ShowGameOver("РАС!", winner);
            return true;
        }

        // Кал (шах)
        bool currentInCheck = engine.IsInCheck(engine.whiteTurn);
        if (currentInCheck)
        {
            List<Move> legalMoves = engine.GetAllLegalMoves(engine.whiteTurn);
            if (legalMoves.Count == 0)
            {
                gameActive = false;
                string winner;
                if (currentMode == GameMode.LocalMultiplayer)
                    winner = (engine.whiteTurn ? "Чёрные" : "Белые") + " победили!";
                else
                {
                    // engine.whiteTurn — сторона которой мат. Сравниваем с playerSide.
                    bool playerIsWhite = (playerSide == PlayerSide.White);
                    winner = (engine.whiteTurn == playerIsWhite) ? "Вы проиграли!" : "Вы выиграли!";
                }
                uiManager.ShowGameOver("РАС!", winner);
                return true;
            }
        }
        else
        {
            List<Move> legalMoves = engine.GetAllLegalMoves(engine.whiteTurn);
            if (legalMoves.Count == 0)
            {
                gameActive = false;
                uiManager.ShowGameOver("Пат", "Ничья — нет доступных ходов");
                return true;
            }
        }
        return false;
    }

    // ==================== СПЕЦСПОСОБНОСТИ ====================

    public void ActivateTemple()
    {
        if (!gameActive) return;
        if (engine == null) return;

        bool isWhite = (currentMode == GameMode.LocalMultiplayer) ?
            engine.whiteTurn : (playerSide == PlayerSide.White);

        if (!engine.CanUseTemple(isWhite))
        {
            uiManager.ShowDialog("Храм", "Телепортация недоступна!", "OK", null);
            return;
        }

        List<Move> teleportMoves = engine.GetTempleTeleportMoves(isWhite);
        if (teleportMoves.Count == 0)
        {
            uiManager.ShowDialog("Храм", "Нет фигуры на клетке Храма!", "OK", null);
            return;
        }

        templeMode = true;
        boardRenderer.ClearHighlights();

        Vector2Int templeSquare = CalessEngine.TEMPLE_SQUARE;
        boardRenderer.ShowSelectedHighlight(templeSquare.x, templeSquare.y);

        foreach (Move m in teleportMoves)
            boardRenderer.ShowTeleportHint(m.to.x, m.to.y);

        selectedSquare = templeSquare;
        selectedMoves = teleportMoves;
    }

    private void HandleTempleInput(Vector2Int boardPos)
    {
        Move? targetMove = null;
        foreach (Move m in selectedMoves)
        {
            if (m.to == boardPos) { targetMove = m; break; }
        }

        if (targetMove.HasValue)
        {
            templeMode = false;
            ExecutePlayerMove(targetMove.Value);
        }
        else
        {
            templeMode = false;
            DeselectPiece();
        }
    }

    public void ActivateCastling()
    {
        if (!gameActive) return;
        if (engine == null) return;

        bool isWhite = (currentMode == GameMode.LocalMultiplayer) ?
            engine.whiteTurn : (playerSide == PlayerSide.White);

        List<Move> castleMoves = engine.GetCastlingMoves(isWhite);
        if (castleMoves.Count == 0)
        {
            uiManager.ShowDialog("Сдвиг", "Рокировка недоступна!", "OK", null);
            return;
        }

        castlingMode = true;
        boardRenderer.ClearHighlights();

        Vector2Int dragonPos = isWhite ? engine.whiteDragonPos : engine.blackDragonPos;
        boardRenderer.ShowSelectedHighlight(dragonPos.x, dragonPos.y);

        foreach (Move m in castleMoves)
            boardRenderer.ShowCastleHint(m.to.x, m.to.y);

        selectedSquare = dragonPos;
        selectedMoves = castleMoves;
    }

    // ==================== КОЗЁЛ ====================

    private bool TryHandleGoatRevive(Move move, bool fromAI, System.Action onContinue)
    {
        if (move.piece == CalessEngine.EMPTY) return false;
        if (move.captured == CalessEngine.EMPTY) return false;
        int capType = CalessEngine.PieceType(move.captured);
        if (capType == CalessEngine.DRAGON || capType == CalessEngine.TIGER) return false;

        bool moverIsWhite = !engine.whiteTurn;

        if (!engine.CheckGoatRevive(move, moverIsWhite))
            return false;

        List<Vector2Int> revivePositions = engine.GetRevivePositions();
        if (revivePositions.Count == 0)
        {
            engine.pendingRevive = false;
            return false;
        }

        if (fromAI)
        {
            ApplyRevive(revivePositions[0]);
            onContinue?.Invoke();
            return true;
        }

        string pieceName = CalessEngine.PieceNames.ContainsKey(capType)
            ? CalessEngine.PieceNames[capType] : "фигура";

        uiManager.ShowReviveDialog(
            pieceName,
            onRevive: () => {
                ApplyRevive(revivePositions[0]);
                boardRenderer.RefreshPieces();
                onContinue?.Invoke();
            },
            onSkip: () => {
                engine.pendingRevive = false;
                onContinue?.Invoke();
            });

        return true;
    }

    private void ApplyRevive(Vector2Int pos)
    {
        if (!engine.pendingRevive) return;

        Move reviveMove = new Move();
        reviveMove.from = pos;
        reviveMove.to = pos;
        reviveMove.piece = engine.pendingRevivePiece;
        reviveMove.captured = CalessEngine.EMPTY;
        reviveMove.isRevive = true;
        reviveMove.revivedPiece = engine.pendingRevivePiece;

        engine.MakeMove(reviveMove, silent: true);
        engine.pendingRevive = false;
    }

    // ==================== BLUETOOTH ====================

    private void OnBluetoothHost()
    {
        bluetoothManager.Initialize(OnBTMoveReceived, OnBTChatReceived, OnBTConnected, OnBTDisconnected);
        bluetoothManager.StartHost();
        uiManager.ShowBluetoothWaiting(bluetoothManager, true, () => {
            bluetoothManager.Disconnect();
            ShowMenu();
        });
        StartCoroutine(PollBluetoothStatus());
    }

    private void OnBluetoothJoin()
    {
        bluetoothManager.Initialize(OnBTMoveReceived, OnBTChatReceived, OnBTConnected, OnBTDisconnected);
        bluetoothManager.StartClient();
        uiManager.ShowBluetoothWaiting(bluetoothManager, false, () => {
            bluetoothManager.Disconnect();
            ShowMenu();
        });
        StartCoroutine(PollBluetoothStatus());
    }

    private IEnumerator PollBluetoothStatus()
    {
        while (!bluetoothManager.IsConnected && currentMode == GameMode.Bluetooth)
        {
            uiManager.UpdateBluetoothStatus(bluetoothManager.StatusMessage);
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void OnBTConnected()
    {
        engine = new CalessEngine();
        engine.InitializeBoard();
        ResetGameState();
        InitTimers();

        playerSide = bluetoothManager.IsHost ? PlayerSide.White : PlayerSide.Black;
        boardRenderer.isBoardFlipped = !bluetoothManager.IsHost;
        boardRenderer.Initialize(engine);

        isPlayerTurn = bluetoothManager.IsHost;
        gameActive = true;

        uiManager.ShowGameUI();
    }

    private void OnBTDisconnected()
    {
        gameActive = false;
        uiManager.ShowDialog("Соединение", "Соединение потеряно", "В меню", () => ShowMenu());
    }

    private void OnBTChatReceived(string message)
    {
        uiManager.ShowDialog("Чат", message, "OK", null);
    }

    private void OnBTMoveReceived(string moveData)
    {
        string[] parts = moveData.Split(',');
        if (parts.Length < 4) return;

        int fr = int.Parse(parts[0]);
        int fc = int.Parse(parts[1]);
        int tr = int.Parse(parts[2]);
        int tc = int.Parse(parts[3]);

        Move move = new Move();
        move.from = new Vector2Int(fr, fc);
        move.to = new Vector2Int(tr, tc);
        move.piece = engine.board[fr, fc];
        move.captured = engine.board[tr, tc];

        if (moveData.Contains("CASTLE"))
        {
            move.isCastling = true;
            int castleIdx = System.Array.IndexOf(parts, "CASTLE");
            if (castleIdx >= 0 && castleIdx + 4 < parts.Length)
            {
                move.castlePieceFrom = new Vector2Int(int.Parse(parts[castleIdx + 1]), int.Parse(parts[castleIdx + 2]));
                move.castlePieceTo = new Vector2Int(int.Parse(parts[castleIdx + 3]), int.Parse(parts[castleIdx + 4]));
            }
        }

        if (moveData.Contains("TELEPORT")) move.isTeleport = true;
        if (moveData.Contains("RANGED")) move.isDragonRanged = true;

        if (move.captured != CalessEngine.EMPTY)
            aiCapturedPieces.Add(move.captured);

        engine.MakeMove(move);
        moveCount++;

        boardRenderer.SetLastMove(move.from, move.to);
        boardRenderer.RefreshPieces();
        uiManager.RefreshMoveHistory(engine.moveHistory);

        if (CheckGameOver()) return;

        isPlayerTurn = true;
        uiManager.UpdateGameInfo();
    }

    // ==================== УТИЛИТЫ ====================

    private Vector2Int GetVulnerableRoyalPos(bool forWhite)
    {
        if (forWhite)
        {
            if (engine.whiteDragonAlive && !engine.whiteTigerAlive)
                return engine.whiteDragonPos;
            if (!engine.whiteDragonAlive && engine.whiteTigerAlive)
                return engine.whiteTigerPos;
        }
        else
        {
            if (engine.blackDragonAlive && !engine.blackTigerAlive)
                return engine.blackDragonPos;
            if (!engine.blackDragonAlive && engine.blackTigerAlive)
                return engine.blackTigerPos;
        }
        return new Vector2Int(-1, -1);
    }
}
