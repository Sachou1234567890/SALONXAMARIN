using System;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Firebase.Database;
using System.IO;
using Plugin.XamarinFormsSaveOpenPDFPackage;
using System.Threading.Tasks;
using Firebase.Storage;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using Firebase.Database.Query;

namespace SALONXAMARIN.candidats_pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Post_details : ContentPage
    {
        FirebaseClient firebase;
        private FirebaseStorage storage = new FirebaseStorage("projet-xamarin.appspot.com");
        private Emploi selectedJob;
        private Person currentUser;
        private MemoryStream pickedFileStream;
        private string uploadedCVName;
        private string uploadedLettreName;
        private Candidature candidature;
        private Stream fileStream;        
        public Post_details(Emploi selectedJob, Person currentUser)
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            NavigationPage.SetHasBackButton(this, false);
            
            firebase = new FirebaseClient("https://projet-xamarin-default-rtdb.firebaseio.com/");
            var viewModel = new PostDetailsViewModel(selectedJob);
            BindingContext = viewModel;
            this.selectedJob = selectedJob;
            this.candidature = new Candidature();

            // Call the asynchronous method to check the cv folder and set the label
            Device.BeginInvokeOnMainThread(async () => await CheckFolderAndSetCVLabelAsync());

            this.currentUser = currentUser;

            // Set the binding context for the button to the view model
            //postulerButton.SetBinding(Button.CommandProperty, nameof(viewModel.PostulerCommand));
        }

        private async void AddCV()
        {
            this.currentUser.PersonId = this.currentUser.PersonId;
            this.currentUser.Name = this.currentUser.Name;
            this.currentUser.Email = this.currentUser.Email;
            this.currentUser.Nom = this.currentUser.Nom;
            this.currentUser.Prenom = this.currentUser.Prenom;
            this.currentUser.Societe = this.currentUser.Societe;
            this.currentUser.Password = this.currentUser.Password;
            this.currentUser.CV_name = this.currentUser.CV_name;
            // this.currentUser.lettre_name = this.currentUser.lettre_name ;

            await UpdateCVInDatabase(this.currentUser);
        }
        private void lettreLabel_Tapped(object sender, EventArgs e)
        {
            popupViewLettre.IsVisible = !popupViewLettre.IsVisible;
        }
        private async void OnRemplacerLettreClicked(object sender, EventArgs e)
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
                    this.uploadedLettreName = Path.GetFileName(pickImage.FileName);

                    var stream = await pickImage.OpenReadAsync();
                    var memorystream = new MemoryStream();
                    await stream.CopyToAsync(memorystream);

                    // Set the text of lettreLabel to the sanitized file name
                    lettreLabel.Text = SanitizeFileName(uploadedLettreName);
                    
                    // Assign memorystream to pickedFileStream
                    pickedFileStream = memorystream;                    
                }
            }
            catch (Exception ex)
            {                
                Console.WriteLine("Exception in OnRemplacerLettreClicked: " + ex.ToString());                
            }
        }
        private async void AfficherButtonLettre_Clicked(object sender, EventArgs e)
        {
            try
            {                                
                if (pickedFileStream != null && !string.IsNullOrWhiteSpace(this.uploadedLettreName))
                {
                    Console.WriteLine("pickedFileStream != null");
                    await CrossXamarinFormsSaveOpenPDFPackage.
                    Current.SaveAndView("Ma Lettre de motivation.pdf", "application/pdf", pickedFileStream, PDFOpenContext.InApp);
                }                
            }
            catch (Exception ex)
            {
                // Handle other exceptions if needed
                Console.WriteLine("Exception in AfficherButtonLettre_Clicked: " + ex.ToString());                
            }
        }

        private async void AddToCandidature(object sender, EventArgs e)
        {
            try
            {
                this.candidature.CandidatureId = Guid.NewGuid().ToString();
                this.candidature.EmploiId = selectedJob.Id_emploi;
                this.candidature.PersonId = currentUser.PersonId;
                this.candidature.Statut = "en attente";
                this.candidature.CV = currentUser.CV_name;
                this.candidature.DatePostulation = DateTime.UtcNow;

                // vérifie si la lettre d'acceptation et le CV ont été uploadés
                if (!string.IsNullOrWhiteSpace(this.uploadedLettreName) && !string.IsNullOrWhiteSpace(this.uploadedCVName))
                {
                    // Check if a record with the same Id_emploi and PersonId already exists
                    if (await DoesCandidatureExist(this.candidature.EmploiId, this.candidature.PersonId))
                    {
                        // Handle the case where the record already exists
                        await DisplayAlert("Message", "Vous avez déjà postulé pour ce poste.", "OK");
                        Console.WriteLine("Une candidature avec le même couple EmploiId PersonId existe déjà.");
                    }
                    else
                    {                        
                        // Update your Firebase database or perform any other actions as needed
                        await UpdateCandidatureInDatabase(this.candidature);
                        await DisplayAlert("Message", "Postulation effectuée avec succés", "OK"); 
                    }
                }    
                else
                {
                    await DisplayAlert("Message", "vous n'avez pas uploadé les documents necessaires (CV et Lettre d'aceptation)", "OK");
                    Console.WriteLine("vous n'avez pas uploadé les documents necessaires (CV et Lettre d'aceptation)");
                }               
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in AddToCandidature: " + ex.ToString());
                // Optionally, display an error message to the user
                await DisplayAlert("Erreur", "Une erreur s'est produite lors de la soumission de votre candidature.", "OK");
            }
        }

        //private async void AddToCandidatureOLd(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        this.candidature.CandidatureId = Guid.NewGuid().ToString();
        //        this.candidature.EmploiId = selectedJob.Id_emploi;
        //        this.candidature.PersonId = currentUser.PersonId;
        //        this.candidature.Statut = "en attente";
        //        this.candidature.CV = currentUser.CV_name;

        //        // vérifie si la lettre d'acceptation a été uploadée 
        //        if (!string.IsNullOrWhiteSpace(uploadedLettreName))
        //        {
        //            this.candidature.Lettre = uploadedLettreName;
        //        }
        //        else
        //        {                    
        //            this.candidature.Lettre = "";
        //        }

        //        this.candidature.DatePostulation = DateTime.UtcNow;

        //        // Check if a record with the same Id_emploi and PersonId already exists
        //        if (await DoesCandidatureExist(this.candidature.EmploiId, this.candidature.PersonId))
        //        {
        //            // Handle the case where the record already exists
        //            await DisplayAlert("Message", "Vous avez déjà postulé pour ce poste.", "OK");
        //            Console.WriteLine("Une candidature avec le même couple EmploiId PersonId existe déjà.");
        //        }
        //        else
        //        {
        //            // Check if the letter has been uploaded
        //            if (string.IsNullOrWhiteSpace(uploadedLettreName))
        //            {
        //                await DisplayAlert("Message", "Vous n'avez pas uploadé une lettre d'acceptation.", "OK");
        //                Console.WriteLine("Vous n'avez pas uploadé une lettre d'acceptation.");
        //            }
        //            else
        //            {
        //                // Update your Firebase database or perform any other actions as needed
        //                await UpdateCandidatureInDatabase(this.candidature);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Exception in AddToCandidature: " + ex.ToString());
        //        // Optionally, display an error message to the user
        //        await DisplayAlert("Erreur", "Une erreur s'est produite lors de la soumission de votre candidature.", "OK");
        //    }
        //}


        private async Task<bool> DoesCandidatureExist(string EmploiId, string personId)
        {
            try
            {
                // Retrieve all candidatures from the database
                var allCandidatures = await firebase.Child("Candidatures")
                                                 .OnceSingleAsync<Dictionary<string, Candidature>>();

                // Check if there is any existing record with the specified EmploiId and PersonId
                return allCandidatures?.Values.Any(candidature =>
                    candidature.EmploiId == EmploiId && candidature.PersonId == personId) ?? false;
            }
            catch (Exception ex)
            {
                // Log the exception details or handle it appropriately
                Console.WriteLine("Exception in DoesCandidatureExist: " + ex.ToString());               
                return false; // Assuming no record exists in case of an exception
            }
        }

        private async Task UpdateCandidatureInDatabase(Candidature candidature)
        {
            try
            {                
                var candidatureRef = firebase.Child("Candidatures").Child(candidature.CandidatureId);

                // Directly set the values under the specified key
                await candidatureRef.PutAsync(candidature);
                
                Console.WriteLine("Candidature updated or created successfully");
                //await DisplayAlert("emploi enregistré avec succes", "emploi enregistré avec succes", "OK");
            }
            catch (Exception ex)
            {               
                Console.WriteLine("Exception in UpdateCandidatureInDatabase: " + ex.ToString());
                await DisplayAlert("message", "Exception in UpdateCandidatureInDatabase: " + ex.Message, "OK");
                // Optionally, you can rethrow the exception if you want it to be propagated
                // throw;
            }
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
                    await DisplayAlert("File Viewer", "File uploaded to Firebase Storage", "OK");
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
                await DisplayAlert("Error", $"Catch Error: {ex.Message}", "OK");
            }
        }

        private async void Enregistrer_ButtonCV_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (pickedFileStream != null && !string.IsNullOrEmpty(uploadedCVName))
                {
                    // Save the file to Firebase Storage using the sanitized uploaded file name
                    await SaveToFirebaseStorage(pickedFileStream, SanitizeFileName(uploadedCVName));

                    currentUser.CV_name = SanitizeFileName(uploadedCVName);
                    
                    AddCV();

                    //var currentUserRef = firebase.Child("Persons").Child(currentUser.CV_name);
                    //// Directly set the values under the specified key
                    //await currentUserRef.PutAsync(currentUser);

                    await DisplayAlert("OK", "File has been uploaded.", "OK");
                }
                else
                {
                    // Handle the case where no file has been picked or uploadedCVName is empty
                    await DisplayAlert("Error", "Aucun document à enregistrer.", "OK");
                    Console.WriteLine("Aucun document à enregistrer.");
                }
            }
            catch (Exception ex)
            {
                // Handle other exceptions if needed
                Console.WriteLine("Exception in Enregistrer_Button_Clicked: " + ex.ToString());
                //await DisplayAlert("Error", "Exception: " + ex.Message, "OK");
            }
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

        private async void OnRemplacerCVClicked(object sender, EventArgs e)
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
                    uploadedCVName = Path.GetFileName(pickImage.FileName);

                    var stream = await pickImage.OpenReadAsync();
                    var memorystream = new MemoryStream();
                    await stream.CopyToAsync(memorystream);

                    // Set the text of CVLabel to the sanitized file name
                    CVLabel.Text = SanitizeFileName(uploadedCVName);

                    this.currentUser.CV_name = SanitizeFileName(uploadedCVName);

                    // Assign memorystream to pickedFileStream
                    pickedFileStream = memorystream;

                    enregistrerButtonCV.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                // Handle other exceptions if needed
                Console.WriteLine("Exception in OnRemplacerIconClicked: " + ex.ToString());
                //await DisplayAlert("Error", "Exception: " + ex.Message, "OK");
            }
        }
        public async Task CheckFolderAndSetCVLabelAsync()
        {
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
                    enregistrerButtonCV.IsVisible = false;
                    CVLabel.Text = currentUser.CV_name;
                }
            }
            catch (Exception)
            {
                // Handle other exceptions if needed
                CVLabel.Text = "Aucun fichier trouvé";
            }
        }

        private void CVLabel_Tapped(object sender, EventArgs e)
        {            
            popupViewCV.IsVisible = !popupViewCV.IsVisible;
        }
        private async void AfficherButtonCV_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (currentUser.CV_name.Equals(""))
                {
                    Console.WriteLine("currentUser.CV_name.Equals(\"\")");
                    await DisplayAlert("Error", "Aucun document à afficher.", "OK");
                }

                if (pickedFileStream == null)
                {
                    Console.WriteLine("pickedFileStream == null");

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
                    Console.WriteLine("pickedFileStream != null");
                    await CrossXamarinFormsSaveOpenPDFPackage.
                    Current.SaveAndView("mon_cv.pdf", "application/pdf", pickedFileStream, PDFOpenContext.InApp);
                }
                if (currentUser.CV_name.Equals("") && pickedFileStream == null)
                {
                    Console.WriteLine("currentUser.CV_name.Equals(\"\") && pickedFileStream == null");
                    await DisplayAlert("Error", "Aucun document à afficher.", "OK");
                }
            }

            catch (Exception ex)
            {
                // Handle other exceptions if needed
                Console.WriteLine("Exception in AfficherButton_Clicked: " + ex.ToString());
                //await DisplayAlert("Error", "Exception: " + ex.Message, "OK");
            }           
        }

        private async void Home_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Postes_acceuil(currentUser)); // Redirige vers la page du profil
        }
        private async void Profil_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Profil(currentUser)); // Redirige vers la page du profil
        }





    public class PostDetailsViewModel : BindableObject
        {
            private Emploi _selectedJob;
            private readonly FirebaseClient firebase;

            public Emploi SelectedJob
            {
                get => _selectedJob;
                set
                {
                    _selectedJob = value;
                    OnPropertyChanged(nameof(SelectedJob));
                }
            }

            //public ICommand PostulerCommand { get; private set; }

            public PostDetailsViewModel(Emploi selectedJob)
            {
                SelectedJob = selectedJob;
                firebase = new FirebaseClient("https://projet-xamarin-default-rtdb.firebaseio.com/");

                //PostulerCommand = new Command(async () => await ExecutePostulerCommand());
            }

            //private async Task ExecutePostulerCommand()
            //{
            //    // Update the 'postule' property to true
            //    SelectedJob.Postule = true;

            //    // Update the data in the Firebase Realtime Database
            //    await firebase
            //        .Child("Emplois")
            //        .Child(SelectedJob.Id_emploi)
            //        .PutAsync(SelectedJob);

            //    // Notify the UI of the changes
            //    OnPropertyChanged(nameof(SelectedJob));
            //}
        }
    }
}
