namespace Agents
{
    public class Move : FSMInterface
    {
        private AgentFSM _agentFSM;

        public Move(AgentFSM agentFSM)
        {
            _agentFSM = agentFSM;
        }

        public void Enter() { }

        public void Exit() { }

        public void Reset() { }

        public void Update() { }
    }
}

