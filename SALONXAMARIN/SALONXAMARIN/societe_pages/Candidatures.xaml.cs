using Firebase.Database;
using Firebase.Storage;
using Plugin.XamarinFormsSaveOpenPDFPackage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SALONXAMARIN.societe_pages
{

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Candidatures : ContentPage
    {
        private FirebaseClient firebase;
        private Person selectedPostulant;
        private MemoryStream pickedFileStream;
        private FirebaseStorage storage = new FirebaseStorage("projet-xamarin.appspot.com");
        private string uploadedLettreName;
        public Candidatures(Person selectedPostulant)
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            NavigationPage.SetHasBackButton(this, false);
            this.selectedPostulant = selectedPostulant;
            // Initialiser la connexion Firebase
            firebase = new FirebaseClient("https://projet-xamarin-default-rtdb.firebaseio.com/");

            //if (selectedPostulant.Sexe.Equals("masculin"))
            //{
            //    labelTitle.Text = "Les Postulations de Monsieur " + selectedPostulant.Prenom + " " + selectedPostulant.Nom;
            //}
            //else
            //{
            //    labelTitle.Text = "Les Postulations de Madame " + selectedPostulant.Prenom + " " + selectedPostulant.Nom;
            //}

            //try
            //{
            //    var CVLabel = FindLabelRecursively(this, "CVLabel");
            //    if (CVLabel != null)
            //    {
            //        var tapGestureRecognizer = new TapGestureRecognizer();
            //        tapGestureRecognizer.Tapped += CVLabel_Tapped;
            //        CVLabel.GestureRecognizers.Add(tapGestureRecognizer);
            //    }
            //    else
            //    {
            //        Console.WriteLine("CVLabel not found in XAML layout");
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("Exception: " + ex.Message);
            //}
            
            // Recursive function to find the Label
            //Label FindLabelRecursively(Element element, string labelAutomationId)
            //{
            //    if (element is Layout layout)
            //    {
            //        foreach (var child in layout.Children)
            //        {
            //            if (child is Label label && label.AutomationId == labelAutomationId)
            //            {
            //                return label;
            //            }
            //            else
            //            {
            //                var foundLabel = FindLabelRecursively(child, labelAutomationId);
            //                if (foundLabel != null)
            //                {
            //                    return foundLabel;
            //                }
            //            }
            //        }
            //    }
            //    return null;
            //}

            //try
            //{
            //    //Label CVLabel = (Label)this.FindByName("CVLabel");
            //    var CVLabel = (Label)FindByName("CVLabel");
            //    Console.WriteLine("CVLabel = " + CVLabel);
            //    var tapGestureRecognizer = new TapGestureRecognizer();
            //    Console.WriteLine("tapGestureRecognizer = " + tapGestureRecognizer);
            //    tapGestureRecognizer.Tapped += CVLabel_Tapped;
            //    CVLabel.GestureRecognizers.Add(tapGestureRecognizer);                
            //}

            //catch (Exception ex)
            //{
            //    Console.WriteLine("Exception in CVLabel" + ex.Message);
            //}

            var viewModel = new EmploisPostulantViewModel();

            // Récupération des emplois et assignation à la propriété Emplois du ViewModel
            Task.Run(async () =>
            {
                var emplois = await GetEmploisPostulant(this.selectedPostulant.PersonId);
                Device.BeginInvokeOnMainThread(() =>
                {
                    viewModel.Emplois = new ObservableCollection<Emploi>(emplois);
                });
            });
            BindingContext = viewModel;                        
        }

        private void CVLabel_Tapped(object sender, EventArgs e)
        {
            //// Access the sender, which is the Label that was tapped
            //var label = sender as Label;
            //if (label != null)
            //{
            //    // Find the parent Frame containing popupViewCV
            //    var parentFrame = label.Parent as Frame;
            //    if (parentFrame != null)
            //    {
            //        // Find popupViewCV inside the parent Frame
            //        var popupViewCV = parentFrame.FindByName<ContentView>("popupViewCV");
            //        if (popupViewCV != null)
            //        {
            //            // Toggle visibility of popupViewCV
            //            popupViewCV.IsVisible = !popupViewCV.IsVisible;
            //        }
            //    }
            //}
        }
        //private void lettreLabel_Tapped(object sender, EventArgs e)
        //{
        //    // Access the sender, which is the Label that was tapped
        //    var label = sender as Label;
        //    if (label != null)
        //    {
        //        // Find the parent Frame containing popupViewLettre
        //        var parentFrame = label.Parent as Frame;
        //        if (parentFrame != null)
        //        {
        //            // Find popupViewLettre inside the parent Frame
        //            var popupViewLettre = parentFrame.FindByName<ContentView>("popupViewLettre");
        //            if (popupViewLettre != null)
        //            {
        //                // Toggle visibility of popupViewLettre
        //                popupViewLettre.IsVisible = !popupViewLettre.IsVisible;
        //            }
        //        }
        //    }
        //}
        private async void AfficherButtonCV_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (selectedPostulant.CV_name.Equals(""))
                {
                    Console.WriteLine("selectedPostulant.CV_name.Equals(\"\")");
                    await DisplayAlert("Error", "Aucun document à afficher.", "OK");
                }

                if (pickedFileStream == null)
                {
                    Console.WriteLine("pickedFileStream == null");

                    // Sanitize the filename by removing spaces and special characters
                    string sanitizedFileName = selectedPostulant.CV_name;

                    // Check if the sanitized filename is not empty
                    if (!string.IsNullOrEmpty(sanitizedFileName))
                    {
                        Console.WriteLine("sanitizedFileName is ok");

                        // Construct the correct path
                        string filePath = $"{selectedPostulant.PersonId}/cv/{sanitizedFileName}";

                        // Attempt to get the download URL (check if the folder and file exist)
                        var downloadUrlTask = storage.Child(filePath).GetDownloadUrlAsync();
                        var downloadUrl = await downloadUrlTask.ConfigureAwait(false);

                        if (!string.IsNullOrEmpty(downloadUrl))
                        {
                            Console.WriteLine("downloadUrl is ok");
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
                if (selectedPostulant.CV_name.Equals("") && pickedFileStream == null)
                {
                    Console.WriteLine("selectedPostulant.CV_name.Equals(\"\") && pickedFileStream == null");
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

        //private async void AfficherButtonLettre_Clicked(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        if (pickedFileStream != null && !string.IsNullOrWhiteSpace(this.uploadedLettreName))
        //        {
        //            Console.WriteLine("pickedFileStream != null");
        //            await CrossXamarinFormsSaveOpenPDFPackage.
        //            Current.SaveAndView("Ma Lettre de motivation.pdf", "application/pdf", pickedFileStream, PDFOpenContext.InApp);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle other exceptions if needed
        //        Console.WriteLine("Exception in AfficherButtonLettre_Clicked: " + ex.ToString());
        //    }
        //}

        private async Task<List<Emploi>> GetEmploisPostulant(string personId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"personId: {personId}"); // ok

                var firebaseEmplois = await firebase.Child("Emplois").OnceAsync<Emploi>();

                System.Diagnostics.Debug.WriteLine($"firebaseEmplois: {string.Join(", ", firebaseEmplois.Select(item => item.Object))}");

                var firebaseCandidature = await firebase.Child("Candidatures").OnceAsync<Candidature>();

                System.Diagnostics.Debug.WriteLine($"firebaseCandidature: {string.Join(", ", firebaseCandidature.Select(item => item.Object))}");

                // Filter candidature list items based on the current user's id
                var userCandidature = firebaseCandidature
                    .Where(candidature => candidature.Object.PersonId == personId)
                    .ToList();

                // Get emploi ids from the user's candidature list
                var emploiIdsInUserCandidature = userCandidature.Select(candidature => candidature.Object.EmploiId).ToList();

                System.Diagnostics.Debug.WriteLine($"emploiIdsInUserCandidature: {string.Join(", ", emploiIdsInUserCandidature)}");

                // Filter emplois based on emploiIdsInUserCandidature
                List<Emploi> emplois = firebaseEmplois
                    .Where(item => emploiIdsInUserCandidature.Contains(item.Object.Id_emploi))
                    .Select(item => item.Object)
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"emplois: {emplois}");

                System.Diagnostics.Debug.WriteLine($"Emplois count: {emplois.Count}");

                return emplois;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"An error occurred: {ex.Message}");
                return new List<Emploi>();
            }
        }
        private async void Societe_Home_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MainPage_societe()); // Redirige vers la page du profil
        }                        
    }

    public class EmploisPostulantViewModel : BindableObject
    {
        private ObservableCollection<Emploi> _emplois;
        public ObservableCollection<Emploi> Emplois
        {
            get => _emplois;
            set
            {
                _emplois = value;
                OnPropertyChanged(nameof(Emplois));
            }
        }
    }


}