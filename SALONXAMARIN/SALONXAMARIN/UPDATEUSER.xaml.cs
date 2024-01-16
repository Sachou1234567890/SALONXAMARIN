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
        private FirebaseClient firebase;

        public UPDATEUSER(Person utilisateur)
        {
            InitializeComponent();
            this.utilisateur = utilisateur;
            NavigationPage.SetHasNavigationBar(this, false);
            NavigationPage.SetHasBackButton(this, false);

            // Initialisez la connexion Firebase
            firebase = new FirebaseClient("https://projet-xamarin-default-rtdb.firebaseio.com/");

            // Initialisez vos champs d'interface utilisateur avec les informations de l'utilisateur
            nomEntry1.Text = utilisateur.Nom;
            prenomEntry1.Text = utilisateur.Prenom;
            emailEntry1.Text = utilisateur.Email;
        }

        private async void OnUpdateButtonClicked(object sender, EventArgs e)
        {
            // Récupérez les nouvelles valeurs saisies par l'utilisateur
            string updatedNom = nomEntry1.Text;
            string updatedPrenom = prenomEntry1.Text;
            string updatedEmail = emailEntry1.Text;

            // Mettez à jour les propriétés de l'objet utilisateur avec les nouvelles valeurs
            utilisateur.Nom = updatedNom;
            utilisateur.Prenom = updatedPrenom;
            utilisateur.Email = updatedEmail;

            // Mettez à jour les informations de l'utilisateur dans la base de données
            await UpdateUserInfo(utilisateur);

            // Affichez une alerte indiquant que les informations ont été mises à jour
            await DisplayAlert("Succès", "Informations utilisateur mises à jour", "OK");
        }

        // Ajoutez cette méthode pour mettre à jour les informations de l'utilisateur dans la base de données
        private async Task UpdateUserInfo(Person utilisateur)
        {
            // Mettez à jour les informations de l'utilisateur dans la base de données Firebase
            await firebase.Child("Persons").Child(utilisateur.PersonId).PutAsync(utilisateur);
        }
    }
}
