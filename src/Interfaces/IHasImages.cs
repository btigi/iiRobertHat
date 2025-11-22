using ii.RobertHat.Model;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ii.RobertHat.Interfaces;

public interface IHasImages
{
    List<(Image<Rgba32> image, CompressionType compressionType)> Images { get; set; }
}
