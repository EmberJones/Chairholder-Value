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

    public enum GameState { Idle, CVOpen, Stamped, Submitted, RoundComplete }
    [Header("State (read-only in inspector)")]
    [SerializeField] private GameState _currentState = GameState.Idle;
    public GameState CurrentState => _currentState;

    [Header("Round Config")]
    [SerializeField] private JobRole currentRole;

    [Header("CV Queue")]
    [SerializeField] private List<CVObject> cvQueue = new();
    private int _currentCVIndex = 0;
    public CVObject CurrentCV => (_currentCVIndex < cvQueue.Count)
        ? cvQueue[_currentCVIndex]
        : null;

    private readonly List<CVObject> _submittedCVs = new();

    [Header("Events")]
    public UnityEvent<CVObject> onCVStamped;
    public UnityEvent<CVObject> onCVSubmitted;
    public UnityEvent<RoundSummary> onRoundComplete;

    [Header("References")]
    [SerializeField] private FaxMachine faxMachine;
    [SerializeField] private PlayerController playerController;

    void Start()
    {
        ScoreQueue();
        ActivateCurrentCV();
    }

    void ScoreQueue()
    {
        if (currentRole == null)
        {
            Debug.LogWarning("[GameManager] No JobRole assigned - cannot score CVs.");
            return;
        }

        foreach (var cvObj in cvQueue)
        {
            if (cvObj.Data != null)
                CVScorer.Score(cvObj.Data, currentRole);
        }
    }

    public void OnCVStamped(CVObject cv)
    {
        if (cv != CurrentCV) return;
        SetState(GameState.Stamped);
        onCVStamped?.Invoke(cv);
        faxMachine?.SetReady(true);
    }

    public void SubmitCurrentCV()
    {
        CVObject cv = CurrentCV;
        if (cv == null || !cv.IsStamped) return;

        SetState(GameState.Submitted);

        _submittedCVs.Add(cv);
        onCVSubmitted?.Invoke(cv);

        RemoveFromDesk(cv); 

        _currentCVIndex++;

        if (_currentCVIndex >= cvQueue.Count)
        {
            CompleteRound();
            return;
        }

        faxMachine?.SetReady(false);
        ActivateCurrentCV();
        SetState(GameState.Idle);
    }

    void RemoveFromDesk(CVObject cv)
    {
        cv.gameObject.SetActive(false);
    }

    void ActivateCurrentCV()
    {
        if (CurrentCV == null) return;
        CurrentCV.gameObject.SetActive(true);
        SetState(GameState.Idle);
    }

    void CompleteRound()
    {
        SetState(GameState.RoundComplete);

        var allData = _submittedCVs
            .Where(cv => cv.Data != null)
            .Select(cv => cv.Data)
            .ToList();

        var approvedCV = _submittedCVs.FirstOrDefault(cv => cv.Decision == StampType.Approve);

        var summary = new RoundSummary();

        if (allData.Count > 0 && approvedCV != null && approvedCV.Data != null)
        {
            summary.Result = CVRanker.Evaluate(allData, approvedCV.Data);
            summary.HasApproval = true;
        }
        else
        {
            // Nobody was approved this round - no RoundResult to give, boss dialogue should handle this case separately
            summary.HasApproval = false;
        }

        var ranked = CVRanker.Rank(allData);
        var bestCV = ranked.FirstOrDefault();

        foreach (var cv in _submittedCVs)
        {
            if (cv.Data == null) continue;

            bool shouldApprove = cv.Data == bestCV;
            bool playerApproved = cv.Decision == StampType.Approve;
            bool correct = shouldApprove == playerApproved;

            summary.Entries.Add(new RoundSummary.Entry
            {
                CV = cv,
                WasCorrect = correct
            });

            if (correct) summary.CorrectCount++;
            else summary.IncorrectCount++;
        }

        Debug.Log(summary.HasApproval
            ? $"[GameManager] Round complete. Approved '{summary.Result.PickedCV.CVName}' - {summary.Result.Rating} (rank {summary.Result.PickedRank}/{summary.Result.TotalCandidates})"
            : "[GameManager] Round complete. No CV was approved.");

        onRoundComplete?.Invoke(summary);
        // TODO: hand `summary` to  boss-dialogue system here
    }

    void SetState(GameState newState)
    {
        _currentState = newState;
        Debug.Log($"[GameManager] State -> {newState}");
    }

    public bool CanSubmit => CurrentCV != null && CurrentCV.IsStamped;
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
    public struct Entry
    {
        public CVObject CV;
        public bool WasCorrect;
    }
}