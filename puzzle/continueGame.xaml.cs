using System;
using System.Text;
using System.Windows;
using puzzle.resources;
using System.Security.Cryptography;


namespace puzzle
{
    public partial class continueGame : Window
    {
        public continueGame()
        {
            InitializeComponent();

            try
            {
                //SET WATERMARKS
                welcome_label.Content =  Global.LanguageAgent.LanguageDataSet.Tables["continueGame"].Select("UIElement_name = 'welcome_label'")[0][1];
                WatermarkService.SetWatermark(username_textBox, Global.LanguageAgent.LanguageDataSet.Tables["continueGame"].Select("UIElement_name = 'username_textBox'")[0][1]);
                WatermarkService.SetWatermark(password_passwordBox, Global.LanguageAgent.LanguageDataSet.Tables["continueGame"].Select("UIElement_name = 'password_passwordBox'")[0][1]);
                finish_button.Content = Global.LanguageAgent.LanguageDataSet.Tables["continueGame"].Select("UIElement_name = 'finish_button'")[0][1];
                this.Title = "Piczle - " + Global.LanguageAgent.LanguageDataSet.Tables["continueGame"].Select("UIElement_name = 'welcome_label'")[0][1];
            } catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }

        private void finish_button_Click(object sender, RoutedEventArgs e)
        {
            if (!Global.rememberedUsers_dictionary.ContainsKey(username_textBox.Text))
            { MessageBox.Show(Global.LanguageAgent.LanguageDataSet.Tables["continueGame"].Select("UIElement_name = 'username_error'")[0][1].ToString()); return; }
            if (password_passwordBox.Password != Encoding.ASCII.GetString(ProtectedData.Unprotect(Convert.FromBase64String(Global.rememberedUsers_dictionary[username_textBox.Text].Password), Encoding.ASCII.GetBytes(username_textBox.Text), DataProtectionScope.CurrentUser)))
            { MessageBox.Show(Global.LanguageAgent.LanguageDataSet.Tables["continueGame"].Select("UIElement_name = 'password_error'")[0][1].ToString()); return; }

            new game(Global.rememberedUsers_dictionary[username_textBox.Text].GUID, username_textBox.Text, password_passwordBox.SecurePassword).Show();
            Global.isRunning = true; this.Close();
        }

        private void cancel_button_Click(object sender, RoutedEventArgs e) { new MainWindow().Show(); Global.isRunning = true; this.Close(); }
        private void Window_Closing(object sender, EventArgs e)
        {
            if (!Global.isRunning)
                Application.Current.Shutdown();
            else
                Global.isRunning = false;
        }
    }
}
