using System;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;
using System.Globalization;
using System.Data.SqlServerCe;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Media.Imaging;


namespace puzzle.Providers
{
    public class DisplayLanguageHandler : IDisposable
    {
        public int LanguageIndex = -1;
        public DataSet LanguageDataSet = new DataSet();
        public List<StackPanel> LanguageComboBoxItems = new List<StackPanel>();

        public DisplayLanguageHandler()
        {
            IEnumerable<string> availableLanguages_names = from string language in Assembly.GetExecutingAssembly().GetManifestResourceNames()
                                                            where language.Contains(".GIF")
                                                            select language;
            int IterationIndex = 0;
            foreach (string languageFlag in availableLanguages_names)
            {
                string name = languageFlag.Substring(languageFlag.Remove(languageFlag.LastIndexOf('.')).LastIndexOf('.') + 1, 2);
                StackPanel languageItem = new StackPanel() { Orientation = Orientation.Horizontal, Name = name };
                languageItem.Children.Add(new Image() { Source = BitmapFrame.Create(Assembly.GetExecutingAssembly().GetManifestResourceStream(languageFlag)), Width = 25, Height = 20, VerticalAlignment = System.Windows.VerticalAlignment.Center, HorizontalAlignment = System.Windows.HorizontalAlignment.Left });
                languageItem.Children.Add(new Label() { Content = puzzle.Resources.languages.LanguageResources.ResourceManager.GetString(name) });
                LanguageComboBoxItems.Add(languageItem);

                if (name == CultureInfo.CurrentUICulture.TwoLetterISOLanguageName || (LanguageIndex == -1 && name == "en"))
                    LanguageIndex = IterationIndex;
                IterationIndex++;
            }
        }
        public void ChangeLanguage(string newLanguage)
        {
            LanguageDataSet.Tables.Clear();
            SqlCeConnection con = new SqlCeConnection("Data Source=" + new System.IO.FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).DirectoryName + "\\resources\\languages\\" + newLanguage + ".sdf;Password=" + ASCIIEncoding.UTF8.GetString(Convert.FromBase64String("fk1nWHF6QSQ=")) + ";Persist Security Info=True");
            con.Open();
            SqlCeDataReader tableReader = new SqlCeCommand("SELECT table_name FROM INFORMATION_SCHEMA.Tables", con).ExecuteReader();
            while (tableReader.Read())
                new SqlCeDataAdapter(String.Format("SELECT * FROM  {0}", tableReader[0]), con).Fill(LanguageDataSet, tableReader[0] as string);
            tableReader.Close();
            con.Close();
        }

        #region IDisposable
        bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~DisplayLanguageHandler() { Dispose(false); }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
                LanguageDataSet.Dispose();

            _disposed = true;
        }
        #endregion
    }
}
