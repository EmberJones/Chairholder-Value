using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

public class CVGenerator : MonoBehaviour
{
    public static CVGenerator Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        } else
        {
            Destroy(gameObject);
        }
    }

    [Header("Shared Pools (populate once, reused across all ~20 roles)")]
    [Tooltip("The full pool of CV building blocks available across all roles/industries.")]
    public List<CVEntry> EntryPool = new List<CVEntry>();

    [Tooltip("The pool of presentation styles a generated CV can have.")]
    public List<CVFormat> FormatProfiles = new List<CVFormat>();

    [Tooltip("Optional flavor names for candidates. If empty, candidates are labeled 'Candidate A/B/C...'.")]
    public List<string> CandidateNamePool = new List<string>();

    [Tooltip("The pool of possible offenses a candidate might have on their record. Shared across all roles - " +
             "impact varies per role via JobRole.CriminalTagWeights, same pattern as EntryPool.")]
    public List<CriminalRecordEntryDefinition> CriminalRecordPool = new List<CriminalRecordEntryDefinition>();

    [Header("Generation Defaults")]
    [Range(2, 8)]
    public int DefaultBatchSize = 4;
    [Range(3, 7)]
    public int DefaultEntriesPerCV = 6;

    [Range(0f, 1f)]
    [Tooltip("Chance (0-1) that any given candidate has a criminal record at all. Most candidates should be " +
             "clean, so keep this low (e.g. 0.2-0.3 = 20-30% of candidates have 1+ offenses).")]
    public float CriminalRecordChance = 0.25f;

    [Range(1, 3)]
    [Tooltip("Max number of offenses a candidate with a record can have (actual count is randomized between 1 and this).")]
    public int MaxOffensesPerRecord = 2;

    //The relevance threshold above which an entry is considered "relevant" to a role for generation-bucketing purposes.
    private const float RelevanceThreshold = 0.15f;

    public List<GeneratedCV> GenerateBatch(JobRole role, int? batchSize = null, int? entriesPerCV = null)
    {
        return GenerateBatch(role, EntryPool, FormatProfiles, CandidateNamePool, CriminalRecordPool,
            batchSize ?? DefaultBatchSize, entriesPerCV ?? DefaultEntriesPerCV, CriminalRecordChance, MaxOffensesPerRecord);
    }

    public static List<GeneratedCV> GenerateBatch(JobRole Role, List<CVEntry> EntryPool, List<CVFormat> FormatPool, List<string> CandidateNamePool, List<CriminalRecordEntryDefinition> CriminalRecordPool, int BatchSize, int EntriesPerCV, float CriminalRecordChance, int MaxOffensesPerRecord)
    {
        var batch = new List<GeneratedCV>();

        if (EntryPool == null || EntryPool.Count == 0)
        {
            Debug.Log("Entry Pool is empty, cannot populate CV's, returning");
            return batch;
        }

        var RelevantPool = EntryPool.Where(e => CVScorer.ComputeTagRelevance(e, Role) >= RelevanceThreshold).ToList();
        var FillerPool = EntryPool.Where(e => CVScorer.ComputeTagRelevance(e, Role) < RelevanceThreshold).ToList();

        if (RelevantPool.Count == 0) RelevantPool = EntryPool;      // fallback if relevance Threshold is too high and our relevant pool of skills is 0
        if (FillerPool.Count == 0) FillerPool = EntryPool;

        for (int i = 0; i < BatchSize; i++)
        {
            float tier = BatchSize > 1 ? (float)i / (BatchSize - 1) : 1f;
            var CV = GenerateSingleCV(Role, RelevantPool, FillerPool, FormatPool, CriminalRecordPool, EntriesPerCV, tier, CriminalRecordChance, MaxOffensesPerRecord);
            CV.CVName = PickCandidateName(CandidateNamePool, i);
            batch.Add(CV);
        }

        Shuffle(batch);     // shuffling so that they aren't returned to the player in preference order already

        return batch;
    }

            // relevance is the rating between 0 and 1 where 1 is perfectly suited for the Role, and 0 is unsuited for the role
    private static GeneratedCV GenerateSingleCV(JobRole Role, List<CVEntry> RelevantPool, List <CVEntry> FillerPool, List<CVFormat> FormatPool, List<CriminalRecordEntryDefinition> CriminalRecordPool, int EntryCountPerCV, float Relevance, float CriminalRecordChance, int MaxOffensesPerRecord)
    {
        var CV = new GeneratedCV();

        int RelevantCount = Mathf.RoundToInt(EntryCountPerCV * Relevance);
        int FillerCount = EntryCountPerCV - RelevantCount;

        var ChosenRelevant = TakeRandomDistinct(RelevantPool, RelevantCount);
        var ChosenFiller = TakeRandomDistinct(FillerPool, FillerCount);

        int ShortFall = EntryCountPerCV - (ChosenRelevant.Count + ChosenFiller.Count);      // one of the buckets emptied up

        if (ShortFall > 0)
        {
            var BackUpPool = RelevantPool.Except(ChosenRelevant).Concat(FillerPool.Except(ChosenFiller)).ToList();
            ChosenRelevant.AddRange(TakeRandomDistinct(BackUpPool, ShortFall));
        }

        CV.Entries.AddRange(ChosenRelevant);
        CV.Entries.AddRange(ChosenFiller);

        CV.FormatProfile = PickFormatForTier(FormatPool, Relevance);

        CV.CriminalRecordEntries = GenerateCriminalRecord(CriminalRecordPool, CriminalRecordChance, MaxOffensesPerRecord);

        CV.BackgroundCheckCode = BackgroundCheckCodeGenerator.GenerateUniqueCode();

        BackgroundCheckRegistry.Register(CV.BackgroundCheckCode, new BackgroundCheckResult
        {
            CandidateName = CV.CVName,
            HasRecord = CV.CriminalRecordEntries.Count > 0,
            Offenses = CV.CriminalRecordEntries
        });

        return CV;
    }

    private static List<CVEntry> TakeRandomDistinct(List<CVEntry> pool, int count)
    {
        if (count <= 0 || pool == null || pool.Count == 0)
            return new List<CVEntry>();

        return pool.OrderBy(_ => Random.value).Take(Mathf.Min(count, pool.Count)).ToList();
    }

    private static CVFormat PickFormatForTier(List<CVFormat> FormatPool, float Tier)
    {
        if (FormatPool == null || FormatPool.Count == 0) return null;

        var Sorted = FormatPool.OrderBy(f => f.FormatScore).ToList();

        if (Random.value < 0.75f)       // 75% of hte time, the appropriate format is chosen, 25% of th time a random one is chosen, to create that good content + bad formatting and vice versa
        {
            int TargetIndex = Mathf.Clamp(Mathf.RoundToInt(Tier * (Sorted.Count - 1)), 0, Sorted.Count - 1);
            int Jitter = Random.Range(-1, 2);
            int Index = Mathf.Clamp(TargetIndex + Jitter, 0, Sorted.Count - 1);
            return Sorted[Index];
        }
        return Sorted[Random.Range(0, Sorted.Count)];
    }

    private static string PickCandidateName(List<string> names, int index)      // if there are names use those, if there aren't, just go with Candidate A, B, C...
    {
        if (names != null && names.Count > 0) return names[Random.Range(0, names.Count)];
        return $"Candidate {(char)('A' + index)}";
    }

    private static void Shuffle(List<GeneratedCV> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private static List<CriminalRecordEntryDefinition> GenerateCriminalRecord(List<CriminalRecordEntryDefinition> CriminalRecordPool, float CriminalRecordChance, int MaxOffensesPerRecord)
    {
        var Record = new List<CriminalRecordEntryDefinition>();

        if (CriminalRecordPool == null || CriminalRecordPool.Count == 0) return Record;
        if (Random.value >= CriminalRecordChance) return Record; // clean criminal record

        int OffenseCount = Random.Range(1, MaxOffensesPerRecord + 1);
        Record.AddRange(TakeRandomDistinct(CriminalRecordPool, OffenseCount));
        return Record;
    }

    private static List<T> TakeRandomDistinct<T>(List<T> Pool, int Count)
    {
        if (Count <= 0 || Pool == null || Pool.Count == 0) return new List<T>();
        return Pool.OrderBy(_ => Random.value).Take(Mathf.Min(Count, Pool.Count)).ToList();
    }
}
