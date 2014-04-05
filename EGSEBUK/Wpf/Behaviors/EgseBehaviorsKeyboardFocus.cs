//-----------------------------------------------------------------------
// <copyright file="EgseBehaviorsKeyboardFocus.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) MSDN Code Gallery. All rights reserved.
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Piotr W?odek, Коробейщиков Иван</author>
//-----------------------------------------------------------------------

namespace Egse.Behaviors
{
    using System.Windows;
    using System.Windows.Input;

    /// <summary>
    /// Обработчик нажатия клавиатуры.
    /// </summary>
    public static class KeyboardFocusBehavior
    {
        /// <summary>
        /// The is focused property
        /// </summary>
        public static readonly DependencyProperty IsFocusedProperty = DependencyProperty.RegisterAttached("IsFocused", typeof(bool), typeof(KeyboardFocusBehavior), new UIPropertyMetadata(false, OnIsFocusedChanged));

        /// <summary>
        /// Gets the is focused.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns><c>true</c> if element had focused.</returns>
        public static bool GetIsFocused(UIElement obj)
        {
            return (bool)obj.GetValue(IsFocusedProperty);
        }

        /// <summary>
        /// Sets the is focused.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="value">if set to <c>true</c> [value].</param>
        public static void SetIsFocused(UIElement obj, bool value)
        {
            obj.SetValue(IsFocusedProperty, value);
        }

        /// <summary>
        /// Called when [is focused changed].
        /// </summary>
        /// <param name="o">The o.</param>
        /// <param name="args">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnIsFocusedChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            bool isFocused = (bool)args.NewValue;
            if (isFocused)
            {
                FrameworkElement element = (FrameworkElement)o;
                if (element.IsLoaded)
                {
                    Keyboard.Focus(element);
                }
                else
                {
                    element.Loaded += (sender, e) => Keyboard.Focus(element);
                }
            }
        }
    }
}