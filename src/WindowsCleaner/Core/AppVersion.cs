using System;
using System.Reflection;

namespace WindowsCleaner
{
    /// <summary>
    /// Classe statique centralisée pour la gestion de la version de l'application
    /// La version est automatiquement lue depuis l'assembly (définie dans WindowsCleaner.csproj)
    /// </summary>
    public static class AppVersion
    {
        /// <summary>
        /// Version actuelle de l'application (ex: "2.0.1")
        /// Lue automatiquement depuis AssemblyInformationalVersion définie dans le .csproj
        /// </summary>
        public static string Current { get; }

        /// <summary>
        /// Version complète avec détails (ex: "2.0.1.0")
        /// </summary>
        public static string Full { get; }

        static AppVersion()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                
                // Récupérer InformationalVersion (correspond à <InformationalVersion> dans .csproj)
                var infoVersionAttr = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
                Current = infoVersionAttr?.InformationalVersion ?? "2.0.1";
                
                // Récupérer FileVersion complète
                var fileVersionAttr = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
                Full = fileVersionAttr?.Version ?? "2.0.1.0";
            }
            catch
            {
                // Fallback en cas d'erreur
                Current = "2.0.1";
                Full = "2.0.1.0";
            }
        }

        /// <summary>
        /// Retourne la version formatée pour affichage
        /// </summary>
        public static string GetDisplayVersion()
        {
            return $"Version: {Current}";
        }
    }
}
