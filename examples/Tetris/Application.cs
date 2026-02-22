using Silk.NET.Maths;
using Aether.Core;
using Aether.Core.Enums;
using Graphics;
using Graphics.Components;
using Graphics.Systems;
using Graphics.Structures;
using Tetris.Systems;

namespace Tetris;

public class Application() : ApplicationBase(
    title: "Tetris",
    width: 800,
    height: 660,
    createDefaultCamera: true )
{
    protected override void OnInitialize()
    {
        WindowBase.SetResizable( false );

        // Set clear color for Tetris
        WindowBase.Gl.ClearColor( 0.1f, 0.1f, 0.15f, 1.0f );
        ShaderSystem shaderSystem = new( WindowBase.Gl );
        FontSystem fontSystem = new( WindowBase.Gl );
        TextureSystem textureSystem = new( WindowBase.Gl );
        MaterialSystem materialSystem = new();

        // Create default shader (no paths needed!)
        ShaderProgram shaderProgram = new( WindowBase.Gl );

        Shader shader = new()
        {
            Program = shaderProgram
        };

        Font font = fontSystem.CreateFont( fontSize: 32f );
        
        // Create default white texture for 2D rendering
        Texture2D whiteTexture = textureSystem.CreateTextureFromColor( 1, 1 );

        World.SetGlobal( shaderSystem );
        World.SetGlobal( fontSystem );
        World.SetGlobal( textureSystem );
        World.SetGlobal( shader );
        World.SetGlobal( font );
        World.SetGlobal( whiteTexture );

        // Add systems (new API - RenderSystem instead of custom render system)
        World.AddSystem( shaderSystem );
        World.AddSystem( new CameraSystem() );
        World.AddSystem( new LightingSystem() );
        World.AddSystem( materialSystem );
        World.AddSystem( new TextRenderSystem() );
        World.AddSystem( new TetrisLogicSystem() );
        World.AddSystem( new TetrisInputSystem() );
        World.AddSystem( new TetrisRenderSystem() ); // Creates entities in OnUpdate
        World.AddSystem( new RenderSystem( WindowBase.Gl ) ); // Renders entities in OnRender

        // Setup camera
        foreach ( Entity e in World.Filter<Camera>().With<Transform>() )
        {
            ref Camera camera = ref World.Get<Camera>( e );

            camera.ProjectionType = ProjectionType.Orthographic;
            camera.IsStatic = true;
            camera.StaticPosition = new Vector3D<float>( 0f, 0f, 0f );
            camera.OrthographicSize = 25f; // Increased from 10 to show more of the game area
            camera.AspectRatio = ( float )WindowBase.LogicalWidth / WindowBase.LogicalHeight;
            camera.NearPlane = -10f;
            camera.FarPlane = 10f;
            break;
        }
    }
}