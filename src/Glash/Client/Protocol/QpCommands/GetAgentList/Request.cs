using Quick.Protocol;
using System.ComponentModel;

namespace Glash.Client.Protocol.QpCommands.GetAgentList
{
    [DisplayName("Get Agent List")]
    public class Request : IQpCommandRequest<Response> { }
}
