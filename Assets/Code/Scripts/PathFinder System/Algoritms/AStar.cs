namespace PathSystem.PathFinding
{
    public class AStarPathFinder : PathFinder
    {
        protected override void AlgorithmSpecificImplementation(Node node)
        {
            if (mClosedList.Exists(n => n.Location == node))
                return;

            float G = CurrentNode.GCost + GCostFunction(CurrentNode.Location, node);
            float H = HCostFunction(node, Goal);
            int index = mOpenList.FindIndex(n => n.Location == node);

            if (index == -1)
            {
                // Node not in open list
                PathFinderNode newNode = new PathFinderNode(node, CurrentNode, G, H);
                mOpenList.Add(newNode);
            }
            else
            {
                // Node is in open list, check if G cost is better
                PathFinderNode existingNode = mOpenList[index];
                if (G < existingNode.GCost)
                {
                    existingNode.SetGCost(G);
                    existingNode.SetHCost(H);
                    existingNode.Parent = CurrentNode;
                }
            }
        }
    }
}
