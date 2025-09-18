using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class ChartRecorder : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource musicSource;
    public float bpm = 120f;
    public float offset = 0f; // Timing offset for calibration

    [Header("Input")]
    public KeyCode[] laneKeys = { KeyCode.D, KeyCode.F, KeyCode.J, KeyCode.K };
    public KeyCode startStopKey = KeyCode.Space;
    public KeyCode playbackKey = KeyCode.P;

    [Header("Recording")]
    public string saveFileName = "chart.json";
    public bool quantizeToGrid = true; // Snap to nearest beat subdivision
    public float gridSubdivision = 4f; // 1/4 beats (16th notes)

    [Header("Playback Test")]
    public NoteSpawner noteSpawner; // Reference for testing

    [Header("UI Feedback")]
    public UnityEngine.UI.Text statusText;
    public UnityEngine.UI.Text beatText;
    public UnityEngine.UI.Text noteCountText;

    private List<NoteData> recordedNotes = new List<NoteData>();
    private bool isRecording = false;
    private bool isPlayingBack = false;
    private double recordStartTime;
    private float secPerBeat;

    // Visual feedback
    private Color[] laneColors = { Color.blue, Color.red, Color.yellow, Color.green };

    void Start()
    {
        secPerBeat = 60f / bpm;
        UpdateUI();
    }

    void Update()
    {
        HandleInput();

        if (isRecording)
        {
            UpdateRecording();
        }

        UpdateUI();
    }

    void HandleInput()
    {
        // Start/Stop recording
        if (Input.GetKeyDown(startStopKey))
        {
            if (!isRecording && !isPlayingBack)
            {
                StartRecording();
            }
            else if (isRecording)
            {
                StopRecording();
            }
        }

        // Playback test
        if (Input.GetKeyDown(playbackKey) && !isRecording)
        {
            TestPlayback();
        }

        // Clear recorded notes
        if (Input.GetKeyDown(KeyCode.C) && !isRecording)
        {
            ClearRecording();
        }

        // Save manually
        if (Input.GetKeyDown(KeyCode.S) && !isRecording && recordedNotes.Count > 0)
        {
            SaveChart();
        }
    }

    void UpdateRecording()
    {
        if (!musicSource.isPlaying) return;

        // Use more accurate timing
        double currentTime = AudioSettings.dspTime - recordStartTime;
        float currentBeat = (float)((currentTime - offset) / secPerBeat);

        // Record note inputs
        for (int i = 0; i < laneKeys.Length; i++)
        {
            if (Input.GetKeyDown(laneKeys[i]))
            {
                RecordNote(currentBeat, i);
            }
        }
    }

    void RecordNote(float beat, int lane)
    {
        // Quantize to grid if enabled
        if (quantizeToGrid)
        {
            float subdivision = 1f / gridSubdivision;
            beat = Mathf.Round(beat / subdivision) * subdivision;
        }

        // Check for duplicate (avoid double-recording)
        bool duplicate = recordedNotes.Exists(n =>
            Mathf.Abs(n.beat - beat) < 0.01f && n.lane == lane);

        if (!duplicate)
        {
            NoteData newNote = new NoteData(beat, lane);
            recordedNotes.Add(newNote);

            Debug.Log($"🎹 Note recorded: Beat {beat:F2}, Lane {lane}");

            // Visual feedback (you can expand this)
            ShowNoteFeedback(lane);
        }
    }

    void StartRecording()
    {
        if (musicSource == null)
        {
            Debug.LogError("No AudioSource assigned!");
            return;
        }

        recordedNotes.Clear();
        recordStartTime = AudioSettings.dspTime + 0.1; // Small delay
        musicSource.PlayScheduled(recordStartTime);
        isRecording = true;

        Debug.Log("🎵 Recording started...");
        Debug.Log($"BPM: {bpm}, Grid: 1/{gridSubdivision}, Quantize: {quantizeToGrid}");
    }

    void StopRecording()
    {
        isRecording = false;
        musicSource.Stop();

        // Sort notes by beat
        recordedNotes.Sort((a, b) => a.beat.CompareTo(b.beat));

        // Auto-save
        if (recordedNotes.Count > 0)
        {
            SaveChart();
        }

        Debug.Log($"✅ Recording stopped! {recordedNotes.Count} notes recorded");
    }

    void TestPlayback()
    {
        if (recordedNotes.Count == 0)
        {
            Debug.Log("No notes to playback!");
            return;
        }

        if (noteSpawner == null)
        {
            Debug.Log("No NoteSpawner assigned for playback test!");
            return;
        }

        // Create temporary chart for testing
        Chart testChart = new Chart
        {
            bpm = bpm,
            offset = offset,
            notes = new List<NoteData>(recordedNotes),
            songName = "Test Recording",
            difficulty = "Custom"
        };

        // Load into spawner
        noteSpawner.LoadChart(testChart);
        noteSpawner.StartSong();

        isPlayingBack = true;
        Debug.Log("🎮 Testing recorded chart...");
    }

    void ClearRecording()
    {
        recordedNotes.Clear();
        Debug.Log("🗑️ Recorded notes cleared!");
    }

    void SaveChart()
    {
        Chart chart = new Chart
        {
            bpm = bpm,
            offset = offset,
            notes = new List<NoteData>(recordedNotes),
            songName = Path.GetFileNameWithoutExtension(saveFileName),
            difficulty = "Custom"
        };

        string json = JsonUtility.ToJson(chart, true);

        // Save to StreamingAssets for easier access
        string folderPath = Path.Combine(Application.streamingAssetsPath, "Charts");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string path = Path.Combine(folderPath, saveFileName);
        File.WriteAllText(path, json);

        Debug.Log($"💾 Chart saved: {path}");
        Debug.Log($"📊 Notes: {recordedNotes.Count}, Duration: {GetChartDuration():F1}s");
    }

    float GetChartDuration()
    {
        if (recordedNotes.Count == 0) return 0f;

        float lastBeat = recordedNotes[recordedNotes.Count - 1].beat;
        return lastBeat * secPerBeat;
    }

    void ShowNoteFeedback(int lane)
    {
        // Simple visual feedback - you can enhance this
        Debug.Log($"Lane {lane} hit!");

        // You could add particle effects, UI flashes, etc. here
    }

    void UpdateUI()
    {
        if (statusText != null)
        {
            string status = isRecording ? "🔴 RECORDING" :
                           isPlayingBack ? "🎮 PLAYING" : "⏸️ READY";
            statusText.text = status;
        }

        if (beatText != null && isRecording && musicSource.isPlaying)
        {
            double currentTime = AudioSettings.dspTime - recordStartTime;
            float currentBeat = (float)((currentTime - offset) / secPerBeat);
            beatText.text = $"Beat: {currentBeat:F2}";
        }

        if (noteCountText != null)
        {
            noteCountText.text = $"Notes: {recordedNotes.Count}";
        }
    }

    void OnGUI()
    {
        if (!isRecording) return;

        // Simple on-screen instructions
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("🎵 RECORDING MODE", GUI.skin.box);
        GUILayout.Label($"BPM: {bpm}");
        GUILayout.Label($"Notes: {recordedNotes.Count}");

        if (musicSource.isPlaying)
        {
            double currentTime = AudioSettings.dspTime - recordStartTime;
            float currentBeat = (float)((currentTime - offset) / secPerBeat);
            GUILayout.Label($"Beat: {currentBeat:F2}");
        }

        GUILayout.Label("Controls:");
        for (int i = 0; i < laneKeys.Length; i++)
        {
            GUILayout.Label($"Lane {i}: {laneKeys[i]}");
        }
        GUILayout.Label($"Stop: {startStopKey}");

        GUILayout.EndArea();
    }

    // Utility methods for external use
    public void SetBPM(float newBPM)
    {
        bpm = newBPM;
        secPerBeat = 60f / bpm;
    }

    public void SetOffset(float newOffset)
    {
        offset = newOffset;
    }

    public List<NoteData> GetRecordedNotes()
    {
        return new List<NoteData>(recordedNotes);
    }

    public void LoadExistingChart(Chart chart)
    {
        if (chart != null && !isRecording)
        {
            recordedNotes = new List<NoteData>(chart.notes);
            bpm = chart.bpm;
            offset = chart.offset;
            secPerBeat = 60f / bpm;

            Debug.Log($"📖 Chart loaded: {recordedNotes.Count} notes");
        }
    }
}