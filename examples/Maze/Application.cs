using Silk.NET.Maths;
using Aether.Core;
using Aether.Core.Enums;
using Graphics;
using Graphics.Components;
using Graphics.Systems;
using Graphics.Structures;
using Maze.Systems;
using Maze.Components;

namespace Maze;

public class Application() : ApplicationBase(
    title: "Maze - Textured Sectors",
    width: 1280,
    height: 720,
    createDefaultCamera: false )
{
    protected override void OnInitialize()
    {
        ShaderSystem shaderSystem = new( WindowBase.Gl );
        TextureSystem textureSystem = new( WindowBase.Gl );
        MaterialSystem materialSystem = new();
        InputSystem inputSystem = new();
        MeshSystem meshSystem = new( WindowBase.Gl );

        Texture2D whiteTexture = textureSystem.CreateTextureFromColor( 1, 1 );

        string projectRoot = "/Users/yami/Documents/RiderProjects/Aether";
        string texturesPath = $"{projectRoot}/src/Graphics/Assets/Textures";

        Material brickMaterial = new()
        {
            Texture = textureSystem.CreateTextureFromFile( $"{texturesPath}/alien-slime1-bl/alien-slime1-albedo.png" ),
            NormalMap = textureSystem.CreateTextureFromFile(
                $"{texturesPath}/alien-slime1-bl/alien-slime1-normal-ogl.png" ),
            MetallicMap =
                textureSystem.CreateTextureFromFile( $"{texturesPath}/alien-slime1-bl/alien-slime1-metallic.png" ),
            RoughnessMap =
                textureSystem.CreateTextureFromFile( $"{texturesPath}/alien-slime1-bl/alien-slime1-roughness.png" ),
            AmbientOcclusionMap =
                textureSystem.CreateTextureFromFile( $"{texturesPath}/alien-slime1-bl/alien-slime1-ao.png" ),
            DiffuseColor = new Vector3D<float>( 1f, 1f, 1f ),
            AmbientColor = new Vector3D<float>( 0.3f, 0.3f, 0.3f ),
            SpecularColor = new Vector3D<float>( 0.5f, 0.5f, 0.5f ),
            Shininess = 32f,
            Metallic = 0.0f,
            Roughness = 0.5f,
            Alpha = 1f
        };

        Material stoneMaterial = new()
        {
            Texture = textureSystem.CreateTextureFromFile(
                $"{texturesPath}/cloudy-veined-quartz-bl/cloudy-veined-quartz_albedo.png" ),
            NormalMap = textureSystem.CreateTextureFromFile(
                $"{texturesPath}/cloudy-veined-quartz-bl/cloudy-veined-quartz_normal-ogl.png" ),
            MetallicMap =
                textureSystem.CreateTextureFromFile(
                    $"{texturesPath}/cloudy-veined-quartz-bl/cloudy-veined-quartz_metallic.png" ),
            RoughnessMap =
                textureSystem.CreateTextureFromFile(
                    $"{texturesPath}/cloudy-veined-quartz-bl/cloudy-veined-quartz_roughness.png" ),
            AmbientOcclusionMap =
                textureSystem.CreateTextureFromFile(
                    $"{texturesPath}/cloudy-veined-quartz-bl/cloudy-veined-quartz_ao.png" ),
            DiffuseColor = new Vector3D<float>( 1f, 1f, 1f ),
            AmbientColor = new Vector3D<float>( 0.3f, 0.3f, 0.3f ),
            SpecularColor = new Vector3D<float>( 0.5f, 0.5f, 0.5f ),
            Shininess = 32f,
            Metallic = 0.0f,
            Roughness = 0.5f,
            Alpha = 1f
        };

        Material tileMaterial = new()
        {
            Texture = textureSystem.CreateTextureFromFile(
                $"{texturesPath}/dark-wood-stain-bl/dark-wood-stain_albedo.png" ),
            NormalMap = textureSystem.CreateTextureFromFile(
                $"{texturesPath}/dark-wood-stain-bl/dark-wood-stain_normal-ogl.png" ),
            MetallicMap =
                textureSystem.CreateTextureFromFile(
                    $"{texturesPath}/dark-wood-stain-bl/dark-wood-stain_metallic.png" ),
            RoughnessMap =
                textureSystem.CreateTextureFromFile(
                    $"{texturesPath}/dark-wood-stain-bl/dark-wood-stain_roughness.png" ),
            AmbientOcclusionMap =
                textureSystem.CreateTextureFromFile( $"{texturesPath}/dark-wood-stain-bl/dark-wood-stain_ao.png" ),
            DiffuseColor = new Vector3D<float>( 1f, 1f, 1f ),
            AmbientColor = new Vector3D<float>( 0.3f, 0.3f, 0.3f ),
            SpecularColor = new Vector3D<float>( 0.5f, 0.5f, 0.5f ),
            Shininess = 32f,
            Metallic = 0.0f,
            Roughness = 0.5f,
            Alpha = 1f
        };

        Material sandstoneMaterial = new()
        {
            Texture = textureSystem.CreateTextureFromFile(
                $"{texturesPath}/scratched-up-steel-bl/scratched-up-steel_albedo.png" ),
            NormalMap = textureSystem.CreateTextureFromFile(
                $"{texturesPath}/scratched-up-steel-bl/scratched-up-steel_normal-ogl.png" ),
            MetallicMap =
                textureSystem.CreateTextureFromFile(
                    $"{texturesPath}/scratched-up-steel-bl/scratched-up-steel_metallic.png" ),
            RoughnessMap =
                textureSystem.CreateTextureFromFile(
                    $"{texturesPath}/scratched-up-steel-bl/scratched-up-steel_roughness.png" ),
            AmbientOcclusionMap =
                textureSystem.CreateTextureFromFile(
                    $"{texturesPath}/scratched-up-steel-bl/scratched-up-steel_ao.png" ),
            DiffuseColor = new Vector3D<float>( 1f, 1f, 1f ),
            AmbientColor = new Vector3D<float>( 0.3f, 0.3f, 0.3f ),
            SpecularColor = new Vector3D<float>( 0.5f, 0.5f, 0.5f ),
            Shininess = 32f,
            Metallic = 0.0f,
            Roughness = 0.5f,
            Alpha = 1f
        };

        Material grassMaterial = new()
        {
            Texture = textureSystem.CreateTextureFromFile(
                $"{texturesPath}/angled-blocks-vegetation-bl/angled-blocks-vegetation_albedo.png" ),
            NormalMap = textureSystem.CreateTextureFromFile(
                $"{texturesPath}/angled-blocks-vegetation-bl/angled-blocks-vegetation_normal-ogl.png" ),
            MetallicMap =
                textureSystem.CreateTextureFromFile(
                    $"{texturesPath}/angled-blocks-vegetation-bl/angled-blocks-vegetation_metallic.png" ),
            RoughnessMap =
                textureSystem.CreateTextureFromFile(
                    $"{texturesPath}/angled-blocks-vegetation-bl/angled-blocks-vegetation_roughness.png" ),
            AmbientOcclusionMap =
                textureSystem.CreateTextureFromFile(
                    $"{texturesPath}/angled-blocks-vegetation-bl/angled-blocks-vegetation_ao.png" ),
            DiffuseColor = new Vector3D<float>( 1f, 1f, 1f ),
            AmbientColor = new Vector3D<float>( 0.3f, 0.3f, 0.3f ),
            SpecularColor = new Vector3D<float>( 0.5f, 0.5f, 0.5f ),
            Shininess = 32f,
            Metallic = 0.0f,
            Roughness = 0.5f,
            Alpha = 1f
        };

        Texture2D skyTexture = textureSystem.CreateTextureFromFile(
            $"{texturesPath}/seamless_8k_pbr_3d_texture_of_twilight_sky_with_sun_halo_and_soft_cirrus_clouds_blender_zip/seamless_8k_pbr_3d_texture_of_twilight_sky_with_sun_halo_and_soft_cirrus_clouds_free_download__BaseColor.png" );

        Input input = inputSystem.CreateInput( WindowBase.Input );

        World.SetGlobal( shaderSystem );
        World.SetGlobal( textureSystem );
        World.SetGlobal( inputSystem );
        World.SetGlobal( meshSystem );
        World.SetGlobal( materialSystem );
        World.SetGlobal( whiteTexture );
        World.SetGlobal( input );

        World.SetGlobal( new MazeMaterials
        {
            BrickMaterial = brickMaterial, // Zone 1 - Alien slime
            StoneMaterial = stoneMaterial, // Zone 2 - Quartz
            TileMaterial = tileMaterial, // Zone 3 - Dark wood
            SandstoneMaterial = sandstoneMaterial, // Zone 4 - Steel
            GrassMaterial = grassMaterial, // Floor - Vegetation
            SkyTexture = skyTexture // Sky - Twilight
        } );

        World.AddSystem( shaderSystem );
        World.AddSystem( new CameraSystem() );
        World.AddSystem( new MazePlayerSystem() );
        World.AddSystem( new LightingSystem() );
        World.AddSystem( materialSystem );

        World.AddSystem( new MazeGridSystem() );
        World.AddSystem( new MazeWallSystem() );
        World.AddSystem( new MazeFloorSystem() );

        World.AddSystem( new RenderSystem( WindowBase.Gl ) );

        Vector3D<float> startPosition = new( 1.5f, 0.5f, 1.5f );

        Entity cameraEntity = World.Spawn();
        World.Add( cameraEntity, new Camera
        {
            ProjectionType = ProjectionType.Perspective,
            FieldOfView = 75f,
            AspectRatio = ( float )WindowBase.LogicalWidth / WindowBase.LogicalHeight,
            NearPlane = 0.1f,
            FarPlane = 100f,
            IsStatic = false,
            Yaw = -90f,
            Pitch = 0f,
            WorldUp = Vector3D<float>.UnitY,
            MovementSpeed = 3f,
            MouseSensitivity = 0.1f
        } );

        World.Add( cameraEntity, new Transform
        {
            Position = startPosition,
            Rotation = Quaternion<float>.Identity,
            Scale = Vector3D<float>.One
        } );

        CreateMazeGrid();
        CreateMazeFloor();
        CreateLights();
        CreateSky();
    }

    private void CreateLights()
    {
        LightingSystem lightingSystem = World.GetSystem<LightingSystem>()!;

        // 4 источника света с краев лабиринта разных цветов
        
        // Красный свет - левый верхний угол
        Entity light1 = World.Spawn();
        World.Add( light1, new Transform
        {
            Position = new Vector3D<float>( 1f, 10f, 1f ),
            Rotation = Quaternion<float>.Identity,
            Scale = Vector3D<float>.One
        } );
        World.Add( light1, lightingSystem.CreatePoint(
            new Vector3D<float>( 1.0f, 0.0f, 0.0f ), // Красный
            intensity: 100.0f
        ) );

        // Зеленый свет - правый верхний угол
        Entity light2 = World.Spawn();
        World.Add( light2, new Transform
        {
            Position = new Vector3D<float>( 14f, 10f, 1f ),
            Rotation = Quaternion<float>.Identity,
            Scale = Vector3D<float>.One
        } );
        World.Add( light2, lightingSystem.CreatePoint(
            new Vector3D<float>( 0.0f, 1.0f, 0.0f ), // Зеленый
            intensity: 100.0f
        ) );

        // Синий свет - левый нижний угол
        Entity light3 = World.Spawn();
        World.Add( light3, new Transform
        {
            Position = new Vector3D<float>( 1f, 10f, 14f ),
            Rotation = Quaternion<float>.Identity,
            Scale = Vector3D<float>.One
        } );
        World.Add( light3, lightingSystem.CreatePoint(
            new Vector3D<float>( 0.0f, 0.0f, 1.0f ), // Синий
            intensity: 100.0f
        ) );

        // Желтый свет - правый нижний угол
        Entity light4 = World.Spawn();
        World.Add( light4, new Transform
        {
            Position = new Vector3D<float>( 14f, 10f, 14f ),
            Rotation = Quaternion<float>.Identity,
            Scale = Vector3D<float>.One
        } );
        World.Add( light4, lightingSystem.CreatePoint(
            new Vector3D<float>( 1.0f, 1.0f, 0.0f ), // Желтый
            intensity: 100.0f
        ) );
    }

    private void CreateMazeGrid()
    {
        int[ , ] mazeLayout = new[ , ]
            {
                { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1 },
                { 1, 1, 1, 1, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1 }, { 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 1 },
                { 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1 }, { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
                { 1, 1, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1 }, { 1, 0, 0, 0, 1, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1 },
                { 1, 0, 1, 0, 1, 0, 1, 0, 1, 1, 1, 0, 1, 1, 1 }, { 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
                { 1, 0, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 0, 1 }, { 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 1 },
                { 1, 1, 1, 0, 1, 1, 1, 0, 1, 0, 1, 0, 1, 1, 1 }, { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
                { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }
            }
            ;

        Entity gridEntity = World.Spawn();
        World.Add( gridEntity, new MazeGrid
        {
            Layout = mazeLayout,
            Width = 15,
            Height = 15,
            IsGenerated = false
        } );
    }

    private void CreateMazeFloor()
    {
        Entity floorEntity = World.Spawn();
        World.Add( floorEntity, new Transform
        {
            Position = new Vector3D<float>( 7.5f, 0f, 7.5f ),
            Rotation = Quaternion<float>.Identity,
            Scale = Vector3D<float>.One
        } );
        World.Add( floorEntity, new MazeFloor
        {
            Size = new Vector2D<float>( 15, 15 ),
            IsGenerated = false
        } );
    }

    private void CreateSky()
    {
        MeshSystem meshSystem = World.GetGlobal<MeshSystem>();
        MaterialSystem materialSystem = World.GetGlobal<MaterialSystem>();
        MazeMaterials materials = World.GetGlobal<MazeMaterials>();

        List<Vertex> vertices = [ ];
        List<uint> indices = [ ];

        Vector3D<float> center = new( 7.5f, 0.5f, 7.5f );
        float radius = 50.0f;
        int segments = 32;
        int rings = 16;

        Vector4D<float> white = new( 1, 1, 1, 1 );

        for ( int ring = 0; ring <= rings; ring++ )
        {
            float phi = MathF.PI * ring / rings;
            float y = MathF.Cos( phi );
            float ringRadius = MathF.Sin( phi );

            for ( int segment = 0; segment <= segments; segment++ )
            {
                float theta = 2.0f * MathF.PI * segment / segments;
                float x = ringRadius * MathF.Cos( theta );
                float z = ringRadius * MathF.Sin( theta );

                Vector3D<float> pos = new Vector3D<float>( x, y, z ) * radius;
                Vector3D<float> normal = -Vector3D.Normalize( new Vector3D<float>( x, y, z ) );

                float u = ( float )segment / segments;
                float v = ( float )ring / rings;

                vertices.Add( new Vertex( pos, new Vector2D<float>( u, v ), white, 0, normal ) );
            }
        }

        for ( int ring = 0; ring < rings; ring++ )
        {
            for ( int segment = 0; segment < segments; segment++ )
            {
                uint current = ( uint )( ring * ( segments + 1 ) + segment );
                uint next = current + ( uint )( segments + 1 );

                indices.Add( current );
                indices.Add( next );
                indices.Add( current + 1 );

                indices.Add( current + 1 );
                indices.Add( next );
                indices.Add( next + 1 );
            }
        }

        Entity skyEntity = World.Spawn();
        World.Add( skyEntity, new Transform
        {
            Position = center,
            Rotation = Quaternion<float>.Identity,
            Scale = Vector3D<float>.One
        } );

        Material skyMaterial = materialSystem.CreateEmissive(
            materials.SkyTexture,
            3.0f
        );
        World.Add( skyEntity, meshSystem.CreateMesh( vertices.ToArray(), indices.ToArray(), skyMaterial ) );
    }
}