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

        protected internal Observable(Uri address, string destinationName) : this(new NMSConnectionFactory(address),destinationName)
        {
            Contract.Requires<ArgumentNullException>(address != null);
            Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(destinationName));
        }

        protected internal Observable(IConnectionFactory factory, string destinationName)
        {
            Contract.Requires<ArgumentNullException>(factory != null);
            Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(destinationName));
            Contract.Ensures(connection.IsStarted);

            // Create a Factory

            connection = factory.CreateConnection();
            session = connection.CreateSession();
            IDestination destination = SessionUtil.GetDestination(session, destinationName);
            connection.ExceptionListener += connection_ExceptionListener;
            consumer = session.CreateConsumer(destination);

            consumer.Listener += consumer_Listener;

            connection.Start();
        }

        private void connection_ExceptionListener(Exception exception)
        {
            Contract.Ensures(_observerList.Count == 0);

            _observerList.ToList().ForEach(item => item.OnError(exception));
        }
        

        private void consumer_Listener(IMessage message)
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
                _observerList.ToList().ForEach(item => item.OnError(err));
            }


            _observerList.ToList().ForEach(item =>
            {
                try { item.OnNext(msg); }
                catch (Exception err)
                {                         
                    // If OnNext raised an exception it means that a single observer has a problem, it not needs to bother other observer nor to stop further event processing.
                    // HACK: Sostituire con metodo leggero di logging (maybe Observable<Exception>?)
                    System.Diagnostics.Trace.WriteLine(err.ToString());
                }
            });

                
        }

        protected override IDisposable SubscribeCore(IObserver<T> observer)
        {
            Contract.Ensures(_observerList != null);

            Contract.Assume(observer != null);

                _observerList.Add(observer);

            return System.Reactive.Disposables.Disposable.Create(() => {  _observerList.Remove(observer); });
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

                    _observerList.ToList().ForEach(item => item.OnCompleted());

                }

            }
            disposed = true;
        }

        private bool disposed = false;



    }
}
