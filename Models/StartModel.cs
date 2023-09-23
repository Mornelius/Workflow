using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Models;
using System.Runtime.CompilerServices;

namespace Workflow.Models
{
    public class StartModel : SvgNodeModel
    {
        public StartModel(Point position = null) : base(position) { }

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
    }
}
