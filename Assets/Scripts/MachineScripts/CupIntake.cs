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

    public IEnumerator IntakeCup(Cup cup)
    {
        CupSnapPoint snap = parentMachine.GetAvailableSnapPoint();
        if (!snap) yield break;

        snap.IsBusy = true;
        cup.TogglePhysics(false);

        Transform target = snap.transform;

        // These are here to provide an exit case if the cup cant snap
        float maxSnapDistance = 5f; 
        float maxSnapTime = 2f;

        float elapsed = 0f;

        while (true)
        {
            if (!cup || !snap)
                break;

            if (cup.IsSnapped)
                break;

            float distance = Vector3.Distance(cup.transform.position, target.position);

            // Success condition
            if (distance <= 0.01f)
                break;

            // Abort if cup gets too far
            if (distance > maxSnapDistance)
            {
                Debug.Log("Intake aborted: cup too far");
                break;
            }

            // Abort if too much time has passed
            elapsed += Time.deltaTime;
            if (elapsed > maxSnapTime)
            {
                Debug.Log("Intake aborted: timeout");
                break;
            }

            cup.transform.position = Vector3.MoveTowards(cup.transform.position, target.position, intakeMoveSpeed * Time.deltaTime);
            cup.transform.rotation = Quaternion.Slerp(cup.transform.rotation, Quaternion.identity, 10f * Time.deltaTime);

            yield return null;
        }

        // Final snap attempt
        if (!cup.IsSnapped && snap.TrySnap(cup))
        {
            // Let the snap point sort it out 
        }
        else
        {
            cup.TogglePhysics(true);
        }

        snap.IsBusy = false;
    }
}
