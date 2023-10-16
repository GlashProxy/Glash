using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace Glash.Blazor.Client.ProxyTypes
{

    [ProxyType(typeof(Texts), nameof(Texts.ProxyTypeName))]
    public class Database : AbstractProxyType<Database_UI>
    {
        public enum Texts
        {
            ProxyTypeName,
            NetType,
            Library,
            User,
            Password,
            ButtonStartView
        }

        /*
Network protocol type:
0 = MariaDB/MySQL (TCP/IP)
1 = MariaDB/MySQL (named pipe)
2 = MariaDB/MySQL (SSH tunnel)
3 = MSSQL (named pipe)
4 = MSSQL (TCP/IP)
5 = MSSQL (SPX/IPX)
6 = MSSQL (Banyan VINES)
7 = MSSQL (Windows RPC)
8 = PostgreSQL (TCP/IP)
9 = PostgreSQL (SSH tunnel)
10 = SQLite
11 = ProxySQL Admin
12 = Interbase (TCP/IP)
13 = Interbase (local)
14 = Firebird (TCP/IP)
15 = Firebird (local)
         */
        [Required]
        public string NetType { get; set; }
        /*
Library or provider (added in v11.1):
MySQL/MariaDB:
    libmariadb.dll
    libmysql.dll
    libmysql-6.1.dll
MS SQL:
    MSOLEDBSQL
    SQLOLEDB
PostgreSQL:
    libpq.dll
    libpq-12.dll
SQLite:
    sqlite3.dll
Interbase:
    ibclient64-14.1.dll
    gds32-14.1.dll
Firebird:
    fbclient-4.0.dll
         */
        [Required]
        public string Library { get; set; }
        [Required]
        public string User { get; set; }
        [Required]
        public string Password { get; set; }

        public override string Icon => "fa fa-database";

        [SupportedOSPlatform("windows")]
        public override ProxyTypeButton[] GetButtons()
        {
            return new ProxyTypeButton[]
            {
                new ProxyTypeButton(
                    Global.Instance.TextManager.GetText(Texts.ButtonStartView),
                    "fa fa-database",
                     t=>
                     {
                         #pragma warning disable CA1416 // 验证平台兼容性
                        //从注册表中读取NSIS的安装目录
                        var regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\HeidiSQL_is1", false);
                        if (regKey == null)
                        {
                            Console.WriteLine("未检测到HeidiSQL，请安装HeidiSQL！");
                            return;
                        }
                        var installLocation = regKey.GetValue("InstallLocation").ToString();
                        var exeFile = Path.Combine(installLocation, "heidisql.exe");
#pragma warning restore CA1416 // 验证平台兼容性

                         var process = Process.Start(exeFile,$"--nettype={NetType} --library={Library} --host={GetLocalIPAddress(t.Config.LocalIPAddress)} --port={t.LocalPort} --user={User} --password={Password}");
                         WaitForProcessMainWindow(process);
                     }
                )
            };
        }
    }
}
