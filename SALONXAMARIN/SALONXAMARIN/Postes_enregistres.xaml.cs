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
    public partial class Postes_enregistres: ContentPage
    {
        FirebaseClient firebase;

        Dictionary<string, List<string>> emploiParEvenement;
        public Postes_enregistres()
        {
            InitializeComponent();
            //collection_jobs.SelectionChanged += OnJobSelected;
            // Initialiser la connexion Firebase
            firebase = new FirebaseClient("https://projet-xamarin-default-rtdb.firebaseio.com/");

            var viewModel = new EmploiViewModel();

            // Récupération des emplois enregistrés et assignation à la propriété Emplois du ViewModel
            Task.Run(async () =>
            {
                var emplois = await GetEmploisEnregistres();
                Device.BeginInvokeOnMainThread(() =>
                {
                    viewModel.Emplois = new ObservableCollection<Emploi>(emplois);
                });
            });
            BindingContext = viewModel;
        }
        private async void OnJobSelected(object sender, EventArgs e)
        {
            if (sender is Frame selectedFrame && selectedFrame.BindingContext is Emploi selectedJob)
            {                
                await Navigation.PushAsync(new Post_details(selectedJob));               

                // Manually deselect the item to remove the highlight
                collection_jobs.SelectedItem = null;
            }
        }
        private async Task<List<Emploi>> GetEmploisEnregistres()
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

                // Check if the "Wishlist" property exists and is true
                if (item.Object.Wishlist)
                {
                    return emploi;
                }

                return null; // If "Wishlist" is not true, return null
            })
            .Where(emploi => emploi != null) // Filter out entries where "Wishlist" is not true
            .ToList();

            return emplois;
        }









        private async void Profil_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Profil()); // Redirige vers la page du profil
        }
        //private async void Post_details_Clicked(object sender, EventArgs e)
        //{
        //    await Navigation.PushAsync(new Post_details()); // Redirige vers la page detaillée du post
        //}
    }

    public class Emploi_enregistresViewModel : BindableObject
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