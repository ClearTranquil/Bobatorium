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

        NotifyMachineCupStateChanged();

        m_cup.RegisterSnapPoint(this);
        return true;
    }

    public override void Clear()
    {
        base.Clear();
        NotifyMachineCupStateChanged();
    }

    private void NotifyMachineCupStateChanged()
    {
        Machine machine = GetComponentInParent<Machine>();
        if (machine != null)
            machine.OnCupStateChanged();
    }
}
