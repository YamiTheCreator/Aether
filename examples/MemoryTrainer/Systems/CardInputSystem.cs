using Aether.Core;
using Graphics;
using Graphics.Components;
using Graphics.Structures;
using Graphics.Systems;
using MemoryTrainer.Components;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace MemoryTrainer.Systems;

public class CardInputSystem : SystemBase
{
    private bool _wasMousePressed;
    private InputSystem? _inputSystem;
    private Input? _input;
    private MeshSystem? _meshSystem;
    private TextureSystem? _textureSystem;

    protected override void OnCreate()
    {
        _inputSystem = World.GetGlobal<InputSystem>();
        _input = World.GetGlobal<Input>();
        _meshSystem = World.GetGlobal<MeshSystem>();
        _textureSystem = World.GetGlobal<TextureSystem>();
    }

    protected override void OnUpdate( float deltaTime )
    {
        if ( _inputSystem is null || _input is null )
            return;

        GameState gameState = World.GetGlobal<GameState>();

        if ( _inputSystem.IsKeyPressed( _input, Key.R ) )
        {
            RestartGame();
            return;
        }

        if ( gameState.IsWaitingForFlipBack )
        {
            gameState.DelayTimer += deltaTime;

            if ( gameState.DelayTimer >= 1f )
            {
                if ( gameState.FirstRevealedCard.HasValue && World.IsAlive( gameState.FirstRevealedCard.Value ) )
                {
                    ref Card card1 = ref World.Get<Card>( gameState.FirstRevealedCard.Value );
                    if ( !card1.IsMatched )
                    {
                        card1.IsFlipping = true;
                        card1.FlipToFront = false;
                        card1.FlipProgress = 0f;
                        card1.IsRevealed = false;
                    }
                }

                if ( gameState.SecondRevealedCard.HasValue && World.IsAlive( gameState.SecondRevealedCard.Value ) )
                {
                    ref Card card2 = ref World.Get<Card>( gameState.SecondRevealedCard.Value );
                    if ( !card2.IsMatched )
                    {
                        card2.IsFlipping = true;
                        card2.FlipToFront = false;
                        card2.FlipProgress = 0f;
                        card2.IsRevealed = false;
                    }
                }

                gameState.FirstRevealedCard = null;
                gameState.SecondRevealedCard = null;
                gameState.IsWaitingForFlipBack = false;
                gameState.DelayTimer = 0f;
            }

            World.SetGlobal( gameState );
            return;
        }

        bool isMousePressed = _inputSystem.IsMouseButtonDown( _input, MouseButton.Left );

        if ( isMousePressed && !_wasMousePressed )
        {
            System.Numerics.Vector2 mousePos = _inputSystem.GetMousePosition( _input );
            Vector2D<float> mousePosVec = new( mousePos.X, mousePos.Y );
            
            Entity? cameraEntity = null;
            foreach ( Entity entity in World.Filter<Camera>() )
            {
                cameraEntity = entity;
                break;
            }

            if ( cameraEntity.HasValue )
            {
                Camera camera = World.Get<Camera>( cameraEntity.Value );
                World.Get<Transform>( cameraEntity.Value );
                
                if ( UnprojectMouseToRay( mousePosVec, camera, out Vector3D<float> rayOrigin,
                        out Vector3D<float> rayDirection ) )
                {
                    CheckCardClick( rayOrigin, rayDirection, ref gameState );
                }
            }
        }

        _wasMousePressed = isMousePressed;
        World.SetGlobal( gameState );
    }

    private bool UnprojectMouseToRay( Vector2D<float> mousePos, Camera camera,
        out Vector3D<float> rayOrigin, out Vector3D<float> rayDirection )
    {
        float width = WindowBase.LogicalWidth;
        float height = WindowBase.LogicalHeight;
        
        float x = 2f * mousePos.X / width - 1f;
        float y = 1f - 2f * mousePos.Y / height;

        Matrix4X4<float> viewMatrix = camera.ViewMatrix;
        Matrix4X4<float> projMatrix = camera.ProjectionMatrix;

        Matrix4X4<float> viewProjMatrix = viewMatrix * projMatrix;
        if ( !Matrix4X4.Invert( viewProjMatrix, out Matrix4X4<float> invMatrix ) )
        {
            rayOrigin = Vector3D<float>.Zero;
            rayDirection = Vector3D<float>.Zero;
            return false;
        }

        Vector4D<float> rayClipNear = new( x, y, -1f, 1f );
        Vector4D<float> rayClipFar = new( x, y, 1f, 1f );

        Vector4D<float> rayWorldNear = Vector4D.Transform( rayClipNear, invMatrix );
        Vector4D<float> rayWorldFar = Vector4D.Transform( rayClipFar, invMatrix );

        if ( MathF.Abs( rayWorldNear.W ) > 0.0001f )
            rayWorldNear /= rayWorldNear.W;
        if ( MathF.Abs( rayWorldFar.W ) > 0.0001f )
            rayWorldFar /= rayWorldFar.W;

        rayOrigin = new Vector3D<float>( rayWorldNear.X, rayWorldNear.Y, rayWorldNear.Z );
        Vector3D<float> rayEnd = new( rayWorldFar.X, rayWorldFar.Y, rayWorldFar.Z );
        rayDirection = Vector3D.Normalize( rayEnd - rayOrigin );

        return true;
    }

    private void CheckCardClick( Vector3D<float> rayOrigin, Vector3D<float> rayDirection, ref GameState gameState )
    {
        float planeY = 0f;
        
        if ( MathF.Abs( rayDirection.Y ) < 0.0001f )
            return;

        float t = ( planeY - rayOrigin.Y ) / rayDirection.Y;
        if ( t < 0 )
            return;

        Vector3D<float> intersectionPoint = rayOrigin + rayDirection * t;

        Console.WriteLine( $"Клик в точке ({intersectionPoint.X:F2}, {intersectionPoint.Z:F2})" );

        // Проверяем каждую карту
        foreach ( Entity entity in World.Filter<Card>().With<Transform>() )
        {
            ref Card card = ref World.Get<Card>( entity );
            Transform transform = World.Get<Transform>( entity );

            // Пропускаем уже найденные пары или переворачивающиеся карты
            if ( card.IsMatched || card.IsFlipping )
                continue;

            // Проверяем, попадает ли точка в границы карты
            // Карты квадратные 0.9x0.9, лежат в плоскости XZ
            const float cardSize = 0.9f;

            float minX = transform.Position.X - cardSize / 2f;
            float maxX = transform.Position.X + cardSize / 2f;
            float minZ = transform.Position.Z - cardSize / 2f;
            float maxZ = transform.Position.Z + cardSize / 2f;

            if ( intersectionPoint.X >= minX && intersectionPoint.X <= maxX &&
                 intersectionPoint.Z >= minZ && intersectionPoint.Z <= maxZ )
            {
                Console.WriteLine(
                    $"  -> Карта {card.CardId} в ({transform.Position.X:F2}, {transform.Position.Z:F2})" );
                HandleCardClick( entity, ref card, ref gameState );
                return;
            }
        }
    }

    private void HandleCardClick( Entity entity, ref Card card, ref GameState gameState )
    {
        // Пропускаем уже найденные пары
        if ( card.IsMatched )
            return;

        // Пропускаем уже открытые карты
        if ( card.IsRevealed )
            return;

        // Если уже открыты две карты, игнорируем клик
        if ( gameState is { FirstRevealedCard: not null, SecondRevealedCard: not null } )
            return;

        // Открываем карту
        card.IsFlipping = true;
        card.FlipToFront = true;
        card.FlipProgress = 0f;

        // Сохраняем открытую карту
        if ( !gameState.FirstRevealedCard.HasValue )
        {
            gameState.FirstRevealedCard = entity;
        }
        else if ( !gameState.SecondRevealedCard.HasValue )
        {
            gameState.SecondRevealedCard = entity;
            gameState.Moves++;

            // Проверяем совпадение
            ref Card firstCard = ref World.Get<Card>( gameState.FirstRevealedCard.Value );

            if ( firstCard.PairId == card.PairId )
            {
                // Найдена пара. Карты остаются открытыми
                firstCard.IsMatched = true;
                card.IsMatched = true;
                gameState.MatchedPairs++;

                Console.WriteLine(
                    $"Пара найдена! Осталось пар: {gameState.GridRows * gameState.GridCols / 2 - gameState.MatchedPairs}" );

                // Проверяем победу
                if ( gameState.MatchedPairs >= gameState.TotalPairs )
                {
                    Console.WriteLine( $"Победа! Количество ходов: {gameState.Moves}" );
                }

                gameState.FirstRevealedCard = null;
                gameState.SecondRevealedCard = null;
            }
            else
            {
                // Не совпало - запускаем таймер для переворота обратно
                gameState.IsWaitingForFlipBack = true;
                gameState.DelayTimer = 0f;
            }
        }
    }

    protected override void OnRender()
    {
    }

    protected override void OnDestroy()
    {
    }

    private void RestartGame()
    {
        if ( _meshSystem is null || _textureSystem is null )
            return;

        Console.WriteLine( "Перезапуск игры..." );

        // Удаляем все карты
        List<Entity> cardsToRemove = [ ];
        foreach ( Entity entity in World.Filter<Card>() )
        {
            cardsToRemove.Add( entity );
        }

        foreach ( Entity entity in cardsToRemove )
        {
            World.Despawn( entity );
        }

        // Сбрасываем состояние игры
        GameState gameState = World.GetGlobal<GameState>();
        gameState.FirstRevealedCard = null;
        gameState.SecondRevealedCard = null;
        gameState.MatchedPairs = 0;
        gameState.Moves = 0;
        gameState.DelayTimer = 0f;
        gameState.IsWaitingForFlipBack = false;
        World.SetGlobal( gameState );

        int rows = gameState.GridRows;
        int cols = gameState.GridCols;
        int totalCards = rows * cols;

        // Создаем массив пар
        List<int> pairIds = [ ];
        for ( int i = 0; i < totalCards / 2; i++ )
        {
            pairIds.Add( i );
            pairIds.Add( i );
        }

        // Перемешиваем
        Random random = new();
        for ( int i = pairIds.Count - 1; i > 0; i-- )
        {
            int j = random.Next( i + 1 );
            ( pairIds[ i ], pairIds[ j ] ) = ( pairIds[ j ], pairIds[ i ] );
        }

        // Размеры карты и отступы
        float cardSize = 0.9f;
        float cardDepth = 0.05f;
        float spacing = 1.0f;

        // Центрируем поле
        float startX = -( cols - 1 ) * spacing / 2f;
        float startZ = -( rows - 1 ) * spacing / 2f;

        int cardIndex = 0;
        for ( int row = 0; row < rows; row++ )
        {
            for ( int col = 0; col < cols; col++ )
            {
                int pairId = pairIds[ cardIndex ];
                int textureIndex = pairId % 8;

                Vector3D<float> position = new(
                    startX + col * spacing,
                    0f,
                    startZ + row * spacing
                );

                CreateCard( cardIndex, pairId, textureIndex,
                    position, cardSize, cardSize, cardDepth, _meshSystem, _textureSystem );

                cardIndex++;
            }
        }

        Console.WriteLine( "Игра перезапущена!" );
    }

    private void CreateCard( int cardId, int pairId, int textureIndex,
        Vector3D<float> position, float width, float height, float depth,
        MeshSystem? meshSystem, TextureSystem? textureSystem )
    {
        if ( meshSystem is null || textureSystem is null )
            return;

        Entity entity = World.Spawn();

        World.Add( entity, new Card
        {
            CardId = cardId,
            PairId = pairId,
            TextureIndex = textureIndex,
            IsRevealed = false,
            IsMatched = false,
            IsFlipping = false,
            FlipProgress = 0f,
            FlipSpeed = 3f,
            FlipToFront = false
        } );

        World.Add( entity, new Transform
        {
            Position = position,
            Rotation = Quaternion<float>.CreateFromAxisAngle(
                new Vector3D<float>( 1f, 0f, 0f ),
                90f * MathF.PI / 180f
            ),
            Scale = Vector3D<float>.One
        } );

        Mesh cardMesh = CreateCardMesh( width, height, depth, meshSystem );
        World.Add( entity, cardMesh );

        Texture2D tempTexture = textureSystem.CreateTextureFromColor( 1, 1, 50, 100, 200 );

        Material material = new()
        {
            Texture = tempTexture,
            DiffuseColor = new Vector3D<float>( 1f, 1f, 1f ),
            AmbientColor = new Vector3D<float>( 0.3f, 0.3f, 0.3f ),
            SpecularColor = new Vector3D<float>( 0.5f, 0.5f, 0.5f ),
            Shininess = 32f,
            Alpha = 1f
        };
        World.Add( entity, material );
    }

    private Mesh CreateCardMesh( float width, float height, float depth, MeshSystem meshSystem )
    {
        float hw = width / 2f;
        float hh = height / 2f;
        float hd = depth / 2f;

        List<Vertex> vertices = [ ];
        List<uint> indices = [ ];

        Vector4D<float> white = new( 1f, 1f, 1f, 1f );

        AddQuad( vertices, indices,
            new Vector3D<float>( -hw, -hh, hd ), new Vector3D<float>( hw, -hh, hd ),
            new Vector3D<float>( hw, hh, hd ), new Vector3D<float>( -hw, hh, hd ),
            new Vector3D<float>( 0f, 0f, 1f ), white );

        AddQuad( vertices, indices,
            new Vector3D<float>( hw, -hh, -hd ), new Vector3D<float>( -hw, -hh, -hd ),
            new Vector3D<float>( -hw, hh, -hd ), new Vector3D<float>( hw, hh, -hd ),
            new Vector3D<float>( 0f, 0f, -1f ), white );

        AddQuad( vertices, indices,
            new Vector3D<float>( -hw, hh, hd ), new Vector3D<float>( hw, hh, hd ),
            new Vector3D<float>( hw, hh, -hd ), new Vector3D<float>( -hw, hh, -hd ),
            new Vector3D<float>( 0f, 1f, 0f ), white );

        AddQuad( vertices, indices,
            new Vector3D<float>( -hw, -hh, -hd ), new Vector3D<float>( hw, -hh, -hd ),
            new Vector3D<float>( hw, -hh, hd ), new Vector3D<float>( -hw, -hh, hd ),
            new Vector3D<float>( 0f, -1f, 0f ), white );

        AddQuad( vertices, indices,
            new Vector3D<float>( hw, -hh, hd ), new Vector3D<float>( hw, -hh, -hd ),
            new Vector3D<float>( hw, hh, -hd ), new Vector3D<float>( hw, hh, hd ),
            new Vector3D<float>( 1f, 0f, 0f ), white );

        AddQuad( vertices, indices,
            new Vector3D<float>( -hw, -hh, -hd ), new Vector3D<float>( -hw, -hh, hd ),
            new Vector3D<float>( -hw, hh, hd ), new Vector3D<float>( -hw, hh, -hd ),
            new Vector3D<float>( -1f, 0f, 0f ), white );

        return meshSystem.CreateMesh( vertices.ToArray(), indices.ToArray() );
    }

    private void AddQuad( List<Vertex> vertices, List<uint> indices,
        Vector3D<float> v0, Vector3D<float> v1, Vector3D<float> v2, Vector3D<float> v3,
        Vector3D<float> normal, Vector4D<float> color )
    {
        uint baseIndex = ( uint )vertices.Count;

        vertices.Add( new Vertex( v0, new Vector2D<float>( 0f, 1f ), color, 0, normal ) );
        vertices.Add( new Vertex( v1, new Vector2D<float>( 1f, 1f ), color, 0, normal ) );
        vertices.Add( new Vertex( v2, new Vector2D<float>( 1f, 0f ), color, 0, normal ) );
        vertices.Add( new Vertex( v3, new Vector2D<float>( 0f, 0f ), color, 0, normal ) );

        indices.Add( baseIndex );
        indices.Add( baseIndex + 1 );
        indices.Add( baseIndex + 2 );

        indices.Add( baseIndex );
        indices.Add( baseIndex + 2 );
        indices.Add( baseIndex + 3 );
    }
}