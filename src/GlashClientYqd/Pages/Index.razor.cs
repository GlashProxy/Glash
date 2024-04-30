using Microsoft.JSInterop;
using System.Threading.Tasks;
using YiQiDong.Agent;

namespace GlashClientYqd.Pages
{
    public partial class Index
    {
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            var imageInfo = AgentContext.Container?.Image;
            if (imageInfo == null)
                await setTitle("Glash客户端 Debug version");
            else
                await setTitle($"{imageInfo.Name} v{imageInfo.Version}");
        }

        private async Task setTitle(string title)
        {
            await JSRuntime.InvokeVoidAsync("setTitle", title);
        }
    }
}
