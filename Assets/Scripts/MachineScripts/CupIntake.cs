using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CupIntake : MonoBehaviour
{
    // Machines have the ability to auto-insert cups that happen to be in front of them. 
    // This should only happen when cups have their physics enabled, like when on a conveyor belt. 
    
    [SerializeField] private float intakeMoveSpeed = 8f;

    private Machine parentMachine;

    private void Awake()
    {
        parentMachine = GetComponentInParent<Machine>();
    }

    public bool CanAcceptCup(Cup cup)
    {
        if (!cup) return false;
        if (cup.IsSnapped) return false;
        if (parentMachine.CheckSpecificCupCompletion(cup)) return false;
        if (parentMachine.GetAvailableSnapPoint() == null) return false;

        return true;
    }

    public IEnumerator IntakeCup(Cup m_cup)
    {
        CupSnapPoint snap = parentMachine.GetAvailableSnapPoint();
        if (!snap) yield break;

        snap.IsBusy = true;
        m_cup.TogglePhysics(false);
        Transform target = snap.transform;

        while (Vector3.Distance(m_cup.transform.position, target.position) > 0.01f)
        {
            m_cup.transform.position = Vector3.MoveTowards(m_cup.transform.position, target.position, intakeMoveSpeed * Time.deltaTime);
            m_cup.transform.rotation = Quaternion.Slerp(m_cup.transform.rotation, Quaternion.identity, 10f * Time.deltaTime);
            yield return null;
        }

        if (snap.TrySnap(m_cup))
        {
            // Let the snapPoint handle it
        }
        else
        {
            m_cup.TogglePhysics(true);
        }

        snap.IsBusy = false;
    }
}
