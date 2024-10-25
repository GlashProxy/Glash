using Quick.Blazor.Bootstrap;
using Quick.Protocol.Utils;
using System.ComponentModel.DataAnnotations;

namespace GlashAgent.Pages;

public partial class LoginPasswordManage
{
    public class PasswordManageModel
    {
        [Required(ErrorMessage = "请输入原密码")]
        public string OldPassword { get; set; }
        [Required(ErrorMessage = "请输入新密码")]
        public string NewPassword { get; set; }
        [Required(ErrorMessage = "请再次输入新密码")]
        public string NewPassword2 { get; set; }
    }

    private ModalAlert modalAlert;

    private PasswordManageModel passwordManageModel;

    protected override void OnInitialized()
    {
        passwordManageModel = new PasswordManageModel();

        base.OnInitialized();
    }

    private void ModifyPassword()
    {
        if (passwordManageModel.OldPassword != Core.LoginPasswordManager.Instance.LoginPassword)
        {
            modalAlert.Show("提示", "原密码不正确！", null, null);
            return;
        }
        if (passwordManageModel.NewPassword != passwordManageModel.NewPassword2)
        {
            modalAlert.Show("提示", "两次输入的新密码不匹配！", null, null);
            return;
        }
        try
        {
            Core.LoginPasswordManager.Instance.SetLoginPassword(passwordManageModel.NewPassword);
            passwordManageModel = new PasswordManageModel();
            modalAlert.Show("提示", "修改登录密码成功！", null, null);
        }
        catch (Exception ex)
        {
            modalAlert.Show("错误", "修改登录密码时出错，原因：" + ExceptionUtils.GetExceptionString(ex), null, null, true);
        }
    }
}
