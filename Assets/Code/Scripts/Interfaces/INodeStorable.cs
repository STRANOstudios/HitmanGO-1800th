using PathSystem;

namespace Interfaces.PathSystem
{
    public interface INodeStorable
    {
        public void StoreNode(Node currentNode, Node targetNode);
    }
}
