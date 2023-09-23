using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Models;

namespace Workflow.Models
{
    public class StartModel : SvgNodeModel
    {
        public StartModel(Point position = null) : base(position) { }
    }
}
