using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace WebApplication5.Hubs
{
    public class CounterHub:Hub
    {

        static long counter = 0;

        public override System.Threading.Tasks.Task OnConnected()
        {
            counter = counter + 1;
            Clients.All.UpdateCount(counter);
            return base.OnConnected();
        }


        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        {

            counter = counter - 1;
            Clients.All.UpdateCount(counter);

            return base.OnDisconnected(stopCalled);
        }

    }
}
