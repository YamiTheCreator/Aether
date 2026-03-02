using Silk.NET.Maths;
using Aether.Core;
using Graphics;
using Graphics.Components;
using Graphics.Systems;
using Graphics.Structures;
using MemoryTrainer.Systems;
using MemoryTrainer.Components;
using MemoryTrainer.Helpers;

namespace MemoryTrainer;

public class Application() : ApplicationBase(
    title: "Memory Trainer 3D",
    width: 1280,
    height: 720 )
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
        World.AddSystem( inputSystem );
        World.AddSystem( new CameraSystem() );
        World.AddSystem( new LightingSystem() );
        World.AddSystem( materialSystem );
        World.AddSystem( new CardInputSystem() );
        World.AddSystem( new GameManagerSystem() );
        World.AddSystem( new CardSystem() );
        World.AddSystem( meshSystem );

        CreateCamera();
        CreateLights();
        CreateGameBoard();
    }

    private void CreateCamera()
    {
        CameraSystem.CreatePerspectiveCamera(
            World,
            position: new Vector3D<float>( 0f, 5f, 3f ),
            yaw: -90f,
            pitch: -60f,
            fov: 45f,
            aspectRatio: ( float )WindowBase.LogicalWidth / WindowBase.LogicalHeight
        );
    }

    private void CreateLights()
    {
        LightingSystem lightingSystem = World.GetSystem<LightingSystem>()!;

        CreateLight( lightingSystem,
            position: new Vector3D<float>( 0f, 10f, 5f ),
            ambientColor: new Vector3D<float>( 0.3f, 0.3f, 0.3f ),
            diffuseColor: new Vector3D<float>( 1f, 1f, 1f ),
            specularColor: new Vector3D<float>( 1f, 1f, 1f ),
            intensity: 20f,
            range: 50f );

        CreateLight( lightingSystem,
            position: new Vector3D<float>( -5f, 5f, 8f ),
            ambientColor: new Vector3D<float>( 0.2f, 0.2f, 0.2f ),
            diffuseColor: new Vector3D<float>( 0.8f, 0.8f, 1f ),
            specularColor: new Vector3D<float>( 0.5f, 0.5f, 0.5f ),
            intensity: 3f,
            range: 30f );
    }

    private void CreateLight( LightingSystem lightingSystem, Vector3D<float> position,
        Vector3D<float> ambientColor, Vector3D<float> diffuseColor, Vector3D<float> specularColor,
        float intensity, float range )
    {
        Entity light = World.Spawn();
        World.Add( light, lightingSystem.CreatePointFull(
            ambientColor: ambientColor,
            diffuseColor: diffuseColor,
            specularColor: specularColor,
            intensity: intensity,
            range: range
        ) );
        World.Add( light, new Transform
        {
            Position = position,
            Rotation = Quaternion<float>.Identity,
            Scale = Vector3D<float>.One
        } );
    }

    private void CreateGameBoard()
    {
        GameState gameState = World.GetGlobal<GameState>();
        MeshSystem meshSystem = World.GetGlobal<MeshSystem>();
        TextureSystem textureSystem = World.GetGlobal<TextureSystem>();

        List<int> pairIds = GenerateShuffledPairs( gameState.GridRows * gameState.GridCols );
        PlaceCards( pairIds, gameState.GridRows, gameState.GridCols, meshSystem, textureSystem );
    }

    private List<int> GenerateShuffledPairs( int totalCards )
    {
        List<int> pairIds = [ ];
        for ( int i = 0; i < totalCards / 2; i++ )
        {
            pairIds.Add( i );
            pairIds.Add( i );
        }

        ShufflePairs( pairIds );
        return pairIds;
    }

    private void ShufflePairs( List<int> pairIds )
    {
        Random random = new();
        for ( int i = pairIds.Count - 1; i > 0; i-- )
        {
            int j = random.Next( i + 1 );
            ( pairIds[ i ], pairIds[ j ] ) = ( pairIds[ j ], pairIds[ i ] );
        }
    }

    private void PlaceCards( List<int> pairIds, int rows, int cols, MeshSystem meshSystem, TextureSystem textureSystem )
    {
        const float cardSize = 0.9f;
        const float cardDepth = 0.05f;
        const float spacing = 1.0f;

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

                CardFactory.CreateCard( World, cardIndex, pairId, textureIndex,
                    position, cardSize, cardSize, cardDepth, meshSystem, textureSystem );

                cardIndex++;
            }
        }
    }



}