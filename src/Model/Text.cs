using ii.RobertHat.Interfaces;

namespace ii.RobertHat.Model;

public class Text : Resource, IHasTextEntries
{
    public int Unknown { get; set; }
    public List<string> TextEntries { get; set; } = [];
}
