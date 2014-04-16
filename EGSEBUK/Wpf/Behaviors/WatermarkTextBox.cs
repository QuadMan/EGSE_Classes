//-----------------------------------------------------------------------
// <copyright file="WatermarkTextBox.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) MSDN Code Gallery. All rights reserved.
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Piotr Włodek, Коробейщиков Иван</author>
//-----------------------------------------------------------------------

namespace Egse.Wpf.Behaviors
{
    using System;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Расширение для TextBox.
    /// </summary>
    public sealed class WatermarkTextBox
    {
        /// <summary>
        /// The label property.
        /// </summary>
        public static readonly DependencyProperty LabelProperty = DependencyProperty.RegisterAttached("Label", typeof(string), typeof(WatermarkTextBox));

        /// <summary>
        /// The label style property.
        /// </summary>
        public static readonly DependencyProperty LabelStyleProperty = DependencyProperty.RegisterAttached("LabelStyle", typeof(Style), typeof(WatermarkTextBox));

        /// <summary>
        /// The enable watermark property.
        /// </summary>
        public static readonly DependencyProperty EnableWatermarkProperty = DependencyProperty.RegisterAttached("EnableWatermark", typeof(bool), typeof(WatermarkTextBox), new UIPropertyMetadata(false, OnEnableWatermarkChanged));
                
        /// <summary>
        /// The watermark text box behavior property.
        /// </summary>
        private static readonly DependencyProperty WatermarkTextBoxBehaviorProperty = DependencyProperty.RegisterAttached("WatermarkTextBoxBehavior", typeof(WatermarkTextBox), typeof(WatermarkTextBox), new UIPropertyMetadata(null));

        /// <summary>
        /// The has text property key.
        /// </summary>
        private static readonly DependencyPropertyKey HasTextPropertyKey = DependencyProperty.RegisterAttachedReadOnly("HasText", typeof(bool), typeof(WatermarkTextBox), new UIPropertyMetadata(false));
       
        /// <summary>
        /// The has text property.
        /// </summary>
        private static readonly DependencyProperty HasTextProperty = HasTextPropertyKey.DependencyProperty;

        /// <summary>
        /// The text box.
        /// </summary>
        private readonly TextBox textBox;

        /// <summary>
        /// The UI element adorner.
        /// </summary>
        private UIElementAdorner uiElement;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="WatermarkTextBox" />.
        /// </summary>
        /// <param name="textBox">The text box.</param>
        /// <exception cref="System.ArgumentNullException">Если аргументом задан null.</exception>
        private WatermarkTextBox(TextBox textBox)
        {
            if (null == textBox)
            {
                throw new ArgumentNullException("textBox");
            }

            this.textBox = textBox;
        }

        /// <summary>
        /// Gets the enable watermark.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns><c>true</c> if enabled watermark.</returns>
        public static bool GetEnableWatermark(TextBox obj)
        {
            return (bool)obj.GetValue(EnableWatermarkProperty);
        }

        /// <summary>
        /// Sets the enable watermark.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="value">if set to <c>true</c> [value].</param>
        public static void SetEnableWatermark(TextBox obj, bool value)
        {
            obj.SetValue(EnableWatermarkProperty, value);
        }

        /// <summary>
        /// Gets the label.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>The label content.</returns>
        public static string GetLabel(TextBox obj)
        {
            return (string)obj.GetValue(LabelProperty);
        }

        /// <summary>
        /// Gets the has text.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns><c>true</c> if textbox has text.</returns>
        public static bool GetHasText(TextBox obj)
        {
            return (bool)obj.GetValue(HasTextProperty);
        }

        /// <summary>
        /// Gets the label style.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>The label style.</returns>
        public static Style GetLabelStyle(TextBox obj)
        {
            return (Style)obj.GetValue(LabelStyleProperty);
        }

        /// <summary>
        /// Sets the label style.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="value">The value.</param>
        public static void SetLabelStyle(TextBox obj, Style value)
        {
            obj.SetValue(LabelStyleProperty, value);
        }

        /// <summary>
        /// Sets the label.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="value">The value.</param>
        public static void SetLabel(TextBox obj, string value)
        {
            obj.SetValue(LabelProperty, value);
        }

        /// <summary>
        /// Sets the has text.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="value">if set to <c>true</c> [value].</param>
        private static void SetHasText(TextBox obj, bool value)
        {
            obj.SetValue(HasTextPropertyKey, value);
        }

        /// <summary>
        /// Gets the watermark text box behavior.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>The watermark TextBox behavior.</returns>
        private static WatermarkTextBox GetWatermarkTextBoxBehavior(DependencyObject obj)
        {
            return (WatermarkTextBox)obj.GetValue(WatermarkTextBoxBehaviorProperty);
        }

        /// <summary>
        /// Sets the watermark text box behavior.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="value">The value.</param>
        private static void SetWatermarkTextBoxBehavior(DependencyObject obj, WatermarkTextBox value)
        {
            obj.SetValue(WatermarkTextBoxBehaviorProperty, value);
        }

        /// <summary>
        /// Called when [enable watermark changed].
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnEnableWatermarkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (null != e.OldValue)
            {
                bool enabled = (bool)e.OldValue;

                if (enabled)
                {
                    TextBox textBox = (TextBox)d;
                    WatermarkTextBox behavior = GetWatermarkTextBoxBehavior(textBox);
                    behavior.Detach();

                    SetWatermarkTextBoxBehavior(textBox, null);
                }
            }

            if (null != e.NewValue)
            {
                bool enabled = (bool)e.NewValue;

                if (enabled)
                {
                    TextBox textBox = (TextBox)d;
                    WatermarkTextBox behavior = new WatermarkTextBox(textBox);
                    behavior.Attach();

                    SetWatermarkTextBoxBehavior(textBox, behavior);
                }
            }
        }

        /// <summary>
        /// Attaches this instance.
        /// </summary>
        private void Attach()
        {
            this.textBox.Loaded += this.TextBoxLoaded;
            this.textBox.TextChanged += this.TextBoxTextChanged;
            this.textBox.DragEnter += this.TextBoxDragEnter;
            this.textBox.DragLeave += this.TextBoxDragLeave;
        }

        /// <summary>
        /// Detaches this instance.
        /// </summary>
        private void Detach()
        {
            this.textBox.Loaded -= this.TextBoxLoaded;
            this.textBox.TextChanged -= this.TextBoxTextChanged;
            this.textBox.DragEnter -= this.TextBoxDragEnter;
            this.textBox.DragLeave -= this.TextBoxDragLeave;
        }

        /// <summary>
        /// Texts the box drag leave.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void TextBoxDragLeave(object sender, DragEventArgs e)
        {
            this.UpdateAdorner();
        }

        /// <summary>
        /// Texts the box drag enter.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void TextBoxDragEnter(object sender, DragEventArgs e)
        {
            this.textBox.TryRemoveAdorners<UIElementAdorner>();
        }

        /// <summary>
        /// Texts the box text changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
        private void TextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            bool hasText = !string.IsNullOrEmpty(this.textBox.Text);
            SetHasText(this.textBox, hasText);
        }

        /// <summary>
        /// Texts the box loaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void TextBoxLoaded(object sender, RoutedEventArgs e)
        {
            this.Init();
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        private void Init()
        {
            this.uiElement = new UIElementAdorner(this.textBox, GetLabel(this.textBox), GetLabelStyle(this.textBox));
            this.UpdateAdorner();

            DependencyPropertyDescriptor focusProp = DependencyPropertyDescriptor.FromProperty(UIElement.IsFocusedProperty, typeof(FrameworkElement));
            if (null != focusProp)
            {
                focusProp.AddValueChanged(this.textBox, (sender, args) => this.UpdateAdorner());
            }

            DependencyPropertyDescriptor containsTextProp = DependencyPropertyDescriptor.FromProperty(HasTextProperty, typeof(TextBox));
            if (null != containsTextProp)
            {
                containsTextProp.AddValueChanged(this.textBox, (sender, args) => this.UpdateAdorner());
            }
        }

        /// <summary>
        /// Обновляет графический элемент.
        /// </summary>
        private void UpdateAdorner()
        {
            if (GetHasText(this.textBox) || (this.textBox.IsFocused && !this.textBox.IsReadOnly))
            {
                // Hide the Watermark Label if the adorner layer is visible
                this.textBox.ToolTip = GetLabel(this.textBox);
                this.textBox.TryRemoveAdorners<UIElementAdorner>();
            }
            else
            {
                // Show the Watermark Label if the adorner layer is visible
                this.textBox.ToolTip = null;
                this.textBox.TryAddAdorner<UIElementAdorner>(this.uiElement);
            }
        }
    }
}