using UnityEngine;

public class CupSealer : Machine
{
    public override void TriggerAction()
    {
        SealCup();
    }

    private void SealCup()
    {
        foreach(SnapPoints snap in snapPoints)
        {
            if(snap.OccupiedCup != null)
            {
                Cup cup = snap.OccupiedCup;

                cup.SealCup();
            }
        }
    }
}
