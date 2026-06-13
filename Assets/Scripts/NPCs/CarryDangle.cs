using UnityEngine;

public class CarryDangle : MonoBehaviour
{
    [SerializeField] private Transform model;
    [SerializeField] private float strength = 20f;
    [SerializeField] private float damping = 6f;

    private Vector3 swingVelocity;
    private Vector3 swingOffset;

    private bool isHeld;

    public void SetHeld(bool held)
    {
        isHeld = held;

        if (!held)
        {
            swingOffset = Vector3.zero;
            swingVelocity = Vector3.zero;
        }
    }

    public void ApplyMotion(Vector3 velocity)
    {
        if (!isHeld || !model) return;

        Vector3 localVelocity =
            transform.InverseTransformDirection(velocity);

        Vector3 targetRotation = new Vector3(
            localVelocity.z,
            0f,
            -localVelocity.x
        ) * strength;

        swingOffset = Vector3.SmoothDamp(
            swingOffset,
            targetRotation,
            ref swingVelocity,
            1f / damping
        );

        model.localRotation = Quaternion.Euler(swingOffset);
    }
}