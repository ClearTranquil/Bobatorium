using UnityEngine;

public class MachineButton : MachineTriggerBase
{
    public override void Interact(PlayerControls player)
    {
        Debug.Log("Button pressed");
        TriggerMachine();
    }

}
