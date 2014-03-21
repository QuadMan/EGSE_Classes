//-----------------------------------------------------------------------
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
        [ExpectedException(typeof(NullReferenceException))]
        public void SpacewireTkMsgEventArgs_1ArgNull_ExceptionThrown()
        {
            // имитируем ошибочку
            SpacewireTkMsgEventArgs msg = new SpacewireTkMsgEventArgs(null, 0x00, 0x00, 0x00);
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

            Assert.AreEqual(to, msg.SptpInfo.To, "Ошибка в парсинге свойства To");
            Assert.AreEqual(protocolId, msg.SptpInfo.ProtocolId, "Ошибка в парсинге свойства ProtocolId");
            Assert.AreEqual(msgType, (byte)msg.SptpInfo.MsgType, "Ошибка в парсинге свойства MsgType");
            Assert.AreEqual(from, msg.SptpInfo.From, "Ошибка в парсинге свойства From");

            // проверяем результаты парсинга заголовка Icd

            Assert.AreEqual((b[0] & 0xe0) >> 5, msg.IcdInfo.Version, "Ошибка в парсинге свойства Version");
            Assert.AreEqual((SpacewireIcdMsgEventArgs.IcdType)((b[0] & 0x10) >> 4), msg.IcdInfo.Type, "Ошибка в парсинге свойства Type");
            Assert.AreEqual((SpacewireIcdMsgEventArgs.IcdFlag)((b[0] & 0x08) >> 3), msg.IcdInfo.Flag, "Ошибка в парсинге свойства Flag");
            Assert.AreEqual(((b[0] & 0x07) << 8) | b[1], msg.IcdInfo.Apid, "Ошибка в парсинге свойства Apid");
            Assert.AreEqual(b[2] >> 6, msg.IcdInfo.Segment, "Ошибка в парсинге свойства Segment");
            Assert.AreEqual(((b[2] & 0x3F) << 8) | b[3], msg.IcdInfo.Counter, "Ошибка в парсинге свойства Counter");
            Assert.AreEqual(((b[0] & 0x07) << 8) | b[1], msg.IcdInfo.Apid, "Ошибка в парсинге свойства Apid");
            Assert.AreEqual(((b[4]) << 8) | b[5], msg.IcdInfo.Size, "Ошибка в парсинге свойства Size");
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
        [ExpectedException(typeof(NullReferenceException))]
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

            Assert.AreEqual(to, msg.SptpInfo.To, "Ошибка в парсинге свойства To");            
            Assert.AreEqual(protocolId, msg.SptpInfo.ProtocolId, "Ошибка в парсинге свойства ProtocolId");
            Assert.AreEqual(msgType, (byte)msg.SptpInfo.MsgType, "Ошибка в парсинге свойства MsgType");
            Assert.AreEqual(from, msg.SptpInfo.From, "Ошибка в парсинге свойства From");
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
            Assert.AreEqual(to, msg.SptpInfo.To, "Ошибка в парсинге свойства To");
            Assert.AreEqual(protocolId, msg.SptpInfo.ProtocolId, "Ошибка в парсинге свойства ProtocolId");
            Assert.AreEqual(msgType, (byte)msg.SptpInfo.MsgType, "Ошибка в парсинге свойства MsgType");
            Assert.AreEqual(from, msg.SptpInfo.From, "Ошибка в парсинге свойства From");

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
        [ExpectedException(typeof(NullReferenceException))]
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
