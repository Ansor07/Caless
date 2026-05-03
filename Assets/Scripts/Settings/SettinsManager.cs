using UnityEngine;

/// <summary>
/// Game settings management. Saved via PlayerPrefs.
/// Supports appearance, sound, gameplay, and other options.
/// </summary>
public class SettingsManager : MonoBehaviour
{
    // Sound
    public bool SoundEnabled { get; set; } = true;
    public bool MusicEnabled { get; set; } = true;
    public float SoundVolume { get; set; } = 1f;
    public float MusicVolume { get; set; } = 0.7f;
    public bool MoveSoundEnabled { get; set; } = true;
    public bool CaptureSoundEnabled { get; set; } = true;

    // Appearance
    public int BoardTheme { get; set; } = 0;    // 0=Dark Fantasy, 1=Classic, 2=Forest
    public int PieceTheme { get; set; } = 0;     // 0=Standard, 1=Minimal
    public int HighlightColor { get; set; } = 0; // 0=Gold, 1=Blue, 2=Green, 3=Purple
    public bool AnimationsEnabled { get; set; } = true;

    // Gameplay
    public bool HintsEnabled { get; set; } = false;
    public bool ShowPossibleMoves { get; set; } = true;
    public bool AutoSaveEnabled { get; set; } = true;
    public bool MoveConfirmation { get; set; } = false;
    public int DefaultDifficulty { get; set; } = 1; // 0=Easy, 1=Medium
    public int DefaultMoveTime { get; set; } = 1;   // 0=5min, 1=10min, 2=15min, 3=30min

    // Language
    public int Language { get; set; } = 0; // 0=Russian

    // Board themes
    public static readonly Color[][] BoardThemes = new Color[][]
    {
        // Dark Fantasy
        new Color[] {
            new Color(0.22f, 0.18f, 0.15f),
            new Color(0.45f, 0.38f, 0.30f),
        },
        // Classic
        new Color[] {
            new Color(0.55f, 0.37f, 0.23f),
            new Color(0.93f, 0.87f, 0.78f),
        },
        // Forest
        new Color[] {
            new Color(0.18f, 0.30f, 0.15f),
            new Color(0.40f, 0.55f, 0.35f),
        }
    };

    public static readonly string[] BoardThemeNames = { "Тёмная фэнтези", "Классика", "Лес" };
    public static readonly string[] PieceThemeNames = { "Стандартные", "Минимальные" };
    public static readonly string[] LanguageNames = { "RU", "EN" };
    public static readonly string[] HighlightColorNames = { "Золото", "Синий", "Зелёный", "Фиолетовый" };
    public static readonly string[] DifficultyNames = { "Лёгкая", "Средняя" };
    public static readonly string[] MoveTimeNames = { "5 мин", "10 мин", "15 мин", "30 мин" };

    // Highlight colors
    public static readonly Color[] HighlightColors = {
        new Color(0.85f, 0.70f, 0.30f, 0.6f),
        new Color(0.30f, 0.50f, 0.90f, 0.6f),
        new Color(0.20f, 0.70f, 0.30f, 0.6f),
        new Color(0.60f, 0.30f, 0.70f, 0.6f)
    };

    public Color GetHighlightColor()
    {
        if (HighlightColor >= 0 && HighlightColor < HighlightColors.Length)
            return HighlightColors[HighlightColor];
        return HighlightColors[0];
    }

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

    public int GetMoveTimeMinutes()
    {
        switch (DefaultMoveTime)
        {
            case 0: return 5;
            case 1: return 10;
            case 2: return 15;
            case 3: return 30;
            default: return 10;
        }
    }

    public void Load()
    {
        SoundEnabled = PlayerPrefs.GetInt("SoundEnabled", 1) == 1;
        MusicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        SoundVolume = PlayerPrefs.GetFloat("SoundVolume", 1f);
        MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        MoveSoundEnabled = PlayerPrefs.GetInt("MoveSoundEnabled", 1) == 1;
        CaptureSoundEnabled = PlayerPrefs.GetInt("CaptureSoundEnabled", 1) == 1;
        BoardTheme = PlayerPrefs.GetInt("BoardTheme", 0);
        PieceTheme = PlayerPrefs.GetInt("PieceTheme", 0);
        HighlightColor = PlayerPrefs.GetInt("HighlightColor", 0);
        AnimationsEnabled = PlayerPrefs.GetInt("AnimationsEnabled", 1) == 1;
        HintsEnabled = PlayerPrefs.GetInt("HintsEnabled", 0) == 1;
        ShowPossibleMoves = PlayerPrefs.GetInt("ShowPossibleMoves", 1) == 1;
        AutoSaveEnabled = PlayerPrefs.GetInt("AutoSaveEnabled", 1) == 1;
        MoveConfirmation = PlayerPrefs.GetInt("MoveConfirmation", 0) == 1;
        DefaultDifficulty = PlayerPrefs.GetInt("DefaultDifficulty", 1);
        DefaultMoveTime = PlayerPrefs.GetInt("DefaultMoveTime", 1);
        Language = PlayerPrefs.GetInt("Language", 0);
    }

    public void Save()
    {
        PlayerPrefs.SetInt("SoundEnabled", SoundEnabled ? 1 : 0);
        PlayerPrefs.SetInt("MusicEnabled", MusicEnabled ? 1 : 0);
        PlayerPrefs.SetFloat("SoundVolume", SoundVolume);
        PlayerPrefs.SetFloat("MusicVolume", MusicVolume);
        PlayerPrefs.SetInt("MoveSoundEnabled", MoveSoundEnabled ? 1 : 0);
        PlayerPrefs.SetInt("CaptureSoundEnabled", CaptureSoundEnabled ? 1 : 0);
        PlayerPrefs.SetInt("BoardTheme", BoardTheme);
        PlayerPrefs.SetInt("PieceTheme", PieceTheme);
        PlayerPrefs.SetInt("HighlightColor", HighlightColor);
        PlayerPrefs.SetInt("AnimationsEnabled", AnimationsEnabled ? 1 : 0);
        PlayerPrefs.SetInt("HintsEnabled", HintsEnabled ? 1 : 0);
        PlayerPrefs.SetInt("ShowPossibleMoves", ShowPossibleMoves ? 1 : 0);
        PlayerPrefs.SetInt("AutoSaveEnabled", AutoSaveEnabled ? 1 : 0);
        PlayerPrefs.SetInt("MoveConfirmation", MoveConfirmation ? 1 : 0);
        PlayerPrefs.SetInt("DefaultDifficulty", DefaultDifficulty);
        PlayerPrefs.SetInt("DefaultMoveTime", DefaultMoveTime);
        PlayerPrefs.SetInt("Language", Language);
        PlayerPrefs.Save();
    }
}
