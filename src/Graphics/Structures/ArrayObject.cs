using Silk.NET.OpenGL;

namespace Graphics.Structures;

public class ArrayObject : IDisposable
{
    private readonly GL _gl;
    public uint Handle { get; }

    public ArrayObject( GL gl )
    {
        _gl = gl;
        Handle = _gl.GenVertexArray();
    }

    public void Bind()
    {
        _gl.BindVertexArray( Handle );
    }

    public void Unbind()
    {
        _gl.BindVertexArray( 0 );
    }

    public unsafe void SetVertexAttribute( uint index, int size, VertexAttribPointerType type, bool normalized,
        uint stride, int offset )
    {
        Bind();
        _gl.VertexAttribPointer( index, size, type, normalized, stride, ( void* )offset );
        _gl.EnableVertexAttribArray( index );
    }

    public void Dispose()
    {
        _gl.DeleteVertexArray( Handle );
    }
}