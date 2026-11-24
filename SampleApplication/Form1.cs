using Coding4Coffee.Basics.Configuration;
using Coding4Coffee.WinForms.Localization.Controls;

namespace SampleApplication
{
    public partial class Form1 : LocalizableForm
    {
        public List<LanguageSelectionItem> LanguageSelectionItems { get; } =
        [
            new() { Key = "en-US", DisplayName = "English" },
            new() { Key = "de-DE", DisplayName = "German" }
        ];

        public Form1()
        {
            InitializeComponent();

            configurationComboBox1.DataSource = LanguageSelectionItems;
        }
    }
}
