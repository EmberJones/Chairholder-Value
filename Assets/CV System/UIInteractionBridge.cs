using UnityEngine;

public class UIInteractionBridge : MonoBehaviour            // DO NOT USE, SPECIFICALLY MADE FOR TESTING
{
    public TestRoundController TRC;

    public JobRole JR;
    public GeneratedCV GCV;

    public void NewCVBatch()
    {
        TRC.StartRound(JR);
    }

    public void SubmitChoice()
    {
        TRC.SubmitPlayerChoice(GCV);
    }
}
