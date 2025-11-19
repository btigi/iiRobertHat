using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ii.RobertHat.Interfaces;

public interface IHasImages
{
    List<Image<Rgba32>> Images { get; set; }
}
