using Glash.Core;
using Quick.Protocol;

namespace Glash.Client.Protocol
{
    public class Instruction
    {
        public static QpInstruction Instance = new QpInstruction()
        {
            Id = typeof(Instruction).Namespace,
            Name = "Glash Client Protocol",
            CommandInfos = new[]
            {
                QpCommandInfo.Create(new QpCommands.Login.Request(),
                    QpCommands.LoginCommandSerializerContext.Default),
                QpCommandInfo.Create(new QpCommands.GetAgentList.Request(),
                    QpCommands.GetAgentListCommandSerializerContext.Default),
                QpCommandInfo.Create(new QpCommands.GetProxyRuleList.Request(),
                    QpCommands.GetProxyRuleListCommandSerializerContext.Default),
                QpCommandInfo.Create(new QpCommands.SaveProxyRule.Request(),
                    QpCommands.SaveProxyRuleCommandSerializerContext.Default),
                QpCommandInfo.Create(new QpCommands.DeleteProxyRule.Request(),
                    QpCommands.DeleteProxyRuleCommandSerializerContext.Default),
                QpCommandInfo.Create(new QpCommands.CreateTunnel.Request(),
                    QpCommands.CreateTunnelCommandSerializerContext.Default),
                QpCommandInfo.Create(new QpCommands.StartTunnel.Request(),
                    QpCommands.StartTunnelCommandSerializerContext.Default)
            },
            NoticeInfos = new[]
            {
                QpNoticeInfo.Create<G.D>(QpNotices.ClientNoticesSerializerContext.Default),
                QpNoticeInfo.Create<TunnelClosed>(QpNotices.ClientNoticesSerializerContext.Default),
                QpNoticeInfo.Create<QpNotices.AgentLoginStatusChanged>(QpNotices.ClientNoticesSerializerContext.Default)
            }
        };
    }
}