using Aether.Core;

namespace BouncingLetters.Components;

public struct Letter( char character ) : IComponent
{
    public readonly char Character = character;
}