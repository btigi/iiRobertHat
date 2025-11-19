using ii.RobertHat.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ii.RobertHat.Model;

public class Cursor : Resource, IHasImages
{
    public int Unknown { get; set; }
    public int Unknown2 { get; set; }
    public int Unknown3 { get; set; }
    public List<Image<Rgba32>> Images { get; set; } = [];
}
