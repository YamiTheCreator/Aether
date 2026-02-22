using Aether.Core;
using Silk.NET.Maths;
using Graphics.Structures;

namespace Graphics.Components;

/// <summary>
/// Material component - defines visual properties for rendering
/// Includes colors, texture, shader, and emission for global illumination
/// </summary>
public struct Material : Component
{
    /// <summary>
    /// Ambient reflection color (цвет фонового отражения)
    /// </summary>
    public Vector3D<float> AmbientColor;
    
    /// <summary>
    /// Diffuse reflection color (цвет диффузного отражения)
    /// </summary>
    public Vector3D<float> DiffuseColor;
    
    /// <summary>
    /// Specular reflection color (цвет зеркального отражения)
    /// </summary>
    public Vector3D<float> SpecularColor;
    
    /// <summary>
    /// Shininess/Specular exponent (степень зеркального блеска, 1-128)
    /// Higher values = sharper highlights
    /// </summary>
    public float Shininess;
    
    /// <summary>
    /// Emission color - light emitted by the material (for global illumination)
    /// Used for skybox and emissive materials
    /// </summary>
    public Vector3D<float> EmissionColor;
    
    /// <summary>
    /// Emission intensity multiplier
    /// </summary>
    public float EmissionIntensity;
    
    /// <summary>
    /// Base texture (optional)
    /// </summary>
    public Texture2D? Texture;
    
    /// <summary>
    /// Normal map texture for bump mapping (optional)
    /// </summary>
    public Texture2D? NormalMap;
    
    /// <summary>
    /// Metallic texture or value (PBR)
    /// For texture: grayscale where white = metallic, black = dielectric
    /// </summary>
    public Texture2D? MetallicMap;
    public float Metallic;
    
    /// <summary>
    /// Roughness texture or value (PBR)
    /// For texture: grayscale where white = rough, black = smooth
    /// </summary>
    public Texture2D? RoughnessMap;
    public float Roughness;
    
    /// <summary>
    /// Ambient Occlusion texture (PBR) - darkens crevices
    /// </summary>
    public Texture2D? AmbientOcclusionMap;
    
    /// <summary>
    /// Emissive texture for glowing parts
    /// </summary>
    public Texture2D? EmissiveMap;
    
    /// <summary>
    /// Shader to use for rendering (optional, uses default if null)
    /// </summary>
    public Shader? Shader;
    
    /// <summary>
    /// Alpha/transparency (0-1)
    /// </summary>
    public float Alpha;
    
    /// <summary>
    /// Custom uniform setter - called before rendering to set shader uniforms
    /// </summary>
    public Action<ShaderProgram>? SetCustomUniforms;
    
    /// <summary>
    /// Default constructor
    /// </summary>
    public Material()
    {
        AmbientColor = new Vector3D<float>(0.2f, 0.2f, 0.2f);
        DiffuseColor = new Vector3D<float>(0.8f, 0.8f, 0.8f);
        SpecularColor = new Vector3D<float>(1.0f, 1.0f, 1.0f);
        Shininess = 32.0f;
        EmissionColor = Vector3D<float>.Zero;
        EmissionIntensity = 0.0f;
        Texture = null;
        NormalMap = null;
        MetallicMap = null;
        Metallic = 0.0f;
        RoughnessMap = null;
        Roughness = 0.5f;
        AmbientOcclusionMap = null;
        EmissiveMap = null;
        Shader = null;
        Alpha = 1.0f;
        SetCustomUniforms = null;
    }
}
