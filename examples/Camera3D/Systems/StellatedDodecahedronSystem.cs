using Aether.Core;
using Camera3D.Components;
using Graphics.Components;
using Graphics.Systems;
using Graphics.Structures;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Camera3D.Systems;

public class StellatedDodecahedronSystem : SystemBase
{
    private MeshSystem? _meshSystem;
    private MaterialSystem? _materialSystem;

    protected override void OnCreate()
    {
        _meshSystem = World.GetGlobal<MeshSystem>();
        _materialSystem = World.GetGlobal<MaterialSystem>();
    }

    protected override void OnUpdate( float deltaTime ) { }

    protected override void OnRender()
    {
        if ( _meshSystem == null || _materialSystem == null ) return;

        foreach ( Entity entity in World.Filter<StellatedDodecahedron, Transform>() )
        {
            if ( World.Has<Mesh>( entity ) )
                continue;

            ref StellatedDodecahedron dodecahedron = ref World.Get<StellatedDodecahedron>( entity );

            if ( dodecahedron.IsGenerated )
                continue;

            GenerateDodecahedronMeshes( entity, ref dodecahedron, _meshSystem, _materialSystem );
            dodecahedron.IsGenerated = true;
        }
    }

    protected override void OnDestroy() { }

    private void GenerateDodecahedronMeshes( Entity parentEntity, ref StellatedDodecahedron dodecahedron,
        MeshSystem meshSystem, MaterialSystem materialSystem )
    {
        Vector3D<float>[] vertices = CreateDodecahedronVertices( dodecahedron.Radius );
        int[][] faces = GetDodecahedronFaces();
        Vector3D<float>[] colors = GetFaceColors();
        
        ref Transform parentTransform = ref World.Get<Transform>( parentEntity );
        List<(Vector3D<float>, Vector3D<float>)> edges = GenerateFaces( vertices, faces, colors, 
            dodecahedron.StellationHeight, parentTransform, meshSystem );

        GenerateWireframeMesh( edges, parentTransform, meshSystem, materialSystem );
    }

    private Vector3D<float>[] CreateDodecahedronVertices( float radius )
    {
        float phi = ( 1f + MathF.Sqrt( 5f ) ) / 2f;

        Vector3D<float>[] verts =
        [
            new( 1, 1, 1 ), new( 1, 1, -1 ), new( 1, -1, 1 ), new( 1, -1, -1 ),
            new( -1, 1, 1 ), new( -1, 1, -1 ), new( -1, -1, 1 ), new( -1, -1, -1 ),
            new( 0, phi, 1 / phi ), new( 0, phi, -1 / phi ), new( 0, -phi, 1 / phi ), new( 0, -phi, -1 / phi ),
            new( 1 / phi, 0, phi ), new( -1 / phi, 0, phi ), new( 1 / phi, 0, -phi ), new( -1 / phi, 0, -phi ),
            new( phi, 1 / phi, 0 ), new( phi, -1 / phi, 0 ), new( -phi, 1 / phi, 0 ), new( -phi, -1 / phi, 0 )
        ];

        for ( int i = 0; i < verts.Length; i++ )
            verts[ i ] = Vector3D.Normalize( verts[ i ] ) * radius;

        return verts;
    }

    private int[][] GetDodecahedronFaces()
    {
        return
        [
            [ 0, 16, 17, 2, 12 ], [ 0, 12, 13, 4, 8 ], [ 0, 8, 9, 1, 16 ],
            [ 1, 9, 5, 15, 14 ], [ 1, 14, 3, 17, 16 ], [ 2, 17, 3, 11, 10 ],
            [ 2, 10, 6, 13, 12 ], [ 3, 14, 15, 7, 11 ], [ 4, 13, 6, 19, 18 ],
            [ 4, 18, 5, 9, 8 ], [ 5, 18, 19, 7, 15 ], [ 6, 10, 11, 7, 19 ]
        ];
    }

    private Vector3D<float>[] GetFaceColors()
    {
        return
        [
            new( 1f, 0.5f, 0.3f ), new( 0.3f, 1f, 0.5f ), new( 0.5f, 0.3f, 1f ),
            new( 1f, 0.5f, 0.3f ), new( 0.3f, 1f, 0.5f ), new( 0.5f, 0.3f, 1f ),
            new( 1f, 0.5f, 0.3f ), new( 0.3f, 1f, 0.5f ), new( 0.5f, 0.3f, 1f ),
            new( 1f, 0.5f, 0.3f ), new( 0.3f, 1f, 0.5f ), new( 0.5f, 0.3f, 1f )
        ];
    }

    private List<(Vector3D<float>, Vector3D<float>)> GenerateFaces( Vector3D<float>[] vertices, int[][] faces,
        Vector3D<float>[] colors, float stellationHeight, Transform parentTransform, MeshSystem meshSystem )
    {
        List<(Vector3D<float>, Vector3D<float>)> edges = [ ];

        for ( int faceIdx = 0; faceIdx < faces.Length; faceIdx++ )
        {
            int[] pentagon = faces[ faceIdx ];
            Vector3D<float> color = colors[ faceIdx ];

            Vector3D<float> apex = CalculateFaceApex( vertices, pentagon, stellationHeight );
            List<(Vector3D<float>, Vector3D<float>)> faceEdges = CreateStellatedFace( 
                vertices, pentagon, apex, color, parentTransform, meshSystem );
            
            edges.AddRange( faceEdges );
        }

        return edges;
    }

    private Vector3D<float> CalculateFaceApex( Vector3D<float>[] vertices, int[] pentagon, float stellationHeight )
    {
        Vector3D<float> center = Vector3D<float>.Zero;
        for ( int i = 0; i < 5; i++ )
            center += vertices[ pentagon[ i ] ];
        center /= 5f;

        Vector3D<float> normal = Vector3D.Normalize( center );
        return center + normal * stellationHeight;
    }

    private List<(Vector3D<float>, Vector3D<float>)> CreateStellatedFace( Vector3D<float>[] vertices, 
        int[] pentagon, Vector3D<float> apex, Vector3D<float> color, Transform parentTransform, MeshSystem meshSystem )
    {
        List<Vertex> faceVertices = [ ];
        List<uint> faceIndices = [ ];
        List<(Vector3D<float>, Vector3D<float>)> edges = [ ];

        for ( int i = 0; i < 5; i++ )
        {
            int next = ( i + 1 ) % 5;
            Vector3D<float> v1 = vertices[ pentagon[ i ] ];
            Vector3D<float> v2 = vertices[ pentagon[ next ] ];

            AddTriangle( faceVertices, faceIndices, apex, v1, v2 );
            
            edges.Add( ( apex, v1 ) );
            edges.Add( ( apex, v2 ) );
            edges.Add( ( v1, v2 ) );
        }

        CreateFaceEntity( faceVertices, faceIndices, color, parentTransform, meshSystem );
        return edges;
    }

    private void AddTriangle( List<Vertex> vertices, List<uint> indices, 
        Vector3D<float> apex, Vector3D<float> v1, Vector3D<float> v2 )
    {
        Vector3D<float> edge1 = v2 - apex;
        Vector3D<float> edge2 = v1 - apex;
        Vector3D<float> normal = Vector3D.Normalize( Vector3D.Cross( edge1, edge2 ) );

        uint baseIdx = ( uint )vertices.Count;
        Vector4D<float> white = new( 1, 1, 1, 1 );

        vertices.Add( new Vertex( apex, new Vector2D<float>( 0.5f, 0 ), white, 0, normal ) );
        vertices.Add( new Vertex( v1, new Vector2D<float>( 0, 1 ), white, 0, normal ) );
        vertices.Add( new Vertex( v2, new Vector2D<float>( 1, 1 ), white, 0, normal ) );

        indices.Add( baseIdx );
        indices.Add( baseIdx + 1 );
        indices.Add( baseIdx + 2 );
    }

    private void CreateFaceEntity( List<Vertex> vertices, List<uint> indices, Vector3D<float> color,
        Transform parentTransform, MeshSystem meshSystem )
    {
        Entity faceEntity = World.Spawn();
        World.Add( faceEntity, new Transform( parentTransform.Position, parentTransform.Rotation, parentTransform.Scale ) );
        World.Add( faceEntity, meshSystem.CreateMesh( vertices.ToArray(), indices.ToArray() ) );

        Material faceMaterial = new()
        {
            Alpha = 0.9f,
            DiffuseColor = color,
            Metallic = 1f,
            Roughness = 0.3f
        };

        World.Add( faceEntity, faceMaterial );
    }

    private void GenerateWireframeMesh( List<(Vector3D<float>, Vector3D<float>)> edges,
        Transform parentTransform, MeshSystem meshSystem, MaterialSystem materialSystem )
    {
        HashSet<(Vector3D<float>, Vector3D<float>)> uniqueEdges = GetUniqueEdges( edges );
        (List<Vertex> vertices, List<uint> indices) = CreateWireframeGeometry( uniqueEdges );
        CreateWireframeEntity( vertices, indices, parentTransform, meshSystem, materialSystem );
    }

    private HashSet<(Vector3D<float>, Vector3D<float>)> GetUniqueEdges( 
        List<(Vector3D<float>, Vector3D<float>)> edges )
    {
        HashSet<(Vector3D<float>, Vector3D<float>)> uniqueEdges = [ ];
        
        foreach ( (Vector3D<float>, Vector3D<float>) edge in edges )
        {
            (Vector3D<float>, Vector3D<float>) normalized = NormalizeEdge( edge );
            uniqueEdges.Add( normalized );
        }

        return uniqueEdges;
    }

    private (Vector3D<float>, Vector3D<float>) NormalizeEdge( (Vector3D<float>, Vector3D<float>) edge )
    {
        bool shouldSwap = !( edge.Item1.X < edge.Item2.X ) &&
                         ( edge.Item1.X != edge.Item2.X || !( edge.Item1.Y < edge.Item2.Y ) ) &&
                         ( edge.Item1.X != edge.Item2.X || edge.Item1.Y != edge.Item2.Y || 
                           !( edge.Item1.Z < edge.Item2.Z ) );
        
        return shouldSwap ? edge : ( edge.Item2, edge.Item1 );
    }

    private (List<Vertex>, List<uint>) CreateWireframeGeometry( 
        HashSet<(Vector3D<float>, Vector3D<float>)> uniqueEdges )
    {
        List<Vertex> vertices = [ ];
        List<uint> indices = [ ];
        Vector4D<float> black = new( 0, 0, 0, 1 );

        foreach ( (Vector3D<float>, Vector3D<float>) edge in uniqueEdges )
        {
            uint baseIdx = ( uint )vertices.Count;
            vertices.Add( new Vertex( edge.Item1, Vector2D<float>.Zero, black, 0, Vector3D<float>.Zero ) );
            vertices.Add( new Vertex( edge.Item2, Vector2D<float>.Zero, black, 0, Vector3D<float>.Zero ) );
            indices.Add( baseIdx );
            indices.Add( baseIdx + 1 );
        }

        return (vertices, indices);
    }

    private void CreateWireframeEntity( List<Vertex> vertices, List<uint> indices, Transform parentTransform,
        MeshSystem meshSystem, MaterialSystem materialSystem )
    {
        Entity wireframeEntity = World.Spawn();
        World.Add( wireframeEntity, new Transform( parentTransform.Position, parentTransform.Rotation, parentTransform.Scale ) );

        Mesh wireframeMesh = meshSystem.CreateMesh( vertices.ToArray(), indices.ToArray() );
        wireframeMesh.Topology = PrimitiveType.Lines;
        
        World.Add( wireframeEntity, wireframeMesh );
        World.Add( wireframeEntity, materialSystem.CreateUnlit( new Vector3D<float>( 0, 0, 0 ) ) );
    }
}