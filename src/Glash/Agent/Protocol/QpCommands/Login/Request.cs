using Quick.Protocol;
using System.ComponentModel;

namespace Glash.Agent.Protocol.QpCommands.Login
{
    [DisplayName("Login as Agent")]
    public class Request : IQpCommandRequest<Response>
    {
        public string Name { get; set; }
        public string Answer { get; set; }
    }
}
