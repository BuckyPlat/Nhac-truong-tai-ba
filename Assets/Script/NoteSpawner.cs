using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    [Header("Song Settings")]
    public float songBPM = 120f;     // Beats per minute of your song
    public float firstBeatOffset = 0f; // Optional delay before first beat
    public AudioSource musicSource;  // Drag your music here in the Inspector

    [Header("Notes")]
    public GameObject notePrefab;    // Arrow prefab
    public Transform[] spawnPoints;  // Assign 4 spawn points for each arrow direction

    private float beatInterval;      // Seconds per beat
    private float timer;
    private bool hasStarted;

    void Start()
    {
        beatInterval = 60f / songBPM; // Convert BPM to seconds per beat
        timer = firstBeatOffset;

        // Set BeatScroller speed globally for all notes
        BeatScroller[] scrollers = FindObjectsOfType<BeatScroller>();
        foreach (BeatScroller scroller in scrollers)
        {
            scroller.SetTempo(songBPM);
        }
    }

    void Update()
    {
        // Start the song when player presses a key (or do it automatically)
        if (!hasStarted && Input.anyKeyDown)
        {
            hasStarted = true;
            musicSource.Play();
        }

        if (!hasStarted) return;

        // Spawn notes every beat
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            SpawnNote();
            timer += beatInterval;
        }
    }

    void SpawnNote()
    {
        // Pick a random lane (0-3)
        int lane = Random.Range(0, spawnPoints.Length);

        // Spawn note
        GameObject note = Instantiate(notePrefab, spawnPoints[lane].position, Quaternion.identity);

        // Set tempo for this note
        BeatScroller scroller = note.GetComponent<BeatScroller>();
        if (scroller != null)
        {
            scroller.SetTempo(songBPM);
            scroller.hasStarted = true; // Start movement
        }
    }
}
