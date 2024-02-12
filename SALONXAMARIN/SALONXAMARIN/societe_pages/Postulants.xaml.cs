using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SALONXAMARIN.societe_pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Postulants : ContentPage
    {
        FirebaseClient firebase;        
        public Postulants()
        {
            {
                InitializeComponent();
                NavigationPage.SetHasNavigationBar(this, false);
                NavigationPage.SetHasBackButton(this, false);

                firebase = new FirebaseClient("https://projet-xamarin-default-rtdb.firebaseio.com/");

                var viewModel = new PostulantsViewModel();

                // Récupération des Postulants et assignation à la propriété Postulants du ViewModel
                Task.Run(async () =>
                {
                    var postulants = await GetPostulants();
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        viewModel.Postulants = new ObservableCollection<Person>(postulants);
                    });
                });
                BindingContext = viewModel;                
            }
        }
        private async void OnPostulantSelected(object sender, EventArgs e)
        {
            if (sender is Frame selectedFrame && selectedFrame.BindingContext is Person selectedPostulant)
            {
                await Navigation.PushAsync(new Candidatures(selectedPostulant));
            }
        }
        private async Task<List<Person>> GetPostulants()
        {
            try
            {                
                var firebasePersons = await firebase.Child("Persons").OnceAsync<Person>();

                System.Diagnostics.Debug.WriteLine($"firebasePersons: {string.Join(", ", firebasePersons.Select(item => item.Object))}");

                var firebaseCandidature = await firebase.Child("Candidatures").OnceAsync<Candidature>();

                System.Diagnostics.Debug.WriteLine($"firebaseCandidature: {string.Join(", ", firebaseCandidature.Select(item => item.Object))}");

                // Filter candidature items based on ...
                var candidatureslist = firebaseCandidature
                    //.Where(candidature => candidature.Object.PersonId == personId)
                    .ToList();

                // Get emploi ids from the user's wish list
                var personIdsInCandidatureslist = candidatureslist.Select(candidature => candidature.Object.PersonId).ToList();

                System.Diagnostics.Debug.WriteLine($"personIdsInCandidatureslist: {string.Join(", ", personIdsInCandidatureslist)}");

                // Filter persons based on personIdsInCandidatureslist
                List<Person> postulants = firebasePersons
                    .Where(item => personIdsInCandidatureslist.Contains(item.Object.PersonId))
                    .Select(item => item.Object)
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"postulants: {postulants}");

                System.Diagnostics.Debug.WriteLine($"Postulants count: {postulants.Count}");

                return postulants;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"An error occurred: {ex.Message}");
                return new List<Person>();
            }
        }

        private async void Societe_Home_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MainPage_societe()); // Redirige vers la page du profil
        }
        
        


    }

    public class PostulantsViewModel : BindableObject
    {
        private ObservableCollection<Person> _postulants;
        public ObservableCollection<Person> Postulants 
        {
            get => _postulants;
            set
            {
                _postulants = value;
                OnPropertyChanged(nameof(Postulants));
            }
        }
    }

}