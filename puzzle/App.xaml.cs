using System;
using System.IO;
using System.Windows;
using System.IO.IsolatedStorage;


namespace puzzle
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                IsolatedStorageFile isolatedStorage = IsolatedStorageFile.GetUserStoreForAssembly();
                StreamWriter srWriter = new StreamWriter(new IsolatedStorageFileStream("rememberedLanguage", FileMode.Create, isolatedStorage));
                
                if (App.Current.Properties[0] != null)
                {
                    srWriter.WriteLine(App.Current.Properties[0].ToString());
                }

                srWriter.Flush();
                srWriter.Close();
            }
            catch (System.Security.SecurityException sx)
            {
                MessageBox.Show(sx.Message);
                throw;
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                IsolatedStorageFile isolatedStorage = IsolatedStorageFile.GetUserStoreForAssembly();
                StreamReader srReader = new StreamReader(new IsolatedStorageFileStream("rememberedLanguage", FileMode.OpenOrCreate, isolatedStorage));

                if (srReader == null)
                {
                    MessageBox.Show("No Data stored!");
                }
                else
                {
                    while (!srReader.EndOfStream)
                    {
                        string item = srReader.ReadLine();
                        App.Current.Properties[0] = Convert.ToInt32(item);
                    }
                }
                srReader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw;
            }
        }
    }
}
