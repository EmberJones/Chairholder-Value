using System;

[Serializable]
public struct MailMessage
{
    public string Sender;   // who it should say the mail is from
    public string Subject;  // the title of the message
    public string Body;     // main content of the mail
    public DateTime Timestamp;
    public bool isRead;

    public MailMessage(string sender, string subject, string body)
    {
        Sender = sender;
        Subject = subject;
        Body = body;
        Timestamp = DateTime.Now;
        this.isRead = false;
    }
}
