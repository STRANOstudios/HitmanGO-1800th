using System.Collections.Generic;

namespace PathSystem.PathFinding
{
    public enum PathFindingAlgorithm
    {
        AStar,
        Dijkstra,
        Greedy_Best_First,
    }

    public enum PathFinderStatus
    {
        NOT_STARTED,
        SUCCESS,
        FAILURE,
        RUNNING,
    }

    public class PathFinder
    {
        public class PathFinderNode
        {
            public PathFinderNode Parent { get; set; }
            public Node Location { get; private set; }
            public float FCost { get { return GCost + HCost; } }
            public float GCost { get; private set; }
            public float HCost { get; private set; }

            public PathFinderNode(Node location, PathFinderNode parent, float gCost, float hCost)
            {
                Location = location;
                Parent = parent;
                GCost = gCost;
                HCost = hCost;
            }

            public void SetGCost(float gCost)
            {
                GCost = gCost;
            }

            public void SetHCost(float hCost)
            {
                HCost = hCost;
            }
        }

        public PathFinderStatus Status { get; private set; } = PathFinderStatus.NOT_STARTED;
        protected List<PathFinderNode> mOpenList = new List<PathFinderNode>();
        protected List<PathFinderNode> mClosedList = new List<PathFinderNode>();
        public Node Start { get; private set; }
        public Node Goal { get; private set; }
        public PathFinderNode CurrentNode { get; private set; }

        public delegate float CostFunction(Node a, Node b);
        public CostFunction HCostFunction { get; set; }
        public CostFunction GCostFunction { get; set; }

        // Helper function to get the least cost node
        protected PathFinderNode GetLeastCostNode(List<PathFinderNode> list)
        {
            int bestIndex = 0;
            float bestPriority = list[0].FCost;

            for (int i = 1; i < list.Count; i++)
            {
                if (list[i].FCost < bestPriority)
                {
                    bestPriority = list[i].FCost;
                    bestIndex = i;
                }
            }

            return list[bestIndex];
        }

        // Initialize pathfinding with start and goal nodes
        public void Initialize(Node start, Node goal)
        {
            Start = start;
            Goal = goal;

            // Initialize the open list with the start node
            float H = HCostFunction(Start, Goal);
            PathFinderNode root = new PathFinderNode(Start, null, 0f, H);
            mOpenList.Add(root);

            CurrentNode = root;
            Status = PathFinderStatus.RUNNING;
        }

        // Reset the pathfinding
        public void Reset()
        {
            if (Status == PathFinderStatus.RUNNING)
                return;

            mOpenList.Clear();
            mClosedList.Clear();
            Status = PathFinderStatus.NOT_STARTED;
        }

        // Perform a step in the pathfinding process
        public PathFinderStatus Step()
        {
            if (mOpenList.Count == 0)
            {
                Status = PathFinderStatus.FAILURE;
                return Status;
            }

            // Move the current node to the closed list
            mClosedList.Add(CurrentNode);

            // Get the least cost node from the open list
            CurrentNode = GetLeastCostNode(mOpenList);
            mOpenList.Remove(CurrentNode);

            // Check if we have reached the goal
            if (CurrentNode.Location == Goal)
            {
                Status = PathFinderStatus.SUCCESS;
                return Status;
            }

            // Loop through all neighbours and add them to the open list
            foreach (Node neighbour in CurrentNode.Location.neighbours)
            {
                AlgorithmSpecificImplementation(neighbour);
            }

            Status = PathFinderStatus.RUNNING;
            return Status;
        }

        // Abstract method for each specific pathfinding algorithm
        protected virtual void AlgorithmSpecificImplementation(Node node) { }
    }
}
