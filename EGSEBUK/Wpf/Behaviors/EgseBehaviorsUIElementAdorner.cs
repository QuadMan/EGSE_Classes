//-----------------------------------------------------------------------
// <copyright file="EgseBehaviorsUIElementAdorner.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) MSDN Code Gallery. All rights reserved.
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Piotr Włodek, Коробейщиков Иван</author>
//-----------------------------------------------------------------------

namespace EGSE.Behaviors
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Media;

    /// <summary>
    /// Цепляется к графическому элементу, для расширения функционала.
    /// </summary>
    public class UIElementAdorner : Adorner
    {
        /// <summary>
        /// The UI element.
        /// </summary>
        private readonly FrameworkElement uiElement;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="UIElementAdorner" />.
        /// </summary>
        /// <param name="adornedElement">The adorned element.</param>
        /// <param name="label">The label.</param>
        /// <param name="labelStyle">The label style.</param>
        public UIElementAdorner(UIElement adornedElement, string label, Style labelStyle)
            : base(adornedElement)
        {
            uiElement = new TextBlock { Style = labelStyle, Text = label };
        }

        /// <summary>
        /// Gets the number of visual child elements within this element.
        /// </summary>
        /// <returns>The number of visual child elements for this element.</returns>
        protected override int VisualChildrenCount
        {
            get
            {
                return 1;
            }
        }

        /// <summary>
        /// Implements any custom measuring behavior for the adorner.
        /// </summary>
        /// <param name="constraint">A size to constrain the adorner to.</param>
        /// <returns>
        /// A <see cref="T:System.Windows.Size" /> object representing the amount of layout space needed by the adorner.
        /// </returns>
        protected override Size MeasureOverride(Size constraint)
        {
            uiElement.Measure(constraint);
            return constraint;
        }

        /// <summary>
        /// When overridden in a derived class, positions child elements and determines a size for a <see cref="T:System.Windows.FrameworkElement" /> derived class.
        /// </summary>
        /// <param name="finalSize">The final area within the parent that this element should use to arrange itself and its children.</param>
        /// <returns>
        /// The actual size used.
        /// </returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            uiElement.Arrange(new Rect(finalSize));
            return finalSize;
        }

        /// <summary>
        /// Overrides <see cref="M:System.Windows.Media.Visual.GetVisualChild(System.Int32)" />, and returns a child at the specified index from a collection of child elements.
        /// </summary>
        /// <param name="index">The zero-based index of the requested child element in the collection.</param>
        /// <returns>
        /// The requested child element. This should not return null; if the provided index is out of range, an exception is thrown.
        /// </returns>
        protected override Visual GetVisualChild(int index)
        {
            return uiElement;
        }
    }
}