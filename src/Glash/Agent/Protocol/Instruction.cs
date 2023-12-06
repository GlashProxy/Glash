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
                QpNoticeInfo.Create(new G.D()),
                QpNoticeInfo.Create(new TunnelClosed())
            },
            CommandInfos = new[]
            {
                QpCommandInfo.Create(new QpCommands.Login.Request()),
                QpCommandInfo.Create(new QpCommands.CreateTunnel.Request()),
                QpCommandInfo.Create(new QpCommands.StartTunnel.Request())
            }
        };
    }
}