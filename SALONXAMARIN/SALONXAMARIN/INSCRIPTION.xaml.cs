using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;
using Xamarin.Forms;

namespace SALONXAMARIN
{
    public partial class INSCRIPTION : ContentPage
    {
        FirebaseClient firebase;

        public INSCRIPTION()
        {
            InitializeComponent();

            // Initialiser la connexion Firebase
            firebase = new FirebaseClient("https://projet-xamarin-default-rtdb.firebaseio.com/");
        }

        private async void OnRegisterButtonClicked(object sender, EventArgs e)
        {
            // Vérifier si l'utilisateur existe déjà
            string username = UsernameEntry.Text;
            bool userExists = await CheckIfUserExists(username);

            if (userExists)
            {
                // Utilisateur déjà enregistré
                ErrorLabel.Text = "Cet utilisateur existe déjà.";
                ErrorLabel.IsVisible = true;
            }
            else
            {
                // Valider si l'adresse e-mail est dans un format valide
                string email = EmailEntry.Text;
                if (!IsValidEmail(email))
                {
                    ErrorLabel.Text = "Veuillez entrer une adresse e-mail valide.";
                    ErrorLabel.IsVisible = true;
                    return;
                }

                // Hacher le mot de passe avec un sel
                string hashedPassword = HashPassword(PasswordEntry.Text);

                // Enregistrer le nouvel utilisateur
                await RegisterNewUser(username, email, NomEntry.Text, PrenomEntry.Text, SocieteEntry.Text, BirthDatePicker.Date, hashedPassword);

                // Afficher un message de succès
                await DisplayAlert("Succès", "Inscription réussie", "OK");

                // Naviguer vers la page de connexion
                await Navigation.PushAsync(new CONNEXION());
            }
        }

        // Méthode pour valider une adresse e-mail avec une expression régulière
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        // Méthode pour hacher un mot de passe avec un sel
        private string HashPassword(string password)
        {
            // Générer un sel aléatoire
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

            // Utiliser PBKDF2 pour dériver une clé de hachage
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
            byte[] hash = pbkdf2.GetBytes(20);

            // Concaténer le sel avec la clé de hachage
            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);

            // Convertir en une représentation base64
            string hashedPassword = Convert.ToBase64String(hashBytes);

            return hashedPassword;
        }

        private async Task<bool> CheckIfUserExists(string username)
        {
            var firebasePersons = await firebase.Child("Persons").OnceAsync<Person>();
            return firebasePersons.Any(item => item.Object.Name == username);
        }

        private async Task RegisterNewUser(string username, string email, string nom, string prenom, string societe, DateTime birthDate, string password)
        {
            // Générer un nouvel identifiant pour l'utilisateur
            string personId = Guid.NewGuid().ToString();

            // Créer un nouvel objet Person
            Person newPerson = new Person
            {
                PersonId = personId,
                Name = username,
                Email = email,
                Nom = nom,
                Prenom = prenom,
                Societe = societe,
                DateNaissance = birthDate,
                Password = password
                // Ajoutez d'autres propriétés au besoin
            };

            // Enregistrer l'utilisateur dans la base de données Firebase
            await firebase.Child("Persons").Child(personId).PutAsync(newPerson);
        }

        private void OnAlreadyHaveAccountButtonClicked(object sender, EventArgs e)
        {
            // Naviguer vers la page de connexion
            // Example: await Navigation.PushAsync(new LoginPage());
        }
    }
}
