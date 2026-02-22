using Silk.NET.Maths;
using Aether.Core;
using Aether.Core.Enums;
using Graphics;
using Graphics.Components;
using Graphics.Systems;
using Graphics.Structures;

namespace Chess3D;

public class Application() : ApplicationBase(
    title: "Chess 3D",
    width: 1280,
    height: 720 )
{
    protected override void OnInitialize()
    {
        ShaderSystem shaderSystem = new( WindowBase.Gl );
        TextureSystem textureSystem = new( WindowBase.Gl );
        MaterialSystem materialSystem = new();
        InputSystem inputSystem = new();
        MeshSystem meshSystem = new( WindowBase.Gl );
        ModelImporterSystem modelImporterSystem = new( textureSystem, meshSystem );

        Texture2D whiteTexture = textureSystem.CreateTextureFromColor( 1, 1 );

        ShaderProgram shaderProgram = new( WindowBase.Gl );
        Shader shader = new()
        {
            Program = shaderProgram
        };

        Input input = inputSystem.CreateInput( WindowBase.Input );

        World.SetGlobal( shaderSystem );
        World.SetGlobal( textureSystem );
        World.SetGlobal( inputSystem );
        World.SetGlobal( meshSystem );
        World.SetGlobal( materialSystem );
        World.SetGlobal( modelImporterSystem );
        World.SetGlobal( whiteTexture );
        World.SetGlobal( shader );
        World.SetGlobal( input );

        World.AddSystem( shaderSystem );
        World.AddSystem( inputSystem );
        World.AddSystem( new CameraSystem() );
        World.AddSystem( new LightingSystem() );
        World.AddSystem( materialSystem );
        World.AddSystem( meshSystem );

        // Создаем перспективную камеру
        CameraSystem.CreatePerspectiveCamera(
            World,
            position: new Vector3D<float>( 0f, 0f, 5f ),
            yaw: -90f,
            pitch: 0f,
            fov: 45f,
            aspectRatio: ( float )WindowBase.LogicalWidth / WindowBase.LogicalHeight
        );

        LoadChessModel();
        CreateLights();
    }

    private void LoadChessModel()
    {
        ModelImporterSystem modelImporterSystem = World.GetGlobal<ModelImporterSystem>();

        string projectRoot = "/Users/yami/Documents/RiderProjects/Aether";
        string modelPath = Path.Combine( projectRoot, "src/Graphics/Assets/Models/chess.glb" );

        if ( !File.Exists( modelPath ) )
        {
            Console.WriteLine( $"Chess model not found: {modelPath}" );
            return;
        }

        try
        {
            List<Entity> entities = modelImporterSystem.LoadModel( World, modelPath );
            Console.WriteLine( $"Loaded chess model: {entities.Count} meshes" );
        }
        catch ( Exception ex )
        {
            Console.WriteLine( $"Error loading chess model: {ex.Message}" );
        }
    }

    private void CreateLights()
    {
        LightingSystem lightingSystem = World.GetSystem<LightingSystem>()!;

        Entity light1 = World.Spawn();
        World.Add( light1, lightingSystem.CreatePointFull(
            ambientColor: new Vector3D<float>( 0.3f, 0.3f, 0.4f ),
            diffuseColor: new Vector3D<float>( 0.5f, 0.7f, 1.0f ),
            specularColor: new Vector3D<float>( 0.8f, 0.9f, 1f ),
            intensity: 200f,
            range: 500f
        ) );
        World.Add( light1, new Transform
        {
            Position = new Vector3D<float>( 0f, 10f, 0f ),
            Rotation = Quaternion<float>.Identity,
            Scale = Vector3D<float>.One
        } );

        Entity light2 = World.Spawn();
        World.Add( light2, lightingSystem.CreatePoint(
            diffuseColor: new Vector3D<float>( 1.0f, 0.4f, 0.3f ),
            intensity: 150f,
            range: 300f
        ) );
        World.Add( light2, new Transform
        {
            Position = new Vector3D<float>( -5f, 8f, 5f ),
            Rotation = Quaternion<float>.Identity,
            Scale = Vector3D<float>.One
        } );
    }
}