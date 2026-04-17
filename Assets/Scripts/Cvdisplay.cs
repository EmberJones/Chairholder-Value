using System.Text;
using TMPro;
using UnityEngine;

/// Call Populate(CVData) to fill all text fields from a loaded CV.
public class CVDisplay : MonoBehaviour
{
    [Header("Header")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text referenceText;
    [SerializeField] private TMP_Text positionText;

    [Header("Personal")]
    [SerializeField] private TMP_Text personalText;     // Age, DOB, nationality, address

    [Header("Cover Note")]
    [SerializeField] private TMP_Text coverNoteText;

    [Header("Experience")]
    [SerializeField] private TMP_Text experienceText;

    [Header("Education")]
    [SerializeField] private TMP_Text educationText;

    [Header("Skills")]
    [SerializeField] private TMP_Text skillsText;

    [Header("References")]
    [SerializeField] private TMP_Text referencesText;


    public void Populate(CVData data)
    {
        if (data == null) return;
        Debug.Log("Populate data in Cvdisplay called");
        // Header
        SetText(nameText, data.personal?.fullName ?? "Unknown");
        SetText(referenceText, data.application?.referenceNumber ?? "");
        SetText(positionText, $"Applying for: {data.application?.position ?? ""}  |  " +
                               $"Dept: {data.application?.department ?? ""}  |  " +
                               $"Received: {data.application?.dateReceived ?? ""}");

        // Personal
        SetText(personalText, BuildPersonal(data));

        // Cover note
        SetText(coverNoteText, data.application?.coverNote ?? "");

        // Experience
        SetText(experienceText, BuildExperience(data));

        // Education
        SetText(educationText, BuildEducation(data));

        // Skills
        SetText(skillsText, BuildSkills(data));

        // References
        SetText(referencesText, BuildReferences(data));
    }


    string BuildPersonal(CVData d)
    {
        var sb = new StringBuilder();
        if (d.personal == null) return "";

        sb.AppendLine($"Age: {d.personal.age}");
        if (!string.IsNullOrEmpty(d.personal.dob))
            sb.AppendLine($"DOB: {d.personal.dob}");
        sb.AppendLine($"Nationality: {d.personal.nationality}");
        sb.AppendLine($"Address: {d.personal.address}");
        if (!string.IsNullOrEmpty(d.personal.phone))
            sb.AppendLine($"Phone: {d.personal.phone}");
        if (!string.IsNullOrEmpty(d.personal.email))
            sb.AppendLine($"Email: {d.personal.email}");
        return sb.ToString().TrimEnd();
    }

    string BuildExperience(CVData d)
    {
        if (d.experience == null || d.experience.Length == 0)
            return "No experience listed.";

        var sb = new StringBuilder();
        foreach (var e in d.experience)
        {
            sb.AppendLine($"{e.jobTitle}  —  {e.company}  ({e.startYear}–{e.endYear})");
            if (!string.IsNullOrEmpty(e.description))
                sb.AppendLine($"  {e.description}");
        }
        return sb.ToString().TrimEnd();
    }

    string BuildEducation(CVData d)
    {
        if (d.education == null || d.education.Length == 0)
            return "No education listed.";

        var sb = new StringBuilder();
        foreach (var e in d.education)
        {
            sb.Append($"{e.qualification}  —  {e.institution}  ({e.yearCompleted})");
            if (!string.IsNullOrEmpty(e.grade))
                sb.Append($"  [{e.grade}]");
            sb.AppendLine();
        }
        return sb.ToString().TrimEnd();
    }

    string BuildSkills(CVData d)
    {
        if (d.skills == null || d.skills.Length == 0)
            return "None listed.";

        var sb = new StringBuilder();
        foreach (var s in d.skills)
            sb.AppendLine($"• {s}");
        return sb.ToString().TrimEnd();
    }

    string BuildReferences(CVData d)
    {
        if (d.references == null || d.references.Length == 0)
            return "No references provided.";

        var sb = new StringBuilder();
        foreach (var r in d.references)
        {
            sb.Append($"{r.name},  {r.title}  —  {r.company}");
            sb.AppendLine(r.contactable ? "" : "  [NOT CONTACTABLE]");
        }
        return sb.ToString().TrimEnd();
    }


    void SetText(TMP_Text field, string value)
    {
        if (field != null) field.text = value;
    }
}