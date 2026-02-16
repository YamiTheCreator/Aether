using System.Numerics;
using Aether.Core;
using Aether.Core.Structures;
using Aether.Core.Systems;
using Graphics;
using Graphics.Components;
using Graphics.Helpers;
using Graphics.Text;
using Graphics.Textures;
using Graphics.Windowing;
using Silk.NET.OpenGL;
using UI.Components;
using Shader = Graphics.Shaders.Shader;

namespace Hangman.Systems;

public class RenderSystem : SystemBase
{
    private Renderer2D _renderer = null!;
    private Shader _shader = null!;
    private Texture2D _buttonTexture2D = null!;
    private Texture2D[] _hangmanTextures = null!;
    private Texture2D _gameOverTexture = null!;
    private Font _font = null!;
    private HangmanState _state = null!;

    protected override void OnInit()
    {
        _renderer = World.GetGlobal<Renderer2D>();
        _shader = World.GetGlobal<Shader>();

        string solutionRoot =
            Path.GetFullPath( Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "../../../../../" ) );
        string buttonPath = Path.Combine( solutionRoot, "src/UI/Assets/button.jpeg" );
        _buttonTexture2D = new Texture2D( MainWindow.Gl, buttonPath );

        _hangmanTextures = new Texture2D[ 6 ];
        string[] hangmanFiles =
        [
            "hangman-first.png",
            "hangman-second.png",
            "hangman-third.png",
            "hangman-forth.png",
            "hangman-fifth.png",
            "hangman-sixth.png"
        ];

        for ( int i = 0; i < 6; i++ )
        {
            string hangmanPath = Path.Combine( solutionRoot, $"examples/Hangman/Assets/{hangmanFiles[ i ]}" );
            _hangmanTextures[ i ] = new Texture2D( MainWindow.Gl, hangmanPath,
                TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge,
                TextureMinFilter.Nearest, TextureMagFilter.Nearest, false );
        }

        string gameOverPath = Path.Combine( solutionRoot, "examples/Hangman/Assets/game-over.png" );
        _gameOverTexture = new Texture2D( MainWindow.Gl, gameOverPath,
            TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge,
            TextureMinFilter.Nearest, TextureMagFilter.Nearest, false );

        _state = World.GetGlobal<HangmanState>();
        _font = World.GetGlobal<Font>();
    }

    protected override void OnRender()
    {
        GL gl = MainWindow.Gl;
        gl.ClearColor( 0.1f, 0.1f, 0.15f, 1.0f );
        gl.Clear( ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit );

        gl.Enable( EnableCap.Blend );
        gl.BlendFunc( BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha );
        gl.Disable( EnableCap.DepthTest );

        Camera camera = default;

        foreach ( Entity e in World.Filter<Camera>().With<Transform>() )
        {
            World.Get<Transform>( e );
            camera = World.Get<Camera>( e );
            break;
        }

        Matrix4x4 viewProjection = camera.ViewProjectionMatrix;

        _renderer.Begin( viewProjection, _shader );
        if ( !_state.IsGameOver || _state.IsWon )
        {
            RenderHangman( _state.WrongGuesses );
        }

        RenderLetterButtons();
        _renderer.End();

        _renderer.Begin( viewProjection, _shader );
        RenderWord();
        RenderLetterButtonsText();
        RenderGameStatus();
        _renderer.End();
    }


    private void RenderHangman( int wrongGuesses )
    {
        int textureIndex = Math.Min( wrongGuesses, 5 );
        Texture2D hangmanTexture = _hangmanTextures[ textureIndex ];

        const float size = 6.0f;
        const float x = -size / 2f;
        const float y = -1.0f;

        Vector4 white = new( 1, 1, 1, 1 );
        RenderTexturedQuad( x, y, size, size, white, hangmanTexture );
    }

    private void RenderLetterButtons()
    {
        foreach ( Entity entity in World.Filter<Button>() )
        {
            ref Button button = ref World.Get<Button>( entity );

            if ( string.IsNullOrEmpty( button.Text ) || button.Text.Length == 0 )
                continue;

            LetterState state = _state.GetLetterState( button.Text[ 0 ] );

            Vector4 color = state switch
            {
                LetterState.Correct => new Vector4( 0.2f, 0.8f, 0.2f, 1.0f ),
                LetterState.Wrong => new Vector4( 0.8f, 0.2f, 0.2f, 1.0f ),
                _ => new Vector4( 1.0f, 1.0f, 1.0f, 1.0f )
            };

            RenderTexturedQuad( button.Position.X, button.Position.Y, button.Size.X, button.Size.Y, color,
                _buttonTexture2D );
        }
    }

    private void RenderWord()
    {
        string displayWord = _state.GetDisplayWord();
        Vector4 white = new( 1, 1, 1, 1 );

        RenderTextCentered( displayWord, new Vector3( 0, 8, 0 ), 0.025f, white );
    }

    private void RenderLetterButtonsText()
    {
        Vector4 black = new( 0f, 0f, 0f, 1f );

        foreach ( Entity entity in World.Filter<Button>() )
        {
            ref Button button = ref World.Get<Button>( entity );

            if ( string.IsNullOrEmpty( button.Text ) )
                continue;

            float textX = button.Position.X + button.Size.X / 2;
            float textY = button.Position.Y + button.Size.Y / 2;

            RenderTextCentered( button.Text, new Vector3( textX, textY, 0 ), 0.02f, black );
        }
    }

    private void RenderGameStatus()
    {
        if ( _state.IsGameOver )
        {
            const float size = 6.0f;
            const float x = -size / 2f;
            const float y = -1.0f;

            if ( _state.IsWon )
            {
                RenderTextCentered( "Win!", new Vector3( 0, y + size / 2f, 0 ), 0.04f, new Vector4( 0, 1, 0, 1 ) );
            }
            else
            {
                RenderTexturedQuad( x, y, size, size, Vector4.One, _gameOverTexture );
            }

            RenderTextCentered( "Press R to restart", new Vector3( 0, -8, 0 ), 0.015f,
                new Vector4( 0.7f, 0.7f, 0.7f, 1 ) );
        }
    }

    private void RenderTextCentered( string text, Vector3 position, float scale, Vector4 color )
    {
        TextRenderer.RenderTextAligned( _renderer, _font, text, position, scale, color, TextAlignment.Center );
    }

    private void RenderTexturedQuad( float x, float y, float width, float height, Vector4 color,
        Texture2D texture2D )
    {
        Vector3 pos = new( x, y, 0 );

        QuadVertex[] vertices =
        [
            new( pos, new Vector2( 0, 1 ), color ),
            new( pos + new Vector3( width, 0, 0 ), new Vector2( 1, 1 ), color ),
            new( pos + new Vector3( width, height, 0 ), new Vector2( 1, 0 ),
                color ),
            new( pos + new Vector3( 0, height, 0 ), new Vector2( 0, 0 ), color )
        ];

        _renderer.SubmitQuad( vertices, texture2D );
    }
}