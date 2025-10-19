namespace BC.Scripts.Common
{
    public class BCStateMachine 
    {
        private IState currentState;

        public void ChangeState(IState newState)
        {
            if (currentState != null)
            {
                currentState.Exit();
            }

            if (newState != null)
            {
                currentState = newState;
                currentState.Enter();
            }
        }
    }
}