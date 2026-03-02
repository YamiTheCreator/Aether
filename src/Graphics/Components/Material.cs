using Aether.Core;
using Silk.NET.Maths;
using Graphics.Structures;

namespace Graphics.Components;

public record struct Material() : Component
{
    // Цвет фонового отражения
    public Vector3D<float> AmbientColor = new( 0.2f, 0.2f, 0.2f );

    // Цвет диффузного отражения
    public Vector3D<float> DiffuseColor = new( 0.8f, 0.8f, 0.8f );

    // Цвет зеркального отражения
    public Vector3D<float> SpecularColor = new( 1.0f, 1.0f, 1.0f );

    // Сстепень зеркального блеска, 1-128
    public float Shininess = 32.0f;

    // Свет илучаемый объектом
    public Vector3D<float> EmissionColor = Vector3D<float>.Zero;

    // Интенсивность излучения
    public float EmissionIntensity = 0.0f;

    public Texture2D? Texture = null;

    public Texture2D? NormalMap = null;

    public Texture2D? MetallicMap = null;
    public float Metallic = 0.0f;

    public Texture2D? RoughnessMap = null;
    public float Roughness = 0.5f;

    public Texture2D? AmbientOcclusionMap = null;

    public Texture2D? EmissiveMap = null;

    public Shader? Shader = null;

    public float Alpha = 1.0f;

    public Action<ShaderProgram>? SetCustomUniforms = null;
}