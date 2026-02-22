using Silk.NET.Input;

namespace Graphics.Components;

public class Input
{
    public IKeyboard Keyboard { get; set; } = null!;
    public IMouse Mouse { get; set; } = null!;
    public HashSet<Key> PreviousKeys { get; set; } = [ ];
    public HashSet<Key> CurrentKeys { get; set; } = [ ];
}