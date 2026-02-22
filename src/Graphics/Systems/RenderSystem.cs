using Aether.Core;
using Graphics.Components;
using Graphics.Structures;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Graphics.Systems;

public class RenderSystem : SystemBase
{
    private const int _maxQuads = 10000;
    private const int _maxVertices = _maxQuads * 4;
    private const int _maxIndices = _maxQuads * 6;

    private readonly GL _gl;
    private readonly ArrayObject _vao;
    private readonly BufferObject _vbo;
    private readonly BufferObject _ebo;
    private readonly List<Vertex> _vertices = new( _maxVertices );
    private readonly List<uint> _indices = new( _maxIndices );

    private Components.Shader? _currentShader;
    private Components.Shader? _basicShader;
    private Components.Shader? _pbrShader;
    private bool _shadersInitialized;

    public RenderSystem( GL gl )
    {
        _gl = gl;
        _vao = new ArrayObject( gl );
        _vbo = new BufferObject( gl, BufferTargetARB.ArrayBuffer );
        _ebo = new BufferObject( gl, BufferTargetARB.ElementArrayBuffer );

        _vao.Bind();
        _vbo.Bind();
        _ebo.Bind();

        uint stride = ( uint )Vertex.SizeInBytes;
        _vao.SetVertexAttribute( 0, 3, VertexAttribPointerType.Float, false, stride, 0 ); // Position
        _vao.SetVertexAttribute( 1, 2, VertexAttribPointerType.Float, false, stride, 12 ); // UV
        _vao.SetVertexAttribute( 2, 4, VertexAttribPointerType.Float, false, stride, 20 ); // Color
        _vao.SetVertexAttribute( 3, 1, VertexAttribPointerType.Float, false, stride, 36 ); // TexIndex
        _vao.SetVertexAttribute( 4, 3, VertexAttribPointerType.Float, false, stride, 40 ); // Normal

        _vao.Unbind();
    }

    private void InitializeShaders()
    {
        if ( _shadersInitialized )
            return;

        ShaderSystem? shaderSystem = World.GetSystem<ShaderSystem>();
        if ( shaderSystem != null )
        {
            _basicShader = shaderSystem.CreateShader(
                "src/Graphics/Assets/Shaders/shader.vert",
                "src/Graphics/Assets/Shaders/basic.frag" );
            _pbrShader = shaderSystem.CreateShader(
                "src/Graphics/Assets/Shaders/shader.vert",
                "src/Graphics/Assets/Shaders/pbr.frag" );
            _shadersInitialized = true;
        }
    }

    protected override void OnRender()
    {
        InitializeShaders();

        _gl.Enable( EnableCap.DepthTest );
        _gl.DepthFunc( DepthFunction.Less );

        _gl.Enable( EnableCap.Blend );
        _gl.BlendFunc( BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha );

        _gl.Disable( EnableCap.CullFace );

        Camera camera = default;
        Vector3D<float> cameraPosition = Vector3D<float>.Zero;
        bool cameraFound = false;
        foreach ( Entity e in World.Filter<Camera>() )
        {
            camera = World.Get<Camera>( e );
            cameraPosition = camera.IsStatic ? camera.StaticPosition : World.Get<Transform>( e ).Position;
            cameraFound = true;
            break;
        }

        if ( !cameraFound )
            return;

        if ( _basicShader.HasValue )
        {
            _currentShader = _basicShader;
            _currentShader.Value.Program.Use();
            _currentShader.Value.Program.SetUniform( "uModel", Matrix4X4<float>.Identity );
            _currentShader.Value.Program.SetUniform( "uViewProjection", camera.ViewProjectionMatrix );
        }

        RenderSprites();

        RenderMeshes( camera, cameraPosition );
    }

    private void RenderSprites()
    {
        MaterialSystem? materialSystem = World.GetSystem<MaterialSystem>();

        foreach ( Entity entity in World.Filter<Sprite>().With<Transform>() )
        {
            ref Sprite sprite = ref World.Get<Sprite>( entity );
            ref Transform transform = ref World.Get<Transform>( entity );

            if ( _vertices.Count > 0 )
            {
                Flush();
            }

            if ( _currentShader.HasValue && materialSystem != null )
            {
                ShaderProgram shader = _currentShader.Value.Program;
                materialSystem.BindMaterial( ref sprite.Material, shader );
            }

            SubmitQuad( sprite, transform );

            Flush();
        }
    }

    private void SubmitQuad( Sprite sprite, Transform transform )
    {
        if ( _vertices.Count + 4 > _maxVertices || _indices.Count + 6 > _maxIndices )
        {
            Flush();
        }

        uint indexOffset = ( uint )_vertices.Count;

        Vector2D<float> size = sprite.Size;
        Vector2D<float> pivot = sprite.Pivot;
        Vector4D<float> color = sprite.Color;

        Vector3D<float> position = transform.Position;
        float rotation = transform.Rotation.Z;
        Vector3D<float> scale = transform.Scale;

        float cos = MathF.Cos( rotation );
        float sin = MathF.Sin( rotation );

        float left = -pivot.X * size.X;
        float right = ( 1 - pivot.X ) * size.X;
        float top = -pivot.Y * size.Y;
        float bottom = ( 1 - pivot.Y ) * size.Y;

        Vector2D<float>[] corners =
        [
            new( left, top ), // Top-left
            new( right, top ), // Top-right
            new( right, bottom ), // Bottom-right
            new( left, bottom ) // Bottom-left
        ];

        Vector2D<float>[] uvs =
        [
            new( 0, 0 ),
            new( 1, 0 ),
            new( 1, 1 ),
            new( 0, 1 )
        ];

        for ( int i = 0; i < 4; i++ )
        {
            float x = corners[ i ].X * scale.X;
            float y = corners[ i ].Y * scale.Y;

            float rotatedX = x * cos - y * sin;
            float rotatedY = x * sin + y * cos;

            Vector3D<float> vertexPos = new(
                position.X + rotatedX,
                position.Y + rotatedY,
                position.Z
            );

            _vertices.Add( new Vertex( vertexPos, uvs[ i ], color, 0, Vector3D<float>.UnitZ ) );
        }

        _indices.Add( indexOffset );
        _indices.Add( indexOffset + 1 );
        _indices.Add( indexOffset + 2 );
        _indices.Add( indexOffset );
        _indices.Add( indexOffset + 2 );
        _indices.Add( indexOffset + 3 );
    }

    private void Flush()
    {
        if ( _vertices.Count == 0 )
            return;

        if ( !_currentShader.HasValue )
            return;

        ShaderProgram shader = _currentShader.Value.Program;
        shader.Use();

        _vao.Bind();

        _vbo.Bind();
        _vbo.SetData( _vertices.ToArray(), BufferUsageARB.DynamicDraw );

        _ebo.Bind();
        _ebo.SetData( _indices.ToArray(), BufferUsageARB.DynamicDraw );

        unsafe
        {
            _gl.DrawElements( PrimitiveType.Triangles, ( uint )_indices.Count, DrawElementsType.UnsignedInt, null );
        }

        _vertices.Clear();
        _indices.Clear();
    }

    private void RenderMeshes( Camera camera, Vector3D<float> cameraPosition )
    {
        MaterialSystem? materialSystem = World.GetSystem<MaterialSystem>();

        foreach ( Entity entity in World.Filter<Mesh>().With<Transform>() )
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
                RenderMeshPhong( mesh, transform, material.Value, materialSystem, camera, cameraPosition );
            }
            else
            {
                RenderMeshBasic( mesh, transform, null, materialSystem, camera );
            }
        }
    }

    private void BindLights( ShaderProgram shader )
    {
        int lightCount = 0;
        Vector3D<float>[] positions = new Vector3D<float>[ 4 ];
        Vector4D<float>[] colors = new Vector4D<float>[ 4 ];
        Vector3D<float>[] ambient = new Vector3D<float>[ 4 ];
        Vector3D<float>[] specular = new Vector3D<float>[ 4 ];

        foreach ( Entity entity in World.Filter<Light>().With<Transform>() )
        {
            if ( lightCount >= 4 )
                break;

            ref Light light = ref World.Get<Light>( entity );
            if ( !light.Enabled )
                continue;

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

    private void RenderMeshBasic( Mesh mesh, Transform transform, Material? material, MaterialSystem? materialSystem,
        Camera camera )
    {
        if ( !_basicShader.HasValue )
            return;

        ShaderProgram shader = _basicShader.Value.Program;
        shader.Use();

        shader.TrySetUniform( "uViewProjection", camera.ViewProjectionMatrix );
        Matrix4X4<float> model =
            TransformSystem.CreateModelMatrix( transform.Position, transform.Rotation, transform.Scale );
        shader.SetUniform( "uModel", model );

        if ( material.HasValue )
        {
            Material mat = material.Value;
            materialSystem?.BindMaterial( ref mat, shader );
        }
        else
        {
            shader.SetUniform( "uColor", new Vector4D<float>( 1, 1, 1, 1 ) );
            shader.SetUniform( "uHasTexture", 0 );
        }

        mesh.Vao.Bind();
        unsafe
        {
            _gl.DrawElements( mesh.Topology, ( uint )mesh.IndexCount, DrawElementsType.UnsignedInt, null );
        }

        mesh.Vao.Unbind();
    }

    private void RenderMeshPhong( Mesh mesh, Transform transform, Material material,
        MaterialSystem? materialSystem, Camera camera, Vector3D<float> cameraPosition )
    {
        ShaderProgram shader;

        if ( material.Shader.HasValue )
        {
            shader = material.Shader.Value.Program;
        }
        else if ( _pbrShader.HasValue )
        {
            shader = _pbrShader.Value.Program;
        }
        else
        {
            return;
        }

        shader.Use();

        shader.TrySetUniform( "uViewProjection", camera.ViewProjectionMatrix );
        Matrix4X4<float> model =
            TransformSystem.CreateModelMatrix( transform.Position, transform.Rotation, transform.Scale );
        shader.TrySetUniform( "uModel", model );

        shader.TrySetUniform( "uCameraPosition", cameraPosition );

        BindLights( shader );

        materialSystem?.BindMaterial( ref material, shader );

        material.SetCustomUniforms?.Invoke( shader );

        mesh.Vao.Bind();
        unsafe
        {
            _gl.DrawElements( mesh.Topology, ( uint )mesh.IndexCount, DrawElementsType.UnsignedInt, null );
        }

        mesh.Vao.Unbind();
    }

    public void Dispose()
    {
        _vao.Dispose();
        _vbo.Dispose();
        _ebo.Dispose();
    }
}