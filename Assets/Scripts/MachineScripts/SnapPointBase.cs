using UnityEngine;

public abstract class SnapPointBase<Obj> : MonoBehaviour where Obj : MonoBehaviour
{
    public Obj Occupant { get; private set; }
    public bool IsOccupied => Occupant != null;

    public virtual bool TrySnap(Obj m_obj)
    {
        if (IsOccupied)
            return false;

        Occupant = m_obj;

        m_obj.transform.SetParent(transform);
        m_obj.transform.localPosition = Vector3.zero;

        return true;
    }

    public virtual void Clear()
    {
        Occupant = null;
    }
}
