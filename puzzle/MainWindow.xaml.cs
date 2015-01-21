using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Documents;
using System.IO.IsolatedStorage;
using System.Collections.Generic;
 

namespace puzzle
{
    public partial class MainWindow : Window
    {
        [Serializable]
        public struct rememberedUser { 
            public Guid GUID {get; set; } 
            public string Username {get; set; }
            public string Password { get; set; }
            public int HighScore { get; set; }
        }

        public MainWindow()
        {
            InitializeComponent();
            //SET LANGUAGE
            language_comboBox.ItemsSource = Global.LanguageAgent.LanguageComboBoxItems;
            if (App.Current.Properties[0] != null)
                language_comboBox.SelectedIndex = (Int32)App.Current.Properties[0];
            else
                language_comboBox.SelectedIndex = Global.LanguageAgent.LanguageIndex;

            try //CHECK THAT ALL USER DATA IS THERE AND IF NOT, DELETE FROM THE DATABASE WHAT IS MISSING
            {
                var folders = Directory.EnumerateDirectories(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\games", "*", SearchOption.TopDirectoryOnly).Select(Path.GetFileName);
                List<string> deleted = new List<string>();
                foreach (rememberedUser x in Global.rememberedUsers_dictionary.Values)
                    if (!folders.Contains(x.Username))
                        deleted.Add(x.Username);
                foreach (string x in deleted)
                    Global.rememberedUsers_dictionary.Remove(x);
            } catch { }      
        }
        private void language_comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            App.Current.Properties[0] = ((ComboBox)sender).SelectedIndex;
            Global.LanguageAgent.LanguageIndex = ((ComboBox)sender).SelectedIndex;
            Global.LanguageAgent.ChangeLanguage(((StackPanel)e.AddedItems[0]).Name);

            this.Title = "Piczle - " + Global.LanguageAgent.LanguageDataSet.Tables["mainPage"].Select("UIElement_name = 'page_title'")[0][1];
            welcome_label.Content = Global.LanguageAgent.LanguageDataSet.Tables["mainPage"].Select("UIElement_name = 'welcome_label'")[0][1];
            regularGame_button.Content = new Viewbox() { Stretch = Stretch.Uniform, Child = new TextBlock(new Run(Global.LanguageAgent.LanguageDataSet.Tables["mainPage"].Select("UIElement_name = 'regularGame_button'")[0][1].ToString())) };
            continueGame_button.Content = new Viewbox() { Stretch = Stretch.Uniform, Child = new TextBlock(new Run(Global.LanguageAgent.LanguageDataSet.Tables["mainPage"].Select("UIElement_name = 'continueGame_button'")[0][1].ToString())) };
            highScore_button.Content = new Viewbox() { Stretch = Stretch.Uniform, Child = new TextBlock(new Run(Global.LanguageAgent.LanguageDataSet.Tables["mainPage"].Select("UIElement_name = 'highScore_button'")[0][1].ToString())) };
            exit_button.Content = new Viewbox() { Stretch = Stretch.Uniform, Child = new TextBlock(new Run(Global.LanguageAgent.LanguageDataSet.Tables["mainPage"].Select("UIElement_name = 'exit_button'")[0][1].ToString())) };
        }

        private void regularGame_button_Click(object sender, RoutedEventArgs e)
        {
            new newGame().Show();
            Global.isRunning = true;
            this.Close();
        }
        private void continueGame_button_Click(object sender, RoutedEventArgs e)
        {
            new continueGame().Show();
            Global.isRunning = true;
            this.Close();
        }
        private void highScore_button_Click(object sender, RoutedEventArgs e)
        {
            new highScores().Show();
            Global.isRunning = true;
            this.Close();
        }

        private void Window_Closing(object sender, EventArgs e) 
        {
            if (!Global.isRunning)
                Application.Current.Shutdown();
            else
                Global.isRunning = false;
        }
    }
}
