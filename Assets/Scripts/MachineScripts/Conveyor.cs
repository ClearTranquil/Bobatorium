using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class Conveyor : Machine
{
    [Header("Conveyor settings")]
    [SerializeField] private float baseSpeed = 2f;
    private float speed;
    [SerializeField] private Vector3 localDirection = Vector3.forward;
    [SerializeField] private float intakeCheckRadius = 0.5f;
    [SerializeField] LayerMask intakeLayerMask;

    private readonly List<Cup> cupsOnBelt = new();

    protected override void Awake()
    {
        speed = baseSpeed;
    }

    private void FixedUpdate()
    {
        Vector3 worldDir = transform.TransformDirection(localDirection.normalized);

        // Move cups along the belt
        for (int i = 0; i < cupsOnBelt.Count; i++)
        {
            Cup cup = cupsOnBelt[i];
            Rigidbody rb = cup.GetRb();
            if (rb == null || rb.isKinematic) continue;

            rb.linearVelocity = worldDir * speed;

            // Check nearby cup intakers
            CheckForIntake(cup);
        }
    }

    private void CheckForIntake(Cup cup)
    {
        Collider[] hits = Physics.OverlapSphere(cup.transform.position, intakeCheckRadius, intakeLayerMask);
        foreach(Collider hit in hits)
        {
            CupIntake intake = hit.GetComponent<CupIntake>();
            if (!intake) continue;

            if (intake.CanAcceptCup(cup))
            {
                StartCoroutine(intake.IntakeCup(cup));
                break;
            }
        }
    }

    public override void SetTriggerStrength(float value)
    {
        value = Mathf.Clamp01(value);
        speed = baseSpeed * value;
    }

    private void OnCollisionStay(Collision collision)
    {
        Cup cup = collision.gameObject.GetComponent<Cup>();
        if (cup != null && !cupsOnBelt.Contains(cup)) cupsOnBelt.Add(cup);
    }

    private void OnCollisionExit(Collision collision)
    {
        Cup cup = collision.gameObject.GetComponent<Cup>();
        if (cup != null)
            cupsOnBelt.Remove(cup);
    }
}
