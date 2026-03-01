using Aether.Core;
using Silk.NET.Maths;

namespace Graphics.Components;

/// <summary>
/// Particle component - contains only particle data
/// Can reference either Sprite (2D) or Mesh (3D)
/// </summary>
public struct Particle : Component
{
    /// <summary>
    /// Velocity
    /// </summary>
    public Vector3D<float> Velocity;
    
    /// <summary>
    /// Acceleration (gravity, forces)
    /// </summary>
    public Vector3D<float> Acceleration;
    
    /// <summary>
    /// Total lifetime in seconds
    /// </summary>
    public float Lifetime;
    
    /// <summary>
    /// Current age in seconds
    /// </summary>
    public float Age;
    
    /// <summary>
    /// Size over lifetime (start → end)
    /// </summary>
    public float StartSize;
    public float EndSize;
    
    /// <summary>
    /// Color over lifetime (start → end)
    /// </summary>
    public Vector4D<float> StartColor;
    public Vector4D<float> EndColor;
    
    /// <summary>
    /// Is particle alive
    /// </summary>
    public readonly bool IsAlive => Age < Lifetime;
}
