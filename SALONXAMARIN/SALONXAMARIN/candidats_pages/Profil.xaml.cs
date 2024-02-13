using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Storage;
using Plugin.XamarinFormsSaveOpenPDFPackage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SALONXAMARIN.candidats_pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Profil : ContentPage
    {
        private string userId;
        private FirebaseClient firebase;
        private FirebaseStorage storage = new FirebaseStorage("projet-xamarin.appspot.com");
        private Person currentUser;
        Stream fileStream;
        MemoryStream pickedFileStream;
        private string uploadedFileName;
        public Profil(Person currentUser)
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            NavigationPage.SetHasBackButton(this, false);

            firebase = new FirebaseClient("https://projet-xamarin-default-rtdb.firebaseio.com/");

            nom.Text = currentUser.Nom;
            prenom.Text = currentUser.Prenom;
            email.Text = currentUser.Email;
            societe.Text = currentUser.Societe;
            
            this.currentUser = currentUser;

            // Call the asynchronous method to check the folder and set the label
            Device.BeginInvokeOnMainThread(async () => await CheckFolderAndSetLabelAsync());

        }

        private async void OnRemplacerIconClicked(object sender, EventArgs e)
        {
            try
            {                
                var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        {DevicePlatform.iOS, new[]{ "com.adobe.pdf" } },
                        {DevicePlatform.Android , new[]{"application/pdf"} },
                        {DevicePlatform.UWP , new[]{".pdf"} },
                        {DevicePlatform.Tizen , new[]{"*"} },
                        {DevicePlatform.macOS , new[]{"pdf"} },
                    });

                var pickImage = await FilePicker.PickAsync(new PickOptions()
                {
                    FileTypes = customFileType,
                    PickerTitle = "Pick a PDF"
                });

                if (pickImage != null)
                {
                    // Extract the file name from the picked file
                    uploadedFileName = Path.GetFileName(pickImage.FileName);

                    var stream = await pickImage.OpenReadAsync();
                    var memorystream = new MemoryStream();
                    await stream.CopyToAsync(memorystream);

                    // Set the text of CVLabel to the sanitized file name
                    CVLabel.Text = SanitizeFileName(uploadedFileName);

                    this.currentUser.CV_name = SanitizeFileName(uploadedFileName);

                    // Assign memorystream to pickedFileStream
                    pickedFileStream = memorystream;
                    
                    enregistrerButton.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                // Handle other exceptions if needed
                Console.WriteLine("Exception in OnRemplacerIconClicked: " + ex.ToString());
                //await DisplayAlert("Error", "Exception: " + ex.Message, "OK");
            }
        }


        public async Task CheckFolderAndSetLabelAsync()
        {
            Console.WriteLine("CheckFolderAndSetLabelAsync: currentUser.CV_name = " + currentUser.CV_name);
            Console.WriteLine("CheckFolderAndSetLabelAsync: currentUser.PersonId = " + currentUser.PersonId);
            
            try
            {
                // Check if the folder ${currentUser.PersonId}/cv exists
                var folderReference = storage.Child($"{currentUser.PersonId}/cv");
                var downloadUrlTask = folderReference.Child(currentUser.CV_name).GetDownloadUrlAsync();

                Console.WriteLine("folderReference = " + folderReference);
                Console.WriteLine("downloadUrlTask = " + downloadUrlTask);


                // Wait for the task to complete without blocking the UI thread
                var downloadUrl = await downloadUrlTask;

                if (!string.IsNullOrEmpty(downloadUrl))
                {
                    // Set the text of CVLabel based on the existence of the file
                    enregistrerButton.IsVisible = false;
                    CVLabel.Text = currentUser.CV_name;
                }
            }
            catch (Exception)
            {
                // Handle other exceptions if needed
                CVLabel.Text = "Aucun fichier trouvé";
            }
        }


        private async Task<Person> GetUserFromDatabase(string userId)
        {
            FirebaseClient firebase = new FirebaseClient("https://projet-xamarin-default-rtdb.firebaseio.com/");

            var userSnapshot = await firebase.Child("Persons").Child(userId).OnceSingleAsync<Person>();

            return userSnapshot;
        }

        private async void UpdateUserButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new UPDATEUSER(currentUser.PersonId));
        }

        private string ExtractFileNameFromPath(string filePath)
        {
            return Path.GetFileName(filePath);
        }

        private async void AfficherButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (currentUser.CV_name.Equals(""))
                {
                    await DisplayAlert("Error", "Aucun document à afficher.", "OK");
                }

                //else
                //{
                if (pickedFileStream == null)
                {
                    // Sanitize the filename by removing spaces and special characters
                    string sanitizedFileName = currentUser.CV_name;

                    // Check if the sanitized filename is not empty
                    if (!string.IsNullOrEmpty(sanitizedFileName))
                    {
                        // Construct the correct path
                        string filePath = $"{currentUser.PersonId}/cv/{sanitizedFileName}";

                        // Attempt to get the download URL (check if the folder and file exist)
                        var downloadUrlTask = storage.Child(filePath).GetDownloadUrlAsync();
                        var downloadUrl = await downloadUrlTask.ConfigureAwait(false);

                        if (!string.IsNullOrEmpty(downloadUrl))
                        {
                            try
                            {
                                using (var httpClient = new HttpClient())
                                {
                                    var fileStream = await httpClient.GetStreamAsync(downloadUrl);
                                    var memoryStream = new MemoryStream();
                                    await fileStream.CopyToAsync(memoryStream);
                                    memoryStream.Position = 0;

                                    // Save the MemoryStream to a temporary file
                                    string tempFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), sanitizedFileName);
                                    File.WriteAllBytes(tempFilePath, memoryStream.ToArray());

                                    // Open the saved file using Xamarin.Essentials Launcher
                                    await Launcher.OpenAsync(new OpenFileRequest
                                    {
                                        File = new ReadOnlyFile(tempFilePath)
                                    });
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Exception while opening PDF: " + ex.ToString());
                                // Handle the exception if needed
                                // You might want to use another approach if this fails
                            }
                        }
                    }
                }
                if (pickedFileStream != null)
                {
                    await CrossXamarinFormsSaveOpenPDFPackage.
                    Current.SaveAndView("mon_cv.pdf", "application/pdf", pickedFileStream, PDFOpenContext.InApp);
                }                
            }


            catch (Exception ex)
            {
                // Handle other exceptions if needed
                Console.WriteLine("Exception in AfficherButton_Clicked: " + ex.ToString());
                //await DisplayAlert("Error", "Exception: " + ex.Message, "OK");
            }            
        }
        private async void Place_Clicked(object sender, EventArgs e)
        {
            // Pass the PersonId to the Place page
            await Navigation.PushAsync(new Place(currentUser.PersonId));
        }
        private async void entreprise_Clicked(object sender, EventArgs e)
        {
            // Pass the PersonId to the Place page
            await Navigation.PushAsync(new entreprise());
        }

        private async Task SaveToFirebaseStorage(MemoryStream stream, string fileName)
        {
            try
            {
                // Reset the stream position to the beginning
                stream.Seek(0, SeekOrigin.Begin);

                // Specify the path where you want to store the file in Firebase Storage
                string path = $"{currentUser.PersonId}/cv/{SanitizeFileName(fileName)}";

                // Extract the file name from the path
                string extractedFileName = Path.GetFileName(fileName);

                // Set the text of CVLabel to the extracted file name
                CVLabel.Text = SanitizeFileName(extractedFileName);

                // Upload the file to Firebase Storage
                var response = await storage.Child(path).PutAsync(stream);

                if (response != null)
                {
                    // File uploaded successfully
                    //await DisplayAlert("File Viewer", "File uploaded to Firebase Storage", "OK");
                }
                else
                {
                    // Handle the case when the file upload fails
                    await DisplayAlert("Error", "Error", "OK");
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur during the process
                //await DisplayAlert("Error", $"Catch Error: {ex.Message}", "OK");
            }
        }

        private string SanitizeFileName(string fileName)
        {
            // Remove invalid characters for Firebase Storage path
            char[] invalidChars = Path.GetInvalidFileNameChars();
            string sanitizedFileName = new string(fileName
                .Where(c => !invalidChars.Contains(c) && c != '/')
                .ToArray());

            // Ensure the filename doesn't end with '/'
            sanitizedFileName = sanitizedFileName.TrimEnd('/');

            // Replace spaces with underscores
            sanitizedFileName = sanitizedFileName.Replace(' ', '_');

            return sanitizedFileName;
        }

        private async void AddCV()
        {            
                this.currentUser.PersonId = this.currentUser.PersonId;
                this.currentUser.Name = this.currentUser.Name ;
                this.currentUser.Email = this.currentUser.Email ;
                this.currentUser.Nom = this.currentUser.Nom ;
                this.currentUser.Prenom = this.currentUser.Prenom ;
                this.currentUser.Societe = this.currentUser.Societe ;
                this.currentUser.Password = this.currentUser.Password ;
                this.currentUser.CV_name = this.currentUser.CV_name ;                
                                                    
                await UpdateCVInDatabase(this.currentUser);                            
        }

        private async Task UpdateCVInDatabase(Person currentUser)
        {
            try
            {                
                var currentUserRef = firebase.Child("Persons").Child(currentUser.PersonId);

                // Directly set the values under the specified key
                await currentUserRef.PutAsync(currentUser);

                // Optionally, you can log success or return a success message
                // Console.WriteLine("CV updated or created success  fully");
                await DisplayAlert("CV enregistré avec succes", "CV enregistré avec succes", "OK");
            }
            catch (Exception ex)
            {
                // Log the exception details or handle it appropriately
                Console.WriteLine("Exception in UpdateCVInDatabase: " + ex.ToString());
                await DisplayAlert("message", "Exception in UpdateCVInDatabase: " + ex.Message, "OK");
                // Optionally, you can rethrow the exception if you want it to be propagated
                // throw;
            }
        }
        private async void Enregistrer_Button_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (pickedFileStream != null && !string.IsNullOrEmpty(uploadedFileName))
                {
                    // Save the file to Firebase Storage using the sanitized uploaded file name
                    await SaveToFirebaseStorage(pickedFileStream, SanitizeFileName(uploadedFileName));

                    this.currentUser.CV_name = SanitizeFileName(uploadedFileName);

                    AddCV();

                    //var currentUserRef = firebase.Child("Persons").Child(currentUser.CV_name);
                    //// Directly set the values under the specified key
                    //await currentUserRef.PutAsync(currentUser);

                    //await DisplayAlert("OK", "File has been uploaded.", "OK");
                }
                else
                {
                    // Handle the case where no file has been picked or uploadedFileName is empty
                    await DisplayAlert("Error", "Aucun document à enregistrer.", "OK");
                }
            }
            catch (Exception ex)
            {
                // Handle other exceptions if needed
                Console.WriteLine("Exception in Enregistrer_Button_Clicked: " + ex.ToString());
                //await DisplayAlert("Error", "Exception: " + ex.Message, "OK");
            }
        }
        
        private async void Logout_Clicked(object sender, EventArgs e)
        {
            //FirebaseAuth.Instance.SignOut(); // requires the installation of a package
            await Navigation.PushAsync(new CONNEXION()); // Redirige vers la page CONNEXION
        }
        private void CVLabel_Tapped(object sender, EventArgs e)
        {
            // Toggle the visibility of popupView
            popupView.IsVisible = !popupView.IsVisible;
        }        
        private async void Postes_enregistres_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Postes_enregistres(currentUser)); // Redirige vers la page du profil
        }
        private async void Postes_en_attente_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Postes_en_attente(currentUser)); // Redirige vers la page du profil
        }

        private async void Postes_acceptes_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Postes_acceptes(currentUser)); // Redirige vers la page du profil
        }
        private async void Postes_refuses_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Postes_refuses(currentUser)); // Redirige vers la page du profil
        }        
        private async void Home_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Postes_acceuil(currentUser)); // Redirige vers la page du profil
        }
        private async void Profil_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Profil(currentUser)); // Redirige vers la page du profil
        }
    }


}