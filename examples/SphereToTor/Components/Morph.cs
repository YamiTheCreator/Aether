using Aether.Core;

namespace SphereToTor.Components;

public struct Morph : Component
{
    public float Time;
    public float Duration;
    public bool IsPlaying;
    public bool IsForward;
    
    public float TorusRadiusMajor;
    public float TorusRadiusMinor;
    public float SphereRadius;
}
