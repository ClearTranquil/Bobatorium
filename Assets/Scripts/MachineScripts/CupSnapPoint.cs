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
    public bool IsSnapped => Occupant.IsSnapped;

    [Header("Cup Ejection")]
    [SerializeField] private Transform ejectPoint;
    [SerializeField] private Vector3 ejectDirection = Vector3.forward;
    [SerializeField] private float ejectForce = 1.5f;

    public bool CanReleaseCup()
    {
        return !IsBusy;
    }

    public override bool TrySnap(Cup m_cup)
    {
        if(!base.TrySnap(m_cup))
            return false;

        NotifyMachineCupInserted();

        m_cup.RegisterSnapPoint(this);
        return true;
    }

    public override void Clear()
    {
        Occupant.ClearSnapPoint();
        base.Clear();
    }

    public void NotifyMachineCupInserted()
    {
        Machine machine = GetComponentInParent<Machine>();
        if (machine != null)
            machine.OnCupInserted(Occupant);
    }

    /*----------Cup Ejection----------*/
    public bool TryEject()
    {
        if (!Occupant || IsBusy)
            return false;

        Cup cup = Occupant;

        // Clear snapPoint and detach
        Clear();
        cup.transform.SetParent(null);
        cup.TogglePhysics(true);

        Rigidbody rb = cup.GetRb();
        if (!rb) return false;

        rb.Sleep();
        rb.WakeUp();

        // Set cup to ejectPoint position
        rb.transform.position = ejectPoint.position;

        // Force cup to be upright and aligned with machine forward
        Machine parentMachine = GetComponentInParent<Machine>();
        rb.transform.rotation = Quaternion.LookRotation(parentMachine.transform.forward, Vector3.up);

        // Apply impulse in machine forward direction
        rb.AddForce(parentMachine.transform.forward * ejectForce, ForceMode.Impulse);

        return true;
    }

}
