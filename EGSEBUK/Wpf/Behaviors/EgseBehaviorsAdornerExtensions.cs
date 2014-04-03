//-----------------------------------------------------------------------
// <copyright file="EgseBehaviorsAdornerExtensions.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) MSDN Code Gallery. All rights reserved.
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Piotr W?odek, Коробейщиков Иван</author>
//-----------------------------------------------------------------------

namespace EGSE.Behaviors
{
    using System;
    using System.Windows;
    using System.Windows.Documents;

    /// <summary>
    /// Декоратор графического элемента.
    /// </summary>
    public static class AdornerExtensions
    {
        /// <summary>
        /// Tries the remove adorners.
        /// </summary>
        /// <typeparam name="T">Type of the element.</typeparam>
        /// <param name="elem">The element.</param>
        public static void TryRemoveAdorners<T>(this UIElement elem)
            where T : Adorner
        {
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(elem);
            if (null != adornerLayer)
            {
                adornerLayer.RemoveAdorners<T>(elem);
            }
        }

        /// <summary>
        /// Removes the adorners.
        /// </summary>
        /// <typeparam name="T">Type of the element.</typeparam>
        /// <param name="adr">The adr.</param>
        /// <param name="elem">The element.</param>
        public static void RemoveAdorners<T>(this AdornerLayer adr, UIElement elem)
            where T : Adorner
        {
            Adorner[] adorners = adr.GetAdorners(elem);

            if (null == adorners)
            {
                return;
            }

            for (int i = adorners.Length - 1; i >= 0; i--)
            {
                if (adorners[i] is T)
                {
                    adr.Remove(adorners[i]);
                }
            }
        }

        /// <summary>
        /// Tries the add adorner.
        /// </summary>
        /// <typeparam name="T">Type of the element.</typeparam>
        /// <param name="elem">The element.</param>
        /// <param name="adorner">The adorner.</param>
        public static void TryAddAdorner<T>(this UIElement elem, Adorner adorner)
            where T : Adorner
        {
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(elem);
            if (null != adornerLayer && !adornerLayer.ContainsAdorner<T>(elem))
            {
                adornerLayer.Add(adorner);
            }
        }

        /// <summary>
        /// Determines whether the specified adr contains adorner.
        /// </summary>
        /// <typeparam name="T">Type of the element.</typeparam>
        /// <param name="adr">The adr.</param>
        /// <param name="elem">The element.</param>
        /// <returns>
        ///   <c>true</c> if layer contained element adorner.
        /// </returns>
        public static bool ContainsAdorner<T>(this AdornerLayer adr, UIElement elem)
            where T : Adorner
        {
            Adorner[] adorners = adr.GetAdorners(elem);

            if (null == adorners)
            {
                return false;
            }

            for (int i = adorners.Length - 1; i >= 0; i--)
            {
                if (adorners[i] is T)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Removes all adorners.
        /// </summary>
        /// <param name="adr">The adr.</param>
        /// <param name="elem">The element.</param>
        public static void RemoveAllAdorners(this AdornerLayer adr, UIElement elem)
        {
            Adorner[] adorners = adr.GetAdorners(elem);

            if (null == adorners)
            {
                return;
            }

            foreach (Adorner toRemove in adorners)
            {
                adr.Remove(toRemove);
            }
        }
    }
}