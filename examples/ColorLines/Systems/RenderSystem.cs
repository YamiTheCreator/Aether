using System.Numerics;
using Aether.Core;
using Aether.Core.Helpers;
using Aether.Core.Structures;
using Aether.Core.Systems;
using GameUtils.Helpers;
using Graphics;
using Graphics.Components;
using Graphics.Helpers;
using Graphics.Text;
using Graphics.Textures;
using Graphics.Windowing;
using Silk.NET.OpenGL;

namespace ColorLines.Systems;

public class RenderSystem : SystemBase
{
    private Renderer2D _renderer = null!;
    private Graphics.Shaders.Shader _shader = null!;
    private Texture2D _ballTexture = null!;
    private Texture2D _blockTexture = null!;
    private Texture2D _inputTexture = null!;
    private Font _font = null!;
    private GameState _state = null!;
    private const float _ballSize = 1f;
    private const float _margin = 1f;

    private const float _cellSize = 1.8f;
    private const float _boardStartX = -9f;
    private const float _boardStartY = -9f;
    private const int _blockTextureSize = 16;
    private const int _blockBorderSize = 3;
    private const int _inputBorderSize = 1;

    protected override void OnInit()
    {
        _renderer = World.GetGlobal<Renderer2D>();
        _shader = World.GetGlobal<Graphics.Shaders.Shader>();
        _font = World.GetGlobal<Font>();
        _state = World.GetGlobal<GameState>();

        string solutionRoot =
            Path.GetFullPath( Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "../../../../../" ) );
        string ballPath = Path.Combine( solutionRoot, "examples/ColorLines/Assets/ball.png" );
        string blockPath = Path.Combine( solutionRoot, "examples/ColorLines/Assets/block.jpeg" );
        string inputPath = Path.Combine( solutionRoot, "examples/ColorLines/Assets/input.jpeg" );

        _ballTexture = new Texture2D( MainWindow.Gl, ballPath,
            TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge,
            TextureMinFilter.Linear );
        _blockTexture = new Texture2D( MainWindow.Gl, blockPath,
            TextureWrapMode.Repeat, TextureWrapMode.Repeat,
            TextureMinFilter.Nearest, TextureMagFilter.Nearest );
        _inputTexture = new Texture2D( MainWindow.Gl, inputPath,
            TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge,
            TextureMinFilter.Nearest, TextureMagFilter.Nearest );
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
            camera = World.Get<Camera>( e );
            break;
        }

        Matrix4x4 viewProjection = camera.ViewProjectionMatrix;
        _renderer.Begin( viewProjection, _shader );

        RenderBoard();
        RenderBalls();
        RenderUi();

        _renderer.End();
    }

    private void RenderBoard()
    {
        Vector4 lightGray = new( 0.3f, 0.3f, 0.3f, 1f );
        Vector4 darkGray = new( 0.2f, 0.2f, 0.2f, 1f );

        for ( int x = 0; x < GameState.BoardSize; x++ )
        {
            for ( int y = 0; y < GameState.BoardSize; y++ )
            {
                Vector4 color = ( x + y ) % 2 == 0 ? lightGray : darkGray;
                float worldX = _boardStartX + x * _cellSize;
                float worldY = _boardStartY + y * _cellSize;

                RenderNineSlice( new Vector2( worldX, worldY ), new Vector2( _cellSize, _cellSize ),
                    color, _blockBorderSize, _blockTextureSize, _blockTexture );
            }
        }

        // Highlight selected cell
        if ( _state.SelectedCell != null )
        {
            Vector2 cell = _state.SelectedCell.Value;
            float worldX = _boardStartX + cell.X * _cellSize;
            float worldY = _boardStartY + cell.Y * _cellSize;

            Vector4 highlightColor = new( 1f, 1f, 0f, 0.5f );
            RenderNineSlice( new Vector2( worldX, worldY ), new Vector2( _cellSize, _cellSize ),
                highlightColor, _blockBorderSize, _blockTextureSize, _blockTexture );
        }
    }

    private void RenderBalls()
    {
        for ( int x = 0; x < GameState.BoardSize; x++ )
        {
            for ( int y = 0; y < GameState.BoardSize; y++ )
            {
                int colorIndex = _state.Board[ x, y ];
                if ( colorIndex == 0 )
                    continue;

                // Skip the ball being moved during animation
                if ( _state is { SelectedCell: not null, IsAnimating: true } &&
                     ( int )_state.SelectedCell.Value.X == x && ( int )_state.SelectedCell.Value.Y == y )
                    continue;

                float worldX = _boardStartX + x * _cellSize + _cellSize / 2f;
                float worldY = _boardStartY + y * _cellSize + _cellSize / 2f;

                RenderBall( worldX, worldY, colorIndex );
            }
        }

        // Render moving ball
        if ( _state is { IsAnimating: true, SelectedCell: not null } && _state.PathIndex < _state.MovementPath.Count )
        {
            Vector2 currentCell = _state.MovementPath[ _state.PathIndex ];
            Vector2 nextCell = _state.PathIndex + 1 < _state.MovementPath.Count
                ? _state.MovementPath[ _state.PathIndex + 1 ]
                : currentCell;

            float t = _state.MoveTimer / GameState.MoveSpeed;
            Vector2 interpolated = Vector2.Lerp( currentCell, nextCell, t );

            float worldX = _boardStartX + interpolated.X * _cellSize + _cellSize / 2f;
            float worldY = _boardStartY + interpolated.Y * _cellSize + _cellSize / 2f;

            // Get color from the original selected cell
            int colorIndex = _state.Board[ ( int )_state.SelectedCell.Value.X, ( int )_state.SelectedCell.Value.Y ];

            if ( colorIndex != 0 )
            {
                RenderBall( worldX, worldY, colorIndex );
            }
        }
    }

    private void RenderBall( float centerX, float centerY, int colorIndex )
    {
        Vector4 color = ColorPalette.GetColor( ColorPalette.ColorLines, colorIndex - 1 );

        Vector3 pos = new( centerX - _ballSize / 2f, centerY - _ballSize / 2f, 0 );

        QuadVertex[] vertices =
        [
            new( pos, new Vector2( 0, 0 ), color ),
            new( pos + new Vector3( _ballSize, 0, 0 ), new Vector2( 1, 0 ), color ),
            new( pos + new Vector3( _ballSize, _ballSize, 0 ), new Vector2( 1, 1 ), color ),
            new( pos + new Vector3( 0, _ballSize, 0 ), new Vector2( 0, 1 ), color )
        ];

        _renderer.SubmitQuad( vertices, _ballTexture );
    }

    private void RenderUi()
    {
        const float scale = 0.012f;
        Vector4 white = new( 1, 1, 1, 1 );
        Vector4 green = new( 0, 1, 0, 1 );

        const float uiX = 8f;
        const float uiY = 4f;

        RenderText( "SCORE", uiX, uiY, scale, white );
        const float inputY = uiY - _margin;
        RenderInputBox( uiX, inputY - 1.5f, 3f, 2f );
        RenderTextCentered( _state.Score.ToString(), uiX, inputY - 0.7f, 3f, 0.7f, scale, green );

        float nextY = uiY - _cellSize - 2f;
        RenderText( "NEXT", uiX, nextY, scale, white );
        nextY -= _margin;

        // Show next 3 balls
        for ( int i = 0; i < GameState.BallsPerTurn; i++ )
        {
            float x = uiX + i * 2f;

            RenderNineSlice( new Vector2( x, nextY - 1.2f ), new Vector2( _cellSize, _cellSize ),
                new Vector4( 0.2f, 0.2f, 0.2f, 1f ), _blockBorderSize, _blockTextureSize, _blockTexture );

            RenderBall( x + _cellSize / 2, nextY - 0.3f, _state.NextBalls[ i ] );
        }

        if ( _state.IsGameOver )
        {
            RenderText( "GAME OVER", -2f, 0f, 0.02f, new Vector4( 1, 0, 0, 1 ) );
            RenderText( "Press R", -1.5f, -1f, 0.015f, white );
        }
    }

    private void RenderInputBox( float x, float y, float width, float height )
    {
        RenderNineSlice( new Vector2( x, y ), new Vector2( width, height ), Vector4.One,
            _inputBorderSize, _blockTextureSize, _inputTexture );
    }

    private void RenderNineSlice( Vector2 position, Vector2 size, Vector4 color, int borderPixels, int textureSize,
        Texture2D texture )
    {
        SliceQuad[] slices =
            NineSliceHelper.CalculateSlices( position, size, borderPixels, textureSize );

        foreach ( SliceQuad slice in slices )
        {
            Vector3 pos0 = new( slice.PositionMin.X, slice.PositionMin.Y, 0 );
            Vector3 pos1 = new( slice.PositionMax.X, slice.PositionMin.Y, 0 );
            Vector3 pos2 = new( slice.PositionMax.X, slice.PositionMax.Y, 0 );
            Vector3 pos3 = new( slice.PositionMin.X, slice.PositionMax.Y, 0 );

            QuadVertex[] vertices =
            [
                new( pos0, new Vector2( slice.UvMin.X, slice.UvMin.Y ), color ),
                new( pos1, new Vector2( slice.UvMax.X, slice.UvMin.Y ), color ),
                new( pos2, new Vector2( slice.UvMax.X, slice.UvMax.Y ), color ),
                new( pos3, new Vector2( slice.UvMin.X, slice.UvMax.Y ), color )
            ];

            _renderer.SubmitQuad( vertices, texture );
        }
    }

    private void RenderTextCentered(string text, float boxX, float boxY, float boxWidth, float boxHeight, float scale,
        Vector4 color)
    {
        TextRenderer.RenderTextCentered(_renderer, _font, text, boxX, boxY, boxWidth, boxHeight, scale, color);
    }

    private void RenderText(string text, float x, float y, float scale, Vector4 color)
    {
        TextRenderer.RenderText(_renderer, _font, text, x, y, scale, color);
    }
}