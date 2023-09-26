using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Workflow.Shared
{
    public class ClipboardService
    {
        private readonly IJSRuntime jsRuntime;

        public ClipboardService(IJSRuntime jsRuntime)
        {
            this.jsRuntime = jsRuntime;
        }

        public async ValueTask WriteTextAsync(string text)
        {
            try
            {
                await jsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", text);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async ValueTask<string> ReadTextAsync()
        {
            try
            {
                return await jsRuntime.InvokeAsync<string>("navigator.clipboard.readText");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
    }
}
