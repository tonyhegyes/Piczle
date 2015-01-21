using System;
using DBreeze;
using System.IO;
using System.Net;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using System.Security;
using Newtonsoft.Json;
using System.Threading;
using DBreeze.DataTypes;
using System.Reflection;
using System.Net.Sockets;
using System.Diagnostics;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Net.NetworkInformation;
using System.Windows.Media.Animation;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

using System.ServiceModel;
using System.ServiceModel.Description;
using onlinePiczle;
using puzzle.Providers.Security;


namespace puzzle
{
    public partial class game : Window
    {
        #region HANDY VARIABLES
            //PUZZLE CREATION
            int imagePos;
            BitmapImage image;
            DateTime startTime;
            bool isSolving = false;
            bool justRotated = false;
            bool IsCompleted = false;
            double nrRows = 4, nrCols = 4;
            int[] completedPositions = new int[16];
            rectangle selectedRect = new rectangle();
            List<Rectangle> allocatedParts = new List<Rectangle>();
            List<Rectangle> initialUnallocatedParts = new List<Rectangle>();
            DispatcherTimer dispatcherTimer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 1) };
            struct rectangle { public Rectangle Rect { get; set; } public int TileRow { get; set; } public int TileCol { get; set; } };
       
            //OUTLOOK AND GROUPING
            int NrDefaultPics = 0;
            sessionInformations sessionInfo = new sessionInformations();
            
            [Serializable]
            public struct picInfo { public string Name, Category, Description, CustomCompletionMessage; };
            public class PicThumbnail { public Image Image { get; set; } public string Category { get; set; } public picInfo Information { get; set; } };
            ObservableCollection<PicThumbnail> thumbnails = new ObservableCollection<PicThumbnail>();
        #endregion

        ServiceHost host;

        public game(Guid guid, string user, SecureString pass)
        {
            InitializeComponent();
            
            sessionInfo.User = user; sessionInfo.Pass = pass; sessionInfo.GUID = guid;
            sessionInfo.Score = Global.rememberedUsers_dictionary[user].HighScore;
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);

            //SET TEXT
            this.Title = "Piczle - " + Global.LanguageAgent.LanguageDataSet.Tables["gameWindow"].Select("UIElement_name = 'page_title'")[0][1];
            score_label.Content = Global.LanguageAgent.LanguageDataSet.Tables["gameWindow"].Select("UIElement_name = 'score_label'")[0][1];
            score_label.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity)); score_label.Arrange(new Rect(score_label.DesiredSize)); //FORCE MEASUREMENT
            score_label.Margin = new Thickness(0, score_label.Margin.Top, puzzleDockPanel.Margin.Right - 10 - score_label.ActualWidth, 0);
            Label x = new Label() { Content = sessionInfo.Score, Name = "scoreValue_label", VerticalAlignment = VerticalAlignment.Top, HorizontalAlignment = HorizontalAlignment.Right, FontSize = 20, FontWeight = FontWeights.Bold, Foreground = Brushes.Red };
            x.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity)); x.Arrange(new Rect(x.DesiredSize)); //FORCE MEASUREMENT
            x.Margin = new Thickness(0, score_label.Margin.Top, puzzleDockPanel.Margin.Right - 10 - score_label.ActualWidth - x.ActualWidth, 0);
            x.SetBinding(Label.ContentProperty, new Binding("Score") { Source = sessionInfo, IsAsync = true, Mode = BindingMode.OneWay });
            mainGrid.Children.Add(x);
            gameType_toggler.Content = Global.LanguageAgent.LanguageDataSet.Tables["gameWindow"].Select("UIElement_name = 'gameType_toggler'")[0][1];
            puzzle.resources.ThemeProperties.SetLeftText(gameType_toggler, Global.LanguageAgent.LanguageDataSet.Tables["gameWindow"].Select("UIElement_name = 'leftText_label'")[0][1] as string);
            puzzle.resources.ThemeProperties.SetRightText(gameType_toggler, Global.LanguageAgent.LanguageDataSet.Tables["gameWindow"].Select("UIElement_name = 'rightText_label'")[0][1] as string);
            rows_label.Content = Global.LanguageAgent.LanguageDataSet.Tables["gameWindow"].Select("UIElement_name = 'rows_label'")[0][1];
            columns_label.Content = Global.LanguageAgent.LanguageDataSet.Tables["gameWindow"].Select("UIElement_name = 'columns_label'")[0][1];
            cr_tooltip_header.Text = Global.LanguageAgent.LanguageDataSet.Tables["gameWindow"].Select("UIElement_name = 'cr_tooltip_header'")[0][1] as string;
            cr_tooltip_content.Text = Global.LanguageAgent.LanguageDataSet.Tables["gameWindow"].Select("UIElement_name = 'cr_tooltip_content'")[0][1] as string;
            rotate_tooltip_header.Text = Global.LanguageAgent.LanguageDataSet.Tables["gameWindow"].Select("UIElement_name = 'rotate_tooltip_header'")[0][1] as string;
            rotate_tooltip_content.Text = Global.LanguageAgent.LanguageDataSet.Tables["gameWindow"].Select("UIElement_name = 'rotate_tooltip_content'")[0][1] as string;
            start_button.Content = Global.LanguageAgent.LanguageDataSet.Tables["gameWindow"].Select("UIElement_name = 'start_button'")[0][1];
            stop_button.Content = Global.LanguageAgent.LanguageDataSet.Tables["gameWindow"].Select("UIElement_name = 'stop_button'")[0][1];
            uploadImage_button.Content = Global.LanguageAgent.LanguageDataSet.Tables["gameWindow"].Select("UIElement_name = 'uploadImage_button'")[0][1];
            scramble_button.Content = Global.LanguageAgent.LanguageDataSet.Tables["gameWindow"].Select("UIElement_name = 'scramble_button'")[0][1];
            solve_button.Content = Global.LanguageAgent.LanguageDataSet.Tables["gameWindow"].Select("UIElement_name = 'solve_button'")[0][1];
            picSelect_label.Content = Global.LanguageAgent.LanguageDataSet.Tables["gameWindow"].Select("UIElement_name = 'picSelect_label'")[0][1];
            picSelect_label.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity)); picSelect_label.Arrange(new Rect(picSelect_label.DesiredSize));
            picSelect_label.Margin = new Thickness(0, picSelect_label.Margin.Top, picSelect_label.Margin.Right - picSelect_label.ActualWidth, 0);

            //DATABASE DEFAULT CONFIGURATION
            Global.userEngine = new DBreezeEngine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\games\" + user);
            DBreeze.Utils.CustomSerializator.Serializator = JsonConvert.SerializeObject; DBreeze.Utils.CustomSerializator.Deserializator = JsonConvert.DeserializeObject;

            //LOAD SECURITY PROTOCOL
            Global.UserDataEncryption = new RijndaelEncryption(RijndaelEncryption.GetBase64sCryptString(guid.ToString(), Global.SecureString_toString(pass), 0), guid.ToString());

            //LOAD STANDARD PICTURES
            IEnumerable<string> pics =
                from string language in Assembly.GetExecutingAssembly().GetManifestResourceNames()
                where language.Contains(".jpg")
                select language;

            foreach (string s in pics)
                thumbnails.Add(new PicThumbnail() { Information = new picInfo() { Name = "p" + s.Substring(31, s.Length - 35), Category = Global.LanguageAgent.LanguageDataSet.Tables["gameWindow"].Select("UIElement_name = 'default_word'")[0][1] as string, CustomCompletionMessage = null, Description = Global.LanguageAgent.LanguageDataSet.Tables["pictureDescription"].Select("pictureName = 'p" + s.Substring(31, s.Length - 35) + "'")[0][1] as string }, Image = new Image() { Name = "p" + s.Substring(31, s.Length - 35), Source = BitmapFrame.Create(Assembly.GetExecutingAssembly().GetManifestResourceStream(s)) }, Category = Global.LanguageAgent.LanguageDataSet.Tables["gameWindow"].Select("UIElement_name = 'default_word'")[0][1] as string });
            NrDefaultPics = thumbnails.Count;

            //CREATE GROUPING IN THE LISTVIEW
            ICollectionView view = CollectionViewSource.GetDefaultView(thumbnails);
            view.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
            pictures_listbox.ItemsSource = view;  

            //
            ipAddress_textBox.Text = LocalIPAddress();

            //////
            this.host = new ServiceHost(typeof(TransferService), new Uri("net.tcp://localhost:8003/HelloWCF"));

            // Service host is opened on the UI thread
            host.AddServiceEndpoint(typeof(ITransferService), new NetTcpBinding(SecurityMode.None), "HelloWCF");

            // Enable exeption details
            ServiceDebugBehavior sdb = host.Description.Behaviors.Find<ServiceDebugBehavior>();
            sdb.IncludeExceptionDetailInFaults = true;

            host.Open();

            // Keep track of the UI thread id
            this.Title = System.Threading.Thread.CurrentThread.ManagedThreadId.ToString();

            EndpointAddress epoint = new EndpointAddress("net.tcp://localhost:8003/HelloWCF/HelloWCF");
            onlinePiczle.ITransferService proxy = ChannelFactory<onlinePiczle.ITransferService>.CreateChannel(new NetTcpBinding(SecurityMode.None), epoint);

            using (proxy as IDisposable)
            {
                MessageBox.Show(proxy.GetData(29));
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //LOAD USER PICTURES
            Task loadUserPictures = new Task(new Action(delegate()
            {
                using (var tran = Global.userEngine.GetTransaction())
                {
                    try
                    {
                        var picTables = Global.userEngine.Scheme.GetUserTableNamesStartingWith("");
                        foreach (string s in picTables)
                        {
                            var localDataSerialized = tran.Select<string, DbCustomSerializer<picInfo>>(s, "information");
                            picInfo Information = new picInfo();
                            if (localDataSerialized.Exists)
                                Information = localDataSerialized.Value.Get;

                            var localDataSerialized2 = tran.Select<string, string>(s, "picture");
                            if (localDataSerialized2.Exists)
                            {
                                byte[] picBytes = Convert.FromBase64String(Global.UserDataEncryption.DecryptStringFromBytes(Convert.FromBase64String(localDataSerialized2.Value)));
                                this.Dispatcher.Invoke(DispatcherPriority.Send, new Action(delegate() { thumbnails.Add(new PicThumbnail() { Information = Information, Image = new Image() { Name = "s" + s, Source = BitmapFrame.Create(new MemoryStream(picBytes)) }, Category = Information.Category }); }));
                            }
                        }
                    } catch (Exception ex) { MessageBox.Show(ex.ToString()); }
                }
            })); loadUserPictures.Start();
        }

        public string LocalIPAddress()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
                return null;

            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            return host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToString();
        }

        private void picBoxExpander_Expanded(object sender, RoutedEventArgs e) { mainGrid.Height += 130; if(parts_listBox != null) parts_listBox.Height += 132; }
        private void picBoxExpander_Collapsed(object sender, RoutedEventArgs e) { mainGrid.Height -= 130; parts_listBox.Height -= 132; }

        private void rows_textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((sender as TextBox).Text.Length == 0)
                return;

            try
            {
                int inputRows = Convert.ToInt16((sender as TextBox).Text);
                if (inputRows < 3 || inputRows > 9)
                    throw new NotSupportedException();

                nrRows = inputRows;
            }
            catch
            {
                Action<TextBox> animMethod = (TextBox x) =>
                {
                    var brush = new SolidColorBrush(Colors.Red);
                    rows_border.BorderBrush = brush; 
                    brush.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation(Colors.White, TimeSpan.FromSeconds(1)));

                    x.Text = nrRows.ToString();
                }; animMethod(sender as TextBox);
            }
        }
        private void columns_textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((sender as TextBox).Text.Length == 0)
                return;

            try
            {
                int inputCols = Convert.ToInt16((sender as TextBox).Text);
                if (inputCols < 3 || inputCols > 9)
                    throw new NotSupportedException();

                nrCols = inputCols;
            }
            catch
            {
                Action<TextBox> animMethod = (TextBox x) =>
                {
                    var brush = new SolidColorBrush(Colors.Red);
                    columns_border.BorderBrush = brush;
                    brush.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation(Colors.White, TimeSpan.FromSeconds(1)));

                    x.Text = nrCols.ToString();
                }; animMethod(sender as TextBox);
            }
        }
  
        private void gameType_toggler_Checked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 35; i++)
            {
                mainGrid.Width += 4; 
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => { })).Wait();
            }

            parts_scrollViewer.Visibility = rotate_help_image.Visibility = rotateLeft_image.Visibility = rotateRight_image.Visibility = System.Windows.Visibility.Visible;
            puzzleGrid.AllowDrop = true; puzzleGrid.Drop += rectPart_Drop; puzzleGrid.ShowGridLines = true;
        }
        private void gameType_toggler_Unchecked(object sender, RoutedEventArgs e)
        {
            parts_scrollViewer.Visibility = rotate_help_image.Visibility = rotateLeft_image.Visibility = rotateRight_image.Visibility = System.Windows.Visibility.Hidden;
            puzzleGrid.AllowDrop = false; puzzleGrid.Drop -= rectPart_Drop; puzzleGrid.ShowGridLines = false;

            for (int i = 0; i < 35; i++)
            {
                mainGrid.Width -= 4; 
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => { })).Wait();
            }
        }

        #region NEW PICTURE MANIPULATION
            private void uploadImage_Click(object sender, RoutedEventArgs e)
            {
                OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog() { Multiselect = false };
                ofd.Filter = "Image Files(*.BMP;*.JPG;*.GIF;*.PNG)|*.BMP;*.JPG;*.GIF;*.PNG" +
                            "|All Files (*.*)|*.*";

                if (ofd.ShowDialog() == true)
                {
                    try
                    {
                        //OPEN AND RESIZE IMAGE
                        byte[] imageBytes = LoadImageData(ofd.FileName);
                        ImageSource newImage = CreateImage(imageBytes, 400, 400);
                        imageBytes = GetEncodedImageData(newImage);
                        //GET INFORMATION ABOUT THE PICTURE
                        var Information = new picInfo();
                        var infoWindow = new CustomPicInfo();
                        if (infoWindow.ShowDialog() == false)
                            Information = infoWindow.Information;
                        //ADD PICTURE TO DATABASES
                        thumbnails.Add(new PicThumbnail() { Image = new Image() { Name = "s" + (thumbnails.Count - NrDefaultPics), Source = BitmapFrame.Create(new MemoryStream(imageBytes)), Width = 100, Height = 75, SnapsToDevicePixels = true, Stretch = Stretch.Fill, ClipToBounds = true, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center }, Category = Information.Category, Information = Information });

                        using (var tran = Global.userEngine.GetTransaction())
                        {
                            tran.Insert<string, DbCustomSerializer<picInfo>>((thumbnails.Count - NrDefaultPics).ToString(), "information", Information);
                            tran.Insert<string, string>((thumbnails.Count - NrDefaultPics).ToString(), "picture", Convert.ToBase64String(Global.UserDataEncryption.EncryptStringToBytes(Convert.ToBase64String(imageBytes, Base64FormattingOptions.InsertLineBreaks)), Base64FormattingOptions.InsertLineBreaks));
                            tran.Commit();
                        }

                        //START GAME WITH UPLOADED PICTURE
                        pictureSelect_MouseLeftButtonUp(thumbnails.Last().Image, null);
                        sessionInfo.Score += 5;
                    } catch (Exception ex) { MessageBox.Show(ex.ToString()); }
                }
            }
            private static byte[] LoadImageData(string filePath)
            {
                FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                BinaryReader br = new BinaryReader(fs);
                byte[] imageBytes = br.ReadBytes((int)fs.Length);
                br.Close(); fs.Close();

                return imageBytes;
            }
            internal byte[] GetEncodedImageData(ImageSource image)
            {
                byte[] result = null;
                BitmapEncoder encoder = new JpegBitmapEncoder();

                if (image is BitmapSource)
                {
                    MemoryStream stream = new MemoryStream();

                    encoder.Frames.Add(BitmapFrame.Create(image as BitmapSource));
                    encoder.Save(stream);

                    stream.Seek(0, SeekOrigin.Begin);

                    result = new byte[stream.Length];

                    BinaryReader br = new BinaryReader(stream);
                    br.Read(result, 0, (int)stream.Length);
                    br.Close(); stream.Close();
                }
                return result;
            }
            private static ImageSource CreateImage(byte[] imageData, int decodePixelWidth, int decodePixelHeight)
            {
                if (imageData == null) 
                    return null;

                BitmapImage result = new BitmapImage();
            
                result.BeginInit();

                if (decodePixelWidth > 0)
                    result.DecodePixelWidth = decodePixelWidth;
                if (decodePixelHeight > 0)
                    result.DecodePixelHeight = decodePixelHeight;

                result.StreamSource = new MemoryStream(imageData);
                result.CreateOptions = BitmapCreateOptions.None;
                result.CacheOption = BitmapCacheOption.Default;

                result.EndInit();

                return result;
            }
        #endregion

        #region PUZZLE MANIPULATION
            private void pictureSelect_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
            {
                if (dispatcherTimer.IsEnabled)
                    return;
                
                parts_listBox.Children.Clear();

                for (int i = 0; i < thumbnails.Count; i++)
                    if (thumbnails[i].Image.Source == ((Image)sender).Source)
                    { imagePos = i; break; }

                image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = new MemoryStream(GetEncodedImageData(((Image)sender).Source));
                image.EndInit();

                timer_label.Content = "00:00:00";
                start_button.IsEnabled = true;
                solve_button.IsEnabled = scramble_button.IsEnabled = stop_button.IsEnabled = false;
                auxiliaryGrid.Children.Add(new Image() { Margin = new Thickness(0), Source = ((Image)sender).Source, Width = puzzleGrid.ActualWidth, Height = puzzleGrid.ActualHeight, Stretch = System.Windows.Media.Stretch.Fill, VerticalAlignment = VerticalAlignment.Top, HorizontalAlignment = HorizontalAlignment.Left });
            }
            private void stopButton_Click(object sender, RoutedEventArgs e)
            {
                var msgbox_response = MessageBox.Show(Global.LanguageAgent.LanguageDataSet.Tables["gameWindow"].Select("UIElement_name = 'stop_message'")[0][1] as string, null, MessageBoxButton.YesNo);
                if (msgbox_response == MessageBoxResult.No)
                    return;

                IsCompleted = false;
                dispatcherTimer.Stop();
                sessionInfo.Score -= 5;
                solve_button.IsEnabled = stop_button.IsEnabled = false;
                start_button.IsEnabled = rows_textBox.IsEnabled = columns_textBox.IsEnabled = gameType_toggler.IsEnabled = uploadImage_button.IsEnabled = true;
            }
            private void startButton_Click(object sender, RoutedEventArgs e)
            {
                rows_textBox.IsEnabled = columns_textBox.IsEnabled = gameType_toggler.IsEnabled = uploadImage_button.IsEnabled = false;
                solve_button.IsEnabled = scramble_button.IsEnabled = stop_button.IsEnabled = true;
                auxiliaryGrid.Children.Clear();

                start_button.IsEnabled = false;
                startTime = DateTime.UtcNow;
                dispatcherTimer.Start();

                CreatePuzzleImage();
            }
            private void dispatcherTimer_Tick(object sender, EventArgs e)
            {
                TimeSpan x = DateTime.UtcNow.Subtract(startTime); 
                decimal y = Math.Round((decimal) x.Seconds + x.Milliseconds / 1000);
                timer_label.Content = (x.Hours < 10 ? "0" : null) + x.Hours + ":" + (x.Minutes < 10 ? "0" : null) + x.Minutes + ":" + (y < 10 ? "0" : null) + y;
            }

            Random rand = new Random();
            private void CreatePuzzleImage()
            {
                IsCompleted = false;
                selectedRect.Rect = null;
                puzzleGrid.Children.Clear();
                parts_listBox.Children.Clear();
                description_textBlock.Text = null;

                allocatedParts.Clear();
                initialUnallocatedParts.Clear();
                completedPositions = new int[(int)(nrCols * nrRows)];
                
                puzzleGrid.RowDefinitions.Clear();
                puzzleGrid.ColumnDefinitions.Clear();
                for (int i = 0; i < nrCols; i++)
                    puzzleGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                for (int i = 0; i < nrRows; i++)
                {
                    puzzleGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
                    for (int j = 0; j < nrCols; j++)
                        CreateImagePart(j / nrCols, i / nrRows, 1 / nrCols, 1 / nrRows);
                }

                RandomizeTiles();

                if (gameType_toggler.IsChecked == false)
                {
                    int index = 0;
                    for (int i = 0; i < nrRows; i++)
                        for (int j = 0; j < nrCols; j++)
                        {
                            allocatedParts[index].SetValue(Grid.RowProperty, i);
                            allocatedParts[index].SetValue(Grid.ColumnProperty, j);
                            puzzleGrid.Children.Add(allocatedParts[index++]);
                        }
                }
                else
                {
                    for (int i = 0; i < nrRows * nrCols; i++)
                        parts_listBox.Children.Add(allocatedParts[i]);
                }
            }
            private void CreateImagePart(double x, double y, double width, double height)
            {
                ImageBrush ib = new ImageBrush()
                {
                    ImageSource = image, Stretch = Stretch.Fill, 
                    Viewport = new Rect(0, 0, 1.0, 1.0), Viewbox = new Rect(x, y, width, height),
                    ViewboxUnits = BrushMappingMode.RelativeToBoundingBox, TileMode = TileMode.None
                }; //GRAB IMAGE PORTION

                Rectangle rectPart = new Rectangle()
                {
                    Name = "r" + x * nrCols + "X" + y * nrRows, 
                    Fill = ib, Stroke = Brushes.Red, StrokeThickness = 0, Margin = new Thickness(0),
                    HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch
                }; rectPart.PreviewMouseLeftButtonDown += rectPart_PreviewMouseLeftButtonDown;

                if (gameType_toggler.IsChecked == true)
                {
                    rectPart.Width = 400 / nrCols; rectPart.Height = 400 / nrRows;
                    rectPart.PreviewMouseMove += new MouseEventHandler(rectPart_PreviewMouseMove);
                    rectPart.RenderTransform = new RotateTransform(90 * rand.Next(0, 4), rectPart.Width / 2, rectPart.Height / 2);
                }

                initialUnallocatedParts.Add(rectPart);
            }
            private void RandomizeTiles()
            {
                Random rand = new Random();
                int allocated = 0;
                while (allocated != nrRows * nrCols)
                {
                    int index = 0;
                    if (initialUnallocatedParts.Count > 1)
                        index = (int)(rand.NextDouble() * initialUnallocatedParts.Count);
                    allocatedParts.Add(initialUnallocatedParts[index]);
                    initialUnallocatedParts.RemoveAt(index);
                    completedPositions[allocated++] = index;
                }
            }


            struct Pos { public int x, y; };
            private Pos GetPieceIntendedPosition(string fromName)
            {
                Pos p;

                int Xp = fromName.IndexOf('X');
                p.x = Convert.ToInt32(fromName.Substring(0, Xp));
                p.y = Convert.ToInt32(fromName.Substring(Xp + 1, fromName.Length - Xp - 1));

                return p;
            }
            private void CompletionTest()
            {
                if (isSolving)
                    return;

                for (int i = 0; i < nrRows * nrCols; i++)
                {
                    Pos p = GetPieceIntendedPosition(allocatedParts[i].Name.Substring(1));
                    if (p.y != (int)allocatedParts[i].GetValue(Grid.RowProperty))
                        return;
                    else if (p.x != (int)allocatedParts[i].GetValue(Grid.ColumnProperty))
                        return;
                    else if (gameType_toggler.IsChecked == true && (allocatedParts[i].RenderTransform as RotateTransform).Angle % 360 != 0)
                        return;
                }

                IsCompleted = true;
                dispatcherTimer.Stop();
                stop_button.IsEnabled = false;
                rows_textBox.IsEnabled = columns_textBox.IsEnabled = gameType_toggler.IsEnabled = true;


                if (thumbnails[imagePos].Information.CustomCompletionMessage != null)
                    MessageBox.Show(thumbnails[imagePos].Information.CustomCompletionMessage);
                if (thumbnails[imagePos].Information.Description != null)
                    description_textBlock.Text = thumbnails[imagePos].Information.Description;

                TimeSpan x = DateTime.UtcNow.Subtract(startTime);
                sessionInfo.Score += (x.TotalSeconds < 10 * Math.Max(nrRows, nrCols) ? 10 : (int)(10 - (x.TotalSeconds - 10 * Math.Max(nrRows, nrCols)) / 10)) * (int)Math.Sqrt(nrRows * nrCols);
            }
            
            private void solvePuzzle_Click(object sender, RoutedEventArgs e)
            {
                var msgbox_response = MessageBox.Show(Global.LanguageAgent.LanguageDataSet.Tables["gameWindow"].Select("UIElement_name = 'solvePic_message'")[0][1] as string, null, MessageBoxButton.YesNo);
                if (msgbox_response == MessageBoxResult.No)
                    return;

                isSolving = true;
                stop_button.IsEnabled = false;
                if (gameType_toggler.IsChecked == true)
                {
                    //CHECK THAT EVERYTHING THE USER DID IS IN PLACE
                    int nr = puzzleGrid.Children.Count;
                    for (int i = 0; i < nr; i++)
                        moveInPlace(puzzleGrid.Children[0] as Rectangle);

                    //PUT EVERYTHING ELSE IN PLACE
                    int x = parts_listBox.Children.Count;
                    for (int i = 0; i < x; i++)
                        moveInPlace(parts_listBox.Children[0] as Rectangle);
                    justRotated = false; //CANCEL EFFECTS OF THE ROTATION
                }
                else
                {
                    bool completed = false;
                    while (!completed)
                    {
                        for (int i = 0; i < nrRows * nrCols; i++)
                        {
                            //WHERE I CURRENTLY AM
                            int x1 = (int)allocatedParts[i].GetValue(Grid.ColumnProperty);
                            int y1 = (int)allocatedParts[i].GetValue(Grid.RowProperty);

                            //WHERE I NEED TO GET
                            Pos p = GetPieceIntendedPosition(allocatedParts[i].Name.Substring(1));
                            int x2 = p.x; int y2 = p.y;
                            //---
                            if (x2 != x1 || y2 != y1)
                            {
                                int aux;
                                while (x1 != x2)
                                {
                                    aux = (x1 < x2 ? x1 + 1 : x1 - 1);
                                    Rectangle neighbour = allocatedParts.Find(rect => { return ((int)rect.GetValue(Grid.RowProperty) == y1 && (int)rect.GetValue(Grid.ColumnProperty) == aux); });
                                    allocatedParts[i].StrokeThickness = 1;

                                    switchPlaces(allocatedParts[i], neighbour, aux, x1, false);
                                    allocatedParts[i].StrokeThickness = 0;

                                    x1 = aux;
                                }
                                int auy;
                                while (y1 != y2)
                                {
                                    auy = (y1 < y2 ? y1 + 1 : y1 - 1);
                                    Rectangle neighbour = allocatedParts.Find(rect => { return ((int)rect.GetValue(Grid.ColumnProperty) == x1 && (int)rect.GetValue(Grid.RowProperty) == auy); });
                                    allocatedParts[i].StrokeThickness = 1;

                                    switchPlaces(allocatedParts[i], neighbour, auy, y1, true);
                                    allocatedParts[i].StrokeThickness = 0;

                                    y1 = auy;
                                }
                            }
                        }

                        completed = true;
                        for (int i = 0; i < nrCols * nrRows; i++)
                        {
                            Pos p = GetPieceIntendedPosition(allocatedParts[i].Name.Substring(1));
                            if (p.y != (int)allocatedParts[i].GetValue(Grid.RowProperty))
                            { completed = false; break; }
                            else if (p.x != (int)allocatedParts[i].GetValue(Grid.ColumnProperty))
                            { completed = false; break; }
                        }
                    }
                }

                isSolving = false;
                IsCompleted = true;
                dispatcherTimer.Stop();

                if (thumbnails[imagePos].Information.CustomCompletionMessage != null)
                    MessageBox.Show(thumbnails[imagePos].Information.CustomCompletionMessage);
                if (thumbnails[imagePos].Information.Description != null)
                    description_textBlock.Text = thumbnails[imagePos].Information.Description;

                sessionInfo.Score -= 5;
            }
            private void switchPlaces(Rectangle rect1, Rectangle rect2, int m, int n, bool y)
            {
                this.IsEnabled = false;

                //POSITION RECTANGLE 1
                Thickness position = new Thickness() { Right = 0, Bottom = 0 };
                position.Top = (int)rect1.GetValue(Grid.RowProperty) * rect1.ActualHeight;
                position.Left = (int)rect1.GetValue(Grid.ColumnProperty) * rect1.ActualWidth;

                puzzleGrid.Children.Remove(rect1);
                rect1.Margin = position;
                rect1.VerticalAlignment = VerticalAlignment.Top;
                rect1.HorizontalAlignment = HorizontalAlignment.Left;
                rect1.Width = rect1.ActualWidth; rect1.Height = rect1.ActualHeight;
                auxiliaryGrid.Children.Add(rect1);

                //POSITION RECTANLE 2
                position = new Thickness() { Right = 0, Bottom = 0 };
                position.Top = (int)rect2.GetValue(Grid.RowProperty) * rect2.ActualHeight;
                position.Left = (int)rect2.GetValue(Grid.ColumnProperty) * rect2.ActualWidth;

                puzzleGrid.Children.Remove(rect2);
                rect2.Margin = position;
                rect2.VerticalAlignment = VerticalAlignment.Top;
                rect2.HorizontalAlignment = HorizontalAlignment.Left;
                rect2.Width = rect2.ActualWidth; rect2.Height = rect2.ActualHeight;
                auxiliaryGrid.Children.Add(rect2);

                //SLOWLY MOVE THE TWO RECTANGLES
                Task movingAnimation = new Task(new Action(delegate()
                {
                    double rect1Top = rect1.Dispatcher.Invoke(new Func<double>(() => { return rect1.Margin.Top; }));
                    double rect1Left = rect1.Dispatcher.Invoke(new Func<double>(() => { return rect1.Margin.Left; }));

                    while ((y == true ? (m < n ? rect1Top > position.Top : rect1Top < position.Top) : (m < n ? rect1Left > position.Left : rect1Left < position.Left)))
                    {
                        rect1.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                        {
                            Thickness x = new Thickness() { Right = 0, Bottom = 0 };
                            x.Top = (y == true ? (m < n ? rect1.Margin.Top - (rect1.ActualHeight / (nrRows * nrCols) / Math.Max(nrRows, nrCols)) : rect1.Margin.Top + (rect1.ActualHeight / (nrRows * nrCols) / Math.Max(nrRows, nrCols))) : rect1.Margin.Top);
                            x.Left = (y == false ? (m < n ? rect1.Margin.Left - (rect1.ActualWidth / (nrRows * nrCols) / Math.Max(nrRows, nrCols)) : rect1.Margin.Left + (rect1.ActualWidth / (nrRows * nrCols) / Math.Max(nrRows, nrCols))) : rect1.Margin.Left);
                            rect1.Margin = x;

                            rect1Top = x.Top; rect1Left = x.Left;
                        }));
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => { })).Wait();
                        rect2.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                        {
                            Thickness x = new Thickness() { Right = 0, Bottom = 0 };
                            x.Top = (y == true ? (m < n ? rect2.Margin.Top + (rect2.ActualHeight / (nrRows * nrCols) / Math.Max(nrRows, nrCols)) : rect2.Margin.Top - (rect2.ActualHeight / (nrRows * nrCols) / Math.Max(nrRows, nrCols))) : rect2.Margin.Top);
                            x.Left = (y == false ? (m < n ? rect2.Margin.Left + (rect2.ActualWidth / (nrRows * nrCols) / Math.Max(nrRows, nrCols)) : rect2.Margin.Left - (rect2.ActualWidth / (nrRows * nrCols) / Math.Max(nrRows, nrCols))) : rect2.Margin.Left);
                            rect2.Margin = x;
                        }));
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => { })).Wait();
                        if (nrCols * nrRows < 36)
                            Thread.Sleep(10);
                    }
                })); movingAnimation.RunSynchronously(TaskScheduler.Default);

                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => { })).Wait();

                auxiliaryGrid.Children.Clear();
                rect1.VerticalAlignment = rect2.VerticalAlignment = VerticalAlignment.Stretch;
                rect1.HorizontalAlignment = rect2.HorizontalAlignment = HorizontalAlignment.Stretch;
                rect1.Margin = new Thickness(0); rect2.Margin = new Thickness(0);

                rect1.SetValue((y == true ? Grid.RowProperty : Grid.ColumnProperty), m); puzzleGrid.Children.Add(rect1);
                rect2.SetValue((y == true ? Grid.RowProperty : Grid.ColumnProperty), n); puzzleGrid.Children.Add(rect2);

                this.IsEnabled = true;
            }
            private void moveInPlace(Rectangle rectPart)
            {
                Pos p = GetPieceIntendedPosition(rectPart.Name.Substring(1));

                PartReposition((int)p.x, (int)p.y);

                if (rectPart.Parent is StackPanel)
                {
                    parts_listBox.Children.Remove(rectPart);
                    rectPart.Margin = new Thickness(parts_scrollViewer.Margin.Left + 10, parts_scrollViewer.Margin.Top + 10, 0, 0);
                }
                else
                {
                    int x = (int)rectPart.GetValue(Grid.ColumnProperty);
                    int y = (int)rectPart.GetValue(Grid.RowProperty);

                    rectPart.Margin = new Thickness(150 + (400 / nrCols) * x, 10 + (400 / nrRows) * y, 0, 0);
                    puzzleGrid.Children.Remove(rectPart);
                }

                rectPart.VerticalAlignment = VerticalAlignment.Top; rectPart.HorizontalAlignment = HorizontalAlignment.Left;
                mainGrid.Children.Add(rectPart);

                Task t = new Task(new Action(delegate()
                {
                    while (rectPart.Margin.Left < 150 + (400 / nrCols) * p.x || ((int)p.y == 0 ? rectPart.Margin.Top > 10 : rectPart.Margin.Top < 10 + (400 / nrRows) * p.y))
                    {
                        rectPart.Dispatcher.Invoke(new Action(delegate()
                        {
                            Thickness tk = new Thickness() { Right = 0, Bottom = 0 };
                            if (((int)p.y == 0 ? rectPart.Margin.Top > 10 : rectPart.Margin.Top < 10 + (400 / nrRows) * p.y))
                                tk.Top += rectPart.Margin.Top + (rectPart.ActualHeight / (nrRows * nrCols) / Math.Max(nrCols, nrRows)) * ((int)p.y == 0 ? -1 : 1);
                            else
                                tk.Top = ((int)p.y == 0 ? 10 : 10 + (400 / nrRows) * p.y);
                            if (rectPart.Margin.Left < 150 + (400 / nrCols) * p.x)
                                tk.Left += rectPart.Margin.Left + (rectPart.ActualWidth / (nrRows * nrCols) / Math.Max(nrCols, nrRows));
                            else
                                tk.Left = 150 + (400 / nrCols) * p.x;

                            rectPart.Margin = tk;
                        }));

                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => { })).Wait();
                        if (nrRows * nrCols < 36)
                            Thread.Sleep(10);
                    }
                })); t.RunSynchronously(TaskScheduler.Default);
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => { })).Wait();

                mainGrid.Children.Remove(rectPart);
                rectPart.VerticalAlignment = VerticalAlignment.Stretch; rectPart.HorizontalAlignment = HorizontalAlignment.Stretch;
                rectPart.Margin = new Thickness(0);

                rectPart.SetValue(Grid.RowProperty, (int)p.y);
                rectPart.SetValue(Grid.ColumnProperty, (int)p.x);
                puzzleGrid.Children.Add(rectPart);

                Thread.Sleep(25);
                selectedRect.Rect = rectPart;
                while ((rectPart.RenderTransform as RotateTransform).Angle % 360 != 0)
                {
                    rotateRight_image_MouseLeftButtonDown(null, null);
                    Thread.Sleep(25);
                }
                Thread.Sleep(100);
            }

            #region Drag&Drop
                private Point _startPosition;
                private void rectPart_PreviewMouseLeftButtonDown(object sender, MouseEventArgs e)
                {
                    if (gameType_toggler.IsChecked == true)
                        _startPosition = e.GetPosition(null);

                    if (IsCompleted || (sender as Rectangle).Parent is StackPanel)
                        return;

                    if ((sender as Rectangle) == selectedRect.Rect)
                    {
                        selectedRect.Rect.StrokeThickness = 0;
                        selectedRect.Rect = null;
                        return;
                    }
                  
                    Rectangle rectCurrent = sender as Rectangle;
                    if (selectedRect.Rect == null || justRotated)
                    {
                        if (justRotated)
                        {
                            justRotated = false;
                            selectedRect.Rect.StrokeThickness = 0;
                        }
                        selectedRect.Rect = rectCurrent;
                        selectedRect.Rect.StrokeThickness = 1;
                        selectedRect.TileRow = (int)rectCurrent.GetValue(Grid.RowProperty);
                        selectedRect.TileCol = (int)rectCurrent.GetValue(Grid.ColumnProperty);
                        return;
                    }

                    int currentTileRow = (int)rectCurrent.GetValue(Grid.RowProperty);
                    int currentTileCol = (int)rectCurrent.GetValue(Grid.ColumnProperty);

                    if ((currentTileCol == selectedRect.TileCol && Math.Abs(currentTileRow - selectedRect.TileRow) == 1) || (currentTileRow == selectedRect.TileRow && Math.Abs(currentTileCol - selectedRect.TileCol) == 1))
                    {
                        bool y = (currentTileRow == selectedRect.TileRow ? false : true);
                        int aux = (y == true ? (currentTileRow < selectedRect.TileRow ? selectedRect.TileRow - 1 : selectedRect.TileRow + 1) : (currentTileCol < selectedRect.TileCol ? selectedRect.TileCol - 1 : selectedRect.TileCol + 1));

                        switchPlaces(selectedRect.Rect, rectCurrent, aux, (y == true ? selectedRect.TileRow : selectedRect.TileCol), y);
                        selectedRect.Rect.StrokeThickness = 0;

                        selectedRect.Rect = null;

                        CompletionTest();
                    }
                }
                private void rectPart_PreviewMouseMove(object sender, MouseEventArgs e)
                {
                    if (sender != null && e.LeftButton == MouseButtonState.Pressed)
                    {
                        Vector diff = _startPosition - e.GetPosition(null);

                        if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                            DragDrop.DoDragDrop(sender as Rectangle, sender, DragDropEffects.Move);
                    }
                }
                private void rectPart_Drop(object sender, DragEventArgs e)
                {
                    Rectangle rectPart = e.Data.GetData("System.Windows.Shapes.Rectangle") as Rectangle;

                    if (rectPart.Parent is StackPanel && sender is Grid)
                    {
                        Point p = e.GetPosition(sender as Grid);
                        int x = (int)(p.X / (400 / nrCols));
                        int y = (int)(p.Y / (400 / nrRows));
       
                        parts_listBox.Children.Remove(rectPart);

                        if (PartReposition(x, y))
                        {
                            rectPart.SetValue(Grid.ColumnProperty, x);
                            rectPart.SetValue(Grid.RowProperty, y);

                            (sender as Grid).Children.Add(rectPart);

                            if (selectedRect.Rect != null)
                            {
                                selectedRect.Rect.StrokeThickness = 0;
                                selectedRect.Rect = null;
                            }
                        }
                    }
                    else if (rectPart.Parent is Grid && sender is StackPanel)
                    {
                        selectedRect.Rect = null; rectPart.StrokeThickness = 0;

                        puzzleGrid.Children.Remove(rectPart);
                        (sender as StackPanel).Children.Add(rectPart);
                    }
                    else if (rectPart.Parent is Grid && sender is Grid)
                    {
                        Point p = e.GetPosition(sender as Grid);
                        int x = (int)(p.X / (400 / nrCols));
                        int y = (int)(p.Y / (400 / nrRows));

                        if (PartReposition(x, y))
                        {
                            rectPart.SetValue(Grid.ColumnProperty, x);
                            rectPart.SetValue(Grid.RowProperty, y);

                            if (selectedRect.Rect != null)
                            {
                                selectedRect.TileCol = x;
                                selectedRect.TileRow = y;
                            }
                        }
                    }
                }

                private Rectangle CheckForPieceIndependence(int x, int y)
                {
                    Rectangle itemInPosition = puzzleGrid.Children
                                                    .Cast<Rectangle>().Where(i => Grid.GetRow(i) == y && Grid.GetColumn(i) == x)
                                                    .FirstOrDefault();

                    return itemInPosition;
                }
                private bool PartReposition(int x, int y)
                {
                    Rectangle existingRect = CheckForPieceIndependence(x, y);
                    if (existingRect != null)
                    {
                        int aux = x, auy = y;

                        for (int i = 1; i <= 4; i++)
                        {
                            switch (i)
                            {
                                case 1:
                                    {
                                        for (int j = x; j < nrCols; j++)
                                        {
                                            for (int k = auy; k < nrRows; k++)
                                                if (CheckForPieceIndependence(j, k) == null)
                                                { aux = j; auy = k; break; }
                                            if (aux != x || auy != y)
                                                break;
                                        }
                                    } break;
                                case 2:
                                    {
                                        for (int j = x; j < nrCols; j++)
                                        {
                                            for (int k = auy; k >= 0; k--)
                                                if (CheckForPieceIndependence(j, k) == null)
                                                { aux = j; auy = k; break; }
                                            if (aux != x || auy != y)
                                                break;
                                        }
                                    } break;
                                case 3:
                                    {
                                        for (int j = x; j >= 0; j--)
                                        {
                                            for (int k = auy; k < nrRows; k++)
                                                if (CheckForPieceIndependence(j, k) == null)
                                                { aux = j; auy = k; break; }
                                            if (aux != x || auy != y)
                                                break;
                                        }
                                    } break;
                                case 4:
                                    {
                                        for (int j = x; j >= 0; j--)
                                        {
                                            for (int k = auy; k >= 0; k--)
                                                if (CheckForPieceIndependence(j, k) == null)
                                                { aux = j; auy = k; break; }
                                            if (aux != x || auy != y)
                                                break;
                                        }
                                    } break;
                            }
                            if (aux != x || auy != y)
                                break;
                        }

                        if (aux != x || auy != y)
                        {
                            existingRect.SetValue(Grid.ColumnProperty, aux);
                            existingRect.SetValue(Grid.RowProperty, auy);
                            return true;
                        }
                        return false;
                    }

                    return true;
                }
            #endregion

            private void scramblePuzzle_Click(object sender, RoutedEventArgs e) { CreatePuzzleImage(); }
            private void rotateRight_image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
            {
                if (selectedRect.Rect != null)
                {
                    selectedRect.Rect.RenderTransform = new RotateTransform((selectedRect.Rect.RenderTransform as RotateTransform).Angle + 90, 200 / nrCols, 200 / nrRows);
                    CompletionTest(); justRotated = true;
                }
            }
            private void rotateLeft_image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
            {
                if (selectedRect.Rect != null)
                {
                    selectedRect.Rect.RenderTransform = new RotateTransform((selectedRect.Rect.RenderTransform as RotateTransform).Angle - 90, 200 / nrCols, 200 / nrRows);
                    CompletionTest(); justRotated = true;
                }
            }
        #endregion

        #region SESSION
            public class sessionInformations : INotifyPropertyChanged
            {
                private int _score;

                public Guid GUID { get; set; }
                public string User { get; set; }
                public SecureString Pass { get; set; }
                public int Score { get { return _score; } set { _score = value; OnPropertyChanged(new PropertyChangedEventArgs("Score")); } }
                
                #region INotifyPropertyChanged
                    public event PropertyChangedEventHandler PropertyChanged;
                    public void OnPropertyChanged(PropertyChangedEventArgs e)
                    {
                        if (PropertyChanged != null)
                            PropertyChanged(this, e);
                    }
                #endregion
            }

            private void back_pictureButton_MouseEnter(object sender, MouseEventArgs e) { back_pictureButton.Source = new BitmapImage(new Uri("resources/pictures/back_hover.png", UriKind.Relative)); }
            private void back_pictureButton_MouseLeave(object sender, MouseEventArgs e) { back_pictureButton.Source = new BitmapImage(new Uri("resources/pictures/back.png", UriKind.Relative)); }
            private void back_pictureButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
            {
                back_pictureButton.Source = new BitmapImage(new Uri("resources/pictures/back_click.png", UriKind.Relative));
                new MainWindow().Show(); Global.isRunning = true; this.Close();
            }

            private void Window_Closed(object sender, EventArgs e)
            {
                Global.rememberedUsers_dictionary[sessionInfo.User] = new MainWindow.rememberedUser() { GUID = sessionInfo.GUID, HighScore = sessionInfo.Score, Username = sessionInfo.User, Password = Global.rememberedUsers_dictionary[sessionInfo.User].Password };
                Global.rememberedUsers_dictionary.Flush();
                Global.userEngine.Dispose();

                if (!Global.isRunning)
                    Application.Current.Shutdown();
                else
                    Global.isRunning = false;
            }
        #endregion
    }
}