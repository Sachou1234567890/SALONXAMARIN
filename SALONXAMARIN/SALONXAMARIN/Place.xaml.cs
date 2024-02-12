using System;
using Xamarin.Forms;
using Firebase.Database;
using Firebase.Database.Query;
using System.Threading.Tasks;
using System.Linq;
using SALONXAMARIN.candidats_pages;

namespace SALONXAMARIN
{
    public partial class Place : ContentPage
    {
        private FirebaseClient firebase;
        public string UserId { get; set; }

        public Place(string userId)
        {
            InitializeComponent();
          
            BindingContext = this;
            UserId = userId;

            // Initialisez FirebaseClient avec votre URL de base de données Firebase
            firebase = new FirebaseClient("https://projet-xamarin-default-rtdb.firebaseio.com/");
        }

        private async Task<bool> CheckIfUserReservationExists(string userId)
        {
            // Effectuez une requête pour vérifier si l'ID de l'utilisateur existe déjà dans la collection reservations
            var reservations = await firebase
                .Child("reservations")
                .OrderBy("UserId")
                .EqualTo(userId)
                .OnceAsync<Reservation>();

            return reservations.Any(); // Renvoie vrai si des réservations existent pour cet utilisateur, sinon faux
        }

        private async void OnReservationCat1Clicked(object sender, EventArgs e)
        {
            // Vérifiez d'abord si une réservation existe déjà pour cet utilisateur
            bool userReservationExists = await CheckIfUserReservationExists(UserId);

            if (userReservationExists)
            {
                await DisplayAlert("Erreur", $"Vous avez déjà une réservation enregistrée.", "OK");
                return; // Sortie de la méthode sans effectuer de réservation supplémentaire
            }

            // Enregistrez la réservation dans Firebase
            await SaveReservation("ADULTE");
            // Afficher un message de confirmation
            await DisplayAlert("Réservation confirmée", $"Merci {UserId}, Votre billet ADULTE est bien réservé", "OK");
        }

        // Les autres méthodes OnReservationCat2Clicked et OnReservationCat3Clicked doivent être modifiées de manière similaire.

        private async void OnReservationCat2Clicked(object sender, EventArgs e)
        {
            // Vérifiez d'abord si une réservation existe déjà pour cet utilisateur
            bool userReservationExists = await CheckIfUserReservationExists(UserId);

            if (userReservationExists)
            {
                await DisplayAlert("Erreur", $"Vous avez déjà une réservation enregistrée.", "OK");
                return; // Sortie de la méthode sans effectuer de réservation supplémentaire
            }

            // Enregistrez la réservation dans Firebase
            await SaveReservation("Senior");
            // Afficher un message de confirmation
            await DisplayAlert("Réservation confirmée", $"Merci {UserId}, Votre billet Senior est bien réservé", "OK");
        }

        private async void OnReservationCat3Clicked(object sender, EventArgs e)
        {
            // Vérifiez d'abord si une réservation existe déjà pour cet utilisateur
            bool userReservationExists = await CheckIfUserReservationExists(UserId);

            if (userReservationExists)
            {
                await DisplayAlert("Erreur", $"Vous avez déjà une réservation enregistrée.", "OK");
                return; // Sortie de la méthode sans effectuer de réservation supplémentaire
            }

            // Enregistrez la réservation dans Firebase
            await SaveReservation("Etudiant");
            // Afficher un message de confirmation
            await DisplayAlert("Réservation confirmée", $"Merci {UserId}, Votre billet Etudiant est bien réservé", "OK");
        }

        private async Task SaveReservation(string ticketType)
        {
            // Créez un nouvel objet réservation à enregistrer dans Firebase
            var reservation = new Reservation
            {
                UserId = UserId,
                TicketType = ticketType,
                DateTime = DateTime.UtcNow // Vous pouvez ajuster le fuseau horaire si nécessaire
            };

            // Enregistrez la réservation dans Firebase
            await firebase.Child("reservations").PostAsync(reservation);
        }
    }

    // Classe pour représenter une réservation
    public class Reservation
    {
        public string UserId { get; set; }
        public string TicketType { get; set; }
        public DateTime DateTime { get; set; }
    }

}
