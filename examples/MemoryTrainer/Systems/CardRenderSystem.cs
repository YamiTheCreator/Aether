using Aether.Core;
using Graphics.Components;
using Graphics.Systems;
using MemoryTrainer.Components;

namespace MemoryTrainer.Systems;

public class CardRenderSystem : SystemBase
{
    private Texture2D[]? _frontTextures;
    private Texture2D? _backTexture;

    protected override void OnCreate()
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

    protected override void OnUpdate( float deltaTime )
    {
        if ( _frontTextures is null || _backTexture is null )
            return;

        foreach ( Entity entity in World.Filter<Card>() )
        {
            if ( !World.Has<Material>( entity ) )
                continue;

            Card card = World.Get<Card>( entity );
            ref Material material = ref World.Get<Material>( entity );

            bool showFront = card.IsRevealed ||
                             ( card.IsFlipping && card is { FlipToFront: true, FlipProgress: > 0.5f } );

            if ( showFront )
            {
                material.Texture = _frontTextures[ card.TextureIndex ];
            }
            else
            {
                material.Texture = _backTexture.Value;
            }
        }
    }

    protected override void OnRender()
    {
    }

    protected override void OnDestroy()
    {
    }
}