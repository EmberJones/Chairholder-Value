using UnityEngine;
using TMPro;
using NUnit.Framework;
using System.Collections.Generic;

public class MailUI : MonoBehaviour
{
    [Header("List")]
    [SerializeField] private Transform listContent;      // ScrollView > Viewport > Content
    [SerializeField] private MailListItem rowPrefab;    // prefab with MailListItem

    [Header("Reading Panel")]
    [SerializeField] private GameObject detailPanel;
    [SerializeField] private TMP_Text detailSender;
    [SerializeField] private TMP_Text detailSubject;
    [SerializeField] private TMP_Text detailBody;

    private readonly List<MailListItem> rows = new List<MailListItem>();

    private void OnEnable()
    {
        MailManager.Instance.OnMailListChanged += Redraw;       // structure change, full redraw
        MailManager.Instance.OnMailRead += HandleMailRead;
        Redraw();
    }

    private void OnDisable()
    {
        if (MailManager.Instance != null)
        {
            MailManager.Instance.OnMailListChanged -= Redraw;
            MailManager.Instance.OnMailRead -= HandleMailRead;
        }
    }

    private void Redraw()
    {
        // Clear existing rows.
        for (int i = listContent.childCount - 1; i >= 0; i--)
            Destroy(listContent.GetChild(i).gameObject);
        rows.Clear();

        var messages = MailManager.Instance.Messages;
        for (int i = 0; i < messages.Count; i++)
        {
            var row = Instantiate(rowPrefab, listContent);
            row.Setup(messages[i], i, this);
            rows.Add(row);
        }

    }

    private void HandleMailRead(int Index)
    {
        if (Index < 0 || Index >= rows.Count) return;
        rows[Index].SetReadIndicator(true);
    }


    public void SelectMail(int Index)
    {
        var Mail = MailManager.Instance.Messages[Index];

        detailSender.text = Mail.Sender;
        detailSubject.text = Mail.Subject;
        detailBody.text = Mail.Body;
        detailPanel.SetActive(true);

        MailManager.Instance.MarkAsRead(Index);
    }
}
