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
    public partial class Postes_enregistres: ContentPage
    {
        FirebaseClient firebase;
        private Person currentUser;

        Dictionary<string, List<string>> emploiParEvenement;
        public Postes_enregistres(Person currentUser)
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            NavigationPage.SetHasBackButton(this, false);
     
            firebase = new FirebaseClient("https://projet-xamarin-default-rtdb.firebaseio.com/");

            var viewModel = new Emploi_enregistresViewModel();            

            // Récupération des emplois enregistrés et assignation à la propriété Emplois du ViewModel
            Task.Run(async () =>
            {
                var emplois = await GetEmploisEnregistres(currentUser.PersonId);
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
        private async void removeFromWishList(object sender, EventArgs e)
        {
            if (sender is Image tappedImage && tappedImage.BindingContext is Emploi selectedEmploi)
            {
                try
                {
                    // Find the corresponding entry in the WishLists collection
                    var wishListToRemove = (await firebase.Child("WishLists")
                        .OrderBy("Id_emploi")
                        .EqualTo(selectedEmploi.Id_emploi)
                        .OnceSingleAsync<Dictionary<string, Wishlist>>())
                        ?.Values.FirstOrDefault(wishlist => wishlist.PersonId == currentUser.PersonId);

                    if (wishListToRemove != null)
                    {
                        // Remove the entry from the WishLists collection
                        await firebase.Child("WishLists")
                            .Child(wishListToRemove.Id_wishList)
                            .DeleteAsync();

                        // Update the UI or perform any other actions as needed
                        // For example, you may want to refresh the displayed list of emplois
                        var viewModel = (Emploi_enregistresViewModel)BindingContext;
                        viewModel.Emplois.Remove(selectedEmploi);
                    }
                    else
                    {
                        await DisplayAlert("Error", "Wishlist entry not found.", "OK");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occurred while removing from Wishlist: " + ex.Message);
                    await DisplayAlert("Error", "An error occurred while removing from Wishlist.", "OK");
                }
            }
        }


        private async Task<List<Emploi>> GetEmploisEnregistres(string personId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"personId: {personId}"); // ok

                var firebaseEmplois = await firebase.Child("Emplois").OnceAsync<Emploi>();

                System.Diagnostics.Debug.WriteLine($"firebaseEmplois: {string.Join(", ", firebaseEmplois.Select(item => item.Object))}");

                var firebaseWishList = await firebase.Child("WishLists").OnceAsync<Wishlist>();

                System.Diagnostics.Debug.WriteLine($"firebaseWishList: {string.Join(", ", firebaseWishList.Select(item => item.Object))}");

                // Filter wish list items based on the current user's id
                var userWishlist = firebaseWishList
                    .Where(wishlist => wishlist.Object.PersonId == personId)
                    .ToList();

                // Get emploi ids from the user's wish list
                var emploiIdsInUserWishlist = userWishlist.Select(wishlist => wishlist.Object.Id_emploi).ToList();

                System.Diagnostics.Debug.WriteLine($"emploiIdsInUserWishlist: {string.Join(", ", emploiIdsInUserWishlist)}");

                // Filter emplois based on emploiIdsInUserWishlist
                List<Emploi> emplois = firebaseEmplois
                    .Where(item => emploiIdsInUserWishlist.Contains(item.Object.Id_emploi))
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
                    await Navigation.PushAsync(new Postes_enregistres(currentUser)); // Redirige vers la page du profil

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


        //private async Task<List<Emploi>> GetEmploisEnregistresOld()
        //{
        //    var firebaseEmplois = await firebase.Child("Emplois").OnceAsync<Emploi>();

        //    // Convert Firebase objects to Emploi objects and assign unique IDs if not present
        //    List<Emploi> emplois = firebaseEmplois.Select(item =>
        //    {
        //        Emploi emploi = item.Object;

        //        // Check if the ID is not set and assign a new Guid
        //        if (string.IsNullOrEmpty(emploi.Id_emploi))
        //        {
        //            emploi.Id_emploi = Guid.NewGuid().ToString();
        //            // You may want to update the entry in the database with the new ID
        //            firebase.Child("Emplois").Child(emploi.Id_emploi).PutAsync(emploi);
        //        }

        //        // Check if the "Wishlist" property exists and is true
        //        if (item.Object.Wishlist)
        //        {
        //            return emploi;
        //        }

        //        return null; // If "Wishlist" is not true, return null
        //    })
        //    .Where(emploi => emploi != null) // Filter out entries where "Wishlist" is not true
        //    .ToList();

        //    return emplois;
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