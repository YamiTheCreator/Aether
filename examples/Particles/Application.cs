using Silk.NET.Maths;
using Aether.Core;
using Aether.Core.Enums;
using Graphics;
using Graphics.Components;
using Graphics.Systems;
using Graphics.Structures;
using Particles.Systems;

namespace Particles;

public class Application() : ApplicationBase(
    title: "Particles Simulation",
    width: 1200,
    height: 900,
    createDefaultCamera: true )
{
    protected override void OnInitialize()
    {
        WindowBase.SetResizable( false );

        WindowBase.Gl.ClearColor( 0.05f, 0.05f, 0.1f, 1.0f );

        ShaderSystem shaderSystem = new( WindowBase.Gl );
        TextureSystem textureSystem = new( WindowBase.Gl );
        MaterialSystem materialSystem = new();
        MeshSystem meshSystem = new( WindowBase.Gl );

        ShaderProgram shaderProgram = new( WindowBase.Gl );
        Shader shader = new()
        {
            Program = shaderProgram
        };

        Texture2D whiteTexture = textureSystem.CreateTextureFromColor( 1, 1 );

        World.SetGlobal( shaderSystem );
        World.SetGlobal( textureSystem );
        World.SetGlobal( meshSystem );
        World.SetGlobal( materialSystem );
        World.SetGlobal( shader );
        World.SetGlobal( whiteTexture );

        World.AddSystem( shaderSystem );
        World.AddSystem( new CameraSystem() );
        World.AddSystem( new LightingSystem() );
        World.AddSystem( materialSystem );
        World.AddSystem( new ParticlePhysicsSystem() );
        World.AddSystem( new ParticleRenderSystem() );
        World.AddSystem( new RenderSystem( WindowBase.Gl ) );

        foreach ( Entity e in World.Filter<Camera>().With<Transform>() )
        {
            ref Camera camera = ref World.Get<Camera>( e );
            camera.ProjectionType = ProjectionType.Orthographic;
            camera.IsStatic = true;
            camera.StaticPosition = new Vector3D<float>( 0f, 0f, 0f );
            camera.OrthographicSize = 30f;
            camera.AspectRatio = ( float )WindowBase.LogicalWidth / WindowBase.LogicalHeight;
            camera.NearPlane = -10f;
            camera.FarPlane = 10f;
            break;
        }
    }
}