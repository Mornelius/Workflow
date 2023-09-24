using System.Xml.Linq;

namespace Workflow.Structures
{
    public class NodeItem
    {
        public NodeItem()
        {
        }

        public NodeItem(string id)
        {
            ID = id;
            Type = Enumerators.ShapeType.Normal;
            Name = id;
            Description = id;
        }

        public NodeItem(string id, Enumerators.ShapeType type)
        {
            ID = id;
            Type = type;
            Name = id;
            Description = id;
        }

        public NodeItem(string id, Enumerators.ShapeType type, string name)
        {
            ID = id;
            Type = type;
            Name = name;
            Description = name;
        }

        public NodeItem(string id, Enumerators.ShapeType type, string name, string desription)
        {
            ID = id;
            Type = type;
            Name = name;
            Description = desription;
        }

        public string ID
        {
            get;
            set;
        }

        public Enumerators.ShapeType Type
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }
    }
}
