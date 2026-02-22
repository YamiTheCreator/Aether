using Aether.Core;
using Silk.NET.Maths;
using Graphics.Components;
using Galaxian3D.Components;

namespace Galaxian3D.Systems;

public class BulletSystem : SystemBase
{
    protected override void OnUpdate(float deltaTime)
    {
        foreach (Entity entity in World.Filter<Bullet>().With<Transform>())
        {
            ref Bullet bullet = ref World.Get<Bullet>(entity);
            ref Transform transform = ref World.Get<Transform>(entity);
            
            // Move bullet
            float direction = bullet.IsPlayerBullet ? -1f : 1f;
            transform.Position.Z += direction * bullet.Speed * deltaTime;
            
            // Remove if out of bounds
            if (transform.Position.Z < -15f || transform.Position.Z > 12f)
            {
                World.Despawn(entity);
                continue;
            }
        }
    }
}
