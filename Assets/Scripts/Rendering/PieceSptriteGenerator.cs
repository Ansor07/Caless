using System.Collections.Generic;
using UnityEngine;
 
/// <summary>
/// Процедурная генерация спрайтов для 12 фигур-животных Caless.
/// Чёрные — тёмные с золотым контуром, белые — те же силуэты со свечением.
/// </summary>
public static class PieceSpriteGenerator
{
    private static readonly Color WHITE_FILL = Color.white;
    private static readonly Color BLACK_FILL = Color.black;
    private static readonly Color WHITE_OUTLINE = Color.white;
    private static readonly Color BLACK_OUTLINE = Color.black;
    private static readonly Color WHITE_GLOW = Color.white;

    private static Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

    private const int SIZE = 128;

    public static Sprite GetPieceSprite(int pieceType, bool isWhite, Sprite[] pieceSprites)
    {
        string key = pieceType + "_" + (isWhite ? "w" : "b");
        if (spriteCache.ContainsKey(key))
            return spriteCache[key];

        // Если есть свои спрайты — возвращаем их, иначе создаем процедурно
        if (pieceSprites != null && pieceSprites.Length > 0)
        {
            if (pieceType >= 0 && pieceType < pieceSprites.Length)
            {
                spriteCache[key] = pieceSprites[pieceType];
                return pieceSprites[pieceType];
            }
        }

        // Иначе — создаем процедурный спрайт (ваш текущий код)
        Texture2D tex = new Texture2D(SIZE, SIZE, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[SIZE * SIZE];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;

        Color fill = isWhite ? WHITE_FILL : BLACK_FILL;
        Color outline = isWhite ? WHITE_OUTLINE : BLACK_OUTLINE;

        switch (pieceType)
        {
            case CalessEngine.RAT:      DrawRat(pixels, fill, outline); break;
            case CalessEngine.OX:       DrawOx(pixels, fill, outline); break;
            case CalessEngine.TIGER:    DrawTiger(pixels, fill, outline); break;
            case CalessEngine.RABBIT:   DrawRabbit(pixels, fill, outline); break;
            case CalessEngine.DRAGON:   DrawDragon(pixels, fill, outline); break;
            case CalessEngine.SNAKE:    DrawSnake(pixels, fill, outline); break;
            case CalessEngine.HORSE:    DrawHorse(pixels, fill, outline); break;
            case CalessEngine.GOAT:     DrawGoat(pixels, fill, outline); break;
            case CalessEngine.MONKEY:   DrawMonkey(pixels, fill, outline); break;
            case CalessEngine.ROOSTER:  DrawRooster(pixels, fill, outline); break;
            case CalessEngine.DOG:      DrawDog(pixels, fill, outline); break;
            case CalessEngine.PIG:      DrawPig(pixels, fill, outline); break;
        }

        if (isWhite)
            AddGlow(pixels, WHITE_GLOW);

        tex.SetPixels(pixels);
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;

        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, SIZE, SIZE), new Vector2(0.5f, 0.5f), SIZE);
        spriteCache[key] = sprite;
        return sprite;
    }

    public static void ClearCache()
    {
        spriteCache.Clear();
    }
 
    // ================ Фигуры ================
 
    private static void DrawRat(Color[] px, Color fill, Color outline)
    {
        // Тело (маленькое овальное)
        DrawFilledEllipse(px, 64, 40, 22, 18, fill);
        DrawEllipse(px, 64, 40, 22, 18, outline);
        // Голова
        DrawFilledCircle(px, 64, 68, 16, fill);
        DrawCircle(px, 64, 68, 16, outline);
        // Уши
        DrawFilledCircle(px, 50, 82, 8, fill);
        DrawCircle(px, 50, 82, 8, outline);
        DrawFilledCircle(px, 78, 82, 8, fill);
        DrawCircle(px, 78, 82, 8, outline);
        // Глаза
        DrawFilledCircle(px, 58, 72, 3, outline);
        DrawFilledCircle(px, 70, 72, 3, outline);
        // Хвост
        DrawLine(px, 64, 22, 45, 10, outline);
        DrawLine(px, 45, 10, 35, 15, outline);
        // Основание
        DrawFilledRect(px, 38, 8, 90, 18, fill);
        DrawRect(px, 38, 8, 90, 18, outline);
    }
 
    private static void DrawOx(Color[] px, Color fill, Color outline)
    {
        // Основание
        DrawFilledRect(px, 25, 8, 103, 22, fill);
        DrawRect(px, 25, 8, 103, 22, outline);
        // Тело (мощное)
        DrawFilledRect(px, 30, 22, 98, 60, fill);
        DrawRect(px, 30, 22, 98, 60, outline);
        // Голова
        DrawFilledRect(px, 40, 60, 88, 85, fill);
        DrawRect(px, 40, 60, 88, 85, outline);
        // Рога
        DrawLine(px, 44, 85, 30, 105, outline);
        DrawLine(px, 30, 105, 35, 110, outline);
        DrawLine(px, 84, 85, 98, 105, outline);
        DrawLine(px, 98, 105, 93, 110, outline);
        // Глаза
        DrawFilledCircle(px, 52, 75, 4, outline);
        DrawFilledCircle(px, 76, 75, 4, outline);
        // Ноздри
        DrawFilledCircle(px, 58, 65, 2, outline);
        DrawFilledCircle(px, 70, 65, 2, outline);
    }
 
    private static void DrawTiger(Color[] px, Color fill, Color outline)
    {
        // Основание
        DrawFilledRect(px, 25, 8, 103, 22, fill);
        DrawRect(px, 25, 8, 103, 22, outline);
        // Тело
        DrawFilledRect(px, 35, 22, 93, 58, fill);
        DrawRect(px, 35, 22, 93, 58, outline);
        // Голова (круглая)
        DrawFilledCircle(px, 64, 75, 22, fill);
        DrawCircle(px, 64, 75, 22, outline);
        // Уши (треугольные)
        DrawFilledTriangle(px, 44, 90, 38, 108, 52, 95, fill);
        DrawTriangleOutline(px, 44, 90, 38, 108, 52, 95, outline);
        DrawFilledTriangle(px, 84, 90, 90, 108, 76, 95, fill);
        DrawTriangleOutline(px, 84, 90, 90, 108, 76, 95, outline);
        // Глаза
        DrawFilledCircle(px, 54, 78, 4, outline);
        DrawFilledCircle(px, 74, 78, 4, outline);
        // Полоски
        DrawLine(px, 52, 68, 48, 60, outline);
        DrawLine(px, 64, 66, 64, 58, outline);
        DrawLine(px, 76, 68, 80, 60, outline);
        // Корона (Тигр — защитник)
        DrawFilledRect(px, 52, 95, 76, 100, outline);
    }

    private static void DrawRabbit(Color[] px, Color fill, Color outline)
    {
        // Основание
        DrawFilledRect(px, 38, 8, 90, 20, fill);
        DrawRect(px, 38, 8, 90, 20, outline);
        // Тело
        DrawFilledEllipse(px, 64, 38, 20, 18, fill);
        DrawEllipse(px, 64, 38, 20, 18, outline);
        // Голова
        DrawFilledCircle(px, 64, 65, 15, fill);
        DrawCircle(px, 64, 65, 15, outline);
        // Уши (длинные)
        DrawFilledRect(px, 52, 78, 58, 108, fill);
        DrawRect(px, 52, 78, 58, 108, outline);
        DrawFilledRect(px, 70, 78, 76, 108, fill);
        DrawRect(px, 70, 78, 76, 108, outline);
        // Глаза
        DrawFilledCircle(px, 58, 68, 3, outline);
        DrawFilledCircle(px, 70, 68, 3, outline);
        // Нос
        DrawFilledCircle(px, 64, 60, 2, outline);
    }
 
    private static void DrawDragon(Color[] px, Color fill, Color outline)
    {
        // Основание (королевское)
        DrawFilledRect(px, 20, 8, 108, 24, fill);
        DrawRect(px, 20, 8, 108, 24, outline);
        // Тело
        DrawFilledTrapezoid(px, 35, 24, 93, 65, 40, 88, fill);
        DrawTrapezoid(px, 35, 24, 93, 65, 40, 88, outline);
        // Голова
        DrawFilledRect(px, 42, 65, 86, 90, fill);
        DrawRect(px, 42, 65, 86, 90, outline);
        // Рога (корона дракона)
        DrawFilledTriangle(px, 42, 90, 35, 110, 50, 92, fill);
        DrawTriangleOutline(px, 42, 90, 35, 110, 50, 92, outline);
        DrawFilledTriangle(px, 58, 90, 54, 115, 66, 90, fill);
        DrawTriangleOutline(px, 58, 90, 54, 115, 66, 90, outline);
        DrawFilledTriangle(px, 74, 90, 74, 115, 82, 90, fill);
        DrawTriangleOutline(px, 74, 90, 74, 115, 82, 90, outline);
        DrawFilledTriangle(px, 86, 90, 93, 110, 78, 92, fill);
        DrawTriangleOutline(px, 86, 90, 93, 110, 78, 92, outline);
        // Глаза (горящие)
        DrawFilledCircle(px, 52, 80, 5, outline);
        DrawFilledCircle(px, 76, 80, 5, outline);
        DrawFilledCircle(px, 52, 80, 2, fill);
        DrawFilledCircle(px, 76, 80, 2, fill);
        // Пасть
        DrawLine(px, 50, 70, 78, 70, outline);
        DrawLine(px, 55, 68, 58, 65, outline);
        DrawLine(px, 70, 68, 67, 65, outline);
    }
 
    private static void DrawSnake(Color[] px, Color fill, Color outline)
    {
        // Основание
        DrawFilledRect(px, 35, 8, 93, 18, fill);
        DrawRect(px, 35, 8, 93, 18, outline);
        // Извивающееся тело
        DrawFilledEllipse(px, 50, 30, 16, 12, fill);
        DrawEllipse(px, 50, 30, 16, 12, outline);
        DrawFilledEllipse(px, 78, 48, 16, 12, fill);
        DrawEllipse(px, 78, 48, 16, 12, outline);
        DrawFilledEllipse(px, 50, 66, 16, 12, fill);
        DrawEllipse(px, 50, 66, 16, 12, outline);
        // Соединения
        DrawFilledRect(px, 54, 30, 70, 48, fill);
        DrawFilledRect(px, 56, 48, 72, 66, fill);
        // Голова
        DrawFilledCircle(px, 64, 85, 14, fill);
        DrawCircle(px, 64, 85, 14, outline);
        // Глаза
        DrawFilledCircle(px, 58, 88, 3, outline);
        DrawFilledCircle(px, 70, 88, 3, outline);
        // Язык
        DrawLine(px, 64, 72, 64, 65, outline);
        DrawLine(px, 64, 65, 60, 60, outline);
        DrawLine(px, 64, 65, 68, 60, outline);
    }
 
    private static void DrawHorse(Color[] px, Color fill, Color outline)
    {
        // Основание
        DrawFilledRect(px, 28, 8, 100, 24, fill);
        DrawRect(px, 28, 8, 100, 24, outline);
        // Тело
        DrawFilledRect(px, 42, 24, 86, 50, fill);
        DrawRect(px, 42, 24, 86, 50, outline);
        // Шея
        DrawFilledTrapezoid(px, 42, 50, 78, 80, 48, 72, fill);
        DrawTrapezoid(px, 42, 50, 78, 80, 48, 72, outline);
        // Голова (морда)
        DrawFilledRect(px, 35, 80, 75, 102, fill);
        DrawRect(px, 35, 80, 75, 102, outline);
        // Ухо
        DrawFilledTriangle(px, 65, 102, 72, 116, 78, 102, fill);
        DrawTriangleOutline(px, 65, 102, 72, 116, 78, 102, outline);
        // Глаз
        DrawFilledCircle(px, 48, 92, 4, outline);
        // Грива
        DrawFilledRect(px, 74, 55, 80, 88, outline);
    }
 
    private static void DrawGoat(Color[] px, Color fill, Color outline)
    {
        // Основание
        DrawFilledRect(px, 32, 8, 96, 22, fill);
        DrawRect(px, 32, 8, 96, 22, outline);
        // Тело
        DrawFilledRect(px, 38, 22, 90, 55, fill);
        DrawRect(px, 38, 22, 90, 55, outline);
        // Голова
        DrawFilledCircle(px, 64, 72, 18, fill);
        DrawCircle(px, 64, 72, 18, outline);
        // Рога (изогнутые)
        DrawLine(px, 50, 88, 38, 100, outline);
        DrawLine(px, 38, 100, 35, 108, outline);
        DrawLine(px, 78, 88, 90, 100, outline);
        DrawLine(px, 90, 100, 93, 108, outline);
        // Борода
        DrawLine(px, 64, 55, 64, 48, outline);
        DrawLine(px, 62, 55, 60, 46, outline);
        DrawLine(px, 66, 55, 68, 46, outline);
        // Глаза
        DrawFilledCircle(px, 56, 75, 3, outline);
        DrawFilledCircle(px, 72, 75, 3, outline);
        // Крест целителя
        DrawFilledRect(px, 62, 30, 66, 42, outline);
        DrawFilledRect(px, 56, 34, 72, 38, outline);
    }
 
    private static void DrawMonkey(Color[] px, Color fill, Color outline)
    {
        // Основание
        DrawFilledRect(px, 32, 8, 96, 22, fill);
        DrawRect(px, 32, 8, 96, 22, outline);
        // Тело
        DrawFilledRect(px, 40, 22, 88, 55, fill);
        DrawRect(px, 40, 22, 88, 55, outline);
        // Голова
        DrawFilledCircle(px, 64, 72, 20, fill);
        DrawCircle(px, 64, 72, 20, outline);
        // Лицо (светлый овал)
        DrawFilledEllipse(px, 64, 68, 12, 14, outline);
        DrawFilledEllipse(px, 64, 68, 10, 12, fill);
        // Уши
 
        DrawFilledCircle(px, 42, 78, 8, fill);
        DrawCircle(px, 42, 78, 8, outline);
        DrawFilledCircle(px, 86, 78, 8, fill);
        DrawCircle(px, 86, 78, 8, outline);
        // Глаза
        DrawFilledCircle(px, 58, 74, 3, outline);
        DrawFilledCircle(px, 70, 74, 3, outline);
        // Хвост
        DrawLine(px, 82, 30, 98, 40, outline);
        DrawLine(px, 98, 40, 102, 50, outline);
        DrawLine(px, 102, 50, 96, 55, outline);
    }
 
    private static void DrawRooster(Color[] px, Color fill, Color outline)
    {
        // Основание
        DrawFilledRect(px, 35, 8, 93, 20, fill);
        DrawRect(px, 35, 8, 93, 20, outline);
        // Тело
        DrawFilledEllipse(px, 64, 38, 22, 18, fill);
        DrawEllipse(px, 64, 38, 22, 18, outline);
        // Шея
        DrawFilledRect(px, 55, 55, 73, 72, fill);
        DrawRect(px, 55, 55, 73, 72, outline);
        // Голова
        DrawFilledCircle(px, 64, 82, 12, fill);
        DrawCircle(px, 64, 82, 12, outline);
        // Гребень
        DrawFilledTriangle(px, 58, 92, 54, 108, 64, 95, outline);
        DrawFilledTriangle(px, 64, 94, 62, 110, 72, 96, outline);
        // Клюв
        DrawFilledTriangle(px, 52, 80, 42, 76, 52, 76, outline);
        // Глаз
        DrawFilledCircle(px, 60, 84, 2, outline);
        // Хвост
        DrawLine(px, 84, 35, 100, 50, outline);
        DrawLine(px, 82, 32, 98, 42, outline);
        DrawLine(px, 86, 38, 104, 45, outline);
    }
 
    private static void DrawDog(Color[] px, Color fill, Color outline)
    {
        // Основание
        DrawFilledRect(px, 28, 8, 100, 22, fill);
        DrawRect(px, 28, 8, 100, 22, outline);
        // Тело
        DrawFilledRect(px, 35, 22, 93, 58, fill);
        DrawRect(px, 35, 22, 93, 58, outline);
        // Голова
        DrawFilledCircle(px, 64, 74, 18, fill);
        DrawCircle(px, 64, 74, 18, outline);
        // Уши (висячие)
        DrawFilledEllipse(px, 44, 86, 8, 14, fill);
        DrawEllipse(px, 44, 86, 8, 14, outline);
        DrawFilledEllipse(px, 84, 86, 8, 14, fill);
        DrawEllipse(px, 84, 86, 8, 14, outline);
        // Глаза
        DrawFilledCircle(px, 56, 78, 4, outline);
        DrawFilledCircle(px, 72, 78, 4, outline);
        // Нос
        DrawFilledCircle(px, 64, 68, 4, outline);
        // Язык
        DrawLine(px, 64, 64, 64, 58, outline);
    }
 
    private static void DrawPig(Color[] px, Color fill, Color outline)
    {
        // Основание
        DrawFilledRect(px, 25, 8, 103, 22, fill);
        DrawRect(px, 25, 8, 103, 22, outline);
        // Тело (большое)
        DrawFilledEllipse(px, 64, 42, 30, 20, fill);
        DrawEllipse(px, 64, 42, 30, 20, outline);
        // Голова
        DrawFilledCircle(px, 64, 72, 18, fill);
        DrawCircle(px, 64, 72, 18, outline);
        // Уши
        DrawFilledTriangle(px, 48, 86, 42, 98, 55, 88, fill);
        DrawTriangleOutline(px, 48, 86, 42, 98, 55, 88, outline);
        DrawFilledTriangle(px, 80, 86, 86, 98, 73, 88, fill);
        DrawTriangleOutline(px, 80, 86, 86, 98, 73, 88, outline);
        // Пятачок
        DrawFilledEllipse(px, 64, 68, 10, 7, outline);
        DrawFilledEllipse(px, 64, 68, 8, 5, fill);
        DrawFilledCircle(px, 60, 68, 2, outline);
        DrawFilledCircle(px, 68, 68, 2, outline);
        // Глаза
        DrawFilledCircle(px, 55, 78, 3, outline);
        DrawFilledCircle(px, 73, 78, 3, outline);
    }
    // ================ Эффект свечения для белых фигур ================
 
    private static void AddGlow(Color[] pixels, Color glowColor)
    {
        Color[] temp = new Color[pixels.Length];
        System.Array.Copy(pixels, temp, pixels.Length);
 
        for (int y = 2; y < SIZE - 2; y++)
        {
            for (int x = 2; x < SIZE - 2; x++)
            {
                if (temp[y * SIZE + x].a > 0.1f) continue;
 
                bool nearSolid = false;
                for (int dy = -2; dy <= 2 && !nearSolid; dy++)
                    for (int dx = -2; dx <= 2 && !nearSolid; dx++)
                    {
                        int nx = x + dx, ny = y + dy;
                        if (nx >= 0 && nx < SIZE && ny >= 0 && ny < SIZE)
                            if (temp[ny * SIZE + nx].a > 0.5f)
                                nearSolid = true;
                    }
 
                if (nearSolid)
                    pixels[y * SIZE + x] = glowColor;
            }
        }
    }
 
    // ================ Примитивы рисования ================
 
    private static void SetPixel(Color[] px, int x, int y, Color color)
    {
        if (x >= 0 && x < SIZE && y >= 0 && y < SIZE)
            px[y * SIZE + x] = color;
    }
 
    private static void DrawFilledRect(Color[] px, int x1, int y1, int x2, int y2, Color color)
    {
        for (int y = y1; y <= y2; y++)
            for (int x = x1; x <= x2; x++)
                SetPixel(px, x, y, color);
    }
 
    private static void DrawRect(Color[] px, int x1, int y1, int x2, int y2, Color color)
    {
        for (int x = x1; x <= x2; x++) { SetPixel(px, x, y1, color); SetPixel(px, x, y2, color); }
        for (int y = y1; y <= y2; y++) { SetPixel(px, x1, y, color); SetPixel(px, x2, y, color); }
    }
 
    private static void DrawFilledCircle(Color[] px, int cx, int cy, int r, Color color)
    {
        for (int y = cy - r; y <= cy + r; y++)
            for (int x = cx - r; x <= cx + r; x++)
                if ((x - cx) * (x - cx) + (y - cy) * (y - cy) <= r * r)
                    SetPixel(px, x, y, color);
    }
 
    private static void DrawCircle(Color[] px, int cx, int cy, int r, Color color)
    {
        int r2 = r * r;
        int o2 = (r + 1) * (r + 1);
        for (int y = cy - r - 1; y <= cy + r + 1; y++)
            for (int x = cx - r - 1; x <= cx + r + 1; x++)
            {
                int d = (x - cx) * (x - cx) + (y - cy) * (y - cy);
                if (d >= r2 && d < o2)
                    SetPixel(px, x, y, color);
            }
    }
 
    private static void DrawFilledEllipse(Color[] px, int cx, int cy, int rx, int ry, Color color)
    {
        for (int y = cy - ry; y <= cy + ry; y++)
            for (int x = cx - rx; x <= cx + rx; x++)
            {
                float dx = (float)(x - cx) / rx;
                float dy = (float)(y - cy) / ry;
                if (dx * dx + dy * dy <= 1f)
                    SetPixel(px, x, y, color);
            }
    }
 
    private static void DrawEllipse(Color[] px, int cx, int cy, int rx, int ry, Color color)
    {
        for (int y = cy - ry - 1; y <= cy + ry + 1; y++)
            for (int x = cx - rx - 1; x <= cx + rx + 1; x++)
            {
                float dx = (float)(x - cx) / rx;
                float dy = (float)(y - cy) / ry;
                float d = dx * dx + dy * dy;
                float dxo = (float)(x - cx) / (rx + 1);
                float dyo = (float)(y - cy) / (ry + 1);
                float d2 = dxo * dxo + dyo * dyo;
                if (d >= 1f && d2 < 1f)
                    SetPixel(px, x, y, color);
            }
    }
 
    private static void DrawLine(Color[] px, int x0, int y0, int x1, int y1, Color color)
    {
        int dx = Mathf.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
        int dy = -Mathf.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
        int err = dx + dy;
 
        while (true)
        {
            SetPixel(px, x0, y0, color);
            SetPixel(px, x0 + 1, y0, color);
            SetPixel(px, x0, y0 + 1, color);
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 >= dy) { err += dy; x0 += sx; }
            if (e2 <= dx) { err += dx; y0 += sy; }
        }
    }
 
    private static void DrawFilledTriangle(Color[] px, int x0, int y0, int x1, int y1, int x2, int y2, Color color)
    {
        int minY = Mathf.Min(y0, Mathf.Min(y1, y2));
        int maxY = Mathf.Max(y0, Mathf.Max(y1, y2));
        int minX = Mathf.Min(x0, Mathf.Min(x1, x2));
        int maxX = Mathf.Max(x0, Mathf.Max(x1, x2));
 
        for (int y = minY; y <= maxY; y++)
            for (int x = minX; x <= maxX; x++)
                if (PointInTriangle(x, y, x0, y0, x1, y1, x2, y2))
                    SetPixel(px, x, y, color);
    }
 
    private static void DrawTriangleOutline(Color[] px, int x0, int y0, int x1, int y1, int x2, int y2, Color color)
    {
        DrawLine(px, x0, y0, x1, y1, color);
        DrawLine(px, x1, y1, x2, y2, color);
        DrawLine(px, x2, y2, x0, y0, color);
    }
 
    private static void DrawFilledTrapezoid(Color[] px, int bx1, int by, int bx2, int ty, int tx1, int tx2, Color color)
    {
        for (int y = by; y <= ty; y++)
        {
            float t = (float)(y - by) / Mathf.Max(1, ty - by);
            int xl = Mathf.RoundToInt(Mathf.Lerp(bx1, tx1, t));
            int xr = Mathf.RoundToInt(Mathf.Lerp(bx2, tx2, t));
            for (int x = xl; x <= xr; x++)
                SetPixel(px, x, y, color);
        }
    }
 
    private static void DrawTrapezoid(Color[] px, int bx1, int by, int bx2, int ty, int tx1, int tx2, Color color)
    {
        DrawLine(px, bx1, by, tx1, ty, color);
        DrawLine(px, bx2, by, tx2, ty, color);
        DrawLine(px, bx1, by, bx2, by, color);
        DrawLine(px, tx1, ty, tx2, ty, color);
    }
 
    private static bool PointInTriangle(int px, int py, int x0, int y0, int x1, int y1, int x2, int y2)
    {
        int d1 = (px - x1) * (y0 - y1) - (x0 - x1) * (py - y1);
        int d2 = (px - x2) * (y1 - y2) - (x1 - x2) * (py - y2);
        int d3 = (px - x0) * (y2 - y0) - (x2 - x0) * (py - y0);
        bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);
        return !(hasNeg && hasPos);
    }
}