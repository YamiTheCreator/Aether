using System.Numerics;
using Silk.NET.OpenGL;

namespace Graphics.Shaders;

public class Shader : IDisposable
{
    private readonly uint _handle;
    private readonly GL _gl;


    public Shader( GL gl, string vertexPath = "src/Graphics/Shaders/shader.vert",
        string fragmentPath = "src/Graphics/Shaders/shader.frag" )
    {
        _gl = gl;

        string basePath = AppDomain.CurrentDomain.BaseDirectory;
        string solutionRoot = Path.GetFullPath( Path.Combine( basePath, "../../../../../" ) );

        string fullVertexPath = Path.Combine( solutionRoot, vertexPath );
        string fullFragmentPath = Path.Combine( solutionRoot, fragmentPath );

        if ( !File.Exists( fullVertexPath ) )
            throw new FileNotFoundException( $"Vertex shader not found: {fullVertexPath}" );
        if ( !File.Exists( fullFragmentPath ) )
            throw new FileNotFoundException( $"Fragment shader not found: {fullFragmentPath}" );

        uint vertex = LoadShader( ShaderType.VertexShader, fullVertexPath );
        uint fragment = LoadShader( ShaderType.FragmentShader, fullFragmentPath );

        _handle = _gl.CreateProgram();

        _gl.AttachShader( _handle, vertex );
        _gl.AttachShader( _handle, fragment );
        _gl.LinkProgram( _handle );
        _gl.GetProgram( _handle, GLEnum.LinkStatus, out int status );
        if ( status == 0 )
        {
            string error = _gl.GetProgramInfoLog( _handle );
            throw new Exception( $"Program failed to link with error: {error}" );
        }

        _gl.DetachShader( _handle, vertex );
        _gl.DetachShader( _handle, fragment );
        _gl.DeleteShader( vertex );
        _gl.DeleteShader( fragment );
    }

    public void Use() => _gl.UseProgram( _handle );


    public void SetUniform( string name, int value )
    {
        int location = _gl.GetUniformLocation( _handle, name );
        if ( location == -1 )
        {
            throw new Exception( $"{name} uniform not found on shader." );
        }

        _gl.Uniform1( location, value );
    }

    public unsafe void SetUniform( string name, ReadOnlySpan<int> values )
    {
        int location = _gl.GetUniformLocation( _handle, name );
        if ( location == -1 )
        {
            throw new Exception( $"{name} uniform not found on shader." );
        }

        fixed ( int* ptr = values )
        {
            _gl.Uniform1( location, ( uint )values.Length, ptr );
        }
    }

    public unsafe void SetUniform( string name, Matrix4x4 value )
    {
        int location = _gl.GetUniformLocation( _handle, name );
        if ( location == -1 )
        {
            throw new Exception( $"{name} uniform not found on shader." );
        }

        _gl.UniformMatrix4( location, 1, false, ( float* )&value );
    }

    public void SetUniform( string name, float value )
    {
        int location = _gl.GetUniformLocation( _handle, name );
        if ( location == -1 )
        {
            throw new Exception( $"{name} uniform not found on shader." );
        }

        _gl.Uniform1( location, value );
    }

    public void SetUniform( string name, Vector3 value )
    {
        int location = _gl.GetUniformLocation( _handle, name );
        if ( location == -1 )
        {
            throw new Exception( $"{name} uniform not found on shader." );
        }

        _gl.Uniform3( location, value.X, value.Y, value.Z );
    }

    public void SetUniform( string name, Vector4 value )
    {
        int location = _gl.GetUniformLocation( _handle, name );
        if ( location == -1 )
        {
            throw new Exception( $"{name} uniform not found on shader." );
        }

        _gl.Uniform4( location, value.X, value.Y, value.Z, value.W );
    }

    public void SetUniform( string name, bool value )
    {
        int location = _gl.GetUniformLocation( _handle, name );
        if ( location == -1 )
        {
            throw new Exception( $"{name} uniform not found on shader." );
        }

        _gl.Uniform1( location, value ? 1 : 0 );
    }

    public void Dispose()
    {
        _gl.DeleteProgram( _handle );
    }

    private uint LoadShader( ShaderType type, string path )
    {
        string src = File.ReadAllText( path );
        uint handle = _gl.CreateShader( type );
        _gl.ShaderSource( handle, src );
        _gl.CompileShader( handle );
        string infoLog = _gl.GetShaderInfoLog( handle );

        if ( !string.IsNullOrWhiteSpace( infoLog ) )
        {
            throw new Exception( $"Error compiling shader of type {type}, failed with error {infoLog}" );
        }

        return handle;
    }
}