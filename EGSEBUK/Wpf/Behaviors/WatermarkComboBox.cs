//-----------------------------------------------------------------------
// <copyright file="WatermarkComboBox.cs" company="IKI RSSI, laboratory №711">
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
    using System.Windows.Controls.Primitives;

    /// <summary>
    /// Расширения для comboBox.
    /// </summary>
    public sealed class WatermarkComboBox
    {
        /// <summary>
        /// The enable watermark property.
        /// </summary>
        public static readonly DependencyProperty EnableWatermarkProperty = DependencyProperty.RegisterAttached("EnableWatermark", typeof(bool), typeof(WatermarkComboBox), new UIPropertyMetadata(false, OnEnableWatermarkChanged));
      
        /// <summary>
        /// The label style property.
        /// </summary>
        public static readonly DependencyProperty LabelStyleProperty = DependencyProperty.RegisterAttached("LabelStyle", typeof(Style), typeof(WatermarkComboBox));

        /// <summary>
        /// The label property.
        /// </summary>
        public static readonly DependencyProperty LabelProperty = DependencyProperty.RegisterAttached("Label", typeof(string), typeof(WatermarkComboBox));

        /// <summary>
        /// The watermark ComboBox behavior property.
        /// </summary>
        private static readonly DependencyProperty WatermarkComboBoxBehaviorProperty = DependencyProperty.RegisterAttached("WatermarkComboBoxBehavior", typeof(WatermarkComboBox), typeof(WatermarkComboBox), new UIPropertyMetadata(null));
    
        /// <summary>
        /// The ComboBox.
        /// </summary>
        private readonly ComboBox comboBox;

        /// <summary>
        /// The UI element adorner.
        /// </summary>
        private UIElementAdorner uiElement;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="WatermarkComboBox" />.
        /// </summary>
        /// <param name="comboBox">The combo box.</param>
        /// <exception cref="System.ArgumentNullException">Если аргументом задан null.</exception>
        private WatermarkComboBox(ComboBox comboBox)
        {
            if (null == comboBox)
            {
                throw new ArgumentNullException("comboBox");
            }

            this.comboBox = comboBox;
        }

        /// <summary>
        /// Gets the label.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>The label content.</returns>
        public static string GetLabel(ComboBox obj)
        {
            return (string)obj.GetValue(LabelProperty);
        }

        /// <summary>
        /// Sets the label.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="value">The value.</param>
        public static void SetLabel(ComboBox obj, string value)
        {
            obj.SetValue(LabelProperty, value);
        }

        /// <summary>
        /// Sets the label style.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="value">The value.</param>
        public static void SetLabelStyle(ComboBox obj, Style value)
        {
            obj.SetValue(LabelStyleProperty, value);
        }

        /// <summary>
        /// Gets the label style.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>The label style.</returns>
        public static Style GetLabelStyle(ComboBox obj)
        {
            return (Style)obj.GetValue(LabelStyleProperty);
        }

        /// <summary>
        /// Gets the enable watermark.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns><c>true</c> if enabled watermark.</returns>
        public static bool GetEnableWatermark(ComboBox obj)
        {
            return (bool)obj.GetValue(EnableWatermarkProperty);
        }

        /// <summary>
        /// Sets the enable watermark.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="value">if set to <c>true</c> [value].</param>
        public static void SetEnableWatermark(ComboBox obj, bool value)
        {
            obj.SetValue(EnableWatermarkProperty, value);
        }

        /// <summary>
        /// Gets the watermark ComboBox behavior.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>The watermark ComboBox behavior.</returns>
        private static WatermarkComboBox GetWatermarkComboBoxBehavior(DependencyObject obj)
        {
            return (WatermarkComboBox)obj.GetValue(WatermarkComboBoxBehaviorProperty);
        }

        /// <summary>
        /// Sets the watermark ComboBox behavior.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="value">The value.</param>
        private static void SetWatermarkComboBoxBehavior(DependencyObject obj, WatermarkComboBox value)
        {
            obj.SetValue(WatermarkComboBoxBehaviorProperty, value);
        }

        /// <summary>
        /// Called when [enable watermark changed].
        /// </summary>
        /// <param name="d">The dependency object.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnEnableWatermarkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (null != e.OldValue)
            {
                bool enabled = (bool)e.OldValue;

                if (enabled)
                {
                    ComboBox comboBox = (ComboBox)d;
                    WatermarkComboBox behavior = GetWatermarkComboBoxBehavior(comboBox);
                    behavior.Detach();

                    SetWatermarkComboBoxBehavior(comboBox, null);
                }
            }

            if (null != e.NewValue)
            {
                bool enabled = (bool)e.NewValue;

                if (enabled)
                {
                    ComboBox comboBox = (ComboBox)d;
                    WatermarkComboBox behavior = new WatermarkComboBox(comboBox);
                    behavior.Attach();

                    SetWatermarkComboBoxBehavior(comboBox, behavior);
                }
            }
        }

        /// <summary>
        /// Attaches this instance.
        /// </summary>
        private void Attach()
        {
            this.comboBox.Loaded += this.ComboBoxLoaded;
            this.comboBox.DragEnter += this.ComboBoxDragEnter;
            this.comboBox.DragLeave += this.ComboBoxDragLeave;
        }

        /// <summary>
        /// Detaches this instance.
        /// </summary>
        private void Detach()
        {
            this.comboBox.Loaded -= this.ComboBoxLoaded;
            this.comboBox.DragEnter -= this.ComboBoxDragEnter;
            this.comboBox.DragLeave -= this.ComboBoxDragLeave;
        }

        /// <summary>
        /// ComboBoxes the drag leave.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void ComboBoxDragLeave(object sender, DragEventArgs e)
        {
            this.UpdateAdorner();
        }

        /// <summary>
        /// ComboBoxes the drag enter.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void ComboBoxDragEnter(object sender, DragEventArgs e)
        {
            this.comboBox.TryRemoveAdorners<UIElementAdorner>();
        }

        /// <summary>
        /// ComboBoxes the loaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ComboBoxLoaded(object sender, RoutedEventArgs e)
        {
            this.Init();
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        private void Init()
        {
            this.uiElement = new UIElementAdorner(this.comboBox, GetLabel(this.comboBox), GetLabelStyle(this.comboBox));
            this.UpdateAdorner();

            DependencyPropertyDescriptor focusProp = DependencyPropertyDescriptor.FromProperty(UIElement.IsFocusedProperty, typeof(ComboBox));
            if (null != focusProp)
            {
                focusProp.AddValueChanged(this.comboBox, (sender, args) => this.UpdateAdorner());
            }

            DependencyPropertyDescriptor focusKeyboardProp = DependencyPropertyDescriptor.FromProperty(UIElement.IsKeyboardFocusedProperty, typeof(ComboBox));
            if (null != focusKeyboardProp)
            {
                focusKeyboardProp.AddValueChanged(this.comboBox, (sender, args) => this.UpdateAdorner());
            }

            DependencyPropertyDescriptor focusKeyboardWithinProp = DependencyPropertyDescriptor.FromProperty(UIElement.IsKeyboardFocusWithinProperty, typeof(ComboBox));
            if (null != focusKeyboardWithinProp)
            {
                focusKeyboardWithinProp.AddValueChanged(this.comboBox, (sender, args) => this.UpdateAdorner());
            }

            DependencyPropertyDescriptor textProp = DependencyPropertyDescriptor.FromProperty(ComboBox.TextProperty, typeof(ComboBox));
            if (null != textProp)
            {
                textProp.AddValueChanged(this.comboBox, (sender, args) => this.UpdateAdorner());
            }

            DependencyPropertyDescriptor selectedIndexProp = DependencyPropertyDescriptor.FromProperty(Selector.SelectedIndexProperty, typeof(ComboBox));
            if (null != selectedIndexProp)
            {
                selectedIndexProp.AddValueChanged(this.comboBox, (sender, args) => this.UpdateAdorner());
            }

            DependencyPropertyDescriptor selectedItemProp = DependencyPropertyDescriptor.FromProperty(Selector.SelectedItemProperty, typeof(ComboBox));
            if (null != selectedItemProp)
            {
                selectedItemProp.AddValueChanged(this.comboBox, (sender, args) => this.UpdateAdorner());
            }
        }

        /// <summary>
        /// Updates the adorner.
        /// </summary>
        private void UpdateAdorner()
        {
            if (!string.IsNullOrEmpty(this.comboBox.Text) || this.comboBox.IsFocused || this.comboBox.IsKeyboardFocused || this.comboBox.IsKeyboardFocusWithin || -1 != this.comboBox.SelectedIndex || null != this.comboBox.SelectedItem)
            {
                // Hide the Watermark Label if the adorner layer is visible
                this.comboBox.ToolTip = GetLabel(this.comboBox);
                this.comboBox.TryRemoveAdorners<UIElementAdorner>();
            }
            else
            {
                // Show the Watermark Label if the adorner layer is visible
                this.comboBox.ToolTip = null;
                this.comboBox.TryAddAdorner<UIElementAdorner>(this.uiElement);
            }
        }
    }
}