﻿using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SALONXAMARIN
{
    public partial class App : Application
    {
        public App ()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new CONNEXION());
            //MainPage = new NavigationPage(new Postes_acceuil());
            //MainPage = new NavigationPage(new CONNEXION());
        }

        protected override void OnStart ()
        {
        }

        protected override void OnSleep ()
        {
        }

        protected override void OnResume ()
        {
        }
    }
}

