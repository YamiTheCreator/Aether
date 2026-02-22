using Aether.Core;
using Graphics.Components;
using Silk.NET.Maths;

namespace Graphics.Systems;

public class LightingSystem : SystemBase
{
    protected override void OnCreate()
    {
    }

    protected override void OnUpdate( float deltaTime )
    {
    }

    public Light CreatePoint( Vector3D<float> diffuseColor, float intensity = 1.0f, float range = 10.0f )
    {
        return new Light
        {
            Enabled = true,
            AmbientColor = diffuseColor * 0.2f,
            DiffuseColor = diffuseColor,
            SpecularColor = new Vector3D<float>( 1.0f, 1.0f, 1.0f ),
            Intensity = intensity
        };
    }

    public Light CreatePointFull( Vector3D<float> ambientColor, Vector3D<float> diffuseColor,
        Vector3D<float> specularColor, float intensity = 1.0f, float range = 10.0f )
    {
        return new Light
        {
            Enabled = true,
            AmbientColor = ambientColor,
            DiffuseColor = diffuseColor,
            SpecularColor = specularColor,
            Intensity = intensity
        };
    }
}