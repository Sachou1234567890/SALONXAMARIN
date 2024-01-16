//using System;
//using System.IO;
//using System.Reflection;

//namespace SALONXAMARIN
//{
//    public class FirestoreManager
//    {
//        public static async void InitializeFirestore()
//        {
//            string jsonCredentials;
//            var assembly = typeof(FirestoreManager).GetTypeInfo().Assembly;
//            Stream resource = assembly.GetManifestResourceStream("SALONXAMARIN.salonxamarin_key.json");

//            if (resource != null)
//            {
//                using (var reader = new StreamReader(resource))
//                {
//                    jsonCredentials = reader.ReadToEnd();
//                }

//                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", jsonCredentials);
//                string project = "users"; // Remplacez-le par votre ID de projet Firestore
//                FirestoreDb db = FirestoreDb.Create(project);
//                Console.WriteLine("Created Cloud Firestore client with project ID: {0}", project);

//                // Utilisez "db" comme nécessaire pour interagir avec Firestore
//            }
//            else
//            {
//                Console.WriteLine("Le fichier JSON des identifiants n'a pas été trouvé.");
//                // Gérer l'erreur ou informer l'utilisateur que le fichier n'est pas disponible
//            }
//        }
//    }
//}
