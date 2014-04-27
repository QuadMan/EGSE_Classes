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
    
    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Enum, Inherited = false)]
    public class ActionAttribute : System.Attribute
    {
        private Type host;



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

        public Action<EgseBukNotify> Act { get; private set; }

        public Action<EgseBukNotify, object> ActArg { get; private set; }
    }
 
}
