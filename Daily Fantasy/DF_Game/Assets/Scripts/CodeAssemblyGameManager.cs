using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CodeAssemblyGameManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform availableBlocksContainer; // Content в Grid Layout
    public Transform buildZoneContainer;       // Content в Vertical Layout
    public TMP_Text titleText;
    public TMP_Text resultText;
    public Button checkButton;
    public Button resetButton;
    public ScrollRect availableBlocksScrollRect; // Добавлено поле для ScrollRect

    [Header("Prefabs")]
    public GameObject codeBlockPrefab;

    [Header("Levels Configuration")]
    public GameLevel[] levels;

    private int currentLevelIndex = 0;
    private GameLevel currentLevel;
    private List<DraggableCodeBlock> allBlocks = new List<DraggableCodeBlock>();

    [System.Serializable]
    public class GameLevel
    {
        public string levelName;
        public string[] correctCodeLines;
        public string description; // Подсказка для игрока
    }

    private void Start()
    {
        // Проверка всех референсов
        if (availableBlocksContainer == null) Debug.LogError("availableBlocksContainer не назначен");
        if (buildZoneContainer == null) Debug.LogError("buildZoneContainer не назначен");
        if (titleText == null) Debug.LogError("titleText не назначен");
        if (resultText == null) Debug.LogError("resultText не назначен");
        if (checkButton == null) Debug.LogError("checkButton не назначен");
        if (resetButton == null) Debug.LogError("resetButton не назначен");
        if (codeBlockPrefab == null) Debug.LogError("codeBlockPrefab не назначен");
        if (availableBlocksScrollRect == null) Debug.LogError("availableBlocksScrollRect не назначен");
        
        // Исправление: назначение Content для ScrollRect если он не задан
        if (availableBlocksScrollRect != null && availableBlocksScrollRect.content == null)
        {
            availableBlocksScrollRect.content = availableBlocksContainer.GetComponent<RectTransform>();
        }

        // Исправление: удаление дублирующихся подписок
        checkButton.onClick.RemoveAllListeners();
        resetButton.onClick.RemoveAllListeners();
        
        if (checkButton != null) checkButton.onClick.AddListener(CheckSolution);
        if (resetButton != null) resetButton.onClick.AddListener(ResetLevel);
        
        // Начальная очистка
        ClearContainers();
        
        // Инициализация уровней
        if (levels == null || levels.Length <= 0) 
        {
            Debug.LogError("Уровни не настроены");
        }
        else 
        {
            LoadLevel(0);
        }
    }

    private void ClearContainers()
    {
        ClearContainer(availableBlocksContainer);
        ClearContainer(buildZoneContainer);
        allBlocks.Clear();
    }

    private void ClearContainer(Transform container)
    {
        if (container == null) return;
        
        foreach (Transform child in container)
            Destroy(child.gameObject);
    }

    private void LoadLevel(int levelIndex)
    {
        ClearContainers();
        
        if (levelIndex < 0 || levelIndex >= levels.Length)
        {
            resultText.text = "🎉 Все уровни пройдены!";
            return;
        }

        currentLevelIndex = levelIndex;
        currentLevel = levels[levelIndex];
        
        Debug.Log($"Загружается уровень: {currentLevel.levelName}");
        Debug.Log($"Количество строк кода: {currentLevel.correctCodeLines.Length}");
        
        // Обновляем UI
        titleText.text = currentLevel.levelName;
        resultText.text = currentLevel.description;
        resultText.color = Color.white;

        // Создаём блоки кода (перемешанные)
        List<string> shuffledLines = new List<string>(currentLevel.correctCodeLines);
        Shuffle(shuffledLines);

        // Создаём блоки в "доступных"
        for (int i = 0; i < shuffledLines.Count; i++)
        {
            CreateCodeBlock(shuffledLines[i], availableBlocksContainer, i);
        }
        
        // Принудительное обновление ScrollRect
        if (availableBlocksScrollRect != null)
        {
            availableBlocksScrollRect.content = availableBlocksContainer.GetComponent<RectTransform>();
            LayoutRebuilder.ForceRebuildLayoutImmediate(availableBlocksContainer.GetComponent<RectTransform>());
        }
    }

    private void CreateCodeBlock(string codeLine, Transform parent, int originalIndex)
    {
        Debug.Log($"Создание блока: {codeLine}");
        
        if (codeBlockPrefab == null)
        {
            Debug.LogError("codeBlockPrefab не назначен!");
            return;
        }
        
        GameObject blockObj = Instantiate(codeBlockPrefab, parent);
        var textComponent = blockObj.GetComponentInChildren<TMP_Text>();
        
        if (textComponent == null)
        {
            Debug.LogError("В блоке кода не найден TMP_Text!");
            // Создаем текст вручную как временное решение
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(blockObj.transform);
            textObj.transform.localScale = Vector3.one;
            
            textComponent = textObj.AddComponent<TMP_Text>();
            textComponent.font = Resources.GetBuiltinResource<TMP_FontAsset>("Arial.ttf");
            textComponent.fontSize = 24;
            textComponent.alignment = TextAlignmentOptions.Left;
            //textComponent.textWrappingMode = TextWrappingMode.NoWrap;
            
            // Настраиваем RectTransform
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            if (textRect != null)
            {
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = new Vector2(10, 0);
                textRect.offsetMax = new Vector2(-10, 0);
            }
        }
        
        textComponent.text = codeLine;
        //textComponent.textWrappingMode = TextWrappingMode.NoWrap;
        
        var block = blockObj.GetComponent<DraggableCodeBlock>();
        if (block == null)
        {
            block = blockObj.AddComponent<DraggableCodeBlock>();
            Debug.LogWarning("DraggableCodeBlock добавлен динамически");
        }
        
        block.Initialize(codeLine, originalIndex);
        allBlocks.Add(block);
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }

    private void CheckSolution()
    {
        // Получаем все блоки в зоне сборки в порядке от верха к низу
        List<string> playerSolution = new List<string>();
        
        foreach (Transform child in buildZoneContainer)
        {
            DraggableCodeBlock block = child.GetComponent<DraggableCodeBlock>();
            if (block != null)
                playerSolution.Add(block.codeLine);
        }

        // Сравниваем с правильным решением
        bool isCorrect = playerSolution.SequenceEqual(currentLevel.correctCodeLines);

        if (isCorrect)
        {
            resultText.text = "✅ Отлично! Код работает!";
            resultText.color = Color.green;
            
            // Автоматический переход на следующий уровень через 2 секунды
            Invoke("LoadNextLevel", 2f);
        }
        else
        {
            resultText.text = "❌ Ошибка в порядке строк. Попробуй ещё!";
            resultText.color = new Color(1, 0.5f, 0.5f);
            
            // Подсветка первой неправильной строки
            //HighlightFirstMistake(playerSolution);
        }
    }

    // private void HighlightFirstMistake(List<string> playerSolution)
    // {
    //     // Сначала сбрасываем цвета всех блоков
    //     foreach (Transform child in buildZoneContainer)
    //     {
    //         Image blockImage = child.GetComponent<Image>();
    //         if (blockImage != null)
    //         {
    //             blockImage.color = Color.white; // исходный цвет
    //         }
    //     }
        
    //     // Находим первую ошибку
    //     for (int i = 0; i < Mathf.Min(playerSolution.Count, currentLevel.correctCodeLines.Length); i++)
    //     {
    //         if (playerSolution[i] != currentLevel.correctCodeLines[i])
    //         {
    //             // Подсвечиваем неправильный блок
    //             if (i < buildZoneContainer.childCount)
    //             {
    //                 Transform wrongBlock = buildZoneContainer.GetChild(i);
    //                 Image blockImage = wrongBlock.GetComponent<Image>();
    //                 if (blockImage != null)
    //                 {
    //                     blockImage.color = new Color(1, 0.5f, 0.5f); // красноватый цвет
    //                 }
    //             }
                
    //             Debug.Log($"Ошибка в строке {i+1}: ожидалось '{currentLevel.correctCodeLines[i]}', получено '{playerSolution[i]}'");
    //             break;
    //         }
    //     }
    // }

    private void ResetLevel()
    {
        // Сбрасываем все блоки из зоны сборки обратно в исходное положение
        List<DraggableCodeBlock> blocksToReset = new List<DraggableCodeBlock>();
        
        foreach (Transform child in buildZoneContainer)
        {
            DraggableCodeBlock block = child.GetComponent<DraggableCodeBlock>();
            if (block != null)
            {
                blocksToReset.Add(block);
            }
        }
        
        foreach (var block in blocksToReset)
        {
            block.ResetPosition();
        }
        
        if (resultText != null)
        {
            resultText.text = currentLevel?.description ?? "Уровень не загружен";
            resultText.color = Color.white;
        }
    }
    
    private void LoadNextLevel()
    {
        LoadLevel(currentLevelIndex + 1);
    }
}