using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Graphics.Structures;

public class ShaderProgram : IDisposable
{
    private readonly GL _gl;
    public uint Handle { get; }
    public string VertexPath { get; }
    public string FragmentPath { get; }

    public ShaderProgram( GL gl, string vertexPath, string fragmentPath )
    {
        _gl = gl;
        VertexPath = vertexPath;
        FragmentPath = fragmentPath;

        uint vertex = LoadShader( ShaderType.VertexShader, vertexPath );
        uint fragment = LoadShader( ShaderType.FragmentShader, fragmentPath );

        Handle = _gl.CreateProgram();

        _gl.AttachShader( Handle, vertex );
        _gl.AttachShader( Handle, fragment );
        _gl.LinkProgram( Handle );
        _gl.GetProgram( Handle, GLEnum.LinkStatus, out int status );

        if ( status == 0 )
        {
            string error = _gl.GetProgramInfoLog( Handle );
            throw new Exception( $"Shader program failed to link: {error}" );
        }

        _gl.DetachShader( Handle, vertex );
        _gl.DetachShader( Handle, fragment );
        _gl.DeleteShader( vertex );
        _gl.DeleteShader( fragment );
    }

    public void Use()
    {
        _gl.UseProgram( Handle );
    }

    public void SetUniform( string name, int value )
    {
        int location = _gl.GetUniformLocation( Handle, name );
        if ( location == -1 )
        {
            throw new Exception( $"Uniform '{name}' not found in shader program." );
        }

        _gl.Uniform1( location, value );
    }

    public unsafe void SetUniform( string name, Matrix4X4<float> value )
    {
        int location = _gl.GetUniformLocation( Handle, name );
        if ( location == -1 )
        {
            throw new Exception( $"Uniform '{name}' not found in shader program." );
        }

        _gl.UniformMatrix4( location, 1, false, ( float* )&value );
    }

    public void SetUniform( string name, float value )
    {
        int location = _gl.GetUniformLocation( Handle, name );
        if ( location == -1 )
        {
            throw new Exception( $"Uniform '{name}' not found in shader program." );
        }

        _gl.Uniform1( location, value );
    }

    public void SetUniform( string name, Vector2D<float> value )
    {
        int location = _gl.GetUniformLocation( Handle, name );
        if ( location == -1 )
        {
            throw new Exception( $"Uniform '{name}' not found in shader program." );
        }

        _gl.Uniform2( location, value.X, value.Y );
    }

    public void SetUniform( string name, Vector3D<float> value )
    {
        int location = _gl.GetUniformLocation( Handle, name );
        if ( location == -1 )
        {
            throw new Exception( $"Uniform '{name}' not found in shader program." );
        }

        _gl.Uniform3( location, value.X, value.Y, value.Z );
    }

    public void SetUniform( string name, Vector4D<float> value )
    {
        int location = _gl.GetUniformLocation( Handle, name );
        if ( location == -1 )
        {
            throw new Exception( $"Uniform '{name}' not found in shader program." );
        }

        _gl.Uniform4( location, value.X, value.Y, value.Z, value.W );
    }

    public void Dispose()
    {
        _gl.DeleteProgram( Handle );
    }

    private uint LoadShader( ShaderType type, string path )
    {
        if ( !File.Exists( path ) )
        {
            throw new FileNotFoundException( $"Shader file not found: {path}" );
        }

        string src = File.ReadAllText( path );
        uint handle = _gl.CreateShader( type );
        _gl.ShaderSource( handle, src );
        _gl.CompileShader( handle );

        string infoLog = _gl.GetShaderInfoLog( handle );
        if ( !string.IsNullOrWhiteSpace( infoLog ) )
        {
            throw new Exception( $"Error compiling {type} shader: {infoLog}" );
        }

        return handle;
    }
}