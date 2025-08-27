using UnityEngine;

public class BeatScroller : MonoBehaviour
{
    private float beatTempo; // Movement speed per second
    public bool hasStarted;

    public void SetTempo(float bpm)
    {
        beatTempo = bpm / 60f; // Convert BPM to units/second
    }

    void Update()
    {
        if (hasStarted)
        {
            transform.position -= new Vector3(0f, beatTempo * Time.deltaTime, 0f);
        }
    }
}
