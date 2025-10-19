using SharpCompress.Compressors;
using SharpCompress.Compressors.BZip2;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ii.RobertHat;

public class MapProcessor
{
    public Image<Rgba32> Read(string filename)
    {
        using FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
        using BinaryReader reader = new BinaryReader(fs);

        var width = reader.ReadInt16();
        var height = reader.ReadInt16();

        _ = reader.ReadBytes(8);

        var compressedDataLength = fs.Length - 12;
        var compressedData = reader.ReadBytes((int)compressedDataLength);

        // Decompress using BZip2
        byte[] decompressedData;
        using (MemoryStream compressedStream = new(compressedData))
        using (BZip2Stream bz2Stream = new(compressedStream, CompressionMode.Decompress, false))
        using (MemoryStream decompressedStream = new())
        {
            bz2Stream.CopyTo(decompressedStream);
            decompressedData = decompressedStream.ToArray();
        }

        var image = ConvertRGB565ToImage(decompressedData, width, height);
        return image;
    }

    static Image<Rgba32> ConvertRGB565ToImage(byte[] rgb565Data, int width, int height)
    {
        var image = new Image<Rgba32>(width, height);

        var dataIndex = 0;
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                if (dataIndex + 1 < rgb565Data.Length)
                {
                    // Read 16-bit RGB565 value (little-endian)
                    ushort rgb565 = (ushort)(rgb565Data[dataIndex] | (rgb565Data[dataIndex + 1] << 8));
                    dataIndex += 2;

                    // Extract RGB components (5-6-5 bits)
                    var r5 = (rgb565 >> 11) & 0x1F;
                    var g6 = (rgb565 >> 5) & 0x3F;
                    var b5 = rgb565 & 0x1F;

                    // Convert to 8-bit values
                    var r = (byte)((r5 * 255 + 15) / 31);
                    var g = (byte)((g6 * 255 + 31) / 63);
                    var b = (byte)((b5 * 255 + 15) / 31);

                    // Set pixel in RGBA format
                    image[x, y] = new Rgba32(r, g, b, 255);
                }
            }
        }

        return image;
    }
}