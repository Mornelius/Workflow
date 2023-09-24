
using Blazor.Diagrams;
using Blazor.Diagrams.Algorithms;
using Blazor.Diagrams.Core;
using Blazor.Diagrams.Core.Anchors;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Models.Base;
using Blazor.Diagrams.Core.Options;
using Blazor.Diagrams.Core.PathGenerators;
using Blazor.Diagrams.Core.Routers;
using Blazor.Diagrams.Models;
using Blazor.Diagrams.Options;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using System.Xml.Linq;
using Workflow.Components;
using Workflow.Models;
using Workflow.Structures;
using static Workflow.Structures.Enumerators;

namespace Workflow.Pages
{
    public partial class FlowDiagram
    {
        private InputText propID;
        private InputText propName;
        private InputTextArea propDescription;


        private bool IsDiagramValid
        {
            get; set;
        } = false;

        private string ValidationReason
        {
            get; set;
        } = "";

        private double ZoomLevel
        {
            get; set;
        } = 0;

        // Properties
        private string ID
        {
            get; set;
        }

        private string name = "";
        private string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                foreach (NodeItem node in nodes)
                {
                    if (node.ID == propID.Value)
                    {
                        node.Name = name;
                        break;
                    }
                }
            }
        }


        private string description = "";
        private string Description
        {
            get
            {
                return description;
            }
            set
            {
                description = value;
                foreach (NodeItem node in nodes)
                {
                    if (node.ID == propName.Value)
                    {
                        node.Description = description;
                        break;
                    }
                }
            }
        }

        private double PosX
        {
            get; set; 
        }

        private double PosY
        {
            get; set;
        }

        private double Width
        {
            get; set;
        }

        private double Height
        {
            get; set;
        }


        private const int defaultX = 400;
        private const int defaultY = 200;

        private int nodeCounter = 0; // Keep kount of the number of nodes
        private List<NodeItem> nodes = new List<NodeItem>();

        private BlazorDiagram Diagram = null;

        private ShapeType? draggedType;

        protected override async Task OnInitializedAsync()
        {
            // Set up basic options for the diagram canvas
            var options = new BlazorDiagramOptions
            {
                AllowMultiSelection = true,
                Zoom =
            {
                Enabled = true,
            },
                Links =
            {
                DefaultRouter = new OrthogonalRouter(),
                DefaultPathGenerator = new StraightPathGenerator()
            },
            };

            Diagram = new BlazorDiagram(options);
            Diagram.ZoomChanged += Diagram_ZoomChanged;
            Diagram.Links.Added += Links_Added;
            Diagram.Nodes.Removed += Nodes_Removed;
            Diagram.PointerUp += Diagram_PointerUp;
            Diagram.Links.Removed += Links_Removed;
            Diagram.SelectionChanged += Diagram_SelectionChanged;

            // We created new custom shapes - they need to be registered on the canvas before they can be used
            Diagram.RegisterComponent<StartModel, Start>();
            Diagram.RegisterComponent<EndModel, End>();
            Diagram.RegisterComponent<RectangleModel, Components.Rectangle>();
            Diagram.RegisterComponent<DiamondModel, Diamond>();
            Diagram.RegisterComponent<TriangleModel, Triangle>();
            
            ZoomLevel = Diagram.Zoom;

            ValidateDiagram();
        }

        private void Diagram_SelectionChanged(SelectableModel obj)
        {
            ID = "";
            Name = "";
            Description = "";
            PosX = 0;
            PosY = 0;
            Width = 0;
            Height = 0;

            foreach (NodeItem node in nodes)
            {
                if (node.ID == obj.Id)
                {
                    ID = obj.Id;
                    Name = node.Name;
                    Description = node.Description;

                    switch (node.Type)
                    {
                        case ShapeType.Start:
                            PosX = ((StartModel) obj).Position.X;
                            PosY = ((StartModel) obj).Position.Y;
                            Width = ((StartModel) obj).Size.Width;
                            Height = ((StartModel) obj).Size.Height;
                            break;
                        case ShapeType.End:
                            PosX = ((EndModel) obj).Position.X;
                            PosY = ((EndModel) obj).Position.Y;
                            Width = ((EndModel) obj).Size.Width;
                            Height = ((EndModel) obj).Size.Height;
                            break;
                        case ShapeType.Diamond:
                            PosX = ((DiamondModel) obj).Position.X;
                            PosY = ((DiamondModel) obj).Position.Y;
                            Width = ((DiamondModel) obj).Size.Width;
                            Height = ((DiamondModel) obj).Size.Height;
                            break;
                        case ShapeType.Rectangle:
                            PosX = ((RectangleModel) obj).Position.X;
                            PosY = ((RectangleModel) obj).Position.Y;
                            Width = ((RectangleModel) obj).Size.Width;
                            Height = ((RectangleModel) obj).Size.Height;
                            break;
                        case ShapeType.Triangle:
                            PosX = ((TriangleModel) obj).Position.X;
                            PosY = ((TriangleModel) obj).Position.Y;
                            Width = ((TriangleModel) obj).Size.Width;
                            Height = ((TriangleModel) obj).Size.Height;
                            break;
                        case ShapeType.Normal:
                            PosX = ((SvgNodeModel) obj).Position.X;
                            PosY = ((SvgNodeModel) obj).Position.Y;
                            Width = ((SvgNodeModel) obj).Size.Width;
                            Height = ((SvgNodeModel) obj).Size.Height;
                            break;
                        default:
                            break;
                    }
                    break;
                }
            }
        }

        private void Links_Removed(BaseLinkModel obj)
        {
            ValidateDiagram();
        }

        private void Diagram_PointerUp(Model? arg1, Blazor.Diagrams.Core.Events.PointerEventArgs arg2)
        {
            ValidateDiagram();

           
        }

        private void Nodes_Removed(NodeModel obj)
        {
            NodeItem delete = null;

            foreach (NodeItem item in nodes)
            {
                if(item.ID == obj.Id)
                {
                    delete = item;
                    break;
                }
            }

            if (delete != null)
            {
                nodes.Remove(delete);
            }

            ValidateDiagram();
        }


        /// <summary>
        /// Define what a link looks like when it is added
        /// </summary>
        /// <param name="obj"></param>
        private void Links_Added(Blazor.Diagrams.Core.Models.Base.BaseLinkModel obj)
        {
            obj.PathGenerator = new StraightPathGenerator();
            obj.Router = new OrthogonalRouter();
            obj.TargetMarker = LinkMarker.Arrow;
            obj.Segmentable = true;
            ValidateDiagram();
        }

        /// <summary>
        /// Site closing down, release all event handlers to prevent memory leaks
        /// </summary>
        public void Dispose()
        {
            Diagram.ZoomChanged -= Diagram_ZoomChanged;
            Diagram.Links.Added -= Links_Added;
            Diagram.Nodes.Removed -= Nodes_Removed;
            Diagram.PointerUp -= Diagram_PointerUp;
            Diagram.Links.Removed -= Links_Removed;
            Diagram.SelectionChanged -= Diagram_SelectionChanged;
        }

        private void Diagram_ZoomChanged()
        {
            ZoomLevel = Diagram.Zoom;
            StateHasChanged();
        }

        /// <summary>
        /// Add a new node to the diagram
        /// </summary>
        /// <param name="x">x-position of the node</param>
        /// <param name="y">y-position of the node</param>
        /// <param name="type">The node ShapeType</param>
        /// <returns>An appropriate node model</returns>
        private NodeModel NewNode(double x, double y, ShapeType type)
        {
            nodeCounter++;

            switch (type)
            {
                case ShapeType.Start:
                    var startNode = new StartModel(new Point(x, y));
                    startNode.Position = new Point(x + startNode.PositionOffSetX, y + startNode.PositionOffSetY);
                    startNode.AddPort(PortAlignment.Top);
                    startNode.AddPort(PortAlignment.Bottom);
                    startNode.AddPort(PortAlignment.Left);
                    startNode.AddPort(PortAlignment.Right);
                    nodes.Add(new NodeItem(startNode.Id, ShapeType.Start, "Start_" + nodeCounter.ToString(), "Start Node"));
                    return startNode;
                case ShapeType.End:
                    var endNode = new EndModel(new Point(x, y));
                    endNode.Position = new Point(x + endNode.PositionOffSetX, y + endNode.PositionOffSetY);
                    endNode.AddPort(PortAlignment.Top);
                    endNode.AddPort(PortAlignment.Bottom);
                    endNode.AddPort(PortAlignment.Left);
                    endNode.AddPort(PortAlignment.Right);
                    nodes.Add(new NodeItem(endNode.Id, ShapeType.End, "End_" + nodeCounter.ToString(), "End Node"));
                    return endNode;
                case ShapeType.Triangle:
                    var triangleNode = new TriangleModel(new Point(x, y));
                    triangleNode.Position = new Point(x + triangleNode.PositionOffSetX, y + triangleNode.PositionOffSetY);
                    triangleNode.AddPort(PortAlignment.Top);
                    triangleNode.AddPort(PortAlignment.Left);
                    triangleNode.AddPort(PortAlignment.Right);
                    nodes.Add(new NodeItem(triangleNode.Id, ShapeType.Triangle, "Triangle_" + nodeCounter.ToString(), "Node"));
                    return triangleNode;
                case ShapeType.Rectangle:
                    var rectangleNode = new RectangleModel(new Point(x, y));
                    rectangleNode.Position = new Point(x + rectangleNode.PositionOffSetX, y + rectangleNode.PositionOffSetY);
                    rectangleNode.AddPort(PortAlignment.Top);
                    rectangleNode.AddPort(PortAlignment.Bottom);
                    rectangleNode.AddPort(PortAlignment.Left);
                    rectangleNode.AddPort(PortAlignment.Right);
                    nodes.Add(new NodeItem(rectangleNode.Id, ShapeType.Rectangle, "Rectangle_" + nodeCounter.ToString(), "Node"));
                    return rectangleNode;
                case ShapeType.Diamond:
                    var diamondNode = new DiamondModel(new Point(x, y));
                    diamondNode.Position = new Point(x + diamondNode.PositionOffSetX, y + diamondNode.PositionOffSetY);
                    diamondNode.AddPort(PortAlignment.Top);
                    diamondNode.AddPort(PortAlignment.Bottom);
                    diamondNode.AddPort(PortAlignment.Left);
                    diamondNode.AddPort(PortAlignment.Right);
                    nodes.Add(new NodeItem(diamondNode.Id, ShapeType.Diamond, "Diamond_" + nodeCounter.ToString(), "Node"));
                    return diamondNode;
                default:
                    var node = new NodeModel(new Point(x, y));
                    node.AddPort(PortAlignment.Left);
                    node.AddPort(PortAlignment.Right);
                    nodes.Add(new NodeItem(node.Id, ShapeType.Normal, "Normal_" + nodeCounter.ToString(), "Node"));
                    return node;
            }
        }

        private void Node_Clicked(object? sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Add a new node to the diagram on a double-click
        /// </summary>
        /// <param name="type">The node ShapeType</param>
        /// <returns>An appropriate node model</returns>
        private void OnElementDblClick(ShapeType type)
        {
            int x = (int) defaultX;
            int y = (int) defaultY;

            Diagram.Nodes.Add(NewNode(x, y, type));

            ValidateDiagram();
        }

        /// <summary>
        /// Clear the Diagram canvas, removing all nodes, links etc.
        /// </summary>
        /// <returns></returns>
        private async Task Clear()
        {
            Diagram.Nodes.Clear();
            nodeCounter = 0;
            nodes.Clear();
            ValidateDiagram();
        }

        /// <summary>
        /// Clear the Diagram canvas, and add a start and end node
        /// </summary>
        /// <returns></returns>
        private async Task Template()
        {
            Diagram.Nodes.Clear();
            nodeCounter = 0;
            nodes.Clear();
            Diagram.Nodes.Add(NewNode(Diagram.Container.Left - 100, Diagram.Container.Center.Y - 60, ShapeType.Start));
            Diagram.Nodes.Add(NewNode(Diagram.Container.Right - 300, Diagram.Container.Center.Y - 60, ShapeType.End));
            ValidateDiagram();
        }

        /// <summary>
        /// Reconnect links to closest nodes
        /// </summary>
        private void ReconnectLinks()
        {
            Diagram.ReconnectLinksToClosestPorts();
            ValidateDiagram();
        }

        /// <summary>
        /// Start dragging an element from the pallette
        /// </summary>
        /// <param name="key"></param>
        private void OnDragStart(ShapeType key)
        {
            draggedType = key;
        }

        /// <summary>
        /// Drop a dragged element onto the canvas
        /// </summary>
        /// <param name="e"></param>
        private void OnDrop(DragEventArgs e)
        {
            if (draggedType == null) // Unkown item
                return;

            var position = Diagram.GetRelativeMousePoint(e.ClientX, e.ClientY);

            Diagram.Nodes.Add(NewNode(position.X, position.Y, (ShapeType) draggedType));

            draggedType = null;

            ValidateDiagram();
        }

        private void ValidateDiagram()
        {
            int startCount = 0;
            int endCount = 0;

            ValidationReason = "";

            foreach (NodeItem node in nodes)
            {
                if (node.Type == ShapeType.Start)
                {
                    startCount++;
                }
                if (node.Type == ShapeType.End)
                {
                    endCount++;
                }
            }

            if (startCount == 1 && endCount == 1)
            {
                IsDiagramValid = true;
            }
            else
            {
                IsDiagramValid = false;

                if (startCount < 1)
                {
                    ValidationReason = ValidationReason + "&#x2022 Diagram must have a Start node.<br>";
                }

                if (startCount > 1)
                {
                    ValidationReason = ValidationReason + "&#x2022 Diagram can have only one Start node.<br>";
                }

                if (endCount < 1)
                {
                    ValidationReason = ValidationReason + "&#x2022 Diagram must have an End node.<br>";
                }

                if (endCount > 1)
                {
                    ValidationReason = ValidationReason + "&#x2022 Diagram can have only one End node.<br>";
                }
            }

            foreach (NodeModel node in Diagram.Nodes)
            {
                int linked = 0;

                foreach (PortModel port in node.Ports)
                {
                    if (port.Links.Count > 0)
                    {
                        linked++;
                    }
                }

                if (linked == 0)
                {
                    IsDiagramValid = false;
                    ValidationReason = ValidationReason + "&#x2022 All nodes must have at least one link.<br>";
                    break;
                }
            }

            foreach (BaseLinkModel link in Diagram.Links)
            {
                if (!link.IsAttached)
                {
                    IsDiagramValid = false;
                    ValidationReason = ValidationReason + "&#x2022 All links must have a start and end element.<br>";
                    break;
                }
            }

            if (IsDiagramValid)
            {
                ValidationReason = "<b>No Errors</b><br><br>&#x2022 Diagram is valid<br>";
            }
            else
            {
                ValidationReason = "<b>Errors Found</b><br><br>" + ValidationReason;
            }

            StateHasChanged();
        }
    }
}
