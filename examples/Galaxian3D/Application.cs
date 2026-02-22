using Silk.NET.Maths;
using Aether.Core;
using Aether.Core.Enums;
using Graphics;
using Graphics.Components;
using Graphics.Systems;
using Galaxian3D.Systems;
using Galaxian3D.Components;

namespace Galaxian3D;

public class Application() : ApplicationBase(
    title: "Galaxian 3D",
    width: 1280,
    height: 720,
    createDefaultCamera: false)
{
    protected override void OnInitialize()
    {
        // Create systems
        ShaderSystem shaderSystem = new(WindowBase.Gl);
        TextureSystem textureSystem = new(WindowBase.Gl);
        MaterialSystem materialSystem = new();
        InputSystem inputSystem = new();
        MeshSystem meshSystem = new(WindowBase.Gl);
        ModelImporterSystem modelImporter = new( textureSystem, meshSystem);

        // Create default material
        Texture2D whiteTexture = textureSystem.CreateTextureFromColor(1, 1);
        Material defaultMaterial = new Material
        {
            Texture = whiteTexture,
            DiffuseColor = new Vector3D<float>(1f, 1f, 1f),
            AmbientColor = new Vector3D<float>(0.3f, 0.3f, 0.3f),
            SpecularColor = new Vector3D<float>(0.5f, 0.5f, 0.5f),
            Shininess = 32f,
            Alpha = 1f
        };

        Input input = inputSystem.CreateInput(WindowBase.Input);

        // Set globals
        World.SetGlobal(shaderSystem);
        World.SetGlobal(textureSystem);
        World.SetGlobal(inputSystem);
        World.SetGlobal(meshSystem);
        World.SetGlobal(materialSystem);
        World.SetGlobal(modelImporter);
        World.SetGlobal(whiteTexture);
        World.SetGlobal(input);
        World.SetGlobal(defaultMaterial);

        // Initialize game state
        World.SetGlobal(new GameState
        {
            Score = 0,
            Lives = 3,
            Level = 1,
            IsGameOver = false,
            EnemiesRemaining = 0,
            AttackTimer = 0f
        });

        // Add systems
        World.AddSystem(shaderSystem);
        World.AddSystem(new CameraSystem());
        World.AddSystem(new OrbitCameraSystem());
        World.AddSystem(new LightingSystem());
        World.AddSystem(materialSystem);
        
        // Game systems
        World.AddSystem(new PlayerSystem());
        World.AddSystem(new EnemySystem());
        World.AddSystem(new BulletSystem());
        World.AddSystem(new AttackSystem());
        World.AddSystem(new CollisionSystem());

        World.AddSystem(new RenderSystem(WindowBase.Gl));

        // Create camera - orbit mode for debugging
        Entity cameraEntity = World.Spawn();
        World.Add(cameraEntity, Camera.CreateOrbit(
            target: new Vector3D<float>(0f, 0f, 0f),
            distance: 30f,
            yaw: 0f,
            pitch: -60f,
            aspectRatio: (float)WindowBase.LogicalWidth / WindowBase.LogicalHeight
        ));

        World.Add(cameraEntity, new Transform
        {
            Position = new Vector3D<float>(0f, 25f, 5f),
            Rotation = Quaternion<float>.Identity,
            Scale = Vector3D<float>.One
        });

        // Create game objects
        CreatePlayer();
        CreateEnemyFormation();
        CreateLights();

        Console.WriteLine("Galaxian 3D");
        Console.WriteLine("Controls: Arrow Keys / A-D - Move, Space / W / Up - Shoot");
        Console.WriteLine("Destroy all enemies to win!");
    }

    private void CreatePlayer()
    {
        ModelImporterSystem modelImporter = World.GetGlobal<ModelImporterSystem>();
        string projectRoot = "/Users/yami/Documents/RiderProjects/Aether";
        string modelPath = $"{projectRoot}/examples/Galaxian3D/Models/spaceship.glb";

        // Load 3D model
        List<Entity> modelEntities = modelImporter.LoadModel(World, modelPath);
        
        if (modelEntities.Count > 0)
        {
            int groupId = modelEntities[0].Id; // Use first entity ID as group ID
            
            // Apply transform and group to ALL model entities
            foreach (Entity entity in modelEntities)
            {
                ref Transform transform = ref World.Get<Transform>(entity);
                transform.Position = new Vector3D<float>(0f, 0f, 9f);
                transform.Scale = new Vector3D<float>(0.15f, 0.15f, 0.15f);
                
                World.Add(entity, new ModelGroup { GroupId = groupId });
            }
            
            // Add Player component only to first entity for control
            World.Add(modelEntities[0], new Player
            {
                Speed = 8f,
                MinX = -8f,
                MaxX = 8f,
                FireCooldown = 0.3f,
                TimeSinceLastShot = 0f
            });
        }
    }

    private void CreateEnemyFormation()
    {
        ModelImporterSystem modelImporter = World.GetGlobal<ModelImporterSystem>();
        string projectRoot = "/Users/yami/Documents/RiderProjects/Aether";
        
        int rows = 4;
        int cols = 8;
        float spacingX = 2f;
        float spacingZ = 1.5f;
        float startX = -(cols - 1) * spacingX / 2f;
        float startZ = -8f;

        GameState state = World.GetGlobal<GameState>();
        int enemyCount = 0;

        for (int row = 0; row < rows; row++)
        {
            int enemyType = row; // Different enemy type per row
            string modelPath = $"{projectRoot}/examples/Galaxian3D/Models/enemy{enemyType + 1}.glb";

            for (int col = 0; col < cols; col++)
            {
                Vector2D<float> gridPos = new Vector2D<float>(
                    startX + col * spacingX,
                    startZ + row * spacingZ
                );

                CreateEnemy(gridPos, enemyType, modelPath);
                enemyCount++;
            }
        }

        state.EnemiesRemaining = enemyCount;
        World.SetGlobal(state);
    }

    private void CreateEnemy(Vector2D<float> gridPosition, int type, string modelPath)
    {
        ModelImporterSystem modelImporter = World.GetGlobal<ModelImporterSystem>();
        Random random = new();

        // Load 3D model
        List<Entity> modelEntities = modelImporter.LoadModel(World, modelPath);
        
        if (modelEntities.Count > 0)
        {
            int groupId = modelEntities[0].Id; // Use first entity ID as group ID
            
            // Apply transform and group to ALL model entities
            foreach (Entity entity in modelEntities)
            {
                ref Transform transform = ref World.Get<Transform>(entity);
                transform.Position = new Vector3D<float>(gridPosition.X, 0f, gridPosition.Y);
                transform.Scale = new Vector3D<float>(0.12f, 0.12f, 0.12f);
                
                World.Add(entity, new ModelGroup { GroupId = groupId });
            }
            
            // Add Enemy component only to first entity for control
            World.Add(modelEntities[0], new Enemy
            {
                Type = type,
                GridPosition = gridPosition,
                SwayAmplitude = 0.3f,
                SwaySpeed = 1f + random.NextSingle() * 0.5f,
                SwayPhase = random.NextSingle() * MathF.PI * 2f,
                IsAttacking = false,
                AttackTarget = Vector2D<float>.Zero,
                AttackSpeed = 5f
            });
        }
    }

    private void CreateLights()
    {
        LightingSystem lightingSystem = World.GetSystem<LightingSystem>()!;

        // Main light from above
        Entity light1 = World.Spawn();
        World.Add(light1, lightingSystem.CreatePointFull(
            ambientColor: new Vector3D<float>(0.3f, 0.3f, 0.4f),
            diffuseColor: new Vector3D<float>(1f, 1f, 1f),
            specularColor: new Vector3D<float>(1f, 1f, 1f),
            intensity: 5f,
            range: 40f
        ));
        World.Add(light1, new Transform
        {
            Position = new Vector3D<float>(0f, 15f, 0f),
            Rotation = Quaternion<float>.Identity,
            Scale = Vector3D<float>.One
        });

        // Side lights for depth
        Entity light2 = World.Spawn();
        World.Add(light2, lightingSystem.CreatePointFull(
            ambientColor: new Vector3D<float>(0.2f, 0.2f, 0.3f),
            diffuseColor: new Vector3D<float>(0.5f, 0.7f, 1f),
            specularColor: new Vector3D<float>(1f, 1f, 1f),
            intensity: 3f,
            range: 30f
        ));
        World.Add(light2, new Transform
        {
            Position = new Vector3D<float>(-10f, 10f, 0f),
            Rotation = Quaternion<float>.Identity,
            Scale = Vector3D<float>.One
        });

        Entity light3 = World.Spawn();
        World.Add(light3, lightingSystem.CreatePointFull(
            ambientColor: new Vector3D<float>(0.2f, 0.2f, 0.3f),
            diffuseColor: new Vector3D<float>(1f, 0.7f, 0.5f),
            specularColor: new Vector3D<float>(1f, 1f, 1f),
            intensity: 3f,
            range: 30f
        ));
        World.Add(light3, new Transform
        {
            Position = new Vector3D<float>(10f, 10f, 0f),
            Rotation = Quaternion<float>.Identity,
            Scale = Vector3D<float>.One
        });
    }
}
