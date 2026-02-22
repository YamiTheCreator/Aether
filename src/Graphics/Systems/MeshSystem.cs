using Silk.NET.OpenGL;
using Graphics.Components;
using Graphics.Structures;
using ArrayObject = Graphics.Structures.ArrayObject;
using BufferObject = Graphics.Structures.BufferObject;
using PrimitiveType = Silk.NET.OpenGL.PrimitiveType;

namespace Graphics.Systems;

public class MeshSystem( GL gl )
{
    public Mesh CreateMesh( ReadOnlySpan<Vertex> vertices, ReadOnlySpan<uint> indices )
    {
        ArrayObject vao = new( gl );
        BufferObject vbo = new( gl, BufferTargetARB.ArrayBuffer );
        BufferObject ebo = new( gl, BufferTargetARB.ElementArrayBuffer );

        vao.Bind();

        vbo.Bind();
        vbo.SetData( vertices, BufferUsageARB.StaticDraw );

        uint stride = ( uint )Vertex.SizeInBytes;

        // Position (vec3) - location 0
        vao.SetVertexAttribute( 0, 3, VertexAttribPointerType.Float, false, stride, 0 );

        // UV (vec2) - location 1
        vao.SetVertexAttribute( 1, 2, VertexAttribPointerType.Float, false, stride, 12 );

        // Color (vec4) - location 2
        vao.SetVertexAttribute( 2, 4, VertexAttribPointerType.Float, false, stride, 20 );

        // TexIndex (float) - location 3
        vao.SetVertexAttribute( 3, 1, VertexAttribPointerType.Float, false, stride, 36 );

        // Normal (vec3) - location 4
        vao.SetVertexAttribute( 4, 3, VertexAttribPointerType.Float, false, stride, 40 );

        // Tangent (vec3) - location 5
        vao.SetVertexAttribute( 5, 3, VertexAttribPointerType.Float, false, stride, 52 );

        // Bitangent (vec3) - location 6
        vao.SetVertexAttribute( 6, 3, VertexAttribPointerType.Float, false, stride, 64 );

        // Bind EBO (element buffer) to VAO
        ebo.Bind();
        ebo.SetData( indices, BufferUsageARB.StaticDraw );

        vao.Unbind();

        return new Mesh
        {
            Vao = vao,
            Vbo = vbo,
            Ebo = ebo,
            Topology = PrimitiveType.Triangles,
            Material = null
        };
    }

    public Mesh CreateMesh( ReadOnlySpan<Vertex> vertices, ReadOnlySpan<uint> indices, Material material )
    {
        Mesh mesh = CreateMesh( vertices, indices );
        mesh.Material = material;
        return mesh;
    }
}