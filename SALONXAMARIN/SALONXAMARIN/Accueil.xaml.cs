using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace SALONXAMARIN
{	
	public partial class Accueil : ContentPage
	{	
		public Accueil ()
		{
			InitializeComponent ();
		}
        private async void EnSavoirPlus_Clicked(object sender, EventArgs e)
        {
            // Redirige vers la page de connexion
            await Navigation.PushAsync(new CONNEXION());
        }
    }
}

