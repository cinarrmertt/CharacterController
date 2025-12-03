using UnityEngine;

public class PlayerState: MonoBehaviour
{
    [field: SerializeField] public StateType currentStat { get; private set; } = StateType.Idling;

    public void SetPlayerMovementState(StateType stat)
    {
        currentStat=stat;
    }

    public bool InGroundedState()
    {
        return currentStat==StateType.Idling || 
               currentStat==StateType.Running || 
               currentStat==StateType.Sprinting;
    }
   
}
public enum StateType
{
    Idling,
    Running,
    Sprinting,
    Jumping,
    Falling,
    Strafing,
        
}
