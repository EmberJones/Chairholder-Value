using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
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
    [SerializeField] private CVGenerator Gen;
    [SerializeField] private int CVCount;
    [SerializeField] private GameObject CVPrefab;
    [SerializeField] private Transform CVSpawnPoint;
    //[SerializeField] private CVSpawner spawner;

    [Header("CV Batch")]
    [SerializeField] private List<CVObject> allCVs = new();

    [Header("Events")]
    public UnityEvent<CVObject> onCVStamped;
    public UnityEvent<RoundSummary> onRoundComplete;

    public bool CanSubmitRound => allCVs.Any(cv => cv.Decision == StampType.Approve);

    void Start()
    {
        allCVs.Clear();
        var CVDataBatch = Gen.GenerateBatch(currentRole, CVCount, 5);

        foreach (GeneratedCV cv in CVDataBatch)
        {
            GameObject CV = Instantiate(CVPrefab, CVSpawnPoint.position, CVSpawnPoint.rotation);
            CVObject CVO = CV.GetComponent<CVObject>();
            CVO.SetData(cv);
            allCVs.Add(CVO);
        }

        // Spawn in CVs and assign CV objects
        ScoreBatch();

        foreach (GeneratedCV CV in CVDataBatch)
        {
            try
            {
                Debug.Log(CV.CVName + "\tCriminal History: " + CV.CriminalRecordEntries.First().name + "\t" + CV.BackgroundCheckCode);
                
                BackgroundCheckRegistry.TryLookup(CV.BackgroundCheckCode, out BackgroundCheckResult R);     // how to get the results from the background check
                Debug.Log("BCR: " + R.CandidateName + "\t" + R.HasRecord + "\t" + R.Offenses.Count);        // out putting the Candidate name, if they have a record (sanity check always true if they have committed any crimes) and the number of crimes they have committed
            }
            catch (InvalidOperationException)   // they don't have any elements in their Criminal Record Entries
            {
                Debug.Log(CV.CVName + "\tNo Criminal History\t" + CV.BackgroundCheckCode);
            }
        }
    }

    void ScoreBatch()
    {
        if (currentRole == null) { Debug.LogWarning("[GameManager] No JobRole assigned."); return; }
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
            ? $"[GameManager] Round complete. Approved '{summary.Result.PickedCV.CVName}' - {summary.Result.Rating}"
            : "[GameManager] Round complete. No CV was approved.");

        onRoundComplete?.Invoke(summary);
        // TODO: hand `summary` to your boss-dialogue system here
    }
}

[System.Serializable]
public class RoundSummary
{
    public bool HasApproval;
    public RoundResult Result;
    public int CorrectCount;
    public int IncorrectCount;
    public List<Entry> Entries = new List<Entry>();

    [System.Serializable]
    public struct Entry { public CVObject CV; public bool WasCorrect; }
}