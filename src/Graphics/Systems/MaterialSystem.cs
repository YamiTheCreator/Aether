using Aether.Core;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Graphics.Components;
using Graphics.Structures;

namespace Graphics.Systems;

public class MaterialSystem : SystemBase
{
    public Material CreateUnlit( Vector3D<float> color )
    {
        return new Material
        {
            AmbientColor = color * 0.3f,
            DiffuseColor = color,
            SpecularColor = new Vector3D<float>( 0.2f, 0.2f, 0.2f ),
            Shininess = 8.0f,
            Alpha = 1.0f
        };
    }

    public void BindMaterial( ref Material material, ShaderProgram shader )
    {
        Texture2D whiteTexture = World.GetGlobal<Texture2D>();

        shader.TrySetUniform( "uColor", new Vector4D<float>(
            material.DiffuseColor.X,
            material.DiffuseColor.Y,
            material.DiffuseColor.Z,
            material.Alpha ) );

        shader.TrySetUniform( "uMaterialAmbient", material.AmbientColor );
        shader.TrySetUniform( "uMaterialDiffuse", material.DiffuseColor );
        shader.TrySetUniform( "uMaterialSpecular", material.SpecularColor );
        shader.TrySetUniform( "uShininess", material.Shininess );
        shader.TrySetUniform( "uAlpha", material.Alpha );

        shader.TrySetUniform( "uEmissionColor", material.EmissionColor );
        shader.TrySetUniform( "uEmissionIntensity", material.EmissionIntensity );

        shader.TrySetUniform( "uMetallic", material.Metallic );
        shader.TrySetUniform( "uRoughness", material.Roughness );

        if ( material.Texture != null )
        {
            material.Texture.Value.Texture.Bind();
            shader.TrySetUniform( "uTexture", 0 );
            shader.TrySetUniform( "uHasTexture", 1 );
        }
        else
        {
            whiteTexture.Texture.Bind();
            shader.TrySetUniform( "uTexture", 0 );
            shader.TrySetUniform( "uHasTexture", 0 );
        }

        if ( material.NormalMap != null )
        {
            material.NormalMap.Value.Texture.Bind( TextureUnit.Texture1 );
            shader.TrySetUniform( "uNormalMap", 1 );
            shader.TrySetUniform( "uHasNormalMap", 1 );
        }
        else
        {
            shader.TrySetUniform( "uHasNormalMap", 0 );
        }

        if ( material.MetallicMap != null )
        {
            material.MetallicMap.Value.Texture.Bind( TextureUnit.Texture2 );
            shader.TrySetUniform( "uMetallicMap", 2 );
            shader.TrySetUniform( "uHasMetallicMap", 1 );
        }
        else
        {
            shader.TrySetUniform( "uHasMetallicMap", 0 );
        }

        if ( material.RoughnessMap != null )
        {
            material.RoughnessMap.Value.Texture.Bind( TextureUnit.Texture3 );
            shader.TrySetUniform( "uRoughnessMap", 3 );
            shader.TrySetUniform( "uHasRoughnessMap", 1 );
        }
        else
        {
            shader.TrySetUniform( "uHasRoughnessMap", 0 );
        }

        if ( material.AmbientOcclusionMap != null )
        {
            material.AmbientOcclusionMap.Value.Texture.Bind( TextureUnit.Texture4 );
            shader.TrySetUniform( "uAOMap", 4 );
            shader.TrySetUniform( "uHasAOMap", 1 );
        }
        else
        {
            shader.TrySetUniform( "uHasAOMap", 0 );
        }

        if ( material.EmissiveMap != null )
        {
            material.EmissiveMap.Value.Texture.Bind( TextureUnit.Texture5 );
            shader.TrySetUniform( "uEmissiveMap", 5 );
            shader.TrySetUniform( "uHasEmissiveMap", 1 );
        }
        else
        {
            shader.TrySetUniform( "uHasEmissiveMap", 0 );
        }
    }

    public Material CreateTextured( Texture2D texture, Vector3D<float> color )
    {
        return new Material
        {
            AmbientColor = color,
            DiffuseColor = color,
            SpecularColor = new Vector3D<float>( 0.0f, 0.0f, 0.0f ),
            Shininess = 1.0f,
            Texture = texture,
            Alpha = 1.0f
        };
    }

    public Material CreateEmissive( Texture2D texture, float intensity = 1.0f )
    {
        return new Material
        {
            AmbientColor = new Vector3D<float>( 1.0f, 1.0f, 1.0f ),
            DiffuseColor = new Vector3D<float>( 1.0f, 1.0f, 1.0f ),
            SpecularColor = Vector3D<float>.Zero,
            Shininess = 1.0f,
            EmissionColor = new Vector3D<float>( 1.0f, 1.0f, 1.0f ),
            EmissionIntensity = intensity,
            Texture = texture,
            Shader = null,
            Alpha = 1.0f
        };
    }
}