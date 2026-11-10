using Coding4Coffee.Basics.Configuration;
using Coding4Coffee.Basics.Localization;
using SampleApplication.Settings;
using System.Globalization;

namespace SampleApplication
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ConfigFactory.Settings = SampleApplicationsSettings.Default;
            BasicsLocalizationExtensions.ResourceManager = Resources.ResourceManager;
            LocalizationManager.CultureConfigKey = nameof(SampleApplicationsSettings.Default.CultureName);

            if(!LocalizationManager.HasConfiguredCulture)
                LocalizationManager.CurrentCulture = CultureInfo.CurrentUICulture;

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
    }
}