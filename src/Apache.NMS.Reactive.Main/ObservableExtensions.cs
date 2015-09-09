using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics.Contracts;

namespace Apache.NMS.Reactive
{
    public static class Observable
    {
        public static System.IObservable<TResult> Create<TResult>(Uri address, string destinationName) where TResult : IMessage
        {
            Contract.Requires(address != null);
            Contract.Requires(!String.IsNullOrEmpty(destinationName));

            return new Apache.NMS.Reactive.Observable<TResult>(address, destinationName);
        }

        public static System.IObservable<TResult> Create<TResult>(IConnectionFactory factory, string destinationName) where TResult : IMessage
        {
            Contract.Requires(factory != null);
            Contract.Requires(!String.IsNullOrEmpty(destinationName));

            return new Apache.NMS.Reactive.Observable<TResult>(factory, destinationName);
        }

    }
}
