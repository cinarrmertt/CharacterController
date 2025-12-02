using UnityEngine;

public class PlayerState: MonoBehaviour
{
    [field: SerializeField] public StatsType currentStat { get; private set; } = StatsType.Idling;

    public void SetPlayerMovementState(StatsType stat)
    {
        currentStat=stat;
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
