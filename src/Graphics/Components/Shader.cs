using Graphics.Structures;
using Silk.NET.OpenGL;

namespace Graphics.Components;

/// <summary>
/// Shader component that references a ShaderProgram.
/// The actual shader logic is in ShaderProgram class.
/// </summary>
public struct Shader
{
    public ShaderProgram Program { get; set; }

    /// <summary>
    /// Gets the OpenGL handle for the shader program
    /// </summary>
    public readonly uint Handle => Program?.Handle ?? 0;
}