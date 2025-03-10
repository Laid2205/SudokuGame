using System;
using System.Collections.Generic;
using UnityEngine;

public class SudokuGenerator
{
    private int[,] board = new int[9, 9];

    public int[,] GenerateSudoku()
    {
        board = new int[9, 9]; // Очистка массива перед заполнением
        FillBoard();
        return (int[,])board.Clone(); // Возвращаем копию, чтобы избежать изменений оригинала
    }

    private bool FillBoard()
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (board[row, col] == 0)
                {
                    List<int> numbers = GetShuffledNumbers();

                    foreach (int num in numbers)
                    {
                        if (IsValidMove(row, col, num))
                        {
                            board[row, col] = num;

                            if (FillBoard())
                                return true;

                            board[row, col] = 0;
                        }
                    }
                    return false;
                }
            }
        }
        return true;
    }

    private List<int> GetShuffledNumbers()
    {
        List<int> numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        System.Random rng = new System.Random();
        for (int i = numbers.Count - 1; i > 0; i--)
        {
            int swapIndex = rng.Next(i + 1);
            (numbers[i], numbers[swapIndex]) = (numbers[swapIndex], numbers[i]);
        }
        return numbers;
    }

    private bool IsValidMove(int row, int col, int num)
    {
        for (int i = 0; i < 9; i++)
        {
            if (board[row, i] == num || board[i, col] == num)
                return false;
        }

        int boxRow = row / 3 * 3;
        int boxCol = col / 3 * 3;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (board[boxRow + i, boxCol + j] == num)
                    return false;
            }
        }
        return true;
    }
    public int[,] RemoveNumbers(int[,] fullBoard, int visibleCells)
    {
        int[,] board = (int[,])fullBoard.Clone();
        System.Random rng = new System.Random();
        int totalCells = 81;
        int cellsToRemove = totalCells - visibleCells; // Точное количество ячеек, которые нужно убрать

        List<(int, int)> filledCells = new List<(int, int)>();
        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                if (board[r, c] != 0)
                {
                    filledCells.Add((r, c)); // Сохраняем координаты заполненных клеток
                }
            }
        }

        while (cellsToRemove > 0 && filledCells.Count > 0)
        {
            int index = rng.Next(filledCells.Count);
            (int row, int col) = filledCells[index];

            board[row, col] = 0;
            filledCells.RemoveAt(index);
            cellsToRemove--;
        }

        int countAfter = 0;
        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                if (board[r, c] != 0) countAfter++;
            }
        }

        return board;
    }
    public int GetVisibleCellsByDifficulty(string difficulty)
    {
        int min, max;
        switch (difficulty.ToLower()) // Учитываем возможные ошибки регистра
        {
            case "beginner":
                min = 36; max = 40;
                break;
            case "medium":
                min = 32; max = 36;
                break;
            case "hard":
                min = 28; max = 32;
                break;
            case "expert":
                min = 24; max = 28;
                break;
            default:
                min = 36; max = 40; // Значение по умолчанию
                break;
        }
        int result = UnityEngine.Random.Range(min, max + 1);
        return result;
    }
}
