using Aether.Core;
using Graphics.Components;
using Graphics.Structures;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using ShaderComponent = Graphics.Components.Shader;

namespace Graphics.Systems;

public class SpriteSystem : SystemBase
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

    private ShaderComponent? _basicShader;
    private bool _shadersInitialized;

    public SpriteSystem( GL gl )
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
        _vao.SetVertexAttribute( 3, 1, VertexAttribPointerType.Float, false, stride, 36 ); // Texture index
        _vao.SetVertexAttribute( 4, 3, VertexAttribPointerType.Float, false, stride, 40 ); // Normal

        _vao.Unbind();
    }

    protected override void OnCreate()
    {
        InitializeShaders();
    }

    protected override void OnUpdate( float deltaTime ) { }

    protected override void OnRender()
    {
        if ( !_shadersInitialized || !_basicShader.HasValue )
            return;

        SetupBlending();

        Camera? camera = GetCamera();
        if ( !camera.HasValue )
            return;

        PrepareShader( camera.Value );

        MaterialSystem? materialSystem = World.GetSystem<MaterialSystem>();

        RenderSprites( materialSystem );
    }

    private void SetupBlending()
    {
        _gl.Enable( EnableCap.Blend );
        _gl.BlendFunc( BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha );
        _gl.Disable( EnableCap.DepthTest );
    }

    private Camera? GetCamera()
    {
        foreach ( Entity e in World.Filter<Camera>() )
        {
            return World.Get<Camera>( e );
        }

        return null;
    }

    private void PrepareShader( Camera camera )
    {
        ShaderProgram shader = _basicShader!.Value.Program;
        shader.Use();
        shader.SetUniform( "uModel", Matrix4X4<float>.Identity );
        shader.SetUniform( "uViewProjection", camera.ViewProjectionMatrix );
    }

    private void RenderSprites( MaterialSystem? materialSystem )
    {
        ShaderProgram shader = _basicShader!.Value.Program;

        foreach ( Entity entity in World.Filter<Sprite, Transform>() )
        {
            ref Sprite sprite = ref World.Get<Sprite>( entity );
            ref Transform transform = ref World.Get<Transform>( entity );

            if ( _vertices.Count > 0 )
            {
                Flush();
            }

            materialSystem?.BindMaterial( ref sprite.Material, shader );

            SubmitQuad( sprite, transform );
            Flush();
        }
    }

    protected override void OnDestroy()
    {
        _vao.Dispose();
        _vbo.Dispose();
        _ebo.Dispose();
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
            _shadersInitialized = true;
        }
    }

    private void SubmitQuad( Sprite sprite, Transform transform )
        {
            if ( _vertices.Count + 4 > _maxVertices || _indices.Count + 6 > _maxIndices )
            {
                Flush();
            }

            uint indexOffset = ( uint )_vertices.Count;

            Vector2D<float>[] corners = CalculateQuadCorners( sprite );
            Vector2D<float>[] uvs = CalculateQuadUVs( sprite );

            AddQuadVertices( corners, uvs, sprite, transform );
            AddQuadIndices( indexOffset );
        }

        private Vector2D<float>[] CalculateQuadCorners( Sprite sprite )
        {
            Vector2D<float> size = sprite.Size;
            Vector2D<float> pivot = sprite.Pivot;

            float left = -pivot.X * size.X;
            float right = ( 1 - pivot.X ) * size.X;
            float top = -pivot.Y * size.Y;
            float bottom = ( 1 - pivot.Y ) * size.Y;

            return
            [
                new( left, top ),
                new( right, top ),
                new( right, bottom ),
                new( left, bottom )
            ];
        }

        private Vector2D<float>[] CalculateQuadUVs( Sprite sprite )
        {
            return
            [
                new( sprite.FlipX ? 1 : 0, sprite.FlipY ? 1 : 0 ),
                new( sprite.FlipX ? 0 : 1, sprite.FlipY ? 1 : 0 ),
                new( sprite.FlipX ? 0 : 1, sprite.FlipY ? 0 : 1 ),
                new( sprite.FlipX ? 1 : 0, sprite.FlipY ? 0 : 1 )
            ];
        }

        private void AddQuadVertices( Vector2D<float>[] corners, Vector2D<float>[] uvs, Sprite sprite, Transform transform )
        {
            Vector3D<float> position = transform.Position;
            Vector3D<float> scale = transform.Scale;
            float rotation = transform.Rotation.Z;
            Vector4D<float> color = sprite.Color;

            float cos = MathF.Cos( rotation );
            float sin = MathF.Sin( rotation );

            for ( int i = 0; i < 4; i++ )
            {
                Vector3D<float> vertexPos = TransformVertex( corners[ i ], position, scale, cos, sin );
                _vertices.Add( new Vertex( vertexPos, uvs[ i ], color, 0, Vector3D<float>.UnitZ ) );
            }
        }

        private Vector3D<float> TransformVertex( Vector2D<float> corner, Vector3D<float> position, Vector3D<float> scale, float cos, float sin )
        {
            float x = corner.X * scale.X;
            float y = corner.Y * scale.Y;

            float rotatedX = x * cos - y * sin;
            float rotatedY = x * sin + y * cos;

            return new Vector3D<float>(
                position.X + rotatedX,
                position.Y + rotatedY,
                position.Z
            );
        }

        private void AddQuadIndices( uint indexOffset )
        {
            _indices.Add( indexOffset );
            _indices.Add( indexOffset + 1 );
            _indices.Add( indexOffset + 2 );
            _indices.Add( indexOffset );
            _indices.Add( indexOffset + 2 );
            _indices.Add( indexOffset + 3 );
        }

    private void Flush()
    {
        if ( _vertices.Count == 0 || !_basicShader.HasValue )
            return;

        ShaderProgram shader = _basicShader.Value.Program;
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
}