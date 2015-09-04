using Apache.NMS.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace Apache.NMS.Reactive
{
    public class Observable<T> : ObservableBase<T>, IDisposable where T : IMessage
    {
        

        SynchronizedCollection<IObserver<T>> _observerList = new SynchronizedCollection<IObserver<T>>();

        IConnection connection;
        ISession session ;
        IMessageConsumer consumer;

        public Observable(Uri address, string destinationName)
        {
            Contract.Requires<ArgumentNullException>(address != null);
            Contract.Requires<ArgumentException>(!String.IsNullOrEmpty( destinationName));
            Contract.Ensures(connection.IsStarted);

            // Create a Factory
            IConnectionFactory factory = new NMSConnectionFactory(address);

            connection = factory.CreateConnection();
            session = connection.CreateSession();            
            IDestination destination = SessionUtil.GetDestination(session, destinationName);

            consumer = session.CreateConsumer(destination);

            consumer.Listener += consumer_Listener;

            connection.Start();

            
        }

        void consumer_Listener(IMessage message)
        {
            Contract.Requires(message is T);
            Contract.Requires(_observerList != null);

            T msg = default(T);
            try
            {
                msg = (T)message;
            }
            catch(Exception err)
            {
                foreach (var item in _observerList)
                { 
                    item.OnError(err);
                }
                _observerList.Clear();
            }

            foreach (var item in _observerList)
            {
                item.OnNext(msg);
            }
            // TODO : Cosa succede se la OnNext solleva errore?
        }

        protected override IDisposable SubscribeCore(IObserver<T> observer)
        {
            Contract.Ensures(_observerList != null);

            Contract.Assume(observer != null);

            _observerList.Add(observer);
            return System.Reactive.Disposables.Disposable.Create(() => _observerList.Remove(observer));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {

            if (!disposed)
            {
                if (disposing)
                {
                    // Managed and unmanaged resources can be disposed.
                    session.Close();
                    connection.Close();
                    session.Dispose();
                    connection.Dispose();
                    consumer.Dispose();

                    foreach (var item in _observerList)
                    {
                        item.OnCompleted();
                    }

                    _observerList.Clear();

                }

            }
            disposed = true;
        }

        private bool disposed = false;
    }
}
