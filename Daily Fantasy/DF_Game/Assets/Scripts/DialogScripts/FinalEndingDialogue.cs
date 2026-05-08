using UnityEngine;

[CreateAssetMenu(fileName = "New Final Ending", menuName = "Dialogue System/Final Ending Dialogue")]
public class FinalEndingDialogue : QuestDialogue
{
    [Header("Ending Sequence")]
    public bool hidePlayer = true;
    public Sprite bedFinalSprite;
    public float[] blinkSteps = new float[] { 0.5f, 0f, 0.5f, 0.25f, 0.5f };
    public float blinkStepDuration = 0.5f;
    public Vector2 bedFinalScale = new Vector2(0.66f, 0.66f);

    [System.Serializable]
    public class FinalDialogueOption
    {
        public string[] conditions;          // условия для этого варианта
        public Dialogue dialogue;            // финальный диалог (с переходом на сцену)
    }

    [Header("Final Dialogues (choose first matching condition)")]
    public FinalDialogueOption[] finalDialogueOptions;

    // Для обратной совместимости (если массив пуст – используется этот одиночный диалог)
    public Dialogue finalDialogue;
}