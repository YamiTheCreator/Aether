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

    public Material CreateMatte( Vector3D<float> color )
    {
        return new Material
        {
            AmbientColor = color * 0.2f,
            DiffuseColor = color,
            SpecularColor = new Vector3D<float>( 0.1f, 0.1f, 0.1f ),
            Shininess = 8.0f,
            Alpha = 1.0f
        };
    }

    public Material CreatePlastic( Vector3D<float> color, float shininess = 32.0f )
    {
        return new Material
        {
            AmbientColor = color * 0.2f,
            DiffuseColor = color,
            SpecularColor = new Vector3D<float>( 0.5f, 0.5f, 0.5f ),
            Shininess = shininess,
            Alpha = 1.0f
        };
    }

    public Material CreateMetal( Vector3D<float> color, float shininess = 128.0f )
    {
        return new Material
        {
            AmbientColor = color * 0.1f,
            DiffuseColor = color * 0.5f,
            SpecularColor = new Vector3D<float>( 1.0f, 1.0f, 1.0f ),
            Shininess = shininess,
            Alpha = 1.0f
        };
    }

    public Material CreateShiny( Vector3D<float> color )
    {
        return new Material
        {
            AmbientColor = color * 0.2f,
            DiffuseColor = color,
            SpecularColor = new Vector3D<float>( 0.8f, 0.8f, 0.8f ),
            Shininess = 64.0f,
            Alpha = 1.0f
        };
    }

    public Material CreatePhong( Vector3D<float> ambient, Vector3D<float> diffuse, Vector3D<float> specular,
        float shininess, float alpha = 1.0f )
    {
        return new Material
        {
            AmbientColor = ambient,
            DiffuseColor = diffuse,
            SpecularColor = specular,
            Shininess = shininess,
            Alpha = alpha
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

    public Material CreateTextured( Texture2D texture, Vector4D<float> color )
    {
        return new Material
        {
            AmbientColor = new Vector3D<float>( color.X, color.Y, color.Z ),
            DiffuseColor = new Vector3D<float>( color.X, color.Y, color.Z ),
            SpecularColor = new Vector3D<float>( 0.0f, 0.0f, 0.0f ),
            Shininess = 1.0f,
            Texture = texture,
            Alpha = color.W
        };
    }

    public Material CreateTextured( Texture2D texture )
    {
        return CreateTextured( texture, new Vector3D<float>( 1.0f, 1.0f, 1.0f ) );
    }

    public Material CreateTexturedLit( Texture2D texture )
    {
        return new Material
        {
            AmbientColor = new Vector3D<float>( 0.4f, 0.4f, 0.4f ), // Умеренный ambient для объема
            DiffuseColor = new Vector3D<float>( 1.0f, 1.0f, 1.0f ), // White - no tint
            SpecularColor = new Vector3D<float>( 0.3f, 0.3f, 0.3f ), // Заметный specular
            Shininess = 32.0f,
            EmissionColor = Vector3D<float>.Zero,
            EmissionIntensity = 0.0f,
            Texture = texture,
            Shader = null,
            Alpha = 1.0f
        };
    }

    public Material CreateEmissive( Texture2D texture, float intensity = 1.0f )
    {
        return new Material
        {
            AmbientColor = new Vector3D<float>( 1.0f, 1.0f, 1.0f ),
            DiffuseColor = new Vector3D<float>( 1.0f, 1.0f, 1.0f ),
            SpecularColor = Vector3D<float>.Zero, // No specular on emissive
            Shininess = 1.0f,
            EmissionColor = new Vector3D<float>( 1.0f, 1.0f, 1.0f ), // White emission
            EmissionIntensity = intensity,
            Texture = texture,
            Shader = null,
            Alpha = 1.0f
        };
    }

    public Material CreateEmissive( Vector3D<float> color, float intensity = 1.0f )
    {
        return new Material
        {
            AmbientColor = color,
            DiffuseColor = color,
            SpecularColor = Vector3D<float>.Zero,
            Shininess = 1.0f,
            EmissionColor = color,
            EmissionIntensity = intensity,
            Texture = null,
            Shader = null,
            Alpha = 1.0f
        };
    }
}