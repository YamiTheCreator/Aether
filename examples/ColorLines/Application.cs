using System.Numerics;
using Aether.Core;
using Aether.Core.Enums;
using ColorLines.Systems;
using Graphics;
using Graphics.Components;
using Graphics.Shaders;
using Graphics.Systems;
using Graphics.Text;
using Graphics.Textures;
using Graphics.Windowing;

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
        MainWindow.SetResizable( false );

        Renderer2D renderer = new();
        Shader shader = new( MainWindow.Gl );
        Font font = new( MainWindow.Gl, fontSize: 32f );

        World.SetGlobal( renderer );
        World.SetGlobal( shader );
        World.SetGlobal( font );

        GameState gameState = new();
        gameState.GenerateNextBalls();
        World.SetGlobal( gameState );

        World.AddSystem( new CameraSystem() );
        World.AddSystem( new GameSystem() );
        World.AddSystem( new InputSystem() );
        World.AddSystem( new RenderSystem() );

        // Setup camera
        foreach ( Entity e in World.Filter<Camera>().With<Transform>() )
        {
            ref Camera camera = ref World.Get<Camera>( e );

            camera.ProjectionType = ProjectionType.Orthographic;
            camera.IsStatic = true;
            camera.StaticPosition = new Vector3( 0f, 0f, 0f );
            camera.OrthographicSize = 11f;
            camera.AspectRatio = ( float )MainWindow.Width / MainWindow.Height;
            camera.NearPlane = -10f;
            camera.FarPlane = 10f;
            break;
        }
    }
}