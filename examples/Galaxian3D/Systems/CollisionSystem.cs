using Aether.Core;
using Silk.NET.Maths;
using Graphics.Components;
using Galaxian3D.Components;

namespace Galaxian3D.Systems;

public class CollisionSystem : SystemBase
{
    protected override void OnUpdate(float deltaTime)
    {
        List<Entity> bulletsToRemove = new();
        List<Entity> enemiesToRemove = new();
        
        // Check bullet-enemy collisions
        foreach (Entity bulletEntity in World.Filter<Bullet>().With<Transform>())
        {
            ref Bullet bullet = ref World.Get<Bullet>(bulletEntity);
            if (!bullet.IsPlayerBullet) continue;
            
            ref Transform bulletTransform = ref World.Get<Transform>(bulletEntity);
            
            foreach (Entity enemyEntity in World.Filter<Enemy>().With<Transform>())
            {
                ref Transform enemyTransform = ref World.Get<Transform>(enemyEntity);
                
                float distance = Vector3D.Distance(bulletTransform.Position, enemyTransform.Position);
                if (distance < 0.8f)
                {
                    bulletsToRemove.Add(bulletEntity);
                    enemiesToRemove.Add(enemyEntity);
                    
                    // Update score
                    GameState state = World.GetGlobal<GameState>();
                    state.Score += 100;
                    state.EnemiesRemaining--;
                    World.SetGlobal(state);
                    
                    break;
                }
            }
        }
        
        // Check enemy-player collisions
        foreach (Entity enemyEntity in World.Filter<Enemy>().With<Transform>())
        {
            ref Enemy enemy = ref World.Get<Enemy>(enemyEntity);
            if (!enemy.IsAttacking) continue;
            
            ref Transform enemyTransform = ref World.Get<Transform>(enemyEntity);
            
            foreach (Entity playerEntity in World.Filter<Player>().With<Transform>())
            {
                ref Transform playerTransform = ref World.Get<Transform>(playerEntity);
                
                float distance = Vector3D.Distance(enemyTransform.Position, playerTransform.Position);
                if (distance < 1f)
                {
                    GameState state = World.GetGlobal<GameState>();
                    state.Lives--;
                    if (state.Lives <= 0)
                        state.IsGameOver = true;
                    World.SetGlobal(state);
                    
                    enemiesToRemove.Add(enemyEntity);
                    break;
                }
            }
        }
        
        // Remove entities
        foreach (Entity entity in bulletsToRemove)
            World.Despawn(entity);
        foreach (Entity entity in enemiesToRemove)
            World.Despawn(entity);
    }
}
