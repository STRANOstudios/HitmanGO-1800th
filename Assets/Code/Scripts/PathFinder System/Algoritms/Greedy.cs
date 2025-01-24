namespace PathSystem.PathFinding
{
    public class GreedyPathFinder : PathFinder
    {
        protected override void AlgorithmSpecificImplementation(Node node)
        {
            if (mClosedList.Exists(n => n.Location == node))
                return;

            float H = HCostFunction(node, Goal);
            int index = mOpenList.FindIndex(n => n.Location == node);

            if (index == -1)
            {
                // Node not in open list
                PathFinderNode newNode = new PathFinderNode(node, CurrentNode, 0f, H);
                mOpenList.Add(newNode);
            }
            else
            {
                // Node is in open list, check if H cost is better
                PathFinderNode existingNode = mOpenList[index];
                if (H < existingNode.HCost)
                {
                    existingNode.SetHCost(H);
                    existingNode.Parent = CurrentNode;
                }
            }
        }
    }
}
