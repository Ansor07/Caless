using UnityEngine;
 
/// <summary>
/// Управление настройками игры. Сохранение через PlayerPrefs.
/// </summary>
public class SettingsManager : MonoBehaviour
{
    // Звук
    public bool SoundEnabled { get; set; } = true;
    public bool MusicEnabled { get; set; } = true;
    public float SoundVolume { get; set; } = 1f;
    public float MusicVolume { get; set; } = 0.7f;
 
    // Визуал
    public int BoardTheme { get; set; } = 0;    // 0=Тёмная фэнтези, 1=Классика, 2=Лес
    public int PieceTheme { get; set; } = 0;     // 0=Стандартные, 1=Минимальные
    public bool AnimationsEnabled { get; set; } = true;
 
    // Язык
    public int Language { get; set; } = 0; // 0=Русский
 
    // Темы доски
    public static readonly Color[][] BoardThemes = new Color[][]
    {
        // Тёмная фэнтези
        new Color[] {
            new Color(0.22f, 0.18f, 0.15f),  // тёмная клетка
            new Color(0.45f, 0.38f, 0.30f),   // светлая клетка
        },
        // Классика
        new Color[] {
            new Color(0.55f, 0.37f, 0.23f),
            new Color(0.93f, 0.87f, 0.78f),
        },
        // Лес
        new Color[] {
            new Color(0.18f, 0.30f, 0.15f),
            new Color(0.40f, 0.55f, 0.35f),
        }
    };
 
    public static readonly string[] BoardThemeNames = { "Тёмная фэнтези", "Классика", "Лес" };
    public static readonly string[] PieceThemeNames = { "Стандартные", "Минимальные" };
    public static readonly string[] LanguageNames = { "Русский" };
 
    public Color GetDarkSquareColor()
    {
        if (BoardTheme >= 0 && BoardTheme < BoardThemes.Length)
            return BoardThemes[BoardTheme][0];
        return BoardThemes[0][0];
    }
 
    public Color GetLightSquareColor()
    {
        if (BoardTheme >= 0 && BoardTheme < BoardThemes.Length)
            return BoardThemes[BoardTheme][1];
        return BoardThemes[0][1];
    }
 
    public void Load()
    {
        SoundEnabled = PlayerPrefs.GetInt("SoundEnabled", 1) == 1;
        MusicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        SoundVolume = PlayerPrefs.GetFloat("SoundVolume", 1f);
        MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        BoardTheme = PlayerPrefs.GetInt("BoardTheme", 0);
        PieceTheme = PlayerPrefs.GetInt("PieceTheme", 0);
        AnimationsEnabled = PlayerPrefs.GetInt("AnimationsEnabled", 1) == 1;
        Language = PlayerPrefs.GetInt("Language", 0);
    }
 
    public void Save()
    {
        PlayerPrefs.SetInt("SoundEnabled", SoundEnabled ? 1 : 0);
        PlayerPrefs.SetInt("MusicEnabled", MusicEnabled ? 1 : 0);
        PlayerPrefs.SetFloat("SoundVolume", SoundVolume);
        PlayerPrefs.SetFloat("MusicVolume", MusicVolume);
        PlayerPrefs.SetInt("BoardTheme", BoardTheme);
        PlayerPrefs.SetInt("PieceTheme", PieceTheme);
        PlayerPrefs.SetInt("AnimationsEnabled", AnimationsEnabled ? 1 : 0);
        PlayerPrefs.SetInt("Language", Language);
        PlayerPrefs.Save();
    }
}