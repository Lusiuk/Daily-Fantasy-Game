using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomController : MonoBehaviour
{

    public GameObject Mother;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        switch (SceneManager.GetActiveScene().name)
        {
            case "MainRoom":
                if (GameState.IsRhythmGame1Completed)
                {
                    Mother.SetActive(false);
                }
                else
                    Mother.SetActive(true);

                break;
            case "KitchenRoom":
                if (GameState.IsRhythmGame1Completed)
                {
                    Mother.SetActive(true);
                }
                else
                    Mother.SetActive(false);

                break;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
