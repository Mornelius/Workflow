using Blazor.Diagrams.Core.Models;

namespace Workflow.Structures
{
    public class ExportPort
    {
        public string ID
        {
            get; set; 
        }
        public PortAlignment PortAlignment
        {
            get; set; 
        }

        public List<ExportLink> exportLinks
        {
            get; set; 
        } = new List<ExportLink>();
    }
}
