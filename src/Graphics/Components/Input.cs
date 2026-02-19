using Silk.NET.Input;

namespace Graphics.Components;

public struct Input
{
    public IKeyboard Keyboard { get; set; }
    public IMouse Mouse { get; set; }
    public HashSet<Key> PreviousKeys { get; set; }
    public HashSet<Key> CurrentKeys { get; set; }
}