using System;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace puzzle
{
    public partial class highScores : Window
    {
        const double gap = 25.0; // pixel gap between each TextBlock
        const int timer_interval = 16; // number of ms between timer ticks. 16 is near 1/60th second, for smoother updates on LCD displays
        const double move_amount = 2.5; // number of pixels to move each timer tick. 1 - 1.5 is ideal, any higher will introduce judders

        private List<LinkedList<TextBlock>> textBlocks = new List<LinkedList<TextBlock>>();
        private Timer timer = new Timer();
        
        public highScores()
        {
            InitializeComponent();

            //SET LANGUAGE
            this.Title = "Piczle - " + Global.LanguageAgent.LanguageDataSet.Tables["highScores"].Select("UIElement_name = 'page_title'")[0][1];
            welcome_label.Content = Global.LanguageAgent.LanguageDataSet.Tables["highScores"].Select("UIElement_name = 'welcome_label'")[0][1];

            //GET HIGH SCORES
            var topPlayers = from player in Global.rememberedUsers_dictionary.Values
                             orderby player.HighScore descending
                             select player;

            for (int i = 0; i < 5; i++)
            {
                textBlocks.Add(new LinkedList<TextBlock>());
                switch (i)
                {
                    case 0: AddTextBlock("-----------", top1); break;
                    case 1: AddTextBlock("-----------", top2); break;
                    case 2: AddTextBlock("-----------", top3); break;
                    case 3: AddTextBlock("-----------", top4); break;
                    case 4: AddTextBlock("-----------", top5); break;
                }
            }

            int x = 0;
            foreach (MainWindow.rememberedUser u in topPlayers)
                switch (++x)
                {
                    case 1: textBlocks[0] = new LinkedList<TextBlock>(); top1.Children.Clear(); AddTextBlock(u.Username + " " + u.HighScore, top1); break;
                    case 2: textBlocks[1] = new LinkedList<TextBlock>(); top2.Children.Clear(); AddTextBlock(u.Username + " " + u.HighScore, top2); break;
                    case 3: textBlocks[2] = new LinkedList<TextBlock>(); top3.Children.Clear(); AddTextBlock(u.Username + " " + u.HighScore, top3); break;
                    case 4: textBlocks[3] = new LinkedList<TextBlock>(); top4.Children.Clear(); AddTextBlock(u.Username + " " + u.HighScore, top4); break;
                    case 5: textBlocks[4] = new LinkedList<TextBlock>(); top5.Children.Clear(); AddTextBlock(u.Username + " " + u.HighScore, top5); break;
                }

            top1.Dispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(delegate(Object state)
            {
                var node = textBlocks[0].First;

                while (node != null)
                {
                    if (node.Previous != null)
                        Canvas.SetLeft(node.Value, Canvas.GetLeft(node.Previous.Value) + node.Previous.Value.ActualWidth + gap);
                    else
                        Canvas.SetLeft(node.Value, top1.Width + gap);

                    node = node.Next;
                }

                return null;
            }), null);
            top2.Dispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(delegate(Object state)
            {
                var node = textBlocks[1].First;

                while (node != null)
                {
                    if (node.Previous != null)
                        Canvas.SetLeft(node.Value, Canvas.GetLeft(node.Previous.Value) + node.Previous.Value.ActualWidth + gap);
                    else
                        Canvas.SetLeft(node.Value, top2.Width + gap);

                    node = node.Next;
                }

                return null;
            }), null);
            top3.Dispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(delegate(Object state)
            {
                var node = textBlocks[2].First;

                while (node != null)
                {
                    if (node.Previous != null)
                        Canvas.SetLeft(node.Value, Canvas.GetLeft(node.Previous.Value) + node.Previous.Value.ActualWidth + gap);
                    else
                        Canvas.SetLeft(node.Value, top3.Width + gap);

                    node = node.Next;
                }

                return null;
            }), null);
            top4.Dispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(delegate(Object state)
            {
                var node = textBlocks[3].First;

                while (node != null)
                {
                    if (node.Previous != null)
                        Canvas.SetLeft(node.Value, Canvas.GetLeft(node.Previous.Value) + node.Previous.Value.ActualWidth + gap);
                    else
                        Canvas.SetLeft(node.Value, top4.Width + gap);

                    node = node.Next;
                }

                return null;
            }), null);
            top5.Dispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(delegate(Object state)
            {
                var node = textBlocks[4].First;

                while (node != null)
                {
                    if (node.Previous != null)
                        Canvas.SetLeft(node.Value, Canvas.GetLeft(node.Previous.Value) + node.Previous.Value.ActualWidth + gap);
                    else
                        Canvas.SetLeft(node.Value, top5.Width + gap);

                    node = node.Next;
                }

                return null;
            }), null);
         
            timer.Interval = timer_interval;
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            timer.Start();
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            top1.Dispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(delegate(Object state)
            {
                var node = textBlocks[0].First;
                var lastNode = textBlocks[0].Last;

                while (node != null)
                {
                    double newLeft = Canvas.GetLeft(node.Value) - move_amount;

                    if (newLeft < (0 - (node.Value.ActualWidth + gap)))
                    {
                        textBlocks[0].Remove(node);

                        var lastNodeLeftPos = Canvas.GetLeft(lastNode.Value);

                        textBlocks[0].AddLast(node);

                        if ((lastNodeLeftPos + lastNode.Value.ActualWidth + gap) > top1.Width) // Last element is offscreen
                            newLeft = lastNodeLeftPos + lastNode.Value.ActualWidth + gap;
                        else
                            newLeft = top1.Width + gap;
                    }

                    Canvas.SetLeft(node.Value, newLeft);

                    node = node == lastNode ? null : node.Next;
                }

                return null;
            }), null);
            top2.Dispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(delegate(Object state)
            {
                var node = textBlocks[1].First;
                var lastNode = textBlocks[1].Last;

                while (node != null)
                {
                    double newLeft = Canvas.GetLeft(node.Value) - move_amount;

                    if (newLeft < (0 - (node.Value.ActualWidth + gap)))
                    {
                        textBlocks[1].Remove(node);

                        var lastNodeLeftPos = Canvas.GetLeft(lastNode.Value);

                        textBlocks[1].AddLast(node);

                        if ((lastNodeLeftPos + lastNode.Value.ActualWidth + gap) > top2.Width) // Last element is offscreen
                            newLeft = lastNodeLeftPos + lastNode.Value.ActualWidth + gap;
                        else
                            newLeft = top2.Width + gap;
                    }

                    Canvas.SetLeft(node.Value, newLeft);

                    node = node == lastNode ? null : node.Next;
                }

                return null;
            }), null);
            top3.Dispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(delegate(Object state)
            {
                var node = textBlocks[2].First;
                var lastNode = textBlocks[2].Last;

                while (node != null)
                {
                    double newLeft = Canvas.GetLeft(node.Value) - move_amount;

                    if (newLeft < (0 - (node.Value.ActualWidth + gap)))
                    {
                        textBlocks[2].Remove(node);

                        var lastNodeLeftPos = Canvas.GetLeft(lastNode.Value);

                        textBlocks[2].AddLast(node);

                        if ((lastNodeLeftPos + lastNode.Value.ActualWidth + gap) > top3.Width) // Last element is offscreen
                            newLeft = lastNodeLeftPos + lastNode.Value.ActualWidth + gap;
                        else
                            newLeft = top3.Width + gap;
                    }

                    Canvas.SetLeft(node.Value, newLeft);

                    node = node == lastNode ? null : node.Next;
                }

                return null;
            }), null);
            top4.Dispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(delegate(Object state)
            {
                var node = textBlocks[3].First;
                var lastNode = textBlocks[3].Last;

                while (node != null)
                {
                    double newLeft = Canvas.GetLeft(node.Value) - move_amount;

                    if (newLeft < (0 - (node.Value.ActualWidth + gap)))
                    {
                        textBlocks[3].Remove(node);

                        var lastNodeLeftPos = Canvas.GetLeft(lastNode.Value);

                        textBlocks[3].AddLast(node);

                        if ((lastNodeLeftPos + lastNode.Value.ActualWidth + gap) > top4.Width) // Last element is offscreen
                            newLeft = lastNodeLeftPos + lastNode.Value.ActualWidth + gap;
                        else
                            newLeft = top4.Width + gap;
                    }

                    Canvas.SetLeft(node.Value, newLeft);

                    node = node == lastNode ? null : node.Next;
                }

                return null;
            }), null);
            top5.Dispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(delegate(Object state)
            {
                var node = textBlocks[4].First;
                var lastNode = textBlocks[4].Last;

                while (node != null)
                {
                    double newLeft = Canvas.GetLeft(node.Value) - move_amount;

                    if (newLeft < (0 - (node.Value.ActualWidth + gap)))
                    {
                        textBlocks[4].Remove(node);

                        var lastNodeLeftPos = Canvas.GetLeft(lastNode.Value);

                        textBlocks[4].AddLast(node);

                        if ((lastNodeLeftPos + lastNode.Value.ActualWidth + gap) > top5.Width) // Last element is offscreen
                            newLeft = lastNodeLeftPos + lastNode.Value.ActualWidth + gap;
                        else
                            newLeft = top5.Width + gap;
                    }

                    Canvas.SetLeft(node.Value, newLeft);

                    node = node == lastNode ? null : node.Next;
                }

                return null;
            }), null);
        }
        void AddTextBlock(string Text, Canvas canvas)
        {
            TextBlock tb = new TextBlock() { Text = Text, FontSize = 20, FontWeight = FontWeights.ExtraBold, Foreground = Brushes.Black };
            
            canvas.Children.Add(tb);
            textBlocks[Convert.ToInt32(canvas.Name[3]) - 49].AddLast(tb);
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

    public partial class highScores : IDisposable
    {
        bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~highScores() { Dispose(false); }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
                timer.Dispose();
                    
            _disposed = true;
        }
    }
}

