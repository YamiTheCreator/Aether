using Aether.Core;
using Graphics.Components;
using Graphics.Systems;
using MemoryTrainer.Components;
using Silk.NET.Maths;

namespace MemoryTrainer.Systems;

public class CardRenderSystem : SystemBase
{
    private readonly Texture2D[] _frontTextures = new Texture2D[ 8 ];
    private Texture2D _backTexture;

    protected override void OnInit()
    {
        TextureSystem textureSystem = World.GetGlobal<TextureSystem>();

        // Загружаем текстуры
        string projectRoot = "/Users/yami/Documents/RiderProjects/Aether";

        for ( int i = 0; i < 8; i++ )
        {
            string texturePath = $"{projectRoot}/examples/MemoryTrainer/Textures/{i + 1}.jpg";
            _frontTextures[ i ] = textureSystem.CreateTextureFromFile( texturePath );
        }

        // Создаем текстуру для обратной стороны
        _backTexture = textureSystem.CreateTextureFromColor( 1, 1, 50, 100, 200 );

        World.SetGlobal( _frontTextures );
        World.SetGlobal( _backTexture );
    }

    protected override void OnUpdate( float deltaTime )
    {
        // Обновляем материалы карт в зависимости от их состояния
        foreach ( Entity entity in World.Filter<Card>().With<Transform>() )
        {
            if ( !World.Has<Material>( entity ) )
                continue;

            Card card = World.Get<Card>( entity );
            ref Material material = ref World.Get<Material>( entity );

            // Определяем, какую сторону показывать
            // Если карта открыта (IsRevealed=true) или открывается (IsFlipping && FlipToFront), показываем лицевую сторону
            bool showFront = card.IsRevealed ||
                             ( card.IsFlipping && card is { FlipToFront: true, FlipProgress: > 0.5f } );

            // Обновляем текстуру материала
            if ( showFront )
            {
                material.Texture = _frontTextures[ card.TextureIndex ];
            }
            else
            {
                material.Texture = _backTexture;
            }
        }
    }
}