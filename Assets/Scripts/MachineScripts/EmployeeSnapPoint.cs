using UnityEngine;

public class EmployeeSnapPoint : SnapPointBase<Employee>
{
    public bool IsBusy { get; set; }

    public bool CanReleaseEmployee()
    {
        return IsBusy;
    }

    public override void Clear()
    {
        OnEmployeeRemoved();

        base.Clear();
    }

    public override bool TrySnap(Employee m_employee)
    {
        if (!base.TrySnap(m_employee))
            return false;

        m_employee.RegisterSnapPoint(this);
        return true;
    }

    public void OnEmployeePlaced()
    {
        if (parentMachine)
        {
            parentMachine.SetActiveEmployee(Occupant);
        }
    }

    public void OnEmployeeRemoved()
    {
        if (parentMachine)
        {
            parentMachine.RemoveActiveEmployee(Occupant);
        }
    }
}
