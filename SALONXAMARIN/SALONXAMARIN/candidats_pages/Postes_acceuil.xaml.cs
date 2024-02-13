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
	    public partial class Postes_acceuil : ContentPage
	    {
            FirebaseClient firebase;
            private Person currentUser;
            private Wishlist wishlist;             

            //Dictionary<string, List<string>> emploiParEvenement;
            public Postes_acceuil(Person currentUser)
            {
                InitializeComponent();
                NavigationPage.SetHasNavigationBar(this, false);
                NavigationPage.SetHasBackButton(this, false);                
                // Initialiser la connexion Firebase
                firebase = new FirebaseClient("https://projet-xamarin-default-rtdb.firebaseio.com/");
                this.currentUser = currentUser;               
                this.wishlist = new Wishlist();               

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

                    Console.WriteLine("this.currentUser.Sexe =" + this.currentUser.Sexe);
                    Console.WriteLine("this.currentUser.Nom =" + this.currentUser.Nom);
                    Console.WriteLine("this.currentUser.PersonId =" + this.currentUser.PersonId);
        }

        private async Task<List<Emploi>> GetAllEmplois()
            {
                var firebaseEmplois = await firebase.Child("Emplois").OnceAsync<Emploi>(TimeSpan.FromSeconds(10));                

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
                if (sender is Image tappedImage && tappedImage.BindingContext is Emploi clickedEmploi)
                {
                    this.wishlist.Id_wishList = Guid.NewGuid().ToString();
                    this.wishlist.Id_emploi = clickedEmploi.Id_emploi;
                    this.wishlist.PersonId = this.currentUser.PersonId;

                    // Check if a record with the same Id_emploi and PersonId already exists
                    if (await DoesWishListExist(this.wishlist.Id_emploi, this.wishlist.PersonId))
                    {
                    // Handle the case where the record already exists, e.g., show a message
                        Console.WriteLine("une Wishlist avec le meme couple Id_emploi PersonId existe déjà");
                        await DisplayAlert("Emploi déjà enregistré", "Emploi déjà enregistré", "OK");
                }
                else
                    {
                        // Update your Firebase database or perform any other actions as needed
                        await UpdateWishListInDatabase(this.wishlist);
                    }
                }
            }

            private async Task<bool> DoesWishListExist(string idEmploi, string personId)
            {
                try
                {
                    // Retrieve all wishlists from the database
                    var allWishLists = await firebase.Child("WishLists")
                                                     .OnceSingleAsync<Dictionary<string, Wishlist>>();

                    // Check if there is any existing record with the specified Id_emploi and PersonId
                    return allWishLists?.Values.Any(wishlist =>
                        wishlist.Id_emploi == idEmploi && wishlist.PersonId == personId) ?? false;
                }
                catch (Exception ex)
                {
                    // Log the exception details or handle it appropriately
                    Console.WriteLine("Exception in DoesWishListExist: " + ex.ToString());
                    // Optionally, you can rethrow the exception if you want it to be propagated
                    // throw;
                    return false; // Assuming no record exists in case of an exception
                }
            }

            private async Task UpdateWishListInDatabase(Wishlist wishlist)
            {
                try
                {
                    //var firebase = new FirebaseClient("your_firebase_url"); // Replace with your actual Firebase URL
                    var wishListRef = firebase.Child("WishLists").Child(wishlist.Id_wishList);

                    // Directly set the values under the specified key
                    await wishListRef.PutAsync(wishlist);

                    // Optionally, you can log success or return a success message
                    // Console.WriteLine("Wishlist updated or created successfully");
                    await DisplayAlert("emploi enregistré avec succes", "emploi enregistré avec succes", "OK");
                }
                catch (Exception ex)
                {
                    // Log the exception details or handle it appropriately
                    Console.WriteLine("Exception in UpdateWishListInDatabase: " + ex.ToString());
                    await DisplayAlert("message", "Exception in UpdateWishListInDatabase: " + ex.Message, "OK");
                    // Optionally, you can rethrow the exception if you want it to be propagated
                    // throw;
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
                await Navigation.PushAsync(new Post_details(selectedJob, currentUser));                        
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