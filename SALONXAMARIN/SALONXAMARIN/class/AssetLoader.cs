using System;
using System.IO;
using Xamarin.Forms;
using SALONXAMARIN; // Change to your correct namespace
using SALONXAMARIN.Droid;

[assembly: Dependency(typeof(AssetLoader))]
namespace SALONXAMARIN
{
    public class AssetLoader : IAssetLoader
    {
        public Stream GetAsset(string filename)
        {
            var assembly = typeof(AssetLoader).Assembly;

            // Replace "SALONXAMARIN.courstuto_key.json" with your actual namespace and file name
            var stream = assembly.GetManifestResourceStream("SALONXAMARIN.courstuto_key.json");

            return stream;
        }
    }
}
