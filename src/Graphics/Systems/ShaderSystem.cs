using Aether.Core;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Graphics.Structures;
using ShaderComponent = Graphics.Components.Shader;

namespace Graphics.Systems;

public class ShaderSystem( GL gl ) : SystemBase
{
    private ShaderComponent? _basicShader;
    private ShaderComponent? _pbrShader;

    public ShaderComponent CreateShader( string vertexPath = "src/Graphics/Assets/Shaders/shader.vert",
        string fragmentPath = "src/Graphics/Assets/Shaders/shader.frag",
        string? geometryPath = null )
    {
        string basePath = AppDomain.CurrentDomain.BaseDirectory;
        string solutionRoot = Path.GetFullPath( Path.Combine( basePath, "../../../../../" ) );

        string fullVertexPath = Path.Combine( solutionRoot, vertexPath );
        string fullFragmentPath = Path.Combine( solutionRoot, fragmentPath );
        string? fullGeometryPath = geometryPath != null ? Path.Combine( solutionRoot, geometryPath ) : null;

        ShaderProgram program = new( gl, fullVertexPath, fullFragmentPath, fullGeometryPath );

        return new ShaderComponent
        {
            Program = program
        };
    }

    public ShaderComponent? GetBasicShader()
    {
        if ( !_basicShader.HasValue )
        {
            _basicShader = CreateShader(
                "src/Graphics/Assets/Shaders/shader.vert",
                "src/Graphics/Assets/Shaders/basic.frag" );
        }
        return _basicShader;
    }

    public ShaderComponent? GetPbrShader()
    {
        if ( !_pbrShader.HasValue )
        {
            _pbrShader = CreateShader(
                "src/Graphics/Assets/Shaders/shader.vert",
                "src/Graphics/Assets/Shaders/pbr.frag" );
        }
        return _pbrShader;
    }

    public void UseShader( ShaderComponent shader )
    {
        shader.Program.Use();
    }

    public void SetUniform( ShaderComponent shader, string name, int value )
    {
        shader.Program.SetUniform( name, value );
    }

    public void SetUniform( ShaderComponent shader, string name, Matrix4X4<float> value )
    {
        shader.Program.SetUniform( name, value );
    }

    public void SetUniform( ShaderComponent shader, string name, float value )
    {
        shader.Program.SetUniform( name, value );
    }

    public void SetUniform( ShaderComponent shader, string name, Vector2D<float> value )
    {
        shader.Program.SetUniform( name, value );
    }

    public void SetUniform( ShaderComponent shader, string name, Vector3D<float> value )
    {
        shader.Program.SetUniform( name, value );
    }

    public void SetUniform( ShaderComponent shader, string name, Vector4D<float> value )
    {
        shader.Program.SetUniform( name, value );
    }

    public void DeleteShader( ShaderComponent shader )
    {
        shader.Program.Dispose();
    }
}