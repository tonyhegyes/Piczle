using System;
using DBreeze;
using System.Linq;
using System.Text;
using System.Windows;
using puzzle.resources;
using System.Reflection;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Security.Cryptography;
using System.Collections.ObjectModel;


namespace puzzle
{
   
    public partial class newGame : Window
    {

        public newGame()
        {
            InitializeComponent();

            //SET WATERMARKS
            WatermarkService.SetWatermark(username_textBox, Global.LanguageAgent.LanguageDataSet.Tables["newGame"].Select("UIElement_name = 'username_textBox'")[0][1]);
            WatermarkService.SetWatermark(password_passwordBox, Global.LanguageAgent.LanguageDataSet.Tables["newGame"].Select("UIElement_name = 'password_passwordBox'")[0][1]);
            WatermarkService.SetWatermark(password2_passwordBox, Global.LanguageAgent.LanguageDataSet.Tables["newGame"].Select("UIElement_name = 'password2_passwordBox'")[0][1]);
            welcome_label.Content = Global.LanguageAgent.LanguageDataSet.Tables["newGame"].Select("UIElement_name = 'welcome_label'")[0][1];
            instructions_label.Text = Global.LanguageAgent.LanguageDataSet.Tables["newGame"].Select("UIElement_name = 'instructions_label'")[0][1] as string;
            finish_button.Content = Global.LanguageAgent.LanguageDataSet.Tables["newGame"].Select("UIElement_name = 'finish_button'")[0][1];
            this.Title = "Piczle - " + Global.LanguageAgent.LanguageDataSet.Tables["newGame"].Select("UIElement_name = 'page_title'")[0][1];
        }

        private void finish_button_Click(object sender, RoutedEventArgs e)
        {
            //DATA VALIDATION
            username_textBox_TextChanged(username_textBox, null);
            if (username_textBox.BorderBrush == Brushes.Red)
                if (username_textBox.Text.Length == 0)
                { MessageBox.Show(Global.LanguageAgent.LanguageDataSet.Tables["newGame"].Select("UIElement_name = 'username_missing_error'")[0][1].ToString()); return; }
                else
                { MessageBox.Show(Global.LanguageAgent.LanguageDataSet.Tables["newGame"].Select("UIElement_name = 'username_error'")[0][1].ToString()); return; }

            password1_passwordBox_PasswordChanged(password_passwordBox, null);
            if(password_passwordBox.BorderBrush == Brushes.Red )
            { MessageBox.Show(Global.LanguageAgent.LanguageDataSet.Tables["newGame"].Select("UIElement_name = 'password_short_error'")[0][1].ToString()); return; }
            password2_passwordBox_PasswordChanged(password2_passwordBox, null);
            if (password2_passwordBox.BorderBrush == Brushes.Red)
            { MessageBox.Show(Global.LanguageAgent.LanguageDataSet.Tables["newGame"].Select("UIElement_name = 'password_mismatch_error'")[0][1].ToString()); return; }

            //ACCOUNT CREATION
            Guid userGUID = Guid.NewGuid();
            Global.rememberedUsers_dictionary.Add(username_textBox.Text, new MainWindow.rememberedUser() { GUID = userGUID, Username = username_textBox.Text, Password = Convert.ToBase64String(ProtectedData.Protect(Encoding.ASCII.GetBytes(password_passwordBox.Password), Encoding.ASCII.GetBytes(username_textBox.Text), DataProtectionScope.CurrentUser))  });
            Global.rememberedUsers_dictionary.Flush(); 

            //LOGIN ACTIONS
            new game(userGUID, username_textBox.Text, password_passwordBox.SecurePassword).Show();
            Global.isRunning = true; this.Close();
        }

        private void username_textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (((TextBox)sender).Text.Length == 0)
            {
                username_textBox.BorderBrush = Brushes.Red;
                username_error_label.Text = Global.LanguageAgent.LanguageDataSet.Tables["newGame"].Select("UIElement_name = 'username_missing_error'")[0][1] as string;
            }
            else
            {
                if (Global.rememberedUsers_dictionary.ContainsKey(((TextBox)sender).Text))
                {
                    username_textBox.BorderBrush = Brushes.Red;
                    username_error_label.Text = Global.LanguageAgent.LanguageDataSet.Tables["newGame"].Select("UIElement_name = 'username_error'")[0][1] as string;
                }
                else
                {
                    username_textBox.BorderBrush = Brushes.Green;
                    username_error_label.Text = String.Empty;
                }
            }
        }
        private void password1_passwordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (password_passwordBox.Password.Length < 8)
            {
                password_passwordBox.BorderBrush = Brushes.Red;
                password1_error_label.Text = Global.LanguageAgent.LanguageDataSet.Tables["newGame"].Select("UIElement_name = 'password_short_error'")[0][1] as string;
            }
            else
            {
                password_passwordBox.BorderBrush = Brushes.Green;
                password1_error_label.Text = String.Empty;
            }

            if (password2_passwordBox.Password.Length > 0)
                if (password_passwordBox.Password.Equals(password2_passwordBox.Password))
                {
                    password2_passwordBox.BorderBrush = Brushes.Green;
                    password2_error_label.Text = String.Empty;
                }
                else
                {
                    password2_passwordBox.BorderBrush = Brushes.Red;
                    password2_error_label.Text = Global.LanguageAgent.LanguageDataSet.Tables["newGame"].Select("UIElement_name = 'password_mismatch_error'")[0][1] as string;
                }
        }
        private void password2_passwordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (password_passwordBox.Password.Equals(password2_passwordBox.Password) && password2_passwordBox.Password.Length != 0)
            {
                password2_passwordBox.BorderBrush = Brushes.Green;
                password2_error_label.Text = String.Empty;

                if (password_passwordBox.Password.Length < 8)
                {
                    password_passwordBox.BorderBrush = Brushes.Red;
                    password1_error_label.Text = Global.LanguageAgent.LanguageDataSet.Tables["newGame"].Select("UIElement_name = 'password_short_error'")[0][1] as string;
                }
                else
                    password1_error_label.Text = String.Empty;
            }
            else
            {
                password2_passwordBox.BorderBrush = Brushes.Red;
                password2_error_label.Text = Global.LanguageAgent.LanguageDataSet.Tables["newGame"].Select("UIElement_name = 'password_mismatch_error'")[0][1] as string;
            }            
        }
        
        private void back_pictureButton_MouseEnter(object sender, MouseEventArgs e) { back_pictureButton.Source = new BitmapImage(new Uri("resources/pictures/back_hover.png", UriKind.Relative)); }
        private void back_pictureButton_MouseLeave(object sender, MouseEventArgs e) { back_pictureButton.Source = new BitmapImage(new Uri("resources/pictures/back.png", UriKind.Relative)); }
        private void back_pictureButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            back_pictureButton.Source = new BitmapImage(new Uri("resources/pictures/back_click.png", UriKind.Relative));
            new MainWindow().Show(); Global.isRunning = true; this.Close();
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
