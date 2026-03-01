using Aether.Core;
using Silk.NET.Maths;
using Graphics.Components;
using Galaxian3D.Components;

namespace Galaxian3D.Systems;

public class EnemySystem : SystemBase
{
    private float _time = 0f;
    
    protected override void OnUpdate(float deltaTime)
    {
        _time += deltaTime;
        
        foreach (Entity entity in World.Filter<Enemy>().With<Transform>())
        {
            ref Enemy enemy = ref World.Get<Enemy>(entity);
            ref ModelGroup group = ref World.Get<ModelGroup>(entity);
            
            Vector3D<float> newPosition;
            Quaternion<float> newRotation;
            
            if (enemy.IsAttacking)
            {
                // Attack dive movement
                ref Transform mainTransform = ref World.Get<Transform>(entity);
                Vector2D<float> currentPos = new(mainTransform.Position.X, mainTransform.Position.Z);
                Vector2D<float> direction = Vector2D.Normalize(enemy.AttackTarget - currentPos);
                
                currentPos += direction * enemy.AttackSpeed * deltaTime;
                newPosition = new Vector3D<float>(currentPos.X, 0f, currentPos.Y);
                
                // Add rotation during attack
                newRotation = Quaternion<float>.CreateFromAxisAngle(
                    Vector3D<float>.UnitY, 
                    _time * 3f
                );
                
                // Check if reached bottom or target
                if (newPosition.Z > 10f)
                {
                    // Return to formation
                    enemy.IsAttacking = false;
                    newPosition = new Vector3D<float>(
                        enemy.GridPosition.X,
                        0f,
                        enemy.GridPosition.Y
                    );
                    newRotation = Quaternion<float>.Identity;
                }
            }
            else
            {
                // Formation sway
                float sway = MathF.Sin(_time * enemy.SwaySpeed + enemy.SwayPhase) * enemy.SwayAmplitude;
                newPosition = new Vector3D<float>(
                    enemy.GridPosition.X + sway,
                    0f,
                    enemy.GridPosition.Y
                );
                
                // Gentle rotation
                newRotation = Quaternion<float>.CreateFromAxisAngle(
                    Vector3D<float>.UnitY,
                    MathF.Sin(_time * 0.5f + enemy.SwayPhase) * 0.2f
                );
            }
            
            // Update all entities in the same group
            foreach (Entity groupEntity in World.Filter<ModelGroup>().With<Transform>())
            {
                ref ModelGroup otherGroup = ref World.Get<ModelGroup>(groupEntity);
                if (otherGroup.GroupId == group.GroupId)
                {
                    ref Transform transform = ref World.Get<Transform>(groupEntity);
                    transform.Position = newPosition;
                    transform.Rotation = newRotation;
                }
            }
        }
    }
}
