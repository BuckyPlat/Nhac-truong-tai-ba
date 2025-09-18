using UnityEngine;
using System.Collections.Generic;

// Enums for different note types
[System.Serializable]
public enum NoteType
{
    Normal = 0,
    Hold = 1,
    Special = 2
}

// Enhanced NoteData structure
[System.Serializable]
public class NoteData
{
    public float beat;
    public int lane;
    public NoteType noteType = NoteType.Normal;
    public float duration = 0f; // For hold notes
    public int specialValue = 0; // For special notes

    // Constructor for backward compatibility
    public NoteData()
    {
        noteType = NoteType.Normal;
    }

    public NoteData(float beat, int lane, NoteType noteType = NoteType.Normal)
    {
        this.beat = beat;
        this.lane = lane;
        this.noteType = noteType;
    }
}

// Enhanced Chart structure
[System.Serializable]
public class Chart
{
    public string songName = "";
    public float bpm = 120f;
    public List<NoteData> notes;
    public float offset = 0f;
    public string difficulty = "Normal";

    public Chart()
    {
        notes = new List<NoteData>();
    }
}

// Additional data structures for rhythm games
[System.Serializable]
public class SongInfo
{
    public string title;
    public string artist;
    public string audioFileName;
    public Sprite coverArt;
    public List<Chart> difficulties;

    public SongInfo()
    {
        difficulties = new List<Chart>();
    }
}

// Hit result data
[System.Serializable]
public enum HitResult
{
    Miss = 0,
    Normal = 1,
    Good = 2,
    Perfect = 3
}

// Timing window settings
[System.Serializable]
public class TimingWindows
{
    public float perfectWindow = 0.05f;  // ±50ms
    public float goodWindow = 0.1f;      // ±100ms
    public float normalWindow = 0.15f;   // ±150ms
    // Anything outside normal window = Miss
}

// Game settings
[System.Serializable]
public class GameSettings
{
    public float noteSpeed = 1f;
    public float audioLatency = 0.1f;
    public TimingWindows timingWindows;
    public bool autoPlay = false;

    public GameSettings()
    {
        timingWindows = new TimingWindows();
    }
}