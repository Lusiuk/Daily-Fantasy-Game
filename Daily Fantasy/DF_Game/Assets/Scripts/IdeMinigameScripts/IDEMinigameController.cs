using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class IDEMinigameController : MonoBehaviour
{

    [Header("UI References")]
    public Transform availableBlocksContainer; 
    private Transform buildZoneContainer;

    public List<Transform> BuildZoneContainers;

    public TMP_Text titleText;
    public TMP_Text resultText;
    public Button checkButton;
    public Button resetButton;

    public GameObject Answer;
    public GameObject AnswerSlot;

    private bool isChecking = false;

    [Header("Levels Configuration")]
    public GameLevel[] levels;

    private int currentLevelIndex = 0;
    private GameLevel currentLevel;
    private List<DraggableAnswer> allBlocks = new List<DraggableAnswer>();

    [Header("Completion Dialogue")]
    public Dialogue completionDialogue;

    [System.Serializable]
    public class GameLevel
    {
        public string levelName;
        public string[] correctCodeLines;
        public string description; // Подсказка для игрока
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
         // Проверка всех референсов
        if (availableBlocksContainer == null) Debug.LogError("availableBlocksContainer не назначен");
        if (BuildZoneContainers == null) Debug.LogError("BuildZoneContainers не назначен");
        if (titleText == null) Debug.LogError("titleText не назначен");
        if (resultText == null) Debug.LogError("resultText не назначен");
        if (checkButton == null) Debug.LogError("checkButton не назначен");
        if (resetButton == null) Debug.LogError("resetButton не назначен");
         
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
            buildZoneContainer = BuildZoneContainers[0];
            LoadLevel(0);
        }
    }

 private void ClearContainers()
{
    ClearContainer(availableBlocksContainer);
    
    ClearBlocksFromBuildZone();
    
    allBlocks.Clear();
}


private void ClearBlocksFromBuildZone()
{
    if (buildZoneContainer == null) return;
    
    foreach (Transform slot in buildZoneContainer.GetChild(0))
    {
        if (slot.childCount > 0)
        {
            Destroy(slot.GetChild(0).gameObject);
        }
    }
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
            resultText.text = "Все уровни пройдены!";
            return;
        }

        currentLevelIndex = levelIndex;
        currentLevel = levels[levelIndex];
        if (buildZoneContainer != null)
        {
            buildZoneContainer.gameObject.SetActive(false);
        }
        buildZoneContainer = BuildZoneContainers[levelIndex];
        buildZoneContainer.gameObject.SetActive(true);

        isChecking = false;

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
        
    }

 private void CreateCodeBlock(string codeLine, Transform parent, int originalIndex)
    {
        Debug.Log($"Создание блока: {codeLine}");
        
        GameObject slot = Instantiate(AnswerSlot,parent);
        GameObject blockObj = Instantiate(Answer, slot.transform);
        var textComponent = blockObj.GetComponentInChildren<TMP_Text>();
         
        textComponent.text = codeLine;
        
        var block = blockObj.GetComponent<DraggableAnswer>();
        if (block == null)
        {
            block = blockObj.AddComponent<DraggableAnswer>();
            Debug.LogWarning("DraggableAnswer добавлен динамически");
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
        if (isChecking) return;

        isChecking = true;

        List<string> playerSolution = new List<string>();

        DraggableAnswer[] blocks = buildZoneContainer.GetComponentsInChildren<DraggableAnswer>();

        List<DraggableAnswer> sortedBlocks = blocks.OrderBy(b => b.transform.GetSiblingIndex()).ToList();

        foreach (var block in sortedBlocks)
        {
            playerSolution.Add(block.codeLine);
        }

        bool isCorrect = playerSolution.SequenceEqual(currentLevel.correctCodeLines);

        if (isCorrect)
        {
            resultText.text = "Отлично! Код работает!";
            resultText.color = Color.green;

            if (currentLevelIndex == levels.Length - 1)
            {
                resultText.text = "Все уровни пройдены!";
                StartCoroutine(ShowCompletionDialogueAfterDelay());
            }
            else
            {
                Invoke("LoadNextLevel", 2f);
            }
        }
        else
        {
            resultText.text = "Ошибка в порядке строк. Попробуй ещё!";
            resultText.color = new Color(1, 0.5f, 0.5f);

            isChecking = false;
        }
    }

    // Показывает завершенный диалог
    private System.Collections.IEnumerator ShowCompletionDialogueAfterDelay()
    {
        yield return new WaitForSeconds(2f);

        // Отключаем всё
        if (resultText != null)
        {
            resultText.text = "";
        }

        if (checkButton != null) checkButton.interactable = false;
        if (resetButton != null) resetButton.interactable = false;

        foreach (var block in allBlocks)
        {
            var draggable = block.GetComponent<DraggableAnswer>();

            if (draggable != null)
            {
                draggable.enabled = false;
            }
        }

        // Устанавливаем флаг завершения мини-игры
        GameState.IsMinigameCompleted = true;
        Debug.Log("Мини-игра отмечена как завершённая в GameState");

        // Показываем диалог
        if (completionDialogue != null && DialogueSystem.Instance != null)
        {
            DialogueSystem.Instance.ShowDialogue(completionDialogue);
            Debug.Log("Показываем диалог завершения мини-игры");
        }

        isChecking = false;
    }

    private void ResetLevel()
{
    LoadLevel(currentLevelIndex);
}
    
    private void LoadNextLevel()
    {
        LoadLevel(currentLevelIndex + 1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
