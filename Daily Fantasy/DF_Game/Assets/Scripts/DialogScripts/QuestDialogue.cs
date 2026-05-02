using UnityEngine;

[CreateAssetMenu(fileName = "New Quest Dialogue", menuName = "Dialogue System/Quest Dialogue")]
public class QuestDialogue : Dialogue
{
    [Header("Quest Settings")]
    [TextArea(1, 3)]
    public string questionText;          // Текст вопроса (показывается после текста диалога, если есть)

    public Outcome positiveOutcome;      // Исход при ответе "Да"
    public Outcome negativeOutcome;      // Исход при ответе "Нет"

    public bool playEyeBlinkOnYes = false;   // эффект при ответе Да
    public bool playEyeBlinkOnNo = false;    // эффект при ответе Нет
}