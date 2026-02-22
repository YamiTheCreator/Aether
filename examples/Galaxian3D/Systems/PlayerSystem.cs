using Aether.Core;
using Silk.NET.Maths;
using Silk.NET.Input;
using Graphics.Components;
using Graphics.Systems;
using Graphics.Structures;
using Galaxian3D.Components;

namespace Galaxian3D.Systems;

public class PlayerSystem : SystemBase
{
    private readonly InputSystem _inputSystem = new();
    
    protected override void OnUpdate(float deltaTime)
    {
        Input input = World.GetGlobal<Input>();
        
        foreach (Entity entity in World.Filter<Player>().With<Transform>())
        {
            ref Player player = ref World.Get<Player>(entity);
            ref ModelGroup group = ref World.Get<ModelGroup>(entity);
            ref Transform mainTransform = ref World.Get<Transform>(entity);
            
            player.TimeSinceLastShot += deltaTime;
            
            // Movement
            float moveX = 0f;
            if (_inputSystem.IsKeyDown(input, Key.Left) || _inputSystem.IsKeyDown(input, Key.A))
                moveX = -1f;
            if (_inputSystem.IsKeyDown(input, Key.Right) || _inputSystem.IsKeyDown(input, Key.D))
                moveX = 1f;
            
            float newX = mainTransform.Position.X + moveX * player.Speed * deltaTime;
            newX = Math.Clamp(newX, player.MinX, player.MaxX);
            
            Vector3D<float> newPosition = new Vector3D<float>(newX, mainTransform.Position.Y, mainTransform.Position.Z);
            
            // Update all entities in the same group
            foreach (Entity groupEntity in World.Filter<ModelGroup>().With<Transform>())
            {
                ref ModelGroup otherGroup = ref World.Get<ModelGroup>(groupEntity);
                if (otherGroup.GroupId == group.GroupId)
                {
                    ref Transform transform = ref World.Get<Transform>(groupEntity);
                    transform.Position = newPosition;
                }
            }
            
            // Shooting
            if ((_inputSystem.IsKeyDown(input, Key.Space) || _inputSystem.IsKeyDown(input, Key.W) || _inputSystem.IsKeyDown(input, Key.Up)) 
                && player.TimeSinceLastShot >= player.FireCooldown)
            {
                CreateBullet(newPosition);
                player.TimeSinceLastShot = 0f;
            }
        }
    }
    
    private void CreateBullet(Vector3D<float> position)
    {
        MeshSystem meshSystem = World.GetGlobal<MeshSystem>();
        Material material = World.GetGlobal<Material>();
        
        Entity bullet = World.Spawn();
        
        World.Add(bullet, new Transform
        {
            Position = position + new Vector3D<float>(0, 0, -0.5f),
            Rotation = Quaternion<float>.Identity,
            Scale = new Vector3D<float>(0.1f, 0.1f, 0.3f)
        });
        
        World.Add(bullet, new Bullet
        {
            Speed = 15f,
            IsPlayerBullet = true
        });
        
        // Simple cube mesh for bullet
        List<Vertex> vertices = new();
        List<uint> indices = new();
        CreateSimpleCube(vertices, indices);
        World.Add(bullet, meshSystem.CreateMesh(vertices.ToArray(), indices.ToArray(), material));
    }
    
    private void CreateSimpleCube(List<Vertex> vertices, List<uint> indices)
    {
        Vector4D<float> white = new(1, 1, 1, 1);
        float h = 0.5f;
        
        // Front
        vertices.Add(new Vertex(new Vector3D<float>(-h, -h, h), Vector2D<float>.Zero, white, 0, new Vector3D<float>(0, 0, 1)));
        vertices.Add(new Vertex(new Vector3D<float>(h, -h, h), Vector2D<float>.Zero, white, 0, new Vector3D<float>(0, 0, 1)));
        vertices.Add(new Vertex(new Vector3D<float>(h, h, h), Vector2D<float>.Zero, white, 0, new Vector3D<float>(0, 0, 1)));
        vertices.Add(new Vertex(new Vector3D<float>(-h, h, h), Vector2D<float>.Zero, white, 0, new Vector3D<float>(0, 0, 1)));
        
        // Back
        vertices.Add(new Vertex(new Vector3D<float>(h, -h, -h), Vector2D<float>.Zero, white, 0, new Vector3D<float>(0, 0, -1)));
        vertices.Add(new Vertex(new Vector3D<float>(-h, -h, -h), Vector2D<float>.Zero, white, 0, new Vector3D<float>(0, 0, -1)));
        vertices.Add(new Vertex(new Vector3D<float>(-h, h, -h), Vector2D<float>.Zero, white, 0, new Vector3D<float>(0, 0, -1)));
        vertices.Add(new Vertex(new Vector3D<float>(h, h, -h), Vector2D<float>.Zero, white, 0, new Vector3D<float>(0, 0, -1)));
        
        indices.AddRange([0, 1, 2, 0, 2, 3, 4, 5, 6, 4, 6, 7]);
    }
}
