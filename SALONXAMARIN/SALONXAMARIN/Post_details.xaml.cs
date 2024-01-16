using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SALONXAMARIN
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Post_details : ContentPage
    {
        private Emploi selectedJob;
        public Post_details(Emploi selectedJob)
        {
            InitializeComponent();
            BindingContext = new PostDetailsViewModel(selectedJob);
            this.selectedJob = selectedJob;


            // You can now use the 'selectedJob' variable to populate the details on this page
            // For example, update labels, images, etc. based on the selected job details
        }
        private async void Profil_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Profil()); // Redirige vers la page du profil
        }

    }



    public class PostDetailsViewModel : BindableObject
    {
        private Emploi _selectedJob;

        public Emploi SelectedJob
        {
            get => _selectedJob;
            set
            {
                _selectedJob = value;
                OnPropertyChanged(nameof(SelectedJob));
            }
        }

        public PostDetailsViewModel(Emploi selectedJob)
        {
            SelectedJob = selectedJob;
        }
    }
}