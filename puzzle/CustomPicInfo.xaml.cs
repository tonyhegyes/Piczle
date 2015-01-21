using System;
using System.Linq;
using System.Windows;
using puzzle.resources;
using DBreeze.DataTypes;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Collections.ObjectModel;


namespace puzzle
{
    public partial class CustomPicInfo : Window
    {
        public game.picInfo Information = new game.picInfo();
        ObservableCollection<string> CategoriesList = new ObservableCollection<string>();

        public CustomPicInfo()
        {
            InitializeComponent();

            //SET WATERMARKS
            WatermarkService.SetWatermark(name_textBox, Global.LanguageAgent.LanguageDataSet.Tables["customPicInfo"].Select("UIElement_name = 'name_textBox'")[0][1]);
            WatermarkService.SetWatermark(category_textBox, Global.LanguageAgent.LanguageDataSet.Tables["customPicInfo"].Select("UIElement_name = 'category_textBox'")[0][1]);
            WatermarkService.SetWatermark(customMessage_textBox, Global.LanguageAgent.LanguageDataSet.Tables["customPicInfo"].Select("UIElement_name = 'customMessage_textBox'")[0][1]);
            WatermarkService.SetWatermark(description_richTextBox, Global.LanguageAgent.LanguageDataSet.Tables["customPicInfo"].Select("UIElement_name = 'description_richTextBox'")[0][1]);
            finish_button.Content = Global.LanguageAgent.LanguageDataSet.Tables["customPicInfo"].Select("UIElement_name = 'finish_button'")[0][1];
            this.Title = Global.LanguageAgent.LanguageDataSet.Tables["customPicInfo"].Select("UIElement_name = 'page_title'")[0][1] as string;

            //LOAD PRE-EXISTING CATEGORIES
            using (var tran = Global.userEngine.GetTransaction())
            {
                try 
                {
                    var localDataSerialized = tran.Select<string, DbCustomSerializer<ObservableCollection<string>>>("userInfo", "categories");
                    if (localDataSerialized.Exists)
                        CategoriesList = localDataSerialized.Value.Get;
                } catch (Exception ex) { MessageBox.Show(ex.ToString()); }
            }
            category_textBox.ItemsSource = CategoriesList;
        }

        private void category_textBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if ((sender as ComboBox).Text != null && (sender as ComboBox).Text.Length > 0 && CategoriesList.FirstOrDefault(category => { return category.Equals((sender as ComboBox).Text); }) == null)
            {
                CategoriesList.Add((sender as ComboBox).Text);

                using (var tran = Global.userEngine.GetTransaction())
                {
                    try
                    {
                        tran.Insert<string, DbCustomSerializer<ObservableCollection<string>>>("userInfo", "categories", CategoriesList);
                        tran.Commit();
                    } catch (Exception ex) { MessageBox.Show(ex.ToString()); }
                }
            }
        }

        private void finish_button_Click(object sender, RoutedEventArgs e) 
        {
            if (name_textBox.Text.Length == 0 || category_textBox.Text.Length == 0 || customMessage_textBox.Text.Length == 0 || new TextRange(description_richTextBox.Document.ContentStart, description_richTextBox.Document.ContentEnd).Text.Length == 0)
            {
                var msg_box = MessageBox.Show(Global.LanguageAgent.LanguageDataSet.Tables["customPicInfo"].Select("UIElement_name = 'blank_message'")[0][1] as string, null, MessageBoxButton.YesNo);
                if (msg_box == MessageBoxResult.No)
                    return;
            }

            Information = new game.picInfo() { Name = name_textBox.Text, Category = (category_textBox.Text.Length == 0 ? "-----" : category_textBox.Text), CustomCompletionMessage = customMessage_textBox.Text, Description = new TextRange(description_richTextBox.Document.ContentStart, description_richTextBox.Document.ContentEnd).Text }; 
            this.Close(); 
        }
        private void cancel_button_Click(object sender, RoutedEventArgs e) 
        {
            var msg_box = MessageBox.Show(Global.LanguageAgent.LanguageDataSet.Tables["customPicInfo"].Select("UIElement_name = 'cancel_message'")[0][1] as string, null, MessageBoxButton.YesNo);
            if (msg_box == MessageBoxResult.No)
                return;

            Information = new game.picInfo() { Category = "-----" };
            this.Close();
        }
    }
}
