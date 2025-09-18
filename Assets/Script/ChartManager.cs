using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class ChartManager : MonoBehaviour
{
    [Header("Chart Loading")]
    public string chartsFolder = "Charts";
    public List<Chart> availableCharts = new List<Chart>();

    [Header("References")]
    public NoteSpawner noteSpawner;
    public ChartRecorder chartRecorder;
    public GameManager gameManager;

    [Header("Current Chart")]
    public Chart currentChart;
    public int selectedChartIndex = 0;

    void Start()
    {
        LoadAllCharts();
    }

    public void LoadAllCharts()
    {
        availableCharts.Clear();

        string folderPath = Path.Combine(Application.streamingAssetsPath, chartsFolder);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            Debug.Log($"Created charts folder: {folderPath}");
            return;
        }

        string[] chartFiles = Directory.GetFiles(folderPath, "*.json");

        foreach (string filePath in chartFiles)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                Chart chart = JsonUtility.FromJson<Chart>(json);

                if (chart != null && chart.notes != null)
                {
                    chart.songName = Path.GetFileNameWithoutExtension(filePath);
                    availableCharts.Add(chart);
                    Debug.Log($"Loaded chart: {chart.songName} ({chart.notes.Count} notes)");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load chart {filePath}: {e.Message}");
            }
        }

        Debug.Log($"Total charts loaded: {availableCharts.Count}");
    }

    public void SelectChart(int index)
    {
        if (index >= 0 && index < availableCharts.Count)
        {
            selectedChartIndex = index;
            currentChart = availableCharts[index];
            Debug.Log($"Selected chart: {currentChart.songName}");
        }
    }

    public void LoadSelectedChart()
    {
        if (currentChart == null || noteSpawner == null) return;

        noteSpawner.LoadChart(currentChart);
        Debug.Log($"Chart loaded into spawner: {currentChart.songName}");
    }

    public void StartSelectedChart()
    {
        LoadSelectedChart();
        if (noteSpawner != null)
        {
            noteSpawner.StartSong();
        }
    }

    public void SaveRecordedChart(string fileName)
    {
        if (chartRecorder == null) return;

        var recordedNotes = chartRecorder.GetRecordedNotes();
        if (recordedNotes.Count == 0)
        {
            Debug.Log("No notes to save!");
            return;
        }

        Chart newChart = new Chart
        {
            songName = fileName,
            bpm = chartRecorder.bpm,
            offset = chartRecorder.offset,
            notes = recordedNotes,
            difficulty = "Custom"
        };

        // Save chart
        string json = JsonUtility.ToJson(newChart, true);
        string folderPath = Path.Combine(Application.streamingAssetsPath, chartsFolder);
        string filePath = Path.Combine(folderPath, fileName + ".json");

        File.WriteAllText(filePath, json);

        // Add to available charts
        availableCharts.Add(newChart);

        Debug.Log($"Chart saved and added to library: {fileName}");
    }

    public List<string> GetChartNames()
    {
        List<string> names = new List<string>();
        foreach (Chart chart in availableCharts)
        {
            names.Add($"{chart.songName} - {chart.difficulty} ({chart.notes.Count} notes)");
        }
        return names;
    }

    public Chart GetChartByName(string name)
    {
        return availableCharts.Find(c => c.songName == name);
    }

    // UI Helper methods
    public void NextChart()
    {
        selectedChartIndex = (selectedChartIndex + 1) % availableCharts.Count;
        SelectChart(selectedChartIndex);
    }

    public void PreviousChart()
    {
        selectedChartIndex = (selectedChartIndex - 1 + availableCharts.Count) % availableCharts.Count;
        SelectChart(selectedChartIndex);
    }
}