using Firebase.Database;
using Firebase.Database.Query;
using SALONXAMARIN.Droid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

    namespace SALONXAMARIN
    {

	    [XamlCompilation(XamlCompilationOptions.Compile)]
	    public partial class Postes_acceuil : ContentPage
	    {
            FirebaseClient firebase;

            Dictionary<string, List<string>> emploiParEvenement;
            public Postes_acceuil()
            {
                InitializeComponent();
                //collection_jobs.SelectionChanged += OnJobSelected;
                // Initialiser la connexion Firebase
                firebase = new FirebaseClient("https://projet-xamarin-default-rtdb.firebaseio.com/");

                var viewModel = new EmploiViewModel();

                // Récupération des emplois et assignation à la propriété Emplois du ViewModel
                Task.Run(async () =>
                {
                    var emplois = await GetAllEmplois();
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        viewModel.Emplois = new ObservableCollection<Emploi>(emplois);
                    });
                });
                BindingContext = viewModel;
            }

            private async Task<List<Emploi>> GetAllEmplois()
            {
                var firebaseEmplois = await firebase.Child("Emplois").OnceAsync<Emploi>();

                // Convert Firebase objects to Emploi objects and assign unique IDs if not present
                List<Emploi> emplois = firebaseEmplois.Select(item =>
                {
                    Emploi emploi = item.Object;
                        
                    // Check if the ID is not set and assign a new Guid
                    if (string.IsNullOrEmpty(emploi.Id_emploi))
                    {
                        emploi.Id_emploi = Guid.NewGuid().ToString();
                        // You may want to update the entry in the database with the new ID
                        firebase.Child("Emplois").Child(emploi.Id_emploi).PutAsync(emploi);
                    }

                    return emploi;
                }).ToList();

                return emplois;
            }

        private async void AddToWishList(object sender, EventArgs e)
        {
            // Get the tapped Emploi object
            if (sender is Image tappedImage && tappedImage.BindingContext is Emploi tappedEmploi)
            {
                // Set the Wishlist property to true
                tappedEmploi.Wishlist = true;

                // Update your Firebase database or perform any other actions as needed
                await UpdateEmploiInDatabase(tappedEmploi);
            }
        }
      
        private async Task UpdateEmploiInDatabase(Emploi emploi)
        {
            // Ensure the ID is not empty or null
            if (!string.IsNullOrEmpty(emploi.Id_emploi))
            {
                Console.WriteLine("Updating entry with Id_emploi: " + emploi.Id_emploi);

                //var firebase = new FirebaseClient("your_firebase_url");

                var emploiRef = firebase.Child("Emplois").Child(emploi.Id_emploi);

                // Get the current data to ensure atomicity
                var currentData = await emploiRef.OnceSingleAsync<Emploi>();

                // Ensure the entry exists before attempting to update
                if (currentData != null)
                {
                    // Perform an atomic update using the current data
                    await emploiRef.PutAsync(new
                    {
                        Id_emploi = emploi.Id_emploi,
                        Titre = emploi.Titre,
                        Description = emploi.Description,
                        Responsabilites = emploi.Responsabilites,
                        Salaire_horaire = emploi.Salaire_horaire,
                        Wishlist = true, // Add or update other fields as needed
                    });
                }
                else
                {
                    Console.WriteLine("Entry with Id_emploi " + emploi.Id_emploi + " not found.");
                }
            }
            else
            {
                // Handle the case where the ID is not set
                Console.WriteLine("Cannot update an entry without a valid ID.");
            }
        }






            private async void Profil_Clicked(object sender, EventArgs e)
            {
                await Navigation.PushAsync(new Profil()); // Redirige vers la page du profil
            }
            //private async void Post_details_Clicked(object sender, EventArgs e)
            //{
            //    await Navigation.PushAsync(new Post_details()); // Redirige vers la page detaillée du post
            //}

            private async void RefreshButton_Clicked(object sender, EventArgs e)
            {
                var viewModel = (EmploiViewModel)BindingContext;

                // Retrieve all jobs again
                var allEmplois = await GetAllEmplois();
                viewModel.Emplois = new ObservableCollection<Emploi>(allEmplois);

                // Reset the label text back to default
                labelTitle.Text = "Tous les postes";
                searchEntry.Text = string.Empty;
            }

            private async void OnJobSelected(object sender, EventArgs e)
            {
                if (sender is Frame selectedFrame && selectedFrame.BindingContext is Emploi selectedJob)
                {
                    // Do something with the selected job
                    // For example, navigate to the Post_details page with the selected job
                    await Navigation.PushAsync(new Post_details(selectedJob));
                    //await Navigation.PushAsync(new Post_details());

                    // Manually deselect the item to remove the highlight
                    collection_jobs.SelectedItem = null;
                }
            }

            private async void SearchButton_Clicked(object sender, EventArgs e)
            {
                string searchText = searchEntry.Text;

                var viewModel = (EmploiViewModel)BindingContext;

                if (!string.IsNullOrEmpty(searchText))
                {
                    // Filter jobs based on the search text using regular expressions
                    var filteredJobs = viewModel.Emplois.Where(job =>
                        System.Text.RegularExpressions.Regex.IsMatch(job.Titre, searchText, System.Text.RegularExpressions.RegexOptions.IgnoreCase) ||
                        System.Text.RegularExpressions.Regex.IsMatch(job.Description, searchText, System.Text.RegularExpressions.RegexOptions.IgnoreCase)
                    // Add more fields to search through if needed
                    ).ToList();

                    // Update the ObservableCollection with filtered jobs
                    viewModel.Emplois = new ObservableCollection<Emploi>(filteredJobs);

                    // Change the label text to indicate filtered search
                    labelTitle.Text = "Postes Correspondants à votre recherche";                
                }
                else
                {
                    // If the search field is empty, display all jobs
                    var allEmplois = await GetAllEmplois();
                    viewModel.Emplois = new ObservableCollection<Emploi>(allEmplois);

                    // Change the label text back to default
                    labelTitle.Text = "Tous les postes";
                }
            }
        }

        public class EmploiViewModel : BindableObject
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