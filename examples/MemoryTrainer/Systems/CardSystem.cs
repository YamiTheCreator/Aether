using Aether.Core;
using Graphics.Components;
using Graphics.Systems;
using MemoryTrainer.Components;
using Silk.NET.Maths;

namespace MemoryTrainer.Systems;

public class CardSystem : SystemBase
{
    private Texture2D[]? _frontTextures;
    private Texture2D? _backTexture;

    protected override void OnCreate()
    {
        LoadTextures();
    }

    protected override void OnUpdate( float deltaTime )
    {
        GameState gameState = World.GetGlobal<GameState>();

        HandleFlipBackDelay( deltaTime, ref gameState );
        HandleCardClick( ref gameState );

        foreach ( Entity entity in World.Filter<Card>().With<Transform>() )
        {
            ref Card card = ref World.Get<Card>( entity );
            ref Transform transform = ref World.Get<Transform>( entity );

            UpdateCardFlip( ref card, ref transform, deltaTime );
            UpdateCardTexture( entity, card );
        }

        World.SetGlobal( gameState );
    }

    protected override void OnRender()
    {
    }

    protected override void OnDestroy()
    {
    }

    private void LoadTextures()
    {
        TextureSystem textureSystem = World.GetGlobal<TextureSystem>();

        _frontTextures = new Texture2D[ 8 ];
        string projectRoot = "/Users/yami/Documents/RiderProjects/Aether";

        for ( int i = 0; i < 8; i++ )
        {
            string texturePath = $"{projectRoot}/examples/MemoryTrainer/Textures/{i + 1}.jpg";
            _frontTextures[ i ] = textureSystem.CreateTextureFromFile( texturePath );
        }

        _backTexture = textureSystem.CreateTextureFromColor( 1, 1, 50, 100, 200 );
    }

    private void UpdateCardFlip( ref Card card, ref Transform transform, float deltaTime )
    {
        if ( !card.IsFlipping )
            return;

        card.FlipProgress += card.FlipSpeed * deltaTime;

        if ( card.FlipProgress >= 1f )
        {
            card.FlipProgress = 1f;
            card.IsFlipping = false;
            card.IsRevealed = card.FlipToFront;
        }

        float flipAngle = card.FlipProgress * MathF.PI;

        Quaternion<float> baseRotation = Quaternion<float>.CreateFromAxisAngle(
            new Vector3D<float>( 1f, 0f, 0f ),
            90f * MathF.PI / 180f
        );

        Quaternion<float> flipRotation = Quaternion<float>.CreateFromAxisAngle(
            new Vector3D<float>( 1f, 0f, 0f ),
            card.FlipToFront ? flipAngle : -flipAngle
        );

        transform.Rotation = flipRotation * baseRotation;
    }

    private void UpdateCardTexture( Entity entity, Card card )
    {
        if ( _frontTextures is null || _backTexture is null )
            return;

        if ( !World.Has<Material>( entity ) )
            return;

        ref Material material = ref World.Get<Material>( entity );

        bool showFront = card.IsRevealed ||
                         ( card.IsFlipping && card is { FlipToFront: true, FlipProgress: > 0.5f } );

        material.Texture = showFront ? _frontTextures[ card.TextureIndex ] : _backTexture.Value;
    }

    private void HandleFlipBackDelay( float deltaTime, ref GameState gameState )
    {
        if ( !gameState.IsWaitingForFlipBack )
            return;

        gameState.DelayTimer += deltaTime;

        if ( gameState.DelayTimer >= 1f )
        {
            FlipCardsBack( gameState );
            ResetRevealedCards( ref gameState );
        }
    }

    private void FlipCardsBack( GameState gameState )
    {
        FlipCardBack( gameState.FirstRevealedCard );
        FlipCardBack( gameState.SecondRevealedCard );
    }

    private void FlipCardBack( Entity? cardEntity )
    {
        if ( !cardEntity.HasValue || !World.IsAlive( cardEntity.Value ) )
            return;

        ref Card card = ref World.Get<Card>( cardEntity.Value );
        if ( card.IsMatched )
            return;

        card.IsFlipping = true;
        card.FlipToFront = false;
        card.FlipProgress = 0f;
        card.IsRevealed = false;
    }

    private void ResetRevealedCards( ref GameState gameState )
    {
        gameState.FirstRevealedCard = null;
        gameState.SecondRevealedCard = null;
        gameState.IsWaitingForFlipBack = false;
        gameState.DelayTimer = 0f;
    }

    private void HandleCardClick( ref GameState gameState )
    {
        if ( !gameState.ClickedCard.HasValue )
            return;

        Entity entity = gameState.ClickedCard.Value;
        ref Card card = ref World.Get<Card>( entity );

        if ( !CanRevealCard( card, gameState ) )
        {
            gameState.ClickedCard = null;
            return;
        }

        RevealCard( ref card );

        if ( !gameState.FirstRevealedCard.HasValue )
        {
            gameState.FirstRevealedCard = entity;
        }
        else if ( !gameState.SecondRevealedCard.HasValue )
        {
            gameState.SecondRevealedCard = entity;
            gameState.Moves++;
            CheckForMatch( ref gameState );
        }

        gameState.ClickedCard = null;
    }

    private bool CanRevealCard( Card card, GameState gameState )
    {
        if ( card.IsMatched || card.IsRevealed )
            return false;

        return gameState is not { FirstRevealedCard: not null, SecondRevealedCard: not null };
    }

    private void RevealCard( ref Card card )
    {
        card.IsFlipping = true;
        card.FlipToFront = true;
        card.FlipProgress = 0f;
    }

    private void CheckForMatch( ref GameState gameState )
    {
        ref Card firstCard = ref World.Get<Card>( gameState.FirstRevealedCard!.Value );
        ref Card secondCard = ref World.Get<Card>( gameState.SecondRevealedCard!.Value );

        if ( firstCard.PairId == secondCard.PairId )
        {
            MarkCardsAsMatched( ref firstCard, ref secondCard );
            UpdateMatchedPairs( ref gameState );
            ResetRevealedCards( ref gameState );
        }
        else
        {
            StartFlipBackDelay( ref gameState );
        }
    }

    private void MarkCardsAsMatched( ref Card card1, ref Card card2 )
    {
        card1.IsMatched = true;
        card2.IsMatched = true;
    }

    private void UpdateMatchedPairs( ref GameState gameState )
    {
        gameState.MatchedPairs++;
    }

    private void StartFlipBackDelay( ref GameState gameState )
    {
        gameState.IsWaitingForFlipBack = true;
        gameState.DelayTimer = 0f;
    }
}
