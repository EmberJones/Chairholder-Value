using UnityEngine;

public class MailTest : MonoBehaviour
{
    public void SendMail()
    {
        MailMessage message = new MailMessage("1", "1", "1");
        MailManager.Instance.ReceiveMail(message);
        message = new MailMessage("2", "2", "2");
        MailManager.Instance.ReceiveMail(message);
        message = new MailMessage("3", "3", "3");
        MailManager.Instance.ReceiveMail(message);
        message = new MailMessage("4", "4", "4");
        MailManager.Instance.ReceiveMail(message);
        message = new MailMessage("5", "5", "5");
        MailManager.Instance.ReceiveMail(message);
        message = new MailMessage("6", "6", "6");
        MailManager.Instance.ReceiveMail(message);
        message = new MailMessage("7", "7", "7");
        MailManager.Instance.ReceiveMail(message);
        message = new MailMessage("8", "8", "8");
        MailManager.Instance.ReceiveMail(message);
        message = new MailMessage("9", "9", "9");
        MailManager.Instance.ReceiveMail(message);
    }
}
