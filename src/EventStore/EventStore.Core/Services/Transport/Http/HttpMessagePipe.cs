﻿// Copyright (c) 2012, Event Store LLP
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are
// met:
// 
// Redistributions of source code must retain the above copyright notice,
// this list of conditions and the following disclaimer.
// Redistributions in binary form must reproduce the above copyright
// notice, this list of conditions and the following disclaimer in the
// documentation and/or other materials provided with the distribution.
// Neither the name of the Event Store LLP nor the names of its
// contributors may be used to endorse or promote products derived from
// this software without specific prior written permission
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// 
using System;
using System.Collections.Generic;
using System.Net;
using EventStore.Common.Utils;
using EventStore.Core.Messaging;

namespace EventStore.Core.Services.Transport.Http
{
    public class HttpMessagePipe
    {
        private readonly Dictionary<Type, IMessageForwarder> _forwarders = new Dictionary<Type, IMessageForwarder>();

        public void RegisterForwarder<T>(IForwarder<T> forwarder) where T : Message
        {
            Ensure.NotNull(forwarder, "forwarder");
            _forwarders.Add(typeof (T), new MessageForwarder<T>(forwarder));
        }

        public void Push(Message message, IPEndPoint endPoint)
        {
            Ensure.NotNull(message, "message");
            Ensure.NotNull(endPoint, "endPoint");

            var type = message.GetType();
            IMessageForwarder forwarder;

            if (_forwarders.TryGetValue(type, out forwarder))
                forwarder.Forward(message, endPoint);
        }
    }

    public interface IForwarder<in T> where T : Message
    {
        void Forward(T message, IPEndPoint endPoint);
    }

    public interface IMessageForwarder
    {
        void Forward(Message message, IPEndPoint endPoint);
    }

    public class MessageForwarder<T> : IMessageForwarder 
        where T : Message
    {
        private readonly IForwarder<T> _forwarder;

        public MessageForwarder(IForwarder<T> forwarder)
        {
            Ensure.NotNull(forwarder, "forwarder");
            _forwarder = forwarder;
        }

        public void Forward(Message message, IPEndPoint endPoint)
        {
            Ensure.NotNull(message, "message");
            Ensure.NotNull(endPoint, "endPoint");

            _forwarder.Forward((T) message, endPoint);
        }
    }
}