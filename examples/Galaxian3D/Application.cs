using Aether.Core.Options;
using Graphics;

namespace Galaxian3D;

public class Application( 
    string title,
    int width, 
    int height, 
    BaseOptions? worldOptions = null,
    bool fullScreen = false ) : ApplicationBase
    ( 
        title, 
        width, 
        height, 
        worldOptions, 
        fullScreen )
{
    protected override void OnInitialize()
    {
        throw new NotImplementedException();
    }
}