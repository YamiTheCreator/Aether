using System.Numerics;
using Aether.Core;
using Aether.Core.Enums;
using Graphics;
using Graphics.Components;
using Graphics.Shaders;
using Graphics.Systems;
using Graphics.Text;
using Graphics.Textures;
using Graphics.Windowing;
using Hangman.Systems;
using UI.Components;

namespace Hangman;

public class Application() : ApplicationBase( title: "Hangman",
    width: 600,
    height: 700,
    createDefaultCamera: true )
{
    protected override void OnInitialize()
    {
        Renderer2D renderer = new();
        Shader shader = new( MainWindow.Gl );
        Texture2D whiteTexture = new( MainWindow.Gl, 1, 1 );
        Font font = new( MainWindow.Gl, fontSize: 48f );

        World.SetGlobal( renderer );
        World.SetGlobal( shader );
        World.SetGlobal( whiteTexture );
        World.SetGlobal( font );

        HangmanState state = new();
        World.SetGlobal( state );

        World.AddSystem( new CameraSystem() );
        World.AddSystem( new GameSystem() );
        World.AddSystem( new InputSystem() );
        World.AddSystem( new RenderSystem() );

        CreateAlphabetButtons();

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

        MainWindow.SetResizable( false );
    }

    private void CreateAlphabetButtons()
    {
        char[] alphabet = HangmanState.UsAlphabet;

        const int lettersPerRow = 11;
        const float buttonWidth = 1.2f;
        const float buttonHeight = 1.2f;
        const float spacing = 0.2f;
        const float startY = -6f;

        for ( int i = 0; i < alphabet.Length; i++ )
        {
            int row = i / lettersPerRow;
            int col = i % lettersPerRow;

            const float totalWidth = lettersPerRow * ( buttonWidth + spacing ) - spacing;
            const float startX = -totalWidth / 2;

            float x = startX + col * ( buttonWidth + spacing );
            float y = startY - row * ( buttonHeight + spacing );

            Entity button = World.Spawn();
            World.Add( button, new Button(
                alphabet[ i ].ToString(),
                new Vector2( x, y ),
                new Vector2( buttonWidth, buttonHeight ),
                new Vector4( 1, 1, 1, 1 )
            ) );
        }
    }
}