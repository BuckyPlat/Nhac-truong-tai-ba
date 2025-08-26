using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    private SpriteRenderer theSR; 
    public Sprite defaultImage; 
    public Sprite pressedImage; 
    
    public KeyCode KeyToPress;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        theSR = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyToPress))
        {
            theSR.sprite = pressedImage;
        }
        if (Input.GetKeyUp(KeyToPress))
        {
            theSR.sprite = defaultImage;
        }
    }
}
