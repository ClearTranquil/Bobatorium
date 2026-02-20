using UnityEngine;
using System.Collections;

public class EmployeeChair : Machine
{
    [SerializeField] private float healInterval = 5f;

    public override bool CanEmployeeWork()
    {
        return true;
    }

    protected override IEnumerator EmployeeWorkLoop(Employee employee)
    {
        if (employee == null)
            yield break;

        while (employee != null)
        {
            yield return new WaitForSeconds(healInterval);

            Debug.Log("Healing fatigue for " + 1);

            if (employee == null)
                yield break;

            employee.HealFatigue(1);
        }
    }
}
