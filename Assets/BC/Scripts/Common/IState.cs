// Enum to represent all possible states.

using System;

public interface IState
{
    void SetCompletionCallback(Action onComplete); // Set callback for state completion.
    void Enter(); // Called when entering the state.
    void Exit(); // Called when exiting the state.
}