namespace Agents
{
    public class Attack : FSMInterface
    {
        private AgentFSM _agentFSM;

        public Attack(AgentFSM agentFSM)
        {
            _agentFSM = agentFSM;
        }

        public void Enter() { }

        public void Exit() { }

        public void Reset() { }

        public void Update()
        {
            // move

            if (_agentFSM.IsPlayerInSight())
            {
                // change in attack
            }
        }
    }
}

