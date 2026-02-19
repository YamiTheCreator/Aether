using Silk.NET.Maths;
using Aether.Core;
using Aether.Core.Helpers;
using GameUtils.Helpers;
using Graphics;
using Graphics.Components;
using Graphics.Systems;
using Silk.NET.OpenGL;
using ColorLines.Components;
using Graphics.Structures;

namespace ColorLines.Systems;

public class ColorLinesRenderSystem : SystemBase
{
    private Renderer2D _renderer = null!;
    private Graphics.Components.Shader _shader;
    private Graphics.Components.Texture2D _ballTexture;
    private Graphics.Components.Texture2D _blockTexture;
    private Graphics.Components.Texture2D _inputTexture;
    private Graphics.Components.Font _font;
    private FontSystem _fontSystem = null!;
    private TextureSystem _textureSystem = null!;
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
        _shader = World.GetGlobal<Graphics.Components.Shader>();
        _font = World.GetGlobal<Graphics.Components.Font>();
        _fontSystem = World.GetGlobal<FontSystem>();
        _textureSystem = World.GetGlobal<TextureSystem>();

        string solutionRoot =
            Path.GetFullPath( Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "../../../../../" ) );
        string ballPath = Path.Combine( solutionRoot, "examples/ColorLines/Assets/ball.png" );
        string blockPath = Path.Combine( solutionRoot, "examples/ColorLines/Assets/block.jpeg" );
        string inputPath = Path.Combine( solutionRoot, "examples/ColorLines/Assets/input.jpeg" );

        _ballTexture = _textureSystem.CreateTextureFromFile( ballPath,
            TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge,
            TextureMinFilter.Linear );
        _blockTexture = _textureSystem.CreateTextureFromFile( blockPath,
            TextureWrapMode.Repeat, TextureWrapMode.Repeat,
            TextureMinFilter.Nearest, TextureMagFilter.Nearest );
        _inputTexture = _textureSystem.CreateTextureFromFile( inputPath,
            TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge,
            TextureMinFilter.Nearest, TextureMagFilter.Nearest );
    }

    protected override void OnRender()
    {
        GL gl = WindowBase.Gl;
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

        Matrix4X4<float> viewProjection = camera.ViewProjectionMatrix;
        _renderer.Begin( viewProjection, _shader );

        RenderBoard();
        RenderBalls();
        RenderUi();

        _renderer.End();
    }

    private void RenderBoard()
    {
        Vector4D<float> lightGray = new( 0.3f, 0.3f, 0.3f, 1f );
        Vector4D<float> darkGray = new( 0.2f, 0.2f, 0.2f, 1f );

        for ( int x = 0; x < GameConstants.BoardSize; x++ )
        {
            for ( int y = 0; y < GameConstants.BoardSize; y++ )
            {
                Vector4D<float> color = ( x + y ) % 2 == 0 ? lightGray : darkGray;
                float worldX = _boardStartX + x * _cellSize;
                float worldY = _boardStartY + y * _cellSize;

                RenderNineSlice( new Vector2D<float>( worldX, worldY ), new Vector2D<float>( _cellSize, _cellSize ),
                    color, _blockBorderSize, _blockTextureSize, _blockTexture );
            }
        }

        // Highlight selected cell
        foreach (Entity gameEntity in World.Filter<BoardComponent>().With<SelectedCellComponent>())
        {
            ref SelectedCellComponent selected = ref World.Get<SelectedCellComponent>(gameEntity);
            
            float worldX = _boardStartX + selected.Position.X * _cellSize;
            float worldY = _boardStartY + selected.Position.Y * _cellSize;

            Vector4D<float> highlightColor = new( 1f, 1f, 0f, 0.5f );
            RenderNineSlice( new Vector2D<float>( worldX, worldY ), new Vector2D<float>( _cellSize, _cellSize ),
                highlightColor, _blockBorderSize, _blockTextureSize, _blockTexture );
            
            break;
        }
    }

    private void RenderBalls()
    {
        foreach (Entity gameEntity in World.Filter<BoardComponent>())
        {
            ref BoardComponent board = ref World.Get<BoardComponent>(gameEntity);
            
            // Получаем выбранную ячейку если есть
            Vector2D<int>? selectedPos = null;
            if (World.Has<SelectedCellComponent>(gameEntity))
            {
                ref SelectedCellComponent selected = ref World.Get<SelectedCellComponent>(gameEntity);
                selectedPos = selected.Position;
            }
            
            // Проверяем анимацию
            bool isAnimating = World.Has<MovementPathComponent>(gameEntity);
            
            for ( int x = 0; x < GameConstants.BoardSize; x++ )
            {
                for ( int y = 0; y < GameConstants.BoardSize; y++ )
                {
                    int colorIndex = board.Cells[ x, y ];
                    if ( colorIndex == 0 )
                        continue;

                    // Skip the ball being moved during animation
                    if ( isAnimating && selectedPos != null &&
                         selectedPos.Value.X == x && selectedPos.Value.Y == y )
                        continue;

                    float worldX = _boardStartX + x * _cellSize + _cellSize / 2f;
                    float worldY = _boardStartY + y * _cellSize + _cellSize / 2f;

                    RenderBall( worldX, worldY, colorIndex );
                }
            }

            // Render moving ball
            if (World.Has<MovementPathComponent>(gameEntity) && 
                World.Has<MovementTimerComponent>(gameEntity) &&
                selectedPos != null)
            {
                ref MovementPathComponent movement = ref World.Get<MovementPathComponent>(gameEntity);
                ref MovementTimerComponent timer = ref World.Get<MovementTimerComponent>(gameEntity);
                
                if (movement.CurrentIndex < movement.Path.Count)
                {
                    Vector2D<int> currentCell = movement.Path[ movement.CurrentIndex ];
                    Vector2D<int> nextCell = movement.CurrentIndex + 1 < movement.Path.Count
                        ? movement.Path[ movement.CurrentIndex + 1 ]
                        : currentCell;

                    float t = timer.Time / GameConstants.MoveSpeed;
                    Vector2D<float> interpolated = new(
                        currentCell.X + (nextCell.X - currentCell.X) * t,
                        currentCell.Y + (nextCell.Y - currentCell.Y) * t
                    );

                    float worldX = _boardStartX + interpolated.X * _cellSize + _cellSize / 2f;
                    float worldY = _boardStartY + interpolated.Y * _cellSize + _cellSize / 2f;

                    // Get color from the original selected cell
                    int colorIndex = board.Cells[ selectedPos.Value.X, selectedPos.Value.Y ];

                    if ( colorIndex != 0 )
                    {
                        RenderBall( worldX, worldY, colorIndex );
                    }
                }
            }
            
            break;
        }
    }

    private void RenderBall( float centerX, float centerY, int colorIndex )
    {
        Vector4D<float> color = ColorPalette.GetColor( ColorPalette.ColorLines, colorIndex - 1 );

        Vector3D<float> pos = new( centerX - _ballSize / 2f, centerY - _ballSize / 2f, 0 );

        Vertex[] vertices =
        [
            new( pos, new Vector2D<float>( 0, 0 ), color ),
            new( pos + new Vector3D<float>( _ballSize, 0, 0 ), new Vector2D<float>( 1, 0 ), color ),
            new( pos + new Vector3D<float>( _ballSize, _ballSize, 0 ), new Vector2D<float>( 1, 1 ), color ),
            new( pos + new Vector3D<float>( 0, _ballSize, 0 ), new Vector2D<float>( 0, 1 ), color )
        ];

        _renderer.SubmitQuad( vertices, _ballTexture );
    }

    private void RenderUi()
    {
        const float scale = 0.012f;
        Vector4D<float> white = new( 1, 1, 1, 1 );
        Vector4D<float> green = new( 0, 1, 0, 1 );

        const float uiX = 8f;
        const float uiY = 4f;

        // Получаем данные из компонентов
        int score = 0;
        int[] nextBalls = new int[GameConstants.BallsPerTurn];
        bool isGameOver = false;

        foreach (Entity gameEntity in World.Filter<BoardComponent>())
        {
            if (World.Has<ScoreComponent>(gameEntity))
            {
                ref ScoreComponent scoreComp = ref World.Get<ScoreComponent>(gameEntity);
                score = scoreComp.Value;
            }

            if (World.Has<NextBallsComponent>(gameEntity))
            {
                ref NextBallsComponent nextBallsComp = ref World.Get<NextBallsComponent>(gameEntity);
                nextBalls = nextBallsComp.Colors;
            }

            isGameOver = World.Has<GameOverComponent>(gameEntity);
            break;
        }

        RenderText( "SCORE", uiX, uiY, scale, white );
        const float inputY = uiY - _margin;
        RenderInputBox( uiX, inputY - 1.5f, 3f, 2f );
        RenderTextCentered( score.ToString(), uiX, inputY - 0.7f, 3f, 0.7f, scale, green );

        float nextY = uiY - _cellSize - 2f;
        RenderText( "NEXT", uiX, nextY, scale, white );
        nextY -= _margin;

        // Show next 3 balls
        for ( int i = 0; i < GameConstants.BallsPerTurn; i++ )
        {
            float x = uiX + i * 2f;

            RenderNineSlice( new Vector2D<float>( x, nextY - 1.2f ), new Vector2D<float>( _cellSize, _cellSize ),
                new Vector4D<float>( 0.2f, 0.2f, 0.2f, 1f ), _blockBorderSize, _blockTextureSize, _blockTexture );

            RenderBall( x + _cellSize / 2, nextY - 0.3f, nextBalls[ i ] );
        }

        if ( isGameOver )
        {
            RenderText( "GAME OVER", -2f, 0f, 0.02f, new Vector4D<float>( 1, 0, 0, 1 ) );
            RenderText( "Press R", -1.5f, -1f, 0.015f, white );
        }
    }

    private void RenderInputBox( float x, float y, float width, float height )
    {
        RenderNineSlice( new Vector2D<float>( x, y ), new Vector2D<float>( width, height ), Vector4D<float>.One,
            _inputBorderSize, _blockTextureSize, _inputTexture );
    }

    private void RenderNineSlice( Vector2D<float> position, Vector2D<float> size, Vector4D<float> color, int borderPixels, int textureSize,
        Graphics.Components.Texture2D texture )
    {
        Slice[] slices =
            NineSliceHelper.CalculateSlices( position, size, borderPixels, textureSize );

        foreach ( Slice slice in slices )
        {
            Vector3D<float> pos0 = new( slice.PositionMin.X, slice.PositionMin.Y, 0 );
            Vector3D<float> pos1 = new( slice.PositionMax.X, slice.PositionMin.Y, 0 );
            Vector3D<float> pos2 = new( slice.PositionMax.X, slice.PositionMax.Y, 0 );
            Vector3D<float> pos3 = new( slice.PositionMin.X, slice.PositionMax.Y, 0 );

            Vertex[] vertices =
            [
                new( pos0, new Vector2D<float>( slice.UvMin.X, slice.UvMin.Y ), color ),
                new( pos1, new Vector2D<float>( slice.UvMax.X, slice.UvMin.Y ), color ),
                new( pos2, new Vector2D<float>( slice.UvMax.X, slice.UvMax.Y ), color ),
                new( pos3, new Vector2D<float>( slice.UvMin.X, slice.UvMax.Y ), color )
            ];

            _renderer.SubmitQuad( vertices, texture );
        }
    }

    private void RenderTextCentered(string text, float boxX, float boxY, float boxWidth, float boxHeight, float scale,
        Vector4D<float> color)
    {
        (float textWidth, float textHeight) = _fontSystem.MeasureText(ref _font, text);
        textWidth *= scale;
        textHeight *= scale;

        float x = boxX + (boxWidth - textWidth) / 2f;
        float y = boxY + (boxHeight - textHeight) / 2f;

        RenderText(text, x, y, scale, color);
    }

    private void RenderText(string text, float x, float y, float scale, Vector4D<float> color)
    {
        Vector3D<float> cursor = new(x, y, 0);

        foreach (char c in text)
        {
            if (c == '\n')
            {
                cursor.X = x;
                cursor.Y -= _font.LineHeight * scale;
                continue;
            }

            if (c == ' ')
            {
                (Glyph spaceGlyph, _) = _fontSystem.GetGlyph(ref _font, ' ');
                cursor.X += spaceGlyph.Advance * scale;
                continue;
            }

            (Glyph glyph, uint textureHandle) = _fontSystem.GetGlyph(ref _font, c);
            
            Texture2D glyphTexture = new Graphics.Components.Texture2D
            {
                Handle = textureHandle,
                Width = (int)glyph.Size.X,
                Height = (int)glyph.Size.Y
            };

            Vector3D<float> glyphPos = new(
                cursor.X,
                cursor.Y - glyph.Size.Y * scale / 2f,
                cursor.Z
            );

            Vector2D<float> size = glyph.Size * scale;
            Span<Vertex> vertices =
            [
                new(glyphPos, glyph.UvMin, color),
                new(glyphPos + new Vector3D<float>(size.X, 0, 0), new Vector2D<float>(glyph.UvMax.X, glyph.UvMin.Y), color),
                new(glyphPos + new Vector3D<float>(size.X, size.Y, 0), glyph.UvMax, color),
                new(glyphPos + new Vector3D<float>(0, size.Y, 0), new Vector2D<float>(glyph.UvMin.X, glyph.UvMax.Y), color)
            ];

            _renderer.SubmitQuad(vertices, glyphTexture);
            cursor.X += glyph.Advance * scale;
        }
    }
}
