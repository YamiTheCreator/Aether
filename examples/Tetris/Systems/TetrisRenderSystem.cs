using Silk.NET.Maths;
using Aether.Core;
using Aether.Core.Helpers;
using Graphics;
using Graphics.Components;
using Graphics.Structures;
using Graphics.Systems;
using Silk.NET.OpenGL;
using Tetris.Components;

namespace Tetris.Systems;

public class TetrisRenderSystem : SystemBase
{
    private Renderer2D _renderer = null!;
    private Graphics.Components.Shader _shader;
    private Graphics.Components.Texture2D _blockTexture;
    private Graphics.Components.Texture2D _inputTexture;
    private Graphics.Components.Font _font;
    private FontSystem _fontSystem = null!;
    private TextureSystem _textureSystem = null!;

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
        _shader = World.GetGlobal<Graphics.Components.Shader>();
        _font = World.GetGlobal<Graphics.Components.Font>();
        _fontSystem = World.GetGlobal<FontSystem>();
        _textureSystem = World.GetGlobal<TextureSystem>();

        // Load textures
        string solutionRoot =
            Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../../"));
        string blockPath = Path.Combine(solutionRoot, "examples/Tetris/Assets/block.jpeg");
        string inputPath = Path.Combine(solutionRoot, "examples/Tetris/Assets/input.jpeg");

        _blockTexture = _textureSystem.CreateTextureFromFile(blockPath,
            TextureWrapMode.Repeat, TextureWrapMode.Repeat,
            TextureMinFilter.Nearest, TextureMagFilter.Nearest);
        _inputTexture = _textureSystem.CreateTextureFromFile(inputPath,
            TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge,
            TextureMinFilter.Nearest, TextureMagFilter.Nearest);
    }

    protected override void OnRender()
    {
        GL gl = WindowBase.Gl;
        gl.ClearColor(0.1f, 0.1f, 0.15f, 1.0f);
        gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        gl.Enable(EnableCap.Blend);
        gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        gl.Disable(EnableCap.DepthTest);

        // Get camera
        Camera camera = default;
        foreach (Entity e in World.Filter<Camera>().With<Transform>())
        {
            camera = World.Get<Camera>(e);
            break;
        }

        Matrix4X4<float> viewProjection = camera.ViewProjectionMatrix;

        _renderer.Begin(viewProjection, _shader);

        RenderWalls();
        RenderLockedBlocks();
        RenderCurrentPiece();
        RenderUi();

        _renderer.End();
    }

    private void RenderWalls()
    {
        Vector4D<float> wallColor = new(0.5f, 0.5f, 0.5f, 1f);

        // Left wall
        for (int y = 0; y < 20; y++)
        {
            RenderBlock(_boardX - _blockSize, _boardY + y * _blockSize, wallColor);
        }

        // Right wall
        for (int y = 0; y < 20; y++)
        {
            RenderBlock(_boardX + 10 * _blockSize, _boardY + y * _blockSize, wallColor);
        }

        // Bottom wall
        for (int x = -1; x <= 10; x++)
        {
            RenderBlock(_boardX + x * _blockSize, _boardY - _blockSize, wallColor);
        }
    }

    private void RenderLockedBlocks()
    {
        foreach (Entity entity in World.Filter<TetrisBoardComponent>())
        {
            ref TetrisBoardComponent board = ref World.Get<TetrisBoardComponent>(entity);

            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 20; y++)
                {
                    int colorIndex = board.Board[x, y];
                    if (colorIndex != 0)
                    {
                        Vector4D<float> color = Tetromino.GetColor(colorIndex);
                        RenderBlock(_boardX + x * _blockSize, _boardY + y * _blockSize, color);
                    }
                }
            }
        }
    }

    private void RenderCurrentPiece()
    {
        foreach (Entity entity in World.Filter<TetrisPieceComponent>())
        {
            ref TetrisPieceComponent piece = ref World.Get<TetrisPieceComponent>(entity);

            if (piece.Type == TetrominoType.None)
                continue;

            Vector2D<float>[] blocks = Tetromino.GetBlocks(piece.Type, piece.Rotation);
            Vector4D<float> color = Tetromino.GetColor(Tetromino.GetColorIndex(piece.Type));

            foreach (Vector2D<float> offset in blocks)
            {
                float x = _boardX + (piece.Position.X + offset.X) * _blockSize;
                float y = _boardY + (piece.Position.Y + offset.Y) * _blockSize;
                RenderBlock(x, y, color);
            }
        }
    }

    private void RenderBlock(float x, float y, Vector4D<float> color)
    {
        RenderNineSlice(new Vector2D<float>(x, y), new Vector2D<float>(_blockSize, _blockSize), color, _blockBorderSize,
            _blockTextureSize, _blockTexture);
    }

    private void RenderUi()
    {
        const float scale = 0.012f;
        Vector4D<float> white = new(1, 1, 1, 1);
        Vector4D<float> green = new(0, 1, 0, 1);

        const float boxWidth = 3f;
        const float boxHeight = 1.5f;

        // Get game state
        int level = 1;
        int linesForNextLevel = 10;
        int score = 0;
        TetrominoType nextType = TetrominoType.None;
        bool isGameOver = false;

        foreach (Entity entity in World.Filter<TetrisGameStateComponent>())
        {
            ref TetrisGameStateComponent state = ref World.Get<TetrisGameStateComponent>(entity);
            level = state.Level;
            linesForNextLevel = (state.Level * 10) - state.LinesCleared;
            score = state.Score;
            nextType = state.NextType;
            isGameOver = state.IsGameOver;
        }

        float currentY = _uiy;
        RenderText("LEVEL", _uix, currentY, scale, white);
        currentY -= _margin;
        RenderInputBox(_uix, currentY, boxWidth, boxHeight);
        RenderTextCentered(level.ToString(), _uix, currentY + 0.15f, boxWidth, boxHeight, scale, green);

        currentY -= boxHeight + 0.5f;
        RenderText("LINES", _uix, currentY, scale, white);
        currentY -= _margin;
        RenderInputBox(_uix, currentY, boxWidth, boxHeight);
        RenderTextCentered(linesForNextLevel.ToString(), _uix, currentY + 0.15f, boxWidth, boxHeight, scale, green);

        currentY -= boxHeight + 0.5f;
        RenderText("SCORE", _uix, currentY, scale, white);
        currentY -= _margin;
        RenderInputBox(_uix, currentY, boxWidth, boxHeight);
        RenderTextCentered(score.ToString(), _uix, currentY + 0.15f, boxWidth, boxHeight, scale, green);

        currentY -= boxHeight + 1.0f;
        RenderText("NEXT", _uix, currentY, scale, white);
        currentY -= _margin;
        RenderPreviewBox(_uix, currentY - 1.5f, boxWidth + 1f, boxWidth);
        RenderNextPiece(_uix + boxWidth / 2f + 0.2f, currentY - 0.3f, nextType);

        if (isGameOver)
        {
            RenderText("GAME OVER", _boardX + 2f, _boardY + 10f, 0.02f, new Vector4D<float>(1, 0, 0, 1));
            RenderText("Press R", _boardX + 2.5f, _boardY + 8f, 0.015f, white);
        }
    }

    private void RenderInputBox(float x, float y, float width, float height)
    {
        RenderNineSlice(new Vector2D<float>(x, y), new Vector2D<float>(width, height), Vector4D<float>.One, _inputBorderSize,
            _blockTextureSize, _inputTexture);
    }

    private void RenderPreviewBox(float x, float y, float width, float height)
    {
        Vector4D<float> darkColor = new(0.2f, 0.2f, 0.2f, 1f);
        RenderNineSlice(new Vector2D<float>(x, y), new Vector2D<float>(width, height), darkColor, _blockBorderSize,
            _blockTextureSize, _blockTexture);
    }

    private void RenderTextCentered(string text, float boxX, float boxY, float boxWidth, float boxHeight, float scale,
        Vector4D<float> color)
    {
        // Calculate text width
        float textWidth = 0f;
        foreach (char c in text)
        {
            if (c == ' ')
            {
                (Glyph spaceGlyph, _) = _fontSystem.GetGlyph(ref _font, ' ');
                textWidth += spaceGlyph.Advance * scale;
            }
            else
            {
                (Glyph glyph, _) = _fontSystem.GetGlyph(ref _font, c);
                textWidth += glyph.Advance * scale;
            }
        }

        float textHeight = _font.LineHeight * scale;
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

    private void RenderNineSlice(Vector2D<float> position, Vector2D<float> size, Vector4D<float> color, int borderPixels, int textureSize,
        Graphics.Components.Texture2D texture)
    {
        Slice[] slices = NineSliceHelper.CalculateSlices(position, size, borderPixels, textureSize);

        foreach (Slice slice in slices)
        {
            Vector3D<float> pos0 = new(slice.PositionMin.X, slice.PositionMin.Y, 0);
            Vector3D<float> pos1 = new(slice.PositionMax.X, slice.PositionMin.Y, 0);
            Vector3D<float> pos2 = new(slice.PositionMax.X, slice.PositionMax.Y, 0);
            Vector3D<float> pos3 = new(slice.PositionMin.X, slice.PositionMax.Y, 0);

            Vertex[] vertices =
            [
                new(pos0, new Vector2D<float>(slice.UvMin.X, slice.UvMin.Y), color),
                new(pos1, new Vector2D<float>(slice.UvMax.X, slice.UvMin.Y), color),
                new(pos2, new Vector2D<float>(slice.UvMax.X, slice.UvMax.Y), color),
                new(pos3, new Vector2D<float>(slice.UvMin.X, slice.UvMax.Y), color)
            ];

            _renderer.SubmitQuad(vertices, texture);
        }
    }

    private void RenderNextPiece(float centerX, float centerY, TetrominoType nextType)
    {
        if (nextType == TetrominoType.None)
            return;

        Vector2D<float>[] blocks = Tetromino.GetBlocks(nextType, 0);
        Vector4D<float> color = Tetromino.GetColor(Tetromino.GetColorIndex(nextType));

        float minX = blocks.Min(b => b.X);
        float maxX = blocks.Max(b => b.X);
        float minY = blocks.Min(b => b.Y);
        float maxY = blocks.Max(b => b.Y);

        float pieceWidth = (maxX - minX + 1) * _blockSize;
        float pieceHeight = (maxY - minY + 1) * _blockSize;

        const float maxSize = 2.4f;
        float scale = Math.Min(maxSize / pieceWidth, maxSize / pieceHeight);
        scale = Math.Min(scale, 0.6f);

        float offsetX = (minX + maxX) / 2f;
        float offsetY = (minY + maxY) / 2f;

        foreach (Vector2D<float> offset in blocks)
        {
            float x = centerX + (offset.X - offsetX) * _blockSize * scale;
            float y = centerY + (offset.Y - offsetY) * _blockSize * scale;
            RenderBlockScaled(x, y, color, scale);
        }
    }

    private void RenderBlockScaled(float x, float y, Vector4D<float> color, float scale)
    {
        float size = _blockSize * scale;
        RenderNineSlice(new Vector2D<float>(x, y), new Vector2D<float>(size, size), color, _blockBorderSize, _blockTextureSize,
            _blockTexture);
    }
}
