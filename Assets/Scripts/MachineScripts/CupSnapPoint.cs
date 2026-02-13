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
        Debug.Log("Trying to eject cup");
        
        if (!Occupant || IsBusy)
            return false;

        Cup cup = Occupant;

        // Clear snapPoint, clear parent, enable physics
        Clear();
        cup.transform.SetParent(null);
        cup.TogglePhysics(true);

        // Reset the cup's velocity, just to be safe
        Rigidbody rb = cup.GetRb();

        if (rb)
        {
            rb.Sleep();
            rb.WakeUp();

            rb.transform.position = ejectPoint.position;

            Vector3 direction = ejectDirection.normalized;
            rb.AddForce(direction * ejectForce, ForceMode.Impulse);
            rb.rotation = Quaternion.Slerp(transform.rotation, Quaternion.identity, 10f * Time.deltaTime);
        }

        return true;
    }
    
}
