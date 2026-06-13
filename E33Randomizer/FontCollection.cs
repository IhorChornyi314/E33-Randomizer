using Avalonia.Media.Fonts;

namespace E33Randomizer;


public sealed class FontCollection : EmbeddedFontCollection
{
    public FontCollection() : base(
        new Uri("fonts:E33Fonts", UriKind.Absolute),
        new Uri("avares://E33Randomizer/Assets/Fonts", UriKind.Absolute))
    {
    }
}