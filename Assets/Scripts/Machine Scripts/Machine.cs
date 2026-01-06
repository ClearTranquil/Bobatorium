using UnityEngine;

public class Machine : MonoBehaviour
{
    [Header("Snap Points")]
    [SerializeField] protected SnapPoints[] snapPoints;

    public virtual void TriggerAction()
    {
        // The machine's payload when triggered
    }

    public virtual void StopTrigger()
    {
        // Stops the payload, if necessary
    }

    public SnapPoints GetAvailableSnapPoint()
    {
        foreach(var snap in snapPoints)
        {
            if (!snap.IsOccupied)
                return snap;
        }

        return null;
    }
}
