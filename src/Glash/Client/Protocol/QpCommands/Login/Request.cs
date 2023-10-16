using Quick.Protocol;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glash.Client.Protocol.QpCommands.Login
{
    [DisplayName("Login as Client")]
    public class Request : IQpCommandRequest<Response> 
    {
        public string Name { get; set; }
        public string Answer { get; set; }
    }
}
