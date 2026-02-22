using Silk.NET.Maths;
using Aether.Core;
using Aether.Core.Enums;
using Graphics;
using Graphics.Components;
using Graphics.Systems;
using Graphics.Structures;
using Camera3D.Systems;
using Camera3D.Components;

namespace Camera3D;

public class Application() : ApplicationBase(
    title: "Basic",
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
        World.SetGlobal( whiteTexture );
        World.SetGlobal( shader );
        World.SetGlobal( input );

        World.AddSystem( shaderSystem );
        World.AddSystem( inputSystem );
        World.AddSystem( new CameraSystem() );
        World.AddSystem( new LightingSystem() );
        World.AddSystem( materialSystem );

        World.AddSystem( new StellatedDodecahedronSystem() );
        World.AddSystem( new KleinBottleSystem() );

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

        CreateStellatedDodecahedron();
        CreateKleinBottle();
        CreateLights();
    }

    private void CreateStellatedDodecahedron()
    {
        Entity entity = World.Spawn();
        World.Add( entity, new Transform
        {
            Position = Vector3D<float>.Zero,
            Rotation = Quaternion<float>.Identity,
            Scale = Vector3D<float>.One
        } );
        World.Add( entity, new StellatedDodecahedron
        {
            Radius = 1.5f,
            StellationHeight = 1.2f,
            IsGenerated = false
        } );
    }

    private void CreateKleinBottle()
    {
        Entity entity = World.Spawn();
        World.Add( entity, new Transform
        {
            Position = new Vector3D<float>( 6, 0, 0 ),
            Rotation = Quaternion<float>.Identity,
            Scale = Vector3D<float>.One
        } );
        World.Add( entity, new KleinBottle
        {
            USegments = 512,
            VSegments = 512,
            Scale = 1.0f,
            IsGenerated = false
        } );
    }

    private void CreateLights()
    {
        LightingSystem lightingSystem = World.GetSystem<LightingSystem>()!;

        Entity light1 = World.Spawn();
        World.Add( light1, lightingSystem.CreatePoint(
            diffuseColor: new Vector3D<float>( 1.0f, 1.0f, 1.0f ),
            intensity: 100.0f,
            range: 50.0f
        ) );
        World.Add( light1, new Transform
        {
            Position = new Vector3D<float>( 0f, 5f, 5f ),
            Rotation = Quaternion<float>.Identity,
            Scale = Vector3D<float>.One
        } );

        Entity light2 = World.Spawn();
        World.Add( light2, lightingSystem.CreatePoint(
            diffuseColor: new Vector3D<float>( 0.8f, 0.8f, 1.0f ),
            intensity: 100.0f,
            range: 50.0f
        ) );
        World.Add( light2, new Transform
        {
            Position = new Vector3D<float>( -5f, 3f, 0f ),
            Rotation = Quaternion<float>.Identity,
            Scale = Vector3D<float>.One
        } );
    }
}