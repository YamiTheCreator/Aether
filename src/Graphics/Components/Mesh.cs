using Aether.Core;
using Graphics.Structures;
using Silk.NET.OpenGL;

namespace Graphics.Components;

public struct Mesh : Component
{
    public ArrayObject Vao;
    public BufferObject Vbo;
    public BufferObject Ebo;
    public PrimitiveType Topology;
    public Material? Material;
    public readonly int VertexCount => Vbo?.ElementCount ?? 0;
    public readonly int IndexCount => Ebo?.ElementCount ?? 0;
}