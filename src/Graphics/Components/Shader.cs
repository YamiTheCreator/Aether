using Aether.Core;
using Graphics.Structures;
using Silk.NET.OpenGL;

namespace Graphics.Components;

public struct Shader : Component
{
    public ShaderProgram Program { get; set; }

    public readonly uint Handle => Program?.Handle ?? 0;
}