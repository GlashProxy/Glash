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
                QpCommandInfo.Create(new QpCommands.Login.Request()),
                QpCommandInfo.Create(new QpCommands.GetAgentList.Request()),
                QpCommandInfo.Create(new QpCommands.GetProxyRuleList.Request()),
                QpCommandInfo.Create(new QpCommands.SaveProxyRule.Request()),
                QpCommandInfo.Create(new QpCommands.DeleteProxyRule.Request()),
                QpCommandInfo.Create(new QpCommands.CreateTunnel.Request()),
                QpCommandInfo.Create(new QpCommands.StartTunnel.Request())
            },
            NoticeInfos = new[]
            {
                QpNoticeInfo.Create(new G.D()),
                QpNoticeInfo.Create(new TunnelClosed()),
                QpNoticeInfo.Create(new QpNotices.AgentLoginStatusChanged())
            }
        };
    }
}