using UnityEngine;

public class CupSnapPoint : SnapPointBase<Cup>, ICupInfo
{
    public bool IsBusy { get; set; }

    // ICupInfo implementation
    public float TeaFillAmount => Occupant != null ? Occupant.TeaFillAmount : 0f;
    public bool TeaFull => Occupant != null ? Occupant.IsTeaFull() : false;
    public int BobaFillAmount => Occupant != null ? Occupant.BobaCount : 0;
    public bool BobaFull => Occupant != null ? Occupant.IsBobaFull() : false;
    public bool IsSealed => Occupant != null && Occupant.GetIsSealed();

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

    public void NotifyMachineCupStateChanged()
    {
        Machine machine = GetComponentInParent<Machine>();
        if (machine != null)
            machine.OnCupStateChanged();
    }
}
