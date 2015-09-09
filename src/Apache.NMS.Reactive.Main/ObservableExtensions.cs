﻿#region Copyright
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
