using UnityEngine;

public class Buttons : MonoBehaviour
{
    private SpriteRenderer SR;
    public Sprite defaultImage;
    public Sprite pressedImage;

    public KeyCode keyToPress;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SR = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(keyToPress))
        {
            SR.sprite = pressedImage;
        }

        if (Input.GetKeyUp(keyToPress))
        {
            SR.sprite = defaultImage;
        }
    }
}
