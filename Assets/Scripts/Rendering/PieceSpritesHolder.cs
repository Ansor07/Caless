using UnityEngine;

public class PieceSpritesHolder : MonoBehaviour
{
    public Sprite[] pieceSprites; // Назначайте сюда ваши спрайты через инспектор

    public static Sprite[] Sprites;

    void Awake()
    {
        Sprites = pieceSprites;
    }
}