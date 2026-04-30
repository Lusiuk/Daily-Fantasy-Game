using UnityEngine;

public class VisualStateController : MonoBehaviour
{
    [System.Serializable]
    public class VisualState
    {
        public string stateName;
        public string[] conditions;      // условия активации
        public Sprite sprite;            // если указан, меняет спрайт на SpriteRenderer
        public bool setActive = true;    // если указан targetObject, включает/выключает его
        public GameObject targetObject;  // если не указан, действует на сам gameObject
    }

    [Header("Visual States")]
    public VisualState[] visualStates;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        GameState.OnFlagChanged += OnFlagChanged;
        UpdateVisual();
    }

    void OnEnable()
    {
        GameState.OnFlagChanged += OnFlagChanged;
        UpdateVisual();
    }

    void OnDisable()
    {
        GameState.OnFlagChanged -= OnFlagChanged;
    }

    private void OnFlagChanged(string flagName)
    {
        UpdateVisual();
    }

    public void UpdateVisual()
    {
        foreach (var state in visualStates)
        {
            if (GameState.AreFlagsSatisfied(state.conditions))
            {
                if (state.sprite != null && spriteRenderer != null)
                    spriteRenderer.sprite = state.sprite;

                GameObject target = state.targetObject != null ? state.targetObject : gameObject;
                if (target != null)
                    target.SetActive(state.setActive);
                break;
            }
        }
    }

    private void OnDestroy()
    {
        GameState.OnFlagChanged -= OnFlagChanged;
    }
}