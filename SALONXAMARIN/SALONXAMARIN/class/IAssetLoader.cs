using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SALONXAMARIN.Droid
{
    // Interface to define a method to access assets
    public interface IAssetLoader
    {
        Stream GetAsset(string filename);
    }

}
