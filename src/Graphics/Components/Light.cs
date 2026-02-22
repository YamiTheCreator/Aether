using Aether.Core;
using Silk.NET.Maths;

namespace Graphics.Components;

/// <summary>
/// Light component for Phong lighting model
/// </summary>
public struct Light : Component
{
    /// <summary>
    /// Whether this light is enabled
    /// </summary>
    public bool Enabled;
    
    /// <summary>
    /// Ambient light color (фоновое излучение)
    /// </summary>
    public Vector3D<float> AmbientColor;
    
    /// <summary>
    /// Diffuse light color (диффузное излучение)
    /// </summary>
    public Vector3D<float> DiffuseColor;
    
    /// <summary>
    /// Specular light color (зеркальное излучение)
    /// </summary>
    public Vector3D<float> SpecularColor;
    
    /// <summary>
    /// Light intensity/brightness multiplier
    /// </summary>
    public float Intensity;
}
