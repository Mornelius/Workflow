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

        public ValueTask WriteTextAsync(string text)
        {
            return jsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", text);
        }

        public ValueTask<string> ReadTextAsync()
        {
            return jsRuntime.InvokeAsync<string>("navigator.clipboard.readText");
        }
    }
}
