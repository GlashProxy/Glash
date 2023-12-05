using Glash.Core;
using Quick.Protocol;

namespace Glash.Agent.Protocol
{
    public class Instruction
    {
        public static QpInstruction Instance = new QpInstruction()
        {
            Id = typeof(Instruction).Namespace,
            Name = "Glash Agent Protocol",
            NoticeInfos = new[]
            {
                QpNoticeInfo.Create<G.D>(QpNotices.AgentNoticesSerializerContext.Default),
                QpNoticeInfo.Create<TunnelClosed>(QpNotices.AgentNoticesSerializerContext.Default)
            },
            CommandInfos = new[]
            {
                QpCommandInfo.Create(new QpCommands.Login.Request(),
                    QpCommands.AgentLoginCommandSerializerContext.Default),
                QpCommandInfo.Create(new QpCommands.CreateTunnel.Request(),
                    QpCommands.AgentCreateTunnelCommandSerializerContext.Default),
                QpCommandInfo.Create(new QpCommands.StartTunnel.Request(),
                    QpCommands.AgentStartTunnelCommandSerializerContext.Default)
            }
        };
    }
}