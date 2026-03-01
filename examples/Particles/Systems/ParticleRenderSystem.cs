using Aether.Core;
using Silk.NET.Maths;
using Graphics.Components;
using Graphics.Systems;
using Particles.Components;

namespace Particles.Systems;

public class ParticleRenderSystem : SystemBase
{
    private readonly Dictionary<Entity, Entity> _particleToSprite = new();
    private Texture2D _whiteTexture;

    protected override void OnInit()
    {
        TextureSystem textureSystem = World.GetGlobal<TextureSystem>();
        _whiteTexture = textureSystem.CreateTextureFromColor( 1, 1 );

        CreateBoundary();
    }

    protected override void OnUpdate( float deltaTime )
    {
        // Обновляем или создаем спрайты для частиц
        HashSet<Entity> currentParticles = [ ];

        foreach ( Entity particleEntity in World.Filter<ChargedParticle>().With<Transform>() )
        {
            currentParticles.Add( particleEntity );

            ref Transform particleTransform = ref World.Get<Transform>( particleEntity );
            ref ChargedParticle particle = ref World.Get<ChargedParticle>( particleEntity );

            // Если спрайт уже существует, обновляем его позицию
            if ( _particleToSprite.TryGetValue( particleEntity, out Entity spriteEntity ) &&
                 World.IsAlive( spriteEntity ) )
            {
                ref Transform spriteTransform = ref World.Get<Transform>( spriteEntity );
                spriteTransform.Position = particleTransform.Position;

                ref Sprite sprite = ref World.Get<Sprite>( spriteEntity );
                sprite.Color = particle.Color;
            }
            else
            {
                // Создаем новый спрайт
                Entity newSprite = CreateParticleCircle( particleTransform.Position, particle.Radius, particle.Color );
                _particleToSprite[ particleEntity ] = newSprite;
            }
        }

        // Удаляем спрайты для несуществующих частиц
        List<Entity> toRemove = [ ];
        foreach ( KeyValuePair<Entity, Entity> kvp in _particleToSprite )
        {
            if ( !currentParticles.Contains( kvp.Key ) || !World.IsAlive( kvp.Key ) )
            {
                if ( World.IsAlive( kvp.Value ) )
                {
                    World.Despawn( kvp.Value );
                }

                toRemove.Add( kvp.Key );
            }
        }

        foreach ( Entity e in toRemove )
        {
            _particleToSprite.Remove( e );
        }
    }

    private Entity CreateParticleCircle( Vector3D<float> position, float radius, Vector4D<float> color )
    {
        Entity circleEntity = World.Spawn();

        Transform transform = new()
        {
            Position = position,
            Rotation = Quaternion<float>.Identity,
            Scale = new Vector3D<float>( 1, 1, 1 )
        };

        float diameter = radius * 2;
        MaterialSystem materialSystem = World.GetSystem<MaterialSystem>()!;
        Material material = materialSystem.CreateTextured( _whiteTexture, color );
        Sprite sprite = Sprite.Create( material, new Vector2D<float>( diameter, diameter ) );
        sprite.Color = color;

        World.Add( circleEntity, transform );
        World.Add( circleEntity, sprite );

        return circleEntity;
    }

    private void CreateBoundary()
    {
        // Создаем 4 линии для границы
        Vector4D<float> borderColor = new( 0.3f, 0.3f, 0.4f, 1.0f );
        float thickness = 0.1f;
        float width = 36.0f;
        float height = 26.0f;

        // Верхняя граница
        CreateBoundaryLine( new Vector3D<float>( 0, height / 2, -1 ), new Vector2D<float>( width, thickness ),
            borderColor );
        // Нижняя граница
        CreateBoundaryLine( new Vector3D<float>( 0, -height / 2, -1 ), new Vector2D<float>( width, thickness ),
            borderColor );
        // Левая граница
        CreateBoundaryLine( new Vector3D<float>( -width / 2, 0, -1 ), new Vector2D<float>( thickness, height ),
            borderColor );
        // Правая граница
        CreateBoundaryLine( new Vector3D<float>( width / 2, 0, -1 ), new Vector2D<float>( thickness, height ),
            borderColor );
    }

    private void CreateBoundaryLine( Vector3D<float> position, Vector2D<float> size, Vector4D<float> color )
    {
        Entity lineEntity = World.Spawn();

        Transform transform = new()
        {
            Position = position,
            Rotation = Quaternion<float>.Identity,
            Scale = new Vector3D<float>( 1, 1, 1 )
        };

        MaterialSystem materialSystem = World.GetSystem<MaterialSystem>()!;
        Material material = materialSystem.CreateTextured( _whiteTexture, color );
        Sprite sprite = Sprite.Create( material, size );
        sprite.Color = color;

        World.Add( lineEntity, transform );
        World.Add( lineEntity, sprite );
    }
}