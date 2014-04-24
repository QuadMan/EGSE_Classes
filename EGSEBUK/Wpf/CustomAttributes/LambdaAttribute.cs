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

    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Enum, Inherited = false)]
    public class ActionAttribute : System.Attribute
    {
        public Action<EgseBukNotify> Act { get; private set; }

        public Action<EgseBukNotify, object> ActArg { get; private set; }

        public ActionAttribute(Type hostingType, string hostingField, bool needArg = false)
        {
            System.Reflection.FieldInfo field = hostingType.GetField(hostingField);
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
    }
 
}
