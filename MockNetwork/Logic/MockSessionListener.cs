using Microcoin.Microcoin.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockNetwork.Logic
{
    public class MockSessionListener : ISessionListener
    {
        public class ListenerInfo
        {
            public string Resource { get; init; }
            public Func<ISessionConnection, Task> OnConnection { get; init; }
            public CancellationToken CancellationToken { get; init; }

            public ListenerInfo(string resource, Func<ISessionConnection, Task> onConnection, CancellationToken cancellationToken)
            {
                Resource = resource;
                OnConnection = onConnection;
                CancellationToken = cancellationToken;
            }
        }

        public readonly MockBroadcastNode ParentNode;
        public readonly Dictionary<string, ListenerInfo> ActiveListeners = new Dictionary<string, ListenerInfo>();

        public MockSessionListener(MockBroadcastNode parentNode)
        {
            ParentNode = parentNode;
        }

        public Task StartListeningAsync(Func<ISessionConnection, Task> onConnection, string resource, CancellationToken cancellationToken)
        {
            lock (this)
            {
                if (ActiveListeners.ContainsKey(resource))
                    throw new InvalidOperationException("Listener for this resource already existing");
                ActiveListeners.Add(resource, new ListenerInfo(resource, onConnection, cancellationToken));
            }
            return Task.CompletedTask;
        }

        public static async Task<ISessionConnection> HandleConnectionAttempt(MockSessionListener sessionListener, string resource)
        {
            if (sessionListener.ActiveListeners.ContainsKey(resource) is not true)
                throw new InvalidOperationException("Listener don't have corresponding handle connection");

            var listenerInfo = sessionListener.ActiveListeners[resource];
            var serverSideConnection = new MockSessionConnection();
            var clientSideConnection = new MockSessionConnection();
            serverSideConnection.OppositeSideConnection = clientSideConnection;
            clientSideConnection.OppositeSideConnection = serverSideConnection;

            Task.Run(async () => await listenerInfo.OnConnection.Invoke(serverSideConnection));
            return clientSideConnection;
        }
    }
}
