using UnityEngine;
using System.Collections.Generic;

public class NoteSpawner : MonoBehaviour
{
    [Header("Song Settings")]
    public float songBPM = 120f;
    public float firstBeatOffset = 0f;
    public AudioSource musicSource;

    [Header("Notes")]
    public GameObject notePrefab;
    public GameObject[] noteTypePrefabs; // Different note types (normal, hold, special)
    public Transform[] spawnPoints;

    [Header("Timing")]
    public float noteSpeed = 1f;
    public float spawnOffsetTime = 2f; // Spawn notes X seconds before hit time
    public float audioLatency = 0.1f;

    [Header("Debug")]
    public bool showDebugInfo = false;

    private double dspSongStartTime;
    private float secPerBeat;
    private List<NoteData> notes;
    private int nextIndex;
    private bool isPlaying = false;

    // Events
    public System.Action OnSongStart;
    public System.Action OnSongEnd;

    // Properties
    public bool IsPlaying => isPlaying;
    public float SongPosition => isPlaying ? (float)(AudioSettings.dspTime - dspSongStartTime) - audioLatency : 0f;
    public float CurrentBeat => isPlaying ? (SongPosition - firstBeatOffset) / secPerBeat : 0f;
    public int NotesRemaining => notes != null ? notes.Count - nextIndex : 0;

    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        secPerBeat = 60f / songBPM;
        dspSongStartTime = -1;
        notes = new List<NoteData>();
        nextIndex = 0;
        isPlaying = false;

        // Validate spawn points
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("NoteSpawner: No spawn points assigned!");
        }
    }

    public void LoadChart(Chart chart)
    {
        if (chart == null)
        {
            Debug.LogError("NoteSpawner: Chart is null!");
            return;
        }

        songBPM = chart.bpm;
        secPerBeat = 60f / songBPM;
        notes = new List<NoteData>(chart.notes); // Create copy
        nextIndex = 0;

        // Sort notes by beat time to ensure proper spawning order
        notes.Sort((a, b) => a.beat.CompareTo(b.beat));

        // Update GameManager with total note count
        if (GameManager.instance != null)
        {
            GameManager.instance.AddTotalNotes(notes.Count);
        }

        Debug.Log($"Chart loaded: {notes.Count} notes, BPM: {songBPM}");
    }

    void Update()
    {
        // Handle song start
        if (dspSongStartTime < 0 && Input.anyKeyDown)
        {
            StartSong();
        }

        // Don't process if not playing or no chart loaded
        if (!isPlaying || notes == null || notes.Count == 0) return;

        // Calculate current position
        double songPos = AudioSettings.dspTime - dspSongStartTime;
        double currentBeat = (songPos - firstBeatOffset) / secPerBeat;

        // Spawn notes with look-ahead time
        SpawnNotesInRange(currentBeat);

        // Check if song ended
        CheckSongEnd();

        // Debug info
        if (showDebugInfo)
        {
            DrawDebugInfo();
        }
    }

    public void StartSong()
    {
        if (isPlaying) return;

        dspSongStartTime = AudioSettings.dspTime + 0.1f;
        musicSource.PlayScheduled(dspSongStartTime);
        isPlaying = true;

        // Start GameManager
        if (GameManager.instance != null)
        {
            GameManager.instance.StartSong();
        }

        OnSongStart?.Invoke();
        Debug.Log("Song started!");
    }

    public void StopSong()
    {
        if (!isPlaying) return;

        musicSource.Stop();
        isPlaying = false;
        OnSongEnd?.Invoke();
        Debug.Log("Song stopped!");
    }

    public void PauseSong()
    {
        if (!isPlaying) return;

        musicSource.Pause();
        // Note: DSP time continues, so we need to handle pause differently
        // for production games
    }

    public void ResumeSong()
    {
        if (isPlaying) return;

        musicSource.UnPause();
    }

    private void SpawnNotesInRange(double currentBeat)
    {
        // Calculate how many beats ahead we should spawn
        double spawnBeatOffset = spawnOffsetTime / secPerBeat;
        double targetBeat = currentBeat + spawnBeatOffset;

        // Spawn all notes that should appear before target beat
        while (nextIndex < notes.Count && notes[nextIndex].beat <= targetBeat)
        {
            SpawnNote(notes[nextIndex]);
            nextIndex++;
        }
    }

    private void SpawnNote(NoteData noteData)
    {
        // Validate lane
        if (noteData.lane < 0 || noteData.lane >= spawnPoints.Length)
        {
            Debug.LogWarning($"Invalid lane {noteData.lane} for note at beat {noteData.beat}");
            return;
        }

        // Choose prefab based on note type
        GameObject prefabToUse = GetNotePrefab(noteData.noteType);
        if (prefabToUse == null)
        {
            Debug.LogWarning($"No prefab found for note type {noteData.noteType}");
            return;
        }

        // Spawn note
        GameObject note = Instantiate(prefabToUse, spawnPoints[noteData.lane].position, Quaternion.identity);

        // Setup note behavior
        SetupNoteComponents(note, noteData);
    }

    private GameObject GetNotePrefab(NoteType noteType)
    {
        // If we have specific prefabs for different note types
        if (noteTypePrefabs != null && noteTypePrefabs.Length > (int)noteType)
        {
            return noteTypePrefabs[(int)noteType];
        }

        // Fallback to default prefab
        return notePrefab;
    }

    private void SetupNoteComponents(GameObject note, NoteData noteData)
    {
        // Setup BeatScroller
        BeatScroller scroller = note.GetComponent<BeatScroller>();
        if (scroller != null)
        {
            scroller.SetTempo(songBPM);
            scroller.noteSpeed = noteSpeed;
            scroller.hasStarted = true;
            scroller.targetBeat = noteData.beat;
        }

        // Setup NoteObjects if it exists
        NoteObjects noteObj = note.GetComponent<NoteObjects>();
        if (noteObj != null)
        {
            noteObj.Initialize(noteData);
        }

        // Set note lane for reference
        note.name = $"Note_Lane{noteData.lane}_Beat{noteData.beat:F2}";
    }

    private void CheckSongEnd()
    {
        // Check if song finished and all notes processed
        if (!musicSource.isPlaying && nextIndex >= notes.Count)
        {
            isPlaying = false;
            OnSongEnd?.Invoke();
            Debug.Log("Song and all notes completed!");
        }
    }

    private void DrawDebugInfo()
    {
        Debug.Log($"Song Position: {SongPosition:F2}s | Beat: {CurrentBeat:F2} | Notes Remaining: {NotesRemaining}");
    }

    // Manual start for testing or specific game modes
    public void StartSongManual()
    {
        StartSong();
    }

    // Reset for restart
    public void Reset()
    {
        StopSong();
        nextIndex = 0;

        // Clear any existing notes in scene
        GameObject[] existingNotes = GameObject.FindGameObjectsWithTag("Note");
        foreach (GameObject note in existingNotes)
        {
            Destroy(note);
        }
    }

    void OnDrawGizmos()
    {
        if (spawnPoints == null) return;

        // Draw spawn points
        Gizmos.color = Color.green;
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] != null)
            {
                Gizmos.DrawWireSphere(spawnPoints[i].position, 0.5f);

                // Draw lane numbers
#if UNITY_EDITOR
                UnityEditor.Handles.Label(spawnPoints[i].position + Vector3.up * 0.8f, $"Lane {i}");
#endif
            }
        }
    }
}