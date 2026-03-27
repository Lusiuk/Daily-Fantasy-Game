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
        switch (SceneManager.GetActiveScene().name)
        {
            case "MainRoom":
                Mother.SetActive(!completed);
                break;

            case "KitchenRoom":
                Mother.SetActive(completed);
                break;
        }
    }
}