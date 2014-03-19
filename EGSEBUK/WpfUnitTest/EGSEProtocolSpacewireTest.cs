namespace EGSE.Protocols.UnitTest
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using EGSE.Protocols;

    [TestClass]
    public class SpacewireSptpMsgEventArgsTest
    {
        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireSptpMsgEventArgs_Income3Bytes_ExceptionThrown()
        {
            // формируем массив из 3 байт и проверяем формирование посылки
            byte[] buf = new byte[3] { 0x33, 0xf2, 0x34 };
            SpacewireSptpMsgEventArgs msg = new SpacewireSptpMsgEventArgs(buf, buf.Length, 0x00, 0x00, 0x00);
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
            SpacewireSptpMsgEventArgs msg = new SpacewireSptpMsgEventArgs(buf, buf.Length, 0x00, 0x00, 0x00);

            // проверяем результаты парсинга заголовка

            Assert.AreEqual(to, msg.Info.To, "Ошибка в парсинге свойства To");            
            Assert.AreEqual(protocolId, msg.Info.ProtocolId, "Ошибка в парсинге свойства ProtocolId");
            Assert.AreEqual(msgType, (byte)msg.Info.MsgType, "Ошибка в парсинге свойства MsgType");
            Assert.AreEqual(from, msg.Info.From, "Ошибка в парсинге свойства From");
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
            SpacewireSptpMsgEventArgs msg = new SpacewireSptpMsgEventArgs(buf, buf.Length, 0x00, 0x00, 0x00);
           
            // проверяем результаты парсинга заголовка
            Assert.AreEqual(to, msg.Info.To, "Ошибка в парсинге свойства To");
            Assert.AreEqual(protocolId, msg.Info.ProtocolId, "Ошибка в парсинге свойства ProtocolId");
            Assert.AreEqual(msgType, (byte)msg.Info.MsgType, "Ошибка в парсинге свойства MsgType");
            Assert.AreEqual(from, msg.Info.From, "Ошибка в парсинге свойства From");

            // проверяем результат парсинга данных кадра
            CollectionAssert.AreEqual(new byte[1] { customData }, msg.Data, "Ошибка в парсинге данных кадра");
        }

        [TestMethod]
        [ExpectedException(typeof(ContextMarshalException))]
        public void SpacewireSptpMsgEventArgs_Income0Bytes_ExceptionThrown()
        {            
            // имитируем ошибочку
            byte[] buf = new byte[] { };
            SpacewireSptpMsgEventArgs msg = new SpacewireSptpMsgEventArgs(buf, buf.Length, 0x00, 0x00, 0x00);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SpacewireSptpMsgEventArgs_1ArgNull_ExceptionThrown()
        {
            // имитируем ошибочку
            SpacewireSptpMsgEventArgs msg = new SpacewireSptpMsgEventArgs(null, 0, 0x00, 0x00, 0x00);
        }

        [TestMethod]
        public void SpacewireSptpMsgEventArgs_Income65540Bytes_ReturnsEqualTokenValue()
        {
            RandomBufferGenerator generator = new RandomBufferGenerator(65540);
            byte[] buf = generator.GenerateBufferFromSeed(65540);
            // формируем максимально допустимую посылку для протокола spacewire.
            SpacewireSptpMsgEventArgs msg = new SpacewireSptpMsgEventArgs(buf, buf.Length, 0x00, 0x00, 0x00);

            // проверяем результат парсинга данных кадра
            CollectionAssert.AreEqual(buf.Skip(4).ToArray(), msg.Data, "Ошибка в парсинге данных кадра");
        }

        [TestMethod]
        public void SpacewireSptpMsgEventArgs_ToArray_ReturnsEqualTokenValue()
        {
            RandomBufferGenerator generator = new RandomBufferGenerator(65540);
            byte[] buf = generator.GenerateBufferFromSeed(65540);
            // формируем максимально допустимую посылку для протокола spacewire.
            SpacewireSptpMsgEventArgs msg = new SpacewireSptpMsgEventArgs(buf, buf.Length, 0x00, 0x00, 0x00);

            // проверяем результат преобразования к массиву
            CollectionAssert.AreEqual(buf, msg.ToArray(), "Ошибка в преобразовании к массиву");
        }

        public class RandomBufferGenerator
        {
            private readonly Random _random = new Random();
            private readonly byte[] _seedBuffer;

            public RandomBufferGenerator(int maxBufferSize)
            {
                _seedBuffer = new byte[maxBufferSize];

                _random.NextBytes(_seedBuffer);
            }

            public byte[] GenerateBufferFromSeed(int size)
            {
                int randomWindow = _random.Next(0, size);

                byte[] buffer = new byte[size];

                Buffer.BlockCopy(_seedBuffer, randomWindow, buffer, 0, size - randomWindow);
                Buffer.BlockCopy(_seedBuffer, 0, buffer, size - randomWindow, randomWindow);

                return buffer;
            }
        }
    }
}
