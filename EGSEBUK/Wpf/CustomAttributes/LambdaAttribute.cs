//-----------------------------------------------------------------------
// <copyright file="LambdaAttribute.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Коробейщиков Иван</author>
//-----------------------------------------------------------------------

namespace Egse.CustomAttributes
{
    using System;
    using Egse.Devices;
    using Egse.Utilites;

    /// <summary>
    /// Исползуется для "привязки" статических методов к полям или перечисления.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Enum, Inherited = false)]
    public class ActionAttribute : System.Attribute
    {
        /// <summary>
        /// The host
        /// </summary>
        private Type host;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ActionAttribute" />.
        /// </summary>
        /// <param name="hostingType">Type of the hosting.</param>
        /// <param name="hostingField">The hosting field.</param>
        /// <param name="needArg">if set to <c>true</c> [need argument].</param>
        public ActionAttribute(Type hostingType, string hostingField, bool needArg = false)
        {
            host = hostingType;
            System.Reflection.FieldInfo field = hostingType.GetField(hostingField, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            if (null != field)
            {
                if (needArg)
                {
                    ActArg = (Action<EgseBukNotify, object>)field.GetValue(null);
                }
                else
                {
                    Act = (Action<EgseBukNotify>)field.GetValue(null);
                }                
            }
            else
            {
                System.Windows.MessageBox.Show(string.Format(Resource.Get(@"eHostingField"), hostingField));
            }
        }

        /// <summary>
        /// Получает "привязанный" метод.
        /// </summary>
        /// <value>
        /// Метод с аргументом нотификатора.
        /// </value>
        public Action<EgseBukNotify> Act { get; private set; }

        /// <summary>
        /// Получает "привязанный" метод.
        /// </summary>
        /// <value>
        /// Метод с аргументом нотификатора и дополнительной ссылкой на экземпляр объекта.
        /// </value>
        public Action<EgseBukNotify, object> ActArg { get; private set; }
    }
}
