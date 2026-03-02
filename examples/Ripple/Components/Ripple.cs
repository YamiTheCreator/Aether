using Aether.Core;
using Graphics.Components;

namespace Ripple.Components;

public struct Ripple : Component
{
    public float Time;
    public float Duration;
    public bool IsPlaying;
    public bool IsForward;
    public Texture2D Texture1;
    public Texture2D Texture2;
}
