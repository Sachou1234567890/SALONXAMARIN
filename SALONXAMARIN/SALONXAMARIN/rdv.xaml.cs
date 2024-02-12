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
    public partial class rdv : ContentPage
    {
        private Entreprise entreprise;

        static FirebaseClient firebase = new FirebaseClient("https://projet-xamarin-default-rtdb.firebaseio.com/");

        public rdv(Entreprise entreprise)
        {

            InitializeComponent();
            this.entreprise = entreprise;

            BindingContext = entreprise;
        }

        public static async Task<bool> SaveRdvAsync(string entrepriseName, DateTime rendezVous)
        {
            try
            {

                var rdv = new Rdv { Name = entrepriseName, RdvDate = rendezVous };
                // Créer un nouvel enregistrement dans la base de données Firebase avec le rendez-vous
                await firebase
                    .Child("rdvs")
                    .PostAsync(rdv);

                return true;
            }
            catch (Exception ex)
            {
                // Gérer les erreurs
                Console.WriteLine($"Erreur : {ex.Message}");
                return false;
            }
        }


        private async void OnConfirmButtonClicked(object sender, EventArgs e)
        {
            TimeSpan selectedTime = timePicker.Time;

            // Créer la date complète en utilisant la date actuelle et l'heure sélectionnée
            DateTime selectedDateTime = DateTime.Today.Add(selectedTime);

            // Enregistrer le rendez-vous dans Firebase
            bool isSuccess = await SaveRdvAsync(entreprise.name, selectedDateTime);

            if (isSuccess)
            {
                await DisplayAlert("Succès", "Rendez-vous enregistré avec succès", "OK");
            }
            else
            {
                await DisplayAlert("Erreur", "Erreur lors de l'enregistrement du rendez-vous", "OK");
            }
        }

        public class Rdv
        {
            public string Name { get; set; }
            public DateTime RdvDate { get; set; }
        }
    }
}