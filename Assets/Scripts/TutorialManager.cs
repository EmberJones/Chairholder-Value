using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class TutorialManager : MonoBehaviour, IRoundManager
{
    public static TutorialManager Instance { get; private set; }
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public enum GameState { InProgress, RoundComplete }
    [SerializeField] private GameState _currentState = GameState.InProgress;
    public GameState CurrentState => _currentState;

    [Header("Round Config")]
    public JobRole currentRole;                              

    [Header("Preset CVs")]
    [Tooltip("Hand-authored CVs used instead of CVGenerator - order doesn't matter, spawn points below control layout.")]
    [SerializeField] private List<PresetCV> presetBatch = new List<PresetCV>();

    [Header("Spawning")]
    [SerializeField] private GameObject CVPrefab;
    [SerializeField] private Transform[] CVSpawnPoints;      

    [Header("CV Batch")]
    [SerializeField] private List<CVObject> allCVs = new();

    [Header("Events - hook the tutorial dialogue/popup system to these")]
    public UnityEvent<CVObject> onCVStamped;
    public UnityEvent<RoundSummary> onRoundComplete;

    public bool CanSubmitRound => allCVs.Any(cv => cv.Decision == StampType.Approve);

    void Start()
    {
        SpawnPresetBatch();
        ScoreBatch();
    }

    void SpawnPresetBatch()
    {
        allCVs.Clear();

        if (presetBatch == null || presetBatch.Count == 0)
        {
            Debug.LogWarning("[TutorialManager] No preset CVs assigned.");
            return;
        }

        if (CVSpawnPoints == null || CVSpawnPoints.Length < presetBatch.Count)
        {
            Debug.LogWarning("[TutorialManager] Fewer spawn points than preset CVs - some candidates won't be placed.");
        }

        for (int i = 0; i < presetBatch.Count && i < CVSpawnPoints.Length; i++)
        {
            GeneratedCV data = presetBatch[i].ToGeneratedCV();

            GameObject CV = Instantiate(CVPrefab, CVSpawnPoints[i].position, CVSpawnPoints[i].rotation);
            CVObject CVO = CV.GetComponent<CVObject>();
            CVO.SetData(data);
            allCVs.Add(CVO);
        }
    }

    void ScoreBatch()
    {
        if (currentRole == null) { Debug.LogWarning("[TutorialManager] No JobRole assigned."); return; }
        foreach (var cv in allCVs)
            if (cv.Data != null) CVScorer.Score(cv.Data, currentRole);
    }

    public void OnCVStamped(CVObject cv) => onCVStamped?.Invoke(cv);

    public void SubmitRound()
    {
        if (_currentState == GameState.RoundComplete) return;
        _currentState = GameState.RoundComplete;

        var allData = allCVs.Where(cv => cv.Data != null).Select(cv => cv.Data).ToList();
        var approvedCV = allCVs.FirstOrDefault(cv => cv.Decision == StampType.Approve);

        var summary = new RoundSummary();
        if (allData.Count > 0 && approvedCV?.Data != null)
        {
            summary.Result = CVRanker.Evaluate(allData, approvedCV.Data);
            summary.HasApproval = true;
        }

        var bestCV = CVRanker.Rank(allData).FirstOrDefault();

        foreach (var cv in allCVs)
        {
            if (cv.Data == null) continue;

            bool shouldApprove = cv.Data == bestCV;
            bool playerApproved = cv.Decision == StampType.Approve;
            bool correct = shouldApprove == playerApproved;

            summary.Entries.Add(new RoundSummary.Entry { CV = cv, WasCorrect = correct });
            if (correct) summary.CorrectCount++; else summary.IncorrectCount++;

            cv.gameObject.SetActive(false);
        }

        Debug.Log(summary.HasApproval
            ? $"[TutorialManager] Tutorial complete. Approved '{summary.Result.PickedCV.CVName}' - {summary.Result.Rating}"
            : "[TutorialManager] Tutorial complete. No CV was approved.");

        bool successfulHire = summary.HasApproval && summary.Result.Rating == PickRating.BestChoice;
        MetaProgressManager.Instance?.ReportHireResult(currentRole, successfulHire);

        onRoundComplete?.Invoke(summary);
        // Tutorial dialogue/popup system should subscribe to onRoundComplete, show its feedback,
        // then call TutorialManager.Instance.FinishTutorial() once the player dismisses it.
    }

    // Call this once the tutorial's end-of-round feedback has been shown and dismissed
    public void FinishTutorial()
    {
        MetaProgressManager.Instance?.CompleteTutorial();
    }
}