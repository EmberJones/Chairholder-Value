using System.Text;
using TMPro;
using UnityEngine;

/// Call Populate(GeneratedCV) to fill all text fields from a generated CV.
public class CVDisplay : MonoBehaviour
{
    [Header("Header")]
    [SerializeField] private TMP_Text nameText;

    [Header("Experience")]
    [SerializeField] private TMP_Text experienceText;

    [Header("Education")]
    [SerializeField] private TMP_Text educationText;

    [Header("Skills")]
    [SerializeField] private TMP_Text skillsText;

    [Header("Certifications")]
    [SerializeField] private TMP_Text certificationsText;

    public void Populate(GeneratedCV data)
    {
        if (data == null) return;

        SetText(nameText, data.CVName ?? "Unknown");
        SetText(experienceText, BuildCategory(data, EntryCategory.Experience, "No experience listed."));
        SetText(educationText, BuildCategory(data, EntryCategory.Education, "No education listed."));
        SetText(skillsText, BuildCategory(data, EntryCategory.Skill, "None listed."));
        SetText(certificationsText, BuildCategory(data, EntryCategory.Certification, "None listed."));
    }

    string BuildCategory(GeneratedCV data, EntryCategory category, string emptyMessage)
    {
        var sb = new StringBuilder();
        bool any = false;

        foreach (var entry in data.EntriesInCategory(category))
        {
            sb.AppendLine(entry.DisplayText);
            any = true;
        }

        return any ? sb.ToString().TrimEnd() : emptyMessage;
    }

    void SetText(TMP_Text field, string value)
    {
        if (field != null) field.text = value;
    }
}