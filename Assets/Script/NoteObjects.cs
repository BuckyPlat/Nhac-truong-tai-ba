using UnityEngine;

public class NoteObjects : MonoBehaviour
{
    public bool canBePressed;
    public KeyCode keyToPress;

    public GameObject hitEffect, goodEffect, perfectEffect, missEffect;

    [Header("Timing")]
    public float perfectThreshold = 0.05f;  // ±0.05 units for perfect
    public float goodThreshold = 0.25f;     // ±0.25 units for good

    private NoteData noteData;
    private bool hasBeenHit = false;

    void Start()
    {

    }

    public void Initialize(NoteData data)
    {
        noteData = data;
    }

    void Update()
    {
        if (Input.GetKeyDown(keyToPress))
        {
            if (canBePressed && !hasBeenHit)
            {
                HitNote();
            }
        }
    }

    void HitNote()
    {
        hasBeenHit = true;
        gameObject.SetActive(false);

        // Calculate hit accuracy based on distance from target position
        float distance = Mathf.Abs(transform.position.y);

        if (distance <= perfectThreshold)
        {
            Debug.Log("Perfect Hit!");
            GameManager.instance.PerfectHit();
            if (perfectEffect != null)
                Instantiate(perfectEffect, transform.position, perfectEffect.transform.rotation);
        }
        else if (distance <= goodThreshold)
        {
            Debug.Log("Good Hit!");
            GameManager.instance.GoodHit();
            if (goodEffect != null)
                Instantiate(goodEffect, transform.position, goodEffect.transform.rotation);
        }
        else
        {
            Debug.Log("Normal Hit!");
            GameManager.instance.NormalHit();
            if (hitEffect != null)
                Instantiate(hitEffect, transform.position, hitEffect.transform.rotation);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Activator")
        {
            canBePressed = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Activator" && gameObject.activeSelf && !hasBeenHit)
        {
            canBePressed = false;

            // Note missed
            GameManager.instance.NoteMissed();
            if (missEffect != null)
                Instantiate(missEffect, transform.position, missEffect.transform.rotation);

            gameObject.SetActive(false);
        }
    }

    // Alternative method name for compatibility
    public void NoteMissed()
    {
        if (!hasBeenHit)
        {
            GameManager.instance.NoteMissed();
            if (missEffect != null)
                Instantiate(missEffect, transform.position, missEffect.transform.rotation);
            gameObject.SetActive(false);
        }
    }
}