using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MailListItem : MonoBehaviour
{
    [SerializeField] private TMP_Text SenderText;
    [SerializeField] private TMP_Text SubjectText;
    [SerializeField] private GameObject ReadIndicator;
    [SerializeField] private Button button;

    private int MyIndex;
    MailUI Owner;

    public void Setup(MailMessage mail, int Index, MailUI owner)
    {
        this.MyIndex = Index;
        this.Owner = owner;

        SenderText.text = mail.Sender;
        SubjectText.text = mail.Subject;
        ReadIndicator.SetActive(mail.isRead);

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => owner.SelectMail(MyIndex));
        button.onClick.AddListener(() => {
            Debug.Log($"[{gameObject.name}] Click fired, MyIndex={MyIndex}, Frame={Time.frameCount}");
            owner.SelectMail(MyIndex);
        });
    }

    public void SetReadIndicator(bool isRead)
    {
        ReadIndicator.SetActive(isRead);
    }
}
