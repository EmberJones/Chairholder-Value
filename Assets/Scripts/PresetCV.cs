using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPresetCV", menuName = "CV System/Preset CV (Tutorial)")]
public class PresetCV : ScriptableObject
{
    [Header("Identity")]
    public string CVName;

    [Header("Content")]
    public List<CVEntry> Entries = new List<CVEntry>();
    public CVFormat FormatProfile;

    [Header("Background Check (Day 2)")]
    [Tooltip("Hidden record, only discoverable via the computer. Leave empty for a clean candidate.")]
    public List<CriminalRecordEntryDefinition> CriminalRecord = new List<CriminalRecordEntryDefinition>();

    public GeneratedCV ToGeneratedCV()
    {
        var cv = new GeneratedCV { CVName = CVName, FormatProfile = FormatProfile };
        cv.Entries.AddRange(Entries);

        cv.CriminalRecordEntries = new List<CriminalRecordEntryDefinition>(CriminalRecord);
        cv.BackgroundCheckCode = BackgroundCheckCodeGenerator.GenerateUniqueCode();

        BackgroundCheckRegistry.Register(cv.BackgroundCheckCode, new BackgroundCheckResult
        {
            CandidateName = cv.CVName,
            HasRecord = cv.CriminalRecordEntries.Count > 0,
            Offenses = cv.CriminalRecordEntries
        });

        return cv;
    }
}