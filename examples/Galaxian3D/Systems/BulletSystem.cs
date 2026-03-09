using Aether.Core;
using Silk.NET.OpenGL;

namespace Galaxian3D.Systems;

public class BulletSystem : SystemBase
{
    private GL _gl;
    protected override void OnCreate()
    {
        _gl = World.GetGlobal<GL>();
    }
    protected override void OnUpdate(float deltaTime)
    {
        
    }

    protected override void OnRender()
    {
        
    }
    
    protected override void OnDestroy()
    {
        
    }
}