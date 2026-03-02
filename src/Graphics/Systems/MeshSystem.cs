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
        Camera? camera = GetCamera();
        if ( !camera.HasValue ) return;

        MaterialSystem? materialSystem = World.GetSystem<MaterialSystem>();

        foreach ( Entity entity in World.Filter<Mesh, Transform>() )
        {
            ref Mesh mesh = ref World.Get<Mesh>( entity );
            ref Transform transform = ref World.Get<Transform>( entity );

            Material? material = GetMaterial( entity, mesh );

            if ( material.HasValue )
            {
                RenderMeshWithMaterial( mesh, transform, material.Value, materialSystem, camera.Value );
            }
            else
            {
                RenderMeshBasic( mesh, transform, camera.Value );
            }
        }
    }

    private Camera? GetCamera()
    {
        foreach ( Entity e in World.Filter<Camera>() )
        {
            return World.Get<Camera>( e );
        }

        return null;
    }

    private Material? GetMaterial( Entity entity, Mesh mesh )
    {
        if ( World.Has<Material>( entity ) )
        {
            return World.Get<Material>( entity );
        }

        return mesh.Material;
    }

    protected override void OnDestroy() { }

    public static Mesh CreateMeshFromVertices( GL gl, ReadOnlySpan<Vertex> vertices, ReadOnlySpan<uint> indices )
    {
        ArrayObject vao = new( gl );
        BufferObject vbo = new( gl, BufferTargetARB.ArrayBuffer );
        BufferObject ebo = new( gl, BufferTargetARB.ElementArrayBuffer );

        SetupMeshBuffers( vao, vbo, ebo, vertices, indices );

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

        SetupMeshBuffers( vao, vbo, ebo, vertices, indices );

        return new Mesh
        {
            Vao = vao,
            Vbo = vbo,
            Ebo = ebo,
            Topology = PrimitiveType.Triangles,
            Material = material
        };
    }

    private static void SetupMeshBuffers( ArrayObject vao, BufferObject vbo, BufferObject ebo,
        ReadOnlySpan<Vertex> vertices, ReadOnlySpan<uint> indices )
    {
        vao.Bind();

        vbo.Bind();
        vbo.SetData( vertices, BufferUsageARB.StaticDraw );

        SetupVertexAttributes( vao );

        ebo.Bind();
        ebo.SetData( indices, BufferUsageARB.StaticDraw );

        vao.Unbind();
    }

    private static void SetupVertexAttributes( ArrayObject vao )
    {
        uint stride = ( uint )Vertex.SizeInBytes;

        vao.SetVertexAttribute( 0, 3, VertexAttribPointerType.Float, false, stride, 0 );
        vao.SetVertexAttribute( 1, 2, VertexAttribPointerType.Float, false, stride, 12 );
        vao.SetVertexAttribute( 2, 4, VertexAttribPointerType.Float, false, stride, 20 );
        vao.SetVertexAttribute( 3, 1, VertexAttribPointerType.Float, false, stride, 36 );
        vao.SetVertexAttribute( 4, 3, VertexAttribPointerType.Float, false, stride, 40 );
    }

    private void RenderMeshBasic( Mesh mesh, Transform transform, Camera camera )
    {
        ShaderProgram? shader = GetBasicShader();
        if ( shader == null ) return;

        shader.Use();

        SetBasicUniforms( shader, transform, camera );
        DrawMesh( mesh );
    }

    private ShaderProgram? GetBasicShader()
    {
        ShaderSystem? shaderSystem = World.GetSystem<ShaderSystem>();
        ShaderComponent? basicShader = shaderSystem?.GetBasicShader();
        return basicShader?.Program;
    }

    private void SetBasicUniforms( ShaderProgram shader, Transform transform, Camera camera )
    {
        shader.TrySetUniform( "uViewProjection", camera.ViewProjectionMatrix );
        Matrix4X4<float> model =
            TransformSystem.CreateModelMatrix( transform.Position, transform.Rotation, transform.Scale );
        shader.SetUniform( "uModel", model );
        shader.SetUniform( "uColor", new Vector4D<float>( 1, 1, 1, 1 ) );
        shader.SetUniform( "uHasTexture", 0 );
    }

    private void DrawMesh( Mesh mesh )
    {
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
        ShaderProgram? shader = GetMaterialShader( material );
        if ( shader == null ) return;

        shader.Use();

        SetMaterialUniforms( shader, transform, camera );
        BindLights( shader );

        materialSystem?.BindMaterial( ref material, shader );
        material.SetCustomUniforms?.Invoke( shader );

        DrawMesh( mesh );
    }

    private ShaderProgram? GetMaterialShader( Material material )
    {
        if ( material.Shader.HasValue )
        {
            return material.Shader.Value.Program;
        }

        ShaderSystem? shaderSystem = World.GetSystem<ShaderSystem>();
        ShaderComponent? pbrShader = shaderSystem?.GetPbrShader();
        return pbrShader?.Program;
    }

    private void SetMaterialUniforms( ShaderProgram shader, Transform transform, Camera camera )
    {
        shader.TrySetUniform( "uViewProjection", camera.ViewProjectionMatrix );
        Matrix4X4<float> model =
            TransformSystem.CreateModelMatrix( transform.Position, transform.Rotation, transform.Scale );
        shader.TrySetUniform( "uModel", model );

        Vector3D<float> cameraPosition = GetCameraPosition();
        shader.TrySetUniform( "uCameraPosition", cameraPosition );
    }

    private Vector3D<float> GetCameraPosition()
    {
        foreach ( Entity e in World.Filter<Camera, Transform>() )
        {
            return World.Get<Transform>( e ).Position;
        }

        return Vector3D<float>.Zero;
    }

    private void BindLights( ShaderProgram shader )
    {
        const int maxLights = 4;
        LightData[] lights = CollectLights( maxLights );

        shader.TrySetUniform( "uNumLights", lights.Length );

        for ( int i = 0; i < maxLights; i++ )
        {
            if ( i < lights.Length )
            {
                SetLightUniforms( shader, i, lights[ i ] );
            }
            else
            {
                SetEmptyLightUniforms( shader, i );
            }
        }
    }

    private LightData[] CollectLights( int maxLights )
    {
        List<LightData> lights = [ ];

        foreach ( Entity entity in World.Filter<Light>() )
        {
            if ( lights.Count >= maxLights ) break;
            if ( !World.Has<Transform>( entity ) ) continue;

            ref Light light = ref World.Get<Light>( entity );
            if ( !light.Enabled ) continue;

            ref Transform transform = ref World.Get<Transform>( entity );

            lights.Add( new LightData
            {
                Position = transform.Position,
                DiffuseColor = light.DiffuseColor,
                Intensity = light.Intensity,
                AmbientColor = light.AmbientColor,
                SpecularColor = light.SpecularColor
            } );
        }

        return lights.ToArray();
    }

    private void SetLightUniforms( ShaderProgram shader, int index, LightData light )
    {
        shader.TrySetUniform( $"uPointLightPositions[{index}]",
            new Vector4D<float>( light.Position.X, light.Position.Y, light.Position.Z, 1.0f ) );
        shader.TrySetUniform( $"uPointLightColors[{index}]",
            new Vector4D<float>( light.DiffuseColor.X, light.DiffuseColor.Y, light.DiffuseColor.Z, light.Intensity ) );
        shader.TrySetUniform( $"uPointLightAmbient[{index}]",
            new Vector4D<float>( light.AmbientColor.X, light.AmbientColor.Y, light.AmbientColor.Z, 1.0f ) );
        shader.TrySetUniform( $"uPointLightSpecular[{index}]",
            new Vector4D<float>( light.SpecularColor.X, light.SpecularColor.Y, light.SpecularColor.Z, 1.0f ) );
    }

    private void SetEmptyLightUniforms( ShaderProgram shader, int index )
    {
        shader.TrySetUniform( $"uPointLightPositions[{index}]", Vector4D<float>.Zero );
        shader.TrySetUniform( $"uPointLightColors[{index}]", Vector4D<float>.Zero );
        shader.TrySetUniform( $"uPointLightAmbient[{index}]", Vector4D<float>.Zero );
        shader.TrySetUniform( $"uPointLightSpecular[{index}]", Vector4D<float>.Zero );
    }

    private struct LightData
    {
        public Vector3D<float> Position;
        public Vector3D<float> DiffuseColor;
        public float Intensity;
        public Vector3D<float> AmbientColor;
        public Vector3D<float> SpecularColor;
    }
}