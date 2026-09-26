using UnityEngine;
using UnityEngine.SceneManagement;

public class MetaProgressManager : MonoBehaviour
{
    public static MetaProgressManager Instance { get; private set; }

    [Header("Win/Lose Thresholds")]
    [SerializeField] private int winScore = 5;
    [SerializeField] private int loseScore = -5;

    [Header("Scenes")]
    [SerializeField] private string tutorialSceneName = "TutorialScene";
    [SerializeField] private string mainGameplaySceneName = "DeskTestScene";
    [SerializeField] private string winSceneName = "WinScene";
    [SerializeField] private string loseSceneName = "LoseScene";

    [Header("Unlocks")]
    [SerializeField] private JobRole itGuyRole;   // the specific JobRole that represents hiring the IT guy
    public bool ITGuyHired { get; private set; }

    public int MetaScore { get; private set; }

    const string TutorialDoneKey = "ChairholderValue_TutorialDone";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartGame()
    {
        bool tutorialDone = PlayerPrefs.GetInt(TutorialDoneKey, 0) == 1;
        SceneManager.LoadScene(tutorialDone ? mainGameplaySceneName : tutorialSceneName);
    }

    public void CompleteTutorial()
    {
        PlayerPrefs.SetInt(TutorialDoneKey, 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene(mainGameplaySceneName);
    }

    // GameManager calls this as soon as a round resolves - just updates the score, no scene load yet
    public void ReportHireResult(JobRole roleThisRound, bool wasSuccessfulHire)
    {
        MetaScore += wasSuccessfulHire ? 1 : -1;

        if (wasSuccessfulHire && roleThisRound == itGuyRole)
            ITGuyHired = true;
    }

    // Whoever owns the round-end dialogue/popup calls this once the player has dismissed it
    public void ProceedAfterRound()
    {
        if (MetaScore >= winScore) { SceneManager.LoadScene(winSceneName); return; }
        if (MetaScore <= loseScore) { SceneManager.LoadScene(loseSceneName); return; }
        SceneManager.LoadScene(mainGameplaySceneName);
    }
}