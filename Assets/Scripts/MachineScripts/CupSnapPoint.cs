using UnityEngine;

public class CupSnapPoint : SnapPointBase<Cup>
{
    public bool IsBusy { get; set; }

    public bool CanReleaseCup()
    {
        return !IsBusy;
    }

    public override bool TrySnap(Cup m_cup)
    {
        if(!base.TrySnap(m_cup))
            return false;

        m_cup.RegisterSnapPoint(this);
        return true;
    }
}
