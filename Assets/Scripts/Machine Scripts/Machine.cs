using UnityEngine;

public class Machine : MonoBehaviour
{
    [Header("Snap Points")]
    [SerializeField] protected SnapPoints[] snapPoints;

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
