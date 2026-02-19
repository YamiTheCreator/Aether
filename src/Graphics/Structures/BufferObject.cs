using Silk.NET.OpenGL;

namespace Graphics.Structures;

public class BufferObject : IDisposable
{
    private readonly GL _gl;
    public uint Handle { get; }
    public BufferTargetARB Target { get; }

    public int ElementCount { get; private set; }

    public int ElementSize { get; private set; }

    public BufferObject( GL gl, BufferTargetARB target )
    {
        _gl = gl;
        Target = target;
        Handle = _gl.GenBuffer();
        ElementCount = 0;
        ElementSize = 0;
    }

    public void Bind()
    {
        _gl.BindBuffer( Target, Handle );
    }

    public void Unbind()
    {
        _gl.BindBuffer( Target, 0 );
    }

    public unsafe void SetData<T>( ReadOnlySpan<T> data, BufferUsageARB usage ) where T : unmanaged
    {
        Bind();
        fixed ( T* ptr = data )
        {
            _gl.BufferData( Target, ( nuint )( data.Length * sizeof(T) ), ptr, usage );
        }

        ElementCount = data.Length;
        ElementSize = sizeof(T);
    }

    public unsafe void SetSubData<T>( ReadOnlySpan<T> data, int offset = 0 ) where T : unmanaged
    {
        Bind();
        fixed ( T* ptr = data )
        {
            _gl.BufferSubData( Target, offset, ( nuint )( data.Length * sizeof(T) ), ptr );
        }

        // Note: SetSubData doesn't change the total element count
        // It only updates a portion of the buffer
    }

    public void Dispose()
    {
        _gl.DeleteBuffer( Handle );
    }
}