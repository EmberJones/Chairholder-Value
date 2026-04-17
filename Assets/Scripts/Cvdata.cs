using System;


[Serializable]
public class CVData
{
    public string id;
    public MetaData meta;
    public Personal personal;
    public Application application;
    public Experience[] experience;
    public Education[] education;
    public string[] skills;
    public Reference[] references;

    [Serializable]
    public class MetaData
    {
        public string difficulty;        // "easy" | "medium" | "hard"
        public string correctDecision;   // "approve" | "reject"  (maps from correct_decision)
        public string decisionReason;
        public string[] redFlags;
        public string[] greenFlags;
        public string photoRef;
    }

    [Serializable]
    public class Personal
    {
        public string fullName;
        public int age;
        public string dob;
        public string nationality;
        public string address;
        public string phone;
        public string email;
    }

    [Serializable]
    public class Application
    {
        public string position;
        public string department;
        public string referenceNumber;
        public string dateReceived;
        public string coverNote;
    }

    [Serializable]
    public class Experience
    {
        public string jobTitle;
        public string company;
        public int startYear;
        public string endYear; 
        public string description;
    }

    [Serializable]
    public class Education
    {
        public string qualification;
        public string institution;
        public string yearCompleted;
        public string grade;
    }

    [Serializable]
    public class Reference
    {
        public string name;
        public string title;
        public string company;
        public bool contactable;
    }
}