﻿//-----------------------------------------------------------------------
// <copyright file="EGSEProtocolSpacewireTest.cs" company="IKI RSSI, laboratory №711">
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
    public class SpacewireTmMsgEventArgsTest
    {
        [TestMethod]
        public void SpacewireTmMsgEventArgs_Income18Bytes_ReturnsEqualTokenValue()
        {
            // формируем массив из 10 байт и проверяем формирование посылки
            Random rnd = new Random();
            Byte[] buf = new Byte[18];
            rnd.NextBytes(buf);
            buf[8] = 0x00;
            buf[9] = 0x02;
            byte[] testCrc = buf.Skip(4).ToArray().Take(buf.Length - 6).ToArray();
            buf[buf.Length - 2] = (byte)(Crc16.Get(testCrc, testCrc.Length) >> 8);
            buf[buf.Length - 1] = (byte)Crc16.Get(testCrc, testCrc.Length);
            SpacewireTmMsgEventArgs msg = new SpacewireTmMsgEventArgs(buf, 0x00, 0x00, 0x00);

            Assert.AreEqual((buf[buf.Length - 2] << 8) | buf[buf.Length - 1], msg.Crc, "Ошибка в парсинге CRC");

            Assert.AreEqual(Crc16.Get(testCrc, testCrc.Length), msg.Crc, "Ошибка в расчете CRC");

            Assert.AreEqual(msg.NeededCrc, msg.Crc, "Ошибка в расчете CRC (внутренний метод)");

            byte[] test = buf.Skip(14).ToArray().Take(2).ToArray();
            CollectionAssert.AreEqual(test, msg.Data, "Ошибка в парсинге данных кадра");
        }

        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireTmMsgEventArgs_Income15Bytes_ExceptionThrown()
        {
            Random rnd = new Random();
            Byte[] buf = new Byte[15];
            rnd.NextBytes(buf);

            // формируем максимально допустимую посылку для протокола spacewire.
            SpacewireTmMsgEventArgs msg = new SpacewireTmMsgEventArgs(buf, 0x00, 0x00, 0x00);
        }

        [TestMethod]
        public void SpacewireTmMsgEventArgs_Income17Bytes_ReturnsEqualTokenValue()
        {
            // формируем массив из 10 байт и проверяем формирование посылки
            Random rnd = new Random();
            Byte[] buf = new Byte[17];
            rnd.NextBytes(buf);
            buf[8] = 0x00;
            buf[9] = 0x01;
            byte[] testCrc = buf.Skip(4).ToArray().Take(buf.Length - 6).ToArray();
            buf[buf.Length - 2] = (byte)(Crc16.Get(testCrc, testCrc.Length) >> 8);
            buf[buf.Length - 1] = (byte)Crc16.Get(testCrc, testCrc.Length);
            SpacewireTmMsgEventArgs msg = new SpacewireTmMsgEventArgs(buf, 0x00, 0x00, 0x00);

            Assert.AreEqual((buf[buf.Length - 2] << 8) | buf[buf.Length - 1], msg.Crc, "Ошибка в парсинге CRC");

            Assert.AreEqual(Crc16.Get(testCrc, testCrc.Length), msg.Crc, "Ошибка в расчете CRC");

            Assert.AreEqual(msg.NeededCrc, msg.Crc, "Ошибка в расчете CRC (внутренний метод)");

            byte[] test = buf.Skip(14).ToArray().Take(1).ToArray();
            CollectionAssert.AreEqual(test, msg.Data, "Ошибка в парсинге данных кадра");
        }

        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireTmMsgEventArgs_Income3Bytes_ExceptionThrown()
        {
            // формируем массив из 3 байт и проверяем формирование посылки
            byte[] buf = new byte[3] { 0x33, 0xf2, 0x34 };
            SpacewireTmMsgEventArgs msg = new SpacewireTmMsgEventArgs(buf, 0x00, 0x00, 0x00);
        }

        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireTmMsgEventArgs_Income4Bytes_ExceptionThrown()
        {
            byte from = 0x33;
            byte to = 0x32;
            byte protocolId = 0xf2;
            byte msgType = 0x00;

            // формируем массив из 4 байт и проверяем формирование посылки
            byte[] buf = new byte[4] { to, protocolId, msgType, from };
            SpacewireTmMsgEventArgs msg = new SpacewireTmMsgEventArgs(buf, 0x00, 0x00, 0x00);
        }

        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireTmMsgEventArgs_Income10Bytes_ExceptionThrown()
        {
            byte from = 0x33;
            byte to = 0x32;
            byte protocolId = 0xf2;
            byte msgType = 0x00;

            Random rnd = new Random();
            Byte[] b = new Byte[6];
            rnd.NextBytes(b);

            // формируем массив из 10 байт и проверяем формирование посылки
            byte[] buf = new byte[10] { to, protocolId, msgType, from, b[0], b[1], b[2], b[3], b[4], b[5] };
            SpacewireTmMsgEventArgs msg = new SpacewireTmMsgEventArgs(buf, 0x00, 0x00, 0x00);
        }

        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireTmMsgEventArgs_Income5Bytes_ExceptionThrown()
        {
            byte from = 0x33;
            byte to = 0x32;
            byte protocolId = 0xf2;
            byte msgType = 0x00;

            byte customData = 0xAB;

            // формируем массив из 5 байт и проверяем формирование посылки
            byte[] buf = new byte[5] { to, protocolId, msgType, from, customData };
            SpacewireTmMsgEventArgs msg = new SpacewireTmMsgEventArgs(buf, 0x00, 0x00, 0x00);
        }

        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireTmMsgEventArgs_Income0Bytes_ExceptionThrown()
        {
            // имитируем ошибочку
            byte[] buf = new byte[] { };
            SpacewireTmMsgEventArgs msg = new SpacewireTmMsgEventArgs(buf, 0x00, 0x00, 0x00);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SpacewireTmMsgEventArgs_1ArgNull_ExceptionThrown()
        {
            // имитируем ошибочку
            SpacewireTmMsgEventArgs msg = new SpacewireTmMsgEventArgs(null, 0x00, 0x00, 0x00);
        }

        [TestMethod]
        public void SpacewireTmMsgEventArgs_Income4100Bytes_ReturnsEqualTokenValue()
        {
            RandomBufferGenerator generator = new RandomBufferGenerator(4100);
            byte[] buf = generator.GenerateBufferFromSeed(4100);
            buf[8] = 0x0f;
            buf[9] = 0xf4;
            // формируем максимально допустимую посылку для протокола spacewire.
            SpacewireTmMsgEventArgs msg = new SpacewireTmMsgEventArgs(buf, 0x00, 0x00, 0x00);

            // проверяем результат парсинга данных кадра
            byte[] test = buf.Skip(14).ToArray().Take(0xff4).ToArray();
            CollectionAssert.AreEqual(test, msg.Data, "Ошибка в парсинге данных кадра");
        }

        [TestMethod]
        public void SpacewireTmMsgEventArgs_ToArray_ReturnsEqualTokenValue()
        {
            RandomBufferGenerator generator = new RandomBufferGenerator(65540);
            byte[] buf = generator.GenerateBufferFromSeed(65540);
            // формируем максимально допустимую посылку для протокола spacewire.
            SpacewireTmMsgEventArgs msg = new SpacewireTmMsgEventArgs(buf, 0x00, 0x00, 0x00);

            // проверяем результат преобразования к массиву
            CollectionAssert.AreEqual(buf, msg.ToArray(), "Ошибка в преобразовании к массиву");
        }
    }


    [TestClass]
    public class SpacewireTkMsgEventArgsTest
    {
        [TestMethod]
        public void SpacewireTkMsgEventArgs_Income18Bytes_ReturnsEqualTokenValue()
        {
            // формируем массив из 10 байт и проверяем формирование посылки
            Random rnd = new Random();
            Byte[] buf = new Byte[18];
            rnd.NextBytes(buf);
            buf[8] = 0x00;
            buf[9] = 0x02;
            byte[] testCrc = buf.Skip(4).ToArray().Take(buf.Length - 6).ToArray();
            buf[buf.Length - 2] = (byte)(Crc16.Get(testCrc, testCrc.Length) >> 8);
            buf[buf.Length - 1] = (byte)Crc16.Get(testCrc, testCrc.Length);
            SpacewireTkMsgEventArgs msg = new SpacewireTkMsgEventArgs(buf, 0x00, 0x00, 0x00);

            Assert.AreEqual((buf[buf.Length - 2] << 8) | buf[buf.Length - 1], msg.Crc, "Ошибка в парсинге CRC");

            Assert.AreEqual(Crc16.Get(testCrc, testCrc.Length), msg.Crc, "Ошибка в расчете CRC");

            Assert.AreEqual(msg.NeededCrc, msg.Crc, "Ошибка в расчете CRC (внутренний метод)");
            
            byte[] test = buf.Skip(14).ToArray().Take(2).ToArray();
            CollectionAssert.AreEqual(test, msg.Data, "Ошибка в парсинге данных кадра");
        }

        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireTkMsgEventArgs_Income15Bytes_ExceptionThrown()
        {
            Random rnd = new Random();
            Byte[] buf = new Byte[15];
            rnd.NextBytes(buf);

            // формируем максимально допустимую посылку для протокола spacewire.
            SpacewireTkMsgEventArgs msg = new SpacewireTkMsgEventArgs(buf, 0x00, 0x00, 0x00);
        }

        [TestMethod]
        public void SpacewireTkMsgEventArgs_Income17Bytes_ReturnsEqualTokenValue()
        {
            // формируем массив из 10 байт и проверяем формирование посылки
            Random rnd = new Random();
            Byte[] buf = new Byte[17];
            rnd.NextBytes(buf);
            buf[8] = 0x00;
            buf[9] = 0x01;
            byte[] testCrc = buf.Skip(4).ToArray().Take(buf.Length - 6).ToArray();
            buf[buf.Length - 2] = (byte)(Crc16.Get(testCrc, testCrc.Length) >> 8);
            buf[buf.Length - 1] = (byte)Crc16.Get(testCrc, testCrc.Length);
            SpacewireTkMsgEventArgs msg = new SpacewireTkMsgEventArgs(buf, 0x00, 0x00, 0x00);

            Assert.AreEqual((buf[buf.Length - 2] << 8) | buf[buf.Length - 1], msg.Crc, "Ошибка в парсинге CRC");
                      
            Assert.AreEqual(Crc16.Get(testCrc, testCrc.Length), msg.Crc, "Ошибка в расчете CRC");

            Assert.AreEqual(msg.NeededCrc, msg.Crc, "Ошибка в расчете CRC (внутренний метод)");

            byte[] test = buf.Skip(14).ToArray().Take(1).ToArray();
            CollectionAssert.AreEqual(test, msg.Data, "Ошибка в парсинге данных кадра");
        }

        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireTkMsgEventArgs_Income3Bytes_ExceptionThrown()
        {
            // формируем массив из 3 байт и проверяем формирование посылки
            byte[] buf = new byte[3] { 0x33, 0xf2, 0x34 };
            SpacewireTkMsgEventArgs msg = new SpacewireTkMsgEventArgs(buf, 0x00, 0x00, 0x00);
        }

        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireTkMsgEventArgs_Income4Bytes_ExceptionThrown()
        {
            byte from = 0x33;
            byte to = 0x32;
            byte protocolId = 0xf2;
            byte msgType = 0x00;

            // формируем массив из 4 байт и проверяем формирование посылки
            byte[] buf = new byte[4] { to, protocolId, msgType, from };
            SpacewireTkMsgEventArgs msg = new SpacewireTkMsgEventArgs(buf, 0x00, 0x00, 0x00);
        }

        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireTkMsgEventArgs_Income10Bytes_ExceptionThrown()
        {
            byte from = 0x33;
            byte to = 0x32;
            byte protocolId = 0xf2;
            byte msgType = 0x00;

            Random rnd = new Random();
            Byte[] b = new Byte[6];
            rnd.NextBytes(b);

            // формируем массив из 10 байт и проверяем формирование посылки
            byte[] buf = new byte[10] { to, protocolId, msgType, from, b[0], b[1], b[2], b[3], b[4], b[5] };
            SpacewireTkMsgEventArgs msg = new SpacewireTkMsgEventArgs(buf, 0x00, 0x00, 0x00);
        }

        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireTkMsgEventArgs_Income5Bytes_ExceptionThrown()
        {
            byte from = 0x33;
            byte to = 0x32;
            byte protocolId = 0xf2;
            byte msgType = 0x00;

            byte customData = 0xAB;

            // формируем массив из 5 байт и проверяем формирование посылки
            byte[] buf = new byte[5] { to, protocolId, msgType, from, customData };
            SpacewireTkMsgEventArgs msg = new SpacewireTkMsgEventArgs(buf, 0x00, 0x00, 0x00);
        }

        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireTkMsgEventArgs_Income0Bytes_ExceptionThrown()
        {
            // имитируем ошибочку
            byte[] buf = new byte[] { };
            SpacewireTkMsgEventArgs msg = new SpacewireTkMsgEventArgs(buf, 0x00, 0x00, 0x00);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SpacewireTkMsgEventArgs_1ArgNull_ExceptionThrown()
        {
            // имитируем ошибочку
            SpacewireTkMsgEventArgs msg = new SpacewireTkMsgEventArgs(null, 0x00, 0x00, 0x00);
        }

        [TestMethod]
        public void SpacewireTkMsgEventArgs_GetNew_ReturnsEqualTokenValue()
        {
            byte to = 0x77;
            byte from = 0x66;
            short apid = 607;

            Random rnd = new Random();
            Byte[] buf = new Byte[6];
            rnd.NextBytes(buf);

            SpacewireTkMsgEventArgs msg = SpacewireTkMsgEventArgs.GetNew(buf, to, from, apid);

            CollectionAssert.AreEqual(buf, msg.Data, "Ошибка в парсинге данных кадра");
            Assert.AreEqual(to, msg.TkInfo.IcdInfo.SptpInfo.To, "Ошибка в парсинге свойства SptpInfo.To");
            Assert.AreEqual((SpacewireSptpMsgEventArgs.SptpProtocol)0xf2, msg.TkInfo.IcdInfo.SptpInfo.ProtocolId, "Ошибка в парсинге свойства SptpInfo.ProtocolId");
            Assert.AreEqual(0x00, (byte)msg.TkInfo.IcdInfo.SptpInfo.MsgType, "Ошибка в парсинге свойства SptpInfo.MsgType");
            Assert.AreEqual(from, msg.TkInfo.IcdInfo.SptpInfo.From, "Ошибка в парсинге свойства SptpInfo.From");

            Assert.AreEqual(0, msg.TkInfo.IcdInfo.Version, "Ошибка в парсинге свойства IcdInfo.Version");
            Assert.AreEqual(apid, msg.TkInfo.IcdInfo.Apid, "Ошибка в парсинге свойства IcdInfo.Apid");
            Assert.AreEqual(0, msg.TkInfo.IcdInfo.Counter, "Ошибка в парсинге свойства IcdInfo.Counter");
            Assert.AreEqual(SpacewireTkMsgEventArgs.IcdFlag.HeaderFill, msg.TkInfo.IcdInfo.Flag, "Ошибка в парсинге свойства IcdInfo.Flag");
            Assert.AreEqual(3, msg.TkInfo.IcdInfo.Segment, "Ошибка в парсинге свойства IcdInfo.Segment");
            Assert.AreEqual(6, msg.TkInfo.IcdInfo.Size, "Ошибка в парсинге свойства IcdInfo.Size");

            string str = msg.TkInfo.ToString(false);
        }

        [TestMethod]
        public void SpacewireTkMsgEventArgs_Income4100Bytes_ReturnsEqualTokenValue()
        {
            RandomBufferGenerator generator = new RandomBufferGenerator(4100);
            byte[] buf = generator.GenerateBufferFromSeed(4100);
            buf[8] = 0x0f;
            buf[9] = 0xf4;
            // формируем максимально допустимую посылку для протокола spacewire.
            SpacewireTkMsgEventArgs msg = new SpacewireTkMsgEventArgs(buf, 0x00, 0x00, 0x00);

            // проверяем результат парсинга данных кадра
            byte[] test = buf.Skip(14).ToArray().Take(0xff4).ToArray();
            CollectionAssert.AreEqual(test, msg.Data, "Ошибка в парсинге данных кадра");
        }

        [TestMethod]
        public void SpacewireTkMsgEventArgs_ToArray_ReturnsEqualTokenValue()
        {
            RandomBufferGenerator generator = new RandomBufferGenerator(65540);
            byte[] buf = generator.GenerateBufferFromSeed(65540);
            // формируем максимально допустимую посылку для протокола spacewire.
            SpacewireTkMsgEventArgs msg = new SpacewireTkMsgEventArgs(buf, 0x00, 0x00, 0x00);

            // проверяем результат преобразования к массиву
            CollectionAssert.AreEqual(buf, msg.ToArray(), "Ошибка в преобразовании к массиву");
        }
    }

    [TestClass]
    public class SpacewireIcdMsgEventArgsTest
    {
        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireIcdMsgEventArgs_Income3Bytes_ExceptionThrown()
        {
            // формируем массив из 3 байт и проверяем формирование посылки
            byte[] buf = new byte[3] { 0x33, 0xf2, 0x34 };
            SpacewireIcdMsgEventArgs msg = new SpacewireIcdMsgEventArgs(buf, 0x00, 0x00, 0x00);
        }

        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireIcdMsgEventArgs_Income4Bytes_ExceptionThrown()
        {
            byte from = 0x33;
            byte to = 0x32;
            byte protocolId = 0xf2;
            byte msgType = 0x00;

            // формируем массив из 4 байт и проверяем формирование посылки
            byte[] buf = new byte[4] { to, protocolId, msgType, from };
            SpacewireIcdMsgEventArgs msg = new SpacewireIcdMsgEventArgs(buf, 0x00, 0x00, 0x00);
        }

        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireIcdMsgEventArgs_Income9Bytes_ExceptionThrown()
        {
            Random rnd = new Random();
            Byte[] buf = new Byte[9];
            rnd.NextBytes(buf);

            // формируем максимально допустимую посылку для протокола spacewire.
            SpacewireIcdMsgEventArgs msg = new SpacewireIcdMsgEventArgs(buf, 0x00, 0x00, 0x00);        
        }

        [TestMethod]
        public void SpacewireIcdMsgEventArgs_Income10Bytes_ReturnsEqualTokenValue()
        {
            byte from = 0x33;
            byte to = 0x32;
            byte protocolId = 0xf2;
            byte msgType = 0x00;

            Random rnd = new Random();
            Byte[] b = new Byte[6];
            rnd.NextBytes(b);

            // формируем массив из 10 байт и проверяем формирование посылки
            byte[] buf = new byte[10] { to, protocolId, msgType, from, b[0], b[1], b[2], b[3], b[4], b[5] };
            SpacewireIcdMsgEventArgs msg = new SpacewireIcdMsgEventArgs(buf, 0x00, 0x00, 0x00);

            // проверяем результаты парсинга заголовка Sptp

            Assert.AreEqual(to, msg.SptpInfo.To, "Ошибка в парсинге свойства SptpInfo.To");
            Assert.AreEqual((SpacewireSptpMsgEventArgs.SptpProtocol)protocolId, msg.SptpInfo.ProtocolId, "Ошибка в парсинге свойства SptpInfo.ProtocolId");
            Assert.AreEqual(msgType, (byte)msg.SptpInfo.MsgType, "Ошибка в парсинге свойства SptpInfo.MsgType");
            Assert.AreEqual(from, msg.SptpInfo.From, "Ошибка в парсинге свойства SptpInfo.From");

            // проверяем результаты парсинга заголовка Icd

            Assert.AreEqual((b[0] & 0xe0) >> 5, msg.IcdInfo.Version, "Ошибка в парсинге свойства IcdInfo.Version");
            Assert.AreEqual((SpacewireIcdMsgEventArgs.IcdType)((b[0] & 0x10) >> 4), msg.IcdInfo.Type, "Ошибка в парсинге свойства IcdInfo.Type");
            Assert.AreEqual((SpacewireIcdMsgEventArgs.IcdFlag)((b[0] & 0x08) >> 3), msg.IcdInfo.Flag, "Ошибка в парсинге свойства IcdInfo.Flag");
            Assert.AreEqual(((b[0] & 0x07) << 8) | b[1], msg.IcdInfo.Apid, "Ошибка в парсинге свойства IcdInfo.Apid");
            Assert.AreEqual(b[2] >> 6, msg.IcdInfo.Segment, "Ошибка в парсинге свойства IcdInfo.Segment");
            Assert.AreEqual(((b[2] & 0x3F) << 8) | b[3], msg.IcdInfo.Counter, "Ошибка в парсинге свойства IcdInfo.Counter");
            Assert.AreEqual(((b[0] & 0x07) << 8) | b[1], msg.IcdInfo.Apid, "Ошибка в парсинге свойства IcdInfo.Apid");
            Assert.AreEqual(((b[4]) << 8) | b[5], msg.IcdInfo.Size, "Ошибка в парсинге свойства IcdInfo.Size");
        }

        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireIcdMsgEventArgs_Income5Bytes_ExceptionThrown()
        {
            byte from = 0x33;
            byte to = 0x32;
            byte protocolId = 0xf2;
            byte msgType = 0x00;

            byte customData = 0xAB;

            // формируем массив из 5 байт и проверяем формирование посылки
            byte[] buf = new byte[5] { to, protocolId, msgType, from, customData };
            SpacewireIcdMsgEventArgs msg = new SpacewireIcdMsgEventArgs(buf, 0x00, 0x00, 0x00);           
        }

        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireIcdMsgEventArgs_Income0Bytes_ExceptionThrown()
        {            
            // имитируем ошибочку
            byte[] buf = new byte[] { };
            SpacewireIcdMsgEventArgs msg = new SpacewireIcdMsgEventArgs(buf, 0x00, 0x00, 0x00);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SpacewireIcdMsgEventArgs_1ArgNull_ExceptionThrown()
        {
            // имитируем ошибочку
            SpacewireIcdMsgEventArgs msg = new SpacewireIcdMsgEventArgs(null, 0x00, 0x00, 0x00);
        }

        [TestMethod]
        public void SpacewireIcdMsgEventArgs_Income65540Bytes_ReturnsEqualTokenValue()
        {
            RandomBufferGenerator generator = new RandomBufferGenerator(65540);
            byte[] buf = generator.GenerateBufferFromSeed(65540);
            // формируем максимально допустимую посылку для протокола spacewire.
            SpacewireIcdMsgEventArgs msg = new SpacewireIcdMsgEventArgs(buf, 0x00, 0x00, 0x00);

            // проверяем результат парсинга данных кадра
            byte[] test = buf.Skip(10).ToArray();
            CollectionAssert.AreEqual(test, msg.Data, "Ошибка в парсинге данных кадра");
        }

        [TestMethod]
        public void SpacewireIcdMsgEventArgs_ToArray_ReturnsEqualTokenValue()
        {
            RandomBufferGenerator generator = new RandomBufferGenerator(65540);
            byte[] buf = generator.GenerateBufferFromSeed(65540);
            // формируем максимально допустимую посылку для протокола spacewire.
            SpacewireIcdMsgEventArgs msg = new SpacewireIcdMsgEventArgs(buf, 0x00, 0x00, 0x00);

            // проверяем результат преобразования к массиву
            CollectionAssert.AreEqual(buf, msg.ToArray(), "Ошибка в преобразовании к массиву");
        }       
    }
    
    [TestClass]
    public class SpacewireObtMsgEventArgsTest
    {
        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireObtMsgEventArgs_Income3Bytes_ExceptionThrown()
        {
            // формируем массив из 3 байт и проверяем формирование посылки
            byte[] buf = new byte[3] { 0x33, 0xf2, 0x34 };
            SpacewireObtMsgEventArgs msg = new SpacewireObtMsgEventArgs(buf, 0x00, 0x00, 0x00);
        }

        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireObtMsgEventArgs_Income4Bytes_ExceptionThrown()
        {
            byte from = 0x33;
            byte to = 0x32;
            byte protocolId = 0xf2;
            byte msgType = 0x00;

            // формируем массив из 4 байт и проверяем формирование посылки
            byte[] buf = new byte[4] { to, protocolId, msgType, from };
            SpacewireObtMsgEventArgs msg = new SpacewireObtMsgEventArgs(buf, 0x00, 0x00, 0x00);
        }

        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireObtMsgEventArgs_Income5Bytes_ExceptionThrown()
        {
            byte from = 0x33;
            byte to = 0x32;
            byte protocolId = 0xf2;
            byte msgType = 0x00;

            byte customData = 0xAB;

            // формируем массив из 5 байт и проверяем формирование посылки
            byte[] buf = new byte[5] { to, protocolId, msgType, from, customData };
            SpacewireObtMsgEventArgs msg = new SpacewireObtMsgEventArgs(buf, 0x00, 0x00, 0x00);
        }

        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireObtMsgEventArgs_Income0Bytes_ExceptionThrown()
        {            
            // имитируем ошибочку
            byte[] buf = new byte[] { };
            SpacewireObtMsgEventArgs msg = new SpacewireObtMsgEventArgs(buf, 0x00, 0x00, 0x00);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SpacewireObtMsgEventArgs_1ArgNull_ExceptionThrown()
        {
            // имитируем ошибочку
            SpacewireObtMsgEventArgs msg = new SpacewireObtMsgEventArgs(null, 0x00, 0x00, 0x00);
        }

        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireObtMsgEventArgs_Income65540Bytes_ExceptionThrown()
        {
            RandomBufferGenerator generator = new RandomBufferGenerator(65540);
            byte[] buf = generator.GenerateBufferFromSeed(65540);
            // формируем максимально допустимую посылку для протокола spacewire.
            SpacewireObtMsgEventArgs msg = new SpacewireObtMsgEventArgs(buf, 0x00, 0x00, 0x00);
        }

        [TestMethod]
        public void SpacewireObtMsgEventArgs_ToArray_ReturnsEqualTokenValue()
        {
            RandomBufferGenerator generator = new RandomBufferGenerator(16);
            byte[] buf = generator.GenerateBufferFromSeed(16);
            // формируем максимально допустимую посылку для протокола spacewire.
            SpacewireObtMsgEventArgs msg = new SpacewireObtMsgEventArgs(buf, 0x00, 0x00, 0x00);

            // проверяем результат преобразования к массиву
            CollectionAssert.AreEqual(buf, msg.ToArray(), "Ошибка в преобразовании к массиву");
        }

        [TestMethod]
        public void SpacewireObtMsgEventArgs_Income16Bytes_ReturnsEqualTokenValue()
        {
            byte from = 0x33;
            byte to = 0x32;
            byte protocolId = 0xf2;
            byte msgType = 0x00;

            Random rnd = new Random();
            Byte[] b = new Byte[12];
            rnd.NextBytes(b);

            // формируем массив из 10 байт и проверяем формирование посылки
            byte[] buf = new byte[16] { to, protocolId, msgType, from, b[0], b[1], b[2], b[3], b[4], b[5], b[6], b[7], b[8], b[9], b[10], b[11] };

            // формируем максимально допустимую посылку для протокола spacewire.
            SpacewireObtMsgEventArgs msg = new SpacewireObtMsgEventArgs(buf, 0x00, 0x00, 0x00);

            // проверяем результаты парсинга заголовка Sptp

            Assert.AreEqual(to, msg.SptpInfo.To, "Ошибка в парсинге свойства SptpInfo.To");
            Assert.AreEqual((SpacewireSptpMsgEventArgs.SptpProtocol)protocolId, msg.SptpInfo.ProtocolId, "Ошибка в парсинге свойства SptpInfo.ProtocolId");
            Assert.AreEqual(msgType, (byte)msg.SptpInfo.MsgType, "Ошибка в парсинге свойства SptpInfo.MsgType");
            Assert.AreEqual(from, msg.SptpInfo.From, "Ошибка в парсинге свойства SptpInfo.From");

            // проверяем результаты парсинга заголовка Icd

            Assert.AreEqual((b[0] & 0xe0) >> 5, msg.IcdInfo.Version, "Ошибка в парсинге свойства IcdInfo.Version");
            Assert.AreEqual((SpacewireIcdMsgEventArgs.IcdType)((b[0] & 0x10) >> 4), msg.IcdInfo.Type, "Ошибка в парсинге свойства IcdInfo.Type");
            Assert.AreEqual((SpacewireIcdMsgEventArgs.IcdFlag)((b[0] & 0x08) >> 3), msg.IcdInfo.Flag, "Ошибка в парсинге свойства IcdInfo.Flag");
            Assert.AreEqual(((b[0] & 0x07) << 8) | b[1], msg.IcdInfo.Apid, "Ошибка в парсинге свойства IcdInfo.Apid");
            Assert.AreEqual(b[2] >> 6, msg.IcdInfo.Segment, "Ошибка в парсинге свойства IcdInfo.Segment");
            Assert.AreEqual(((b[2] & 0x3F) << 8) | b[3], msg.IcdInfo.Counter, "Ошибка в парсинге свойства IcdInfo.Counter");
            Assert.AreEqual(((b[0] & 0x07) << 8) | b[1], msg.IcdInfo.Apid, "Ошибка в парсинге свойства IcdInfo.Apid");
            Assert.AreEqual(((b[4]) << 8) | b[5], msg.IcdInfo.Size, "Ошибка в парсинге свойства IcdInfo.Size");

            // проверяем результаты парсинга Obt

            Assert.AreEqual(b[6], msg.ObtInfo.Normal, "Ошибка в парсинге свойства ObtInfo.Normal");
            Assert.AreEqual(b[7], msg.ObtInfo.Extended, "Ошибка в парсинге свойства ObtInfo.Extended");
            Assert.AreEqual((uint)SpacewireIcdMsgEventArgs.ConvertToInt(new byte[4] { b[11], b[10], b[9], b[8] }), msg.ObtInfo.Value, "Ошибка в парсинге свойства ObtInfo.Value");
        }
    }

    [TestClass]
    public class SpacewireTimeTickMsgEventArgsTest
    {
        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireTimeTickMsgEventArgs_Income3Bytes_ExceptionThrown()
        {
            // формируем массив из 3 байт и проверяем формирование посылки
            byte[] buf = new byte[3] { 0x33, 0xf2, 0x34 };
            SpacewireTimeTickMsgEventArgs msg = new SpacewireTimeTickMsgEventArgs(buf, 0x00, 0x00, 0x00);
        }

        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireTimeTickMsgEventArgs_Income4Bytes_ExceptionThrown()
        {
            byte from = 0x33;
            byte to = 0x32;
            byte protocolId = 0xf2;
            byte msgType = 0x00;

            // формируем массив из 4 байт и проверяем формирование посылки
            byte[] buf = new byte[4] { to, protocolId, msgType, from };
            SpacewireTimeTickMsgEventArgs msg = new SpacewireTimeTickMsgEventArgs(buf, 0x00, 0x00, 0x00);
        }

        [TestMethod]
        public void SpacewireTimeTickMsgEventArgs_Income1Bytes_ReturnsEqualTokenValue()
        {
            byte customData = 0xAB;

            // формируем массив из 1 байт и проверяем формирование посылки
            byte[] buf = new byte[1] { customData };
            SpacewireTimeTickMsgEventArgs msg = new SpacewireTimeTickMsgEventArgs(buf, 0x00, 0x00, 0x00);

            // проверяем результаты парсинга заголовка
            Assert.AreEqual(customData, msg.TimeTickInfo.Value, "Ошибка в парсинге свойства TimeTickInfo.Value");

        }

        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireTimeTickMsgEventArgs_Income0Bytes_ExceptionThrown()
        {
            // имитируем ошибочку
            byte[] buf = new byte[] { };
            SpacewireTimeTickMsgEventArgs msg = new SpacewireTimeTickMsgEventArgs(buf, 0x00, 0x00, 0x00);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SpacewireTimeTickMsgEventArgs_1ArgNull_ExceptionThrown()
        {
            // имитируем ошибочку
            SpacewireTimeTickMsgEventArgs msg = new SpacewireTimeTickMsgEventArgs(null, 0x00, 0x00, 0x00);
        }
    }

    [TestClass]
    public class SpacewireSptpMsgEventArgsTest
    {
        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireSptpMsgEventArgs_Income3Bytes_ExceptionThrown()
        {
            // формируем массив из 3 байт и проверяем формирование посылки
            byte[] buf = new byte[3] { 0x33, 0xf2, 0x34 };
            SpacewireSptpMsgEventArgs msg = new SpacewireSptpMsgEventArgs(buf, 0x00, 0x00, 0x00);
        }

        [TestMethod]
        public void SpacewireSptpMsgEventArgs_GetNew_ReturnsEqualTokenValue()
        {
            byte to = 0x77;
            byte from = 0x66;

            Random rnd = new Random();
            Byte[] buf = new Byte[6];
            rnd.NextBytes(buf);

            SpacewireSptpMsgEventArgs msg = SpacewireSptpMsgEventArgs.GetNew(buf, to, from);

            CollectionAssert.AreEqual(buf, msg.Data, "Ошибка в парсинге данных кадра");
            Assert.AreEqual(to, msg.SptpInfo.To, "Ошибка в парсинге свойства SptpInfo.To");
            Assert.AreEqual(SpacewireSptpMsgEventArgs.SptpProtocol.Standard, msg.SptpInfo.ProtocolId, "Ошибка в парсинге свойства SptpInfo.ProtocolId");
            Assert.AreEqual(SpacewireSptpMsgEventArgs.SptpType.Data, msg.SptpInfo.MsgType, "Ошибка в парсинге свойства SptpInfo.MsgType");
            Assert.AreEqual(from, msg.SptpInfo.From, "Ошибка в парсинге свойства SptpInfo.From");

            string str = msg.SptpInfo.ToString(false);
        }

        [TestMethod]
        public void SpacewireSptpMsgEventArgs_Income4Bytes_ReturnsEqualTokenValue()
        {
            byte from = 0x33;
            byte to = 0x32;
            byte protocolId = 0xf2;
            byte msgType = 0x00;

            // формируем массив из 4 байт и проверяем формирование посылки
            byte[] buf = new byte[4] { to, protocolId, msgType, from };
            SpacewireSptpMsgEventArgs msg = new SpacewireSptpMsgEventArgs(buf, 0x00, 0x00, 0x00);

            // проверяем результаты парсинга заголовка

            Assert.AreEqual(to, msg.SptpInfo.To, "Ошибка в парсинге свойства SptpInfo.To");
            Assert.AreEqual((SpacewireSptpMsgEventArgs.SptpProtocol)protocolId, msg.SptpInfo.ProtocolId, "Ошибка в парсинге свойства SptpInfo.ProtocolId");
            Assert.AreEqual(msgType, (byte)msg.SptpInfo.MsgType, "Ошибка в парсинге свойства SptpInfo.MsgType");
            Assert.AreEqual(from, msg.SptpInfo.From, "Ошибка в парсинге свойства SptpInfo.From");
        }

        [TestMethod]
        public void SpacewireSptpMsgEventArgs_Income5Bytes_ReturnsEqualTokenValue()
        {
            byte from = 0x33;
            byte to = 0x32;
            byte protocolId = 0xf2;
            byte msgType = 0x00;

            byte customData = 0xAB;

            // формируем массив из 5 байт и проверяем формирование посылки
            byte[] buf = new byte[5] { to, protocolId, msgType, from, customData };
            SpacewireSptpMsgEventArgs msg = new SpacewireSptpMsgEventArgs(buf, 0x00, 0x00, 0x00);
           
            // проверяем результаты парсинга заголовка
            Assert.AreEqual(to, msg.SptpInfo.To, "Ошибка в парсинге свойства SptpInfo.To");
            Assert.AreEqual((SpacewireSptpMsgEventArgs.SptpProtocol)protocolId, msg.SptpInfo.ProtocolId, "Ошибка в парсинге свойства SptpInfo.ProtocolId");
            Assert.AreEqual(msgType, (byte)msg.SptpInfo.MsgType, "Ошибка в парсинге свойства SptpInfo.MsgType");
            Assert.AreEqual(from, msg.SptpInfo.From, "Ошибка в парсинге свойства SptpInfo.From");

            // проверяем результат парсинга данных кадра
            CollectionAssert.AreEqual(new byte[1] { customData }, msg.Data, "Ошибка в парсинге данных кадра");
        }

        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireSptpMsgEventArgs_Income0Bytes_ExceptionThrown()
        {            
            // имитируем ошибочку
            byte[] buf = new byte[] { };
            SpacewireSptpMsgEventArgs msg = new SpacewireSptpMsgEventArgs(buf, 0x00, 0x00, 0x00);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SpacewireSptpMsgEventArgs_1ArgNull_ExceptionThrown()
        {
            // имитируем ошибочку
            SpacewireSptpMsgEventArgs msg = new SpacewireSptpMsgEventArgs(null, 0x00, 0x00, 0x00);
        }

        [TestMethod]
        public void SpacewireSptpMsgEventArgs_Income65540Bytes_ReturnsEqualTokenValue()
        {
            RandomBufferGenerator generator = new RandomBufferGenerator(65540);
            byte[] buf = generator.GenerateBufferFromSeed(65540);
            // формируем максимально допустимую посылку для протокола spacewire.
            SpacewireSptpMsgEventArgs msg = new SpacewireSptpMsgEventArgs(buf, 0x00, 0x00, 0x00);

            // проверяем результат парсинга данных кадра
            CollectionAssert.AreEqual(buf.Skip(4).ToArray(), msg.Data, "Ошибка в парсинге данных кадра");
        }

        [TestMethod]
        public void SpacewireSptpMsgEventArgs_ToArray_ReturnsEqualTokenValue()
        {
            RandomBufferGenerator generator = new RandomBufferGenerator(65540);
            byte[] buf = generator.GenerateBufferFromSeed(65540);
            // формируем максимально допустимую посылку для протокола spacewire.
            SpacewireSptpMsgEventArgs msg = new SpacewireSptpMsgEventArgs(buf, 0x00, 0x00, 0x00);

            // проверяем результат преобразования к массиву
            CollectionAssert.AreEqual(buf, msg.ToArray(), "Ошибка в преобразовании к массиву");
        }       
    }
}