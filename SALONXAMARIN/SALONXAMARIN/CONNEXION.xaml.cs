using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Firebase.Database;
using Xamarin.Forms;

namespace SALONXAMARIN
{
    public partial class CONNEXION : ContentPage
    {
        FirebaseClient firebase;        
        public CONNEXION()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            NavigationPage.SetHasBackButton(this, false);

            // Initialiser la connexion Firebase
            firebase = new FirebaseClient("https://projet-xamarin-default-rtdb.firebaseio.com/");
        }

        private async void OnLoginButtonClicked(object sender, EventArgs e)
        {
            try
            {
                // Vérifier si les champs sont vides
                if (string.IsNullOrEmpty(UsernameEntry.Text) || string.IsNullOrEmpty(PasswordEntry.Text))
                {
                    await DisplayAlert("Erreur", "Veuillez saisir un nom d'utilisateur et un mot de passe", "OK");
                    return;
                }

                // Récupérer toutes les personnes depuis Firebase
                List<Person> allPersons = await GetAllPersons();

                // Vérifier les informations de connexion
                string username = UsernameEntry.Text;
                string password = PasswordEntry.Text;

                Person user = allPersons.FirstOrDefault(p => p.Name == username);

                if (user != null)
                {
                    // Comparer le mot de passe haché
                    if (VerifyPassword(password, user.Password))
                    {
                        if (user.Name.Equals("aa"))
                        {
                            await Navigation.PushAsync(new MainPage_societe());
                        }
                        else
                        {
                            // Connexion réussie
                            await Navigation.PushAsync(new candidats_pages.Postes_acceuil(user));
                            //await Navigation.PushAsync(new UPDATEUSER(user));
                        }
                    }
                    else
                    {
                        // Mot de passe incorrect
                        await DisplayAlert("Erreur", "Mot de passe incorrect", "OK");
                    }
                }
                else
                {
                    // Utilisateur non trouvé
                    await DisplayAlert("Erreur", "Utilisateur non trouvé", "OK");
                }
            }
            catch (Exception ex)
            {
                // Gérer les exceptions (afficher ou journaliser)
                System.Diagnostics.Debug.WriteLine($"An error occurred: {ex.Message}");
                await DisplayAlert("Erreur", $"Une erreur s'est produite : {ex.Message}", "OK");
            }
        }
        private bool VerifyPassword(string enteredPassword, string storedHashedPassword)
        {
            // Extraire le sel et la clé de hachage de la valeur stockée
            byte[] hashBytes = Convert.FromBase64String(storedHashedPassword);
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            // Utiliser PBKDF2 pour dériver une nouvelle clé de hachage avec le même sel
            var pbkdf2 = new Rfc2898DeriveBytes(enteredPassword, salt, 10000);
            byte[] newHash = pbkdf2.GetBytes(20);

            // Comparer la nouvelle clé de hachage avec celle stockée
            for (int i = 0; i < 20; i++)
            {
                if (hashBytes[i + 16] != newHash[i])
                {
                    return false;
                }
            }
            return true;
        }

        public async Task<List<Person>> GetAllPersons()
        {
            var firebasePersons = await firebase.Child("Persons").OnceAsync<Person>();
            return firebasePersons.Select(item => new Person
            {
                PersonId = item.Object.PersonId,
                Name = item.Object.Name,
                Email = item.Object.Email,
                Nom = item.Object.Nom,
                Prenom = item.Object.Prenom,
                Sexe = item.Object.Sexe,
                Societe = item.Object.Societe,
                DateNaissance = item.Object.DateNaissance,
                Password = item.Object.Password,
                CV_name = item.Object.CV_name,                
                admin = item.Object.admin,
                //lettre_name  = item.Object.lettre_name 
                // Ajoutez d'autres propriétés au besoin
            }).ToList();
        }
        private async void OnCreateAccountButtonClicked(object sender, EventArgs e)
        {

          await Navigation.PushAsync(new INSCRIPTION());
        }
    }
}
