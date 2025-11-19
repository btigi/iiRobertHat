using ii.RobertHat.Interfaces;

namespace ii.RobertHat.Model;

public class Wave : Resource, IHasTextEntries
{
    public int Unknown { get; set; }
    public List<string> TextEntries { get; set; } = [];
}
