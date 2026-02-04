using UnityEngine;
using System;

public abstract class MachineTriggerBase : MonoBehaviour, IInteractable
{
    public Machine machine;
    protected bool isHeld = false;
    
    // Called when player clicks/taps on trigger
    public virtual void Interact(PlayerControls player)
    {
        isHeld = true;
    }

    // Called when player lets go of trigger
    public virtual void OnRelease(Vector3 releasePos)
    {
        isHeld = false;
    }

    // Is called while the player is holding the trigger
    public virtual void OnHold()
    {

    }

    protected void TriggerMachine()
    {
        // This is the payload thats triggered when a button/lever/ripcord is engaged 
        if (machine)
        {
            machine.TriggerAction();
            //Debug.Log("Machine triggered");
        } else
        {
            Debug.Log("Machine not found");
        }
    }

    protected void StopTriggerMachine()
    {
        if (machine)
        {
            machine.StopTrigger();
        } 
    }

    public void BeginHold()
    {
        isHeld = true;
    }

    public void EndHold()
    {
        isHeld = false;
        OnRelease(Vector3.zero);
    }

    // There shouldnt be any reason triggers cant be interacted with
    public bool CanInteract(PlayerControls player)
    {
        return true;
    }

    // How employees interact with teiggers 
    public virtual void BeginRemoteHold()
    {
        isHeld = true;
    }

    public virtual void EndRemoteHold()
    {
        isHeld = false;
        OnRelease(Vector3.zero);
    }

    public virtual void TickRemoteHold(float deltaTime, float workSpeed)
    {
        // Default: do nothing
        // Lever / ripcord can override if needed
    }

    // Used by employee interactions
    public virtual float GetHoldDuration(float workSpeed)
    {
        return 0.1f / workSpeed;
    }
}
