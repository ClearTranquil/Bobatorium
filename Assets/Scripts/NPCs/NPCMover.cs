using UnityEngine;

public class NPCMover : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    private Transform target;
    [SerializeField] private GameObject cupSlot;

    public void MoveTo(Transform m_target)
    {
        target = m_target;
    }

    public void TeleportTo(Transform m_target)
    {
        target = null;
        transform.position = m_target.position;
    }

    private void Update()
    {
        if (!target)
            return;

        transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime); 
    }

    public GameObject GetCupSlot()
    {
        return cupSlot;
    }
}
