using Aether.Core;
using Graphics.Systems;
using MemoryTrainer.Components;
using MemoryTrainer.Helpers;
using Silk.NET.Maths;

namespace MemoryTrainer.Systems;

public class GameManagerSystem : SystemBase
{
    private MeshSystem? _meshSystem;
    private TextureSystem? _textureSystem;

    protected override void OnCreate()
    {
        _meshSystem = World.GetGlobal<MeshSystem>();
        _textureSystem = World.GetGlobal<TextureSystem>();
    }

    protected override void OnUpdate( float deltaTime )
    {
        GameState gameState = World.GetGlobal<GameState>();

        if ( gameState.RestartRequested )
        {
            RestartGame();
            gameState.RestartRequested = false;
            World.SetGlobal( gameState );
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

        RemoveAllCards();
        ResetGameState();
        CreateNewCards();
    }

    private void RemoveAllCards()
    {
        List<Entity> cardsToRemove = [ ];
        foreach ( Entity entity in World.Filter<Card>() )
        {
            cardsToRemove.Add( entity );
        }

        foreach ( Entity entity in cardsToRemove )
        {
            World.Despawn( entity );
        }
    }

    private void ResetGameState()
    {
        GameState gameState = World.GetGlobal<GameState>();
        gameState.FirstRevealedCard = null;
        gameState.SecondRevealedCard = null;
        gameState.MatchedPairs = 0;
        gameState.Moves = 0;
        gameState.DelayTimer = 0f;
        gameState.IsWaitingForFlipBack = false;
        gameState.ClickedCard = null;
        World.SetGlobal( gameState );
    }

    private void CreateNewCards()
    {
        GameState gameState = World.GetGlobal<GameState>();
        List<int> pairIds = GenerateShuffledPairs( gameState.GridRows * gameState.GridCols );
        PlaceCards( pairIds, gameState.GridRows, gameState.GridCols );
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

    private void PlaceCards( List<int> pairIds, int rows, int cols )
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
                    position, cardSize, cardSize, cardDepth, _meshSystem!, _textureSystem! );

                cardIndex++;
            }
        }
    }
}
