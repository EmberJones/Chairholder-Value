using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MailManager : MonoBehaviour
{
    public static MailManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        } else
        {
            Destroy(gameObject);
        }
    }

    private readonly List<MailMessage> messages = new List<MailMessage>();
    public IReadOnlyList<MailMessage> Messages => messages;

    public event Action<MailMessage> OnMailReceived;
    public event Action OnMailListChanged;
    public event Action<int> OnMailRead;

    public void ReceiveMail(MailMessage Mail)       // call this to give mail to the player
    {
        messages.Insert(0, Mail); // newest first

        while (messages.Count > 8)
        {
            messages.Remove(messages.Last());
        }

        OnMailReceived?.Invoke(Mail);
        OnMailListChanged?.Invoke();
    }

    public void MarkAsRead(int Index)
    {
        if (Index < 0 || Index >= messages.Count) return;
        var mail = messages[Index];
        if (mail.isRead) return;        // already read

        mail.isRead = true;
        messages[Index] = mail;
        OnMailRead?.Invoke(Index);
    }

    public void DeleteMail(int Index)
    {
        if (Index < 0 || Index >= messages.Count) return;
        messages.RemoveAt(Index);
        OnMailListChanged?.Invoke();
    }

    public bool HasUnread()
    {
        foreach (var m in messages)
            if (!m.isRead) return true;
        return false;
    }
}
