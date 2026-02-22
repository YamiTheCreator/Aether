using Silk.NET.Maths;
using Aether.Core;
using Graphics;
using Graphics.Components;
using Graphics.Systems;
using Graphics.Structures;
using MemoryTrainer.Systems;
using MemoryTrainer.Components;

namespace MemoryTrainer;

public class Application() : ApplicationBase(
    title: "Memory Trainer 3D",
    width: 1280,
    height: 720,
    createDefaultCamera: false )
{
    protected override void OnInitialize()
    {
        ShaderSystem shaderSystem = new( WindowBase.Gl );
        TextureSystem textureSystem = new( WindowBase.Gl );
        MaterialSystem materialSystem = new();
        InputSystem inputSystem = new();
        MeshSystem meshSystem = new( WindowBase.Gl );

        Texture2D whiteTexture = textureSystem.CreateTextureFromColor( 1, 1 );

        ShaderProgram shaderProgram = new( WindowBase.Gl );
        Shader shader = new()
        {
            Program = shaderProgram
        };

        Input input = inputSystem.CreateInput( WindowBase.Input );

        World.SetGlobal( shaderSystem );
        World.SetGlobal( textureSystem );
        World.SetGlobal( inputSystem );
        World.SetGlobal( meshSystem );
        World.SetGlobal( materialSystem );
        World.SetGlobal( whiteTexture );
        World.SetGlobal( shader );
        World.SetGlobal( input );

        World.SetGlobal( new GameState
        {
            GridRows = 4,
            GridCols = 4,
            TotalPairs = 8,
            MatchedPairs = 0,
            Moves = 0
        } );

        World.AddSystem( shaderSystem );
        World.AddSystem( new CameraSystem() );
        World.AddSystem( new LightingSystem() );
        World.AddSystem( materialSystem );
        World.AddSystem( new CardFlipSystem() );
        World.AddSystem( new CardInputSystem() );
        World.AddSystem( new CardRenderSystem() );
        World.AddSystem( new RenderSystem( WindowBase.Gl ) );

        CreateCamera();
        CreateLights();
        CreateGameBoard();
    }

    private void CreateCamera()
    {
        Entity cameraEntity = World.Spawn();
        Camera cam = Camera.CreatePerspective(
            fov: 45f,
            aspectRatio: ( float )WindowBase.LogicalWidth / WindowBase.LogicalHeight
        );

        // Настраиваем камеру для взгляда вниз на карты
        cam.Yaw = -90f; // Смотрим вдоль оси -Z
        cam.Pitch = -60f; // Наклон вниз на 60 градусов

        World.Add( cameraEntity, cam );

        World.Add( cameraEntity, new Transform
        {
            Position = new Vector3D<float>( 0f, 5f, 3f ), // Ближе к картам
            Rotation = Quaternion<float>.Identity,
            Scale = Vector3D<float>.One
        } );
    }

    private void CreateLights()
    {
        LightingSystem lightingSystem = World.GetSystem<LightingSystem>()!;

        Entity light1 = World.Spawn();
        World.Add( light1, lightingSystem.CreatePointFull(
            ambientColor: new Vector3D<float>( 0.3f, 0.3f, 0.3f ),
            diffuseColor: new Vector3D<float>( 1f, 1f, 1f ),
            specularColor: new Vector3D<float>( 1f, 1f, 1f ),
            intensity: 20f,
            range: 50f
        ) );
        World.Add( light1, new Transform
        {
            Position = new Vector3D<float>( 0f, 10f, 5f ),
            Rotation = Quaternion<float>.Identity,
            Scale = Vector3D<float>.One
        } );

        Entity light2 = World.Spawn();
        World.Add( light2, lightingSystem.CreatePointFull(
            ambientColor: new Vector3D<float>( 0.2f, 0.2f, 0.2f ),
            diffuseColor: new Vector3D<float>( 0.8f, 0.8f, 1f ),
            specularColor: new Vector3D<float>( 0.5f, 0.5f, 0.5f ),
            intensity: 3f,
            range: 30f
        ) );
        World.Add( light2, new Transform
        {
            Position = new Vector3D<float>( -5f, 5f, 8f ),
            Rotation = Quaternion<float>.Identity,
            Scale = Vector3D<float>.One
        } );
    }

    private void CreateGameBoard()
    {
        GameState gameState = World.GetGlobal<GameState>();
        MeshSystem meshSystem = World.GetGlobal<MeshSystem>();

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

        // Размеры карты и отступы (делаем карты квадратными и ближе друг к другу)
        float cardSize = 0.9f; // Квадратные карты
        float cardDepth = 0.05f; // Тонкие карты
        float spacing = 1.0f; // Меньше расстояние между картами

        // Центрируем поле
        float startX = -( cols - 1 ) * spacing / 2f;
        float startZ = -( rows - 1 ) * spacing / 2f;

        int cardIndex = 0;
        for ( int row = 0; row < rows; row++ )
        {
            for ( int col = 0; col < cols; col++ )
            {
                int pairId = pairIds[ cardIndex ];
                int textureIndex = pairId % 8; // 8 текстур

                Vector3D<float> position = new(
                    startX + col * spacing,
                    0f,
                    startZ + row * spacing
                );

                CreateCard( cardIndex, pairId, textureIndex,
                    position, cardSize, cardSize, cardDepth, meshSystem );

                cardIndex++;
            }
        }
    }

    private void CreateCard( int cardId, int pairId, int textureIndex,
        Vector3D<float> position, float width, float height, float depth,
        MeshSystem meshSystem )
    {
        Entity entity = World.Spawn();

        // Добавляем компонент карты
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

        // Добавляем трансформ (поворачиваем карту на 90 градусов вокруг X, чтобы она лежала)
        World.Add( entity, new Transform
        {
            Position = position,
            Rotation = Quaternion<float>.CreateFromAxisAngle(
                new Vector3D<float>( 1f, 0f, 0f ),
                90f * MathF.PI / 180f
            ),
            Scale = Vector3D<float>.One
        } );

        // Создаем меш карты (простой куб)
        Mesh cardMesh = CreateCardMesh( width, height, depth, meshSystem );
        World.Add( entity, cardMesh );

        // Создаем материал (будет обновляться в CardRenderSystem)
        TextureSystem textureSystem = World.GetGlobal<TextureSystem>();
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

        // Создаем вершины для куба
        List<Vertex> vertices = [ ];
        List<uint> indices = [ ];

        Vector4D<float> white = new( 1f, 1f, 1f, 1f );

        // Передняя грань (лицевая сторона)
        AddQuad( vertices, indices,
            new Vector3D<float>( -hw, -hh, hd ), new Vector3D<float>( hw, -hh, hd ),
            new Vector3D<float>( hw, hh, hd ), new Vector3D<float>( -hw, hh, hd ),
            new Vector3D<float>( 0f, 0f, 1f ), white );

        // Задняя грань (обратная сторона)
        AddQuad( vertices, indices,
            new Vector3D<float>( hw, -hh, -hd ), new Vector3D<float>( -hw, -hh, -hd ),
            new Vector3D<float>( -hw, hh, -hd ), new Vector3D<float>( hw, hh, -hd ),
            new Vector3D<float>( 0f, 0f, -1f ), white );

        // Верхняя грань
        AddQuad( vertices, indices,
            new Vector3D<float>( -hw, hh, hd ), new Vector3D<float>( hw, hh, hd ),
            new Vector3D<float>( hw, hh, -hd ), new Vector3D<float>( -hw, hh, -hd ),
            new Vector3D<float>( 0f, 1f, 0f ), white );

        // Нижняя грань
        AddQuad( vertices, indices,
            new Vector3D<float>( -hw, -hh, -hd ), new Vector3D<float>( hw, -hh, -hd ),
            new Vector3D<float>( hw, -hh, hd ), new Vector3D<float>( -hw, -hh, hd ),
            new Vector3D<float>( 0f, -1f, 0f ), white );

        // Правая грань
        AddQuad( vertices, indices,
            new Vector3D<float>( hw, -hh, hd ), new Vector3D<float>( hw, -hh, -hd ),
            new Vector3D<float>( hw, hh, -hd ), new Vector3D<float>( hw, hh, hd ),
            new Vector3D<float>( 1f, 0f, 0f ), white );

        // Левая грань
        AddQuad( vertices, indices,
            new Vector3D<float>( -hw, -hh, -hd ), new Vector3D<float>( -hw, -hh, hd ),
            new Vector3D<float>( -hw, hh, hd ), new Vector3D<float>( -hw, hh, -hd ),
            new Vector3D<float>( -1f, 0f, 0f ), white );

        Mesh mesh = meshSystem.CreateMesh( vertices.ToArray(), indices.ToArray() );
        return mesh;
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

        // Первый треугольник
        indices.Add( baseIndex );
        indices.Add( baseIndex + 1 );
        indices.Add( baseIndex + 2 );

        // Второй треугольник
        indices.Add( baseIndex );
        indices.Add( baseIndex + 2 );
        indices.Add( baseIndex + 3 );
    }
}