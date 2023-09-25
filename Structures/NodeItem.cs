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
            Label = "";
        }

        public NodeItem(string id, Enumerators.ShapeType type)
        {
            ID = id;
            Type = type;
            Name = id;
            Description = id;
            Label = "";
        }

        public NodeItem(string id, Enumerators.ShapeType type, string name)
        {
            ID = id;
            Type = type;
            Name = name;
            Description = name;
            Label = "";
        }

        public NodeItem(string id, Enumerators.ShapeType type, string name, string desription)
        {
            ID = id;
            Type = type;
            Name = name;
            Description = desription;
            Label = "";
        }

        public NodeItem(string id, Enumerators.ShapeType type, string name, string desription, string label)
        {
            ID = id;
            Type = type;
            Name = name;
            Description = desription;
            Label = label;
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

        public string Label
        {
            get;
            set;
        }
    }
}
