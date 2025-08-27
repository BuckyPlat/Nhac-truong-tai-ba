using UnityEngine;

public class NoteObjects : MonoBehaviour
{
    public bool canBePressed;

    public KeyCode keyToPress;

    public GameObject hitEffect, goodEffect, perfectEffect, missEffect;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(keyToPress))
        {
            if(canBePressed)
            {
                gameObject.SetActive(false);

                if (!GameManager.instance.startPlaying)
                {
                    GameManager.instance.startPlaying = true;
                    GameManager.instance.theBS.hasStarted = true;
                    GameManager.instance.theMusic.Play();

                    // Start all BeatScrollers
                    BeatScroller[] allNotes = FindObjectsOfType<BeatScroller>();
                    foreach (BeatScroller note in allNotes)
                    {
                        note.hasStarted = true;
                    }
                }

                //GameManager.instance.NoteHit();

                if (Mathf.Abs(transform.position.y) > 0.25)
                {
                    Debug.Log("Hit");
                    GameManager.instance.NormalHit();
                    Instantiate(hitEffect, transform.position, hitEffect.transform.rotation);
                }else if(Mathf.Abs(transform.position.y)> 0.05f)
                {
                    Debug.Log("Good");
                    GameManager.instance.GoodHit();
                    Instantiate(goodEffect, transform.position, goodEffect.transform.rotation);
                }
                else
                {
                    Debug.Log("Perfect");
                    GameManager.instance.PerfectHit();
                    Instantiate(perfectEffect, transform.position, perfectEffect.transform.rotation);
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag == "Activator")
        {
            canBePressed = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Activator"&& gameObject.activeSelf)
        {
            canBePressed = false;

            GameManager.instance.NotMissed();
            Instantiate(missEffect, transform.position, missEffect.transform.rotation);
        }
    }
}
