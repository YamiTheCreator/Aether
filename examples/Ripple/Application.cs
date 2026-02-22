using Silk.NET.Maths;
using Aether.Core;
using Graphics;
using Graphics.Components;
using Graphics.Systems;
using Graphics.Structures;
using Ripple.Components;
using Ripple.Systems;

namespace Ripple;

public class Application() : ApplicationBase(
    title: "Ripple Transition Effect",
    width: 1280,
    height: 720 )
{
    private const float _transitionDuration = 3f;

    protected override void OnInitialize()
    {
        ShaderSystem shaderSystem = new( WindowBase.Gl );
        TextureSystem textureSystem = new( WindowBase.Gl );
        MaterialSystem materialSystem = new();
        MeshSystem meshSystem = new( WindowBase.Gl );
        InputSystem inputSystem = new();

        const string projectRoot = "/Users/yami/Documents/RiderProjects/Aether";
        const string texturesPath = $"{projectRoot}/src/Graphics/Assets/Textures";

        Texture2D texture1 =
            textureSystem.CreateTextureFromFile( $"{texturesPath}/alien-slime1-bl/alien-slime1-preview.jpg" );
        Texture2D texture2 =
            textureSystem.CreateTextureFromFile(
                $"{texturesPath}/scratched-up-steel-bl/scratched-up-steel_preview.jpg" );

        Console.WriteLine( $"Loaded texture1: Handle={texture1.Handle}" );
        Console.WriteLine( $"Loaded texture2: Handle={texture2.Handle}" );

        Shader rippleShader = shaderSystem.CreateShader(
            "src/Graphics/Assets/Shaders/shader.vert",
            "examples/Ripple/Shaders/ripple.frag" );

        Input input = inputSystem.CreateInput( WindowBase.Input );

        World.SetGlobal( shaderSystem );
        World.SetGlobal( textureSystem );
        World.SetGlobal( meshSystem );
        World.SetGlobal( materialSystem );
        World.SetGlobal( inputSystem );
        World.SetGlobal( input );
        World.SetGlobal( texture1 );
        World.SetGlobal( texture2 );
        World.SetGlobal( rippleShader );

        World.AddSystem( shaderSystem );
        World.AddSystem( inputSystem );
        World.AddSystem( new CameraSystem() );
        World.AddSystem( new RippleSystem() );
        World.AddSystem( meshSystem );

        CameraSystem.CreateOrthographicCamera(
            World,
            position: new Vector3D<float>( 0f, 0f, 5f ),
            size: 1f,
            aspectRatio: ( float )WindowBase.LogicalWidth / WindowBase.LogicalHeight
        );

        CreateQuad( meshSystem, rippleShader, texture1, texture2 );
    }

    private void CreateQuad( MeshSystem meshSystem, Shader rippleShader,
        Texture2D texture1, Texture2D texture2 )
    {
        Entity entity = World.Spawn();

        World.Add( entity, new Transform
        {
            Position = Vector3D<float>.Zero,
            Rotation = Quaternion<float>.Identity,
            Scale = new Vector3D<float>( 2f, 2f, 1f )
        } );

        List<Vertex> vertices = [ ];
        List<uint> indices = [ ];

        Vector4D<float> white = new( 1, 1, 1, 1 );
        Vector3D<float> normal = new( 0, 0, 1 );

        vertices.Add( new Vertex( new Vector3D<float>( -0.5f, -0.5f, 0f ), new Vector2D<float>( 0, 0 ), white, 0,
            normal ) );
        vertices.Add( new Vertex( new Vector3D<float>( 0.5f, -0.5f, 0f ), new Vector2D<float>( 1, 0 ), white, 0,
            normal ) );
        vertices.Add(
            new Vertex( new Vector3D<float>( 0.5f, 0.5f, 0f ), new Vector2D<float>( 1, 1 ), white, 0, normal ) );
        vertices.Add( new Vertex( new Vector3D<float>( -0.5f, 0.5f, 0f ), new Vector2D<float>( 0, 1 ), white, 0,
            normal ) );

        indices.AddRange( [ 0, 1, 2, 0, 2, 3 ] );

        Material material = new()
        {
            Shader = rippleShader,
            DiffuseColor = Vector3D<float>.One,
            Alpha = 1f,
            SetCustomUniforms = shader =>
            {
                foreach ( Entity e in World.Filter<Components.Ripple>() )
                {
                    Components.Ripple ripple = World.Get<Components.Ripple>( e );

                    shader.TrySetUniform( "uTime", ripple.Time );
                    shader.TrySetUniform( "uResolution", new Vector2D<float>( 1280f, 720f ) );
                    shader.TrySetUniform( "uRippleCenter", new Vector2D<float>( 0.5f, 0.5f ) );

                    Texture2D startTexture = ripple.IsForward ? ripple.Texture1 : ripple.Texture2;
                    Texture2D endTexture = ripple.IsForward ? ripple.Texture2 : ripple.Texture1;

                    startTexture.Texture.Bind();
                    shader.TrySetUniform( "uTexture", 0 );

                    endTexture.Texture.Bind( Silk.NET.OpenGL.TextureUnit.Texture1 );
                    shader.TrySetUniform( "uTexture2", 1 );

                    break;
                }
            }
        };

        World.Add( entity, meshSystem.CreateMesh( vertices.ToArray(), indices.ToArray(), material ) );
        World.Add( entity, new Components.Ripple
        {
            Time = 0f,
            Duration = _transitionDuration,
            IsPlaying = false,
            IsForward = false,
            Texture1 = texture1,
            Texture2 = texture2
        } );
    }
}