//-----------------------------------------------------------------------
// <copyright file="EGSEUtilitesTest.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Коробейщиков Иван</author>
//-----------------------------------------------------------------------

namespace EGSE.Utilites.UnitTest
{
    using System;
    using System.Configuration;   
    using System.Linq;
    using System.Reflection; 
    using EGSE.Utilites;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Тест структуры EgseTime.
    /// </summary>
    [TestClass]
    public class ConverterTest
    {
        /// <summary>
        /// Egses the time_ income5 bytes_ exception thrown.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void EgseTime_Income5Bytes_ExceptionThrown()
        {
            byte[] b = new byte[5] { 0, 0, 0, 0, 0 };         
            EgseTime test = b.AsEgseTime();
        }

        /// <summary>
        /// Egses the time_ income null_ exception thrown.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EgseTime_IncomeNull_ExceptionThrown()
        {
            byte[] b = null;
            EgseTime test = b.AsEgseTime();
        }

        /// <summary>
        /// Egses the time_ income0 bytes_ exception thrown.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void EgseTime_Income0Bytes_ExceptionThrown()
        {
            byte[] b = new byte[] { };
            EgseTime test = b.AsEgseTime();
        }

        /// <summary>
        /// Egses the time_ now_ returns equal token value.
        /// </summary>
        [TestMethod]
        public void EgseTime_Now_ReturnsEqualTokenValue()
        {
            DateTime nowTest = DateTime.Now;
            EgseTime test = EgseTime.Now();

            Assert.AreEqual(0, test.Day, string.Format(Resource.Get(@"stErrorInProperty"), "Day"));
            Assert.AreEqual(nowTest.Hour, test.Hour, string.Format(Resource.Get(@"stErrorInProperty"), "Hour"));
            Assert.AreEqual(nowTest.Minute, test.Minute, string.Format(Resource.Get(@"stErrorInProperty"), "Minute"));
            Assert.AreEqual(nowTest.Second, test.Second, string.Format(Resource.Get(@"stErrorInProperty"), "Second"));
            Assert.AreEqual(nowTest.Millisecond, test.Millisecond, nowTest.Millisecond - test.Millisecond, string.Format(Resource.Get(@"stErrorInProperty"), "Millisecond"));
            Assert.AreEqual(0, test.Microsecond, string.Format(Resource.Get(@"stErrorInProperty"), "Microsecond"));
        }

        /// <summary>
        /// Egses the time_ to array_ returns equal token value.
        /// </summary>
        [TestMethod]
        public void EgseTime_ToArray_ReturnsEqualTokenValue()
        {
            EgseTime test = EgseTime.Now();

            byte[] b = test.ToArray();

            Assert.AreEqual((b[0] << 3) | (b[1] >> 5), test.Day, string.Format(Resource.Get(@"stErrorInProperty"), "Day"));
            Assert.AreEqual(b[1] & 0x1F, test.Hour, string.Format(Resource.Get(@"stErrorInProperty"), "Hour"));
            Assert.AreEqual(b[2] >> 2, test.Minute, string.Format(Resource.Get(@"stErrorInProperty"), "Minute"));
            Assert.AreEqual(((b[2] & 0x03) << 4) | (b[3] >> 4), test.Second, string.Format(Resource.Get(@"stErrorInProperty"), "Second"));
            Assert.AreEqual(((b[3] & 0xF) << 4) | (b[4] >> 6), test.Millisecond, string.Format(Resource.Get(@"stErrorInProperty"), "Millisecond"));
            Assert.AreEqual(((b[4] & 0x03) << 8) | b[5], test.Microsecond, string.Format(Resource.Get(@"stErrorInProperty"), "Microsecond"));
        }

        /// <summary>
        /// Converter_s as egse time_ returns equal token value.
        /// </summary>
        [TestMethod]
        public void Converter_AsEgseTime_ReturnsEqualTokenValue()
        {
            EgseTime arr = EgseTime.Now();
            byte[] b = arr.ToArray();
            EgseTime test = b.AsEgseTime();

            Assert.AreEqual((b[0] << 3) | (b[1] >> 5), test.Day, string.Format(Resource.Get(@"stErrorInProperty"), "Day"));
            Assert.AreEqual(b[1] & 0x1F, test.Hour, string.Format(Resource.Get(@"stErrorInProperty"), "Hour"));
            Assert.AreEqual(b[2] >> 2, test.Minute, string.Format(Resource.Get(@"stErrorInProperty"), "Minute"));
            Assert.AreEqual(((b[2] & 0x03) << 4) | (b[3] >> 4), test.Second, string.Format(Resource.Get(@"stErrorInProperty"), "Second"));
            Assert.AreEqual(((b[3] & 0xF) << 4) | (b[4] >> 6), test.Millisecond, string.Format(Resource.Get(@"stErrorInProperty"), "Millisecond"));
            Assert.AreEqual(((b[4] & 0x03) << 8) | b[5], test.Microsecond, string.Format(Resource.Get(@"stErrorInProperty"), "Microsecond"));

            string str = test.ToString();
        }
    }
}
