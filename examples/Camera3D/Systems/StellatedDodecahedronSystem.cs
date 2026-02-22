using Aether.Core;
using Camera3D.Components;
using Graphics.Components;
using Graphics.Systems;
using Graphics.Structures;
using Silk.NET.Maths;

namespace Camera3D.Systems;

public class StellatedDodecahedronSystem : SystemBase
{
    protected override void OnUpdate( float deltaTime )
    {
        MeshSystem meshSystem = World.GetGlobal<MeshSystem>();
        MaterialSystem materialSystem = World.GetGlobal<MaterialSystem>();

        foreach ( Entity entity in World.Filter<StellatedDodecahedron>().With<Transform>() )
        {
            if ( World.Has<Mesh>( entity ) )
                continue;

            ref StellatedDodecahedron dodecahedron = ref World.Get<StellatedDodecahedron>( entity );

            if ( dodecahedron.IsGenerated )
                continue;

            GenerateDodecahedronMeshes( entity, ref dodecahedron, meshSystem, materialSystem );
            dodecahedron.IsGenerated = true;
        }
    }

    private void GenerateDodecahedronMeshes( Entity parentEntity, ref StellatedDodecahedron dodecahedron,
        MeshSystem meshSystem, MaterialSystem materialSystem )
    {
        float phi = ( 1f + MathF.Sqrt( 5f ) ) / 2f;

        Vector3D<float>[] verts =
        [
            new( 1, 1, 1 ), new( 1, 1, -1 ), new( 1, -1, 1 ), new( 1, -1, -1 ), new( -1, 1, 1 ), new( -1, 1, -1 ),
            new( -1, -1, 1 ), new( -1, -1, -1 ), new( 0, phi, 1 / phi ), new( 0, phi, -1 / phi ),
            new( 0, -phi, 1 / phi ), new( 0, -phi, -1 / phi ), new( 1 / phi, 0, phi ), new( -1 / phi, 0, phi ),
            new( 1 / phi, 0, -phi ), new( -1 / phi, 0, -phi ), new( phi, 1 / phi, 0 ), new( phi, -1 / phi, 0 ),
            new( -phi, 1 / phi, 0 ), new( -phi, -1 / phi, 0 )
        ];

        for ( int i = 0; i < verts.Length; i++ )
            verts[ i ] = Vector3D.Normalize( verts[ i ] ) * dodecahedron.Radius;

        int[][] faces =
        [
            [ 0, 16, 17, 2, 12 ], [ 0, 12, 13, 4, 8 ], [ 0, 8, 9, 1, 16 ],
            [ 1, 9, 5, 15, 14 ], [ 1, 14, 3, 17, 16 ], [ 2, 17, 3, 11, 10 ],
            [ 2, 10, 6, 13, 12 ], [ 3, 14, 15, 7, 11 ], [ 4, 13, 6, 19, 18 ],
            [ 4, 18, 5, 9, 8 ], [ 5, 18, 19, 7, 15 ], [ 6, 10, 11, 7, 19 ]
        ];

        Vector4D<float>[] colors =
        [
            new( 0.8f, 0.5f, 0.3f, 0.7f ), new( 0.4f, 0.6f, 0.4f, 0.7f ), new( 0.5f, 0.6f, 0.7f, 0.7f ),
            new( 0.7f, 0.6f, 0.4f, 0.7f ), new( 0.6f, 0.4f, 0.5f, 0.7f ), new( 0.4f, 0.5f, 0.6f, 0.7f ),
            new( 0.7f, 0.5f, 0.4f, 0.7f ), new( 0.5f, 0.5f, 0.6f, 0.7f ), new( 0.4f, 0.6f, 0.5f, 0.7f ),
            new( 0.6f, 0.5f, 0.4f, 0.7f ), new( 0.5f, 0.6f, 0.6f, 0.7f ), new( 0.6f, 0.6f, 0.5f, 0.7f )
        ];

        List<(Vector3D<float>, Vector3D<float>)> edges = [ ];
        ref Transform parentTransform = ref World.Get<Transform>( parentEntity );

        for ( int faceIdx = 0; faceIdx < faces.Length; faceIdx++ )
        {
            int[] pentagon = faces[ faceIdx ];
            Vector4D<float> color = colors[ faceIdx ];

            Vector3D<float> center = Vector3D<float>.Zero;
            for ( int i = 0; i < 5; i++ )
                center += verts[ pentagon[ i ] ];
            center /= 5f;

            Vector3D<float> normal = Vector3D.Normalize( center );
            Vector3D<float> apex = center + normal * dodecahedron.StellationHeight;

            List<Vertex> vertices = [ ];
            List<uint> indices = [ ];

            for ( int i = 0; i < 5; i++ )
            {
                int next = ( i + 1 ) % 5;
                Vector3D<float> v1 = verts[ pentagon[ i ] ];
                Vector3D<float> v2 = verts[ pentagon[ next ] ];

                Vector3D<float> edge1 = v2 - apex;
                Vector3D<float> edge2 = v1 - apex;
                Vector3D<float> triNormal = Vector3D.Normalize( Vector3D.Cross( edge1, edge2 ) );

                uint baseIdx = ( uint )vertices.Count;

                Vector4D<float> white = new( 1, 1, 1, 1 );
                vertices.Add( new Vertex( apex, new Vector2D<float>( 0.5f, 0 ), white, 0, triNormal ) );
                vertices.Add( new Vertex( v1, new Vector2D<float>( 0, 1 ), white, 0, triNormal ) );
                vertices.Add( new Vertex( v2, new Vector2D<float>( 1, 1 ), white, 0, triNormal ) );

                indices.Add( baseIdx );
                indices.Add( baseIdx + 1 );
                indices.Add( baseIdx + 2 );

                edges.Add( ( apex, v1 ) );
                edges.Add( ( apex, v2 ) );
                edges.Add( ( v1, v2 ) );
            }

            // Создаем базу
            Entity faceEntity = World.Spawn();
            World.Add( faceEntity,
                new Transform( parentTransform.Position, parentTransform.Rotation, parentTransform.Scale ) );
            World.Add( faceEntity, meshSystem.CreateMesh( vertices.ToArray(), indices.ToArray() ) );
            World.Add( faceEntity, materialSystem.CreateUnlit( new Vector3D<float>( color.X, color.Y, color.Z ) ) );
        }

        // Добавляем сетку
        Entity wireframeEntity = World.Spawn();
        World.Add( wireframeEntity,
            new Transform( parentTransform.Position, parentTransform.Rotation, parentTransform.Scale ) );
        World.Add( wireframeEntity, new Wireframe
        {
            Color = new Vector3D<float>( 0, 0, 0 ),
            IsGenerated = false
        } );
        
        World.Add( wireframeEntity, new WireframeEdges
        {
            Edges = edges
        } );
    }
}

// Временный компонент для хранения граней сетки фигуры
public struct WireframeEdges : Component
{
    public List<(Vector3D<float>, Vector3D<float>)> Edges;
}