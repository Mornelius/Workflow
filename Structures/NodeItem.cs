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
        }

        public NodeItem(string id, Enumerators.ShapeType type)
        {
            ID = id;
            Type = type;
        }

        public NodeItem(string id, Enumerators.ShapeType type, string name)
        {
            ID = id;
            Type = type;
            Name = name;
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

    }
}
