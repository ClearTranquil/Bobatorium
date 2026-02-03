using UnityEngine;

public class EmployeeSnapPoint : SnapPointBase<Employee>
{
    public bool IsBusy { get; set; }

    public bool CanReleaseEmployee()
    {
        return IsBusy;
    }

    public override bool TrySnap(Employee m_employee)
    {
        if (!base.TrySnap(m_employee))
            return false;

        m_employee.RegisterSnapPoint(this);
        return true;
    }
}
