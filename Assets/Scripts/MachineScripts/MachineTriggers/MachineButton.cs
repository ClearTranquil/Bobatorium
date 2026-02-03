using Unity.VisualScripting;
using UnityEngine;

public class MachineButton : MachineTriggerBase
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public override void Interact(PlayerControls player)
    {
        //Debug.Log("Button pressed");
        TriggerMachine();
    }

    private void OnEnable()
    {
        if (machine != null)
            machine.OnMachineTriggered += AnimateButton;
    }

    private void OnDisable()
    {
        if (machine != null)
            machine.OnMachineTriggered -= AnimateButton;
    }


    private void AnimateButton()
    {
        if (animator)
            animator.SetTrigger("Trigger");
    }

}
