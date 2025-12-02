using UnityEngine;

public class PlayerState: MonoBehaviour
{
    [field: SerializeField] public StatsType currentStat { get; private set; } = StatsType.Idling;

    public void SetPlayerMovementState(StatsType stat)
    {
        currentStat=stat;
    }

    public bool InGroundedState()
    {
        return currentStat==StatsType.Idling || 
               currentStat==StatsType.Running || 
               currentStat==StatsType.Sprinting;
    }
   
}
public enum StatsType
{
    Idling,
    Running,
    Sprinting,
    Jumping,
    Falling,
    Strafing,
        
}
