namespace Agents
{
    public class Idle : FSMInterface
    {
        private AgentFSM _agent;

        public Idle(AgentFSM agent)
        {
            _agent = agent;
        }

        public void Enter()
        {
            _agent.SetTargetNode = null;
        }

        public void Exit() { }

        public void Reset() { }

        public void Update() { }
    }
}

