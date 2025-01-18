using UnityEngine;

public class NodeManager : MonoBehaviour
{
    private void Awake()
    {
        ServiceLocator.Instance.NodeManager = this;
    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }


}
