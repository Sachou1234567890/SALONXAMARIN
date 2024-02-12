using System;
using System.Collections.Generic;
using System.Text;

namespace SALONXAMARIN
{
    public class Candidature
    {
        public string CandidatureId { get; set; }
        public string EmploiId { get; set; }
        public string PersonId { get; set; }
        public string Statut { get; set; }
        public string CV { get; set; }
        public string Lettre { get; set; }
        public DateTime DatePostulation { get; set; }

        //Constructeur par défaut
        //public Candidature()
        //{
        //    // Initialisez les propriétés par défaut si nécessaire
        //    CandidatureId = Guid.NewGuid().ToString();
        //    DatePostulation = DateTime.UtcNow;
        //    Statut = "En attente";
        //    CV = string.Empty;
        //    Lettre = string.Empty;
        //}

        //Constructeur surchargé pour faciliter la création d'une candidature
        //public Candidature(string emploiId, string personId)
        //    : this()
        //{
        //    EmploiId = emploiId;
        //    PersonId = personId;
        //}
    }

}
