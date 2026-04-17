using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    // Singleton

    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // State

    public enum GameState { Idle, CVOpen, Stamped, Submitted }

    [Header("State (read-only in inspector)")]
    [SerializeField] private GameState _currentState = GameState.Idle;
    public GameState CurrentState => _currentState;

    // CV queue

    [Header("CV Queue")]
    [SerializeField] private List<CVObject> cvQueue = new();   // Drag CV GameObjects in here in the inspector
    private int _currentCVIndex = 0;

    public CVObject CurrentCV => (_currentCVIndex < cvQueue.Count)
        ? cvQueue[_currentCVIndex]
        : null;

    // Scoring

    [Header("Scoring")]
    [SerializeField] private int _score = 0;
    [SerializeField] private int _correctDecisions = 0;
    [SerializeField] private int _incorrectDecisions = 0;

    public int Score => _score;
    public int CorrectDecisions => _correctDecisions;
    public int IncorrectDecisions => _incorrectDecisions;

    // Events — wire these up in the inspector or subscribe in code

    [Header("Events")]
    public UnityEvent<CVObject> onCVStamped;
    public UnityEvent<CVObject, bool> onCVSubmitted;  // bool = was decision correct
    public UnityEvent onAllCVsComplete;

    // Fax machine

    [Header("References")]
    [SerializeField] private FaxMachine faxMachine;
    [SerializeField] private PlayerController playerController;


    void Start()
    {
        ActivateCurrentCV();
    }

    // Called by CVObject when a stamp is applied

    public void OnCVStamped(CVObject cv)
    {
        if (cv != CurrentCV) return;
        SetState(GameState.Stamped);
        onCVStamped?.Invoke(cv);

        faxMachine?.SetReady(true);
    }

    // Called by FaxMachine when the player clicks it with a stamped CV

    public void SubmitCurrentCV()
    {
        CVObject cv = CurrentCV;
        if (cv == null || !cv.IsStamped) return;

        SetState(GameState.Submitted);

        // Score the decision
        bool correct = EvaluateDecision(cv);
        if (correct) _correctDecisions++;
        else _incorrectDecisions++;

        _score += correct ? 100 : -50;

        onCVSubmitted?.Invoke(cv, correct);

        // Deactivate the submitted CV and move to the next
        cv.gameObject.SetActive(false);
        _currentCVIndex++;

        if (_currentCVIndex >= cvQueue.Count)
        {
            onAllCVsComplete?.Invoke();
            Debug.Log($"Game complete. Score: {_score} | Correct: {_correctDecisions} | Wrong: {_incorrectDecisions}");
            return;
        }

        faxMachine?.SetReady(false);
        ActivateCurrentCV();
        SetState(GameState.Idle);
    }


    void ActivateCurrentCV()
    {
        if (CurrentCV == null) return;
        CurrentCV.gameObject.SetActive(true);
        SetState(GameState.Idle);
    }

    bool EvaluateDecision(CVObject cv)
    {

        if (cv.Data == null) return true;
        return cv.Data.meta.correctDecision == cv.Decision.ToString().ToLower();
    }

    void SetState(GameState newState)
    {
        _currentState = newState;
        Debug.Log($"[GameManager] State -> {newState}");
    }



    public bool CanSubmit => CurrentCV != null && CurrentCV.IsStamped;
}