using Silk.NET.OpenGL;
using Graphics.Components;
using Graphics.Structures;
using ArrayObject = Graphics.Structures.ArrayObject;
using BufferObject = Graphics.Structures.BufferObject;

namespace Graphics.Systems;

public class MeshSystem( GL gl )
{
    public Mesh CreateMesh( ReadOnlySpan<Vertex> vertices, ReadOnlySpan<uint> indices )
    {
        ArrayObject vao = new ArrayObject( gl );
        BufferObject vbo = new BufferObject( gl, BufferTargetARB.ArrayBuffer );
        BufferObject ebo = new BufferObject( gl, BufferTargetARB.ElementArrayBuffer );

        vao.Bind();

        // SetData automatically stores element count in the BufferObject
        vbo.SetData( vertices, BufferUsageARB.StaticDraw );
        ebo.SetData( indices, BufferUsageARB.StaticDraw );

        // Position (vec3) - location 0
        vao.SetVertexAttribute( 0, 3, VertexAttribPointerType.Float, false,
            ( uint )Vertex.SizeInBytes, 0 );

        // UV (vec2) - location 1
        vao.SetVertexAttribute( 1, 2, VertexAttribPointerType.Float, false,
            ( uint )Vertex.SizeInBytes, 12 );

        // Color (vec4) - location 2
        vao.SetVertexAttribute( 2, 4, VertexAttribPointerType.Float, false,
            ( uint )Vertex.SizeInBytes, 20 );

        // TexIndex (float) - location 3
        vao.SetVertexAttribute( 3, 1, VertexAttribPointerType.Float, false,
            ( uint )Vertex.SizeInBytes, 36 );

        vao.Unbind();

        return new Mesh
        {
            Vao = vao,
            Vbo = vbo,
            Ebo = ebo
            // VertexCount and IndexCount are now computed properties from Vbo and Ebo
        };
    }

    /// <summary>
    /// Updates mesh vertices. Element count is automatically updated in the BufferObject.
    /// </summary>
    public void UpdateMeshVertices( Mesh mesh, ReadOnlySpan<Vertex> vertices )
    {
        mesh.Vbo.SetSubData( vertices );
        // No need to set VertexCount - it's computed from Vbo.ElementCount
    }

    /// <summary>
    /// Updates mesh indices. Element count is automatically updated in the BufferObject.
    /// </summary>
    public void UpdateMeshIndices( Mesh mesh, ReadOnlySpan<uint> indices )
    {
        mesh.Ebo.SetSubData( indices );
        // No need to set IndexCount - it's computed from Ebo.ElementCount
    }

    public void BindMesh( Mesh mesh )
    {
        mesh.Vao.Bind();
    }

    public unsafe void DrawMesh( Mesh mesh )
    {
        mesh.Vao.Bind();
        gl.DrawElements( PrimitiveType.Triangles, ( uint )mesh.IndexCount, DrawElementsType.UnsignedInt, null );
    }

    public void DeleteMesh( Mesh mesh )
    {
        mesh.Vao.Dispose();
        mesh.Vbo.Dispose();
        mesh.Ebo.Dispose();
    }
}