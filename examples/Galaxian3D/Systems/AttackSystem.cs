using Aether.Core;
using Silk.NET.Maths;
using Graphics.Components;
using Galaxian3D.Components;

namespace Galaxian3D.Systems;

public class AttackSystem : SystemBase
{
    private Random _random = new();
    
    protected override void OnUpdate(float deltaTime)
    {
        GameState state = World.GetGlobal<GameState>();
        state.AttackTimer += deltaTime;
        
        // Trigger attack every 2-4 seconds
        if (state.AttackTimer > 2f + _random.NextSingle() * 2f)
        {
            state.AttackTimer = 0f;
            
            // Find a random non-attacking enemy
            List<Entity> availableEnemies = new();
            foreach (Entity entity in World.Filter<Enemy>())
            {
                ref Enemy enemy = ref World.Get<Enemy>(entity);
                if (!enemy.IsAttacking)
                    availableEnemies.Add(entity);
            }
            
            if (availableEnemies.Count > 0)
            {
                Entity attackerEntity = availableEnemies[_random.Next(availableEnemies.Count)];
                ref Enemy attacker = ref World.Get<Enemy>(attackerEntity);
                
                // Get player position as target
                foreach (Entity playerEntity in World.Filter<Player>().With<Transform>())
                {
                    ref Transform playerTransform = ref World.Get<Transform>(playerEntity);
                    attacker.IsAttacking = true;
                    attacker.AttackTarget = new Vector2D<float>(
                        playerTransform.Position.X,
                        playerTransform.Position.Z + 5f
                    );
                    attacker.AttackSpeed = 5f + _random.NextSingle() * 3f;
                    break;
                }
            }
        }
        
        World.SetGlobal(state);
    }
}
