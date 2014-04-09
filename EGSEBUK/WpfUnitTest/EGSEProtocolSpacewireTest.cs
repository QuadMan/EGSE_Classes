//-----------------------------------------------------------------------
// <copyright file="EGSEProtocolSpacewireTest.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Коробейщиков Иван</author>
//-----------------------------------------------------------------------

namespace Egse.Protocols.UnitTest
{
    using System;
    using System.Linq;
    using Egse.Protocols;
    using Egse.Utilites;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Организует тестирование сообщений телеметрии.
    /// </summary>
    [TestClass]
    public class SpacewireTmMsgEventArgsTest
    {
        /// <summary>
        /// Spacewires the tm MSG event args_ income18 bytes_ returns equal token value.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireTmMsgEventArgs_Income18Bytes_ExceptionThrown()
        {
            // формируем массив из 10 байт и проверяем формирование посылки
            Random rnd = new Random();
            byte[] buf = new byte[18];
            rnd.NextBytes(buf);
            buf[8] = 0x00;
            buf[9] = 0x02;
            byte[] testCrc = buf.Skip(4).ToArray().Take(buf.Length - 6).ToArray();
            buf[buf.Length - 2] = (byte)(Crc16.Get(testCrc, testCrc.Length) >> 8);
            buf[buf.Length - 1] = (byte)Crc16.Get(testCrc, testCrc.Length);
            SpacewireTmMsgEventArgs msg = new SpacewireTmMsgEventArgs(buf, 0x00, 0x00, 0x00);
        }

        /// <summary>
        /// Spacewires the tm MSG event args_ income21 bytes_ exception thrown.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireTmMsgEventArgs_Income21Bytes_ExceptionThrown()
        {
            // формируем массив из 10 байт и проверяем формирование посылки
            Random rnd = new Random();
            byte[] buf = new byte[21];
            rnd.NextBytes(buf);
            buf[8] = 0x00;
            buf[9] = 0x02;
            byte[] testCrc = buf.Skip(4).ToArray().Take(buf.Length - 6).ToArray();
            buf[buf.Length - 2] = (byte)(Crc16.Get(testCrc, testCrc.Length) >> 8);
            buf[buf.Length - 1] = (byte)Crc16.Get(testCrc, testCrc.Length);
            SpacewireTmMsgEventArgs msg = new SpacewireTmMsgEventArgs(buf, 0x00, 0x00, 0x00);
        }

        /// <summary>
        /// Spacewires the tm MSG event args_ income22 bytes_ returns equal token value.
        /// </summary>
        [TestMethod]
        public void SpacewireTmMsgEventArgs_Income22Bytes_ReturnsEqualTokenValue()
        {
            // формируем массив из 10 байт и проверяем формирование посылки
            Random rnd = new Random();
            byte[] buf = new byte[22];
            rnd.NextBytes(buf);
            buf[8] = 0x00;
            buf[9] = 0x02;
            buf[10] = 0x01;
            buf[11] = 0x02;
            buf[12] = 0x03;
            buf[13] = 0x04;
            buf[14] = 0x05;
            buf[15] = 0x06;
            buf[16] = 0x00;
            buf[17] = 0x00;
            buf[18] = 0x00;
            buf[19] = 0x09;
            byte[] testCrc = buf.Skip(4).ToArray().Take(buf.Length - 6).ToArray();
            buf[buf.Length - 2] = (byte)(Crc16.Get(testCrc, testCrc.Length) >> 8);
            buf[buf.Length - 1] = (byte)Crc16.Get(testCrc, testCrc.Length);
            SpacewireTmMsgEventArgs msg = new SpacewireTmMsgEventArgs(buf, 0x00, 0x00, 0x00);

            Assert.AreEqual((buf[buf.Length - 2] << 8) | buf[buf.Length - 1], msg.Crc, "Ошибка в парсинге CRC");

            Assert.AreEqual(Crc16.Get(testCrc, testCrc.Length), msg.Crc, "Ошибка в расчете CRC");

            Assert.AreEqual(msg.NeededCrc, msg.Crc, "Ошибка в расчете CRC (внутренний метод)");

            string str = msg.TmInfo.ToString(false);
        }

        /// <summary>
        /// Spacewires the tm MSG event args_ income15 bytes_ exception thrown.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireTmMsgEventArgs_Income15Bytes_ExceptionThrown()
        {
            Random rnd = new Random();
            byte[] buf = new byte[15];
            rnd.NextBytes(buf);

            // формируем максимально допустимую посылку для протокола spacewire.
            SpacewireTmMsgEventArgs msg = new SpacewireTmMsgEventArgs(buf, 0x00, 0x00, 0x00);
        }

        /// <summary>
        /// Spacewires the tm MSG event args_ income17 bytes_ returns equal token value.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireTmMsgEventArgs_Income17Bytes_ExceptionThrown()
        {
            // формируем массив из 10 байт и проверяем формирование посылки
            Random rnd = new Random();
            byte[] buf = new byte[17];
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

        /// <summary>
        /// Spacewires the tm MSG event args_ income3 bytes_ exception thrown.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireTmMsgEventArgs_Income3Bytes_ExceptionThrown()
        {
            // формируем массив из 3 байт и проверяем формирование посылки
            byte[] buf = new byte[3] { 0x33, 0xf2, 0x34 };
            SpacewireTmMsgEventArgs msg = new SpacewireTmMsgEventArgs(buf, 0x00, 0x00, 0x00);
        }

        /// <summary>
        /// Spacewires the tm MSG event args_ income4 bytes_ exception thrown.
        /// </summary>
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

        /// <summary>
        /// Spacewires the tm MSG event args_ income10 bytes_ exception thrown.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireTmMsgEventArgs_Income10Bytes_ExceptionThrown()
        {
            byte from = 0x33;
            byte to = 0x32;
            byte protocolId = 0xf2;
            byte msgType = 0x00;

            Random rnd = new Random();
            byte[] b = new byte[6];
            rnd.NextBytes(b);

            // формируем массив из 10 байт и проверяем формирование посылки
            byte[] buf = new byte[10] { to, protocolId, msgType, from, b[0], b[1], b[2], b[3], b[4], b[5] };
            SpacewireTmMsgEventArgs msg = new SpacewireTmMsgEventArgs(buf, 0x00, 0x00, 0x00);
        }

        /// <summary>
        /// Spacewires the tm MSG event args_ income5 bytes_ exception thrown.
        /// </summary>
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

        /// <summary>
        /// Spacewires the tm MSG event args_ income0 bytes_ exception thrown.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireTmMsgEventArgs_Income0Bytes_ExceptionThrown()
        {
            // имитируем ошибочку
            byte[] buf = new byte[] { };
            SpacewireTmMsgEventArgs msg = new SpacewireTmMsgEventArgs(buf, 0x00, 0x00, 0x00);
        }

        /// <summary>
        /// Spacewires the tm MSG event args_1 argument null_ exception thrown.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SpacewireTmMsgEventArgs_1ArgNull_ExceptionThrown()
        {
            // имитируем ошибочку
            SpacewireTmMsgEventArgs msg = new SpacewireTmMsgEventArgs(null, 0x00, 0x00, 0x00);
        }

        /// <summary>
        /// Spacewires the tm MSG event args_ income4100 bytes_ returns equal token value.
        /// </summary>
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
            byte[] test = buf.Skip(20).ToArray().Take(0xfee).ToArray();
            CollectionAssert.AreEqual(test, msg.Data, "Ошибка в парсинге данных кадра");
        }

        /// <summary>
        /// Spacewires the tm MSG event args_ to array_ returns equal token value.
        /// </summary>
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

    /// <summary>
    /// Организует тестирование сообщений телекоманд.
    /// </summary>
    [TestClass]
    public class SpacewireTkMsgEventArgsTest
    {
        /// <summary>
        /// Spacewires the tk MSG event args_ income18 bytes_ returns equal token value.
        /// </summary>
        [TestMethod]
        public void SpacewireTkMsgEventArgs_Income18Bytes_ReturnsEqualTokenValue()
        {
            // формируем массив из 10 байт и проверяем формирование посылки
            Random rnd = new Random();
            byte[] buf = new byte[18];
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

        /// <summary>
        /// Spacewires the tk MSG event args_ income15 bytes_ exception thrown.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireTkMsgEventArgs_Income15Bytes_ExceptionThrown()
        {
            Random rnd = new Random();
            byte[] buf = new byte[15];
            rnd.NextBytes(buf);

            // формируем максимально допустимую посылку для протокола spacewire.
            SpacewireTkMsgEventArgs msg = new SpacewireTkMsgEventArgs(buf, 0x00, 0x00, 0x00);
        }

        /// <summary>
        /// Spacewires the tk MSG event args_ income17 bytes_ returns equal token value.
        /// </summary>
        [TestMethod]
        public void SpacewireTkMsgEventArgs_Income17Bytes_ReturnsEqualTokenValue()
        {
            // формируем массив из 10 байт и проверяем формирование посылки
            Random rnd = new Random();
            byte[] buf = new byte[17];
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

        /// <summary>
        /// Spacewires the tk MSG event args_ income3 bytes_ exception thrown.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireTkMsgEventArgs_Income3Bytes_ExceptionThrown()
        {
            // формируем массив из 3 байт и проверяем формирование посылки
            byte[] buf = new byte[3] { 0x33, 0xf2, 0x34 };
            SpacewireTkMsgEventArgs msg = new SpacewireTkMsgEventArgs(buf, 0x00, 0x00, 0x00);
        }

        /// <summary>
        /// Spacewires the tk MSG event args_ income4 bytes_ exception thrown.
        /// </summary>
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

        /// <summary>
        /// Spacewires the tk MSG event args_ income10 bytes_ exception thrown.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireTkMsgEventArgs_Income10Bytes_ExceptionThrown()
        {
            byte from = 0x33;
            byte to = 0x32;
            byte protocolId = 0xf2;
            byte msgType = 0x00;

            Random rnd = new Random();
            byte[] b = new byte[6];
            rnd.NextBytes(b);

            // формируем массив из 10 байт и проверяем формирование посылки
            byte[] buf = new byte[10] { to, protocolId, msgType, from, b[0], b[1], b[2], b[3], b[4], b[5] };
            SpacewireTkMsgEventArgs msg = new SpacewireTkMsgEventArgs(buf, 0x00, 0x00, 0x00);
        }

        /// <summary>
        /// Spacewires the tk MSG event args_ income5 bytes_ exception thrown.
        /// </summary>
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

        /// <summary>
        /// Spacewires the tk MSG event args_ income0 bytes_ exception thrown.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireTkMsgEventArgs_Income0Bytes_ExceptionThrown()
        {
            // имитируем ошибочку
            byte[] buf = new byte[] { };
            SpacewireTkMsgEventArgs msg = new SpacewireTkMsgEventArgs(buf, 0x00, 0x00, 0x00);
        }

        /// <summary>
        /// Spacewires the tk MSG event args_1 argument null_ exception thrown.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SpacewireTkMsgEventArgs_1ArgNull_ExceptionThrown()
        {
            // имитируем ошибочку
            SpacewireTkMsgEventArgs msg = new SpacewireTkMsgEventArgs(null, 0x00, 0x00, 0x00);
        }

        /// <summary>
        /// Spacewires the tk MSG event args_ get new_ returns equal token value.
        /// </summary>
        [TestMethod]
        public void SpacewireTkMsgEventArgs_GetNew_ReturnsEqualTokenValue()
        {
            byte to = 0x77;
            byte from = 0x66;
            short apid = 612;

           // Random rnd = new Random();
            byte[] buf = new byte[6];
            //rnd.NextBytes(buf);

            SpacewireTkMsgEventArgs msg = SpacewireTkMsgEventArgs.GetNew(buf, to, from, apid, true, true);

            byte[] buf_new = msg.ToArray();

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

            Assert.AreEqual((byte)((1 << 3) | 1), msg.TkInfo.Acknowledgment, "Ошибка в парсинге свойства TkInfo.Acknowledgment");

            string str = msg.TkInfo.ToString(false);
        }

        /// <summary>
        /// Spacewires the tk MSG event args_ income4100 bytes_ returns equal token value.
        /// </summary>
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

        /// <summary>
        /// Spacewires the tk MSG event args_ to array_ returns equal token value.
        /// </summary>
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

    /// <summary>
    /// Организует тестирование Icd сообщений.
    /// </summary>
    [TestClass]
    public class SpacewireIcdMsgEventArgsTest
    {
        /// <summary>
        /// Spacewires the icd MSG event args_ income3 bytes_ exception thrown.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireIcdMsgEventArgs_Income3Bytes_ExceptionThrown()
        {
            // формируем массив из 3 байт и проверяем формирование посылки
            byte[] buf = new byte[3] { 0x33, 0xf2, 0x34 };
            SpacewireIcdMsgEventArgs msg = new SpacewireIcdMsgEventArgs(buf, 0x00, 0x00, 0x00);
        }

        /// <summary>
        /// Spacewires the icd MSG event args_ income4 bytes_ exception thrown.
        /// </summary>
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

        /// <summary>
        /// Spacewires the icd MSG event args_ income9 bytes_ exception thrown.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireIcdMsgEventArgs_Income9Bytes_ExceptionThrown()
        {
            Random rnd = new Random();
            byte[] buf = new byte[9];
            rnd.NextBytes(buf);

            // формируем максимально допустимую посылку для протокола spacewire.
            SpacewireIcdMsgEventArgs msg = new SpacewireIcdMsgEventArgs(buf, 0x00, 0x00, 0x00);        
        }

        /// <summary>
        /// Spacewires the icd MSG event args_ income10 bytes_ returns equal token value.
        /// </summary>
        [TestMethod]
        public void SpacewireIcdMsgEventArgs_Income10Bytes_ReturnsEqualTokenValue()
        {
            byte from = 0x33;
            byte to = 0x32;
            byte protocolId = 0xf2;
            byte msgType = 0x00;

            Random rnd = new Random();
            byte[] b = new byte[6];
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
            Assert.AreEqual((b[4] << 8) | b[5], msg.IcdInfo.Size, "Ошибка в парсинге свойства IcdInfo.Size");
        }

        /// <summary>
        /// Spacewires the icd MSG event args_ income5 bytes_ exception thrown.
        /// </summary>
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

        /// <summary>
        /// Spacewires the icd MSG event args_ income0 bytes_ exception thrown.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireIcdMsgEventArgs_Income0Bytes_ExceptionThrown()
        {            
            // имитируем ошибочку
            byte[] buf = new byte[] { };
            SpacewireIcdMsgEventArgs msg = new SpacewireIcdMsgEventArgs(buf, 0x00, 0x00, 0x00);
        }

        /// <summary>
        /// Spacewires the icd MSG event args_1 argument null_ exception thrown.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SpacewireIcdMsgEventArgs_1ArgNull_ExceptionThrown()
        {
            // имитируем ошибочку
            SpacewireIcdMsgEventArgs msg = new SpacewireIcdMsgEventArgs(null, 0x00, 0x00, 0x00);
        }

        /// <summary>
        /// Spacewires the icd MSG event args_ income65540 bytes_ returns equal token value.
        /// </summary>
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

        /// <summary>
        /// Spacewires the icd MSG event args_ to array_ returns equal token value.
        /// </summary>
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

    /// <summary>
    /// Организует тестирование КБВ сообщений.
    /// </summary>
    [TestClass]
    public class SpacewireObtMsgEventArgsTest
    {
        /// <summary>
        /// Spacewires the obt MSG event args_ income3 bytes_ exception thrown.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireObtMsgEventArgs_Income3Bytes_ExceptionThrown()
        {
            // формируем массив из 3 байт и проверяем формирование посылки
            byte[] buf = new byte[3] { 0x33, 0xf2, 0x34 };
            SpacewireObtMsgEventArgs msg = new SpacewireObtMsgEventArgs(buf, 0x00, 0x00, 0x00);
        }

        /// <summary>
        /// Spacewires the obt MSG event args_ income4 bytes_ exception thrown.
        /// </summary>
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

        /// <summary>
        /// Spacewires the obt MSG event args_ income5 bytes_ exception thrown.
        /// </summary>
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

        /// <summary>
        /// Spacewires the obt MSG event args_ income0 bytes_ exception thrown.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireObtMsgEventArgs_Income0Bytes_ExceptionThrown()
        {            
            // имитируем ошибочку
            byte[] buf = new byte[] { };
            SpacewireObtMsgEventArgs msg = new SpacewireObtMsgEventArgs(buf, 0x00, 0x00, 0x00);
        }

        /// <summary>
        /// Spacewires the obt MSG event args_1 argument null_ exception thrown.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SpacewireObtMsgEventArgs_1ArgNull_ExceptionThrown()
        {
            // имитируем ошибочку
            SpacewireObtMsgEventArgs msg = new SpacewireObtMsgEventArgs(null, 0x00, 0x00, 0x00);
        }

        /// <summary>
        /// Spacewires the obt MSG event args_ income65540 bytes_ exception thrown.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireObtMsgEventArgs_Income65540Bytes_ExceptionThrown()
        {
            RandomBufferGenerator generator = new RandomBufferGenerator(65540);
            byte[] buf = generator.GenerateBufferFromSeed(65540);

            // формируем максимально допустимую посылку для протокола spacewire.
            SpacewireObtMsgEventArgs msg = new SpacewireObtMsgEventArgs(buf, 0x00, 0x00, 0x00);
        }

        /// <summary>
        /// Spacewires the obt MSG event args_ to array_ returns equal token value.
        /// </summary>
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

        /// <summary>
        /// Spacewires the obt MSG event args_ income16 bytes_ returns equal token value.
        /// </summary>
        [TestMethod]
        public void SpacewireObtMsgEventArgs_Income16Bytes_ReturnsEqualTokenValue()
        {
            byte from = 0x33;
            byte to = 0x32;
            byte protocolId = 0xf2;
            byte msgType = 0x00;

            Random rnd = new Random();
            byte[] b = new byte[12];
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
            Assert.AreEqual((b[4] << 8) | b[5], msg.IcdInfo.Size, "Ошибка в парсинге свойства IcdInfo.Size");

            // проверяем результаты парсинга Obt
            Assert.AreEqual(b[6], msg.ObtInfo.Normal, "Ошибка в парсинге свойства ObtInfo.Normal");
            Assert.AreEqual(b[7], msg.ObtInfo.Extended, "Ошибка в парсинге свойства ObtInfo.Extended");
            Assert.AreEqual((uint)SpacewireIcdMsgEventArgs.ConvertToInt(new byte[4] { b[11], b[10], b[9], b[8] }), msg.ObtInfo.Value, "Ошибка в парсинге свойства ObtInfo.Value");
        }
    }

    /// <summary>
    /// Организует тестирование TimeTick сообщений.
    /// </summary>
    [TestClass]
    public class SpacewireTimeTickMsgEventArgsTest
    {
        /// <summary>
        /// Spacewires the time tick MSG event args_ income3 bytes_ exception thrown.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireTimeTickMsgEventArgs_Income3Bytes_ExceptionThrown()
        {
            // формируем массив из 3 байт и проверяем формирование посылки
            byte[] buf = new byte[3] { 0x33, 0xf2, 0x34 };
            SpacewireTimeTickMsgEventArgs msg = new SpacewireTimeTickMsgEventArgs(buf, 0x00, 0x00, 0x00);
        }

        /// <summary>
        /// Spacewires the time tick MSG event args_ income4 bytes_ exception thrown.
        /// </summary>
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

        /// <summary>
        /// Spacewires the time tick MSG event args_ income1 bytes_ returns equal token value.
        /// </summary>
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

        /// <summary>
        /// Spacewires the time tick MSG event args_ income0 bytes_ exception thrown.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireTimeTickMsgEventArgs_Income0Bytes_ExceptionThrown()
        {
            // имитируем ошибочку
            byte[] buf = new byte[] { };
            SpacewireTimeTickMsgEventArgs msg = new SpacewireTimeTickMsgEventArgs(buf, 0x00, 0x00, 0x00);
        }

        /// <summary>
        /// Spacewires the time tick MSG event args_1 argument null_ exception thrown.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SpacewireTimeTickMsgEventArgs_1ArgNull_ExceptionThrown()
        {
            // имитируем ошибочку
            SpacewireTimeTickMsgEventArgs msg = new SpacewireTimeTickMsgEventArgs(null, 0x00, 0x00, 0x00);
        }
    }

    /// <summary>
    /// Организует тестирование Sptp сообщений.
    /// </summary>
    [TestClass]
    public class SpacewireSptpMsgEventArgsTest
    {
        /// <summary>
        /// Spacewires the SPTP MSG event args_ income3 bytes_ exception thrown.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireSptpMsgEventArgs_Income3Bytes_ExceptionThrown()
        {
            // формируем массив из 3 байт и проверяем формирование посылки
            byte[] buf = new byte[3] { 0x33, 0xf2, 0x34 };
            SpacewireSptpMsgEventArgs msg = new SpacewireSptpMsgEventArgs(buf, 0x00, 0x00, 0x00);
        }

        /// <summary>
        /// Spacewires the SPTP MSG event args_ get new_ returns equal token value.
        /// </summary>
        [TestMethod]
        public void SpacewireSptpMsgEventArgs_GetNew_ReturnsEqualTokenValue()
        {
            byte to = 0x77;
            byte from = 0x66;

            Random rnd = new Random();
            byte[] buf = new byte[6];
            rnd.NextBytes(buf);

            SpacewireSptpMsgEventArgs msg = SpacewireSptpMsgEventArgs.GetNew(buf, to, from);

            CollectionAssert.AreEqual(buf, msg.Data, "Ошибка в парсинге данных кадра");
            Assert.AreEqual(to, msg.SptpInfo.To, "Ошибка в парсинге свойства SptpInfo.To");
            Assert.AreEqual(SpacewireSptpMsgEventArgs.SptpProtocol.Standard, msg.SptpInfo.ProtocolId, "Ошибка в парсинге свойства SptpInfo.ProtocolId");
            Assert.AreEqual(SpacewireSptpMsgEventArgs.SptpType.Data, msg.SptpInfo.MsgType, "Ошибка в парсинге свойства SptpInfo.MsgType");
            Assert.AreEqual(from, msg.SptpInfo.From, "Ошибка в парсинге свойства SptpInfo.From");

            string str = msg.SptpInfo.ToString(false);
        }

        /// <summary>
        /// Spacewires the SPTP MSG event args_ income4 bytes_ returns equal token value.
        /// </summary>
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

        /// <summary>
        /// Spacewires the SPTP MSG event args_ income5 bytes_ returns equal token value.
        /// </summary>
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

        /// <summary>
        /// Spacewires the SPTP MSG event args_ income0 bytes_ exception thrown.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireSptpMsgEventArgs_Income0Bytes_ExceptionThrown()
        {            
            // имитируем ошибочку
            byte[] buf = new byte[] { };
            SpacewireSptpMsgEventArgs msg = new SpacewireSptpMsgEventArgs(buf, 0x00, 0x00, 0x00);
        }

        /// <summary>
        /// Spacewires the SPTP MSG event args_1 argument null_ exception thrown.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SpacewireSptpMsgEventArgs_1ArgNull_ExceptionThrown()
        {
            // имитируем ошибочку
            SpacewireSptpMsgEventArgs msg = new SpacewireSptpMsgEventArgs(null, 0x00, 0x00, 0x00);
        }

        /// <summary>
        /// Spacewires the SPTP MSG event args_ income65540 bytes_ returns equal token value.
        /// </summary>
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

        /// <summary>
        /// Spacewires the SPTP MSG event args_ to array_ returns equal token value.
        /// </summary>
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

    /// <summary>
    /// Организует тестирование EmptyProto сообщений.
    /// </summary>
    [TestClass]
    public class SpacewireEmptyProtoMsgEventArgsTest
    {
        /// <summary>
        /// Spacewires the empty proto MSG event args_ income5 bytes_ returns equal token value.
        /// </summary>
        [TestMethod]
        public void SpacewireEmptyProtoMsgEventArgs_Income5Bytes_ReturnsEqualTokenValue()
        {
            byte from = 0x33;
            byte to = 0x32;
            byte protocolId = 0xf2;
            byte msgType = 0x00;

            byte customData = 0xAB;

            // формируем массив из 5 байт и проверяем формирование посылки
            byte[] buf = new byte[5] { to, protocolId, msgType, from, customData };
            SpacewireEmptyProtoMsgEventArgs msg = new SpacewireEmptyProtoMsgEventArgs(buf, 0x00, 0x00, 0x00);

            // проверяем результат парсинга данных кадра
            CollectionAssert.AreEqual(buf, msg.Data, "Ошибка в парсинге данных кадра");
        }

        /// <summary>
        /// Spacewires the empty proto MSG event args_ income0 bytes_ exception thrown.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireEmptyProtoMsgEventArgs_Income0Bytes_ExceptionThrown()
        {
            // имитируем ошибочку
            byte[] buf = new byte[] { };
            SpacewireEmptyProtoMsgEventArgs msg = new SpacewireEmptyProtoMsgEventArgs(buf, 0x00, 0x00, 0x00);
        }

        /// <summary>
        /// Spacewires the empty proto MSG event args_1 argument null_ exception thrown.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SpacewireEmptyProtoMsgEventArgs_1ArgNull_ExceptionThrown()
        {
            // имитируем ошибочку
            SpacewireEmptyProtoMsgEventArgs msg = new SpacewireEmptyProtoMsgEventArgs(null, 0x00, 0x00, 0x00);
        }

        /// <summary>
        /// Spacewires the empty proto MSG event args_ income65540 bytes_ returns equal token value.
        /// </summary>
        [TestMethod]
        public void SpacewireEmptyProtoMsgEventArgs_Income65540Bytes_ReturnsEqualTokenValue()
        {
            RandomBufferGenerator generator = new RandomBufferGenerator(65540);
            byte[] buf = generator.GenerateBufferFromSeed(65540);

            // формируем максимально допустимую посылку для протокола spacewire.
            SpacewireEmptyProtoMsgEventArgs msg = new SpacewireEmptyProtoMsgEventArgs(buf, 0x00, 0x00, 0x00);

            // проверяем результат парсинга данных кадра
            CollectionAssert.AreEqual(buf.ToArray(), msg.Data, "Ошибка в парсинге данных кадра");
        }

        /// <summary>
        /// Spacewires the empty proto MSG event args_ to array_ returns equal token value.
        /// </summary>
        [TestMethod]
        public void SpacewireEmptyProtoMsgEventArgs_ToArray_ReturnsEqualTokenValue()
        {
            RandomBufferGenerator generator = new RandomBufferGenerator(65540);
            byte[] buf = generator.GenerateBufferFromSeed(65540);

            // формируем максимально допустимую посылку для протокола spacewire.
            SpacewireEmptyProtoMsgEventArgs msg = new SpacewireEmptyProtoMsgEventArgs(buf, 0x00, 0x00, 0x00);

            // проверяем результат преобразования к массиву
            CollectionAssert.AreEqual(buf, msg.ToArray(), "Ошибка в преобразовании к массиву");
        }
    }
}
