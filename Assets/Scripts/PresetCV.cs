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

    public GeneratedCV ToGeneratedCV()
    {
        var cv = new GeneratedCV
        {
            CVName = CVName,
            FormatProfile = FormatProfile
        };
        cv.Entries.AddRange(Entries);

        cv.CriminalRecordEntries = new List<CriminalRecordEntryDefinition>();
        cv.BackgroundCheckCode = BackgroundCheckCodeGenerator.GenerateUniqueCode();

        BackgroundCheckRegistry.Register(cv.BackgroundCheckCode, new BackgroundCheckResult
        {
            CandidateName = cv.CVName,
            HasRecord = false,
            Offenses = cv.CriminalRecordEntries
        });

        return cv;
    }
}