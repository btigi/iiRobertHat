using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ii.RobertHat.Model;

public class Toggle : Resource
{
    public int Unknown { get; set; }
    public List<Image<Rgba32>> Images { get; set; } = [];
}
