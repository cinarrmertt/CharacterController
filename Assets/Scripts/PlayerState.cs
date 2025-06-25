using UnityEngine;

public class PlayerState : MonoBehaviour
{
    //mevcut state default olarak ıdle pozisyonunda, field yaparak inspector penceresinden debug ediyoruz.
    [field:SerializeField]
    public PlayerMovementState currentPlayerState { get; private set; } = PlayerMovementState.Idling;

    public void SetPlayerMovementState(PlayerMovementState playerMovementState)
    {
        currentPlayerState = playerMovementState;
    }
}
public enum PlayerMovementState
{
    Idling=0,
    Walking=1,
    Running=2,
    Sprinting=3,
    Jumping=4,
    Falling=5,
    Strafing=6,
}
