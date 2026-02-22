using Assimp;
using Silk.NET.Maths;
using Graphics.Components;
using Graphics.Structures;
using Aether.Core;

namespace Graphics.Systems;

public class ModelImporterSystem(
    TextureSystem textureSystem,
    MeshSystem meshSystem )
{
    private readonly AssimpContext _importer = new();

    public List<Entity> LoadModel( World world, string filePath )
    {
        if ( !File.Exists( filePath ) )
        {
            throw new FileNotFoundException( $"Model file not found: {filePath}" );
        }

        Scene scene = _importer.ImportFile( filePath,
            PostProcessSteps.Triangulate |
            PostProcessSteps.GenerateSmoothNormals |
            PostProcessSteps.FlipUVs |
            PostProcessSteps.CalculateTangentSpace |
            PostProcessSteps.JoinIdenticalVertices );

        if ( scene == null || scene.SceneFlags.HasFlag( SceneFlags.Incomplete ) || scene.RootNode == null )
        {
            throw new Exception( "Failed to load model" );
        }

        string directory = Path.GetDirectoryName( filePath ) ?? "";
        List<Entity> entities = [ ];

        Matrix4x4 rootTransform = Matrix4x4.Identity;

        ProcessNode( world, scene.RootNode, scene, directory, entities, rootTransform );

        return entities;
    }

    private void ProcessNode( World world, Node node, Scene scene, string directory, List<Entity> entities,
        Matrix4x4 parentTransform )
    {
        Matrix4x4 globalTransform = node.Transform * parentTransform;
        for ( int i = 0; i < node.MeshCount; i++ )
        {
            Assimp.Mesh mesh = scene.Meshes[ node.MeshIndices[ i ] ];
            Entity entity = ProcessMesh( world, mesh, scene, directory, globalTransform );
            entities.Add( entity );
        }

        for ( int i = 0; i < node.ChildCount; i++ )
        {
            ProcessNode( world, node.Children[ i ], scene, directory, entities, globalTransform );
        }
    }

    private Entity ProcessMesh( World world, Assimp.Mesh mesh, Scene scene, string directory,
        Matrix4x4 globalTransform )
    {
        List<Vertex> vertices = [ ];
        List<uint> indices = [ ];

        for ( int i = 0; i < mesh.VertexCount; i++ )
        {
            Vector3D<float> vertexPosition = new(
                mesh.Vertices[ i ].X,
                mesh.Vertices[ i ].Y,
                mesh.Vertices[ i ].Z
            );

            Vector2D<float> texCoords = mesh.HasTextureCoords( 0 )
                ? new Vector2D<float>( mesh.TextureCoordinateChannels[ 0 ][ i ].X,
                    mesh.TextureCoordinateChannels[ 0 ][ i ].Y )
                : Vector2D<float>.Zero;

            Vector4D<float> color = mesh.HasVertexColors( 0 )
                ? new Vector4D<float>(
                    mesh.VertexColorChannels[ 0 ][ i ].R,
                    mesh.VertexColorChannels[ 0 ][ i ].G,
                    mesh.VertexColorChannels[ 0 ][ i ].B,
                    mesh.VertexColorChannels[ 0 ][ i ].A )
                : new Vector4D<float>( 1f, 1f, 1f, 1f );

            Vector3D<float> normal = mesh.HasNormals
                ? new Vector3D<float>( mesh.Normals[ i ].X, mesh.Normals[ i ].Y, mesh.Normals[ i ].Z )
                : new Vector3D<float>( 0, 1, 0 );

            vertices.Add( new Vertex( vertexPosition, texCoords, color, 0, normal ) );
        }

        for ( int i = 0; i < mesh.FaceCount; i++ )
        {
            Face face = mesh.Faces[ i ];
            for ( int j = 0; j < face.IndexCount; j++ )
            {
                indices.Add( ( uint )face.Indices[ j ] );
            }
        }

        Components.Material material =
            ProcessMaterial( scene.Materials[ mesh.MaterialIndex ], scene, directory );

        Entity entity = world.Spawn();

        globalTransform.Decompose( out Assimp.Vector3D scale, out Quaternion rotation,
            out Assimp.Vector3D position );

        Transform transform = new()
        {
            Position = new Vector3D<float>( position.X, position.Y, position.Z ),
            Rotation = new Quaternion<float>( rotation.X, rotation.Y, rotation.Z, rotation.W ),
            Scale = new Vector3D<float>( scale.X, scale.Y, scale.Z )
        };

        world.Add( entity, transform );
        world.Add( entity, meshSystem.CreateMesh( vertices.ToArray(), indices.ToArray(), material ) );
        world.Add( entity, material );

        return entity;
    }

    private Components.Material ProcessMaterial( Assimp.Material assimpMaterial,
        Scene scene, string directory )
    {
        Components.Material material = new()
        {
            DiffuseColor = new Vector3D<float>( 0.8f, 0.8f, 0.8f ),
            AmbientColor = new Vector3D<float>( 0.2f, 0.2f, 0.2f ),
            SpecularColor = new Vector3D<float>( 1f, 1f, 1f ),
            Shininess = 32f,
            Alpha = 1f
        };

        if ( assimpMaterial.HasColorDiffuse )
        {
            material.DiffuseColor = new Vector3D<float>(
                assimpMaterial.ColorDiffuse.R,
                assimpMaterial.ColorDiffuse.G,
                assimpMaterial.ColorDiffuse.B
            );
        }

        if ( assimpMaterial.HasColorAmbient )
        {
            material.AmbientColor = new Vector3D<float>(
                assimpMaterial.ColorAmbient.R,
                assimpMaterial.ColorAmbient.G,
                assimpMaterial.ColorAmbient.B
            );
        }

        if ( assimpMaterial.HasColorSpecular )
        {
            material.SpecularColor = new Vector3D<float>(
                assimpMaterial.ColorSpecular.R,
                assimpMaterial.ColorSpecular.G,
                assimpMaterial.ColorSpecular.B
            );
        }

        if ( assimpMaterial.HasShininess )
        {
            material.Shininess = assimpMaterial.Shininess;
        }

        if ( assimpMaterial.HasOpacity )
        {
            material.Alpha = assimpMaterial.Opacity;
        }

        if ( assimpMaterial.HasProperty( "$mat.metallicFactor" ) )
        {
            material.Metallic = assimpMaterial.GetProperty( "$mat.metallicFactor" ).GetFloatValue();
        }

        if ( assimpMaterial.HasProperty( "$mat.roughnessFactor" ) )
        {
            material.Roughness = assimpMaterial.GetProperty( "$mat.roughnessFactor" ).GetFloatValue();
        }

        LoadTexture( assimpMaterial, scene, directory, TextureType.BaseColor, ref material.Texture );
        LoadTexture( assimpMaterial, scene, directory, TextureType.Diffuse, ref material.Texture );
        LoadTexture( assimpMaterial, scene, directory, TextureType.Normals, ref material.NormalMap );
        LoadTexture( assimpMaterial, scene, directory, TextureType.Metalness, ref material.MetallicMap );
        LoadTexture( assimpMaterial, scene, directory, TextureType.Roughness, ref material.RoughnessMap );
        LoadTexture( assimpMaterial, scene, directory, TextureType.Roughness, ref material.RoughnessMap );
        LoadTexture( assimpMaterial, scene, directory, TextureType.AmbientOcclusion, ref material.AmbientOcclusionMap );
        LoadTexture( assimpMaterial, scene, directory, TextureType.Emissive, ref material.EmissiveMap );

        return material;
    }

    private void LoadTexture( Assimp.Material assimpMaterial, Scene scene, string directory,
        TextureType textureType, ref Texture2D? targetTexture )
    {
        if ( targetTexture != null )
        {
            return;
        }

        int textureCount = assimpMaterial.GetMaterialTextureCount( textureType );

        if ( textureCount > 0 )
        {
            assimpMaterial.GetMaterialTexture( textureType, 0, out TextureSlot textureSlot );
            string texturePath = textureSlot.FilePath;

            if ( texturePath.StartsWith( "*" ) )
            {
                if ( int.TryParse( texturePath.AsSpan( 1 ), out int textureIndex ) )
                {
                    if ( textureIndex < scene.TextureCount )
                    {
                        EmbeddedTexture embeddedTexture = scene.Textures[ textureIndex ];

                        try
                        {
                            targetTexture = LoadEmbeddedTexture( embeddedTexture );
                        }
                        catch ( Exception )
                        {
                        }
                    }
                }
            }
            else if ( !string.IsNullOrEmpty( texturePath ) )
            {
                string[] possiblePaths =
                [
                    Path.Combine( directory, texturePath ),
                    Path.Combine( directory, Path.GetFileName( texturePath ) ),
                    texturePath
                ];

                string? foundPath = null;
                foreach ( string path in possiblePaths )
                {
                    if ( File.Exists( path ) )
                    {
                        foundPath = path;
                        break;
                    }
                }

                if ( foundPath != null )
                {
                    try
                    {
                        targetTexture = textureSystem.CreateTextureFromFile( foundPath );
                    }
                    catch ( Exception )
                    {
                    }
                }
            }
        }
    }

    private Texture2D LoadEmbeddedTexture( EmbeddedTexture embeddedTexture )
    {
        if ( embeddedTexture.IsCompressed )
        {
            byte[] data = embeddedTexture.CompressedData;
            return textureSystem.CreateTextureFromMemory( data );
        }

        int width = embeddedTexture.Width;
        int height = embeddedTexture.Height;
        Texel[] texels = embeddedTexture.NonCompressedData;

        byte[] pixels = new byte[ width * height * 4 ];
        for ( int i = 0; i < texels.Length; i++ )
        {
            pixels[ i * 4 + 0 ] = texels[ i ].R;
            pixels[ i * 4 + 1 ] = texels[ i ].G;
            pixels[ i * 4 + 2 ] = texels[ i ].B;
            pixels[ i * 4 + 3 ] = texels[ i ].A;
        }

        return textureSystem.CreateTextureFromPixels( pixels, width, height );
    }

    public void Dispose()
    {
        _importer.Dispose();
    }
}