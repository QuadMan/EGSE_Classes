﻿//-----------------------------------------------------------------------
// <copyright file="EGSEProtocolHsiTest.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Коробейщиков Иван</author>
//-----------------------------------------------------------------------

namespace EGSE.Protocols.UnitTest
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using EGSE.Protocols;
    using EGSE.Utilites;

    [TestClass]
    public class HsiMsgEventArgsTest
    {
        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void HsiMsgEventArgs_Income3Bytes_ExceptionThrown()
        {
            // формируем массив из 3 байт и проверяем формирование посылки
            byte[] buf = new byte[3] { 0x33, 0xf2, 0x34 };
            HsiMsgEventArgs msg = new HsiMsgEventArgs(buf, buf.Length);
        }

        [TestMethod]
        public void HsiMsgEventArgs_Income4Bytes_ReturnsEqualTokenValue()
        {
            byte flag = 0x33;
            ushort size = 0xfFF2;

            // формируем массив из 4 байт и проверяем формирование посылки
            byte[] buf = new byte[4] { 0xa4, flag, (byte)(size >> 8), (byte)size };
            HsiMsgEventArgs msg = new HsiMsgEventArgs(buf, buf.Length);

            // проверяем результаты парсинга заголовка
            Assert.AreEqual((HsiMsgEventArgs.Type)flag, msg.Info.Flag, "Ошибка в парсинге свойства Flag");
            Assert.AreEqual((size & 0x7FFF), msg.Info.Size, "Ошибка в парсинге свойства Size");
            Assert.AreEqual((HsiMsgEventArgs.HsiLine)(size >> 15), msg.Info.Line, "Ошибка в парсинге свойства Line");            
        }

        [TestMethod]
        public void HsiMsgEventArgs_Income5Bytes_ReturnsEqualTokenValue()
        {
            byte flag = 0x33;
            ushort size = 0xfFF2;

            byte customData = 0xAB;

            // формируем массив из 4 байт и проверяем формирование посылки
            byte[] buf = new byte[5] { 0xa4, flag, (byte)(size >> 8), (byte)size, customData };
            HsiMsgEventArgs msg = new HsiMsgEventArgs(buf, buf.Length);

            // проверяем результаты парсинга заголовка
            Assert.AreEqual((HsiMsgEventArgs.Type)flag, msg.Info.Flag, "Ошибка в парсинге свойства Flag");
            Assert.AreEqual((size & 0x7FFF), msg.Info.Size, "Ошибка в парсинге свойства Size");
            Assert.AreEqual((HsiMsgEventArgs.HsiLine)(size >> 15), msg.Info.Line, "Ошибка в парсинге свойства Line"); 

            // проверяем результат парсинга данных кадра
            CollectionAssert.AreEqual(new byte[1] { customData }, msg.Data, "Ошибка в парсинге данных кадра");
        }

        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void HsiMsgEventArgs_Income0Bytes_ExceptionThrown()
        {
            // имитируем ошибочку
            byte[] buf = new byte[] { };
            HsiMsgEventArgs msg = new HsiMsgEventArgs(buf, buf.Length);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void HsiMsgEventArgs_1ArgNull_ExceptionThrown()
        {
            // имитируем ошибочку
            HsiMsgEventArgs msg = new HsiMsgEventArgs(null, 0);
        }

        [TestMethod]
        public void HsiMsgEventArgs_Income65540Bytes_ReturnsEqualTokenValue()
        {
            RandomBufferGenerator generator = new RandomBufferGenerator(65535);
            byte[] buf = generator.GenerateBufferFromSeed(65535);
            // формируем максимально допустимую посылку для протокола spacewire.
            HsiMsgEventArgs msg = new HsiMsgEventArgs(buf, buf.Length);

            // проверяем результат парсинга данных кадра
            CollectionAssert.AreEqual(buf.Skip(4).ToArray(), msg.Data, "Ошибка в парсинге данных кадра");
        }

        [TestMethod]
        public void SpacewireSptpMsgEventArgs_ToArray_ReturnsEqualTokenValue()
        {
            RandomBufferGenerator generator = new RandomBufferGenerator(65535);
            byte[] buf = generator.GenerateBufferFromSeed(65535);
            // формируем максимально допустимую посылку для протокола spacewire.
            HsiMsgEventArgs msg = new HsiMsgEventArgs(buf, buf.Length);

            // проверяем результат преобразования к массиву
            CollectionAssert.AreEqual(buf, msg.ToArray(), "Ошибка в преобразовании к массиву");
        }
    }
}