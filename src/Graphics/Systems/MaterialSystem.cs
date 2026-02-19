using Graphics.Components;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using ShaderComponent = Graphics.Components.Shader;
using Texture2DComponent = Graphics.Components.Texture2D;

namespace Graphics.Systems;

public class MaterialSystem( ShaderSystem shaderSystem, TextureSystem textureSystem )
{
    public Material CreateMaterial( ShaderComponent shader, Texture2DComponent texture, Vector4D<float> color )
    {
        return new Material
        {
            Shader = shader,
            Texture = texture,
            Color = color
        };
    }

    public Material CreateMaterial( ShaderComponent shader, Texture2DComponent texture )
    {
        return CreateMaterial( shader, texture, new Vector4D<float>( 1, 1, 1, 1 ) );
    }

    public void UseMaterial( Material material )
    {
        shaderSystem.UseShader( material.Shader );
        textureSystem.BindTexture( material.Texture, TextureUnit.Texture0 );
    }

    public void SetMaterialColor( ref Material material, Vector4D<float> color )
    {
        material.Color = color;
    }

    public void SetMaterialTexture( ref Material material, Texture2DComponent texture )
    {
        material.Texture = texture;
    }

    public void SetMaterialShader( ref Material material, ShaderComponent shader )
    {
        material.Shader = shader;
    }
}