//-----------------------------------------------------------------------
// <copyright file="EgseBehaviorsWatermarkComboBox.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) MSDN Code Gallery. All rights reserved.
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Piotr Włodek, Коробейщиков Иван</author>
//-----------------------------------------------------------------------

namespace EGSE.Behaviors
{
    using System;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;

    /// <summary>
    /// Расширения для comboBox.
    /// </summary>
    public sealed class WatermarkComboBoxBehavior
    {
        /// <summary>
        /// The enable watermark property.
        /// </summary>
        public static readonly DependencyProperty EnableWatermarkProperty = DependencyProperty.RegisterAttached("EnableWatermark", typeof(bool), typeof(WatermarkComboBoxBehavior), new UIPropertyMetadata(false, OnEnableWatermarkChanged));
      
        /// <summary>
        /// The label style property.
        /// </summary>
        public static readonly DependencyProperty LabelStyleProperty = DependencyProperty.RegisterAttached("LabelStyle", typeof(Style), typeof(WatermarkComboBoxBehavior));

        /// <summary>
        /// The label property.
        /// </summary>
        public static readonly DependencyProperty LabelProperty = DependencyProperty.RegisterAttached("Label", typeof(string), typeof(WatermarkComboBoxBehavior));

        /// <summary>
        /// The watermark ComboBox behavior property.
        /// </summary>
        private static readonly DependencyProperty WatermarkComboBoxBehaviorProperty = DependencyProperty.RegisterAttached("WatermarkComboBoxBehavior", typeof(WatermarkComboBoxBehavior), typeof(WatermarkComboBoxBehavior), new UIPropertyMetadata(null));
    
        /// <summary>
        /// The ComboBox.
        /// </summary>
        private readonly ComboBox _comboBox;

        /// <summary>
        /// The UI element adorner.
        /// </summary>
        private UIElementAdorner uiElementAdorner;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="WatermarkComboBoxBehavior" />.
        /// </summary>
        /// <param name="comboBox">The combo box.</param>
        /// <exception cref="System.ArgumentNullException">Если аргументом задан null.</exception>
        private WatermarkComboBoxBehavior(ComboBox comboBox)
        {
            if (null == comboBox)
            {
                throw new ArgumentNullException("comboBox");
            }

            _comboBox = comboBox;
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
        private static WatermarkComboBoxBehavior GetWatermarkComboBoxBehavior(DependencyObject obj)
        {
            return (WatermarkComboBoxBehavior)obj.GetValue(WatermarkComboBoxBehaviorProperty);
        }

        /// <summary>
        /// Sets the watermark ComboBox behavior.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="value">The value.</param>
        private static void SetWatermarkComboBoxBehavior(DependencyObject obj, WatermarkComboBoxBehavior value)
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
                var enabled = (bool)e.OldValue;

                if (enabled)
                {
                    ComboBox comboBox = (ComboBox)d;
                    WatermarkComboBoxBehavior behavior = GetWatermarkComboBoxBehavior(comboBox);
                    behavior.Detach();

                    SetWatermarkComboBoxBehavior(comboBox, null);
                }
            }

            if (null != e.NewValue)
            {
                var enabled = (bool)e.NewValue;

                if (enabled)
                {
                    ComboBox comboBox = (ComboBox)d;
                    WatermarkComboBoxBehavior behavior = new WatermarkComboBoxBehavior(comboBox);
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
            _comboBox.Loaded += ComboBoxLoaded;
            _comboBox.DragEnter += ComboBoxDragEnter;
            _comboBox.DragLeave += ComboBoxDragLeave;
        }

        /// <summary>
        /// Detaches this instance.
        /// </summary>
        private void Detach()
        {
            _comboBox.Loaded -= ComboBoxLoaded;
            _comboBox.DragEnter -= ComboBoxDragEnter;
            _comboBox.DragLeave -= ComboBoxDragLeave;
        }

        /// <summary>
        /// ComboBoxes the drag leave.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void ComboBoxDragLeave(object sender, DragEventArgs e)
        {
            UpdateAdorner();
        }

        /// <summary>
        /// ComboBoxes the drag enter.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void ComboBoxDragEnter(object sender, DragEventArgs e)
        {
            _comboBox.TryRemoveAdorners<UIElementAdorner>();
        }

        /// <summary>
        /// ComboBoxes the loaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ComboBoxLoaded(object sender, RoutedEventArgs e)
        {
            Init();
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        private void Init()
        {
            uiElementAdorner = new UIElementAdorner(_comboBox, GetLabel(_comboBox), GetLabelStyle(_comboBox));
            UpdateAdorner();

            DependencyPropertyDescriptor focusProp = DependencyPropertyDescriptor.FromProperty(UIElement.IsFocusedProperty, typeof(ComboBox));
            if (null != focusProp)
            {
                focusProp.AddValueChanged(_comboBox, (sender, args) => UpdateAdorner());
            }

            DependencyPropertyDescriptor focusKeyboardProp = DependencyPropertyDescriptor.FromProperty(UIElement.IsKeyboardFocusedProperty, typeof(ComboBox));
            if (null != focusKeyboardProp)
            {
                focusKeyboardProp.AddValueChanged(_comboBox, (sender, args) => UpdateAdorner());
            }

            DependencyPropertyDescriptor focusKeyboardWithinProp = DependencyPropertyDescriptor.FromProperty(UIElement.IsKeyboardFocusWithinProperty, typeof(ComboBox));
            if (null != focusKeyboardWithinProp)
            {
                focusKeyboardWithinProp.AddValueChanged(_comboBox, (sender, args) => UpdateAdorner());
            }

            DependencyPropertyDescriptor textProp = DependencyPropertyDescriptor.FromProperty(ComboBox.TextProperty, typeof(ComboBox));
            if (null != textProp)
            {
                textProp.AddValueChanged(_comboBox, (sender, args) => UpdateAdorner());
            }

            DependencyPropertyDescriptor selectedIndexProp = DependencyPropertyDescriptor.FromProperty(Selector.SelectedIndexProperty, typeof(ComboBox));
            if (null != selectedIndexProp)
            {
                selectedIndexProp.AddValueChanged(_comboBox, (sender, args) => UpdateAdorner());
            }

            DependencyPropertyDescriptor selectedItemProp = DependencyPropertyDescriptor.FromProperty(Selector.SelectedItemProperty, typeof(ComboBox));
            if (null != selectedItemProp)
            {
                selectedItemProp.AddValueChanged(_comboBox, (sender, args) => UpdateAdorner());
            }
        }

        /// <summary>
        /// Updates the adorner.
        /// </summary>
        private void UpdateAdorner()
        {
            if (!string.IsNullOrEmpty(_comboBox.Text) ||
                _comboBox.IsFocused ||
                _comboBox.IsKeyboardFocused ||
                _comboBox.IsKeyboardFocusWithin ||
                -1 != _comboBox.SelectedIndex ||
                null != _comboBox.SelectedItem)
            {
                // Hide the Watermark Label if the adorner layer is visible
                _comboBox.ToolTip = GetLabel(_comboBox);
                _comboBox.TryRemoveAdorners<UIElementAdorner>();
            }
            else
            {
                // Show the Watermark Label if the adorner layer is visible
                _comboBox.ToolTip = null;
                _comboBox.TryAddAdorner<UIElementAdorner>(uiElementAdorner);
            }
        }
    }
}