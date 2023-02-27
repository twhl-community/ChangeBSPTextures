using Sledge.Formats.Bsp;

namespace ChangeBSPTextures
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: ChangeBSPTextures <filename>");
                return;
            }

            string fileName = args[0];

            var stream = File.OpenRead(fileName);

            var bspFile = new BspFile(stream);

            // Close stream now because we'll be writing our changes to the same file.
            stream.Close();

            // Modify texture names.
            var texture = bspFile.Textures.FirstOrDefault(t => t.Name == "subwayfloor2");

            if (texture is not null)
            {
                texture.Name = "out_pav3";

                // Clear out embedded texture data so the texture is loaded from a wad file.
                // If the wad file isn't in use by the map then it will need to be added to the worldspawn "wad" keyvalue.
                texture.NumMips = 0;
                texture.MipData = Array.Empty<byte[]>();
                texture.Palette = Array.Empty<byte>();
            }

            var worldspawn = bspFile.Entities[0];

            if (!worldspawn.KeyValues.TryGetValue("wad", out var wads))
            {
                wads = "";
            }

            // Original HL maps reference a non-existent wad file that will cause a fatal error, so remove it.
            var wadList = wads.Split(';').ToList();

            wadList = wadList.Where(w => !w.Contains("sample.wad")).ToList();

            //wadList.Add("mywad.wad");

            wads = string.Join(';', wadList);

            worldspawn.KeyValues["wad"] = wads;

            // Change filename to different one for testing
            fileName = Path.Combine(Path.GetDirectoryName(fileName) ?? "", Path.GetFileNameWithoutExtension(fileName) + "_modified.bsp");

            using var destination = File.Open(fileName, FileMode.Create);

            bspFile.WriteToStream(destination, Sledge.Formats.Bsp.Version.Goldsource);
        }
    }
}