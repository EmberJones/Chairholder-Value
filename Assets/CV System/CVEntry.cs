using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCVEntry", menuName = "CV System/CV Entry")]
public class CVEntry : ScriptableObject
{
    [Header("Display")]
    [Tooltip("Text shown on the generated CV, e.g. 'Retail Sales Associate - Northgate Mall (2019-2021)'.")]
    [TextArea]
    public string DisplayText;

    public EntryCategory Category;

    [Header("Relevant Skills")]
    [Tooltip("Which domains/competencies this entry relates to. An entry can have multiple tags " +
             "(e.g. a 'Help Desk Technician' role might tag both IT and CustomerService).")]
    public List<SkillTag> Tags = new List<SkillTag>();

    [Header("Intrinsic Quality")]
    [Range(0f, 1f)]
    [Tooltip("How impressive this entry is on its own merits, independent of role relevance. " +
             "0 = weak/junior (e.g. 3-month unpaid internship), 1 = outstanding (e.g. 10 years, senior title, notable employer). " +
             "For Experience entries this typically scales with years/seniority; for Education, with level " +
             "(e.g. certificate=0.3, bachelor's=0.6, master's=0.8, PhD=1.0); for Skills/Certifications, with how " +
             "in-demand/advanced the skill is.")]
    public float QualityScore = 0.5f;

    [Tooltip("Approximate years of experience this entry represents, if applicable. Used for generator hints and UI display only (not scoring).")]
    public int YearsOfExperience = 0;
}
