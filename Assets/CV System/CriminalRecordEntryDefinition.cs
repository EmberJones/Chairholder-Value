using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CriminalRecordEntry", menuName = "CV System/Criminal Record Entry Definition")]
public class CriminalRecordEntryDefinition : ScriptableObject
{
    [Header("Display")]
    [Tooltip("Text shown in the background-check result, e.g. 'Convicted of Petty Theft - Cook County, 2018'.")]
    [TextArea]
    public string DisplayText;

    [Header("Relevance Tagging")]
    [Tooltip("Which offense categories this record relates to. Most offenses will only have one tag, but some can span multiple (e.g. an armed robbery might tag both Theft and ViolentOffense).")]
    public List<CrimeTag> Tags = new List<CrimeTag>();

    [Header("Intrinsic Severity")]
    [Range(0f, 1f)]
    [Tooltip("How severe this offense is on its own merits, independent of role relevance. 0 = very minor (e.g. an old misdemeanor, fully resolved), 1 = extremely severe " +
            "(e.g. a serious felony conviction). This is combined with the role's relevance weight for the offense's tags to determine actual impact on hireability for a given role.")]
    public float SeverityScore = 0.5f;

    [Tooltip("How many years ago this occurred. Display/flavor only - not currently used in scoring")]
    public int YearsAgo = 1;
}
