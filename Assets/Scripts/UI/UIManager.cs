using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// UIManager для Caless. Таймер вверху, история ходов + undo внизу.
/// Рабочие настройки: тема, язык, доска. Bluetooth с экраном ожидания.
/// </summary>
public class UIManager : MonoBehaviour
{
    [HideInInspector] public GameManager gameManager;

    [Header("Главное меню — фон")]
    public Sprite menuBackgroundImage;

    [Header("Главное меню — шапка")]
    public Sprite titleImage;
    public Sprite subtitleImage;
    public Sprite profileIconImage;
    public Sprite trophyIconImage;

    [Header("Главное меню — карточки режимов")]
    public Sprite vsComputerCardImage;
    public Sprite vsHumanCardImage;
    public Sprite bluetoothCardImage;

    [Header("Нижняя панель вкладок")]
    public Sprite tabBarBackgroundImage;
    public Sprite tabPlayIconImage;
    public Sprite tabEducationIconImage;
    public Sprite tabSettingsIconImage;

    private Canvas canvas;
    private CanvasScaler canvasScaler;

    // Panels
    private GameObject menuPanel;
    private GameObject playSubMenu;
    private GameObject settingsPanel;
    private GameObject gameUIPanel;
    private GameObject gameUITopPanel;
    private GameObject gameOverPanel;
    private GameObject trainingUIPanel;
    private GameObject difficultyPanel;
    private GameObject sidePanel;
    private GameObject dialogPanel;
    private GameObject bluetoothWaitingPanel;

    // Tab system
    private enum Tab { Play, Education, Settings }
    private Tab currentTab = Tab.Play;
    private GameObject tabBar;
    private GameObject tabContentArea;

    // Game UI — верхняя панель (таймер)
    private Text whiteTimerText;
    private Text blackTimerText;
    private Text turnText;
    private Text statusText;
    private Text thinkingText;

    // Game UI — нижняя панель (история + кнопки)
    private Text moveCountText;
    private Text captureText;
    private ScrollRect moveHistoryScroll;
    private Transform moveHistoryContent;

    // Bluetooth waiting
    private Text bluetoothStatusText;

    private Font cachedFont;

    // Colors
    private static readonly Color BG_DARK         = new Color(0.06f, 0.05f, 0.04f, 1f);
    private static readonly Color BG_PANEL        = new Color(0.10f, 0.08f, 0.07f, 0.98f);
    private static readonly Color BG_CARD         = new Color(0.12f, 0.10f, 0.09f, 0.95f);
    private static readonly Color BG_SECTION      = new Color(0.14f, 0.12f, 0.10f, 0.90f);
    private static readonly Color ACCENT_GOLD     = new Color(0.85f, 0.70f, 0.30f);
    private static readonly Color ACCENT_GOLD_DIM = new Color(0.65f, 0.50f, 0.20f);
    private static readonly Color ACCENT_LIGHT    = new Color(0.90f, 0.85f, 0.75f);
    private static readonly Color BORDER_GOLD     = new Color(0.70f, 0.55f, 0.25f, 0.6f);
    private static readonly Color BTN_GREEN       = new Color(0.12f, 0.35f, 0.18f);
    private static readonly Color BTN_ORANGE      = new Color(0.50f, 0.32f, 0.10f);
    private static readonly Color BTN_BLUE        = new Color(0.12f, 0.22f, 0.45f);
    private static readonly Color BTN_RED         = new Color(0.45f, 0.12f, 0.12f);
    private static readonly Color BTN_PURPLE      = new Color(0.30f, 0.12f, 0.45f);
    private static readonly Color BTN_GRAY        = new Color(0.22f, 0.20f, 0.18f);
    private static readonly Color TEXT_DIM        = new Color(0.55f, 0.50f, 0.42f);
    private static readonly Color TEXT_SUBTITLE   = new Color(0.70f, 0.65f, 0.55f);
    private static readonly Color TAB_ACTIVE      = new Color(0.85f, 0.70f, 0.30f);
    private static readonly Color TAB_INACTIVE    = new Color(0.45f, 0.40f, 0.35f);
    private static readonly Color TOGGLE_ON       = new Color(0.75f, 0.60f, 0.20f);
    private static readonly Color TOGGLE_OFF      = new Color(0.30f, 0.28f, 0.25f);
    private static readonly Color SLIDER_BG       = new Color(0.20f, 0.18f, 0.15f);
    private static readonly Color SLIDER_FILL     = new Color(0.75f, 0.60f, 0.22f);

    // ==================== ЛОКАЛИЗАЦИЯ ====================

    private string Tr(string key)
    {
        bool eng = (gameManager != null && gameManager.settings != null &&
                    gameManager.settings.Language == 1);
        if (!eng) return key;
        switch (key)
        {
            case "Ваш ход":        return "Your turn";
            case "Ход компьютера": return "Computer's turn";
            case "Ход белых":      return "White's turn";
            case "Ход чёрных":    return "Black's turn";
            case "Белые":          return "White";
            case "Чёрные":         return "Black";
            case "Ход":            return "Move";
            case "Вы":             return "You";
            case "ИИ":             return "AI";
            case "КАЛ!":           return "CHECK!";
            case "Думаю...":       return "Thinking...";
            case "Меню":           return "Menu";
            case "Заново":         return "Restart";
            case "Повернуть":      return "Flip";
            case "Назад":          return "Back";
            case "Храм":           return "Temple";
            case "Сдвиг":          return "Castle";
            case "Ход назад":      return "Undo";
            case "Ходы":           return "Moves";
            case "ИГРАТЬ":         return "PLAY";
            case "ОБУЧЕНИЕ":       return "LEARN";
            case "НАСТРОЙКИ":      return "SETTINGS";
            default:               return key;
        }
    }

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
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        canvasScaler = canvasObj.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution  = new Vector2(1080, 1920);
        canvasScaler.matchWidthOrHeight   = 0.5f;

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
        currentTab = Tab.Play;
        BuildTabLayout();
    }

    private void BuildTabLayout()
    {
        menuPanel = CreateFullPanel("MainMenu");
        Image menuBg = menuPanel.GetComponent<Image>();
        if (menuBackgroundImage != null)
        {
            menuBg.sprite        = menuBackgroundImage;
            menuBg.type          = Image.Type.Simple;
            menuBg.preserveAspect= false;
            menuBg.color         = Color.white;
        }
        else
        {
            menuBg.color = BG_DARK;
        }

        tabContentArea = new GameObject("TabContent");
        tabContentArea.transform.SetParent(menuPanel.transform, false);
        RectTransform contentRT = tabContentArea.AddComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 0.08f);
        contentRT.anchorMax = Vector2.one;
        contentRT.offsetMin = contentRT.offsetMax = Vector2.zero;

        BuildTabBar(menuPanel.transform);

        switch (currentTab)
        {
            case Tab.Play:      BuildPlayTab();      break;
            case Tab.Education: BuildEducationTab(); break;
            case Tab.Settings:  BuildSettingsTab();  break;
        }
    }

    private void BuildTabBar(Transform parent)
    {
        tabBar = new GameObject("TabBar");
        tabBar.transform.SetParent(parent, false);
        RectTransform barRT = tabBar.AddComponent<RectTransform>();
        barRT.anchorMin = new Vector2(0, 0);
        barRT.anchorMax = new Vector2(1, 0.08f);
        barRT.offsetMin = barRT.offsetMax = Vector2.zero;

        Image barBg = tabBar.AddComponent<Image>();
        if (tabBarBackgroundImage != null)
        {
            barBg.sprite        = tabBarBackgroundImage;
            barBg.type          = Image.Type.Simple;
            barBg.preserveAspect= false;
            barBg.color         = Color.white;
        }
        else
        {
            barBg.color = new Color(0.08f, 0.07f, 0.06f, 1f);
        }

        // Top border
        GameObject topLine = new GameObject("TabBarTopLine");
        topLine.transform.SetParent(tabBar.transform, false);
        RectTransform lineRT = topLine.AddComponent<RectTransform>();
        lineRT.anchorMin = new Vector2(0, 1);
        lineRT.anchorMax = new Vector2(1, 1);
        lineRT.offsetMin = lineRT.offsetMax = Vector2.zero;
        lineRT.sizeDelta = new Vector2(0, 2);
        lineRT.pivot     = new Vector2(0.5f, 1);
        topLine.AddComponent<Image>().color = BORDER_GOLD;

        string[] tabIcons       = { "\u265E", "\uD83D\uDCD6", "\u2699" };
        Sprite[] tabIconSprites = { tabPlayIconImage, tabEducationIconImage, tabSettingsIconImage };
        string[] tabLabels      = { Tr("ИГРАТЬ"), Tr("ОБУЧЕНИЕ"), Tr("НАСТРОЙКИ") };
        Tab[]    tabs           = { Tab.Play, Tab.Education, Tab.Settings };

        for (int i = 0; i < 3; i++)
        {
            float xMin    = i / 3f;
            float xMax    = (i + 1) / 3f;
            Tab   tab     = tabs[i];
            bool  isActive= (tab == currentTab);

            GameObject tabBtn = new GameObject("Tab_" + tabLabels[i]);
            tabBtn.transform.SetParent(tabBar.transform, false);
            RectTransform tabRT = tabBtn.AddComponent<RectTransform>();
            tabRT.anchorMin = new Vector2(xMin, 0);
            tabRT.anchorMax = new Vector2(xMax, 1);
            tabRT.offsetMin = tabRT.offsetMax = Vector2.zero;
            tabBtn.AddComponent<Image>().color = Color.clear;

            Button btn = tabBtn.AddComponent<Button>();
            Tab capturedTab = tab;
            btn.onClick.AddListener(() => SwitchTab(capturedTab));

            if (tabIconSprites[i] != null)
            {
                CreateImageElement("TabIcon", tabBtn.transform,
                    new Vector2(0.30f, 0.40f), new Vector2(0.70f, 0.85f),
                    tabIconSprites[i],
                    isActive ? Color.white : new Color(0.55f, 0.55f, 0.55f, 0.85f), true);
            }
            else
            {
                CreateText(tabIcons[i], tabBtn.transform,
                    new Vector2(0, 0.40f), new Vector2(1, 0.85f),
                    36, isActive ? TAB_ACTIVE : TAB_INACTIVE, TextAnchor.MiddleCenter);
            }

            CreateText(tabLabels[i], tabBtn.transform,
                new Vector2(0, 0.02f), new Vector2(1, 0.42f),
                18, isActive ? TAB_ACTIVE : TAB_INACTIVE, TextAnchor.MiddleCenter, FontStyle.Bold);

            if (isActive)
            {
                GameObject ind = new GameObject("Indicator");
                ind.transform.SetParent(tabBtn.transform, false);
                RectTransform indRT = ind.AddComponent<RectTransform>();
                indRT.anchorMin = new Vector2(0.15f, 0.92f);
                indRT.anchorMax = new Vector2(0.85f, 1f);
                indRT.offsetMin = indRT.offsetMax = Vector2.zero;
                ind.AddComponent<Image>().color = TAB_ACTIVE;
            }
        }
    }

    private void SwitchTab(Tab tab)
    {
        if (tab == currentTab) return;
        currentTab = tab;
        HideAll();
        BuildTabLayout();
    }

    // ==================== PLAY TAB ====================

    private void BuildPlayTab()
    {
        Transform parent = tabContentArea.transform;

        if (titleImage != null)
        {
            CreateImageElement("TitleLogo", parent,
                new Vector2(0.10f, 0.83f), new Vector2(0.90f, 0.99f),
                titleImage, Color.white, true);
        }
        else
        {
            CreateText("\u2655", parent,
                new Vector2(0.40f, 0.92f), new Vector2(0.60f, 0.98f),
                40, ACCENT_GOLD, TextAnchor.MiddleCenter);
            CreateDecorativeLine(parent, new Vector2(0.10f, 0.905f), new Vector2(0.38f, 0.915f));
            CreateDecorativeLine(parent, new Vector2(0.62f, 0.905f), new Vector2(0.90f, 0.915f));
            CreateText("CALESS", parent,
                new Vector2(0.10f, 0.84f), new Vector2(0.90f, 0.93f),
                64, ACCENT_GOLD, TextAnchor.MiddleCenter, FontStyle.Bold);
        }

        if (subtitleImage != null)
        {
            CreateImageElement("SubtitleImg", parent,
                new Vector2(0.20f, 0.79f), new Vector2(0.80f, 0.84f),
                subtitleImage, Color.white, true);
        }
        else
        {
            CreateText("СТРАТЕГИЯ. ТАКТИКА. ПОБЕДА.", parent,
                new Vector2(0.10f, 0.80f), new Vector2(0.90f, 0.85f),
                24, TEXT_SUBTITLE, TextAnchor.MiddleCenter);
        }

        BuildDecorativeBoardPreview(parent,
            new Vector2(0.04f, 0.30f), new Vector2(0.96f, 0.79f));

        BuildModeCard(parent,
            new Vector2(0.02f, 0.05f), new Vector2(0.33f, 0.28f),
            "\uD83D\uDCBB", "С КОМПЬЮТЕРОМ",
            "БРОСЬТЕ ВЫЗОВ\nИСКУССТВЕННОМУ\nИНТЕЛЛЕКТУ",
            new Color(0.08f, 0.18f, 0.12f, 0.95f), vsComputerCardImage,
            () => ShowDifficultySelect());

        BuildModeCard(parent,
            new Vector2(0.345f, 0.05f), new Vector2(0.665f, 0.28f),
            "\u263A", "С ЧЕЛОВЕКОМ",
            "ИГРАЙТЕ С ДРУГОМ\nНА ОДНОМ\nУСТРОЙСТВЕ",
            new Color(0.08f, 0.15f, 0.18f, 0.95f), vsHumanCardImage,
            () => gameManager.StartLocalMultiplayer());

        BuildModeCard(parent,
            new Vector2(0.69f, 0.05f), new Vector2(0.98f, 0.28f),
            "\u2726", "BLUETOOTH",
            "ПОДКЛЮЧИСЬ И\nИГРАЙ С ДРУГОМ\nРЯДОМ",
            new Color(0.12f, 0.08f, 0.18f, 0.95f), bluetoothCardImage,
            () => gameManager.StartBluetoothGame());
    }

    private void BuildModeCard(Transform parent, Vector2 anchorMin, Vector2 anchorMax,
        string icon, string title, string subtitle, Color cardColor, Sprite cardImage, Action onClick)
    {
        GameObject card = new GameObject("Card_" + title);
        card.transform.SetParent(parent, false);
        RectTransform cardRT = card.AddComponent<RectTransform>();
        cardRT.anchorMin = anchorMin;
        cardRT.anchorMax = anchorMax;
        cardRT.offsetMin = cardRT.offsetMax = Vector2.zero;

        Image cardBg = card.AddComponent<Image>();
        if (cardImage != null)
        {
            cardBg.sprite        = cardImage;
            cardBg.type          = Image.Type.Simple;
            cardBg.preserveAspect= false;
            cardBg.color         = Color.white;
        }
        else
        {
            cardBg.color = cardColor;
        }

        Button cardBtn = card.AddComponent<Button>();
        ColorBlock cb  = cardBtn.colors;
        cb.normalColor      = Color.white;
        cb.highlightedColor = new Color(1.15f, 1.15f, 1.15f);
        cb.pressedColor     = new Color(0.8f, 0.8f, 0.8f);
        cardBtn.colors = cb;
        cardBtn.onClick.AddListener(() => onClick?.Invoke());

        GameObject border = new GameObject("Border");
        border.transform.SetParent(card.transform, false);
        RectTransform borderRT = border.AddComponent<RectTransform>();
        borderRT.anchorMin = Vector2.zero;
        borderRT.anchorMax = Vector2.one;
        borderRT.offsetMin = borderRT.offsetMax = Vector2.zero;
        Outline outline = border.AddComponent<Outline>();
        outline.effectColor    = BORDER_GOLD;
        outline.effectDistance = new Vector2(2, 2);

        if (cardImage != null) return;

        CreateText(icon, card.transform,
            new Vector2(0.1f, 0.55f), new Vector2(0.9f, 0.90f),
            42, ACCENT_GOLD, TextAnchor.MiddleCenter);
        CreateText(title, card.transform,
            new Vector2(0.02f, 0.35f), new Vector2(0.98f, 0.55f),
            16, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
        CreateText(subtitle, card.transform,
            new Vector2(0.02f, 0.05f), new Vector2(0.98f, 0.35f),
            12, TEXT_DIM, TextAnchor.MiddleCenter);
        CreateText("\u2666", card.transform,
            new Vector2(0.40f, 0.00f), new Vector2(0.60f, 0.08f),
            12, ACCENT_GOLD_DIM, TextAnchor.MiddleCenter);
    }

    private void BuildDecorativeBoardPreview(Transform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject boardFrame = new GameObject("BoardFrame");
        boardFrame.transform.SetParent(parent, false);
        RectTransform frameRT = boardFrame.AddComponent<RectTransform>();
        frameRT.anchorMin = anchorMin;
        frameRT.anchorMax = anchorMax;
        frameRT.offsetMin = frameRT.offsetMax = Vector2.zero;

        boardFrame.AddComponent<Image>().color = new Color(0.15f, 0.12f, 0.08f, 0.9f);
        Outline frameOutline = boardFrame.AddComponent<Outline>();
        frameOutline.effectColor    = BORDER_GOLD;
        frameOutline.effectDistance = new Vector2(3, 3);

        GameObject boardGrid = new GameObject("BoardGrid");
        boardGrid.transform.SetParent(boardFrame.transform, false);
        RectTransform gridRT = boardGrid.AddComponent<RectTransform>();
        gridRT.anchorMin = new Vector2(0.06f, 0.04f);
        gridRT.anchorMax = new Vector2(0.94f, 0.96f);
        gridRT.offsetMin = gridRT.offsetMax = Vector2.zero;

        SettingsManager s = (gameManager != null) ? gameManager.settings : null;
        Color darkSq  = (s != null) ? s.GetDarkSquareColor()  : new Color(0.22f, 0.18f, 0.15f);
        Color lightSq = (s != null) ? s.GetLightSquareColor() : new Color(0.45f, 0.38f, 0.30f);

        for (int r = 0; r < 10; r++)
        {
            for (int c = 0; c < 10; c++)
            {
                bool isLight = (r + c) % 2 == 0;
                GameObject cell = new GameObject("Cell");
                cell.transform.SetParent(boardGrid.transform, false);
                RectTransform cellRT = cell.AddComponent<RectTransform>();
                cellRT.anchorMin = new Vector2(c / 10f, r / 10f);
                cellRT.anchorMax = new Vector2((c + 1) / 10f, (r + 1) / 10f);
                cellRT.offsetMin = cellRT.offsetMax = Vector2.zero;
                Image cellImg = cell.AddComponent<Image>();
                cellImg.color        = isLight ? lightSq : darkSq;
                cellImg.raycastTarget= false;
            }
        }

        string[] cols = { "A","B","C","D","E","F","G","H","I","J" };
        for (int i = 0; i < 10; i++)
        {
            float yCenter = 0.04f + (0.92f * (i + 0.5f) / 10f);
            CreateText((i + 1).ToString(), boardFrame.transform,
                new Vector2(0f, yCenter - 0.03f), new Vector2(0.06f, yCenter + 0.03f),
                16, TEXT_DIM, TextAnchor.MiddleCenter);

            float xCenter = 0.06f + (0.88f * (i + 0.5f) / 10f);
            CreateText(cols[i], boardFrame.transform,
                new Vector2(xCenter - 0.03f, 0f), new Vector2(xCenter + 0.03f, 0.04f),
                16, TEXT_DIM, TextAnchor.MiddleCenter);
        }
    }

    // ==================== EDUCATION TAB ====================

    private void BuildEducationTab()
    {
        Transform parent = tabContentArea.transform;

        CreateText("Caless", parent,
            new Vector2(0.15f, 0.92f), new Vector2(0.85f, 0.99f),
            52, ACCENT_GOLD, TextAnchor.MiddleCenter, FontStyle.BoldAndItalic);

        CreateText("\u2666  ОБУЧЕНИЕ  \u2666", parent,
            new Vector2(0.15f, 0.86f), new Vector2(0.85f, 0.91f),
            36, ACCENT_GOLD, TextAnchor.MiddleCenter, FontStyle.Bold);

        CreateText("Освойте игру шаг за шагом", parent,
            new Vector2(0.15f, 0.83f), new Vector2(0.85f, 0.87f),
            22, TEXT_SUBTITLE, TextAnchor.MiddleCenter);

        GameObject scrollArea = CreateScrollView(parent,
            new Vector2(0.03f, 0.01f), new Vector2(0.97f, 0.82f));
        Transform content = scrollArea.transform.Find("Viewport/Content");

        string[] numbers = { "1.", "2.", "3.", "4.", "5.", "6." };
        string[] titles  = { "ОСНОВЫ", "ТАКТИКА", "СТРАТЕГИЯ", "МИТТЕЛЬШПИЛЬ", "ЭНДШПИЛЬ", "ПРАКТИКА" };
        string[] descs   = {
            "Изучите доску, фигуры и их\nбазовые ходы.",
            "Узнайте о тактических приёмах\nи комбинациях.",
            "Понимайте планы, позиции\nи ключевые принципы.",
            "Научитесь планировать и находить\nлучшие продолжения.",
            "Освойте техники завершения\nпартии и выигрыша.",
            "Закрепите знания на\nпрактических примерах и задачах."
        };
        string[] icons = { "\u265E", "\u2694", "\u265C", "\u265E", "\u265A", "\u25CE" };

        float cardH = 160f, cardSpacing = 16f;
        for (int i = 0; i < titles.Length; i++)
            BuildLessonCard(content, i, cardH, cardSpacing, numbers[i], titles[i], descs[i], icons[i]);

        RectTransform contentRT = content.GetComponent<RectTransform>();
        contentRT.sizeDelta = new Vector2(0, titles.Length * (cardH + cardSpacing) + 20);
    }

    private void BuildLessonCard(Transform parent, int index, float cardH, float spacing,
        string number, string title, string desc, string icon)
    {
        float yOffset = -(index * (cardH + spacing) + 10);

        GameObject card = new GameObject("Lesson_" + title);
        card.transform.SetParent(parent, false);
        RectTransform rt = card.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.offsetMin = new Vector2(10, 0);
        rt.offsetMax = new Vector2(-10, 0);
        rt.anchoredPosition = new Vector2(0, yOffset);
        rt.sizeDelta        = new Vector2(rt.sizeDelta.x, cardH);

        card.AddComponent<Image>().color = BG_CARD;
        Outline o = card.AddComponent<Outline>();
        o.effectColor    = index == 0 ? new Color(0.3f, 0.6f, 0.9f, 0.8f) : BORDER_GOLD;
        o.effectDistance = new Vector2(index == 0 ? 3 : 2, index == 0 ? 3 : 2);

        card.AddComponent<Button>().onClick.AddListener(() => {
            if (gameManager != null) gameManager.StartTraining();
        });

        GameObject iconCircle = new GameObject("IconCircle");
        iconCircle.transform.SetParent(card.transform, false);
        RectTransform iconRT = iconCircle.AddComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(0.02f, 0.15f);
        iconRT.anchorMax = new Vector2(0.16f, 0.85f);
        iconRT.offsetMin = iconRT.offsetMax = Vector2.zero;
        iconCircle.AddComponent<Image>().color = new Color(0.18f, 0.15f, 0.12f, 0.9f);
        CreateText(icon, iconCircle.transform, Vector2.zero, Vector2.one,
            38, ACCENT_GOLD, TextAnchor.MiddleCenter);

        CreateText(number + " " + title, card.transform,
            new Vector2(0.19f, 0.50f), new Vector2(0.75f, 0.88f),
            30, Color.white, TextAnchor.MiddleLeft, FontStyle.Bold);

        CreateText(desc, card.transform,
            new Vector2(0.19f, 0.08f), new Vector2(0.75f, 0.52f),
            20, TEXT_DIM, TextAnchor.UpperLeft);

        CreateText("\u276F", card.transform,
            new Vector2(0.88f, 0.30f), new Vector2(0.98f, 0.70f),
            32, TEXT_DIM, TextAnchor.MiddleCenter);
    }

    // ==================== SETTINGS TAB ====================

    private void BuildSettingsTab()
    {
        Transform parent = tabContentArea.transform;

        CreateText("Caless", parent,
            new Vector2(0.15f, 0.93f), new Vector2(0.85f, 0.99f),
            52, ACCENT_GOLD, TextAnchor.MiddleCenter, FontStyle.BoldAndItalic);

        CreateText("\u2666  НАСТРОЙКИ  \u2666", parent,
            new Vector2(0.10f, 0.87f), new Vector2(0.90f, 0.93f),
            36, ACCENT_GOLD, TextAnchor.MiddleCenter, FontStyle.Bold);

        CreateText("Настройте игру под себя", parent,
            new Vector2(0.15f, 0.84f), new Vector2(0.85f, 0.88f),
            22, TEXT_SUBTITLE, TextAnchor.MiddleCenter);

        GameObject scrollArea = CreateScrollView(parent,
            new Vector2(0.02f, 0.01f), new Vector2(0.98f, 0.83f));
        Transform content = scrollArea.transform.Find("Viewport/Content");

        SettingsManager s = (gameManager != null) ? gameManager.settings : null;
        float yPos = -16f;

        // ВНЕШНИЙ ВИД
        yPos = BuildSectionHeader(content, "ВНЕШНИЙ ВИД", yPos);

        yPos = BuildSettingRowWithOptions(content, "\uD83C\uDFA8  Тема интерфейса", yPos,
            SettingsManager.BoardThemeNames, s != null ? s.BoardTheme : 0,
            (idx) => {
                if (s != null) { s.BoardTheme = idx; s.Save(); }
                gameManager?.ApplyBoardTheme();
            });

        yPos = BuildSettingRowWithColorSwatches(content, "\u2B1C  Стиль доски", yPos,
            new Color[] {
                new Color(0.45f, 0.38f, 0.30f),
                new Color(0.93f, 0.87f, 0.78f),
                new Color(0.18f, 0.30f, 0.15f),
                new Color(0.25f, 0.25f, 0.30f)
            },
            s != null ? s.BoardTheme : 0,
            (idx) => {
                if (s != null) { s.BoardTheme = idx; s.Save(); }
                gameManager?.ApplyBoardTheme();
            });

        yPos = BuildSettingRowWithColorSwatches(content, "\u2600  Цвет подсветки", yPos,
            new Color[] {
                ACCENT_GOLD,
                new Color(0.3f, 0.5f, 0.9f),
                new Color(0.2f, 0.7f, 0.3f),
                new Color(0.6f, 0.3f, 0.7f)
            },
            s != null ? s.HighlightColor : 0,
            (idx) => { if (s != null) { s.HighlightColor = idx; s.Save(); } });

        yPos = BuildToggleRow(content, "\uD83C\uDF00  Анимации", yPos,
            s != null ? s.AnimationsEnabled : true,
            (val) => { if (s != null) { s.AnimationsEnabled = val; s.Save(); } });

        yPos -= 20f;

        // ЗВУК
        yPos = BuildSectionHeader(content, "ЗВУК", yPos);

        yPos = BuildSliderRow(content, "\uD83C\uDFB5  Музыка", yPos,
            s != null ? s.MusicVolume : 0.7f,
            (val) => { if (s != null) { s.MusicVolume = val; s.Save(); } });

        yPos = BuildSliderRow(content, "\uD83D\uDD0A  Звуковые эффекты", yPos,
            s != null ? s.SoundVolume : 0.8f,
            (val) => { if (s != null) { s.SoundVolume = val; s.Save(); } });

        yPos = BuildToggleRow(content, "\u265E  Звук перемещения", yPos,
            s != null ? s.MoveSoundEnabled : true,
            (val) => { if (s != null) { s.MoveSoundEnabled = val; s.Save(); } });

        yPos = BuildToggleRow(content, "\u2694  Звук захвата", yPos,
            s != null ? s.CaptureSoundEnabled : true,
            (val) => { if (s != null) { s.CaptureSoundEnabled = val; s.Save(); } });

        yPos -= 20f;

        // ИГРА
        yPos = BuildSectionHeader(content, "ИГРА", yPos);

        yPos = BuildToggleRow(content, "\uD83D\uDCA1  Подсказки", yPos,
            s != null ? s.HintsEnabled : false,
            (val) => { if (s != null) { s.HintsEnabled = val; s.Save(); } });

        yPos = BuildToggleRow(content, "\u2699  Показывать возможные ходы", yPos,
            s != null ? s.ShowPossibleMoves : true,
            (val) => { if (s != null) { s.ShowPossibleMoves = val; s.Save(); } });

        yPos = BuildToggleRow(content, "\uD83D\uDEE1  Автосохранение", yPos,
            s != null ? s.AutoSaveEnabled : true,
            (val) => { if (s != null) { s.AutoSaveEnabled = val; s.Save(); } });

        yPos = BuildToggleRow(content, "\u2714  Подтверждение хода", yPos,
            s != null ? s.MoveConfirmation : false,
            (val) => { if (s != null) { s.MoveConfirmation = val; s.Save(); } });

        yPos = BuildDropdownRow(content, "\uD83D\uDCCA  Сложность по умолчанию", yPos,
            new string[] { "Лёгкая", "Средняя" },
            s != null ? s.DefaultDifficulty : 1,
            (idx) => { if (s != null) { s.DefaultDifficulty = idx; s.Save(); } });

        yPos = BuildDropdownRow(content, "\u23F0  Время на ход", yPos,
            new string[] { "5 мин", "10 мин", "15 мин", "30 мин" },
            s != null ? s.DefaultMoveTime : 1,
            (idx) => { if (s != null) { s.DefaultMoveTime = idx; s.Save(); } });

        yPos -= 20f;

        // ДРУГОЕ
        yPos = BuildSectionHeader(content, "ДРУГОЕ", yPos);

        yPos = BuildSettingRowWithOptions(content, "\uD83C\uDF10  Язык / Language", yPos,
            SettingsManager.LanguageNames,
            s != null ? s.Language : 0,
            (idx) => {
                if (s != null) { s.Language = idx; s.Save(); }
                HideAll(); BuildTabLayout();
            });

        yPos = BuildNavigationRow(content, "\u21BA  Сбросить настройки", yPos, () => {
            PlayerPrefs.DeleteAll();
            if (s != null) s.Load();
            HideAll(); BuildTabLayout();
        });

        yPos = BuildNavigationRow(content, "\u24D8  О приложении", yPos, () => {
            ShowDialog("О приложении",
                "Caless v1.0\n\nСтратегическая настольная игра\nс 12 уникальными фигурами\nна доске 10\u00D710.\n\n\u00A9 2025",
                "Закрыть", () => { HideAll(); BuildTabLayout(); });
        });

        yPos -= 40f;

        RectTransform contentRT = content.GetComponent<RectTransform>();
        contentRT.sizeDelta = new Vector2(0, Mathf.Abs(yPos) + 20);
    }

    // ==================== SETTINGS HELPERS ====================

    private float BuildSectionHeader(Transform parent, string title, float yPos)
    {
        float height = 50f;
        GameObject header = new GameObject("Section_" + title);
        header.transform.SetParent(parent, false);
        RectTransform rt = header.AddComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0, 1);
        rt.anchorMax        = new Vector2(1, 1);
        rt.pivot            = new Vector2(0.5f, 1);
        rt.anchoredPosition = new Vector2(0, yPos);
        rt.sizeDelta        = new Vector2(0, height);
        rt.offsetMin = new Vector2(20, rt.offsetMin.y);
        rt.offsetMax = new Vector2(-20, rt.offsetMax.y);
        CreateText(title, header.transform,
            Vector2.zero, Vector2.one, 24, TEXT_DIM, TextAnchor.MiddleLeft, FontStyle.Bold);
        return yPos - height;
    }

    private float BuildToggleRow(Transform content, string label, float yPos, bool value, Action<bool> onChange)
    {
        float height = 80f;
        GameObject row = CreateSettingRow(content, label, yPos, height);

        GameObject toggle = new GameObject("Toggle");
        toggle.transform.SetParent(row.transform, false);
        RectTransform toggleRT = toggle.AddComponent<RectTransform>();
        toggleRT.anchorMin = new Vector2(0.82f, 0.25f);
        toggleRT.anchorMax = new Vector2(0.97f, 0.75f);
        toggleRT.offsetMin = toggleRT.offsetMax = Vector2.zero;
        toggle.AddComponent<Image>().color = value ? TOGGLE_ON : TOGGLE_OFF;

        Button toggleBtn = toggle.AddComponent<Button>();
        bool   cur = value;
        toggleBtn.onClick.AddListener(() => {
            onChange?.Invoke(!cur);
            HideAll(); currentTab = Tab.Settings; BuildTabLayout();
        });

        GameObject knob = new GameObject("Knob");
        knob.transform.SetParent(toggle.transform, false);
        RectTransform knobRT = knob.AddComponent<RectTransform>();
        knobRT.anchorMin = value ? new Vector2(0.55f, 0.10f) : new Vector2(0.05f, 0.10f);
        knobRT.anchorMax = value ? new Vector2(0.95f, 0.90f) : new Vector2(0.45f, 0.90f);
        knobRT.offsetMin = knobRT.offsetMax = Vector2.zero;
        knob.AddComponent<Image>().color = value ? Color.white : new Color(0.5f, 0.48f, 0.45f);

        return yPos - height - 4;
    }

    private float BuildSliderRow(Transform content, string label, float yPos, float value, Action<float> onChange)
    {
        float height = 80f;
        GameObject row = CreateSettingRow(content, label, yPos, height);

        int percent = Mathf.RoundToInt(value * 100f);
        CreateText(percent + "%", row.transform,
            new Vector2(0.85f, 0.50f), new Vector2(0.97f, 0.90f),
            22, ACCENT_GOLD, TextAnchor.MiddleRight);

        GameObject sliderBg = new GameObject("SliderBg");
        sliderBg.transform.SetParent(row.transform, false);
        RectTransform sbRT = sliderBg.AddComponent<RectTransform>();
        sbRT.anchorMin = new Vector2(0.40f, 0.15f);
        sbRT.anchorMax = new Vector2(0.83f, 0.45f);
        sbRT.offsetMin = sbRT.offsetMax = Vector2.zero;
        sliderBg.AddComponent<Image>().color = SLIDER_BG;

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(sliderBg.transform, false);
        RectTransform fillRT = fill.AddComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = new Vector2(value, 1);
        fillRT.offsetMin = fillRT.offsetMax = Vector2.zero;
        fill.AddComponent<Image>().color = SLIDER_FILL;

        GameObject knob = new GameObject("Knob");
        knob.transform.SetParent(sliderBg.transform, false);
        RectTransform knobRT = knob.AddComponent<RectTransform>();
        knobRT.anchorMin = new Vector2(value - 0.03f, -0.3f);
        knobRT.anchorMax = new Vector2(value + 0.03f,  1.3f);
        knobRT.offsetMin = knobRT.offsetMax = Vector2.zero;
        knob.AddComponent<Image>().color = ACCENT_LIGHT;

        CreateSmallButton("-", row.transform,
            new Vector2(0.36f, 0.15f), new Vector2(0.40f, 0.45f), BTN_GRAY, ACCENT_LIGHT,
            () => { onChange?.Invoke(Mathf.Clamp01(value - 0.1f));
                    HideAll(); currentTab = Tab.Settings; BuildTabLayout(); });

        CreateSmallButton("+", row.transform,
            new Vector2(0.83f, 0.15f), new Vector2(0.87f, 0.45f), BTN_GRAY, ACCENT_LIGHT,
            () => { onChange?.Invoke(Mathf.Clamp01(value + 0.1f));
                    HideAll(); currentTab = Tab.Settings; BuildTabLayout(); });

        return yPos - height - 4;
    }

    private float BuildSettingRowWithOptions(Transform content, string label, float yPos,
        string[] options, int selected, Action<int> onChange)
    {
        float height = 80f;
        GameObject row = CreateSettingRow(content, label, yPos, height);

        float btnWidth = 0.55f / options.Length;
        for (int i = 0; i < options.Length; i++)
        {
            bool isSelected = (i == selected);
            int  capturedI  = i;
            float xStart = 0.43f + i * btnWidth;
            CreateSmallButton(options[i], row.transform,
                new Vector2(xStart, 0.15f), new Vector2(xStart + btnWidth - 0.01f, 0.85f),
                isSelected ? BTN_ORANGE : BTN_GRAY,
                isSelected ? Color.white : TEXT_DIM,
                () => { onChange?.Invoke(capturedI); HideAll(); currentTab = Tab.Settings; BuildTabLayout(); });
        }
        return yPos - height - 4;
    }

    private float BuildSettingRowWithColorSwatches(Transform content, string label, float yPos,
        Color[] colors, int selected, Action<int> onChange)
    {
        float height = 80f;
        GameObject row = CreateSettingRow(content, label, yPos, height);

        float swatchW = 0.55f / colors.Length;
        for (int i = 0; i < colors.Length; i++)
        {
            int  capturedI  = i;
            bool isSelected = (i == selected);
            float xStart = 0.43f + i * swatchW;

            GameObject swatch = new GameObject("Swatch_" + i);
            swatch.transform.SetParent(row.transform, false);
            RectTransform swRT = swatch.AddComponent<RectTransform>();
            swRT.anchorMin = new Vector2(xStart, 0.2f);
            swRT.anchorMax = new Vector2(xStart + swatchW - 0.01f, 0.8f);
            swRT.offsetMin = swRT.offsetMax = Vector2.zero;
            swatch.AddComponent<Image>().color = colors[i];

            if (isSelected)
            {
                Outline sel = swatch.AddComponent<Outline>();
                sel.effectColor    = Color.white;
                sel.effectDistance = new Vector2(3, 3);
            }

            swatch.AddComponent<Button>().onClick.AddListener(() => {
                onChange?.Invoke(capturedI);
                HideAll(); currentTab = Tab.Settings; BuildTabLayout();
            });
        }
        return yPos - height - 4;
    }

    private float BuildDropdownRow(Transform content, string label, float yPos,
        string[] options, int selected, Action<int> onChange)
    {
        float height = 80f;
        GameObject row = CreateSettingRow(content, label, yPos, height);
        string displayText = (selected >= 0 && selected < options.Length) ? options[selected] : "-";

        GameObject dropBtn = new GameObject("Dropdown");
        dropBtn.transform.SetParent(row.transform, false);
        RectTransform dropRT = dropBtn.AddComponent<RectTransform>();
        dropRT.anchorMin = new Vector2(0.60f, 0.15f);
        dropRT.anchorMax = new Vector2(0.97f, 0.85f);
        dropRT.offsetMin = dropRT.offsetMax = Vector2.zero;
        dropBtn.AddComponent<Image>().color = BTN_GRAY;

        int capturedSelected = selected;
        dropBtn.AddComponent<Button>().onClick.AddListener(() => {
            int next = (capturedSelected + 1) % options.Length;
            onChange?.Invoke(next);
            HideAll(); currentTab = Tab.Settings; BuildTabLayout();
        });

        CreateText(displayText, dropBtn.transform,
            new Vector2(0.05f, 0), new Vector2(0.85f, 1), 22, ACCENT_GOLD, TextAnchor.MiddleLeft);
        CreateText("\u25BE", dropBtn.transform,
            new Vector2(0.80f, 0), new Vector2(1, 1), 22, ACCENT_GOLD, TextAnchor.MiddleCenter);

        return yPos - height - 4;
    }

    private float BuildNavigationRow(Transform content, string label, float yPos, Action onClick)
    {
        float height = 70f;
        GameObject row = CreateSettingRow(content, label, yPos, height);
        if (onClick != null)
            row.AddComponent<Button>().onClick.AddListener(() => onClick?.Invoke());
        CreateText("\u276F", row.transform,
            new Vector2(0.90f, 0.20f), new Vector2(0.98f, 0.80f),
            24, TEXT_DIM, TextAnchor.MiddleCenter);
        return yPos - height - 4;
    }

    private GameObject CreateSettingRow(Transform parent, string label, float yPos, float height)
    {
        string safeName = label.Length > 30 ? label.Substring(0, 30) : label;
        GameObject row = new GameObject("Row_" + safeName);
        row.transform.SetParent(parent, false);
        RectTransform rt = row.AddComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0, 1);
        rt.anchorMax        = new Vector2(1, 1);
        rt.pivot            = new Vector2(0.5f, 1);
        rt.anchoredPosition = new Vector2(0, yPos);
        rt.sizeDelta        = new Vector2(0, height);
        rt.offsetMin = new Vector2(16, rt.offsetMin.y);
        rt.offsetMax = new Vector2(-16, rt.offsetMax.y);
        row.AddComponent<Image>().color = BG_SECTION;
        CreateText(label, row.transform,
            new Vector2(0.03f, 0.10f), new Vector2(0.60f, 0.90f),
            22, ACCENT_LIGHT, TextAnchor.MiddleLeft);
        return row;
    }

    // ==================== SUBMENU ====================

    public void ShowPlaySubMenu()
    {
        HideAll();
        playSubMenu = CreateFullPanel("PlaySubMenu");
        playSubMenu.GetComponent<Image>().color = BG_DARK;

        CreateText("Caless", playSubMenu.transform,
            new Vector2(0.15f, 0.88f), new Vector2(0.85f, 0.96f),
            52, ACCENT_GOLD, TextAnchor.MiddleCenter, FontStyle.BoldAndItalic);

        CreateText("\u2666  РЕЖИМ ИГРЫ  \u2666", playSubMenu.transform,
            new Vector2(0.10f, 0.80f), new Vector2(0.90f, 0.88f),
            36, ACCENT_GOLD, TextAnchor.MiddleCenter, FontStyle.Bold);

        CreatePremiumButton("Против компьютера", "\uD83D\uDCBB", playSubMenu.transform,
            new Vector2(0.08f, 0.60f), new Vector2(0.92f, 0.74f), BTN_GREEN,
            () => ShowDifficultySelect());

        CreatePremiumButton("2 игрока (локально)", "\u263A", playSubMenu.transform,
            new Vector2(0.08f, 0.45f), new Vector2(0.92f, 0.59f), BTN_BLUE,
            () => gameManager.StartLocalMultiplayer());

        CreatePremiumButton("Bluetooth мультиплеер", "\u2726", playSubMenu.transform,
            new Vector2(0.08f, 0.30f), new Vector2(0.92f, 0.44f), BTN_PURPLE,
            () => gameManager.StartBluetoothGame());

        CreatePremiumButton("Назад", "\u2190", playSubMenu.transform,
            new Vector2(0.20f, 0.12f), new Vector2(0.80f, 0.24f), BTN_GRAY,
            () => ShowMainMenu());
    }

    private void ShowDifficultySelect()
    {
        HideAll();
        difficultyPanel = CreateFullPanel("DifficultySelect");
        difficultyPanel.GetComponent<Image>().color = BG_DARK;

        CreateText("Caless", difficultyPanel.transform,
            new Vector2(0.15f, 0.88f), new Vector2(0.85f, 0.96f),
            52, ACCENT_GOLD, TextAnchor.MiddleCenter, FontStyle.BoldAndItalic);

        CreateText("\u2666  СЛОЖНОСТЬ  \u2666", difficultyPanel.transform,
            new Vector2(0.10f, 0.76f), new Vector2(0.90f, 0.86f),
            36, ACCENT_GOLD, TextAnchor.MiddleCenter, FontStyle.Bold);

        CreatePremiumButton("Лёгкий", "\u2605", difficultyPanel.transform,
            new Vector2(0.10f, 0.55f), new Vector2(0.90f, 0.69f), BTN_GREEN,
            () => ShowSideSelection(CalessAI.Difficulty.Easy));

        CreatePremiumButton("Средний", "\u2605\u2605", difficultyPanel.transform,
            new Vector2(0.10f, 0.38f), new Vector2(0.90f, 0.52f), BTN_ORANGE,
            () => ShowSideSelection(CalessAI.Difficulty.Medium));

        CreatePremiumButton("Назад", "\u2190", difficultyPanel.transform,
            new Vector2(0.20f, 0.18f), new Vector2(0.80f, 0.30f), BTN_GRAY,
            () => ShowPlaySubMenu());
    }

    private void ShowSideSelection(CalessAI.Difficulty difficulty)
    {
        HideAll();
        sidePanel = CreateFullPanel("SideSelect");
        sidePanel.GetComponent<Image>().color = BG_DARK;

        string diffName = difficulty == CalessAI.Difficulty.Easy ? "Лёгкий" : "Средний";

        CreateText("Caless", sidePanel.transform,
            new Vector2(0.15f, 0.88f), new Vector2(0.85f, 0.96f),
            52, ACCENT_GOLD, TextAnchor.MiddleCenter, FontStyle.BoldAndItalic);

        CreateText("Сложность: " + diffName, sidePanel.transform,
            new Vector2(0.10f, 0.80f), new Vector2(0.90f, 0.88f),
            28, TEXT_SUBTITLE, TextAnchor.MiddleCenter);

        CreateText("\u2666  ВЫБЕРИТЕ СТОРОНУ  \u2666", sidePanel.transform,
            new Vector2(0.05f, 0.70f), new Vector2(0.95f, 0.80f),
            34, ACCENT_GOLD, TextAnchor.MiddleCenter, FontStyle.Bold);

        CreatePremiumButton("Белые", "\u2654", sidePanel.transform,
            new Vector2(0.10f, 0.50f), new Vector2(0.90f, 0.64f),
            new Color(0.60f, 0.58f, 0.52f),
            () => gameManager.StartGame(difficulty, GameManager.PlayerSide.White));

        CreatePremiumButton("Чёрные", "\u265A", sidePanel.transform,
            new Vector2(0.10f, 0.33f), new Vector2(0.90f, 0.47f),
            new Color(0.12f, 0.10f, 0.08f),
            () => gameManager.StartGame(difficulty, GameManager.PlayerSide.Black));

        CreatePremiumButton("Назад", "\u2190", sidePanel.transform,
            new Vector2(0.20f, 0.14f), new Vector2(0.80f, 0.26f), BTN_GRAY,
            () => ShowDifficultySelect());
    }

    public void ShowSettings() { HideAll(); currentTab = Tab.Settings; BuildTabLayout(); }

    // ==================== GAME UI ====================

    public void ShowGameUI()
    {
        HideAll();

        // ---- ВЕРХНЯЯ ПАНЕЛЬ: ТАЙМЕРЫ ----
        gameUITopPanel = CreatePanel("GameUITop", canvas.transform);
        RectTransform topRT = gameUITopPanel.GetComponent<RectTransform>();
        topRT.anchorMin = new Vector2(0, 0.82f);
        topRT.anchorMax = new Vector2(1, 1.00f);
        topRT.offsetMin = topRT.offsetMax = Vector2.zero;
        gameUITopPanel.GetComponent<Image>().color = new Color(0.06f, 0.05f, 0.04f, 0.97f);
        AddHorizontalLine(gameUITopPanel.transform, false, BORDER_GOLD);

        // Таймер ЧЁРНЫХ (слева)
        GameObject blackBox = new GameObject("BlackTimer");
        blackBox.transform.SetParent(gameUITopPanel.transform, false);
        RectTransform bbRT = blackBox.AddComponent<RectTransform>();
        bbRT.anchorMin = new Vector2(0.01f, 0.05f);
        bbRT.anchorMax = new Vector2(0.42f, 0.95f);
        bbRT.offsetMin = bbRT.offsetMax = Vector2.zero;
        blackBox.AddComponent<Image>().color = new Color(0.10f, 0.08f, 0.06f, 0.9f);

        CreateText("\u265F ЧЕРНЫЕ", blackBox.transform,
            new Vector2(0.04f, 0.56f), new Vector2(0.96f, 0.96f),
            18, new Color(0.75f, 0.70f, 0.60f), TextAnchor.MiddleCenter, FontStyle.Bold);

        blackTimerText = CreateText("10:00", blackBox.transform,
            new Vector2(0.04f, 0.02f), new Vector2(0.96f, 0.60f),
            36, ACCENT_LIGHT, TextAnchor.MiddleCenter, FontStyle.Bold).GetComponent<Text>();

        // Таймер БЕЛЫХ (справа)
        GameObject whiteBox = new GameObject("WhiteTimer");
        whiteBox.transform.SetParent(gameUITopPanel.transform, false);
        RectTransform wbRT = whiteBox.AddComponent<RectTransform>();
        wbRT.anchorMin = new Vector2(0.58f, 0.05f);
        wbRT.anchorMax = new Vector2(0.99f, 0.95f);
        wbRT.offsetMin = wbRT.offsetMax = Vector2.zero;
        whiteBox.AddComponent<Image>().color = new Color(0.10f, 0.08f, 0.06f, 0.9f);

        CreateText("\u2659 БЕЛЫЕ", whiteBox.transform,
            new Vector2(0.04f, 0.56f), new Vector2(0.96f, 0.96f),
            18, new Color(0.95f, 0.93f, 0.85f), TextAnchor.MiddleCenter, FontStyle.Bold);

        whiteTimerText = CreateText("10:00", whiteBox.transform,
            new Vector2(0.04f, 0.02f), new Vector2(0.96f, 0.60f),
            36, ACCENT_LIGHT, TextAnchor.MiddleCenter, FontStyle.Bold).GetComponent<Text>();

        // Центр: ход + КАЛ / Думаю
        turnText = CreateText("", gameUITopPanel.transform,
            new Vector2(0.43f, 0.52f), new Vector2(0.57f, 0.98f),
            17, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold).GetComponent<Text>();

        statusText = CreateText("", gameUITopPanel.transform,
            new Vector2(0.43f, 0.02f), new Vector2(0.57f, 0.52f),
            19, new Color(1f, 0.3f, 0.3f), TextAnchor.MiddleCenter, FontStyle.Bold).GetComponent<Text>();

        thinkingText = CreateText("", gameUITopPanel.transform,
            new Vector2(0.43f, 0.02f), new Vector2(0.57f, 0.52f),
            15, new Color(0.5f, 0.8f, 1f), TextAnchor.MiddleCenter, FontStyle.Italic).GetComponent<Text>();

        // ---- НИЖНЯЯ ПАНЕЛЬ: ИСТОРИЯ + КНОПКИ ----
        gameUIPanel = CreatePanel("GameUIBottom", canvas.transform);
        RectTransform rt = gameUIPanel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 0.20f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        gameUIPanel.GetComponent<Image>().color = new Color(0.06f, 0.05f, 0.04f, 0.97f);
        AddHorizontalLine(gameUIPanel.transform, true, BORDER_GOLD);

        // Строка истории ходов
        GameObject historyBar = new GameObject("HistoryBar");
        historyBar.transform.SetParent(gameUIPanel.transform, false);
        RectTransform histRT = historyBar.AddComponent<RectTransform>();
        histRT.anchorMin = new Vector2(0, 0.62f);
        histRT.anchorMax = new Vector2(1, 1.00f);
        histRT.offsetMin = histRT.offsetMax = Vector2.zero;
        historyBar.AddComponent<Image>().color = new Color(0.09f, 0.07f, 0.06f, 1f);

        CreateText(Tr("Ходы") + ":", historyBar.transform,
            new Vector2(0.00f, 0.10f), new Vector2(0.12f, 0.90f),
            18, TEXT_DIM, TextAnchor.MiddleCenter);

        // Горизонтальный scroll для ходов
        GameObject histScroll = new GameObject("HistoryScroll");
        histScroll.transform.SetParent(historyBar.transform, false);
        RectTransform hsRT = histScroll.AddComponent<RectTransform>();
        hsRT.anchorMin = new Vector2(0.12f, 0.05f);
        hsRT.anchorMax = new Vector2(1.00f, 0.95f);
        hsRT.offsetMin = hsRT.offsetMax = Vector2.zero;
        histScroll.AddComponent<Image>().color = Color.clear;

        moveHistoryScroll = histScroll.AddComponent<ScrollRect>();
        moveHistoryScroll.horizontal        = true;
        moveHistoryScroll.vertical          = false;
        moveHistoryScroll.movementType      = ScrollRect.MovementType.Clamped;
        moveHistoryScroll.inertia           = true;
        moveHistoryScroll.decelerationRate  = 0.135f;

        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(histScroll.transform, false);
        RectTransform vpRT = viewport.AddComponent<RectTransform>();
        vpRT.anchorMin = Vector2.zero;
        vpRT.anchorMax = Vector2.one;
        vpRT.offsetMin = vpRT.offsetMax = Vector2.zero;
        viewport.AddComponent<Image>().color = Color.clear;
        viewport.AddComponent<Mask>().showMaskGraphic = false;
        moveHistoryScroll.viewport = vpRT;

        GameObject histContent = new GameObject("Content");
        histContent.transform.SetParent(viewport.transform, false);
        RectTransform hcRT = histContent.AddComponent<RectTransform>();
        hcRT.anchorMin = new Vector2(0, 0);
        hcRT.anchorMax = new Vector2(0, 1);
        hcRT.pivot     = new Vector2(0, 0.5f);
        hcRT.offsetMin = Vector2.zero;
        hcRT.sizeDelta = new Vector2(0, 0);
        moveHistoryContent = histContent.transform;
        moveHistoryScroll.content = hcRT;

        // Строка кнопок
        float btnY1 = 0.03f, btnY2 = 0.58f;

        CreateGameButton(Tr("Меню"), gameUIPanel.transform,
            new Vector2(0.01f, btnY1), new Vector2(0.16f, btnY2), BTN_RED,
            () => gameManager.ShowMenu());

        CreateGameButton(Tr("Заново"), gameUIPanel.transform,
            new Vector2(0.17f, btnY1), new Vector2(0.32f, btnY2), BTN_GREEN,
            () => gameManager.RestartGame());

        CreateGameButton("\u21A9 " + Tr("Ход назад"), gameUIPanel.transform,
            new Vector2(0.33f, btnY1), new Vector2(0.50f, btnY2), BTN_GRAY,
            () => gameManager.UndoMove());

        CreateGameButton(Tr("Повернуть"), gameUIPanel.transform,
            new Vector2(0.51f, btnY1), new Vector2(0.66f, btnY2), BTN_BLUE,
            () => gameManager.boardRenderer.FlipBoard());

        CreateGameButton(Tr("Храм"), gameUIPanel.transform,
            new Vector2(0.67f, btnY1), new Vector2(0.82f, btnY2), BTN_PURPLE,
            () => gameManager.ActivateTemple());

        CreateGameButton(Tr("Сдвиг"), gameUIPanel.transform,
            new Vector2(0.83f, btnY1), new Vector2(0.99f, btnY2), BTN_ORANGE,
            () => gameManager.ActivateCastling());

        moveCountText = CreateText("", gameUIPanel.transform,
            new Vector2(0.01f, 0.58f), new Vector2(0.33f, 0.64f),
            15, TEXT_DIM, TextAnchor.MiddleLeft).GetComponent<Text>();

        captureText = CreateText("", gameUIPanel.transform,
            new Vector2(0.33f, 0.58f), new Vector2(1.0f, 0.64f),
            15, ACCENT_GOLD, TextAnchor.MiddleLeft).GetComponent<Text>();

        UpdateGameInfo();
    }

    // ==================== ТАЙМЕР ====================

    public void UpdateTimers(float white, float black, bool whiteTurn)
    {
        if (whiteTimerText == null || blackTimerText == null) return;

        whiteTimerText.text = FormatTime(white);
        blackTimerText.text = FormatTime(black);

        if (whiteTurn)
        {
            whiteTimerText.color = (white < 30f) ? new Color(1f, 0.2f, 0.2f) : ACCENT_GOLD;
            blackTimerText.color = new Color(0.50f, 0.47f, 0.40f);
        }
        else
        {
            blackTimerText.color = (black < 30f) ? new Color(1f, 0.2f, 0.2f) : ACCENT_GOLD;
            whiteTimerText.color = new Color(0.50f, 0.47f, 0.40f);
        }
    }

    private string FormatTime(float seconds)
    {
        int m = Mathf.FloorToInt(seconds / 60f);
        int s = Mathf.FloorToInt(seconds % 60f);
        return m.ToString("D2") + ":" + s.ToString("D2");
    }

    // ==================== ОБНОВЛЕНИЕ ИГРЫ ====================

    public void UpdateGameInfo()
    {
        if (turnText == null) return;

        bool isLocal = gameManager.currentMode == GameManager.GameMode.LocalMultiplayer;

        if (isLocal)
        {
            bool wt = gameManager.engine.whiteTurn;
            turnText.text  = Tr(wt ? "Ход белых" : "Ход чёрных");
            turnText.color = wt ? new Color(0.9f, 0.9f, 0.8f) : ACCENT_GOLD;
        }
        else
        {
            turnText.text  = Tr(gameManager.isPlayerTurn ? "Ваш ход" : "Ход компьютера");
            turnText.color = gameManager.isPlayerTurn ?
                new Color(0.5f, 1f, 0.5f) : new Color(1f, 0.7f, 0.3f);
        }

        if (moveCountText != null)
            moveCountText.text = Tr("Ход") + " " + (gameManager.moveCount / 2 + 1);

        string pCap = FormatCaptures(gameManager.playerCapturedPieces);
        string aCap = FormatCaptures(gameManager.aiCapturedPieces);
        if (captureText != null)
            captureText.text = Tr("Вы") + ": " + (pCap.Length > 0 ? pCap : "\u2014") +
                               "  " + Tr("ИИ") + ": " + (aCap.Length > 0 ? aCap : "\u2014");

        if (statusText != null)
        {
            bool check = gameManager.engine != null &&
                         (gameManager.engine.IsInCheck(true) || gameManager.engine.IsInCheck(false));
            statusText.text = check ? Tr("КАЛ!") : "";
        }

        if (gameManager != null && gameManager.engine != null)
            UpdateTimers(gameManager.whiteTimeLeft, gameManager.blackTimeLeft,
                         gameManager.engine.whiteTurn);
    }

    public void ShowThinking(bool show)
    {
        if (thinkingText != null)
            thinkingText.text = show ? Tr("Думаю...") : "";
        if (statusText != null && show)
            statusText.text = "";
    }

    // ==================== ИСТОРИЯ ХОДОВ ====================

    public void RefreshMoveHistory(List<CalessEngine.MoveRecord> history)
    {
        if (moveHistoryContent == null) return;

        foreach (Transform child in moveHistoryContent)
            UnityEngine.Object.Destroy(child.gameObject);

        if (history == null || history.Count == 0)
        {
            RectTransform crt = moveHistoryContent.GetComponent<RectTransform>();
            crt.sizeDelta = new Vector2(10, 0);
            return;
        }

        float chipW = 148f, chipSpacing = 5f, totalW = 4f;

        for (int i = 0; i < history.Count; i++)
        {
            string notation  = FormatMoveNotation(history[i].move, i + 1);
            bool isWhiteMove = CalessEngine.IsWhite(history[i].move.piece);
            bool isLast      = (i == history.Count - 1);

            GameObject chip = new GameObject("Chip_" + i);
            chip.transform.SetParent(moveHistoryContent, false);
            RectTransform chipRT = chip.AddComponent<RectTransform>();
            chipRT.anchorMin        = new Vector2(0, 0);
            chipRT.anchorMax        = new Vector2(0, 1);
            chipRT.pivot            = new Vector2(0, 0.5f);
            chipRT.anchoredPosition = new Vector2(totalW, 0);
            chipRT.sizeDelta        = new Vector2(chipW, 0);

            chip.AddComponent<Image>().color = isWhiteMove ?
                new Color(0.20f, 0.18f, 0.14f) : new Color(0.14f, 0.12f, 0.10f);

            if (isLast)
            {
                Outline o = chip.AddComponent<Outline>();
                o.effectColor    = ACCENT_GOLD;
                o.effectDistance = new Vector2(2, 2);
            }

            CreateText(notation, chip.transform,
                Vector2.zero, Vector2.one,
                14, isWhiteMove ? ACCENT_LIGHT : new Color(0.72f, 0.67f, 0.57f),
                TextAnchor.MiddleCenter);

            totalW += chipW + chipSpacing;
        }

        RectTransform contentRT = moveHistoryContent.GetComponent<RectTransform>();
        contentRT.sizeDelta = new Vector2(totalW, 0);

        if (moveHistoryScroll != null)
        {
            Canvas.ForceUpdateCanvases();
            moveHistoryScroll.horizontalNormalizedPosition = 1f;
        }
    }

    private string FormatMoveNotation(Move m, int moveNum)
    {
        int    type = CalessEngine.PieceType(m.piece);
        string name = CalessEngine.PieceNames.ContainsKey(type) ? CalessEngine.PieceNames[type] : "?";
        if (name.Length > 3) name = name.Substring(0, 3);

        char fromCol = (char)('a' + m.from.y);
        char toCol   = (char)('a' + m.to.y);
        string from  = fromCol.ToString() + (m.from.x + 1);
        string to    = toCol.ToString()   + (m.to.x + 1);

        string prefix = moveNum.ToString() + ".";
        string cap    = (m.captured != CalessEngine.EMPTY) ? "x" : "-";

        if (m.isCastling) return prefix + "0-0";
        if (m.isTeleport) return prefix + name + "\u2605" + to;
        if (m.isRevive)   return prefix + name + "\u2191";
        return prefix + name + " " + from + cap + to;
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

    // ==================== GAME OVER ====================

    public void ShowGameOver(string title, string message)
    {
        if (gameOverPanel != null) Destroy(gameOverPanel);

        gameOverPanel = CreatePanel("GameOver", canvas.transform);
        RectTransform rt = gameOverPanel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.05f, 0.28f);
        rt.anchorMax = new Vector2(0.95f, 0.72f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        gameOverPanel.GetComponent<Image>().color = new Color(0.05f, 0.04f, 0.03f, 0.98f);

        Outline outline = gameOverPanel.AddComponent<Outline>();
        outline.effectColor    = BORDER_GOLD;
        outline.effectDistance = new Vector2(3, 3);

        CreateText(title, gameOverPanel.transform,
            new Vector2(0.05f, 0.68f), new Vector2(0.95f, 0.92f),
            48, ACCENT_GOLD, TextAnchor.MiddleCenter, FontStyle.Bold);

        CreateText(message, gameOverPanel.transform,
            new Vector2(0.05f, 0.42f), new Vector2(0.95f, 0.68f),
            30, ACCENT_LIGHT, TextAnchor.MiddleCenter);

        CreatePremiumButton("Заново", "\u21BA", gameOverPanel.transform,
            new Vector2(0.08f, 0.10f), new Vector2(0.48f, 0.32f), BTN_GREEN,
            () => { Destroy(gameOverPanel); gameManager.RestartGame(); });

        CreatePremiumButton("Меню", "\u2302", gameOverPanel.transform,
            new Vector2(0.52f, 0.10f), new Vector2(0.92f, 0.32f), BTN_GRAY,
            () => { Destroy(gameOverPanel); gameManager.ShowMenu(); });
    }

    // ==================== REVIVE DIALOG ====================

    public void ShowReviveDialog(string pieceName, Action onRevive, Action onSkip)
    {
        ShowTwoButtonDialog("Оживление",
            "Козёл может оживить: " + pieceName + "\nИспользовать способность?",
            "Оживить", onRevive, "Пропустить", onSkip);
    }

    // ==================== TRAINING ====================

    public void ShowTrainingUI(Action<int> onPieceSelected, Action onBack, Action onClear,
        Action onRandomEnemy, Action onNextPuzzle, Action onPrevPuzzle,
        int puzzleIndex, int totalPuzzles, string puzzleDescription)
    {
        HideAll();
        trainingUIPanel = CreatePanel("TrainingUI", canvas.transform);
        RectTransform rt = trainingUIPanel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 0.20f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        trainingUIPanel.GetComponent<Image>().color = new Color(0.06f, 0.05f, 0.04f, 0.96f);
        AddHorizontalLine(trainingUIPanel.transform, true, BORDER_GOLD);

        CreateText(puzzleDescription, trainingUIPanel.transform,
            new Vector2(0.02f, 0.68f), new Vector2(0.98f, 0.96f),
            24, ACCENT_LIGHT, TextAnchor.MiddleCenter);

        CreateText("Задача " + (puzzleIndex + 1) + " / " + totalPuzzles, trainingUIPanel.transform,
            new Vector2(0.02f, 0.52f), new Vector2(0.98f, 0.68f),
            22, ACCENT_GOLD, TextAnchor.MiddleCenter);

        float btnY1 = 0.04f, btnY2 = 0.48f;
        CreateGameButton("Назад",  trainingUIPanel.transform, new Vector2(0.01f, btnY1), new Vector2(0.19f, btnY2), BTN_RED,    onBack);
        CreateGameButton("< Пред", trainingUIPanel.transform, new Vector2(0.21f, btnY1), new Vector2(0.39f, btnY2), BTN_GRAY,   onPrevPuzzle);
        CreateGameButton("След >", trainingUIPanel.transform, new Vector2(0.41f, btnY1), new Vector2(0.59f, btnY2), BTN_GRAY,   onNextPuzzle);
        CreateGameButton("Враги",  trainingUIPanel.transform, new Vector2(0.61f, btnY1), new Vector2(0.79f, btnY2), BTN_ORANGE, onRandomEnemy);
        CreateGameButton("Сброс",  trainingUIPanel.transform, new Vector2(0.81f, btnY1), new Vector2(0.99f, btnY2), BTN_BLUE,   onClear);
    }

    // ==================== BLUETOOTH ====================

    public void ShowBluetoothUI(Action onHost, Action onJoin, Action onBack)
    {
        HideAll();
        menuPanel = CreateFullPanel("BluetoothMenu");
        menuPanel.GetComponent<Image>().color = BG_DARK;

        CreateText("Caless", menuPanel.transform,
            new Vector2(0.15f, 0.88f), new Vector2(0.85f, 0.96f),
            52, ACCENT_GOLD, TextAnchor.MiddleCenter, FontStyle.BoldAndItalic);

        CreateText("\u2666  BLUETOOTH  \u2666", menuPanel.transform,
            new Vector2(0.05f, 0.78f), new Vector2(0.95f, 0.88f),
            36, ACCENT_GOLD, TextAnchor.MiddleCenter, FontStyle.Bold);

        CreateText("Убедитесь, что оба устройства\nсопряжены через Bluetooth.", menuPanel.transform,
            new Vector2(0.08f, 0.67f), new Vector2(0.92f, 0.77f),
            22, TEXT_SUBTITLE, TextAnchor.MiddleCenter);

        BuildInfoCard(menuPanel.transform, "\uD83D\uDCE1", "Создать игру",
            "Вы станете хостом и ждёте подключения",
            new Vector2(0.05f, 0.47f), new Vector2(0.95f, 0.65f),
            BTN_GREEN, onHost);

        BuildInfoCard(menuPanel.transform, "\uD83D\uDD17", "Присоединиться",
            "Поиск хоста среди сопряжённых устройств",
            new Vector2(0.05f, 0.27f), new Vector2(0.95f, 0.45f),
            BTN_BLUE, onJoin);

        CreatePremiumButton("Назад", "\u2190", menuPanel.transform,
            new Vector2(0.20f, 0.10f), new Vector2(0.80f, 0.22f), BTN_GRAY, onBack);
    }

    public void ShowBluetoothWaiting(BluetoothManager bt, bool isHost, Action onCancel)
    {
        if (bluetoothWaitingPanel != null) Destroy(bluetoothWaitingPanel);
        if (menuPanel != null) { Destroy(menuPanel); menuPanel = null; }

        bluetoothWaitingPanel = CreateFullPanel("BluetoothWaiting");
        bluetoothWaitingPanel.GetComponent<Image>().color = BG_DARK;

        CreateText("Caless", bluetoothWaitingPanel.transform,
            new Vector2(0.15f, 0.88f), new Vector2(0.85f, 0.96f),
            52, ACCENT_GOLD, TextAnchor.MiddleCenter, FontStyle.BoldAndItalic);

        string roleTitle = isHost ? "\uD83D\uDCE1  ОЖИДАНИЕ ПОДКЛЮЧЕНИЯ" : "\uD83D\uDD17  ПОИСК ХОСТА";
        CreateText(roleTitle, bluetoothWaitingPanel.transform,
            new Vector2(0.05f, 0.76f), new Vector2(0.95f, 0.88f),
            32, ACCENT_GOLD, TextAnchor.MiddleCenter, FontStyle.Bold);

        CreateText("\u2022  \u2022  \u2022", bluetoothWaitingPanel.transform,
            new Vector2(0.30f, 0.62f), new Vector2(0.70f, 0.74f),
            46, ACCENT_GOLD_DIM, TextAnchor.MiddleCenter);

        bluetoothStatusText = CreateText(
            bt != null ? bt.StatusMessage : "Инициализация...",
            bluetoothWaitingPanel.transform,
            new Vector2(0.05f, 0.48f), new Vector2(0.95f, 0.62f),
            26, ACCENT_LIGHT, TextAnchor.MiddleCenter).GetComponent<Text>();

        CreateText("Убедитесь, что Bluetooth включён\nи устройства сопряжены.", bluetoothWaitingPanel.transform,
            new Vector2(0.08f, 0.33f), new Vector2(0.92f, 0.48f),
            22, TEXT_DIM, TextAnchor.MiddleCenter);

        CreatePremiumButton("Отмена", "\u2715", bluetoothWaitingPanel.transform,
            new Vector2(0.20f, 0.14f), new Vector2(0.80f, 0.26f), BTN_RED, onCancel);
    }

    public void UpdateBluetoothStatus(string status)
    {
        if (bluetoothStatusText != null)
            bluetoothStatusText.text = status;
    }

    private void BuildInfoCard(Transform parent, string icon, string title, string subtitle,
        Vector2 anchorMin, Vector2 anchorMax, Color bgColor, Action onClick)
    {
        GameObject card = new GameObject("InfoCard_" + title);
        card.transform.SetParent(parent, false);
        RectTransform rt = card.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        card.AddComponent<Image>().color = bgColor;

        Outline o = card.AddComponent<Outline>();
        o.effectColor    = BORDER_GOLD;
        o.effectDistance = new Vector2(2, 2);

        card.AddComponent<Button>().onClick.AddListener(() => onClick?.Invoke());

        CreateText(icon, card.transform,
            new Vector2(0.02f, 0.10f), new Vector2(0.18f, 0.90f),
            36, Color.white, TextAnchor.MiddleCenter);
        CreateText(title, card.transform,
            new Vector2(0.20f, 0.52f), new Vector2(0.90f, 0.94f),
            26, Color.white, TextAnchor.MiddleLeft, FontStyle.Bold);
        CreateText(subtitle, card.transform,
            new Vector2(0.20f, 0.08f), new Vector2(0.90f, 0.52f),
            20, TEXT_DIM, TextAnchor.MiddleLeft);
        CreateText("\u276F", card.transform,
            new Vector2(0.90f, 0.25f), new Vector2(0.98f, 0.75f),
            26, ACCENT_GOLD, TextAnchor.MiddleCenter);
    }

    // ==================== DIALOGS ====================

    public void ShowDialog(string title, string message, string btnText, Action onConfirm)
    {
        if (dialogPanel != null) Destroy(dialogPanel);

        dialogPanel = CreatePanel("Dialog", canvas.transform);
        RectTransform rt = dialogPanel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.05f, 0.28f);
        rt.anchorMax = new Vector2(0.95f, 0.72f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        dialogPanel.GetComponent<Image>().color = new Color(0.05f, 0.04f, 0.03f, 0.98f);

        Outline outline = dialogPanel.AddComponent<Outline>();
        outline.effectColor    = BORDER_GOLD;
        outline.effectDistance = new Vector2(3, 3);

        CreateText(title, dialogPanel.transform,
            new Vector2(0.05f, 0.70f), new Vector2(0.95f, 0.92f),
            40, ACCENT_GOLD, TextAnchor.MiddleCenter, FontStyle.Bold);

        CreateText(message, dialogPanel.transform,
            new Vector2(0.05f, 0.32f), new Vector2(0.95f, 0.70f),
            26, ACCENT_LIGHT, TextAnchor.MiddleCenter);

        CreatePremiumButton(btnText, "\u2714", dialogPanel.transform,
            new Vector2(0.20f, 0.08f), new Vector2(0.80f, 0.28f), BTN_GREEN,
            () => { Destroy(dialogPanel); onConfirm?.Invoke(); });
    }

    private void ShowTwoButtonDialog(string title, string message,
        string btn1Text, Action onBtn1, string btn2Text, Action onBtn2)
    {
        if (dialogPanel != null) Destroy(dialogPanel);

        dialogPanel = CreatePanel("TwoButtonDialog", canvas.transform);
        RectTransform rt = dialogPanel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.05f, 0.25f);
        rt.anchorMax = new Vector2(0.95f, 0.75f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        dialogPanel.GetComponent<Image>().color = new Color(0.05f, 0.04f, 0.03f, 0.98f);

        Outline outline = dialogPanel.AddComponent<Outline>();
        outline.effectColor    = BORDER_GOLD;
        outline.effectDistance = new Vector2(3, 3);

        CreateText(title, dialogPanel.transform,
            new Vector2(0.05f, 0.74f), new Vector2(0.95f, 0.96f),
            36, ACCENT_GOLD, TextAnchor.MiddleCenter, FontStyle.Bold);

        CreateText(message, dialogPanel.transform,
            new Vector2(0.05f, 0.36f), new Vector2(0.95f, 0.74f),
            24, ACCENT_LIGHT, TextAnchor.MiddleCenter);

        CreatePremiumButton(btn1Text, "\u2714", dialogPanel.transform,
            new Vector2(0.05f, 0.06f), new Vector2(0.48f, 0.28f), BTN_GREEN,
            () => { Destroy(dialogPanel); onBtn1?.Invoke(); });

        CreatePremiumButton(btn2Text, "\u2715", dialogPanel.transform,
            new Vector2(0.52f, 0.06f), new Vector2(0.95f, 0.28f), BTN_GRAY,
            () => { Destroy(dialogPanel); onBtn2?.Invoke(); });
    }

    // ==================== HIDE ALL ====================

    private void HideAll()
    {
        if (menuPanel          != null) { Destroy(menuPanel);          menuPanel = null; }
        if (playSubMenu        != null) { Destroy(playSubMenu);        playSubMenu = null; }
        if (settingsPanel      != null) { Destroy(settingsPanel);      settingsPanel = null; }
        if (gameUIPanel        != null) { Destroy(gameUIPanel);        gameUIPanel = null; }
        if (gameUITopPanel     != null) { Destroy(gameUITopPanel);     gameUITopPanel = null; }
        if (gameOverPanel      != null) { Destroy(gameOverPanel);      gameOverPanel = null; }
        if (trainingUIPanel    != null) { Destroy(trainingUIPanel);    trainingUIPanel = null; }
        if (difficultyPanel    != null) { Destroy(difficultyPanel);    difficultyPanel = null; }
        if (sidePanel          != null) { Destroy(sidePanel);          sidePanel = null; }
        if (dialogPanel        != null) { Destroy(dialogPanel);        dialogPanel = null; }
        if (bluetoothWaitingPanel != null) { Destroy(bluetoothWaitingPanel); bluetoothWaitingPanel = null; }

        whiteTimerText      = null;
        blackTimerText      = null;
        turnText            = null;
        statusText          = null;
        thinkingText        = null;
        moveCountText       = null;
        captureText         = null;
        moveHistoryScroll   = null;
        moveHistoryContent  = null;
        bluetoothStatusText = null;
    }

    // ==================== UI PRIMITIVES ====================

    private GameObject CreateFullPanel(string name)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(canvas.transform, false);
        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        panel.AddComponent<Image>().color = BG_DARK;
        return panel;
    }

    private GameObject CreatePanel(string name, Transform parent)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        panel.AddComponent<RectTransform>();
        panel.AddComponent<Image>().color = BG_PANEL;
        return panel;
    }

    private GameObject CreateText(string text, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax,
        int fontSize, Color color,
        TextAnchor alignment = TextAnchor.MiddleCenter,
        FontStyle fontStyle  = FontStyle.Normal)
    {
        string safeName = text.Length > 20 ? text.Substring(0, 20) : text;
        GameObject obj = new GameObject("T_" + safeName);
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        Text t = obj.AddComponent<Text>();
        t.text               = text;
        t.font               = cachedFont;
        t.fontSize           = fontSize;
        t.color              = color;
        t.alignment          = alignment;
        t.fontStyle          = fontStyle;
        t.supportRichText    = false;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow   = VerticalWrapMode.Overflow;
        t.raycastTarget      = false;
        return obj;
    }

    private GameObject CreateImageElement(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax,
        Sprite sprite, Color tint, bool preserveAspect)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        Image img = obj.AddComponent<Image>();
        img.sprite         = sprite;
        img.color          = tint;
        img.preserveAspect = preserveAspect;
        img.raycastTarget  = false;
        return obj;
    }

    private void CreateGameButton(string label, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax,
        Color bgColor, Action onClick)
    {
        GameObject btn = new GameObject("Btn_" + label);
        btn.transform.SetParent(parent, false);
        RectTransform rt = btn.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        btn.AddComponent<Image>().color = bgColor;

        Button b = btn.AddComponent<Button>();
        ColorBlock cb = b.colors;
        cb.normalColor      = Color.white;
        cb.highlightedColor = new Color(1.2f, 1.2f, 1.2f);
        cb.pressedColor     = new Color(0.75f, 0.75f, 0.75f);
        b.colors = cb;
        b.onClick.AddListener(() => onClick?.Invoke());

        Text t = btn.AddComponent<Text>();
        t.text               = label;
        t.font               = cachedFont;
        t.fontSize           = 17;
        t.color              = Color.white;
        t.alignment          = TextAnchor.MiddleCenter;
        t.fontStyle          = FontStyle.Bold;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow   = VerticalWrapMode.Overflow;
    }

    private void CreatePremiumButton(string label, string icon, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax, Color bgColor, Action onClick)
    {
        GameObject btn = new GameObject("PBtn_" + label);
        btn.transform.SetParent(parent, false);
        RectTransform rt = btn.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        btn.AddComponent<Image>().color = bgColor;

        Outline o = btn.AddComponent<Outline>();
        o.effectColor    = BORDER_GOLD;
        o.effectDistance = new Vector2(2, 2);

        Button b = btn.AddComponent<Button>();
        ColorBlock cb = b.colors;
        cb.normalColor      = Color.white;
        cb.highlightedColor = new Color(1.15f, 1.15f, 1.15f);
        cb.pressedColor     = new Color(0.8f, 0.8f, 0.8f);
        b.colors = cb;
        b.onClick.AddListener(() => onClick?.Invoke());

        CreateText(icon, btn.transform,
            new Vector2(0.02f, 0.05f), new Vector2(0.18f, 0.95f),
            32, ACCENT_GOLD, TextAnchor.MiddleCenter);
        CreateText(label, btn.transform,
            new Vector2(0.20f, 0.05f), new Vector2(0.92f, 0.95f),
            26, Color.white, TextAnchor.MiddleLeft, FontStyle.Bold);
        CreateText("\u276F", btn.transform,
            new Vector2(0.88f, 0.20f), new Vector2(0.98f, 0.80f),
            22, ACCENT_GOLD_DIM, TextAnchor.MiddleCenter);
    }

    private void CreateSmallButton(string label, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax,
        Color bgColor, Color textColor, Action onClick)
    {
        GameObject btn = new GameObject("SmBtn_" + label);
        btn.transform.SetParent(parent, false);
        RectTransform rt = btn.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        btn.AddComponent<Image>().color = bgColor;
        btn.AddComponent<Button>().onClick.AddListener(() => onClick?.Invoke());

        Text t = btn.AddComponent<Text>();
        t.text               = label;
        t.font               = cachedFont;
        t.fontSize           = 20;
        t.color              = textColor;
        t.alignment          = TextAnchor.MiddleCenter;
        t.fontStyle          = FontStyle.Bold;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow   = VerticalWrapMode.Overflow;
    }

    private void CreateDecorativeLine(Transform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject line = new GameObject("DecoLine");
        line.transform.SetParent(parent, false);
        RectTransform rt = line.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        line.AddComponent<Image>().color = BORDER_GOLD;
    }

    private void AddHorizontalLine(Transform parent, bool isTop, Color color)
    {
        GameObject line = new GameObject(isTop ? "TopLine" : "BottomLine");
        line.transform.SetParent(parent, false);
        RectTransform rt = line.AddComponent<RectTransform>();
        float y = isTop ? 1f : 0f;
        rt.anchorMin = new Vector2(0, y);
        rt.anchorMax = new Vector2(1, y);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.sizeDelta = new Vector2(0, 2);
        rt.pivot     = new Vector2(0.5f, isTop ? 0f : 1f);
        line.AddComponent<Image>().color = color;
    }

    private GameObject CreateScrollView(Transform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject scrollObj = new GameObject("ScrollView");
        scrollObj.transform.SetParent(parent, false);
        RectTransform scrollRT = scrollObj.AddComponent<RectTransform>();
        scrollRT.anchorMin = anchorMin;
        scrollRT.anchorMax = anchorMax;
        scrollRT.offsetMin = scrollRT.offsetMax = Vector2.zero;
        scrollObj.AddComponent<Image>().color = Color.clear;

        ScrollRect sr = scrollObj.AddComponent<ScrollRect>();
        sr.horizontal        = false;
        sr.vertical          = true;
        sr.movementType      = ScrollRect.MovementType.Elastic;
        sr.scrollSensitivity = 30f;

        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollObj.transform, false);
        RectTransform vpRT = viewport.AddComponent<RectTransform>();
        vpRT.anchorMin = Vector2.zero;
        vpRT.anchorMax = Vector2.one;
        vpRT.offsetMin = vpRT.offsetMax = Vector2.zero;
        viewport.AddComponent<Image>().color = Color.clear;
        viewport.AddComponent<Mask>().showMaskGraphic = false;
        sr.viewport = vpRT;

        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRT = content.AddComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot     = new Vector2(0.5f, 1);
        contentRT.offsetMin = contentRT.offsetMax = Vector2.zero;
        contentRT.sizeDelta = Vector2.zero;
        sr.content = contentRT;

        return scrollObj;
    }

    public void ShowKalHighlight(int row, int col) { }
}
