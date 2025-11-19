using ii.RobertHat.Model;
using SharpCompress.Compressors;
using SharpCompress.Compressors.BZip2;
using SharpCompress.Compressors.Deflate;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Text;

namespace ii.RobertHat;

public class ResProcessor
{
    public List<Resource> Read(string filename)
    {
        using var fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(fs);

        var signatureBytes = reader.ReadBytes(4);
        var signature = Encoding.ASCII.GetString(signatureBytes);
        if (signature != "SRES")
        {
            throw new InvalidDataException("Invalid RES file signature.");
        }

        var version = reader.ReadInt32();
        var fileCount = reader.ReadInt32();

        Image<Rgba32>? image = null;
        var resources = new List<Resource>();
        for (var i = 0; i < fileCount; i++)
        {
            var fileTypeBytes = reader.ReadBytes(4);
            var fileType = Encoding.ASCII.GetString(fileTypeBytes);
            switch (fileType)
            {
                case "BTTN":
                    var button = new Button();
                    button.ResourceId = reader.ReadInt32();
                    button.Unknown = reader.ReadInt32();
                    var buttonCount = reader.ReadInt32();
                    var buttonImageCount = CountSetBits(buttonCount, 4);
                    for (var index = 0; index < buttonImageCount; index++)
                    {
                        image = GetImage(reader);
                        button.Images.Add(image);
                    }
                    resources.Add(button);
                    break;
                case "CUR ":
                    var cursor = new Cursor();
                    cursor.ResourceId = reader.ReadInt32();
                    cursor.Unknown = reader.ReadInt32();
                    cursor.Unknown2 = reader.ReadInt32();
                    cursor.Unknown3 = reader.ReadInt32();
                    var cursorCount = reader.ReadInt32();
                    for (var index = 0; index < cursorCount; index++)
                    {
                        image = GetImage(reader);
                        cursor.Images.Add(image);
                    }
                    resources.Add(cursor);
                    break;
                case "NPTF":
                    var input = new Input();
                    input.ResourceId = reader.ReadInt32();
                    input.Unknown = reader.ReadInt32();
                    var inputCount = reader.ReadInt32();
                    var inputImageCount = CountSetBits(inputCount, 6);
                    for (var index = 0; index < inputImageCount; index++)
                    {
                        image = GetImage(reader);
                        input.Images.Add(image);
                    }
                    resources.Add(input);
                    break;
                case "PIC ":
                    var picture = new Picture();
                    picture.ResourceId = reader.ReadInt32();
                    picture.Unknown = reader.ReadInt32();
                    image = GetImage(reader);
                    picture.Images.Add(image);
                    resources.Add(picture);
                    break;
                case "PICC":
                    var pictureMultiple = new PictureMultiple();
                    pictureMultiple.ResourceId = reader.ReadInt32();
                    pictureMultiple.Unknown = reader.ReadInt32();
                    var pictureMultipleCount = reader.ReadInt32();
                    for (var index = 0; index < pictureMultipleCount; index++)
                    {
                        image = GetImage(reader);
                        pictureMultiple.Images.Add(image);
                    }
                    resources.Add(pictureMultiple);
                    break;
                case "RDO ":
                    var radioButton = new RadioButton();
                    radioButton.ResourceId = reader.ReadInt32();
                    radioButton.Unknown = reader.ReadInt32();
                    var radioCount = reader.ReadInt32();
                    var radioImageCount = CountSetBits(radioCount, 7);
                    for (var index = 0; index < radioImageCount; index++)
                    {
                        image = GetImage(reader);
                        radioButton.Images.Add(image);
                    }
                    resources.Add(radioButton);
                    break;
                case "SLID":
                    var slider = new Slider();
                    slider.ResourceId = reader.ReadInt32();
                    slider.Unknown = reader.ReadInt32();
                    var slidCount = reader.ReadInt32();
                    int slidImageCount = CountSetBits(slidCount, 6);
                    for (var index = 0; index < slidImageCount; index++)
                    {
                        image = GetImage(reader);
                        slider.Images.Add(image);
                    }
                    resources.Add(slider);
                    break;
                case "TEXT":
                    var text = new Text();
                    text.ResourceId = reader.ReadInt32();
                    text.Unknown = reader.ReadInt32();
                    var textCount = reader.ReadInt16();
                    for (var index = 0; index < textCount; index++)
                    {
                        var textEntryCount = reader.ReadInt16();
                        text.TextEntries.Add(Encoding.Unicode.GetString(reader.ReadBytes(textEntryCount * 2)));
                    }
                    resources.Add(text);
                    break;
                case "TOGL":
                    var toggle = new Toggle();
                    toggle.ResourceId = reader.ReadInt32();
                    toggle.Unknown = reader.ReadInt32();
                    var toggleCount = reader.ReadInt16();
                    var toggleImageCount = CountSetBits(toggleCount, 5);
                    for (var index = 0; index < toggleImageCount; index++)
                    {
                        image = GetImage(reader);
                        toggle.Images.Add(image);
                    }
                    resources.Add(toggle);
                    break;
                case "WAVE":
                    var wave = new Wave();
                    wave.ResourceId = reader.ReadInt32();
                    wave.Unknown = reader.ReadInt32();
                    var waveCount = reader.ReadInt16();
                    for (var index = 0; index < waveCount; index++)
                    {
                        var waveEntryCount = reader.ReadInt16();
                        wave.TextEntries.Add(Encoding.Default.GetString(reader.ReadBytes(waveEntryCount)));
                    }
                    resources.Add(wave);
                    break;
            }
        }

        return resources;
    }

    private static int CountSetBits(int bitmask, int bitCount)
    {
        var count = 0;
        for (var index = 0; index < bitCount; index++)
        {
            if ((1 << index & bitmask) > 0)
                count++;
        }
        return count;
    }

    private enum CompressionType
    {
        Zlib,
        Bzip2,
        None,
    }

    private static byte[] DecompressZlib(byte[] compressedData)
    {
        // Skip the first 2 bytes (zlib header)
        using var compressedStream = new MemoryStream(compressedData, 2, compressedData.Length - 2);
        using var deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress);
        using var decompressedStream = new MemoryStream();
        deflateStream.CopyTo(decompressedStream);
        return decompressedStream.ToArray();
    }

    public Image<Rgba32> GetImage(BinaryReader reader)
    {
        var width = reader.ReadInt16();
        var height = reader.ReadInt16();
        var compressionType = GetCompressionType(reader.ReadInt32());
        var compressedSize = reader.ReadInt32();
        var compressedData = reader.ReadBytes(compressedSize);

        var imageData = DecompressData(compressedData, compressionType);
        return ReadImage(width, height, imageData);
    }

    private static CompressionType GetCompressionType(int value)
    {
        return value switch
        {
            0 => CompressionType.None,
            1 => CompressionType.Zlib,
            2 => CompressionType.Bzip2,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown compression type value")
        };
    }

    private static byte[] DecompressData(byte[] compressedData, CompressionType compressionType)
    {
        return compressionType switch
        {
            CompressionType.Zlib => DecompressZlib(compressedData),
            CompressionType.Bzip2 => DecompressBzip2(compressedData),
            CompressionType.None => compressedData,
            _ => throw new ArgumentOutOfRangeException(nameof(compressionType), compressionType, "Unknown compression type")
        };
    }

    private static byte[] DecompressBzip2(byte[] compressedData)
    {
        using var compressedStream = new MemoryStream(compressedData);
        using var bz2Stream = new BZip2Stream(compressedStream, CompressionMode.Decompress, false);
        using var decompressedStream = new MemoryStream();
        bz2Stream.CopyTo(decompressedStream);
        return decompressedStream.ToArray();
    }

    private static Image<Rgba32> ReadImage(int width, int height, byte[] imageData)
    {
        var image = new Image<Rgba32>(width, height);
        var expectedSize = width * height * 2; // 2 bytes per pixel (RGB565)

        if (imageData.Length < expectedSize)
        {
            throw new InvalidDataException($"Image data size ({imageData.Length}) is less than expected ({expectedSize})");
        }

        var span = imageData.AsSpan();
        var pixelIndex = 0;

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var rgb565 = ReadUInt16LittleEndian(span, pixelIndex);
                pixelIndex += 2;

                var pixel = ConvertRgb565ToRgba32(rgb565);
                image[x, y] = pixel;
            }
        }

        return image;
    }

    private static ushort ReadUInt16LittleEndian(Span<byte> span, int index)
    {
        return (ushort)(span[index] | (span[index + 1] << 8));
    }

    private static Rgba32 ConvertRgb565ToRgba32(ushort rgb565)
    {
        // Extract RGB components (5-6-5 bits)
        var r5 = (rgb565 >> 11) & 0x1F;
        var g6 = (rgb565 >> 5) & 0x3F;
        var b5 = rgb565 & 0x1F;

        // Convert to 8-bit values with proper scaling
        var r = (byte)((r5 * 255 + 15) / 31);
        var g = (byte)((g6 * 255 + 31) / 63);
        var b = (byte)((b5 * 255 + 15) / 31);

        return new Rgba32(r, g, b, 255);
    }
}