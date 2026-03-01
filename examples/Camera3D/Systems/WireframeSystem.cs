using Aether.Core;
using Camera3D.Components;
using Graphics.Components;
using Graphics.Systems;
using Graphics.Structures;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Camera3D.Systems;

public class WireframeSystem : SystemBase
{
    protected override void OnUpdate( float deltaTime )
    {
        MeshSystem meshSystem = World.GetGlobal<MeshSystem>();
        MaterialSystem materialSystem = World.GetGlobal<MaterialSystem>();

        foreach ( Entity entity in World.Filter<Wireframe>().With<WireframeEdges>() )
        {
            if ( World.Has<Mesh>( entity ) || !World.Has<Transform>( entity ) )
                continue;

            ref Wireframe wireframe = ref World.Get<Wireframe>( entity );

            if ( wireframe.IsGenerated )
                continue;

            ref WireframeEdges edgesComponent = ref World.Get<WireframeEdges>( entity );
            GenerateWireframeMesh( entity, ref wireframe, edgesComponent.Edges, meshSystem, materialSystem );

            wireframe.IsGenerated = true;
        }
    }

    private void GenerateWireframeMesh( Entity entity, ref Wireframe wireframe,
        List<(Vector3D<float>, Vector3D<float>)> edges, MeshSystem meshSystem, MaterialSystem materialSystem )
    {
        // Удаляем дублирующиеся грани
        HashSet<(Vector3D<float>, Vector3D<float>)> uniqueEdges = [ ];
        foreach ( (Vector3D<float>, Vector3D<float>) edge in edges )
        {
            (Vector3D<float>, Vector3D<float>) normalized = !( edge.Item1.X < edge.Item2.X ) &&
                                                            ( edge.Item1.X != edge.Item2.X ||
                                                              !( edge.Item1.Y < edge.Item2.Y ) ) &&
                                                            ( edge.Item1.X != edge.Item2.X ||
                                                              edge.Item1.Y != edge.Item2.Y ||
                                                              !( edge.Item1.Z < edge.Item2.Z ) )
                ? edge
                : ( edge.Item2, edge.Item1 );
            uniqueEdges.Add( normalized );
        }

        List<Vertex> lineVertices = [ ];
        List<uint> lineIndices = [ ];
        Vector4D<float> color = new( wireframe.Color.X, wireframe.Color.Y, wireframe.Color.Z, 1 );

        foreach ( (Vector3D<float>, Vector3D<float>) edge in uniqueEdges )
        {
            uint baseIdx = ( uint )lineVertices.Count;
            lineVertices.Add( new Vertex( edge.Item1, Vector2D<float>.Zero, color, 0, Vector3D<float>.Zero ) );
            lineVertices.Add( new Vertex( edge.Item2, Vector2D<float>.Zero, color, 0, Vector3D<float>.Zero ) );
            lineIndices.Add( baseIdx );
            lineIndices.Add( baseIdx + 1 );
        }

        // Создаем меш на основе топологии основной фигуры
        Mesh wireframeMesh = meshSystem.CreateMesh( lineVertices.ToArray(), lineIndices.ToArray() );
        wireframeMesh.Topology = PrimitiveType.Lines;
        World.Add( entity, wireframeMesh );
        World.Add( entity, materialSystem.CreateUnlit( wireframe.Color ) );
    }
}