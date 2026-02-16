using System.Numerics;
using Aether.Core;
using Aether.Core.Helpers;
using Aether.Core.Structures;
using Aether.Core.Systems;
using Graphics;
using Graphics.Components;
using Graphics.Helpers;
using Graphics.Systems;
using Graphics.Text;
using Graphics.Textures;
using Graphics.Windowing;
using Silk.NET.OpenGL;

namespace Tetris.Systems;

public class RenderSystem : SystemBase
{
    private Renderer2D _renderer = null!;
    private Graphics.Shaders.Shader _shader = null!;
    private Texture2D _blockTexture = null!;
    private Texture2D _inputTexture = null!;
    private Font _font = null!;
    private GameState _state = null!;

    private const float _blockSize = 1f;
    private const float _boardX = -6f;
    private const float _boardY = -10f;
    private const float _uix = 6f;
    private const float _uiy = 9f;
    private const float _margin = 2f;

    private const int _blockTextureSize = 16;
    private const int _blockBorderSize = 3;
    private const int _inputBorderSize = 1;

    protected override void OnInit()
    {
        _renderer = World.GetGlobal<Renderer2D>();
        _shader = World.GetGlobal<Graphics.Shaders.Shader>();
        _font = World.GetGlobal<Font>();
        _state = World.GetGlobal<GameState>();

        // Load textures
        string solutionRoot =
            Path.GetFullPath( Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "../../../../../" ) );
        string blockPath = Path.Combine( solutionRoot, "examples/Tetris/Assets/block.jpeg" );
        string inputPath = Path.Combine( solutionRoot, "examples/Tetris/Assets/input.jpeg" );

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

        // Get camera
        Camera camera = default;
        foreach ( Entity e in World.Filter<Camera>().With<Transform>() )
        {
            camera = World.Get<Camera>( e );
            break;
        }

        Matrix4x4 viewProjection = camera.ViewProjectionMatrix;

        _renderer.Begin( viewProjection, _shader );

        RenderWalls();

        RenderLockedBlocks();

        RenderCurrentPiece();

        RenderUi();

        _renderer.End();
    }

    private void RenderWalls()
    {
        Vector4 wallColor = new( 0.5f, 0.5f, 0.5f, 1f );

        // Left wall
        for ( int y = 0; y < GameState.Height; y++ )
        {
            RenderBlock( _boardX - _blockSize, _boardY + y * _blockSize, wallColor );
        }

        // Right wall
        for ( int y = 0; y < GameState.Height; y++ )
        {
            RenderBlock( _boardX + GameState.Width * _blockSize, _boardY + y * _blockSize, wallColor );
        }

        // Bottom wall
        for ( int x = -1; x <= GameState.Width; x++ )
        {
            RenderBlock( _boardX + x * _blockSize, _boardY - _blockSize, wallColor );
        }
    }

    private void RenderLockedBlocks()
    {
        for ( int x = 0; x < GameState.Width; x++ )
        {
            for ( int y = 0; y < GameState.Height; y++ )
            {
                int colorIndex = _state.Board[ x, y ];
                if ( colorIndex != 0 )
                {
                    Vector4 color = Tetromino.GetColor( colorIndex );
                    RenderBlock( _boardX + x * _blockSize, _boardY + y * _blockSize, color );
                }
            }
        }
    }

    private void RenderCurrentPiece()
    {
        if ( _state.CurrentType == TetrominoType.None )
            return;

        Vector2[] blocks = Tetromino.GetBlocks( _state.CurrentType, _state.CurrentRotation );
        Vector4 color = Tetromino.GetColor( Tetromino.GetColorIndex( _state.CurrentType ) );

        foreach ( Vector2 offset in blocks )
        {
            float x = _boardX + ( _state.CurrentPosition.X + offset.X ) * _blockSize;
            float y = _boardY + ( _state.CurrentPosition.Y + offset.Y ) * _blockSize;
            RenderBlock( x, y, color );
        }
    }

    private void RenderBlock( float x, float y, Vector4 color )
    {
        RenderNineSlice( new Vector2( x, y ), new Vector2( _blockSize, _blockSize ), color, _blockBorderSize,
            _blockTextureSize, _blockTexture );
    }

    private void RenderUi()
    {
        const float scale = 0.012f;
        Vector4 white = new( 1, 1, 1, 1 );
        Vector4 green = new( 0, 1, 0, 1 );

        const float boxWidth = 3f;
        const float boxHeight = 1.5f;

        float currentY = _uiy;
        RenderText( "LEVEL", _uix, currentY, scale, white );
        currentY -= _margin;
        RenderInputBox( _uix, currentY, boxWidth, boxHeight );
        RenderTextCentered( _state.Level.ToString(), _uix, currentY + 0.15f, boxWidth, boxHeight, scale, green );

        currentY -= boxHeight + 0.5f;
        RenderText( "LINES", _uix, currentY, scale, white );
        currentY -= _margin;
        RenderInputBox( _uix, currentY, boxWidth, boxHeight );
        int linesForNextLevel = ( _state.Level * 10 ) - _state.LinesCleared;
        RenderTextCentered( linesForNextLevel.ToString(), _uix, currentY + 0.15f, boxWidth, boxHeight, scale, green );

        currentY -= boxHeight + 0.5f;
        RenderText( "SCORE", _uix, currentY, scale, white );
        currentY -= _margin;
        RenderInputBox( _uix, currentY, boxWidth, boxHeight );
        RenderTextCentered( _state.Score.ToString(), _uix, currentY + 0.15f, boxWidth, boxHeight, scale, green );

        currentY -= boxHeight + 1.0f;
        RenderText( "NEXT", _uix, currentY, scale, white );
        currentY -= _margin;
        RenderPreviewBox( _uix, currentY - 1.5f, boxWidth + 1f, boxWidth );
        RenderNextPiece( _uix + boxWidth / 2f + 0.2f, currentY - 0.3f );

        if ( _state.IsGameOver )
        {
            RenderText( "GAME OVER", _boardX + 2f, _boardY + 10f, 0.02f, new Vector4( 1, 0, 0, 1 ) );
            RenderText( "Press R", _boardX + 2.5f, _boardY + 8f, 0.015f, white );
        }
    }

    private void RenderInputBox( float x, float y, float width, float height )
    {
        RenderNineSlice( new Vector2( x, y ), new Vector2( width, height ), Vector4.One, _inputBorderSize,
            _blockTextureSize, _inputTexture );
    }

    private void RenderPreviewBox( float x, float y, float width, float height )
    {
        Vector4 darkColor = new( 0.2f, 0.2f, 0.2f, 1f );
        RenderNineSlice( new Vector2( x, y ), new Vector2( width, height ), darkColor, _blockBorderSize,
            _blockTextureSize, _blockTexture );
    }

    private void RenderTextCentered( string text, float boxX, float boxY, float boxWidth, float boxHeight, float scale,
        Vector4 color )
    {
        TextRenderer.RenderTextCentered(_renderer, _font, text, boxX, boxY, boxWidth, boxHeight, scale, color);
    }

    private void RenderText( string text, float x, float y, float scale, Vector4 color )
    {
        TextRenderer.RenderText(_renderer, _font, text, x, y, scale, color);
    }

    private void RenderNineSlice( Vector2 position, Vector2 size, Vector4 color, int borderPixels, int textureSize,
        Texture2D texture )
    {
        SliceQuad[] slices = NineSliceHelper.CalculateSlices( position, size, borderPixels, textureSize );

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

    private void RenderNextPiece( float centerX, float centerY )
    {
        if ( _state.NextType == TetrominoType.None )
            return;

        Vector2[] blocks = Tetromino.GetBlocks( _state.NextType, 0 );
        Vector4 color = Tetromino.GetColor( Tetromino.GetColorIndex( _state.NextType ) );

        float minX = blocks.Min( b => b.X );
        float maxX = blocks.Max( b => b.X );
        float minY = blocks.Min( b => b.Y );
        float maxY = blocks.Max( b => b.Y );

        float pieceWidth = ( maxX - minX + 1 ) * _blockSize;
        float pieceHeight = ( maxY - minY + 1 ) * _blockSize;

        const float maxSize = 2.4f;
        float scale = Math.Min( maxSize / pieceWidth, maxSize / pieceHeight );
        scale = Math.Min( scale, 0.6f );

        float offsetX = ( minX + maxX ) / 2f;
        float offsetY = ( minY + maxY ) / 2f;

        foreach ( Vector2 offset in blocks )
        {
            float x = centerX + ( offset.X - offsetX ) * _blockSize * scale;
            float y = centerY + ( offset.Y - offsetY ) * _blockSize * scale;
            RenderBlock( x, y, color, scale );
        }
    }

    private void RenderBlock( float x, float y, Vector4 color, float scale )
    {
        float size = _blockSize * scale;
        RenderNineSlice( new Vector2( x, y ), new Vector2( size, size ), color, _blockBorderSize, _blockTextureSize,
            _blockTexture );
    }
}