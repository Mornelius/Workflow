using static Workflow.Structures.Enumerators;

namespace Workflow.Structures
{
    public class ExportNode
    {
        public string ID
        {
            get; set;
        }
        public string Name
        {
            get; set; 
        }
        public string Description
        {
            get; set;
        }
        public string Label
        {
            get; set;
        }
        public double XPosition
        {
            get; set;
        }
        public double YPosition
        {
            get; set;
        }
        public double Width
        {
            get; set;
        }
        public double Height
        {
            get; set;
        }

        public ShapeType Type
        {
            get; set;
        }

        public List<ExportPort> Ports
        {
            get; set;
        } = new List<ExportPort>();

    }
}
