namespace GlashAgent.Core;

public class LoginPasswordManager
{
    public const string DEFAULT_LOGIN_PASSWORD = "admin";
    public static LoginPasswordManager Instance { get; } = new LoginPasswordManager();
    public string LoginPassword { get; private set; }


    public void Init()
    {
        LoginPassword = Glash.Blazor.Agent.Model.Config.GetConfig(nameof(LoginPassword));
        if (string.IsNullOrEmpty(LoginPassword))
        {
            Console.WriteLine($"初始化登录密码：{DEFAULT_LOGIN_PASSWORD}");
            SetLoginPassword(DEFAULT_LOGIN_PASSWORD);
        }
    }

    public void SetLoginPassword(string password)
    {
        Glash.Blazor.Agent.Model.Config.SetConfig(nameof(LoginPassword), password);
        LoginPassword = password;
    }
}
