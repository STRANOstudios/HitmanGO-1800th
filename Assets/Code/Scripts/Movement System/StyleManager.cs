using PathSystem;
using UnityEngine;

public class StyleManager : Singleton<StyleManager>
{
    [Header("Style Design")]
    [SerializeField] private PathDesign pathDesign = null;

    public PathDesign PathDesign => pathDesign;
}
