using Assimp;
using Silk.NET.Maths;
using Graphics.Components;
using Graphics.Structures;
using Aether.Core;

namespace Graphics.Systems;

public class ModelSystem(
    TextureSystem textureSystem,
    MeshSystem meshSystem )
{
    private readonly AssimpContext _importer = new();

    public List<Entity> LoadModel( World world, string filePath, Vector3D<float>? scale = null )
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

        ProcessNode( world, scene.RootNode, scene, directory, entities, rootTransform, scale );

        return entities;
    }

    private void ProcessNode( World world, Node node, Scene scene, string directory, List<Entity> entities,
        Matrix4x4 parentTransform, Vector3D<float>? scale = null )
    {
        Matrix4x4 globalTransform = node.Transform * parentTransform;
        for ( int i = 0; i < node.MeshCount; i++ )
        {
            Assimp.Mesh mesh = scene.Meshes[ node.MeshIndices[ i ] ];
            Entity entity = ProcessMesh( world, mesh, scene, directory, globalTransform, scale );
            entities.Add( entity );
        }

        for ( int i = 0; i < node.ChildCount; i++ )
        {
            ProcessNode( world, node.Children[ i ], scene, directory, entities, globalTransform, scale );
        }
    }

    private Entity ProcessMesh( World world, Assimp.Mesh mesh, Scene scene, string directory,
        Matrix4x4 globalTransform, Vector3D<float>? scale = null )
    {
        List<Vertex> vertices = ExtractVertices( mesh );
        List<uint> indices = ExtractIndices( mesh );
        Components.Material material = ProcessMaterial( scene.Materials[ mesh.MaterialIndex ], scene, directory );

        return CreateMeshEntity( world, vertices, indices, material, globalTransform, scale );
    }

    private List<Vertex> ExtractVertices( Assimp.Mesh mesh )
    {
        List<Vertex> vertices = [ ];

        for ( int i = 0; i < mesh.VertexCount; i++ )
        {
            Vector3D<float> position = ExtractVertexPosition( mesh, i );
            Vector2D<float> texCoords = ExtractTexCoords( mesh, i );
            Vector4D<float> color = ExtractVertexColor( mesh, i );
            Vector3D<float> normal = ExtractNormal( mesh, i );

            vertices.Add( new Vertex( position, texCoords, color, 0, normal ) );
        }

        return vertices;
    }

    private Vector3D<float> ExtractVertexPosition( Assimp.Mesh mesh, int index )
    {
        return new Vector3D<float>(
            mesh.Vertices[ index ].X,
            mesh.Vertices[ index ].Y,
            mesh.Vertices[ index ].Z
        );
    }

    private Vector2D<float> ExtractTexCoords( Assimp.Mesh mesh, int index )
    {
        return mesh.HasTextureCoords( 0 )
            ? new Vector2D<float>( mesh.TextureCoordinateChannels[ 0 ][ index ].X,
                mesh.TextureCoordinateChannels[ 0 ][ index ].Y )
            : Vector2D<float>.Zero;
    }

    private Vector4D<float> ExtractVertexColor( Assimp.Mesh mesh, int index )
    {
        return mesh.HasVertexColors( 0 )
            ? new Vector4D<float>(
                mesh.VertexColorChannels[ 0 ][ index ].R,
                mesh.VertexColorChannels[ 0 ][ index ].G,
                mesh.VertexColorChannels[ 0 ][ index ].B,
                mesh.VertexColorChannels[ 0 ][ index ].A )
            : new Vector4D<float>( 1f, 1f, 1f, 1f );
    }

    private Vector3D<float> ExtractNormal( Assimp.Mesh mesh, int index )
    {
        return mesh.HasNormals
            ? new Vector3D<float>( mesh.Normals[ index ].X, mesh.Normals[ index ].Y, mesh.Normals[ index ].Z )
            : new Vector3D<float>( 0, 1, 0 );
    }

    private List<uint> ExtractIndices( Assimp.Mesh mesh )
    {
        List<uint> indices = [ ];

        for ( int i = 0; i < mesh.FaceCount; i++ )
        {
            Face face = mesh.Faces[ i ];
            for ( int j = 0; j < face.IndexCount; j++ )
            {
                indices.Add( ( uint )face.Indices[ j ] );
            }
        }

        return indices;
    }

    private Entity CreateMeshEntity( World world, List<Vertex> vertices, List<uint> indices,
        Components.Material material, Matrix4x4 globalTransform, Vector3D<float>? scale = null )
    {
        Entity entity = world.Spawn();
        Transform transform = ExtractTransform( globalTransform );
        
        if ( scale.HasValue )
        {
            transform.Scale *= scale.Value;
        }

        world.Add( entity, transform );
        world.Add( entity, meshSystem.CreateMesh( vertices.ToArray(), indices.ToArray(), material ) );
        world.Add( entity, material );

        return entity;
    }

    private Transform ExtractTransform( Matrix4x4 globalTransform )
    {
        globalTransform.Decompose( out Assimp.Vector3D scale, out Quaternion rotation,
            out Assimp.Vector3D position );

        return new Transform
        {
            Position = new Vector3D<float>( position.X, position.Y, position.Z ),
            Rotation = new Quaternion<float>( rotation.X, rotation.Y, rotation.Z, rotation.W ),
            Scale = new Vector3D<float>( scale.X, scale.Y, scale.Z )
        };
    }

    private Components.Material ProcessMaterial( Assimp.Material assimpMaterial,
        Scene scene, string directory )
    {
        Components.Material material = CreateDefaultMaterial();

        ExtractMaterialColors( assimpMaterial, ref material );
        ExtractMaterialProperties( assimpMaterial, ref material );
        LoadMaterialTextures( assimpMaterial, scene, directory, ref material );

        return material;
    }

    private Components.Material CreateDefaultMaterial()
    {
        return new Components.Material
        {
            DiffuseColor = new Vector3D<float>( 0.8f, 0.8f, 0.8f ),
            AmbientColor = new Vector3D<float>( 0.2f, 0.2f, 0.2f ),
            SpecularColor = new Vector3D<float>( 1f, 1f, 1f ),
            Shininess = 32f,
            Alpha = 1f
        };
    }

    private void ExtractMaterialColors( Assimp.Material assimpMaterial, ref Components.Material material )
    {
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
    }

    private void ExtractMaterialProperties( Assimp.Material assimpMaterial, ref Components.Material material )
    {
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
    }

    private void LoadMaterialTextures( Assimp.Material assimpMaterial, Scene scene, string directory,
        ref Components.Material material )
    {
        LoadTexture( assimpMaterial, scene, directory, TextureType.BaseColor, ref material.Texture );
        LoadTexture( assimpMaterial, scene, directory, TextureType.Diffuse, ref material.Texture );
        LoadTexture( assimpMaterial, scene, directory, TextureType.Normals, ref material.NormalMap );
        LoadTexture( assimpMaterial, scene, directory, TextureType.Metalness, ref material.MetallicMap );
        LoadTexture( assimpMaterial, scene, directory, TextureType.Roughness, ref material.RoughnessMap );
        LoadTexture( assimpMaterial, scene, directory, TextureType.AmbientOcclusion, ref material.AmbientOcclusionMap );
        LoadTexture( assimpMaterial, scene, directory, TextureType.Emissive, ref material.EmissiveMap );
    }

    private void LoadTexture( Assimp.Material assimpMaterial, Scene scene, string directory,
        TextureType textureType, ref Texture2D? targetTexture )
    {
        if ( targetTexture != null )
            return;

        int textureCount = assimpMaterial.GetMaterialTextureCount( textureType );
        if ( textureCount == 0 )
            return;

        assimpMaterial.GetMaterialTexture( textureType, 0, out TextureSlot textureSlot );
        string texturePath = textureSlot.FilePath;

        if ( texturePath.StartsWith( "*" ) )
        {
            targetTexture = LoadEmbeddedTextureByIndex( scene, texturePath );
        }
        else if ( !string.IsNullOrEmpty( texturePath ) )
        {
            targetTexture = LoadExternalTexture( directory, texturePath );
        }
    }

    private Texture2D? LoadEmbeddedTextureByIndex( Scene scene, string texturePath )
    {
        if ( !int.TryParse( texturePath.AsSpan( 1 ), out int textureIndex ) )
            return null;

        if ( textureIndex >= scene.TextureCount )
            return null;

        try
        {
            return LoadEmbeddedTexture( scene.Textures[ textureIndex ] );
        }
        catch ( Exception )
        {
            return null;
        }
    }

    private Texture2D? LoadExternalTexture( string directory, string texturePath )
    {
        string? foundPath = FindTexturePath( directory, texturePath );
        if ( foundPath == null )
            return null;

        try
        {
            return textureSystem.CreateTextureFromFile( foundPath );
        }
        catch ( Exception )
        {
            return null;
        }
    }

    private string? FindTexturePath( string directory, string texturePath )
    {
        string[] possiblePaths =
        [
            Path.Combine( directory, texturePath ),
            Path.Combine( directory, Path.GetFileName( texturePath ) ),
            texturePath
        ];

        foreach ( string path in possiblePaths )
        {
            if ( File.Exists( path ) )
                return path;
        }

        return null;
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