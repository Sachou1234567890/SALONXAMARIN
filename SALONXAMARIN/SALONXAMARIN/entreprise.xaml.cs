using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;


namespace SALONXAMARIN
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class entreprise : ContentPage
    {

        private FirebaseClient firebase;

        public entreprise()
        {
            InitializeComponent();

            firebase = new FirebaseClient("https://projet-xamarin-default-rtdb.firebaseio.com/");
        }

        // Gestionnaire d'événements pour le clic sur le point 1
        private void OnPointClicked(object sender, EventArgs e)
        {

        }


        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Récupérer les données d'entreprise depuis Firebase Realtime Database
            var entreprises = await GetEntreprises();

            Random random = new Random();

            // Créer des boutons pour chaque entreprise avec des positions aléatoires
            foreach (var entreprise in entreprises)
            {
                var button = new Button
                {
                    Text = entreprise.stand,
                    CommandParameter = entreprise, // Utilisé pour transmettre l'entreprise au gestionnaire d'événements
                    Margin = new Thickness(10),
                    WidthRequest = 100, // Définir la largeur du bouton
                    HeightRequest = 50 // Définir la hauteur du bouton
                };

                // Générer des positions aléatoires dans une plage plus restreinte
                double x = random.NextDouble() * 0.6 + 0.2; // Plage de 0.2 à 0.8
                double y = random.NextDouble() * 0.4 + 0.3; // Plage de 0.3 à 0.7


                // Positionner le bouton de manière aléatoire
                AbsoluteLayout.SetLayoutFlags(button, AbsoluteLayoutFlags.PositionProportional);
                AbsoluteLayout.SetLayoutBounds(button, new Rectangle(x, y, -1, -1));

                button.Clicked += OnEntrepriseButtonClickedAsync;
                absoluteLayout.Children.Add(button);
            }
        }

        private async Task<List<Entreprise>> GetEntreprises()
        {
            // Récupérer les données depuis Firebase Realtime Database
            var entreprises = await firebase
                .Child("entreprises")
                .OnceAsync<Entreprise>();

            var entreprisesList = new List<Entreprise>();

            int count = 0; // Compteur pour limiter le nombre d'entreprises à 5

            // Convertir les données Firebase en liste d'objets Entreprise
            foreach (var entreprise in entreprises)
            {
                if (count < 5)
                {
                    entreprisesList.Add(entreprise.Object);
                    count++;
                }
                else
                {
                    break; // Sortir de la boucle si 5 entreprises ont été ajoutées
                }
            }

            return entreprisesList;
        }


        private async void OnEntrepriseButtonClickedAsync(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var entreprise = (Entreprise)button.CommandParameter;

            await HandleEntrepriseButtonClickedAsync(entreprise);
        }

        private async Task HandleEntrepriseButtonClickedAsync(Entreprise entreprise)
        {
            // Afficher les informations de l'entreprise de manière asynchrone
            var action = await DisplayAlert("Information du stand", $"Nom: {entreprise.name}\nStand: {entreprise.stand}\nEmail: {entreprise.email}", "Prendre RDV", "Quitter");

            // Si l'utilisateur appuie sur "Prendre RDV", naviguer vers la page de détails
            if (action)
            {
               Navigation.PushAsync(new rdv(entreprise));
            }
        }

    }
}