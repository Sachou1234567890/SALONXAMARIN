//using Android.Content;
//using SALONXAMARIN.Droid;
//using System;
//using System.IO;
//using Xamarin.Forms;

//[assembly: Dependency(typeof(FileOpener))]
//namespace SALONXAMARIN.Droid
//{
//    public class FileOpener : IFileOpener
//    {
//        public void OpenFile(Stream fileStream, string fileName)
//        {
//            // Convert the stream to a byte array
//            byte[] byteArray = ((MemoryStream)fileStream).ToArray();

//            // Save the byte array to a file
//            string filePath = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, fileName);
//            File.WriteAllBytes(filePath, byteArray);

//            // Use an Intent to open the file with the default PDF viewer
//            Intent intent = new Intent(Intent.ActionView);
//            intent.SetDataAndType(Android.Net.Uri.Parse("file://" + filePath), "application/pdf");
//            intent.SetFlags(ActivityFlags.ClearWhenTaskReset | ActivityFlags.NewTask);

//            try
//            {
//                Xamarin.Forms.Forms.Context.StartActivity(intent);
//            }
//            catch (Exception ex)
//            {
//                // Handle exceptions
//            }
//        }
//    }
//}
