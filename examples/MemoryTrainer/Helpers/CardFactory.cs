using Aether.Core;
using Graphics.Components;
using Graphics.Structures;
using Graphics.Systems;
using MemoryTrainer.Components;
using Silk.NET.Maths;

namespace MemoryTrainer.Helpers;

public static class CardFactory
{
    public static void CreateCard( World world, int cardId, int pairId, int textureIndex,
        Vector3D<float> position, float width, float height, float depth,
        MeshSystem meshSystem, TextureSystem textureSystem )
    {
        Entity entity = world.Spawn();

        world.Add( entity, new Card
        {
            CardId = cardId,
            PairId = pairId,
            TextureIndex = textureIndex,
            IsRevealed = false,
            IsMatched = false,
            IsFlipping = false,
            FlipProgress = 0f,
            FlipSpeed = 3f,
            FlipToFront = false
        } );

        world.Add( entity, new Transform
        {
            Position = position,
            Rotation = Quaternion<float>.CreateFromAxisAngle(
                new Vector3D<float>( 1f, 0f, 0f ),
                90f * MathF.PI / 180f
            ),
            Scale = Vector3D<float>.One
        } );

        Mesh cardMesh = CreateCardMesh( width, height, depth, meshSystem );
        world.Add( entity, cardMesh );

        Texture2D tempTexture = textureSystem.CreateTextureFromColor( 1, 1, 50, 100, 200 );

        Material material = new()
        {
            Texture = tempTexture,
            DiffuseColor = new Vector3D<float>( 1f, 1f, 1f ),
            AmbientColor = new Vector3D<float>( 0.3f, 0.3f, 0.3f ),
            SpecularColor = new Vector3D<float>( 0.5f, 0.5f, 0.5f ),
            Shininess = 32f,
            Alpha = 1f
        };
        world.Add( entity, material );
    }

    private static Mesh CreateCardMesh( float width, float height, float depth, MeshSystem meshSystem )
    {
        float hw = width / 2f;
        float hh = height / 2f;
        float hd = depth / 2f;

        List<Vertex> vertices = [ ];
        List<uint> indices = [ ];

        Vector4D<float> white = new( 1f, 1f, 1f, 1f );

        AddQuad( vertices, indices,
            new Vector3D<float>( -hw, -hh, hd ), new Vector3D<float>( hw, -hh, hd ),
            new Vector3D<float>( hw, hh, hd ), new Vector3D<float>( -hw, hh, hd ),
            new Vector3D<float>( 0f, 0f, 1f ), white );

        AddQuad( vertices, indices,
            new Vector3D<float>( hw, -hh, -hd ), new Vector3D<float>( -hw, -hh, -hd ),
            new Vector3D<float>( -hw, hh, -hd ), new Vector3D<float>( hw, hh, -hd ),
            new Vector3D<float>( 0f, 0f, -1f ), white );

        AddQuad( vertices, indices,
            new Vector3D<float>( -hw, hh, hd ), new Vector3D<float>( hw, hh, hd ),
            new Vector3D<float>( hw, hh, -hd ), new Vector3D<float>( -hw, hh, -hd ),
            new Vector3D<float>( 0f, 1f, 0f ), white );

        AddQuad( vertices, indices,
            new Vector3D<float>( -hw, -hh, -hd ), new Vector3D<float>( hw, -hh, -hd ),
            new Vector3D<float>( hw, -hh, hd ), new Vector3D<float>( -hw, -hh, hd ),
            new Vector3D<float>( 0f, -1f, 0f ), white );

        AddQuad( vertices, indices,
            new Vector3D<float>( hw, -hh, hd ), new Vector3D<float>( hw, -hh, -hd ),
            new Vector3D<float>( hw, hh, -hd ), new Vector3D<float>( hw, hh, hd ),
            new Vector3D<float>( 1f, 0f, 0f ), white );

        AddQuad( vertices, indices,
            new Vector3D<float>( -hw, -hh, -hd ), new Vector3D<float>( -hw, -hh, hd ),
            new Vector3D<float>( -hw, hh, hd ), new Vector3D<float>( -hw, hh, -hd ),
            new Vector3D<float>( -1f, 0f, 0f ), white );

        return meshSystem.CreateMesh( vertices.ToArray(), indices.ToArray() );
    }

    private static void AddQuad( List<Vertex> vertices, List<uint> indices,
        Vector3D<float> v0, Vector3D<float> v1, Vector3D<float> v2, Vector3D<float> v3,
        Vector3D<float> normal, Vector4D<float> color )
    {
        uint baseIndex = ( uint )vertices.Count;

        vertices.Add( new Vertex( v0, new Vector2D<float>( 0f, 1f ), color, 0, normal ) );
        vertices.Add( new Vertex( v1, new Vector2D<float>( 1f, 1f ), color, 0, normal ) );
        vertices.Add( new Vertex( v2, new Vector2D<float>( 1f, 0f ), color, 0, normal ) );
        vertices.Add( new Vertex( v3, new Vector2D<float>( 0f, 0f ), color, 0, normal ) );

        indices.Add( baseIndex );
        indices.Add( baseIndex + 1 );
        indices.Add( baseIndex + 2 );

        indices.Add( baseIndex );
        indices.Add( baseIndex + 2 );
        indices.Add( baseIndex + 3 );
    }
}
