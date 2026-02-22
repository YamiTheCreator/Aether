using Aether.Core;
using Graphics.Components;
using Graphics.Structures;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using ShaderComponent = Graphics.Components.Shader;

namespace Graphics.Systems;

public class MeshSystem( GL gl ) : SystemBase
{
    protected override void OnCreate() { }

    protected override void OnUpdate( float deltaTime ) { }

    protected override void OnRender()
    {
        Camera camera = default;
        bool cameraFound = false;

        foreach ( Entity e in World.Filter<Camera>() )
        {
            camera = World.Get<Camera>( e );
            cameraFound = true;
            break;
        }

        if ( !cameraFound ) return;

        MaterialSystem? materialSystem = World.GetSystem<MaterialSystem>();

        foreach ( Entity entity in World.Filter<Mesh, Transform>() )
        {
            ref Mesh mesh = ref World.Get<Mesh>( entity );
            ref Transform transform = ref World.Get<Transform>( entity );

            Material? material = null;

            if ( World.Has<Material>( entity ) )
            {
                material = World.Get<Material>( entity );
            }
            else if ( mesh.Material.HasValue )
            {
                material = mesh.Material.Value;
            }

            if ( material.HasValue )
            {
                RenderMeshWithMaterial( mesh, transform, material.Value, materialSystem, camera );
            }
            else
            {
                RenderMeshBasic( mesh, transform, camera );
            }
        }
    }

    protected override void OnDestroy() { }

    public static Mesh CreateMeshFromVertices( GL gl, ReadOnlySpan<Vertex> vertices, ReadOnlySpan<uint> indices )
    {
        ArrayObject vao = new( gl );
        BufferObject vbo = new( gl, BufferTargetARB.ArrayBuffer );
        BufferObject ebo = new( gl, BufferTargetARB.ElementArrayBuffer );

        vao.Bind();

        vbo.Bind();
        vbo.SetData( vertices, BufferUsageARB.StaticDraw );

        uint stride = ( uint )Vertex.SizeInBytes;

        vao.SetVertexAttribute( 0, 3, VertexAttribPointerType.Float, false, stride, 0 );
        vao.SetVertexAttribute( 1, 2, VertexAttribPointerType.Float, false, stride, 12 );
        vao.SetVertexAttribute( 2, 4, VertexAttribPointerType.Float, false, stride, 20 );
        vao.SetVertexAttribute( 3, 1, VertexAttribPointerType.Float, false, stride, 36 );
        vao.SetVertexAttribute( 4, 3, VertexAttribPointerType.Float, false, stride, 40 );

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

    public Mesh CreateMesh( Vertex[] vertices, uint[] indices, Material? material = null )
    {
        ArrayObject vao = new( gl );
        BufferObject vbo = new( gl, BufferTargetARB.ArrayBuffer );
        BufferObject ebo = new( gl, BufferTargetARB.ElementArrayBuffer );

        vao.Bind();

        vbo.Bind();
        vbo.SetData( vertices, BufferUsageARB.StaticDraw );

        uint stride = ( uint )Vertex.SizeInBytes;

        vao.SetVertexAttribute( 0, 3, VertexAttribPointerType.Float, false, stride, 0 );
        vao.SetVertexAttribute( 1, 2, VertexAttribPointerType.Float, false, stride, 12 );
        vao.SetVertexAttribute( 2, 4, VertexAttribPointerType.Float, false, stride, 20 );
        vao.SetVertexAttribute( 3, 1, VertexAttribPointerType.Float, false, stride, 36 );
        vao.SetVertexAttribute( 4, 3, VertexAttribPointerType.Float, false, stride, 40 );

        ebo.Bind();
        ebo.SetData( indices, BufferUsageARB.StaticDraw );

        vao.Unbind();

        return new Mesh
        {
            Vao = vao,
            Vbo = vbo,
            Ebo = ebo,
            Topology = PrimitiveType.Triangles,
            Material = material
        };
    }

    private void RenderMeshBasic( Mesh mesh, Transform transform, Camera camera )
    {
        ShaderSystem? shaderSystem = World.GetSystem<ShaderSystem>();

        ShaderComponent? basicShader = shaderSystem?.GetBasicShader();
        if ( !basicShader.HasValue ) return;

        ShaderProgram shader = basicShader.Value.Program;
        shader.Use();

        shader.TrySetUniform( "uViewProjection", camera.ViewProjectionMatrix );
        Matrix4X4<float> model =
            TransformSystem.CreateModelMatrix( transform.Position, transform.Rotation, transform.Scale );
        shader.SetUniform( "uModel", model );
        shader.SetUniform( "uColor", new Vector4D<float>( 1, 1, 1, 1 ) );
        shader.SetUniform( "uHasTexture", 0 );

        mesh.Vao.Bind();
        unsafe
        {
            gl.DrawElements( mesh.Topology, ( uint )mesh.IndexCount, DrawElementsType.UnsignedInt, null );
        }

        mesh.Vao.Unbind();
    }

    private void RenderMeshWithMaterial( Mesh mesh, Transform transform, Material material,
        MaterialSystem? materialSystem, Camera camera )
    {
        ShaderProgram shader;

        if ( material.Shader.HasValue )
        {
            shader = material.Shader.Value.Program;
        }
        else
        {
            ShaderSystem? shaderSystem = World.GetSystem<ShaderSystem>();
            ShaderComponent? pbrShader = shaderSystem?.GetPbrShader();
            if ( !pbrShader.HasValue ) return;
            shader = pbrShader.Value.Program;
        }

        shader.Use();

        shader.TrySetUniform( "uViewProjection", camera.ViewProjectionMatrix );
        Matrix4X4<float> model =
            TransformSystem.CreateModelMatrix( transform.Position, transform.Rotation, transform.Scale );
        shader.TrySetUniform( "uModel", model );

        Vector3D<float> cameraPosition = Vector3D<float>.Zero;
        foreach ( Entity e in World.Filter<Camera, Transform>() )
        {
            cameraPosition = World.Get<Transform>( e ).Position;
            break;
        }

        shader.TrySetUniform( "uCameraPosition", cameraPosition );

        BindLights( shader );

        materialSystem?.BindMaterial( ref material, shader );
        material.SetCustomUniforms?.Invoke( shader );

        mesh.Vao.Bind();
        unsafe
        {
            gl.DrawElements( mesh.Topology, ( uint )mesh.IndexCount, DrawElementsType.UnsignedInt, null );
        }

        mesh.Vao.Unbind();
    }

    private void BindLights( ShaderProgram shader )
    {
        int lightCount = 0;
        Vector3D<float>[] positions = new Vector3D<float>[ 4 ];
        Vector4D<float>[] colors = new Vector4D<float>[ 4 ];
        Vector3D<float>[] ambient = new Vector3D<float>[ 4 ];
        Vector3D<float>[] specular = new Vector3D<float>[ 4 ];

        foreach ( Entity entity in World.Filter<Light>() )
        {
            if ( lightCount >= 4 ) break;
            if ( !World.Has<Transform>( entity ) )
                continue;

            ref Light light = ref World.Get<Light>( entity );
            if ( !light.Enabled ) continue;

            ref Transform transform = ref World.Get<Transform>( entity );

            positions[ lightCount ] = transform.Position;
            colors[ lightCount ] = new Vector4D<float>( light.DiffuseColor.X, light.DiffuseColor.Y,
                light.DiffuseColor.Z, light.Intensity );
            ambient[ lightCount ] = light.AmbientColor;
            specular[ lightCount ] = light.SpecularColor;

            lightCount++;
        }

        shader.TrySetUniform( "uNumLights", lightCount );

        for ( int i = 0; i < 4; i++ )
        {
            shader.TrySetUniform( $"uPointLightPositions[{i}]",
                new Vector4D<float>( positions[ i ].X, positions[ i ].Y, positions[ i ].Z, 1.0f ) );
            shader.TrySetUniform( $"uPointLightColors[{i}]", colors[ i ] );
            shader.TrySetUniform( $"uPointLightAmbient[{i}]",
                new Vector4D<float>( ambient[ i ].X, ambient[ i ].Y, ambient[ i ].Z, 1.0f ) );
            shader.TrySetUniform( $"uPointLightSpecular[{i}]",
                new Vector4D<float>( specular[ i ].X, specular[ i ].Y, specular[ i ].Z, 1.0f ) );
        }
    }
}