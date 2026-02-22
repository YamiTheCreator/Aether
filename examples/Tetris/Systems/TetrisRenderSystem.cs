using Silk.NET.Maths;
using Aether.Core;
using Graphics;
using Graphics.Components;
using Graphics.Systems;
using Silk.NET.OpenGL;
using Tetris.Components;

namespace Tetris.Systems;

public class TetrisRenderSystem : SystemBase
{
    private Texture2D _blockTexture;
    private Texture2D _inputTexture;
    private TextRenderSystem? _textRenderSystem;

    private const float _blockSize = 1f;
    private const float _boardX = -6f;
    private const float _boardY = -10f;
    private const float _uix = 6f;
    private const float _uiy = 9f;
    private const float _margin = 2f;

    private readonly List<Entity> _blockEntities = [ ];
    private readonly List<Entity> _staticEntities = [ ]; // Static UI elements
    private readonly List<Entity> _lockedBlockEntities = [ ]; // Locked blocks on the board
    private int _lastBoardHash; // Track when board changes

    protected override void OnInit()
    {
        TextureSystem textureSystem = World.GetGlobal<TextureSystem>();
        _textRenderSystem = World.GetSystem<TextRenderSystem>();

        string solutionRoot =
            Path.GetFullPath( Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "../../../../../" ) );
        string blockPath = Path.Combine( solutionRoot, "examples/Tetris/Assets/block.jpeg" );
        string inputPath = Path.Combine( solutionRoot, "examples/Tetris/Assets/input.jpeg" );

        _blockTexture = textureSystem.CreateTextureFromFile( blockPath,
            TextureWrapMode.Repeat, TextureWrapMode.Repeat,
            TextureMinFilter.Nearest, TextureMagFilter.Nearest );
        _inputTexture = textureSystem.CreateTextureFromFile( inputPath,
            TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge,
            TextureMinFilter.Nearest, TextureMagFilter.Nearest );

        // Render static elements once
        RenderStaticUi();
        RenderWalls();
        RenderLockedBlocks(); // Initial render of locked blocks
    }

    protected override void OnUpdate( float deltaTime )
    {
        // Clear only dynamic entities (current piece, dynamic UI text)
        foreach ( Entity e in _blockEntities )
        {
            if ( World.IsAlive( e ) )
                World.Despawn( e );
        }

        _blockEntities.Clear();

        // Calculate board hash to detect changes
        int boardHash = 0;
        foreach ( Entity entity in World.Filter<TetrisBoardComponent>() )
        {
            ref TetrisBoardComponent board = ref World.Get<TetrisBoardComponent>( entity );

            for ( int x = 0; x < 10; x++ )
            {
                for ( int y = 0; y < 20; y++ )
                {
                    if ( board.Board[ x, y ] != 0 )
                    {
                        boardHash = boardHash * 31 + x * 100 + y * 10 + board.Board[ x, y ];
                    }
                }
            }

            break;
        }

        // Re-render locked blocks only when board changes
        if ( boardHash != _lastBoardHash )
        {
            foreach ( Entity e in _lockedBlockEntities )
            {
                if ( World.IsAlive( e ) )
                    World.Despawn( e );
            }

            _lockedBlockEntities.Clear();
            RenderLockedBlocks();
            _lastBoardHash = boardHash;
        }

        // Render dynamic elements every frame
        RenderCurrentPiece();
        RenderDynamicUi();
    }

    protected override void OnRender()
    {
        GL gl = WindowBase.Gl;

        gl.Enable( EnableCap.Blend );
        gl.BlendFunc( BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha );
        gl.Disable( EnableCap.DepthTest );
    }

    private void RenderWalls()
    {
        Vector4D<float> wallColor = new( 0.5f, 0.5f, 0.5f, 1f );

        // Left wall
        for ( int y = 0; y < 20; y++ )
        {
            CreateStaticBlock( _boardX - _blockSize, _boardY + y * _blockSize, wallColor );
        }

        // Right wall
        for ( int y = 0; y < 20; y++ )
        {
            CreateStaticBlock( _boardX + 10 * _blockSize, _boardY + y * _blockSize, wallColor );
        }

        // Bottom wall
        for ( int x = -1; x <= 10; x++ )
        {
            CreateStaticBlock( _boardX + x * _blockSize, _boardY - _blockSize, wallColor );
        }
    }

    private void RenderLockedBlocks()
    {
        foreach ( Entity entity in World.Filter<TetrisBoardComponent>() )
        {
            ref TetrisBoardComponent board = ref World.Get<TetrisBoardComponent>( entity );

            for ( int x = 0; x < 10; x++ )
            {
                for ( int y = 0; y < 20; y++ )
                {
                    int colorIndex = board.Board[ x, y ];
                    if ( colorIndex != 0 )
                    {
                        Vector4D<float> color = Tetromino.GetColor( colorIndex );
                        CreateBlock( _boardX + x * _blockSize, _boardY + y * _blockSize, color );
                    }
                }
            }
        }
    }

    private void RenderCurrentPiece()
    {
        foreach ( Entity entity in World.Filter<TetrisPieceComponent>() )
        {
            ref TetrisPieceComponent piece = ref World.Get<TetrisPieceComponent>( entity );

            if ( piece.Type == TetrominoType.None )
                continue;

            Vector2D<float>[] blocks = Tetromino.GetBlocks( piece.Type, piece.Rotation );
            Vector4D<float> color = Tetromino.GetColor( Tetromino.GetColorIndex( piece.Type ) );

            foreach ( Vector2D<float> offset in blocks )
            {
                float x = _boardX + ( piece.Position.X + offset.X ) * _blockSize;
                float y = _boardY + ( piece.Position.Y + offset.Y ) * _blockSize;
                CreateDynamicBlock( x, y, color );
            }
        }
    }

    private void CreateDynamicBlock( float x, float y, Vector4D<float> color )
    {
        MaterialSystem materialSystem = World.GetSystem<MaterialSystem>()!;
        Material material = materialSystem.CreateTextured( _blockTexture, color );

        Sprite sprite = Sprite.Create( material, new Vector2D<float>( _blockSize, _blockSize ) );
        sprite.Pivot = new Vector2D<float>( 0, 0 ); // Bottom-left pivot

        Entity blockEntity = World.Spawn();
        World.Add( blockEntity,
            new Transform( new Vector3D<float>( x, y, 0 ), Quaternion<float>.Identity, Vector3D<float>.One ) );
        World.Add( blockEntity, sprite );

        _blockEntities.Add( blockEntity );
    }

    private void CreateBlock( float x, float y, Vector4D<float> color )
    {
        MaterialSystem materialSystem = World.GetSystem<MaterialSystem>()!;
        Material material = materialSystem.CreateTextured( _blockTexture, color );

        Sprite sprite = Sprite.Create( material, new Vector2D<float>( _blockSize, _blockSize ) );
        sprite.Pivot = new Vector2D<float>( 0, 0 ); // Bottom-left pivot

        Entity blockEntity = World.Spawn();
        World.Add( blockEntity,
            new Transform( new Vector3D<float>( x, y, 0 ), Quaternion<float>.Identity, Vector3D<float>.One ) );
        World.Add( blockEntity, sprite );

        _lockedBlockEntities.Add( blockEntity ); // Add to locked blocks list
    }

    private void CreateStaticBlock( float x, float y, Vector4D<float> color )
    {
        MaterialSystem materialSystem = World.GetSystem<MaterialSystem>()!;
        Material material = materialSystem.CreateTextured( _blockTexture, color );

        Sprite sprite = Sprite.Create( material, new Vector2D<float>( _blockSize, _blockSize ) );
        sprite.Pivot = new Vector2D<float>( 0, 0 ); // Bottom-left pivot

        Entity blockEntity = World.Spawn();
        World.Add( blockEntity,
            new Transform( new Vector3D<float>( x, y, 0 ), Quaternion<float>.Identity, Vector3D<float>.One ) );
        World.Add( blockEntity, sprite );

        _staticEntities.Add( blockEntity ); // Add to static list
    }

    private void RenderStaticUi()
    {
        const float scale = 0.012f;
        Vector4D<float> white = new( 1, 1, 1, 1 );
        const float boxWidth = 3f;
        const float boxHeight = 1.5f;

        float currentY = _uiy;

        _textRenderSystem?.RenderText( "LEVEL", new Vector3D<float>( _uix, currentY, 0.1f ), scale, white,
            flipY: true );

        currentY -= _margin;
        CreateStaticInputBox( _uix, currentY, boxWidth, boxHeight );

        currentY -= boxHeight + 0.5f;

        _textRenderSystem?.RenderText( "LINES", new Vector3D<float>( _uix, currentY, 0.1f ), scale, white,
            flipY: true );

        currentY -= _margin;
        CreateStaticInputBox( _uix, currentY, boxWidth, boxHeight );

        currentY -= boxHeight + 0.5f;

        _textRenderSystem?.RenderText( "SCORE", new Vector3D<float>( _uix, currentY, 0.1f ), scale, white,
            flipY: true );

        currentY -= _margin;
        CreateStaticInputBox( _uix, currentY, boxWidth, boxHeight );

        currentY -= boxHeight + 1.0f;

        _textRenderSystem?.RenderText( "NEXT", new Vector3D<float>( _uix, currentY, 0.1f ), scale, white,
            flipY: true );

        currentY -= _margin;
        CreateStaticPreviewBox( _uix, currentY - 1.5f, boxWidth + 1f, boxWidth );
    }

    private void RenderDynamicUi()
    {
        const float scale = 0.012f;
        Vector4D<float> green = new( 0, 1, 0, 1 );
        Vector4D<float> white = new( 1, 1, 1, 1 );
        const float boxWidth = 3f;
        const float boxHeight = 1.5f;

        // Get game state
        int level = 1;
        int linesForNextLevel = 10;
        int score = 0;
        TetrominoType nextType = TetrominoType.None;
        bool isGameOver = false;

        foreach ( Entity entity in World.Filter<TetrisGameStateComponent>() )
        {
            ref TetrisGameStateComponent state = ref World.Get<TetrisGameStateComponent>( entity );
            level = state.Level;
            linesForNextLevel = ( state.Level * 10 ) - state.LinesCleared;
            score = state.Score;
            nextType = state.NextType;
            isGameOver = state.IsGameOver;
        }

        float currentY = _uiy;
        currentY -= _margin;

        _textRenderSystem?.RenderTextCentered( level.ToString(),
            new Vector3D<float>( _uix + boxWidth / 2f, currentY + 0.15f, 0.1f ), scale, green, flipY: true );

        currentY -= boxHeight + 0.5f;
        currentY -= _margin;

        _textRenderSystem?.RenderTextCentered( linesForNextLevel.ToString(),
            new Vector3D<float>( _uix + boxWidth / 2f, currentY + 0.15f, 0.1f ), scale, green, flipY: true );

        currentY -= boxHeight + 0.5f;
        currentY -= _margin;

        _textRenderSystem?.RenderTextCentered( score.ToString(),
            new Vector3D<float>( _uix + boxWidth / 2f, currentY + 0.15f, 0.1f ), scale, green, flipY: true );

        currentY -= boxHeight + 1.0f;
        currentY -= _margin;
        RenderNextPiece( _uix + boxWidth / 2f + 0.2f, currentY - 0.3f, nextType );

        if ( isGameOver && _textRenderSystem != null )
        {
            _textRenderSystem.RenderText( "GAME OVER", new Vector3D<float>( _boardX + 2f, _boardY + 10f, 0.1f ),
                0.02f, new Vector4D<float>( 1, 0, 0, 1 ), flipY: true );
            _textRenderSystem.RenderText( "Press R", new Vector3D<float>( _boardX + 2.5f, _boardY + 8f, 0.1f ),
                0.015f, white, flipY: true );
        }
    }

    private void CreateStaticInputBox( float x, float y, float width, float height )
    {
        MaterialSystem materialSystem = World.GetSystem<MaterialSystem>()!;
        Material material = materialSystem.CreateTextured( _inputTexture );
        Sprite sprite = Sprite.Create( material, new Vector2D<float>( width, height ) );
        sprite.Pivot = new Vector2D<float>( 0, 0 );

        Entity boxEntity = World.Spawn();
        World.Add( boxEntity,
            new Transform( new Vector3D<float>( x, y, 0 ), Quaternion<float>.Identity, Vector3D<float>.One ) );
        World.Add( boxEntity, sprite );

        _staticEntities.Add( boxEntity );
    }

    private void CreateStaticPreviewBox( float x, float y, float width, float height )
    {
        MaterialSystem materialSystem = World.GetSystem<MaterialSystem>()!;
        Vector4D<float> darkColor = new( 0.2f, 0.2f, 0.2f, 1f );
        Material material = materialSystem.CreateTextured( _blockTexture, darkColor );
        Sprite sprite = Sprite.Create( material, new Vector2D<float>( width, height ) );
        sprite.Pivot = new Vector2D<float>( 0, 0 );

        Entity boxEntity = World.Spawn();
        World.Add( boxEntity,
            new Transform( new Vector3D<float>( x, y, 0 ), Quaternion<float>.Identity, Vector3D<float>.One ) );
        World.Add( boxEntity, sprite );

        _staticEntities.Add( boxEntity );
    }

    private void RenderNextPiece( float centerX, float centerY, TetrominoType nextType )
    {
        if ( nextType == TetrominoType.None )
            return;

        Vector2D<float>[] blocks = Tetromino.GetBlocks( nextType, 0 );
        Vector4D<float> color = Tetromino.GetColor( Tetromino.GetColorIndex( nextType ) );

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

        foreach ( Vector2D<float> offset in blocks )
        {
            float x = centerX + ( offset.X - offsetX ) * _blockSize * scale;
            float y = centerY + ( offset.Y - offsetY ) * _blockSize * scale;
            CreateBlockScaled( x, y, color, scale );
        }
    }

    private void CreateBlockScaled( float x, float y, Vector4D<float> color, float scale )
    {
        MaterialSystem materialSystem = World.GetSystem<MaterialSystem>()!;
        float size = _blockSize * scale;
        Material material = materialSystem.CreateTextured( _blockTexture, color );
        Sprite sprite = Sprite.Create( material, new Vector2D<float>( size, size ) );
        sprite.Pivot = new Vector2D<float>( 0, 0 );

        Entity blockEntity = World.Spawn();
        World.Add( blockEntity,
            new Transform( new Vector3D<float>( x, y, 0.1f ), Quaternion<float>.Identity,
                Vector3D<float>.One ) ); // Z=0.1 to render above preview box
        World.Add( blockEntity, sprite );

        _blockEntities.Add( blockEntity );
    }
}