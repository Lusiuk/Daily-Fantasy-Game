using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HoldToLoadLevel : MonoBehaviour
{
    public float holdDuration = 1f;
    public Image fillCircle;
    public GameObject visualRoot;

    private float holdTimer = 0;
    private bool isHolding = false;
    private bool canFinish = false;

    public bool needQuickAccess; // Set to true if you want the level to load immediately without holding

    public static event Action OnHoldComplete;

    void Start()
    {
        if (visualRoot != null) visualRoot.SetActive(false);
        fillCircle.fillAmount = 0;
    }

    public void SetPlayerInside(bool inside)
    {
        canFinish = inside;
        
        if (visualRoot != null) 
            visualRoot.SetActive(inside || needQuickAccess);

        if (!inside) ResetHold();
    }

    void Update()
    {
        if ((isHolding && canFinish) || (needQuickAccess == true) && isHolding)
        {
            holdTimer += Time.deltaTime;
            fillCircle.fillAmount = holdTimer / holdDuration;

            if (holdTimer >= holdDuration)
            {
                OnHoldComplete?.Invoke();
                ResetHold();
            }
        }
        else if (holdTimer > 0)
        {
            holdTimer -= Time.deltaTime * 2;
            fillCircle.fillAmount = Mathf.Max(0, holdTimer / holdDuration);
        }
    }

    public void OnHold(InputAction.CallbackContext context)
    {
        if (context.started || context.performed) isHolding = true;
        else if (context.canceled) isHolding = false;
    }

    private void ResetHold()
    {
        isHolding = false;
        holdTimer = 0;
        fillCircle.fillAmount = 0;
    }
}