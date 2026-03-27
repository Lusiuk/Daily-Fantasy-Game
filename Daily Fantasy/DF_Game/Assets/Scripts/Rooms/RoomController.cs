using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomController : MonoBehaviour
{
    public GameObject Mother;
    private Collider2D motherCollider;    
    private SpriteRenderer motherSprite;

    void Start()
    {
        bool completed = GameState.IsRhythmGame1Completed;
        motherCollider = Mother.GetComponent<Collider2D>();
        motherSprite = Mother.GetComponent<SpriteRenderer>();
        switch (SceneManager.GetActiveScene().name)
        {
            case "MainRoom":
                SetMotherVisible(!completed);
                break;

            case "KitchenRoom":
                SetMotherVisible(completed);
                break;
        }
    }

    void SetMotherVisible(bool visible)
    {
        if (motherSprite != null) motherSprite.enabled = visible;
        if (motherCollider != null) motherCollider.enabled = visible;
    }
}