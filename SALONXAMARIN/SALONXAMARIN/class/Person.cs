﻿// Person.cs
using System;

namespace SALONXAMARIN
{
    public class Person
    {
        public string PersonId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string Sexe { get; set; }
        public string Societe { get; set; }
        public DateTime DateNaissance { get; set; }
        public string Password { get; set; }
        public string CV_name { get; set; }        
        public bool admin { get; set; }        
        // Ajoutez d'autres propriétés au besoin
    }
}
