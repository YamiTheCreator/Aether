using Silk.NET.Maths;
using Aether.Core;
using Graphics;
using Graphics.Components;
using Graphics.Systems;
using Graphics.Structures;
using Maze.Systems;
using Maze.Components;

namespace Maze;

public class Application() : ApplicationBase(
    title: "Maze",
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

        World.SetGlobal( new Materials
        {
            BrickMaterial = brickMaterial,
            StoneMaterial = stoneMaterial,
            TileMaterial = tileMaterial,
            SandstoneMaterial = sandstoneMaterial,
            GrassMaterial = grassMaterial,
            SkyTexture = skyTexture
        } );

        World.AddSystem( shaderSystem );
        World.AddSystem( inputSystem );
        World.AddSystem( new CameraSystem() );
        World.AddSystem( new MazePlayerController() );
        World.AddSystem( new LightingSystem() );
        World.AddSystem( materialSystem );
        World.AddSystem( new MazeSystem() );
        World.AddSystem( meshSystem );

        CreateMazeGrid();

        Vector3D<float> startPosition = new( 1.5f, 0.5f, 1.5f );
        
        CameraSystem.CreatePerspectiveCamera(
            World,
            position: startPosition,
            yaw: -90f,
            pitch: 0f,
            fov: 75f,
            aspectRatio: ( float )WindowBase.LogicalWidth / WindowBase.LogicalHeight
        );

        CreateLights();
        CreateSky();
    }

    private void CreateMazeGrid()
    {
        int[,] layout = CreateMazeLayout();
        
        Entity gridEntity = World.Spawn();
        World.Add( gridEntity, new Grid
        {
            Layout = layout,
            Width = layout.GetLength( 1 ),
            Height = layout.GetLength( 0 )
        } );
    }

    private int[,] CreateMazeLayout()
    {
        return new[,]
        {
            { 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1 },
            { 1, 1, 1, 1, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1 },
            { 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 1 },
            { 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1 },
            { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
            { 1, 1, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1 },
            { 1, 0, 0, 0, 1, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1 },
            { 1, 0, 1, 0, 1, 0, 1, 0, 1, 1, 1, 0, 1, 1, 1 },
            { 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
            { 1, 0, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 0, 1 },
            { 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 1 },
            { 1, 1, 1, 0, 1, 1, 1, 0, 1, 0, 1, 0, 1, 1, 1 },
            { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1 }
        };
    }

    private void CreateLights()
    {
        LightingSystem lightingSystem = World.GetSystem<LightingSystem>()!;

        CreateLight( lightingSystem, new Vector3D<float>( 1f, 10f, 1f ), new Vector3D<float>( 1.0f, 0.0f, 0.0f ) );
        CreateLight( lightingSystem, new Vector3D<float>( 14f, 10f, 1f ), new Vector3D<float>( 0.0f, 1.0f, 0.0f ) );
        CreateLight( lightingSystem, new Vector3D<float>( 1f, 10f, 14f ), new Vector3D<float>( 0.0f, 0.0f, 1.0f ) );
        CreateLight( lightingSystem, new Vector3D<float>( 14f, 10f, 14f ), new Vector3D<float>( 1.0f, 1.0f, 0.0f ) );
    }

    private void CreateLight( LightingSystem lightingSystem, Vector3D<float> position, Vector3D<float> color )
    {
        Entity light = World.Spawn();
        World.Add( light, new Transform
        {
            Position = position,
            Rotation = Quaternion<float>.Identity,
            Scale = Vector3D<float>.One
        } );
        World.Add( light, lightingSystem.CreatePoint( color, intensity: 100.0f ) );
    }

    private void CreateSky()
    {
        MeshSystem meshSystem = World.GetGlobal<MeshSystem>();
        MaterialSystem materialSystem = World.GetGlobal<MaterialSystem>();
        Materials materials = World.GetGlobal<Materials>();

        (List<Vertex> vertices, List<uint> indices) = CreateSphereGeometry();
        CreateSkyEntity( meshSystem, materialSystem, materials, vertices, indices );
    }

    private (List<Vertex>, List<uint>) CreateSphereGeometry()
    {
        List<Vertex> vertices = [ ];
        List<uint> indices = [ ];

        const float radius = 50.0f;
        const int segments = 32;
        const int rings = 16;
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

        return (vertices, indices);
    }

    private void CreateSkyEntity( MeshSystem meshSystem, MaterialSystem materialSystem, Materials materials,
        List<Vertex> vertices, List<uint> indices )
    {
        Vector3D<float> center = new( 7.5f, 0.5f, 7.5f );

        Entity skyEntity = World.Spawn();
        World.Add( skyEntity, new Transform
        {
            Position = center,
            Rotation = Quaternion<float>.Identity,
            Scale = Vector3D<float>.One
        } );

        Material skyMaterial = materialSystem.CreateEmissive( materials.SkyTexture, 3.0f );
        World.Add( skyEntity, meshSystem.CreateMesh( vertices.ToArray(), indices.ToArray(), skyMaterial ) );
    }
}