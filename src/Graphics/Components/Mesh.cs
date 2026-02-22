using Aether.Core;
using Graphics.Structures;
using Silk.NET.OpenGL;

namespace Graphics.Components;

/// <summary>
/// Mesh component - contains geometry and material
/// For 3D rendering
/// </summary>
public struct Mesh : Component
{
    /// <summary>
    /// Vertex Array Object (uses existing structure!)
    /// </summary>
    public ArrayObject Vao;

    /// <summary>
    /// Vertex Buffer Object (uses existing structure!)
    /// </summary>
    public BufferObject Vbo;

    /// <summary>
    /// Element Buffer Object (uses existing structure!)
    /// </summary>
    public BufferObject Ebo;

    /// <summary>
    /// Primitive topology (Triangles by default, can be Lines for wireframe)
    /// </summary>
    public PrimitiveType Topology;

    /// <summary>
    /// Material for rendering (optional - can be overridden by separate Material component)
    /// </summary>
    public Material? Material;

    /// <summary>
    /// Gets the number of vertices in the mesh
    /// </summary>
    public readonly int VertexCount => Vbo?.ElementCount ?? 0;

    /// <summary>
    /// Gets the number of indices in the mesh
    /// </summary>
    public readonly int IndexCount => Ebo?.ElementCount ?? 0;
}