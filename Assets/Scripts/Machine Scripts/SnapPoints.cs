using UnityEngine;

public class SnapPoints : MonoBehaviour
{
    //This script tracks whether a cup is currently insterted into a snap point

    public Cup OccupiedCup { get; private set; }

    public bool IsOccupied => OccupiedCup != null;

    public bool TrySnapCup(Cup m_cup)
    {
        if (IsOccupied)
            return false;

        m_cup.transform.SetParent(transform);
        m_cup.transform.localPosition = Vector3.zero;
        OccupiedCup = m_cup;
        return true;
    }

    public void Clear()
    {
        OccupiedCup = null;
    }
}
