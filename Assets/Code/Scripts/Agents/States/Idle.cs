namespace Agents
{
    public class Idle : FSMInterface
    {
        private AgentFSM _agentFSM;

        public Idle(AgentFSM agentFSM)
        {
            _agentFSM = agentFSM;
        }

        public void Enter()
        {
            _agentFSM.SetTargetNode = null;
        }

        public void Exit() { }

        public void Reset() { }

        public void Update() { }
    }
}

