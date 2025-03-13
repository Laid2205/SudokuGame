using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SudokuGrid : MonoBehaviour
{
    public GameObject cellPrefab;
    private TMP_InputField[,] cells = new TMP_InputField[9, 9];
    public GameObject uiPanel; // Панель с кнопками
    public Button hintButton; // Кнопка подсказки
    private int[,] solutionBoard; // Полное решение судоку
    private int hintsRemaining = 10; // Количество оставшихся подсказок
    public TextMeshProUGUI hintText; // Текстовое поле для отображения количества подсказок

    // Новые переменные
    public TextMeshProUGUI scoreText; // Текстовое поле для счета (сверху справа)
    public TextMeshProUGUI highScoresText; // Таблица рекордов (сверху слева)
    private int score = 0;

    private float elapsedTime = 0f;
    private bool isGameActive = false;
    public TextMeshProUGUI timerText;
    public Button newGameButton; // Кнопка "Новая игра"

    void Start()
    {
        uiPanel.SetActive(true);
        hintButton.gameObject.SetActive(false); // Скрываем кнопку подсказки
        newGameButton.gameObject.SetActive(false); // Скрываем кнопку новой игры
        UpdateHintText();
        UpdateHighScoresText();
    }

    private Sprite CreateBorderSprite(int width, int height, int borderThickness, bool topThick, bool bottomThick, bool leftThick, bool rightThick)
    {
        Texture2D texture = new Texture2D(width, height);
        Color borderColor = Color.black;
        Color fillColor = Color.white; // Цвет фона ячейки

        // Заполняем текстуру прозрачным цветом или белым фоном
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                texture.SetPixel(x, y, fillColor);
            }
        }

        // Верхняя граница
        int topThickness = topThick ? borderThickness : 1;
        for (int x = 0; x < width; x++)
        {
            for (int y = height - topThickness; y < height; y++)
            {
                texture.SetPixel(x, y, borderColor);
            }
        }

        // Нижняя граница
        int bottomThickness = bottomThick ? borderThickness : 1;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < bottomThickness; y++)
            {
                texture.SetPixel(x, y, borderColor);
            }
        }

        // Левая граница
        int leftThickness = leftThick ? borderThickness : 1;
        for (int x = 0; x < leftThickness; x++)
        {
            for (int y = 0; y < height; y++)
            {
                texture.SetPixel(x, y, borderColor);
            }
        }

        // Правая граница
        int rightThickness = rightThick ? borderThickness : 1;
        for (int x = width - rightThickness; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                texture.SetPixel(x, y, borderColor);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
    }
    void GenerateGrid()
    {
        GridLayoutGroup gridLayout = GetComponent<GridLayoutGroup>();
        float cellSize = gridLayout != null ? gridLayout.cellSize.x : 100f; // Размер ячейки
        int borderThickness = 4; // Толщина жирных границ для квадратов 3x3

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                GameObject cell = Instantiate(cellPrefab, transform);
                cells[row, col] = cell.GetComponent<TMP_InputField>();
                cells[row, col].text = "";
                cells[row, col].SetTextWithoutNotify("");
                cells[row, col].ForceLabelUpdate();

                // Определяем, какие границы должны быть жирными
                bool topThick = row % 3 == 0; // Жирная верхняя граница для строк 0, 3, 6
                bool bottomThick = row % 3 == 2 || row == 8; // Жирная нижняя граница для строк 2, 5, 8
                bool leftThick = col % 3 == 0; // Жирная левая граница для столбцов 0, 3, 6
                bool rightThick = col % 3 == 2 || col == 8; // Жирная правая граница для столбцов 2, 5, 8

                // Создаём текстуру с нужными границами
                Sprite borderSprite = CreateBorderSprite((int)cellSize, (int)cellSize, borderThickness, topThick, bottomThick, leftThick, rightThick);

                // Применяем текстуру к ячейке
                Image cellImage = cells[row, col].GetComponent<Image>();
                cellImage.sprite = borderSprite;
                cellImage.type = Image.Type.Simple;
            }
        }
    }


    public void OnDifficultySelected(string difficulty)
    {
        uiPanel.SetActive(false);
        hintButton.gameObject.SetActive(true);
        newGameButton.gameObject.SetActive(true);
        GenerateGrid();
        GenerateNewSudoku(difficulty);
        score = 0; // Обнуляем счет при новой игре
        UpdateScoreText();
        StartGame();
    }

    public void GenerateNewSudoku(string difficulty)
    {
        SudokuGenerator generator = new SudokuGenerator();
        solutionBoard = generator.GenerateSudoku() ?? new int[9, 9];
        int visibleCells = generator.GetVisibleCellsByDifficulty(difficulty);
        int[,] gameBoard = generator.RemoveNumbers(solutionBoard, visibleCells);

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                TMP_InputField inputField = cells[row, col];
                inputField.onValueChanged.RemoveAllListeners();
                if (gameBoard[row, col] != 0)
                {
                    cells[row, col].text = gameBoard[row, col].ToString();
                    cells[row, col].interactable = false;
                    inputField.GetComponent<Image>().color = Color.white;
                }
                else
                {
                    cells[row, col].text = "";
                    cells[row, col].interactable = true;
                    inputField.GetComponent<Image>().color = Color.white;

                    int r = row, c = col;
                    inputField.onValueChanged.AddListener(delegate { ValidateInput(r, c, inputField); });
                }
            }
        }
    }

    private void ValidateInput(int row, int col, TMP_InputField inputField)
    {
        if (inputField.text.Length == 1 && int.TryParse(inputField.text, out int enteredNumber))
        {
            if (enteredNumber == solutionBoard[row, col])
            {
                inputField.GetComponent<Image>().color = Color.green;
                score += 5; // +5 очков за правильный ответ
                UpdateScoreText();
                UpdateAvailableNumbers();
                CheckGameCompletion();
            }
            else
            {
                inputField.GetComponent<Image>().color = Color.red;
                score -= 1; // -1 очко за ошибку
                UpdateScoreText();
            }
        }
        else
        {
            inputField.GetComponent<Image>().color = Color.white;
        }
    }

    public void UseHint()
    {
        if (hintsRemaining <= 0)
        {
            Debug.Log("No hint!");
            return;
        }

        for (int attempt = 0; attempt < 100; attempt++)
        {
            int row = UnityEngine.Random.Range(0, 9);
            int col = UnityEngine.Random.Range(0, 9);

            if (cells[row, col].text == "")
            {
                TMP_InputField inputField = cells[row, col];

                // Убираем обработчики перед изменением текста, чтобы не начислялись +5 очков
                inputField.onValueChanged.RemoveAllListeners();

                // Устанавливаем значение из решения и делаем ячейку неактивной
                inputField.text = solutionBoard[row, col].ToString();
                inputField.interactable = false;

                // Минус очки за подсказку (-3 за первую, -4 за вторую и т. д.)
                score -= (3 + (10 - hintsRemaining));
                hintsRemaining--;

                UpdateHintText();
                UpdateScoreText();
                UpdateAvailableNumbers();

                return;
            }
        }
    }

    private void UpdateHintText()
    {
        if (hintText != null)
        {
            hintText.text = "Hint: " + hintsRemaining;
        }
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

    private void SaveScore()
    {
        int[] topScores = GetTopScores();
        Array.Sort(topScores);
        Array.Reverse(topScores);

        if (score > topScores[2]) // Если новый счет выше третьего рекорда
        {
            topScores[2] = score;
            Array.Sort(topScores);
            Array.Reverse(topScores);

            PlayerPrefs.SetInt("Top1", topScores[0]);
            PlayerPrefs.SetInt("Top2", topScores[1]);
            PlayerPrefs.SetInt("Top3", topScores[2]);
            PlayerPrefs.Save();
        }
    }

    private int[] GetTopScores()
    {
        return new int[]
        {
            PlayerPrefs.GetInt("Top1", 0),
            PlayerPrefs.GetInt("Top2", 0),
            PlayerPrefs.GetInt("Top3", 0)
        };
    }

    private void UpdateHighScoresText()
    {
        if (highScoresText != null)
        {
            int[] topScores = GetTopScores();
            highScoresText.text = "High Scores:\n1. " + topScores[0] + "\n2. " + topScores[1] + "\n3. " + topScores[2];
        }
    }

    private void CheckGameCompletion()
    {
        foreach (var cell in cells)
        {
            if (cell.text == "")
                return; // Игра не завершена
        }

        isGameActive = false; // Останавливаем таймер
        int timeBonus = Mathf.Max(0, 500 - Mathf.FloorToInt(elapsedTime)); // Очки за время
        score += timeBonus;

        SaveScore();
        UpdateHighScoresText();
        Debug.Log("Game Completed! Final Score: " + score);
    }

    void StartGame()
    {
        elapsedTime = 0f;
        isGameActive = true;
        StartCoroutine(Timer());
    }

    IEnumerator Timer()
    {
        while (isGameActive)
        {
            elapsedTime += Time.deltaTime;
            timerText.text = "Time: " + Mathf.FloorToInt(elapsedTime).ToString();
            yield return null;
        }
    }

    void UpdateAvailableNumbers()
    {
        int[] counts = new int[10]; // Массив для подсчета чисел от 1 до 9

        foreach (var cell in cells)
        {
            if (int.TryParse(cell.text, out int num))
                counts[num]++;
        }

        // Проходим по всем ячейкам и меняем цвет текста, если число встречается 9 раз
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                TMP_InputField inputField = cells[row, col];

                if (int.TryParse(inputField.text, out int num) && counts[num] >= 9)
                {
                    inputField.textComponent.color = Color.gray; // Делаем текст серым
                }
                else
                {
                    inputField.textComponent.color = Color.black; // Возвращаем обычный цвет
                }
            }
        }
    }
    public void RestartGame()
    {
        StopAllCoroutines(); // Останавливаем таймер
        isGameActive = false;

        // Очищаем поле и скрываем элементы
        foreach (var cell in cells)
        {
            Destroy(cell.gameObject);
        }

        // Сброс подсказок
        hintsRemaining = 10;
        UpdateHintText();

        uiPanel.SetActive(true); // Показываем выбор сложности
        hintButton.gameObject.SetActive(false);
        newGameButton.gameObject.SetActive(false); // Скрываем кнопку новой игры
    }
}
