#region Copyright
/*
 * Copyright 2014 Angelo Simone Scotto
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * 
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
#endregion

using MbUnit.Framework;
using FakeItEasy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Apache.NMS.Reactive;

namespace Apache.NMS.Reactive.Tests
{
    [TestFixture()]
    public class NMSTestStub
    {
        [Test()]
        public void TestNormalExecution()
        {
            var factory = A.Fake<IConnectionFactory>();
            var connection = A.Fake<IConnection>();
            var session = A.Fake<ISession>();
            var destination = A.Fake<IDestination>();
            var consumer = A.Fake<IMessageConsumer>();
            var msg = A.Dummy<IMessage>();

            A.CallTo(() => factory.CreateConnection()).Returns(connection);
            A.CallTo(() => connection.CreateSession()).Returns(session);

            A.CallTo(() => session.CreateConsumer(A<IDestination>.Ignored)).Returns(consumer);

            var observable = Apache.NMS.Reactive.Observable.Create<IMessage>(factory,"TestDestination");

            var onNext = new List<string>();
            var onCompleted = false;
            var onError = new List<Exception>();

            var disposable = observable.Subscribe(
                (item) => { onNext.Add(item.ToString()); },
                (error) => { onError.Add(error); },
                () => { onCompleted = true; }
                );

            consumer.Listener += Raise.With<MessageListener>(msg);
            consumer.Listener += Raise.With<MessageListener>(msg);
            consumer.Listener += Raise.With<MessageListener>(msg);

            System.Threading.Thread.Sleep(1000);

            Assert.AreEqual(3, onNext.Count);
            Assert.AreEqual(false, onCompleted);
            Assert.IsEmpty(onError);
            Assert.IsInstanceOfType<IDisposable>(disposable);

        }

        [Test()]
        public void TestException()
        {
            var factory = A.Fake<IConnectionFactory>();
            var connection = A.Fake<IConnection>();
            var session = A.Fake<ISession>();
            var destination = A.Fake<IDestination>();
            var consumer = A.Fake<IMessageConsumer>();
            var msg = A.Dummy<IMessage>();

            A.CallTo(() => factory.CreateConnection()).Returns(connection);
            A.CallTo(() => connection.CreateSession()).Returns(session);

            A.CallTo(() => session.CreateConsumer(A<IDestination>.Ignored)).Returns(consumer);

            var observable = Apache.NMS.Reactive.Observable.Create<IMessage>(factory, "TestDestination");

            var onNext = new List<string>();
            var onCompleted = false;
            var onError = new List<Exception>();

            var disposable = observable.Subscribe(
                (item) => { onNext.Add(item.ToString()); }, 
                (error) => { onError.Add(error); }, 
                () => { onCompleted = true; }
                );

            var raisedError = new Exception();
            consumer.Listener += Raise.With<MessageListener>(msg);
            consumer.Listener += Raise.With<MessageListener>(msg);
            connection.ExceptionListener += Raise.With<ExceptionListener>(raisedError);
            consumer.Listener += Raise.With<MessageListener>(msg);

            System.Threading.Thread.Sleep(1000);

            Assert.AreEqual(2, onNext.Count);
            Assert.IsFalse(onCompleted);
            Assert.Contains(onError, raisedError);

            Assert.IsInstanceOfType<IDisposable>(disposable);
            
        }

        [Test()]
        public void TestDispose()
        {
            var factory = A.Fake<IConnectionFactory>();
            var connection = A.Fake<IConnection>();
            var session = A.Fake<ISession>();
            var destination = A.Fake<IDestination>();
            var consumer = A.Fake<IMessageConsumer>();
            var msg = A.Dummy<IMessage>();

            A.CallTo(() => factory.CreateConnection()).Returns(connection);
            A.CallTo(() => connection.CreateSession()).Returns(session);

            A.CallTo(() => session.CreateConsumer(A<IDestination>.Ignored)).Returns(consumer);

            var observable = Apache.NMS.Reactive.Observable.Create<IMessage>(factory, "TestDestination");

            var onNext = new List<string>();
            var onCompleted = false;
            var onError = new List<Exception>();

            var disposable = observable.Subscribe(
                (item) => { onNext.Add(item.ToString()); },
                (error) => { onError.Add(error); },
                () => { onCompleted = true; }
                );

            var raisedError = new Exception();
            consumer.Listener += Raise.With<MessageListener>(msg);
            consumer.Listener += Raise.With<MessageListener>(msg);

            disposable.Dispose();
            consumer.Listener += Raise.With<MessageListener>(msg);

            System.Threading.Thread.Sleep(1000);

            Assert.AreEqual(2, onNext.Count);
            Assert.IsFalse(onCompleted);
            Assert.DoesNotContain(onError, raisedError);

            Assert.IsInstanceOfType<IDisposable>(disposable);

        }

    }
}
