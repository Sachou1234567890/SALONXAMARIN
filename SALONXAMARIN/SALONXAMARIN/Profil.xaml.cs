//using System;
//using System.IO;
//using System.Net.Http;
//using System.Threading.Tasks;
//using Xamarin.Essentials;
//using Xamarin.Forms;
//using Xamarin.Forms.Xaml;

//namespace SALONXAMARIN
//{
//    [XamlCompilation(XamlCompilationOptions.Compile)]
//    public partial class Profil : ContentPage
//    {       
//        public Profil()
//        {
//            InitializeComponent();
//        }

//        private void CVLabel_Tapped(object sender, EventArgs e)
//        {
//            // Toggle the visibility of popupView
//            popupView.IsVisible = !popupView.IsVisible;
//        }


//        private async void DownloadButton_Clicked(object sender, EventArgs e)
//        {
//            // Determine the file type and download the file accordingly
//            await DownloadFile();
//        }

//        private async Task DownloadFile()
//        {
//            try
//            {
//                // Replace with the actual file path in Firebase Storage
//                var storageFilePath = "cv/cv_detaille.pdf";

//                // Generate a signed URL for the file
//                var signedUrl = await GenerateSignedUrl(storageFilePath);

//                // Check if the file type is supported (case-insensitive)
//                //if (signedUrl.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
//                //{
//                    // Download the file to the Downloads folder
//                    var downloadedFilePath = await DownloadFileAsync(signedUrl, "cv_detaille.pdf");

//                    // Display a message or perform further actions with the downloaded file
//                    await DisplayAlert("File Downloaded", $"The file has been downloaded to: {downloadedFilePath}", "OK");
//                //}
//                //else
//                //{
//                //    // Display a message or perform other actions for unsupported file types
//                //    await DisplayAlert("File Download Error", "Le fichier n'est pas un document pris en charge", "OK");
//                //}
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Error downloading file: {ex.Message}");
//                // Handle the error appropriately
//            }
//        }

//        private async Task<string> DownloadFileAsync(string fileUrl, string fileName)
//        {
//            try
//            {
//                //string downloadsFolderPath = Path.Combine(Environment.GetEnvironmentVariable(Environment.SystemDirectory DirectoryDownloads).AbsolutePath, "myFile.txt");
//                //string downloadsFolderPath = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, Android.OS.Environment.DirectoryDownloads);
//                //var downloadsFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
//                //var downloadPath = Path.Combine(downloadsFolderPath, fileName);

//                using (var httpClient = new HttpClient())
//                {
//                    var content = await httpClient.GetByteArrayAsync(fileUrl);
//                    File.WriteAllBytes(downloadPath, content);
//                }

//                return downloadPath;
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Error downloading file asynchronously: {ex.Message}");
//                throw;
//            }
//        }












//        private async void AfficherButton_Clicked(object sender, EventArgs e)
//        {
//            //var file = await Xamarin.Essentials.OpenFileAsync()

//             //Determine the file type and open the viewer accordingly
//            OpenFileViewer();
//        }                        
//        private async Task<string> GenerateSignedUrl(string filePath)
//        {
//            try
//            {
//                var storage = new FirebaseStorage("projet-xamarin.appspot.com");
//                return await storage
//                    .Child(filePath)
//                    .GetDownloadUrlAsync();
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Error generating signed URL: {ex.Message}");
//                throw;
//            }
//        }

//        private async void OpenFileViewer()
//        {
//            try
//            {
//                // Replace with the actual file path in Firebase Storage
//                var storageFilePath = "cv/cv_detaille.pdf";

//                // Generate a signed URL for the file
//                var signedUrl = await GenerateSignedUrl(storageFilePath);

//                // Check if the file type is supported (case-insensitive)
//                if (signedUrl.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
//                {
//                    // Use a PDF viewer (replace this with a different viewer if needed)
//                    await Launcher.OpenAsync(new OpenFileRequest
//                    {
//                        File = new ReadOnlyFile(signedUrl),
//                        Title = "afficher le PDF"
//                    });
//                }
//                else
//                {
//                    // Display a message or use a default viewer
//                    await DisplayAlert("File Viewer", "Le fichier n'est pas un document pris en charge", "OK");
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Error opening file: {ex.Message}");
//                // Handle the error appropriately
//            }
//        }



//        //private async Task<string> GenerateSignedUrl(string filePath)
//        //{
//        //    try
//        //    {
//        //        var task = new FirebaseStorage("projet-xamarin.appspot.com")
//        //            .Child(filePath)
//        //            .GetDownloadUrlAsync();

//        //        return await task;
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        Console.WriteLine($"Error generating signed URL: {ex.Message}");
//        //        throw;
//        //    }
//        //}

//        //private async void OpenFileViewer()
//        //{
//        //    try
//        //    {
//        //        // Replace with the actual file path in Firebase Storage
//        //        var storageFilePath = "cv/cv_detaille.pdf";

//        //        // Generate a signed URL for the file
//        //        var signedUrl = await GenerateSignedUrl(storageFilePath);

//        //        // Open the file viewer based on the file type
//        //        if (signedUrl.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
//        //        {
//        //            // Use a PDF viewer (replace this with a different viewer if needed)
//        //            await Launcher.OpenAsync(new OpenFileRequest
//        //            {
//        //                File = new ReadOnlyFile(signedUrl),
//        //                Title = "afficher le PDF"
//        //            });
//        //        }
//        //        // Add more conditions for other file types (e.g., images, documents) if needed
//        //        else
//        //        {
//        //            // Display a message or use a default viewer
//        //            await DisplayAlert("File Viewer", "Le fichier n'est pas un document pris en charge", "OK");
//        //        }
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        Console.WriteLine($"Error opening file: {ex.Message}");
//        //        // Handle the error appropriately
//        //    }
//        //}





//        private async void OnRemplacerIconClicked(object sender, EventArgs e)
//        {
//            try
//            {
//                // Perform file picking logic to select a PDF file
//                var pickedFile = await PickPdfFile();

//                if (pickedFile != null)
//                {
//                    // Update the CVLabel with the selected file name
//                    UpdateCVLabel(pickedFile.FileName);

//                    // Save the file to the "cv" folder
//                    await SaveFileToFirebaseStorage(pickedFile);
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Error picking or saving file: {ex.Message}");
//            }
//        }

//        private async Task SaveFileToFirebaseStorage(FileResult pickedFile)
//        {
//            try
//            {
//                // Create a Firebase Storage reference
//                var storage = new FirebaseStorage("projet-xamarin.appspot.com");
//                var storageRef = storage.Child("cv").Child(pickedFile.FileName);

//                // Open an input stream from the picked file
//                using (var sourceStream = await pickedFile.OpenReadAsync())
//                {
//                    // Upload the file to Firebase Storage
//                    var uploadTask = storageRef.PutAsync(sourceStream);

//                    // Wait for the upload to complete
//                    var downloadUrl = await uploadTask;

//                    Console.WriteLine($"File uploaded to {downloadUrl}");
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Error uploading file to Firebase Storage: {ex.Message}");
//            }
//        }

//        private async Task<FileResult> PickPdfFile()
//        {
//            return await FilePicker.PickAsync(new PickOptions
//            {
//                PickerTitle = "Sélectionner un fichier",
//                FileTypes = FilePickerFileType.Pdf, // Adjust file types as needed
//            });
//        }
//        private void UpdateCVLabel(string fileName)
//        {
//            CVLabel.Text = $"CV : {fileName}";
//        }
//        private void MoreOptionsButton_Clicked(object sender, EventArgs e)
//        {
//            // Toggle the visibility of popupView
//            popupView.IsVisible = !popupView.IsVisible;
//        }        
//        private async void Postes_enregistres_Clicked(object sender, EventArgs e)
//        {
//            await Navigation.PushAsync(new Postes_enregistres()); // Redirige vers la page du profil
//        }
//        private async void Profil_Clicked(object sender, EventArgs e)
//        {
//            await Navigation.PushAsync(new Profil()); // Redirige vers la page du profil
//        }                       
//    }    
//}