iiRobertHat
=====

iiRobertHat is a C# library supporting the modification of files relating to Lionheart - Legacy of the Crusader, the 2003 RPG game developed by Reflexive Entertainment.
The library supports:

| Name     | Read | Write | Comment |
|----------|:----:|-------|:--------|
| BFN      | ✗   |   ✗   | Font related
| BKS      | ✗   |   ✗   | 
| DAT      | ✗   |   ✗   | Sound related
| DIC      | ✗   |   ✗   | 
| FXG      | ✗   |   ✗   | 
| MAP      | ✔   |   ✗   | Area graphics
| MIN      | ✗   |   ✗   | Area related
| PAK      | ✗   |   ✗   | 
| RED      | ✗   |   ✗   | 
| RES      | ✔   |   ✗   | Archive (ui graphics, sounds, text)
| RHM      | ✗   |   ✗   | 
| RHP      | ✗   |   ✗   | 
| RHS      | ✗   |   ✗   | 
| SCB      | ✗   |   ✗   | Level script
| SFK      | ✗   |   ✗   | Music related
| TNF      | ✗   |   ✗   | Font related


## Usage

Instantiate the relevant class and call the `Read` method passing the filename.

```csharp
var mapProcessor = new MapProcessor();

var image = mapProcessor.Read(@"D:\Games\Robin Hood - The Legend of Sherwood\DATA\Levels\Day\Croisement01.map");
image.SaveAsJpeg(@"D:\data\Robin Hood - The Legend of Sherwood\Croisement01.jpg");



var resources = resProcessor.Read(@"D:\Games\Robin Hood - The Legend of Sherwood\2047\data\Text\level.res");
var idx = 0;
foreach (var r in resources)
{
    if (r is IHasImages images)
    {
        var idx2 = 0;
        foreach (var i in images.Images)
        {
            i.SaveAsJpeg(@$"D:\data\robinhood\{idx}_{idx2}.jpg");
            idx2++;
        }
    }

    if (r is IHasTextEntries texts)
    {
        foreach (var i in texts.TextEntries)
        {

        }
    }
    idx++;
}
```


## Compiling

To clone and run this application, you'll need [Git](https://git-scm.com) and [.NET](https://dotnet.microsoft.com/) installed on your computer. From your command line:

```
# Clone this repository
$ git clone https://github.com/btigi/iiRobertHat

# Go into the repository
$ cd src

# Build  the app
$ dotnet build
```


## Licencing

iiRobertHat is licenced under the MIT License. Full licence details are available in licence.md