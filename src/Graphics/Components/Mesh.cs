using Graphics.Structures;

namespace Graphics.Components;

/// <summary>
/// Mesh component containing vertex and index data.
/// Element counts are stored in the BufferObjects themselves.
/// </summary>
public struct Mesh
{
    public ArrayObject Vao { get; set; }
    public BufferObject Vbo { get; set; }
    public BufferObject Ebo { get; set; }
    
    /// <summary>
    /// Gets the number of vertices in the mesh
    /// </summary>
    public readonly int VertexCount => Vbo?.ElementCount ?? 0;
    
    /// <summary>
    /// Gets the number of indices in the mesh
    /// </summary>
    public readonly int IndexCount => Ebo?.ElementCount ?? 0;
}