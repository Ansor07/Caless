
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
 
/// <summary>
/// Управляет всем UI: главное меню, подменю, настройки, игровой интерфейс.
/// Весь интерфейс на русском языке. Тёмная фэнтези тема.
/// </summary>
public class UIManager : MonoBehaviour
{
    [HideInInspector] public GameManager gameManager;
 
    private Canvas canvas;
    private CanvasScaler canvasScaler;
 
    // Панели
    private GameObject menuPanel;
    private GameObject playSubMenu;
    private GameObject settingsPanel;
    private GameObject gameUIPanel;
    private GameObject gameOverPanel;
    private GameObject trainingUIPanel;
    private GameObject difficultyPanel;
    private GameObject sidePanel;
    private GameObject dialogPanel;
 
    // Игровые тексты
    private Text turnText;
    private Text moveCountText;
    private Text captureText;
    private Text statusText;
    private Text thinkingText;
 
    private Font cachedFont;
 
    // Цвета темы
    private static readonly Color BG_DARK = new Color(0.08f, 0.06f, 0.05f, 0.97f);
    private static readonly Color BG_PANEL = new Color(0.12f, 0.10f, 0.08f, 0.95f);
    private static readonly Color ACCENT_GOLD = new Color(0.85f, 0.70f, 0.30f);
    private static readonly Color ACCENT_LIGHT = new Color(0.9f, 0.85f, 0.75f);
    private static readonly Color BTN_GREEN = new Color(0.18f, 0.45f, 0.22f);
    private static readonly Color BTN_ORANGE = new Color(0.55f, 0.35f, 0.10f);
    private static readonly Color BTN_BLUE = new Color(0.15f, 0.25f, 0.50f);
    private static readonly Color BTN_RED = new Color(0.50f, 0.15f, 0.15f);
    private static readonly Color BTN_PURPLE = new Color(0.35f, 0.15f, 0.50f);
    private static readonly Color BTN_GRAY = new Color(0.25f, 0.22f, 0.20f);
    private static readonly Color TEXT_DIM = new Color(0.6f, 0.55f, 0.45f);
 
    void Awake()
    {
        LoadFont();
        CreateCanvas();
        EnsureEventSystem();
    }
 
    private void LoadFont()
    {
        cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (cachedFont == null)
            cachedFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (cachedFont == null)
            cachedFont = Font.CreateDynamicFontFromOSFont("Arial", 14);
    }
 
    private void CreateCanvas()
    {
        GameObject canvasObj = new GameObject("UICanvas");
        canvasObj.transform.SetParent(transform);
 
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
 
        canvasScaler = canvasObj.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1080, 1920);
        canvasScaler.matchWidthOrHeight = 0.5f;
 
        canvasObj.AddComponent<GraphicRaycaster>();
    }
 
    private void EnsureEventSystem()
    {
        if (EventSystem.current == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<StandaloneInputModule>();
        }
    }
 
    // ==================== ГЛАВНОЕ МЕНЮ ====================
 
    public void ShowMainMenu()
    {
        HideAll();

        menuPanel = CreateFullPanel("MainMenu");
        Image bg = menuPanel.GetComponent<Image>();
        bg.color = BG_DARK;
 
        // Заголовок
        CreateText("CALESS", menuPanel.transform,
            new Vector2(0.05f, 0.82f), new Vector2(0.95f, 0.94f),
            72, ACCENT_GOLD, TextAnchor.MiddleCenter, FontStyle.Bold);
 
        CreateText("Стратегическая настольная игра", menuPanel.transform,
            new Vector2(0.1f, 0.76f), new Vector2(0.9f, 0.82f),
            28, TEXT_DIM, TextAnchor.MiddleCenter);
 
        // Кнопки меню
        CreateButton("Играть", menuPanel.transform,
            new Vector2(0.15f, 0.60f), new Vector2(0.85f, 0.70f),
            40, BTN_GREEN, Color.white, () => ShowPlaySubMenu());

        CreateButton("Обучение", menuPanel.transform,
            new Vector2(0.15f, 0.48f), new Vector2(0.85f, 0.58f),
            38, BTN_BLUE, Color.white, () => gameManager.StartTraining());
 
        CreateButton("Настройки", menuPanel.transform,
            new Vector2(0.15f, 0.36f), new Vector2(0.85f, 0.46f),
            38, BTN_GRAY, ACCENT_LIGHT, () => ShowSettings());

        // Декоративные иконки
        float iconY = 0.18f;
        float iconStartX = 0.02f;
        float iconStep = 0.082f;
        int[] pieceTypes = {
            CalessEngine.RAT, CalessEngine.OX, CalessEngine.TIGER, CalessEngine.RABBIT,
            CalessEngine.DRAGON, CalessEngine.SNAKE, CalessEngine.HORSE, CalessEngine.GOAT,
            CalessEngine.MONKEY, CalessEngine.ROOSTER, CalessEngine.DOG, CalessEngine.PIG
        };
        for (int i = 0; i < pieceTypes.Length; i++)
        {
            float x = iconStartX + i * iconStep;
            CreatePieceIcon(menuPanel.transform, pieceTypes[i], i % 2 == 0, new Vector2(x + 0.02f, iconY), 60);
        }
 
        // Версия
        CreateText("v1.0", menuPanel.transform,
            new Vector2(0.8f, 0.02f), new Vector2(0.98f, 0.06f),
            20, TEXT_DIM, TextAnchor.MiddleRight);
    }
 
    // ==================== ПОДМЕНЮ «ИГРАТЬ» ====================
 
    public void ShowPlaySubMenu()
    {
        HideAll();
 
        playSubMenu = CreateFullPanel("PlaySubMenu");
        playSubMenu.GetComponent<Image>().color = BG_DARK;
 ;
        CreateText("Режим игры", playSubMenu.transform,
            new Vector2(0.1f, 0.82f), new Vector2(0.9f, 0.92f),
            48, ACCENT_GOLD, TextAnchor.MiddleCenter, FontStyle.Bold);
 
        CreateButton("Против компьютера", playSubMenu.transform,
            new Vector2(0.12f, 0.65f), new Vector2(0.88f, 0.75f),
            36, BTN_GREEN, Color.white, () => ShowDifficultySelect());
 
        CreateButton("2 игрока (локально)", playSubMenu.transform,
            new Vector2(0.12f, 0.52f), new Vector2(0.88f, 0.62f),
            36, BTN_BLUE, Color.white, () => gameManager.StartLocalMultiplayer());

        CreateButton("Bluetooth мультиплеер", playSubMenu.transform,
            new Vector2(0.12f, 0.39f), new Vector2(0.88f, 0.49f),
            34, BTN_PURPLE, Color.white, () => gameManager.StartBluetoothGame());
 
        CreateButton("Назад", playSubMenu.transform,
            new Vector2(0.25f, 0.22f), new Vector2(0.75f, 0.30f),
            32, BTN_GRAY, ACCENT_LIGHT, () => ShowMainMenu());
    }
 
    // ==================== ВЫБОР СЛОЖНОСТИ ====================
 
    private void ShowDifficultySelect()
    {
        HideAll();
 
        difficultyPanel = CreateFullPanel("DifficultySelect");
        difficultyPanel.GetComponent<Image>().color = BG_DARK;
 
        CreateText("Выберите сложность", difficultyPanel.transform,
            new Vector2(0.1f, 0.75f), new Vector2(0.9f, 0.88f),
            44, ACCENT_GOLD, TextAnchor.MiddleCenter, FontStyle.Bold);
 
        CreateButton("Лёгкий", difficultyPanel.transform,
            new Vector2(0.15f, 0.55f), new Vector2(0.85f, 0.65f),
            40, BTN_GREEN, Color.white,
            () => ShowSideSelection(CalessAI.Difficulty.Easy));
 
        CreateButton("Средний", difficultyPanel.transform,
            new Vector2(0.15f, 0.42f), new Vector2(0.85f, 0.52f),
            40, BTN_ORANGE, Color.white,
            () => ShowSideSelection(CalessAI.Difficulty.Medium));
 
        CreateButton("Назад", difficultyPanel.transform,
            new Vector2(0.25f, 0.25f), new Vector2(0.75f, 0.33f),
            32, BTN_GRAY, ACCENT_LIGHT, () => ShowPlaySubMenu());
    }
 
    // ==================== ВЫБОР СТОРОНЫ ====================
 
    private void ShowSideSelection(CalessAI.Difficulty difficulty)
    {
        HideAll();
 
        sidePanel = CreateFullPanel("SideSelect");
        sidePanel.GetComponent<Image>().color = BG_DARK;
 
        string diffName = difficulty == CalessAI.Difficulty.Easy ? "Лёгкий" : "Средний";
        CreateText("Сложность: " + diffName, sidePanel.transform,
            new Vector2(0.1f, 0.78f), new Vector2(0.9f, 0.88f),
            36, TEXT_DIM, TextAnchor.MiddleCenter);
 
        CreateText("Выберите сторону", sidePanel.transform,
            new Vector2(0.1f, 0.68f), new Vector2(0.9f, 0.78f),
            44, ACCENT_GOLD, TextAnchor.MiddleCenter, FontStyle.Bold);
 
        CreateButton("Белые", sidePanel.transform,
            new Vector2(0.15f, 0.50f), new Vector2(0.85f, 0.60f),
            42, new Color(0.75f, 0.72f, 0.65f), new Color(0.1f, 0.1f, 0.1f),
            () => gameManager.StartGame(difficulty, GameManager.PlayerSide.White));
 
        CreateButton("Чёрные", sidePanel.transform,
            new Vector2(0.15f, 0.37f), new Vector2(0.85f, 0.47f),
            42, new Color(0.15f, 0.12f, 0.10f), ACCENT_GOLD,
            () => gameManager.StartGame(difficulty, GameManager.PlayerSide.Black));

        CreateButton("Назад", sidePanel.transform,
            new Vector2(0.25f, 0.22f), new Vector2(0.75f, 0.30f),
            32, BTN_GRAY, ACCENT_LIGHT, () => ShowDifficultySelect());
    }
 
    // ==================== НАСТРОЙКИ ====================
 
    public void ShowSettings()
    {
        HideAll();
 
        settingsPanel = CreateFullPanel("Settings");
        settingsPanel.GetComponent<Image>().color = BG_DARK;
 
        CreateText("Настройки", settingsPanel.transform,
            new Vector2(0.1f, 0.88f), new Vector2(0.9f, 0.96f),
            48, ACCENT_GOLD, TextAnchor.MiddleCenter, FontStyle.Bold);
 
        SettingsManager s = gameManager.settings;
        float y = 0.78f;
        float step = 0.085f;
 
        // Звук
        CreateToggleRow("Звук", s.SoundEnabled, settingsPanel.transform, y, (val) => {
            s.SoundEnabled = val; s.Save();
        });
        y -= step;
 
        // Музыка
        CreateToggleRow("Музыка", s.MusicEnabled, settingsPanel.transform, y, (val) => {
            s.MusicEnabled = val; s.Save();
        });
        y -= step;
 
        // Анимации
        CreateToggleRow("Анимации", s.AnimationsEnabled, settingsPanel.transform, y, (val) => {
            s.AnimationsEnabled = val; s.Save();
        });
        y -= step;
 
        // Тема доски
        CreateText("Тема доски:", settingsPanel.transform,
            new Vector2(0.05f, y), new Vector2(0.45f, y + 0.06f),
            28, ACCENT_LIGHT, TextAnchor.MiddleLeft);
 
        int boardThemeIdx = s.BoardTheme;
        CreateButton(SettingsManager.BoardThemeNames[boardThemeIdx], settingsPanel.transform,
            new Vector2(0.50f, y), new Vector2(0.95f, y + 0.06f),
            26, BTN_BLUE, Color.white, () => {
                s.BoardTheme = (s.BoardTheme + 1) % SettingsManager.BoardThemeNames.Length;
                s.Save();
                ShowSettings();
            });
        y -= step;
 
        // Тема фигур
        CreateText("Тема фигур:", settingsPanel.transform,
            new Vector2(0.05f, y), new Vector2(0.45f, y + 0.06f),
            28, ACCENT_LIGHT, TextAnchor.MiddleLeft);
 
        CreateButton(SettingsManager.PieceThemeNames[s.PieceTheme], settingsPanel.transform,
            new Vector2(0.50f, y), new Vector2(0.95f, y + 0.06f),
            26, BTN_BLUE, Color.white, () => {
                s.PieceTheme = (s.PieceTheme + 1) % SettingsManager.PieceThemeNames.Length;
                s.Save();
                PieceSpriteGenerator.ClearCache();
                ShowSettings();
            });
        y -= step;
 
        // Язык
        CreateText("Язык:", settingsPanel.transform,
            new Vector2(0.05f, y), new Vector2(0.45f, y + 0.06f),
            28, ACCENT_LIGHT, TextAnchor.MiddleLeft);
 
        CreateButton(SettingsManager.LanguageNames[s.Language], settingsPanel.transform,
            new Vector2(0.50f, y), new Vector2(0.95f, y + 0.06f),
            26, BTN_BLUE, Color.white, () => { });
        y -= step;
 
        // Импорт спрайтов (заглушка)
        CreateButton("Импорт своих фигур", settingsPanel.transform,
            new Vector2(0.12f, y - 0.02f), new Vector2(0.88f, y + 0.055f),
            28, BTN_PURPLE, Color.white, () => {
                ShowDialog("Импорт фигур",
                    "Поместите PNG-файлы фигур (чёрные силуэты) в\nпапку StreamingAssets/Pieces/\nФормат: rat.png, ox.png, tiger.png...\nБелые фигуры создаются автоматически\nсо свечением.",
                    "Понятно", () => ShowSettings());
            });
 
        // Назад
        CreateButton("Назад", settingsPanel.transform,
            new Vector2(0.25f, 0.06f), new Vector2(0.75f, 0.14f),
            34, BTN_GRAY, ACCENT_LIGHT, () => ShowMainMenu());
    }
 
    // ==================== ИГРОВОЙ ИНТЕРФЕЙС ====================
 
    public void ShowGameUI()
    {
        HideAll();
 
        gameUIPanel = CreatePanel("GameUI", canvas.transform);
        RectTransform rt = gameUIPanel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 0.18f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
 
        gameUIPanel.GetComponent<Image>().color = new Color(0.08f, 0.06f, 0.05f, 0.94f);
 
        // Текст хода
        turnText = CreateText("", gameUIPanel.transform,
            new Vector2(0.02f, 0.78f), new Vector2(0.60f, 0.96f),
            30, Color.white, TextAnchor.MiddleLeft).GetComponent<Text>();
 
        // Номер хода
        moveCountText = CreateText("", gameUIPanel.transform,
            new Vector2(0.62f, 0.78f), new Vector2(0.98f, 0.96f),
            26, TEXT_DIM, TextAnchor.MiddleRight).GetComponent<Text>();
 
        // Трофеи
        captureText = CreateText("", gameUIPanel.transform,
            new Vector2(0.02f, 0.55f), new Vector2(0.98f, 0.76f),
            22, ACCENT_GOLD, TextAnchor.MiddleLeft).GetComponent<Text>();

        // Статус (Kal, спецходы)
        statusText = CreateText("", gameUIPanel.transform,
            new Vector2(0.02f, 0.35f), new Vector2(0.98f, 0.55f),
            28, new Color(1f, 0.3f, 0.3f), TextAnchor.MiddleCenter).GetComponent<Text>();
 
        // Думаю...
        thinkingText = CreateText("", gameUIPanel.transform,
            new Vector2(0.02f, 0.35f), new Vector2(0.98f, 0.55f),
            26, new Color(0.5f, 0.8f, 1f), TextAnchor.MiddleCenter).GetComponent<Text>();
 
        // Кнопки
        float btnY1 = 0.02f, btnY2 = 0.30f;
 
        CreateButton("Меню", gameUIPanel.transform,
            new Vector2(0.01f, btnY1), new Vector2(0.19f, btnY2),
            22, BTN_RED, Color.white, () => gameManager.ShowMenu());
 
        CreateButton("Заново", gameUIPanel.transform,
            new Vector2(0.21f, btnY1), new Vector2(0.39f, btnY2),
            22, BTN_GREEN, Color.white, () => gameManager.RestartGame());
 
        CreateButton("Повернуть", gameUIPanel.transform,
            new Vector2(0.41f, btnY1), new Vector2(0.59f, btnY2),
            20, BTN_BLUE, Color.white, () => gameManager.boardRenderer.FlipBoard());
 
        // Храм (телепортация)
        CreateButton("Храм", gameUIPanel.transform,
            new Vector2(0.61f, btnY1), new Vector2(0.79f, btnY2),
            22, BTN_PURPLE, Color.white, () => gameManager.ActivateTemple());
 
        // Рокировка
        CreateButton("Сдвиг", gameUIPanel.transform,
            new Vector2(0.81f, btnY1), new Vector2(0.99f, btnY2),
            22, BTN_ORANGE, Color.white, () => gameManager.ActivateCastling());
 
        UpdateGameInfo();
    }
 
    public void UpdateGameInfo()
    {
        if (turnText == null) return;
 
        bool playerTurn = gameManager.isPlayerTurn;
        bool isLocal = gameManager.currentMode == GameManager.GameMode.LocalMultiplayer;
 
        if (isLocal)
        {
            turnText.text = gameManager.engine.whiteTurn ? "Ход белых" : "Ход чёрных";
            turnText.color = gameManager.engine.whiteTurn ?
                new Color(0.9f, 0.9f, 0.8f) : ACCENT_GOLD;
        }
        else
        {
            turnText.text = playerTurn ? "Ваш ход" : "Ход компьютера";
            turnText.color = playerTurn ? new Color(0.5f, 1f, 0.5f) : new Color(1f, 0.7f, 0.3f);
        }
 
        moveCountText.text = "Ход: " + (gameManager.moveCount / 2 + 1);
 
        string pCap = FormatCaptures(gameManager.playerCapturedPieces);
        string aCap = FormatCaptures(gameManager.aiCapturedPieces);
        captureText.text = "Вы: " + (pCap.Length > 0 ? pCap : "—") +
                           "  |  ИИ: " + (aCap.Length > 0 ? aCap : "—");
 
        // Kal
        bool whiteCheck = gameManager.engine.IsInCheck(true);
        bool blackCheck = gameManager.engine.IsInCheck(false);
        if (whiteCheck || blackCheck)
            statusText.text = "КАЛ!";
        else
            statusText.text = "";
    }
 
    public void ShowThinking(bool show)
    {
        if (thinkingText != null)
            thinkingText.text = show ? "Думаю..." : "";
    }
 
    private string FormatCaptures(List<int> pieces)
    {
        if (pieces.Count == 0) return "";
        string result = "";
        foreach (int p in pieces)
        {
            int type = CalessEngine.PieceType(p);
            if (CalessEngine.PieceNames.ContainsKey(type))
            {
                string name = CalessEngine.PieceNames[type];
                if (name.Length > 2) name = name.Substring(0, 2);
                result += name + " ";
            }
        }
        return result.Trim();
    }
 
    // ==================== КОНЕЦ ИГРЫ ====================
 
    public void ShowGameOver(string title, string message)
    {
        if (gameOverPanel != null) Destroy(gameOverPanel);
 
        gameOverPanel = CreatePanel("GameOver", canvas.transform);
        RectTransform rt = gameOverPanel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.05f, 0.30f);
        rt.anchorMax = new Vector2(0.95f, 0.75f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
 
        gameOverPanel.GetComponent<Image>().color = new Color(0.05f, 0.04f, 0.03f, 0.96f);
 
        CreateText(title, gameOverPanel.transform,
            new Vector2(0.05f, 0.70f), new Vector2(0.95f, 0.92f),
            48, ACCENT_GOLD, TextAnchor.MiddleCenter, FontStyle.Bold);
 
        CreateText(message, gameOverPanel.transform,
            new Vector2(0.05f, 0.45f), new Vector2(0.95f, 0.68f),
            30, ACCENT_LIGHT, TextAnchor.MiddleCenter);
 
        CreateButton("Заново", gameOverPanel.transform,
            new Vector2(0.10f, 0.12f), new Vector2(0.48f, 0.35f),
            32, BTN_GREEN, Color.white, () => {
                Destroy(gameOverPanel);
                gameManager.RestartGame();
            });
 
        CreateButton("Меню", gameOverPanel.transform,
            new Vector2(0.52f, 0.12f), new Vector2(0.90f, 0.35f),
            32, BTN_GRAY, ACCENT_LIGHT, () => {
                Destroy(gameOverPanel);
                gameManager.ShowMenu();
            });
    }
 
    // ==================== ДИАЛОГ ОЖИВЛЕНИЯ ====================
 
    public void ShowReviveDialog(string pieceName, Action onRevive, Action onSkip)
    {
        ShowTwoButtonDialog("Оживление",
            "Козёл может оживить: " + pieceName + "\nИспользовать способность?",
            "Оживить", onRevive,
            "Пропустить", onSkip);
    }
 
    // ==================== ТРЕНИРОВКА ====================

    public void ShowTrainingUI(Action<int> onPieceSelected, Action onBack, Action onClear,
                                Action onRandomEnemy, Action onNextPuzzle, Action onPrevPuzzle,
                                int puzzleIndex, int totalPuzzles, string puzzleDescription)
    {
        HideAll();
 
        trainingUIPanel = CreatePanel("TrainingUI", canvas.transform);
        RectTransform rt = trainingUIPanel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 0.20f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        trainingUIPanel.GetComponent<Image>().color = new Color(0.08f, 0.06f, 0.05f, 0.94f);

        // Описание задачи
        CreateText(puzzleDescription, trainingUIPanel.transform,
            new Vector2(0.02f, 0.68f), new Vector2(0.98f, 0.96f),
            24, ACCENT_LIGHT, TextAnchor.MiddleCenter);
 
        // Номер задачи
        CreateText("Задача " + (puzzleIndex + 1) + " / " + totalPuzzles, trainingUIPanel.transform,
            new Vector2(0.02f, 0.52f), new Vector2(0.98f, 0.68f),
            22, ACCENT_GOLD, TextAnchor.MiddleCenter);
 
        // Кнопки
        float btnY1 = 0.04f, btnY2 = 0.48f;
 
        CreateButton("Назад", trainingUIPanel.transform,
            new Vector2(0.01f, btnY1), new Vector2(0.19f, btnY2),
            20, BTN_RED, Color.white, onBack);
 
        CreateButton("< Пред", trainingUIPanel.transform,
            new Vector2(0.21f, btnY1), new Vector2(0.39f, btnY2),
            20, BTN_GRAY, ACCENT_LIGHT, onPrevPuzzle);
 
        CreateButton("След >", trainingUIPanel.transform,
            new Vector2(0.41f, btnY1), new Vector2(0.59f, btnY2),
            20, BTN_GRAY, ACCENT_LIGHT, onNextPuzzle);
 
        CreateButton("Враги", trainingUIPanel.transform,
            new Vector2(0.61f, btnY1), new Vector2(0.79f, btnY2),
            20, BTN_ORANGE, Color.white, onRandomEnemy);
 
        CreateButton("Сброс", trainingUIPanel.transform,
            new Vector2(0.81f, btnY1), new Vector2(0.99f, btnY2),
            20, BTN_BLUE, Color.white, onClear);
    }
    // ==================== BLUETOOTH ====================
 
    public void ShowBluetoothUI(Action onHost, Action onJoin, Action onBack)
    {
        HideAll();
 
        menuPanel = CreateFullPanel("BluetoothMenu");
        menuPanel.GetComponent<Image>().color = BG_DARK;
 
        CreateText("Bluetooth мультиплеер", menuPanel.transform,
            new Vector2(0.05f, 0.80f), new Vector2(0.95f, 0.92f),
            42, ACCENT_GOLD, TextAnchor.MiddleCenter, FontStyle.Bold);
 
        CreateButton("Создать игру (хост)", menuPanel.transform,
            new Vector2(0.12f, 0.58f), new Vector2(0.88f, 0.68f),
            36, BTN_GREEN, Color.white, onHost);
 
        CreateButton("Присоединиться", menuPanel.transform,
            new Vector2(0.12f, 0.44f), new Vector2(0.88f, 0.54f),
            36, BTN_BLUE, Color.white, onJoin);
 
        CreateButton("Назад", menuPanel.transform,
            new Vector2(0.25f, 0.25f), new Vector2(0.75f, 0.33f),
            32, BTN_GRAY, ACCENT_LIGHT, onBack);
    }

    // ==================== ОБЩИЙ ДИАЛОГ ====================
 
    public void ShowDialog(string title, string message, string btnText, Action onConfirm)
    {
        if (dialogPanel != null) Destroy(dialogPanel);
 
        dialogPanel = CreatePanel("Dialog", canvas.transform);
        RectTransform rt = dialogPanel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.05f, 0.30f);
        rt.anchorMax = new Vector2(0.95f, 0.70f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        dialogPanel.GetComponent<Image>().color = new Color(0.06f, 0.05f, 0.04f, 0.97f);
 
        CreateText(title, dialogPanel.transform,
            new Vector2(0.05f, 0.72f), new Vector2(0.95f, 0.92f),
            38, ACCENT_GOLD, TextAnchor.MiddleCenter, FontStyle.Bold);
 
        CreateText(message, dialogPanel.transform,
            new Vector2(0.05f, 0.30f), new Vector2(0.95f, 0.70f),
            26, ACCENT_LIGHT, TextAnchor.MiddleCenter);
 
        CreateButton(btnText, dialogPanel.transform,
            new Vector2(0.20f, 0.08f), new Vector2(0.80f, 0.26f),
            32, BTN_GREEN, Color.white, () => {
                Destroy(dialogPanel);
                onConfirm?.Invoke();
            });
    }
 
    public void ShowTwoButtonDialog(string title, string message,
        string btn1Text, Action onBtn1, string btn2Text, Action onBtn2)
    {
        if (dialogPanel != null) Destroy(dialogPanel);

        dialogPanel = CreatePanel("Dialog2", canvas.transform);
        RectTransform rt = dialogPanel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.05f, 0.30f);
        rt.anchorMax = new Vector2(0.95f, 0.70f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        dialogPanel.GetComponent<Image>().color = new Color(0.06f, 0.05f, 0.04f, 0.97f);
 
        CreateText(title, dialogPanel.transform,
            new Vector2(0.05f, 0.72f), new Vector2(0.95f, 0.92f),
            38, ACCENT_GOLD, TextAnchor.MiddleCenter, FontStyle.Bold);
 
        CreateText(message, dialogPanel.transform,
            new Vector2(0.05f, 0.30f), new Vector2(0.95f, 0.70f),
            26, ACCENT_LIGHT, TextAnchor.MiddleCenter);
 
        CreateButton(btn1Text, dialogPanel.transform,
            new Vector2(0.05f, 0.08f), new Vector2(0.47f, 0.26f),
            28, BTN_GREEN, Color.white, () => {
                Destroy(dialogPanel);
                onBtn1?.Invoke();
            });
 
        CreateButton(btn2Text, dialogPanel.transform,
            new Vector2(0.53f, 0.08f), new Vector2(0.95f, 0.26f),
            28, BTN_GRAY, ACCENT_LIGHT, () => {
                Destroy(dialogPanel);
                onBtn2?.Invoke();
            });
    }
 
    // ==================== УТИЛИТЫ ====================
 
    public void HideAll()
    {
        if (menuPanel != null) { Destroy(menuPanel); menuPanel = null; }
        if (playSubMenu != null) { Destroy(playSubMenu); playSubMenu = null; }
        if (settingsPanel != null) { Destroy(settingsPanel); settingsPanel = null; }
        if (gameUIPanel != null) { Destroy(gameUIPanel); gameUIPanel = null; }
        if (gameOverPanel != null) { Destroy(gameOverPanel); gameOverPanel = null; }
        if (trainingUIPanel != null) { Destroy(trainingUIPanel); trainingUIPanel = null; }
        if (difficultyPanel != null) { Destroy(difficultyPanel); difficultyPanel = null; }
        if (sidePanel != null) { Destroy(sidePanel); sidePanel = null; }
        if (dialogPanel != null) { Destroy(dialogPanel); dialogPanel = null; }
    }
 
    private GameObject CreateFullPanel(string name)
    {
        GameObject panel = CreatePanel(name, canvas.transform);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return panel;
    }
 
    private GameObject CreatePanel(string name, Transform parent)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
 
        Image img = panel.AddComponent<Image>();
        img.color = BG_PANEL;
        img.raycastTarget = true;
 
        return panel;
    }

    private GameObject CreateText(string text, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax,
        int fontSize, Color color, TextAnchor anchor, FontStyle style = FontStyle.Normal)
    {
        GameObject obj = new GameObject("Text");
        obj.transform.SetParent(parent, false);
 
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Text t = obj.AddComponent<Text>();
        t.text = text;
        t.font = cachedFont;
        t.fontSize = fontSize;
        t.color = color;
        t.alignment = anchor;
        t.fontStyle = style;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Truncate;
        t.raycastTarget = false;
 
        return obj;
    }

    private void CreateButton(string text, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax,
        int fontSize, Color bgColor, Color textColor, Action onClick)
    {
        GameObject btnObj = new GameObject("Button_" + text);
        btnObj.transform.SetParent(parent, false);
 
        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
 
        Image img = btnObj.AddComponent<Image>();
        img.color = bgColor;
 
        Button btn = btnObj.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(1.2f, 1.2f, 1.2f);
        cb.pressedColor = new Color(0.7f, 0.7f, 0.7f);
        btn.colors = cb;
 
        if (onClick != null)
            btn.onClick.AddListener(() => onClick());

        // Текст
        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(btnObj.transform, false);
 
        RectTransform trt = txtObj.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(4, 2);
        trt.offsetMax = new Vector2(-4, -2);
 
        Text t = txtObj.AddComponent<Text>();
        t.text = text;
        t.font = cachedFont;
        t.fontSize = fontSize;
        t.color = textColor;
        t.alignment = TextAnchor.MiddleCenter;
        t.fontStyle = FontStyle.Bold;
        t.raycastTarget = false;
    }
 
    private void CreateToggleRow(string label, bool currentValue, Transform parent,
        float y, Action<bool> onToggle)
    {
        CreateText(label, parent,
            new Vector2(0.05f, y), new Vector2(0.55f, y + 0.06f),
            28, ACCENT_LIGHT, TextAnchor.MiddleLeft);
 
        string btnText = currentValue ? "ВКЛ" : "ВЫКЛ";
        Color btnColor = currentValue ? BTN_GREEN : BTN_RED;
 
        CreateButton(btnText, parent,
            new Vector2(0.60f, y), new Vector2(0.95f, y + 0.06f),
            26, btnColor, Color.white, () => onToggle(!currentValue));
    }

    private void CreatePieceIcon(Transform parent, int pieceType, bool isWhite, Vector2 position, int size)
    {
        Sprite sprite = PieceSpriteGenerator.GetPieceSprite(pieceType, isWhite, PieceSpritesHolder.Sprites);
        if (sprite == null) return;
    
        GameObject iconObj = new GameObject("PieceIcon_" + pieceType);
        iconObj.transform.SetParent(parent, false);
    
        RectTransform rt = iconObj.AddComponent<RectTransform>();
        rt.anchorMin = position;
        rt.anchorMax = new Vector2(position.x + 0.06f, position.y + 0.08f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    
        Image img = iconObj.AddComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;
        img.raycastTarget = false;

        // Устанавливаем цвет в зависимости от цвета фигуры
        img.color = Color.red;
    }
}