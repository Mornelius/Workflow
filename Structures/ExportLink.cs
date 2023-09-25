namespace Workflow.Structures
{
    public class ExportLink
    {
        public string ID
        {
            get; set; 
        }

        public string StartPortID
        {
            get; set; 
        }

        public string EndPortID
        {
            get; set;
        }

        public List<ExportVertice> Vertices
        {
            get; set;
        } = new List<ExportVertice>();
    }
}
