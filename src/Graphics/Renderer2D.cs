using System.Numerics;
using System.Runtime.InteropServices;
using Aether.Core.Structures;
using Graphics.Textures;
using Graphics.Windowing;
using Silk.NET.OpenGL;
using Shader = Graphics.Shaders.Shader;

namespace Graphics;

public class Renderer2D : IDisposable
{
    private const int _maxQuads = 10000;
    private const int _maxVertices = _maxQuads * 4;
    private const int _maxIndices = _maxQuads * 6;

    private readonly GL _gl;
    private readonly BatchData _batchData;
    private readonly List<QuadVertex> _vertices;
    private readonly List<uint> _indices;

    private Shader? _shader;
    private Texture2D? _currentTexture;
    private Matrix4x4 _viewProjection;
    private bool _isInBatch;

    public Renderer2D()
    {
        _gl = MainWindow.Gl;
        _vertices = new List<QuadVertex>( _maxVertices );
        _indices = new List<uint>( _maxIndices );
        _batchData = new BatchData( _gl, _maxVertices, _maxIndices );
    }

    public void Begin( Matrix4x4 viewProjection, Shader shader )
    {
        if ( _isInBatch )
            throw new InvalidOperationException( "Already in batch. Call End() first." );

        _viewProjection = viewProjection;
        _shader = shader;
        _currentTexture = null;
        _vertices.Clear();
        _indices.Clear();
        _isInBatch = true;
    }

    public void SubmitQuad( ReadOnlySpan<QuadVertex> vertices, Texture2D texture )
    {
        if ( vertices.Length != 4 )
            throw new ArgumentException( "Quad must have exactly 4 vertices" );

        if ( _vertices.Count + 4 > _maxVertices ||
             _indices.Count + 6 > _maxIndices ||
             ( _currentTexture != null && _currentTexture != texture ) )
        {
            Flush();
        }

        _currentTexture = texture;
        uint baseVertex = ( uint )_vertices.Count;

        foreach ( QuadVertex vertex in vertices )
        {
            _vertices.Add( vertex );
        }

        _indices.Add( baseVertex + 0 );
        _indices.Add( baseVertex + 1 );
        _indices.Add( baseVertex + 2 );
        _indices.Add( baseVertex + 2 );
        _indices.Add( baseVertex + 3 );
        _indices.Add( baseVertex + 0 );
    }

    public void SubmitVertices( ReadOnlySpan<QuadVertex> vertices, ReadOnlySpan<uint> indices, Texture2D texture )
    {
        if ( _vertices.Count + vertices.Length > _maxVertices ||
             _indices.Count + indices.Length > _maxIndices ||
             ( _currentTexture != null && _currentTexture != texture ) )
        {
            Flush();
        }

        _currentTexture = texture;
        uint baseVertex = ( uint )_vertices.Count;

        foreach ( QuadVertex vertex in vertices )
        {
            _vertices.Add( vertex );
        }

        foreach ( uint index in indices )
        {
            _indices.Add( baseVertex + index );
        }
    }

    public void End()
    {
        if ( !_isInBatch )
            throw new InvalidOperationException( "Not in batch. Call Begin() first." );

        Flush();
        _isInBatch = false;
    }

    private void Flush()
    {
        if ( _vertices.Count == 0 || _indices.Count == 0 || _shader == null || _currentTexture == null )
            return;

        // Use CollectionsMarshal.AsSpan to avoid ToArray() allocation
        ReadOnlySpan<QuadVertex> vertexSpan = CollectionsMarshal.AsSpan( _vertices );
        ReadOnlySpan<uint> indexSpan = CollectionsMarshal.AsSpan( _indices );
        
        _batchData.UpdateData( vertexSpan, indexSpan );

        _shader.Use();
        _currentTexture.Bind();

        _shader.SetUniform( "uViewProjection", _viewProjection );
        _shader.SetUniform( "uTexture", 0 );

        _gl.BindVertexArray( _batchData.Vao );
        unsafe
        {
            _gl.DrawElements( PrimitiveType.Triangles, ( uint )_indices.Count, DrawElementsType.UnsignedInt, null );
        }

        _vertices.Clear();
        _indices.Clear();
    }

    public void Dispose()
    {
        _batchData.Dispose();
    }

    private class BatchData : IDisposable
    {
        private readonly GL _gl;
        public uint Vao { get; }
        private readonly uint _vbo;
        private readonly uint _ebo;

        public unsafe BatchData( GL gl, int maxVertices, int maxIndices )
        {
            _gl = gl;

            Vao = _gl.GenVertexArray();
            _gl.BindVertexArray( Vao );

            _vbo = _gl.GenBuffer();
            _gl.BindBuffer( BufferTargetARB.ArrayBuffer, _vbo );
            _gl.BufferData( BufferTargetARB.ArrayBuffer,
                ( nuint )( maxVertices * QuadVertex.SizeInBytes ),
                null,
                BufferUsageARB.DynamicDraw );

            // Position (vec3) - location 0
            _gl.VertexAttribPointer( 0, 3, VertexAttribPointerType.Float, false,
                ( uint )QuadVertex.SizeInBytes, ( void* )0 );
            _gl.EnableVertexAttribArray( 0 );

            // UV (vec2) - location 1
            _gl.VertexAttribPointer( 1, 2, VertexAttribPointerType.Float, false,
                ( uint )QuadVertex.SizeInBytes, ( void* )12 );
            _gl.EnableVertexAttribArray( 1 );

            // Color (vec4) - location 2
            _gl.VertexAttribPointer( 2, 4, VertexAttribPointerType.Float, false,
                ( uint )QuadVertex.SizeInBytes, ( void* )20 );
            _gl.EnableVertexAttribArray( 2 );

            _ebo = _gl.GenBuffer();
            _gl.BindBuffer( BufferTargetARB.ElementArrayBuffer, _ebo );
            _gl.BufferData( BufferTargetARB.ElementArrayBuffer,
                ( nuint )( maxIndices * sizeof(uint) ),
                null,
                BufferUsageARB.DynamicDraw );

            _gl.BindVertexArray( 0 );
        }

        public unsafe void UpdateData( ReadOnlySpan<QuadVertex> vertices, ReadOnlySpan<uint> indices )
        {
            _gl.BindBuffer( BufferTargetARB.ArrayBuffer, _vbo );
            fixed ( QuadVertex* ptr = vertices )
            {
                _gl.BufferSubData( BufferTargetARB.ArrayBuffer, 0,
                    ( nuint )( vertices.Length * QuadVertex.SizeInBytes ), ptr );
            }

            _gl.BindBuffer( BufferTargetARB.ElementArrayBuffer, _ebo );
            fixed ( uint* ptr = indices )
            {
                _gl.BufferSubData( BufferTargetARB.ElementArrayBuffer, 0,
                    ( nuint )( indices.Length * sizeof(uint) ), ptr );
            }
        }

        public void Dispose()
        {
            _gl.DeleteBuffer( _vbo );
            _gl.DeleteBuffer( _ebo );
            _gl.DeleteVertexArray( Vao );
        }
    }
}