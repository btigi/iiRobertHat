using ii.RobertHat.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ii.RobertHat.Model;

public class Picture : Resource, IHasImages
{
    public int Unknown { get; set; }
    public List<(Image<Rgba32> image, CompressionType compressionType)> Images { get; set; } = [];
}
