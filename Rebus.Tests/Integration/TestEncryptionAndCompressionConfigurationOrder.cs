﻿using System;
using NUnit.Framework;
using Rebus.Activation;
using Rebus.Compression;
using Rebus.Config;
using Rebus.Encryption;
using Rebus.Logging;
using Rebus.Transport.InMem;

namespace Rebus.Tests.Integration
{
    [TestFixture]
    public class TestEncryptionAndCompressionConfigurationOrder : FixtureBase
    {
        const string InputQueueName = "config-order";
        const string EncryptionKey = "gMPg8ySmshUk3gA+OnUNSUIrd253zQyUDJHW4359L3E=";
        readonly InMemNetwork _network = new InMemNetwork();

        [Test]
        public void CompressionFirst()
        {
            SetUpBus(o =>
            {
                o.EnableCompression();
                o.EnableEncryption(EncryptionKey);
            });

            var transportMessage = _network.GetNextOrNull(InputQueueName);

            Console.WriteLine($"Size: {transportMessage.Body.Length} bytes");
        }

        [Test]
        public void EncryptionFirst()
        {
            SetUpBus(o =>
            {
                o.EnableEncryption(EncryptionKey);
                o.EnableCompression();
            });

            var transportMessage = _network.GetNextOrNull(InputQueueName);

            Console.WriteLine($"Size: {transportMessage.Body.Length} bytes");
        }

        void SetUpBus(Action<OptionsConfigurer> configurer)
        {
            var handlerActivator = Using(new BuiltinHandlerActivator());

            var bus = Configure.With(handlerActivator)
                .Logging(l => l.Console(LogLevel.Warn))
                .Transport(t => t.UseInMemoryTransport(_network, InputQueueName))
                .Options(configurer)
                .Start();

            bus.SendLocal("hej med dig min ven!").Wait();
        }
    }
}