using UnityEngine;

public class BeatScroller : MonoBehaviour
{
    private float beatTempo; // BPM converted to units/second
    public bool hasStarted = false;

    [Header("Note Properties")]
    public float noteSpeed = 1f;        // Speed multiplier
    public float targetBeat = 0f;       // Target beat for this note

    [Header("Movement")]
    public Vector3 moveDirection = Vector3.down; // Direction to move (default: down)
    public float baseSpeed = 5f; // Base movement speed

    private float actualSpeed;

    public void SetTempo(float bpm)
    {
        beatTempo = bpm / 60f; // BPM to beats per second
        CalculateSpeed();
    }

    void Start()
    {
        CalculateSpeed();
    }

    void CalculateSpeed()
    {
        // Calculate actual movement speed based on BPM and noteSpeed
        actualSpeed = baseSpeed * beatTempo * noteSpeed;
    }

    void Update()
    {
        if (hasStarted)
        {
            // Move the note
            transform.position += moveDirection * actualSpeed * Time.deltaTime;

            // Optional: Destroy note if it goes too far off screen
            CheckBounds();
        }
    }

    void CheckBounds()
    {
        // Destroy note if it goes too far below the screen
        if (transform.position.y < -10f) // Adjust based on your scene
        {
            // Note missed - could trigger miss event here
            Destroy(gameObject);
        }
    }

    // Method to set note speed specifically
    public void SetNoteSpeed(float speed)
    {
        noteSpeed = speed;
        CalculateSpeed();
    }

    // Method to set target beat
    public void SetTargetBeat(float beat)
    {
        targetBeat = beat;
    }

    // Get current beat position (useful for timing calculations)
    public float GetCurrentBeat()
    {
        if (GameManager.instance != null)
        {
            return GameManager.instance.SongPosition / (60f / beatTempo);
        }
        return 0f;
    }
}