//-----------------------------------------------------------------------
// <copyright file="EgseBehaviorsWatermarkTextBox.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) MSDN Code Gallery. All rights reserved.
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Piotr Włodek, Коробейщиков Иван</author>
//-----------------------------------------------------------------------

namespace Egse.Behaviors
{
    using System;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Расширение для TextBox.
    /// </summary>
    public sealed class WatermarkTextBoxBehavior
    {
        /// <summary>
        /// The label property.
        /// </summary>
        public static readonly DependencyProperty LabelProperty = DependencyProperty.RegisterAttached("Label", typeof(string), typeof(WatermarkTextBoxBehavior));

        /// <summary>
        /// The label style property.
        /// </summary>
        public static readonly DependencyProperty LabelStyleProperty = DependencyProperty.RegisterAttached("LabelStyle", typeof(Style), typeof(WatermarkTextBoxBehavior));

        /// <summary>
        /// The enable watermark property.
        /// </summary>
        public static readonly DependencyProperty EnableWatermarkProperty = DependencyProperty.RegisterAttached("EnableWatermark", typeof(bool), typeof(WatermarkTextBoxBehavior), new UIPropertyMetadata(false, OnEnableWatermarkChanged));
                
        /// <summary>
        /// The watermark text box behavior property.
        /// </summary>
        private static readonly DependencyProperty WatermarkTextBoxBehaviorProperty = DependencyProperty.RegisterAttached("WatermarkTextBoxBehavior", typeof(WatermarkTextBoxBehavior), typeof(WatermarkTextBoxBehavior), new UIPropertyMetadata(null));

        /// <summary>
        /// The has text property key.
        /// </summary>
        private static readonly DependencyPropertyKey HasTextPropertyKey = DependencyProperty.RegisterAttachedReadOnly("HasText", typeof(bool), typeof(WatermarkTextBoxBehavior), new UIPropertyMetadata(false));
       
        /// <summary>
        /// The has text property.
        /// </summary>
        private static readonly DependencyProperty HasTextProperty = HasTextPropertyKey.DependencyProperty;

        /// <summary>
        /// The text box.
        /// </summary>
        private readonly TextBox _textBox;

        /// <summary>
        /// The UI element adorner.
        /// </summary>
        private UIElementAdorner _uiElementAdorner;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="WatermarkTextBoxBehavior" />.
        /// </summary>
        /// <param name="textBox">The text box.</param>
        /// <exception cref="System.ArgumentNullException">Если аргументом задан null.</exception>
        private WatermarkTextBoxBehavior(TextBox textBox)
        {
            if (null == textBox)
            {
                throw new ArgumentNullException("textBox");
            }

            _textBox = textBox;
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
        private static WatermarkTextBoxBehavior GetWatermarkTextBoxBehavior(DependencyObject obj)
        {
            return (WatermarkTextBoxBehavior)obj.GetValue(WatermarkTextBoxBehaviorProperty);
        }

        /// <summary>
        /// Sets the watermark text box behavior.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="value">The value.</param>
        private static void SetWatermarkTextBoxBehavior(DependencyObject obj, WatermarkTextBoxBehavior value)
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
                var enabled = (bool)e.OldValue;

                if (enabled)
                {
                    var textBox = (TextBox)d;
                    var behavior = GetWatermarkTextBoxBehavior(textBox);
                    behavior.Detach();

                    SetWatermarkTextBoxBehavior(textBox, null);
                }
            }

            if (e.NewValue != null)
            {
                var enabled = (bool)e.NewValue;

                if (enabled)
                {
                    var textBox = (TextBox)d;
                    var behavior = new WatermarkTextBoxBehavior(textBox);
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
            _textBox.Loaded += TextBoxLoaded;
            _textBox.TextChanged += TextBoxTextChanged;
            _textBox.DragEnter += TextBoxDragEnter;
            _textBox.DragLeave += TextBoxDragLeave;
        }

        /// <summary>
        /// Detaches this instance.
        /// </summary>
        private void Detach()
        {
            _textBox.Loaded -= TextBoxLoaded;
            _textBox.TextChanged -= TextBoxTextChanged;
            _textBox.DragEnter -= TextBoxDragEnter;
            _textBox.DragLeave -= TextBoxDragLeave;
        }

        /// <summary>
        /// Texts the box drag leave.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void TextBoxDragLeave(object sender, DragEventArgs e)
        {
            UpdateAdorner();
        }

        /// <summary>
        /// Texts the box drag enter.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void TextBoxDragEnter(object sender, DragEventArgs e)
        {
            _textBox.TryRemoveAdorners<UIElementAdorner>();
        }

        /// <summary>
        /// Texts the box text changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
        private void TextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            var hasText = !string.IsNullOrEmpty(_textBox.Text);
            SetHasText(_textBox, hasText);
        }

        /// <summary>
        /// Texts the box loaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void TextBoxLoaded(object sender, RoutedEventArgs e)
        {
            Init();
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        private void Init()
        {
            _uiElementAdorner = new UIElementAdorner(_textBox, GetLabel(_textBox), GetLabelStyle(_textBox));
            UpdateAdorner();

            DependencyPropertyDescriptor focusProp = DependencyPropertyDescriptor.FromProperty(UIElement.IsFocusedProperty, typeof(FrameworkElement));
            if (null != focusProp)
            {
                focusProp.AddValueChanged(_textBox, (sender, args) => UpdateAdorner());
            }

            DependencyPropertyDescriptor containsTextProp = DependencyPropertyDescriptor.FromProperty(HasTextProperty, typeof(TextBox));
            if (null != containsTextProp)
            {
                containsTextProp.AddValueChanged(_textBox, (sender, args) => UpdateAdorner());
            }
        }

        /// <summary>
        /// Updates the adorner.
        /// </summary>
        private void UpdateAdorner()
        {
            if (GetHasText(_textBox) || (_textBox.IsFocused && !_textBox.IsReadOnly))
            {
                // Hide the Watermark Label if the adorner layer is visible
                _textBox.ToolTip = GetLabel(_textBox);
                _textBox.TryRemoveAdorners<UIElementAdorner>();
            }
            else
            {
                // Show the Watermark Label if the adorner layer is visible
                _textBox.ToolTip = null;
                _textBox.TryAddAdorner<UIElementAdorner>(_uiElementAdorner);
            }
        }
    }
}