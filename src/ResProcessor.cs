using ii.RobertHat.Model;
using SharpCompress.Compressors;
using SharpCompress.Compressors.BZip2;
using SharpCompress.Compressors.Deflate;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Text;
using CompressionType = ii.RobertHat.Model.CompressionType;

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
                        var (img, compType) = GetImage(reader);
                        button.Images.Add((img, compType));
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
                        var (img, compType) = GetImage(reader);
                        cursor.Images.Add((img, compType));
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
                        var (img, compType) = GetImage(reader);
                        input.Images.Add((img, compType));
                    }
                    resources.Add(input);
                    break;
                case "PIC ":
                    var picture = new Picture();
                    picture.ResourceId = reader.ReadInt32();
                    picture.Unknown = reader.ReadInt32();
                    var (picImg, picCompType) = GetImage(reader);
                    picture.Images.Add((picImg, picCompType));
                    resources.Add(picture);
                    break;
                case "PICC":
                    var pictureMultiple = new PictureMultiple();
                    pictureMultiple.ResourceId = reader.ReadInt32();
                    pictureMultiple.Unknown = reader.ReadInt32();
                    var pictureMultipleCount = reader.ReadInt32();
                    for (var index = 0; index < pictureMultipleCount; index++)
                    {
                        var (img, compType) = GetImage(reader);
                        pictureMultiple.Images.Add((img, compType));
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
                        var (img, compType) = GetImage(reader);
                        radioButton.Images.Add((img, compType));
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
                        var (img, compType) = GetImage(reader);
                        slider.Images.Add((img, compType));
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
                        var (img, compType) = GetImage(reader);
                        toggle.Images.Add((img, compType));
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

    private static byte[] DecompressZlib(byte[] compressedData)
    {
        // Skip the first 2 bytes (zlib header)
        using var compressedStream = new MemoryStream(compressedData, 2, compressedData.Length - 2);
        using var deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress);
        using var decompressedStream = new MemoryStream();
        deflateStream.CopyTo(decompressedStream);
        return decompressedStream.ToArray();
    }

    public (Image<Rgba32> image, CompressionType compressionType) GetImage(BinaryReader reader)
    {
        var width = reader.ReadInt16();
        var height = reader.ReadInt16();
        var compressionTypeValue = reader.ReadInt32();
        var compressionType = GetCompressionType(compressionTypeValue);
        var compressedSize = reader.ReadInt32();
        var compressedData = reader.ReadBytes(compressedSize);

        var imageData = DecompressData(compressedData, compressionType);
        var image = ReadImage(width, height, imageData);
        return (image, compressionType);
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

    public void Write(List<Resource> resources, string filename, int version = 1)
    {
        using var fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
        using var writer = new BinaryWriter(fs);

        writer.Write(Encoding.ASCII.GetBytes("SRES"));
        writer.Write(version);
        writer.Write(resources.Count);

        foreach (var resource in resources)
        {
            switch (resource)
            {
                case Button button:
                    WriteButton(writer, button);
                    break;
                case Cursor cursor:
                    WriteCursor(writer, cursor);
                    break;
                case Input input:
                    WriteInput(writer, input);
                    break;
                case Picture picture:
                    WritePicture(writer, picture);
                    break;
                case PictureMultiple pictureMultiple:
                    WritePictureMultiple(writer, pictureMultiple);
                    break;
                case RadioButton radioButton:
                    WriteRadioButton(writer, radioButton);
                    break;
                case Slider slider:
                    WriteSlider(writer, slider);
                    break;
                case Text text:
                    WriteText(writer, text);
                    break;
                case Toggle toggle:
                    WriteToggle(writer, toggle);
                    break;
                case Wave wave:
                    WriteWave(writer, wave);
                    break;
                default:
                    throw new NotSupportedException($"Resource type {resource.GetType().Name} is not supported for writing.");
            }
        }
    }

    private static void WriteButton(BinaryWriter writer, Button button)
    {
        writer.Write(Encoding.ASCII.GetBytes("BTTN"));
        writer.Write(button.ResourceId);
        writer.Write(button.Unknown);
        var bitmask = CreateBitmask(button.Images.Count, 4);
        writer.Write(bitmask);
        foreach (var (image, compressionType) in button.Images)
        {
            WriteImage(writer, image, compressionType);
        }
    }

    private static void WriteCursor(BinaryWriter writer, Cursor cursor)
    {
        writer.Write(Encoding.ASCII.GetBytes("CUR "));
        writer.Write(cursor.ResourceId);
        writer.Write(cursor.Unknown);
        writer.Write(cursor.Unknown2);
        writer.Write(cursor.Unknown3);
        writer.Write(cursor.Images.Count);
        foreach (var (image, compressionType) in cursor.Images)
        {
            WriteImage(writer, image, compressionType);
        }
    }

    private static void WriteInput(BinaryWriter writer, Input input)
    {
        writer.Write(Encoding.ASCII.GetBytes("NPTF"));
        writer.Write(input.ResourceId);
        writer.Write(input.Unknown);
        var bitmask = CreateBitmask(input.Images.Count, 6);
        writer.Write(bitmask);
        foreach (var (image, compressionType) in input.Images)
        {
            WriteImage(writer, image, compressionType);
        }
    }

    private static void WritePicture(BinaryWriter writer, Picture picture)
    {
        writer.Write(Encoding.ASCII.GetBytes("PIC "));
        writer.Write(picture.ResourceId);
        writer.Write(picture.Unknown);
        if (picture.Images.Count > 0)
        {
            var (image, compressionType) = picture.Images[0];
            WriteImage(writer, image, compressionType);
        }
    }

    private static void WritePictureMultiple(BinaryWriter writer, PictureMultiple pictureMultiple)
    {
        writer.Write(Encoding.ASCII.GetBytes("PICC"));
        writer.Write(pictureMultiple.ResourceId);
        writer.Write(pictureMultiple.Unknown);
        writer.Write(pictureMultiple.Images.Count);
        foreach (var (image, compressionType) in pictureMultiple.Images)
        {
            WriteImage(writer, image, compressionType);
        }
    }

    private static void WriteRadioButton(BinaryWriter writer, RadioButton radioButton)
    {
        writer.Write(Encoding.ASCII.GetBytes("RDO "));
        writer.Write(radioButton.ResourceId);
        writer.Write(radioButton.Unknown);
        var bitmask = CreateBitmask(radioButton.Images.Count, 7);
        writer.Write(bitmask);
        foreach (var (image, compressionType) in radioButton.Images)
        {
            WriteImage(writer, image, compressionType);
        }
    }

    private static void WriteSlider(BinaryWriter writer, Slider slider)
    {
        writer.Write(Encoding.ASCII.GetBytes("SLID"));
        writer.Write(slider.ResourceId);
        writer.Write(slider.Unknown);
        var bitmask = CreateBitmask(slider.Images.Count, 6);
        writer.Write(bitmask);
        foreach (var (image, compressionType) in slider.Images)
        {
            WriteImage(writer, image, compressionType);
        }
    }

    private static void WriteText(BinaryWriter writer, Text text)
    {
        writer.Write(Encoding.ASCII.GetBytes("TEXT"));
        writer.Write(text.ResourceId);
        writer.Write(text.Unknown);
        writer.Write((short)text.TextEntries.Count);
        foreach (var textEntry in text.TextEntries)
        {
            var textBytes = Encoding.Unicode.GetBytes(textEntry);
            writer.Write((short)textEntry.Length);
            writer.Write(textBytes);
        }
    }

    private static void WriteToggle(BinaryWriter writer, Toggle toggle)
    {
        writer.Write(Encoding.ASCII.GetBytes("TOGL"));
        writer.Write(toggle.ResourceId);
        writer.Write(toggle.Unknown);
        var bitmask = CreateBitmask(toggle.Images.Count, 5);
        writer.Write((short)bitmask);
        foreach (var (image, compressionType) in toggle.Images)
        {
            WriteImage(writer, image, compressionType);
        }
    }

    private static void WriteWave(BinaryWriter writer, Wave wave)
    {
        writer.Write(Encoding.ASCII.GetBytes("WAVE"));
        writer.Write(wave.ResourceId);
        writer.Write(wave.Unknown);
        writer.Write((short)wave.TextEntries.Count);
        foreach (var textEntry in wave.TextEntries)
        {
            var textBytes = Encoding.Default.GetBytes(textEntry);
            writer.Write((short)textBytes.Length);
            writer.Write(textBytes);
        }
    }

    private static int CreateBitmask(int imageCount, int bitCount)
    {
        // Create a bitmask with the first imageCount bits set
        var bitmask = 0;
        for (var i = 0; i < imageCount && i < bitCount; i++)
        {
            bitmask |= 1 << i;
        }
        return bitmask;
    }

    private static void WriteImage(BinaryWriter writer, Image<Rgba32> image, CompressionType compressionType)
    {
        var width = (short)image.Width;
        var height = (short)image.Height;
        writer.Write(width);
        writer.Write(height);

        var imageData = ConvertImageToRgb565(image);
        var compressedData = CompressData(imageData, compressionType);
        var compressionTypeValue = compressionType switch
        {
            CompressionType.None => 0,
            CompressionType.Zlib => 1,
            CompressionType.Bzip2 => 2,
            _ => throw new ArgumentOutOfRangeException(nameof(compressionType), compressionType, "Unknown compression type")
        };
        writer.Write(compressionTypeValue);
        writer.Write(compressedData.Length);
        writer.Write(compressedData);
    }

    private static byte[] ConvertImageToRgb565(Image<Rgba32> image)
    {
        var data = new byte[image.Width * image.Height * 2];
        var index = 0;

        for (var y = 0; y < image.Height; y++)
        {
            for (var x = 0; x < image.Width; x++)
            {
                var pixel = image[x, y];
                var rgb565 = ConvertRgba32ToRgb565(pixel);
                data[index++] = (byte)(rgb565 & 0xFF);
                data[index++] = (byte)((rgb565 >> 8) & 0xFF);
            }
        }

        return data;
    }

    private static ushort ConvertRgba32ToRgb565(Rgba32 pixel)
    {
        // Convert 8-bit RGB to 5-6-5 bits
        var r5 = (pixel.R >> 3) & 0x1F;
        var g6 = (pixel.G >> 2) & 0x3F;
        var b5 = (pixel.B >> 3) & 0x1F;

        return (ushort)((r5 << 11) | (g6 << 5) | b5);
    }

    private static byte[] CompressData(byte[] data, CompressionType compressionType)
    {
        return compressionType switch
        {
            CompressionType.None => data,
            CompressionType.Zlib => CompressZlib(data),
            CompressionType.Bzip2 => CompressBzip2(data),
            _ => throw new ArgumentOutOfRangeException(nameof(compressionType), compressionType, "Unknown compression type")
        };
    }

    private static byte[] CompressZlib(byte[] data)
    {
        using var outputStream = new MemoryStream();
        
        // Write zlib header (2 bytes)
        // CMF: 0x78 (deflate, 32K window)
        // FLG: 0x9C (default compression, no dictionary, checksum)
        outputStream.WriteByte(0x78);
        outputStream.WriteByte(0x9C);
        
        using var deflateStream = new DeflateStream(outputStream, CompressionMode.Compress);
        deflateStream.Write(data);
        deflateStream.Close();
        return outputStream.ToArray();
    }

    private static byte[] CompressBzip2(byte[] data)
    {
        using var outputStream = new MemoryStream();
        using var bz2Stream = new BZip2Stream(outputStream, CompressionMode.Compress, false);
        bz2Stream.Write(data);
        bz2Stream.Close();
        return outputStream.ToArray();
    }
}