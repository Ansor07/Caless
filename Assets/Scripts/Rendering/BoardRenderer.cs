using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
/// <summary>
/// Отрисовка доски 10×10, фигур, подсветки и анимаций для Caless.
/// Тёмная фэнтези тема. Поддерживает смену темы через SettingsManager.
/// </summary>
public class BoardRenderer : MonoBehaviour
{
    private const float SQUARE_SIZE = 1f;
    private const float BOARD_OFFSET = -4.5f; // 10 клеток: от -4.5 до 4.5
 
    // Цвета подсветки
    private static readonly Color SELECTED_COLOR = new Color(0.4f, 0.75f, 0.4f, 0.6f);
    private static readonly Color MOVE_HINT_COLOR = new Color(0.3f, 0.6f, 0.3f, 0.5f);
    private static readonly Color CAPTURE_HINT_COLOR = new Color(0.8f, 0.2f, 0.2f, 0.5f);
    private static readonly Color CASTLE_HINT_COLOR = new Color(0.2f, 0.4f, 0.9f, 0.6f);
    private static readonly Color LAST_MOVE_COLOR = new Color(0.9f, 0.85f, 0.3f, 0.35f);
    private static readonly Color TEMPLE_COLOR = new Color(0.6f, 0.2f, 0.8f, 0.4f);
    private static readonly Color TELEPORT_HINT_COLOR = new Color(0.7f, 0.3f, 0.9f, 0.5f);
    private static readonly Color REVIVE_HINT_COLOR = new Color(0.2f, 0.9f, 0.5f, 0.5f);
    private static readonly Color KAL_COLOR = new Color(1f, 0.1f, 0.1f, 0.5f);
 
    private CalessEngine engine;
    private SettingsManager settings;
 
    private GameObject[,] squareObjects = new GameObject[10, 10];
    private GameObject[,] pieceObjects = new GameObject[10, 10];
    private List<GameObject> highlightObjects = new List<GameObject>();
    private List<GameObject> lastMoveHighlights = new List<GameObject>();
    private Vector2Int lastMoveFrom = new Vector2Int(-1, -1);
    private Vector2Int lastMoveTo = new Vector2Int(-1, -1);
 
    public bool isBoardFlipped = false;
    public bool IsAnimating { get; private set; } = false;

    private Transform boardParent;
 
    public void SetSettings(SettingsManager settings)
    {
        this.settings = settings;
    }
    public void Initialize(CalessEngine engine)
    {
        this.engine = engine;

        // ✅ Правильная очистка
        if (boardParent != null)
        {
            Destroy(boardParent.gameObject);
            boardParent = null;
        }
        
        // Очистка массивов
        squareObjects = new GameObject[10, 10];
        pieceObjects = new GameObject[10, 10];
        highlightObjects.Clear();
        lastMoveHighlights.Clear();
        lastMoveFrom = new Vector2Int(-1, -1);
        lastMoveTo = new Vector2Int(-1, -1);

        // ✅ Создание нового parent
        GameObject boardObj = new GameObject("BoardParent");
        boardParent = boardObj.transform;
        boardParent.SetParent(transform);
        
        CreateBoard();
        RefreshPieces();
    }
    private void CreateBoard()
    {
        Color darkSq = (settings != null) ? settings.GetDarkSquareColor() : new Color(0.22f, 0.18f, 0.15f);
        Color lightSq = (settings != null) ? settings.GetLightSquareColor() : new Color(0.45f, 0.38f, 0.30f);
 
        for (int r = 0; r < 10; r++)
        {
            for (int c = 0; c < 10; c++)
            {
                Vector3 pos = GetWorldPosition(r, c);
                bool isLight = (r + c) % 2 == 0;
 
                GameObject square = CreateSprite("Square_" + r + "_" + c, pos,
                    isLight ? lightSq : darkSq, 0);
                square.transform.SetParent(boardParent);
                squareObjects[r, c] = square;
            }
        }
 
        // Подсветка Храма (i5 = row 4, col 8)
        Vector3 templePos = GetWorldPosition(CalessEngine.TEMPLE_SQUARE.x, CalessEngine.TEMPLE_SQUARE.y);
        templePos.z = -0.2f;
        GameObject templeHL = CreateSprite("Temple", templePos, TEMPLE_COLOR, 2);
        templeHL.transform.SetParent(boardParent);
 
        CreateBoardLabels();
    }
 
    private void CreateBoardLabels()
    {
        string[] files = { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j" };
 
        for (int i = 0; i < 10; i++)
        {
            int displayIndex = isBoardFlipped ? (9 - i) : i;
 
            Vector3 rowPos = new Vector3(BOARD_OFFSET - 0.5f, GetWorldPosition(i, 0).y, 0);
            CreateTextLabel((displayIndex + 1).ToString(), rowPos);
 
            Vector3 colPos = new Vector3(GetWorldPosition(0, i).x, BOARD_OFFSET - 0.5f, 0);
            CreateTextLabel(files[displayIndex], colPos);
        }
    }
 
    private void CreateTextLabel(string text, Vector3 position)
    {
        GameObject labelObj = new GameObject("Label_" + text);
        labelObj.transform.SetParent(boardParent);
        labelObj.transform.position = position;
        TextMesh tm = labelObj.AddComponent<TextMesh>();
        tm.text = text;
        tm.fontSize = 32;
        tm.characterSize = 0.12f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = new Color(0.6f, 0.55f, 0.45f);
        tm.GetComponent<MeshRenderer>().sortingOrder = 5;
    }
 
    public void RefreshPieces()
    {
        if (engine == null) return;
 
        for (int r = 0; r < 10; r++)
        {
            for (int c = 0; c < 10; c++)
            {
                if (pieceObjects[r, c] != null)
                {
                    Destroy(pieceObjects[r, c]);
                    pieceObjects[r, c] = null;
                }
            }
        }

        for (int r = 0; r < 10; r++)
        {
            for (int c = 0; c < 10; c++)
            {
                int piece = engine.board[r, c];
                if (piece == CalessEngine.EMPTY) continue;
 
                Vector3 pos = GetWorldPosition(r, c);
                pos.z = -1;
 
                bool isWhite = CalessEngine.IsWhite(piece);
                int type = CalessEngine.PieceType(piece);
 
                Sprite sprite = PieceSpriteGenerator.GetPieceSprite(type, isWhite, PieceSpritesHolder.Sprites);
                if (sprite == null) continue;

                GameObject pieceObj = new GameObject("Piece_" + r + "_" + c);
                pieceObj.transform.SetParent(boardParent);
                pieceObj.transform.position = pos;
 
                SpriteRenderer sr = pieceObj.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.sortingOrder = 10;
 
                float scale = SQUARE_SIZE * 0.8f / sprite.bounds.size.x;
                pieceObj.transform.localScale = Vector3.one * scale;
 
                pieceObjects[r, c] = pieceObj;
            }
        }
 
        foreach (GameObject go in lastMoveHighlights)
        {
            if (go != null) Destroy(go);
        }
        lastMoveHighlights.Clear();
        ShowLastMoveHighlight();
    }
    // ==================== ПОДСВЕТКА ====================
 
    public void ShowSelectedHighlight(int row, int col)
    {
        Vector3 pos = GetWorldPosition(row, col);
        pos.z = -0.5f;
        GameObject hl = CreateSprite("SelectedHL", pos, SELECTED_COLOR, 5);
        hl.transform.SetParent(boardParent);
        highlightObjects.Add(hl);
    }
 
    public void ShowMoveHint(int row, int col, bool isCapture)
    {
        Vector3 pos = GetWorldPosition(row, col);
        pos.z = 0.5f;
        Color color = isCapture ? CAPTURE_HINT_COLOR : MOVE_HINT_COLOR;
        GameObject hint = CreateSprite("MoveHint", pos, color, 5);
        hint.transform.SetParent(boardParent);
 
        if (!isCapture)
            hint.transform.localScale = Vector3.one * 0.35f;
 
        highlightObjects.Add(hint);
    }

    public void ShowCastleHint(int row, int col)
    {
        Vector3 pos = GetWorldPosition(row, col);
        pos.z = -0.5f;
        GameObject hint = CreateSprite("CastleHint", pos, CASTLE_HINT_COLOR, 5);
        hint.transform.SetParent(boardParent);
        highlightObjects.Add(hint);
    }

    public void ShowTeleportHint(int row, int col)
    {
        Vector3 pos = GetWorldPosition(row, col);
        pos.z = -0.5f;
        GameObject hint = CreateSprite("TeleportHint", pos, TELEPORT_HINT_COLOR, 5);
        hint.transform.SetParent(boardParent);
        highlightObjects.Add(hint);
    }
 
    public void ShowReviveHint(int row, int col)
    {
        Vector3 pos = GetWorldPosition(row, col);
        pos.z = -0.5f;
        GameObject hint = CreateSprite("ReviveHint", pos, REVIVE_HINT_COLOR, 5);
        hint.transform.SetParent(boardParent);
        highlightObjects.Add(hint);
    }
 
    public void ShowKalHighlight(int row, int col)
    {
        Vector3 pos = GetWorldPosition(row, col);
        pos.z = -0.4f;
        GameObject hl = CreateSprite("KalHL", pos, KAL_COLOR, 4);
        hl.transform.SetParent(boardParent);
        highlightObjects.Add(hl);
    }
 
    public void ClearHighlights()
    {
        foreach (GameObject go in highlightObjects)
        {
            if (go != null) Destroy(go);
        }
        highlightObjects.Clear();
    }
 
    public void SetLastMove(Vector2Int from, Vector2Int to)
    {   
        lastMoveFrom = from;
        lastMoveTo = to;
        ShowLastMoveHighlight();
    }

    private void ShowLastMoveHighlight()
    {
        if (lastMoveFrom.x < 0) return;
 
        Vector3 posFrom = GetWorldPosition(lastMoveFrom.x, lastMoveFrom.y);
        posFrom.z = -0.3f;
        GameObject hlFrom = CreateSprite("LastMoveFrom", posFrom, LAST_MOVE_COLOR, 3);

        hlFrom.transform.SetParent(boardParent);
        lastMoveHighlights.Add(hlFrom);
 
        Vector3 posTo = GetWorldPosition(lastMoveTo.x, lastMoveTo.y);
        posTo.z = -0.3f;
        GameObject hlTo = CreateSprite("LastMoveTo", posTo, LAST_MOVE_COLOR, 3);
        hlTo.transform.SetParent(boardParent);
        lastMoveHighlights.Add(hlTo);
    }
    // ==================== АНИМАЦИЯ ====================
 
    public Coroutine AnimateMove(Move move, Action onComplete)
    {
        return StartCoroutine(AnimateMoveCoroutine(move, onComplete));
         }
 
    private IEnumerator AnimateMoveCoroutine(Move move, Action onComplete)
    {
        IsAnimating = true;
 
        Vector3 fromPos = GetWorldPosition(move.from.x, move.from.y);
        Vector3 toPos = GetWorldPosition(move.to.x, move.to.y);
        fromPos.z = -2;
        toPos.z = -2;
 
        GameObject pieceObj = pieceObjects[move.from.x, move.from.y];
 
        if (pieceObj == null)
        {
            int type = CalessEngine.PieceType(move.piece);
            bool isWhite = CalessEngine.IsWhite(move.piece);
            Sprite sprite = PieceSpriteGenerator.GetPieceSprite(type, isWhite, PieceSpritesHolder.Sprites);
 
            if (sprite != null)
            {
                pieceObj = new GameObject("AnimPiece");
                pieceObj.transform.SetParent(boardParent);
                pieceObj.transform.position = fromPos;
                SpriteRenderer sr = pieceObj.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.sortingOrder = 20;
                float scale = SQUARE_SIZE * 0.8f / sprite.bounds.size.x;
                pieceObj.transform.localScale = Vector3.one * scale;
            }
        }

        if (pieceObj != null)
        {
            SpriteRenderer psr = pieceObj.GetComponent<SpriteRenderer>();
            if (psr != null) psr.sortingOrder = 20;
 
            float duration = 0.25f;
            float elapsed = 0;
            float arcHeight = 0.3f;
 
            if (move.isTeleport)
            {
                duration = 0.4f;
                arcHeight = 1.5f;
            }
 
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float smoothT = t * t * (3f - 2f * t);
 
                Vector3 pos = Vector3.Lerp(fromPos, toPos, smoothT);
                pos.y += Mathf.Sin(t * Mathf.PI) * arcHeight;
                pieceObj.transform.position = pos;
 
                yield return null;
            }
 
            pieceObj.transform.position = toPos;
 
            if (psr != null) psr.sortingOrder = 10;
        }
 
        IsAnimating = false;
        onComplete?.Invoke();
    }

    public Coroutine AnimateCastling(Move move, Action onComplete)
    {
        return StartCoroutine(AnimateCastlingCoroutine(move, onComplete));
    }
 
    private IEnumerator AnimateCastlingCoroutine(Move move, Action onComplete)
    {
        IsAnimating = true;
 
        Vector3 dragonFrom = GetWorldPosition(move.from.x, move.from.y);
        Vector3 dragonTo = GetWorldPosition(move.to.x, move.to.y);
        Vector3 pieceFrom = GetWorldPosition(move.castlePieceFrom.x, move.castlePieceFrom.y);
        Vector3 pieceTo = GetWorldPosition(move.castlePieceTo.x, move.castlePieceTo.y);
        dragonFrom.z = dragonTo.z = pieceFrom.z = pieceTo.z = -2;
 
        GameObject dragonObj = pieceObjects[move.from.x, move.from.y];
        GameObject castleObj = pieceObjects[move.castlePieceFrom.x, move.castlePieceFrom.y];
 
        float duration = 0.35f;
        float elapsed = 0;
 
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float smoothT = t * t * (3f - 2f * t);
 
            if (dragonObj != null)
            {
                Vector3 pos = Vector3.Lerp(dragonFrom, dragonTo, smoothT);
                pos.y += Mathf.Sin(t * Mathf.PI) * 0.5f;
                dragonObj.transform.position = pos;
            }
            if (castleObj != null)
            {
                Vector3 pos = Vector3.Lerp(pieceFrom, pieceTo, smoothT);
                pos.y += Mathf.Sin(t * Mathf.PI) * 0.5f;
                castleObj.transform.position = pos;
            }
            yield return null;
        }
 
        IsAnimating = false;
        onComplete?.Invoke();
    }
    // ==================== УТИЛИТЫ ====================
 
    public Vector3 GetWorldPosition(int row, int col)
    {
        int displayRow = isBoardFlipped ? (9 - row) : row;
        int displayCol = isBoardFlipped ? (9 - col) : col;
        return new Vector3(
            BOARD_OFFSET + displayCol * SQUARE_SIZE + SQUARE_SIZE * 0.5f,
            BOARD_OFFSET + displayRow * SQUARE_SIZE + SQUARE_SIZE * 0.5f,
            0
        );
    }

    public Vector2Int GetBoardPosition(Vector3 worldPos)
    {
        int displayCol = Mathf.FloorToInt((worldPos.x - BOARD_OFFSET) / SQUARE_SIZE);
        int displayRow = Mathf.FloorToInt((worldPos.y - BOARD_OFFSET) / SQUARE_SIZE);
 
        int row = isBoardFlipped ? (9 - displayRow) : displayRow;
        int col = isBoardFlipped ? (9 - displayCol) : displayCol;
 
        return new Vector2Int(row, col);
    }
 
    public void FlipBoard()
    {
        isBoardFlipped = !isBoardFlipped;
        Initialize(engine);
    }
 
    // Кэшируем один белый 1×1 спрайт, чтобы не плодить Texture2D на каждый вызов
    private static Sprite _whitePixelSprite;
    private static Sprite GetWhitePixelSprite()
    {
        if (_whitePixelSprite == null)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            _whitePixelSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
            _whitePixelSprite.hideFlags = HideFlags.DontSave;
        }
        return _whitePixelSprite;
    }

    private GameObject CreateSprite(string name, Vector3 position, Color color, int sortingOrder)
    {
        GameObject obj = new GameObject(name);
        obj.transform.position = position;

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = GetWhitePixelSprite();
        sr.color = color;
        sr.sortingOrder = sortingOrder;

        return obj;
    }
}