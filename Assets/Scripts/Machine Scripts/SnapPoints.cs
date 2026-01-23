using UnityEngine;

public class SnapPoints : MonoBehaviour
{
    //This script tracks whether a cup is currently insterted into a snap point

    public Cup OccupiedCup { get; private set; }
    public bool IsBusy { get; set; }

    public bool IsOccupied => OccupiedCup != null;

    public bool TrySnapCup(Cup m_cup)
    {
        if (IsOccupied)
            return false;

        OccupiedCup = m_cup;

        m_cup.transform.SetParent(transform);
        m_cup.transform.localPosition = Vector3.zero;
        m_cup.RegisterSnapPoint(this); 

        return true;
    }

    public void Clear()
    {
        OccupiedCup = null;
    }

    public bool CanReleaseCup()
    {
        return !IsBusy;
    }
}
