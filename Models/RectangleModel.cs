using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Models;

namespace Workflow.Models
{
    public class RectangleModel : SvgNodeModel
    {
        public RectangleModel(Point position = null) : base(position) { }

        public string Label
        {
            get; set;
        } = "";
    }
}
