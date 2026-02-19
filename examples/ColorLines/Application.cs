using Silk.NET.Maths;
using Aether.Core;
using Aether.Core.Enums;
using ColorLines.Systems;
using Graphics;
using Graphics.Components;
using Graphics.Systems;

namespace ColorLines;

public class Application() : ApplicationBase(
    title: "Color Lines",
    width: 1280,
    height: 920,
    fullScreen: true,
    createDefaultCamera: true )
{
    protected override void OnInitialize()
    {
        WindowBase.SetResizable( false );

        Renderer2D renderer = new();
        ShaderSystem shaderSystem = new( WindowBase.Gl );
        FontSystem fontSystem = new( WindowBase.Gl );
        TextureSystem textureSystem = new( WindowBase.Gl );
        
        Shader shader = shaderSystem.CreateShader();
        Font font = fontSystem.CreateFont( fontSize: 32f );

        World.SetGlobal( renderer );
        World.SetGlobal( shaderSystem );
        World.SetGlobal( fontSystem );
        World.SetGlobal( textureSystem );
        World.SetGlobal( shader );
        World.SetGlobal( font );

        // Добавляем системы в правильном порядке
        World.AddSystem( new CameraSystem() );
        World.AddSystem( new BoardInitSystem() );
        World.AddSystem( new BallSelectionSystem() );
        World.AddSystem( new BallMovementSystem() );
        World.AddSystem( new BallAnimationSystem() );
        World.AddSystem( new LineCheckSystem() );
        World.AddSystem( new BallSpawnSystem() );
        World.AddSystem( new GameRestartSystem() );
        World.AddSystem( new ColorLinesRenderSystem() );

        // Setup camera
        foreach ( Entity e in World.Filter<Camera>().With<Transform>() )
        {
            ref Camera camera = ref World.Get<Camera>( e );

            camera.ProjectionType = ProjectionType.Orthographic;
            camera.IsStatic = true;
            camera.StaticPosition = new Vector3D<float>( 0f, 0f, 0f );
            camera.OrthographicSize = 11f;
            camera.AspectRatio = ( float )WindowBase.LogicalWidth / WindowBase.LogicalHeight;
            camera.NearPlane = -10f;
            camera.FarPlane = 10f;
            break;
        }
    }
}