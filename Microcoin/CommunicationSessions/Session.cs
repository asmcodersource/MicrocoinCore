using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.CommunicationSessions
{
    // TODO: change to NodeNet messages
    interface Message { }
    interface Node { }

    /// <summary>
    /// Implements communication across different sessions on top of one communication stream. 
    /// Allows you to conduct multiple threads of communication simultaneously.
    /// </summary>
    internal class Session
    {
        protected Queue<Message> messages = new Queue<Message>();
        protected event Action<Message> onMessageReceive = null;

        public short LocalSideId { get; protected set; }
        public short RemoteSideId { get; protected set; }


        public Session(short localSideId, short remoteSideId)
        {
            this.LocalSideId = localSideId;
            this.RemoteSideId = remoteSideId;
        }

        public void AddMessageToQueue(Message message)
        {
            lock (messages)
            {
                messages.Enqueue(message);
                onMessageReceive?.Invoke(message);
            }
        }

        public async Task<Message> GetMessage(CancellationToken token)
        {
            var taskCompletionSource = new TaskCompletionSource<Message>();
            try
            {
                lock (messages)
                {
                    if (messages.Count != 0)
                        return messages.Dequeue();
                    onMessageReceive += (message) =>
                    {
                        onMessageReceive = null;
                        taskCompletionSource.SetResult(messages.Dequeue());
                    };
                }
                return await taskCompletionSource.Task;
            } finally { 
                onMessageReceive = null;
            }
        }
    }
}
