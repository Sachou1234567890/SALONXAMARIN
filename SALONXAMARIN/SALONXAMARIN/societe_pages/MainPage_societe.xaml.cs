using SALONXAMARIN.societe_pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SALONXAMARIN
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage_societe : ContentPage
    {
        public MainPage_societe()
        {
            InitializeComponent();
        }

        private async void PostulantsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Postulants()); // Redirige vers la page du profil
        }
        private async void EmployesClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Employes()); // Redirige vers la page du profil
        }
        //private async void CandidaturesClicked(object sender, EventArgs e)
        //{
        //    await Navigation.PushAsync(new Candidatures()); // Redirige vers la page du profil
        //}


    }
}