using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Graphics.Structures;

public class ShaderProgram : IDisposable
{
    private readonly GL _gl;
    public uint Handle { get; }
    public string VertexPath { get; }
    public string FragmentPath { get; }
    public string? GeometryPath { get; }

    public ShaderProgram( GL gl, string vertexPath = "../Assets/Shaders/shader.vert",
        string fragmentPath = "../Assets/Shaders/basic.frag",
        string? geometryPath = null )
    {
        _gl = gl;

        if ( vertexPath.StartsWith( "../" ) )
        {
            string solutionRoot =
                Path.GetFullPath( Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "../../../../../" ) );
            vertexPath = Path.Combine( solutionRoot, "src/Graphics/Assets/Shaders", Path.GetFileName( vertexPath ) );
        }

        if ( fragmentPath.StartsWith( "../" ) )
        {
            string solutionRoot =
                Path.GetFullPath( Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "../../../../../" ) );
            fragmentPath = Path.Combine( solutionRoot, "src/Graphics/Assets/Shaders",
                Path.GetFileName( fragmentPath ) );
        }

        if ( geometryPath != null && geometryPath.StartsWith( "../" ) )
        {
            string solutionRoot =
                Path.GetFullPath( Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "../../../../../" ) );
            geometryPath = Path.Combine( solutionRoot, "src/Graphics/Assets/Shaders",
                Path.GetFileName( geometryPath ) );
        }

        VertexPath = vertexPath;
        FragmentPath = fragmentPath;
        GeometryPath = geometryPath;

        uint vertex = LoadShader( ShaderType.VertexShader, vertexPath );
        uint fragment = LoadShader( ShaderType.FragmentShader, fragmentPath );
        uint? geometry = geometryPath != null ? LoadShader( ShaderType.GeometryShader, geometryPath ) : null;

        Handle = _gl.CreateProgram();

        _gl.AttachShader( Handle, vertex );
        _gl.AttachShader( Handle, fragment );
        if ( geometry.HasValue )
        {
            _gl.AttachShader( Handle, geometry.Value );
        }
        
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
        
        if ( geometry.HasValue )
        {
            _gl.DetachShader( Handle, geometry.Value );
            _gl.DeleteShader( geometry.Value );
        }
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

    public bool TrySetUniform( string name, int value )
    {
        int location = _gl.GetUniformLocation( Handle, name );
        if ( location == -1 )
        {
            return false;
        }

        _gl.Uniform1( location, value );
        return true;
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

    public unsafe bool TrySetUniform( string name, Matrix4X4<float> value )
    {
        int location = _gl.GetUniformLocation( Handle, name );
        if ( location == -1 )
        {
            return false;
        }

        _gl.UniformMatrix4( location, 1, false, ( float* )&value );
        return true;
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
    
    public bool TrySetUniform( string name, float value )
    {
        int location = _gl.GetUniformLocation( Handle, name );
        if ( location == -1 )
        {
            return false;
        }

        _gl.Uniform1( location, value );
        return true;
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
    
    public bool TrySetUniform( string name, Vector2D<float> value )
    {
        int location = _gl.GetUniformLocation( Handle, name );
        if ( location == -1 )
        {
            return false;
        }

        _gl.Uniform2( location, value.X, value.Y );
        return true;
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
    
    public bool TrySetUniform( string name, Vector3D<float> value )
    {
        int location = _gl.GetUniformLocation( Handle, name );
        if ( location == -1 )
        {
            return false;
        }

        _gl.Uniform3( location, value.X, value.Y, value.Z );
        return true;
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

    public bool TrySetUniform( string name, Vector4D<float> value )
    {
        int location = _gl.GetUniformLocation( Handle, name );
        if ( location == -1 )
        {
            return false;
        }

        _gl.Uniform4( location, value.X, value.Y, value.Z, value.W );
        return true;
    }
    
    public unsafe void SetUniformArray( string name, Vector4D<float>[] values )
    {
        int location = _gl.GetUniformLocation( Handle, name );
        if ( location == -1 )
        {
            throw new Exception( $"Uniform '{name}' not found in shader program." );
        }

        fixed ( Vector4D<float>* ptr = values )
        {
            _gl.Uniform4( location, ( uint )values.Length, ( float* )ptr );
        }
    }
    
    public unsafe bool TrySetUniformArray( string name, Vector4D<float>[] values )
    {
        int location = _gl.GetUniformLocation( Handle, name );
        if ( location == -1 )
        {
            return false;
        }

        fixed ( Vector4D<float>* ptr = values )
        {
            _gl.Uniform4( location, ( uint )values.Length, ( float* )ptr );
        }
        return true;
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