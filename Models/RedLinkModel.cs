using Blazor.Diagrams.Core.Models;

namespace Workflow.Models
{
    public class RedLinkModel : LinkModel
    {
        public RedLinkModel(PortModel sourcePort, PortModel targetPort = null) : base(sourcePort, targetPort) { }
    }
}
