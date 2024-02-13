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
    }

}
