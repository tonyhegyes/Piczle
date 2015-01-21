using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Collections.Generic;
using System.Windows.Controls.Primitives;


namespace puzzle.resources
{
    public static class WatermarkService
    {
        public static readonly DependencyProperty WatermarkProperty = DependencyProperty.RegisterAttached("Watermark", typeof(object), typeof(WatermarkService));
        private static readonly Dictionary<object, ItemsControl> itemsControls = new Dictionary<object, ItemsControl>();
        //
        public static void SetWatermark(DependencyObject d, object value)
        {
            if (d is ItemsControl && !(d is ComboBox))
            {
                ItemsControl i = (ItemsControl)d;
                // for Items property  
                i.ItemContainerGenerator.ItemsChanged -= ItemsChanged;
                itemsControls.Remove(i.ItemContainerGenerator);
                // for ItemsSource property  
                DependencyPropertyDescriptor prop = DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, i.GetType());
                prop.RemoveValueChanged(i, ItemsSourceChanged);
            }
            else if (d is DatePicker) { ((DatePicker)d).GotKeyboardFocus -= Control_GotFocus; ((DatePicker)d).LostKeyboardFocus -= Control_LostFocus; }
            else { ((Control)d).GotFocus -= Control_GotFocus; ((Control)d).LostFocus -= Control_LostFocus; }

            RemoveWatermark(d as UIElement);
            d.SetValue(WatermarkProperty, value);
            if (ShouldShowWatermark(d as Control))
                ShowWatermark(d as Control);

            if (d is ItemsControl && !(d is ComboBox))
            {
                ItemsControl i = (ItemsControl)d;
                // for Items property  
                i.ItemContainerGenerator.ItemsChanged += ItemsChanged;
                itemsControls.Add(i.ItemContainerGenerator, i);
                // for ItemsSource property  
                DependencyPropertyDescriptor prop = DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, i.GetType());
                prop.AddValueChanged(i, ItemsSourceChanged);
            }
            else if (d is DatePicker) { ((DatePicker)d).GotKeyboardFocus += Control_GotFocus; ((DatePicker)d).LostKeyboardFocus += Control_LostFocus; }
            else { ((Control)d).GotFocus += Control_GotFocus; ((Control)d).LostFocus += Control_LostFocus; }
        }
        public static object GetWatermark(DependencyObject d) { return (object)d.GetValue(WatermarkProperty); }

        #region Event Handlers
        private static void Control_GotFocus(object sender, RoutedEventArgs e) { if (ShouldShowWatermark(sender as Control)) { RemoveWatermark(sender as Control); } }
        private static void Control_LostFocus(object sender, RoutedEventArgs e) { if (ShouldShowWatermark(sender as Control)) { ShowWatermark(sender as Control); } else { RemoveWatermark(sender as Control); } }
        //
        private static void ItemsSourceChanged(object sender, EventArgs e)
        {
            ItemsControl c = sender as ItemsControl;
            if (c.ItemsSource == null || ShouldShowWatermark(c))
                ShowWatermark(c);
            else
                RemoveWatermark(c);
        }
        private static void ItemsChanged(object sender, ItemsChangedEventArgs e)
        {
            ItemsControl control;
            if (itemsControls.TryGetValue(sender, out control))
                if (ShouldShowWatermark(control))
                    ShowWatermark(control);
                else
                    RemoveWatermark(control);
        }
        #endregion

        #region Helper Methods
        public static void RemoveWatermark(UIElement control)
        {
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(control);
            if (layer != null) // layer could be null if control is no longer in the visual tree
            {
                Adorner[] adorners = layer.GetAdorners(control);
                if (adorners == null) return;

                foreach (Adorner adorner in adorners)
                    if (adorner is WatermarkAdorner)
                    { adorner.Visibility = Visibility.Hidden; layer.Remove(adorner); }
            }
        }
        private static void ShowWatermark(Control control)
        {
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(control);
            if (layer != null) { layer.Add(new WatermarkAdorner(control, GetWatermark(control))); } // layer could be null if control is no longer in the visual tree
        }
        private static bool ShouldShowWatermark(Control c)
        {
            if (c is ComboBox) { return (c as ComboBox).Text == String.Empty; }
            else if (c is PasswordBox) { return (c as PasswordBox).Password == string.Empty; }
            else if (c is DatePicker) { return (c as DatePicker).Text == string.Empty; }
            else if (c is ItemsControl) { return (c as ItemsControl).Items.Count == 0; }
            else if (c is TextBoxBase)
            {
                try { return (c as TextBox).Text == string.Empty; }
                catch
                {
                    return (c as RichTextBox).CaretPosition.GetTextInRun(LogicalDirection.Backward) == string.Empty
                            && (from Paragraph p in (c as RichTextBox).Document.Blocks select p.Inlines.Count).FirstOrDefault(nr => { return nr != 0; }) == 0;
                }
            }
            else { return false; }
        }
        #endregion
    }

    internal sealed class WatermarkAdorner : Adorner
    {
        private readonly ContentPresenter contentPresenter;
        protected override int VisualChildrenCount { get { return 1; } }
        private Control Control { get { return (Control)this.AdornedElement; } }

        public WatermarkAdorner(UIElement adornedElement, object watermark)
            : base(adornedElement)
        {
            IsHitTestVisible = false;
            SetBinding(VisibilityProperty, new Binding("IsVisible") { Source = adornedElement, Converter = new BooleanToVisibilityConverter() });         //Hide the control adorner when the adorned element is hidden
            contentPresenter = new ContentPresenter() { Content = watermark, Opacity = 0.5, Margin = new Thickness(Control.Margin.Left, Control.Margin.Top, 0, 0), HorizontalAlignment = (Control is DatePicker ? HorizontalAlignment.Left : HorizontalAlignment.Center), VerticalAlignment = VerticalAlignment.Center };
        }

        protected override Visual GetVisualChild(int index) { return this.contentPresenter; }
        protected override Size MeasureOverride(Size constraint)
        {
            // Here's the secret to getting the adorner to cover the whole control
            this.contentPresenter.Measure(Control.RenderSize);
            return Control.RenderSize;
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            this.contentPresenter.Arrange(new Rect(finalSize));
            return finalSize;
        }
    }
}
