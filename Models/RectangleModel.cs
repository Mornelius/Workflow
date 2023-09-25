using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Models;
using Workflow.Structures;

namespace Workflow.Models
{
    public class RectangleModel : SvgNodeModel
    {
        public RectangleModel(Point position = null) : base(position) { }

        public int PositionOffSetX
        {
            get
            {
                return -50;
            }
        }

        public int PositionOffSetY
        {
            get
            {
                return -30;
            }
        }


        public NodeItem Properties = new NodeItem();
    }
}
