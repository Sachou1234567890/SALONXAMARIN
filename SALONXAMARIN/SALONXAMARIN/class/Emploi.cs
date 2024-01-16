using System;

namespace SALONXAMARIN
{
    public class Emploi
    {
        public string Id_emploi { get; set; }
        public string Titre { get; set; }
        public string Description { get; set; }
        public string[] Responsabilites { get; set; }        
        public float Salaire_horaire { get; set; }
        public bool Wishlist { get; set; } 
    }
}
