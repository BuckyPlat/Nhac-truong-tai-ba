using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Audio")]
    public AudioSource theMusic;

    [Header("Scoring")]
    public int currentScore;
    public int scorePerNote = 100;
    public int scorePerGoodNote = 125;
    public int scorePerPerfectNote = 150;

    [Header("Multiplier")]
    public int currentMultiplier;
    public int multiplierTracker;
    public int[] multiplierThresholds;
    public int maxMultiplier = 8; // Cap multiplier

    [Header("Combo & Stats")]
    public int currentCombo;
    public int maxCombo;
    public int totalNotes;
    public int notesHit;
    public int perfectHits;
    public int goodHits;
    public int normalHits;
    public int missedNotes;

    [Header("UI")]
    public Text scoreText;
    public Text multiText;
    public Text comboText;
    public Text accuracyText;

    [Header("Audio Sync")]
    public float audioLatency = 0.1f; // Adjustable latency compensation

    // Events for other systems to listen
    public System.Action<int> OnScoreChanged;
    public System.Action<int> OnComboChanged;
    public System.Action OnNoteMissed;

    // Properties for external access
    public float SongPosition => (float)(AudioSettings.dspTime - songStartTime) - audioLatency;
    public float Accuracy => totalNotes > 0 ? (float)notesHit / totalNotes * 100f : 100f;
    public bool IsPlaying => theMusic != null && theMusic.isPlaying;

    private double songStartTime;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Keep between scenes if needed
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InitializeGame();
    }

    public void InitializeGame()
    {
        currentMultiplier = 1;
        currentCombo = 0;
        currentScore = 0;
        multiplierTracker = 0;

        // Reset stats
        totalNotes = 0;
        notesHit = 0;
        perfectHits = 0;
        goodHits = 0;
        normalHits = 0;
        missedNotes = 0;
        maxCombo = 0;

        UpdateUI();
    }

    public void StartSong()
    {
        if (theMusic != null)
        {
            songStartTime = AudioSettings.dspTime;
            theMusic.Play();
        }
    }

    // Hit methods with improved feedback
    public void NormalHit()
    {
        ProcessHit(scorePerNote, "Normal");
        normalHits++;
        OnHitFeedback();
    }

    public void GoodHit()
    {
        ProcessHit(scorePerGoodNote, "Good");
        goodHits++;
        OnHitFeedback();
    }

    public void PerfectHit()
    {
        ProcessHit(scorePerPerfectNote, "Perfect");
        perfectHits++;
        OnHitFeedback();
    }

    public void NoteMissed()
    {
        Debug.Log("Note Missed!");
        missedNotes++;
        totalNotes++;

        // Reset combo and multiplier
        currentCombo = 0;
        currentMultiplier = 1;
        multiplierTracker = 0;

        // Trigger miss events
        OnNoteMissed?.Invoke();
        OnComboChanged?.Invoke(currentCombo);

        UpdateUI();
    }

    // Also add this method for compatibility
    public void NotMissed()
    {
        NoteMissed(); // Redirect to correct method
    }

    private void ProcessHit(int baseScore, string hitType)
    {
        notesHit++;
        totalNotes++;
        currentCombo++;

        // Update max combo
        if (currentCombo > maxCombo)
            maxCombo = currentCombo;

        // Handle multiplier progression
        UpdateMultiplier();

        // Calculate final score
        int finalScore = baseScore * currentMultiplier;
        currentScore += finalScore;

        // Trigger events
        OnScoreChanged?.Invoke(currentScore);
        OnComboChanged?.Invoke(currentCombo);

        Debug.Log($"{hitType} Hit! +{finalScore} points");
        UpdateUI();
    }

    private void UpdateMultiplier()
    {
        if (currentMultiplier < maxMultiplier && currentMultiplier - 1 < multiplierThresholds.Length)
        {
            multiplierTracker++;
            if (multiplierThresholds[currentMultiplier - 1] <= multiplierTracker)
            {
                multiplierTracker = 0;
                currentMultiplier++;
                Debug.Log($"Multiplier increased to x{currentMultiplier}!");
            }
        }
    }

    private void OnHitFeedback()
    {
        // Placeholder for hit effects (particles, screen shake, etc.)
        // You can expand this later
    }

    private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + currentScore.ToString("N0");

        if (multiText != null)
            multiText.text = "Multiplier: x" + currentMultiplier;

        if (comboText != null)
            comboText.text = "Combo: " + currentCombo;

        if (accuracyText != null)
            accuracyText.text = "Accuracy: " + Accuracy.ToString("F1") + "%";
    }

    // Utility methods for external scripts
    public void AddTotalNotes(int count)
    {
        totalNotes += count; // For when loading chart
    }

    public void PauseGame()
    {
        if (theMusic != null)
            theMusic.Pause();
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        if (theMusic != null)
            theMusic.UnPause();
        Time.timeScale = 1f;
    }

    // Get final results for results screen
    public GameResults GetResults()
    {
        return new GameResults
        {
            finalScore = currentScore,
            maxCombo = maxCombo,
            accuracy = Accuracy,
            perfectHits = perfectHits,
            goodHits = goodHits,
            normalHits = normalHits,
            missedNotes = missedNotes,
            totalNotes = totalNotes
        };
    }
}

// Data class for results
[System.Serializable]
public class GameResults
{
    public int finalScore;
    public int maxCombo;
    public float accuracy;
    public int perfectHits;
    public int goodHits;
    public int normalHits;
    public int missedNotes;
    public int totalNotes;
}