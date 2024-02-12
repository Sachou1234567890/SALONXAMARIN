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
        private string selectedActualValue;
        public INSCRIPTION()
        {
            InitializeComponent();

            // Initialiser la connexion Firebase
            firebase = new FirebaseClient("https://projet-xamarin-default-rtdb.firebaseio.com/");
            NavigationPage.SetHasNavigationBar(this, false);
            NavigationPage.SetHasBackButton(this, false);

            // Subscribe to the event handler for selection change
            SexePicker.SelectedIndexChanged += SexePicker_SelectedIndexChanged;

            // Set the default selected item
            SexePicker.SelectedIndex = 0; // Select "Sélectionnez votre sexe"
        }
        private void SexePicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Get the selected display value
            string selectedDisplayValue = (string)SexePicker.SelectedItem;

            // Map the selected display value to the corresponding actual value
            this.selectedActualValue = GetActualValue(selectedDisplayValue);
            //Console.WriteLine("this.selectedActualValue : " + this.selectedActualValue); // OK


            // Use the selected actual value as needed
            // For example, you can store it in a variable or pass it to a method
            //string actualValue = selectedActualValue;
        }

        // Method to map display values to actual values
        private string GetActualValue(string displayValue)
        {
            switch (displayValue)
            {
                case "masculin":
                    return "masculin";
                case "féminin":
                    return "feminin";
                default:
                    return string.Empty;
            }
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

                // Enregistrer un admin
                if (username.Equals("aa") && email.Equals("aa@gmail.com") )
                {
                    await RegisterAdmin(username, email, NomEntry.Text, PrenomEntry.Text, SocieteEntry.Text, BirthDatePicker.Date, hashedPassword);
                }
                // Enregistrer le nouvel utilisateur
                else
                {
                    await RegisterNewUser(username, email, NomEntry.Text, PrenomEntry.Text, this.selectedActualValue, SocieteEntry.Text, BirthDatePicker.Date, hashedPassword);
                }

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

        // inscription d'un admin (usage temporaire, juste pour créér des comptes admin)
        private async Task RegisterAdmin(string username, string email, string nom, string prenom, string societe, DateTime birthDate, string password)
        {
            // Générer un nouvel identifiant pour l'utilisateur
            string personId = Guid.NewGuid().ToString();

            // Créer un nouvel objet Person avec le champ 'admin' mis à true
            Person newAdmin = new Person
            {
                PersonId = personId,
                Name = username,
                Email = email,
                Nom = nom,
                Prenom = prenom,
                Societe = societe,
                DateNaissance = birthDate,
                Password = password,
                CV_name = "",
                admin = true
            };

            // Enregistrer le nouvel administrateur dans la base de données Firebase
            await firebase.Child("Persons").Child(personId).PutAsync(newAdmin);
        }

        private async Task RegisterNewUser(string username, string email, string nom, string prenom, string sexe,
            string societe, DateTime birthDate, string password)
        {
            // Générer un nouvel identifiant pour l'utilisateur
            string personId = Guid.NewGuid().ToString();

            Console.WriteLine("selectedActualValue = " + this.selectedActualValue);

            // Créer un nouvel objet Person
            Person newPerson = new Person
            {                
                PersonId = personId,
                Name = username,
                Email = email,
                Nom = nom,
                Prenom = prenom,
                Sexe = this.selectedActualValue,
                Societe = societe,
                DateNaissance = birthDate,
                Password = password,
                CV_name = "",
                admin = false,                                              
                // Ajoutez d'autres propriétés au besoin
        };

            // Enregistrer l'utilisateur dans la base de données Firebase
            await firebase.Child("Persons").Child(personId).PutAsync(newPerson);
        }

        private async void OnAlreadyHaveAccountButtonClicked(object sender, EventArgs e)
        {

            await Navigation.PushAsync(new CONNEXION());
        }
    }
}
