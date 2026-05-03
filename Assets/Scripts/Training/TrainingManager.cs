using System.Collections.Generic;
using UnityEngine;
 
/// <summary>
/// Режим обучения с 30+ головоломками.
/// Каждая задача: расставлены фигуры, нужно найти лучший ход или мат.
/// </summary>
public class TrainingManager : MonoBehaviour
{
    [HideInInspector] public GameManager gameManager;
 
    private CalessEngine engine;
    private int currentPuzzle = 0;
    private Vector2Int selectedSquare = new Vector2Int(-1, -1);
    private List<Move> selectedMoves = new List<Move>();
    private Camera mainCam;
    private bool puzzleSolved = false;
 
    // ==================== ДАННЫЕ ГОЛОВОЛОМОК ====================
 
    public struct Puzzle
    {
        public string description;
        public int[,] board;
        public Vector2Int expectedFrom;
        public Vector2Int expectedTo;
        public bool anyCapture;
    }
 
    private List<Puzzle> puzzles = new List<Puzzle>();
 
    void Awake()
    {
        GeneratePuzzles();
    }
 
    private void GeneratePuzzles()
    {
        puzzles.Clear();

        // 1. Дракон берёт Тигра
        AddPuzzle("Дракон атакует Тигра",
            new int[,] {
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,5,0,0,0,0},
                {0,0,0,0,0,-3,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0}
            }, 4, 5, 5, 5);
 
        // 2. Свинья берёт по горизонтали
        AddPuzzle("Свинья атакует по горизонтали",
            new int[,] {
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {12,0,0,0,0,0,0,-1,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0}
            }, 3, 0, 3, 7);
 
        // 3. Змея диагональная атака
        AddPuzzle("Змея атакует по диагонали",
            new int[,] {
                {0,0,0,0,0,0,0,0,0,0},
                {6,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,-8,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0}
            }, 1, 0, 4, 3, true);
 
        // 4. Обезьяна — ход конём
        AddPuzzle("Обезьяна атакует конём",
            new int[,] {
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,9,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,-5,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0}
            }, 3, 3, 5, 4);
 
        // 5. Бык вперёд 4 клетки
        AddPuzzle("Бык продвигается вперёд",
            new int[,] {
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,2,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,-4,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0}
            }, 2, 2, 6, 2);
 
        // 6. Крыса ровно 3 клетки
        AddPuzzle("Крыса прыгает на 3 клетки",
            new int[,] {
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,1,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,-10,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0}
            }, 3, 3, 6, 3);
 
        // 7. Лошадь V-H-V
        AddPuzzle("Лошадь прыгает V-H-V",
            new int[,] {
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,7,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,-9,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0}
            }, 3, 4, 5, 5);
 
        // 8. Собака — ладья + диагональ
        AddPuzzle("Собака использует ход ладьи",
            new int[,] {
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {11,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {-11,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0}
            }, 2, 0, 8, 0);
 
        // 9. Петух первый ход
        AddPuzzle("Петух: первый ход на 2 клетки",
            new int[,] {
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,10,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0}
            }, 1, 2, 3, 2);
 
        // 10. Кролик атакует вперёд
        AddPuzzle("Кролик атакует вертикально",
            new int[,] {
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,4,0,0,0,0,0,0},
                {0,0,0,-10,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0}
            }, 3, 3, 4, 3);
 
        // 11. Козёл ходит на 1 клетку
        AddPuzzle("Козёл захватывает врага",
            new int[,] {
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,8,0,0,0,0,0,0},
                {0,0,0,0,-1,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0}
            }, 4, 3, 5, 4);
 
        // 12. Тигр как ферзь (дракон жив)
        AddPuzzle("Тигр атакует как ферзь",
            new int[,] {
                {0,0,0,0,0,5,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,3,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,-5,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0}
            }, 2, 2, 7, 7, true);
 
        // 13. Дракон: дальняя атака по вертикали
        AddPuzzle("Дракон: дальняя атака по вертикали",
            new int[,] {
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,5,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,-8,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0}
            }, 2, 3, 7, 3);
 
        // 14. Обезьяна: диагональная атака змеи
        AddPuzzle("Обезьяна атакует по диагонали",
            new int[,] {
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,9,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,-12,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0}
            }, 2, 1, 6, 5, true);
 
        // 15. Петух: диагональная атака
        AddPuzzle("Петух бьёт по диагонали",
            new int[,] {
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,10,0,0,0,0,0,0},
                {0,0,0,0,-1,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0}
            }, 3, 3, 4, 4);
 
        // 16-30: Дополнительные задачи
        AddPuzzle("Свинья защищает Дракона",
            new int[,] {
                {0,0,0,0,0,5,0,0,0,0},
                {0,0,0,0,0,12,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,-6,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0}
            }, 1, 5, 3, 5);
 
        AddPuzzle("Крыса обходит защиту",
            new int[,] {
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,1,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,-8,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0}
            }, 3, 4, 6, 7);
 
        AddPuzzle("Бык прорывается к центру",
            new int[,] {
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,2,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,-4,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0}
            }, 2, 3, 5, 6, true);
 
        AddPuzzle("Лошадь перепрыгивает барьер",
            new int[,] {
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,7,0,0,0,0,0,0},
                {0,0,0,-10,-10,0,0,0,0,0},
                {0,0,0,0,-5,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0}
            }, 3, 3, 5, 4);
 
        AddPuzzle("Собака: комбо диагональ + ладья",
            new int[,] {
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,11,0,0,0,0,0},
                {0,0,0,0,0,-1,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0}
            }, 4, 4, 5, 5);
 
        AddPuzzle("Змея: максимальная дистанция",
            new int[,] {
                {0,0,0,0,0,0,0,0,0,0},
                {6,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,-10,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0}
            }, 1, 0, 7, 6);
 
        AddPuzzle("Тигр: защита после потери Дракона",
            new int[,] {
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,3,0,0,0,0,0,0},
                {0,0,0,0,-1,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0}
            }, 2, 3, 3, 4);
 
        AddPuzzle("Козёл: позиция для оживления",
            new int[,] {
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,8,12,0,0,0,0,0},
                {0,0,0,0,-6,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0}
            }, 4, 3, 5, 4, true);
 
        AddPuzzle("Кролик: первый ход на 2",
            new int[,] {
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,4,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0}
            }, 1, 4, 3, 4);
 
        AddPuzzle("Свинья пересекает доску",
            new int[,] {
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {12,0,0,0,0,0,0,0,0,-4},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0}
            }, 4, 0, 4, 9);
 
        AddPuzzle("Обезьяна: двойная угроза",
            new int[,] {
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,9,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,-5,0,0,-3,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0}
            }, 3, 4, 5, 5);
 
        AddPuzzle("Дракон: ход на 1 клетку",
            new int[,] {
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,5,0,0,0,0,0},
                {0,0,0,0,0,-10,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0}
            }, 4, 4, 5, 5);
 
        AddPuzzle("Бык: 3 клетки по диагонали",
            new int[,] {
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,2,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,-1,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0}
            }, 2, 2, 5, 5);
 
        AddPuzzle("Крыса: ровно 3 по диагонали",
            new int[,] {
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,1,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,-8,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0}
            }, 3, 1, 6, 4);
 
        AddPuzzle("Собака: вертикальная атака",
            new int[,] {
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,11,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,-10,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0,0,0}
            }, 2, 5, 7, 5);
    }
 
    private void AddPuzzle(string desc, int[,] b, int fr, int fc, int tr, int tc, bool anyCap = false)
    {
        Puzzle p = new Puzzle();
        p.description = desc;
        p.board = b;
        p.expectedFrom = new Vector2Int(fr, fc);
        p.expectedTo = new Vector2Int(tr, tc);
        p.anyCapture = anyCap;
        puzzles.Add(p);
    }
 
    // ==================== УПРАВЛЕНИЕ ====================
 
    public int GetPuzzleCount() { return puzzles.Count; }
    public int GetCurrentPuzzle() { return currentPuzzle; }
 
    public void StartTraining()
    {
        mainCam = Camera.main;
        LoadPuzzle(currentPuzzle);
    }
 
    public void LoadPuzzle(int index)
    {
        if (index < 0) index = puzzles.Count - 1;
        if (index >= puzzles.Count) index = 0;
        currentPuzzle = index;
 
        Puzzle p = puzzles[currentPuzzle];
        engine = new CalessEngine();
        engine.board = (int[,])p.board.Clone();
        engine.whiteTurn = true;
 
        // Найти позиции королевских фигур
        UpdateRoyalPositions();
 
        puzzleSolved = false;
        selectedSquare = new Vector2Int(-1, -1);
        selectedMoves.Clear();

 
        gameManager.boardRenderer.isBoardFlipped = false;
        gameManager.boardRenderer.Initialize(engine);
 
        gameManager.uiManager.ShowTrainingUI(
            null, OnBack, OnClear, OnRandomEnemy,
            OnNextPuzzle, OnPrevPuzzle,
            currentPuzzle, puzzles.Count, p.description
        );
    }
 
    private void UpdateRoyalPositions()
    {
        engine.whiteDragonAlive = false;
        engine.blackDragonAlive = false;
        engine.whiteTigerAlive = false;
        engine.blackTigerAlive = false;
 
        for (int r = 0; r < CalessEngine.BOARD_SIZE; r++)
        {
            for (int c = 0; c < CalessEngine.BOARD_SIZE; c++)
            {
                int piece = engine.board[r, c];
                int type = CalessEngine.PieceType(piece);
                if (type == CalessEngine.DRAGON)
                {
                    if (CalessEngine.IsWhite(piece))
                    { engine.whiteDragonPos = new Vector2Int(r, c); engine.whiteDragonAlive = true; }
                    else
                    { engine.blackDragonPos = new Vector2Int(r, c); engine.blackDragonAlive = true; }
                }
                else if (type == CalessEngine.TIGER)
                {
                    if (CalessEngine.IsWhite(piece))
                    { engine.whiteTigerPos = new Vector2Int(r, c); engine.whiteTigerAlive = true; }
                    else
                    { engine.blackTigerPos = new Vector2Int(r, c); engine.blackTigerAlive = true; }
                }
            }
        }
    }
 
    private void OnBack() { gameManager.ShowMenu(); }
 
    private void OnClear() { LoadPuzzle(currentPuzzle); }
 
    private void OnRandomEnemy()
    {
        int count = Random.Range(2, 4);
        int[] types = { CalessEngine.OX, CalessEngine.SNAKE, CalessEngine.HORSE,
                        CalessEngine.RAT, CalessEngine.ROOSTER, CalessEngine.RABBIT };
 
        for (int i = 0; i < count; i++)
        {
            int attempts = 0;
            while (attempts < 50)
            {
                int r = Random.Range(3, 9);
                int c = Random.Range(0, 10);
                if (engine.board[r, c] == CalessEngine.EMPTY)
                {
                    engine.board[r, c] = -types[Random.Range(0, types.Length)];
                    break;
                }
                attempts++;
            }
        } 
        gameManager.boardRenderer.RefreshPieces();
    }
 
    private void OnNextPuzzle() { LoadPuzzle(currentPuzzle + 1); }
    private void OnPrevPuzzle() { LoadPuzzle(currentPuzzle - 1); }
 
    // ==================== ВВОД ====================
 
    public void HandleInput()
    {
        if (Time.frameCount % 2 != 0) return;
        if (!Input.GetMouseButtonDown(0)) return;
        if (puzzleSolved) return;
        if (mainCam == null) mainCam = Camera.main;
        if (mainCam == null) return;
 
        Vector3 mouseWorld = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;
 
        Vector2Int boardPos = gameManager.boardRenderer.GetBoardPosition(mouseWorld);
        if (!CalessEngine.InBounds(boardPos.x, boardPos.y)) return;
 
        if (selectedSquare.x >= 0)
        {
            // Проверяем ход
            Move? targetMove = null;
            foreach (Move m in selectedMoves)
            {
                if (m.to == boardPos)
                {
                    targetMove = m;
                    break;
                }
            }
 
            if (targetMove.HasValue)
            {
                Move move = targetMove.Value;
                engine.MakeMove(move);
                gameManager.boardRenderer.SetLastMove(move.from, move.to);
                gameManager.boardRenderer.RefreshPieces();
                gameManager.boardRenderer.ClearHighlights();
 
                Puzzle p = puzzles[currentPuzzle];
                bool correct = false;
                if (p.anyCapture && move.captured != CalessEngine.EMPTY)
                    correct = true;
                if (move.from == p.expectedFrom && move.to == p.expectedTo)
                    correct = true;
 
                if (correct)
                {
                    puzzleSolved = true;
                    gameManager.uiManager.ShowDialog("Правильно!", "Задача решена!", "Далее",
                        () => LoadPuzzle(currentPuzzle + 1));
                }
                else
                {
                    engine.UndoLastMove();
                    gameManager.boardRenderer.RefreshPieces();
                    gameManager.boardRenderer.ClearHighlights();
                    gameManager.uiManager.ShowDialog("Неверно!", "Попробуйте другой ход.", "OK", null);
                }
 
                selectedSquare = new Vector2Int(-1, -1);
                selectedMoves.Clear();
                return;
            }
 
            // Выбрать другую фигуру
            int clicked = engine.board[boardPos.x, boardPos.y];
            if (clicked > 0)
            {
                SelectPiece(boardPos);
                return;
            }
 
            selectedSquare = new Vector2Int(-1, -1);
            selectedMoves.Clear();
            gameManager.boardRenderer.ClearHighlights();
 
            return;
        }

        // Выбрать фигуру
        int piece = engine.board[boardPos.x, boardPos.y];
        if (piece > 0)
            SelectPiece(boardPos);
    }
 
    private void SelectPiece(Vector2Int pos)
    {
        selectedSquare = pos;
        selectedMoves = engine.GetLegalMovesForPiece(pos.x, pos.y);
 
        gameManager.boardRenderer.ClearHighlights();
        gameManager.boardRenderer.ShowSelectedHighlight(pos.x, pos.y);
 
        foreach (Move m in selectedMoves)
        {
            bool isCapture = m.captured != CalessEngine.EMPTY;
            gameManager.boardRenderer.ShowMoveHint(m.to.x, m.to.y, isCapture);
        }
    }
}