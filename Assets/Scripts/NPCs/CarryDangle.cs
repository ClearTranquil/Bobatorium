using UnityEngine;

public class CarryDangle : MonoBehaviour
{
    [SerializeField] private Transform pivot;
    [SerializeField] private Transform model;
    [SerializeField] private float strength = 8f;
    [SerializeField] private float damping = 5f;
    [SerializeField] private float returnSpeed = 6f;
    private Quaternion lastPivotRotation;
    private Vector3 computedAngularVelocity;

    private Vector3 angularVelocity;
    private Vector3 rotationOffset;

    private bool isHeld;

    private Quaternion initialLocalRotation;

    private void Awake()
    {
        if (model)
            initialLocalRotation = model.localRotation;

        if (pivot)
            lastPivotRotation = pivot.rotation;
    }

    public void SetHeld(bool held)
    {
        isHeld = held;

        if (!held)
        {
            rotationOffset = Vector3.zero;
            angularVelocity = Vector3.zero;

            if (model)
                model.localRotation = initialLocalRotation;
        }
    }

    public Vector3 GetAngularVelocity()
    {
        return Vector3.ClampMagnitude(computedAngularVelocity, 3f);
    }

    public void ApplyMotion(Vector3 worldVelocity)
    {
        if (!isHeld || !model) return;

        Vector3 localVelocity = transform.InverseTransformDirection(worldVelocity);

        // stronger side swing than forward tilt
        Vector3 target = new Vector3(
            localVelocity.z * 0.6f,  // pitch (forward/back)
            0f,
            -localVelocity.x * 1.2f  // roll (side to side)
        ) * strength;

        // Spring toward target offset
        Vector3 force = (target - rotationOffset) * returnSpeed;

        angularVelocity += force * Time.deltaTime;

        // damping (energy loss)
        angularVelocity *= Mathf.Exp(-damping * Time.deltaTime);

        rotationOffset += angularVelocity * Time.deltaTime;

        pivot.localRotation = Quaternion.Euler(rotationOffset);
        Quaternion delta = pivot.rotation * Quaternion.Inverse(lastPivotRotation);

        delta.ToAngleAxis(out float angle, out Vector3 axis);

        if (angle > 180f) angle -= 360f;

        computedAngularVelocity = axis * (angle * Mathf.Deg2Rad / Time.deltaTime);

        lastPivotRotation = pivot.rotation;
    }

    public void ResetDangleRotation()
    {
        rotationOffset = Vector3.zero;
        angularVelocity = Vector3.zero;

        if (pivot)
            pivot.localRotation = Quaternion.identity;
    }
}