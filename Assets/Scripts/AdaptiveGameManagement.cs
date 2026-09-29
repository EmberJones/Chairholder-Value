using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AdaptiveGameManager : MonoBehaviour, IRoundManager
{
    private enum CurrentLevel
    {
        Lower,
        Middle,
        Upper
    }

    public static AdaptiveGameManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        } else
        {
            if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private List<CVObject> AllCVs = new List<CVObject>();
    public bool CanSubmitRound => AllCVs.Any(cv => cv.Decision == StampType.Approve);

    [Header("Round Components")]
    [SerializeField] private List<JobRole> AllJobRoles;
    [SerializeField] private GameObject CVPrefab;

    [Header("Scene Names")]
    [SerializeField] private string FailSceneName;
    [SerializeField] private string CreditsSceneName;

    private JobRole CurrentRole;
    private CurrentLevel ActiveLevel;
    private string Feedback = "";
    private List<GeneratedCV> CVBatch;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // initialise the relevant variables, as start won't be called when we move to another scene
        ActiveLevel = CurrentLevel.Middle;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == FailSceneName || scene.name == CreditsSceneName) return;

        BeginningOfRound();
    }

    void BeginningOfRound()
    {
        if (CVGenerator.Instance == null)
        {
            Debug.LogWarning("Generator Missing, Aborting");
            return;
        }

        if (MailManager.Instance == null)
        {
            Debug.LogWarning("Mail Manager Missing, Aborting");
            return;
        }

        if (AllJobRoles.Count == 0)
        {
            Debug.LogWarning("No Job Roles Provided, Aborting");
            return;
        }

        if (CVSpawnPoint.Instance == null)
        {
            Debug.LogWarning("CV Spawn Point Missing, Aborting");
            return;
        }

        CurrentRole = AllJobRoles[Random.Range(0, AllJobRoles.Count)];       // pick a random JobRole
        int CVCount;

        if (ActiveLevel == CurrentLevel.Lower || ActiveLevel == CurrentLevel.Middle)        // lower and middle levels get 3 CV's, upper level gets 4
        {
            CVCount = 3;
        } else
            CVCount = 4;

        AllCVs.Clear();
        CVBatch = CVGenerator.Instance.GenerateBatch(CurrentRole, CVCount);        // generate but not spawn the CV's
        CVScorer.ScoreAll(CVBatch, CurrentRole);

        if (Feedback != "")         // give the player feedback if there is any to give
        {
            MailMessage NewMail = new MailMessage("HR", "Hiring Feedback", Feedback);
            MailManager.Instance.ReceiveMail(NewMail);
        }

        MailMessage CurrentJobMail = new MailMessage("The Boss", "Todays' Open Position", "Today, we need you to find one suitable applicant for the newly opened role of " + CurrentRole.name + ". The role needs " + CurrentRole.RoleDescription);
        MailManager.Instance.ReceiveMail(CurrentJobMail);                       // send an email to the player with the details for the roole they are filling today

        MailManager.Instance.OnMailRead += SpawnCVs;        // subscribe to the email reading
        // once they read the email, we should spawn the CV's
    }

    private void SpawnCVs(int I)
    {
        foreach (GeneratedCV cv in CVBatch)
        {
            GameObject CV = Instantiate(CVPrefab, CVSpawnPoint.Instance.transform.position, CVSpawnPoint.Instance.transform.rotation);
            CVObject CVO = CV.GetComponent<CVObject>();
            CVO.SetData(cv);
            AllCVs.Add(CVO);
        }
        MailManager.Instance.OnMailRead -= SpawnCVs;
    }

    public void SubmitRound()
    {
        if (!CanSubmitRound) return;

        // the player has submitted their chosen CV
        var AllData = AllCVs.Where(cv => cv.Data != null).Select(cv => cv.Data).ToList();
        var ApprovedCV = AllCVs.FirstOrDefault(cv => cv.Decision == StampType.Approve);

        var Summary = new RoundResult();
        Summary = CVRanker.Evaluate(AllData, ApprovedCV.Data);        // we evaluate their decision

        foreach (CVObject CV in AllCVs)
        {
            CV.gameObject.SetActive(false);
        }

        switch (Summary.Rating)
        {
            case PickRating.BestChoice:                     // if it is correct, we bump them up the chain, and leave positive feedback
                Feedback = "Great Job Yesterday! The person you hired is performing well already!";
                LoadLevelUp();
                break;
            case PickRating.GoodChoice:                 // if it is somehwere in the middle, we just reload the current scene leaving the player with neutral feedback.
                Feedback = "This new hire shows a willingness to learn and adapt, they'll improve in no time!";
                ReloadLevel();
                break;
            case PickRating.MediocreChoice:
                Feedback = "Not the greatest choice yesterday. This new hire will need some serious training.";
                ReloadLevel();
                break;
            case PickRating.WorstChoice:                // if it is incorrect, we bump them down the chain, and leave constructive/negative feedback
                Feedback = "The person you hired yesterday, was a poor fit for the business, I hope you do better next time";
                LoadLevelDown();
                break;
        }
        
    }

    private void LoadLevelUp()
    {
        switch (ActiveLevel)
        {
            case CurrentLevel.Lower:
                ActiveLevel = CurrentLevel.Middle;
                MetaProgressManager.Instance.LoadMiddleScene();
                // load middle scene
                break;
            case CurrentLevel.Middle:
                ActiveLevel = CurrentLevel.Upper;
                MetaProgressManager.Instance.LoadBestScene();
                // load upper scene
                break;
            case CurrentLevel.Upper:
                // go to the credits
                break;
        }
    }

    private void ReloadLevel()
    {
        switch (ActiveLevel)
        {
            case CurrentLevel.Lower:
                MetaProgressManager.Instance.LoadWorstScene();
                // reload lower scene
                break;
            case CurrentLevel.Middle:
                MetaProgressManager.Instance.LoadMiddleScene();
                // reload middle scene
                break;
            case CurrentLevel.Upper:
                MetaProgressManager.Instance.LoadBestScene();
                // reload upper scene
                break;
        }
    }

    private void LoadLevelDown()
    {
        switch (ActiveLevel)
        {
            case CurrentLevel.Lower:
                // load the failure scene
                break;
            case CurrentLevel.Middle:
                ActiveLevel = CurrentLevel.Lower;
                MetaProgressManager.Instance.LoadWorstScene();
                // load the lower scene
                break;
            case CurrentLevel.Upper:
                ActiveLevel = CurrentLevel.Middle;
                MetaProgressManager.Instance.LoadMiddleScene();
                // load the middle scene
                break;
        }
    }
}
