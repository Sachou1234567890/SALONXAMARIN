using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;


namespace SALONXAMARIN.candidats_pages
{

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Postes_acceptes : ContentPage
    {
        FirebaseClient firebase;
        private Person currentUser;

        Dictionary<string, List<string>> emploiParEvenement;
        public Postes_acceptes(Person currentUser)
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            NavigationPage.SetHasBackButton(this, false);

            firebase = new FirebaseClient("https://projet-xamarin-default-rtdb.firebaseio.com/");

            var viewModel = new Emploi_acceptesViewModel();
            //var viewModel = new EmploiViewModel();

            // Récupération des emplois en attente et assignation à la propriété Emplois du ViewModel
            Task.Run(async () =>
            {
                var emplois = await GetEmploisAcceptes(currentUser.PersonId);
                Device.BeginInvokeOnMainThread(() =>
                {
                    viewModel.Emplois = new ObservableCollection<Emploi>(emplois);
                });
            });
            BindingContext = viewModel;

            this.currentUser = currentUser;
        }
        private async void OnJobSelected(object sender, EventArgs e)
        {
            if (sender is Frame selectedFrame && selectedFrame.BindingContext is Emploi selectedJob)
            {
                await Navigation.PushAsync(new Post_details(selectedJob, currentUser));
            }
        }
        private async void Home_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Postes_acceuil(currentUser)); // Redirige vers la page du profil
        }
        private async Task<List<Emploi>> GetEmploisAcceptes(string personId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"personId: {personId}");

                var firebaseEmplois = await firebase.Child("Emplois").OnceAsync<Emploi>();

                System.Diagnostics.Debug.WriteLine($"firebaseEmplois: {string.Join(", ", firebaseEmplois.Select(item => item.Object))}");

                var firebaseCandidature = await firebase.Child("Candidatures").OnceAsync<Candidature>();

                System.Diagnostics.Debug.WriteLine($"firebaseCandidature: {string.Join(", ", firebaseCandidature.Select(item => item.Object))}");

                // Filter candidature list items based on the current user's id and status "acceptes"
                var userCandidature = firebaseCandidature
                    .Where(candidature => candidature.Object.PersonId == personId && candidature.Object.Statut == "acceptees")
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
        private async void Profil_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Profil(currentUser)); // Redirige vers la page du profil
        }
        //private async void Post_details_Clicked(object sender, EventArgs e)
        //{
        //    await Navigation.PushAsync(new Post_details()); // Redirige vers la page detaillée du post
        //}

        private async Task UpdateEmploiInDatabase(Emploi emploi)
        {
            // Ensure the ID is not empty or null
            if (!string.IsNullOrEmpty(emploi.Id_emploi))
            {
                //Console.WriteLine("Updating entry with Id_emploi: " + emploi.Id_emploi);

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
                    });
                    await Navigation.PushAsync(new Postes_en_attente(currentUser)); // Redirige vers la page du profil

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
    }

    public class Emploi_acceptesViewModel : BindableObject
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