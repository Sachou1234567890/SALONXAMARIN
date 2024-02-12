using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Firebase.Database;
using Firebase.Database.Query;

namespace SALONXAMARIN
{
    public partial class UPDATEUSER : ContentPage
    {
        private Person utilisateur;
        private string userId; 
        private FirebaseClient firebase;

        public UPDATEUSER(string userId)
        {
            InitializeComponent();
            this.userId = userId;

            // Initialisez la connexion Firebase
            firebase = new FirebaseClient("https://projet-xamarin-default-rtdb.firebaseio.com/");

            // Chargez les informations de l'utilisateur à partir de la base de données
            LoadUserData(userId);
        }

        private async void LoadUserData(string userId)
        {
            // Récupérer les informations de l'utilisateur depuis la base de données
            Person currentUser = await GetUserFromDatabase(userId);

            // Initialisez vos champs d'interface utilisateur avec les informations de l'utilisateur
            nomEntry1.Text = currentUser.Nom;
            prenomEntry1.Text = currentUser.Prenom;
            emailEntry1.Text = currentUser.Email;
        }

        private async void OnUpdateButtonClicked(object sender, EventArgs e)
        {
            // Récupérez les nouvelles valeurs saisies par l'utilisateur
            string updatedNom = nomEntry1.Text;
            string updatedPrenom = prenomEntry1.Text;
            string updatedEmail = emailEntry1.Text;

            // Créez un objet temporaire pour stocker les nouvelles informations de l'utilisateur
            Person updatedUser = new Person
            {
                Nom = updatedNom,
                Prenom = updatedPrenom,
                Email = updatedEmail
            };

            // Récupérez les informations actuelles de l'utilisateur depuis la base de données en utilisant son ID
            Person currentUser = await GetUserFromDatabase(userId); // Utilisation de userId au lieu de utilisateur.PersonId

            // Vérifiez quelles informations ont été modifiées
            if (currentUser.Nom != updatedNom)
                currentUser.Nom = updatedNom;
            if (currentUser.Prenom != updatedPrenom)
                currentUser.Prenom = updatedPrenom;
            if (currentUser.Email != updatedEmail)
                currentUser.Email = updatedEmail;

            // Mettez à jour les informations de l'utilisateur dans la base de données
            await UpdateUserInfo(currentUser);

            // Affichez une alerte indiquant que les informations ont été mises à jour
            await DisplayAlert("Succès", "Informations utilisateur mises à jour", "OK");
        }


        private async Task<Person> GetUserFromDatabase(string userId)
        {
            // Utilisez Firebase pour récupérer les informations de l'utilisateur à partir de son ID
            var userSnapshot = await firebase.Child("Persons").Child(userId).OnceSingleAsync<Person>();

            return userSnapshot;
        }

        private async Task UpdateUserInfo(Person utilisateur)
        {
            // Mettez à jour les informations de l'utilisateur dans la base de données Firebase
            await firebase.Child("Persons").Child(utilisateur.PersonId).PutAsync(utilisateur);
        }

        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync(); // Par exemple, pour revenir à la page précédente
        }
    }
}
