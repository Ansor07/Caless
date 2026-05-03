using UnityEngine;

public class PieceSpritesHolder : MonoBehaviour
{
    public Sprite[] pieceSprites; // Назначайте сюда ваши спрайты через инспектор

    public static Sprite[] Sprites;

    void Awake()
    {
        if (pieceSprites != null && pieceSprites.Length > 0)
            Sprites = pieceSprites;
    }

    /// <summary>
    /// Программная установка спрайтов. Полезно, если спрайты генерируются
    /// в рантайме (например, из PieceSpriteGenerator) и не назначены в инспекторе.
    /// </summary>
    public static void SetSprites(Sprite[] sprites)
    {
        Sprites = sprites;
    }
}