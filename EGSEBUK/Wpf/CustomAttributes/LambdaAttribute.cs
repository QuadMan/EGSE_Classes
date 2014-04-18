//-----------------------------------------------------------------------
// <copyright file="LambdaAttribute.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Коробейщиков Иван</author>
//-----------------------------------------------------------------------

namespace Egse.CustomAttributes
{    
    using System;
    using System.Linq.Expressions;
    using Egse.Utilites;
    using Egse.Devices;

    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class ActionAttribute : System.Attribute
    {
        public Action<EgseBukNotify> Act { get; private set; }
        
        public ActionAttribute(Type hostingType, string hostingField)
        {
            System.Reflection.FieldInfo field = hostingType.GetField(hostingField);
            if (null != field)
            {
                Act = (Action<EgseBukNotify>)field.GetValue(null);
            }
            else
            {
                System.Windows.MessageBox.Show(string.Format(Resource.Get(@"eHostingField"), hostingField));
            }
        }
    }
 
}
