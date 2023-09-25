
using Blazor.Diagrams;
using Blazor.Diagrams.Algorithms;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Models.Base;
using Blazor.Diagrams.Core.PathGenerators;
using Blazor.Diagrams.Core.Routers;
using Blazor.Diagrams.Options;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Workflow.Components;
using Workflow.Models;
using Workflow.Structures;
using static Workflow.Structures.Enumerators;


namespace Workflow.Pages
{
    public partial class FlowDiagram
    {
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

        NodeItem? nodeItem
        {
            get; set;
        }
        = new NodeItem();

        private const int defaultX = 400;
        private const int defaultY = 200;

        private int nodeCounter = 0; // Keep kount of the number of nodes

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

            // Setup event handlers
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

        /// <summary>
        /// Update the properties when a new node is selected
        /// </summary>
        /// <param name="obj"></param>
        private void Diagram_SelectionChanged(SelectableModel obj)
        {
            nodeItem = null;

            foreach (NodeModel node in Diagram.Nodes)
            {
                if (node.Id == obj.Id)
                {
                    switch (node.GetType().Name)
                    {
                        case "StartModel":
                            StartModel startModel = node as StartModel;
                            nodeItem = new NodeItem(startModel.Id, ShapeType.Start, startModel.Properties.Name, startModel.Properties.Description, startModel.Properties.Label);
                            break;
                        case "EndModel":
                            EndModel endModel = node as EndModel;
                            nodeItem = new NodeItem(endModel.Id, ShapeType.Start, endModel.Properties.Name, endModel.Properties.Description, endModel.Properties.Label);
                            break;
                        case "DiamondModel":
                            DiamondModel diamondModel = node as DiamondModel;
                            nodeItem = new NodeItem(diamondModel.Id, ShapeType.Start, diamondModel.Properties.Name, diamondModel.Properties.Description, diamondModel.Properties.Label);
                            break;
                        case "RectangleModel":
                            RectangleModel rectangleModel = node as RectangleModel;
                            nodeItem = new NodeItem(rectangleModel.Id, ShapeType.Start, rectangleModel.Properties.Name, rectangleModel.Properties.Description, rectangleModel.Properties.Label);
                            break;
                        case "TriangleModel":
                            TriangleModel triangleModel = node as TriangleModel;
                            nodeItem = new NodeItem(triangleModel.Id, ShapeType.Start, triangleModel.Properties.Name, triangleModel.Properties.Description, triangleModel.Properties.Label);
                            break;
                        default:
                            break;
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Re-validation if a link is removed
        /// </summary>

        private void Links_Removed(BaseLinkModel obj)
        {
            ValidateDiagram();
        }

        /// <summary>
        /// Re-validation if a the mouse button is released
        /// </summary>
        private void Diagram_PointerUp(Model? arg1, Blazor.Diagrams.Core.Events.PointerEventArgs arg2)
        {
            ValidateDiagram();
        }


        /// <summary>
        /// Re-validation if a node is removed
        /// </summary>
        private void Nodes_Removed(NodeModel obj)
        {
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

        /// <summary>
        /// Update the zoom indicator
        /// </summary>
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
                    startNode.Properties.ID = startNode.Id;
                    startNode.Properties.Label = "Start";
                    startNode.Properties.Name = "Start_" + nodeCounter.ToString();
                    startNode.Properties.Description = "Start Node";
                    return startNode;
                case ShapeType.End:
                    var endNode = new EndModel(new Point(x, y));
                    endNode.Position = new Point(x + endNode.PositionOffSetX, y + endNode.PositionOffSetY);
                    endNode.AddPort(PortAlignment.Top);
                    endNode.AddPort(PortAlignment.Bottom);
                    endNode.AddPort(PortAlignment.Left);
                    endNode.AddPort(PortAlignment.Right);
                    endNode.Properties.ID = endNode.Id;
                    endNode.Properties.Label = "End";
                    endNode.Properties.Name = "End_" + nodeCounter.ToString();
                    endNode.Properties.Description = "End Node";
                    return endNode;
                case ShapeType.Triangle:
                    var triangleNode = new TriangleModel(new Point(x, y));
                    triangleNode.Position = new Point(x + triangleNode.PositionOffSetX, y + triangleNode.PositionOffSetY);
                    triangleNode.AddPort(PortAlignment.Top);
                    triangleNode.AddPort(PortAlignment.Left);
                    triangleNode.AddPort(PortAlignment.Right);
                    triangleNode.Properties.ID = triangleNode.Id;
                    triangleNode.Properties.Label = "";
                    triangleNode.Properties.Name = "Triangle_" + nodeCounter.ToString();
                    triangleNode.Properties.Description = "Triangle Node";
                    return triangleNode;
                case ShapeType.Rectangle:
                    var rectangleNode = new RectangleModel(new Point(x, y));
                    rectangleNode.Position = new Point(x + rectangleNode.PositionOffSetX, y + rectangleNode.PositionOffSetY);
                    rectangleNode.AddPort(PortAlignment.Top);
                    rectangleNode.AddPort(PortAlignment.Bottom);
                    rectangleNode.AddPort(PortAlignment.Left);
                    rectangleNode.AddPort(PortAlignment.Right);
                    rectangleNode.Properties.ID = rectangleNode.Id;
                    rectangleNode.Properties.Label = "";
                    rectangleNode.Properties.Name = "Rectangle_" + nodeCounter.ToString();
                    rectangleNode.Properties.Description = "Rectangle Node";
                    return rectangleNode;
                case ShapeType.Diamond:
                    var diamondNode = new DiamondModel(new Point(x, y));
                    diamondNode.Position = new Point(x + diamondNode.PositionOffSetX, y + diamondNode.PositionOffSetY);
                    diamondNode.AddPort(PortAlignment.Top);
                    diamondNode.AddPort(PortAlignment.Bottom);
                    diamondNode.AddPort(PortAlignment.Left);
                    diamondNode.AddPort(PortAlignment.Right);
                    diamondNode.Properties.ID = diamondNode.Id;
                    diamondNode.Properties.Label = "";
                    diamondNode.Properties.Name = "Diamond_" + nodeCounter.ToString();
                    diamondNode.Properties.Description = "Diamond Node";
                    return diamondNode;
                default:
                    var node = new NodeModel(new Point(x, y));
                    node.AddPort(PortAlignment.Left);
                    node.AddPort(PortAlignment.Right);
                    return node;
            }
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
            ValidateDiagram();
        }

        /// <summary>
        /// Clear the Diagram canvas, and automatically add a start and end node
        /// </summary>
        /// <returns></returns>
        private async Task Template()
        {
            Diagram.Nodes.Clear();
            nodeCounter = 0;
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
        /// Apply property changes to the selected node
        /// </summary>
        private void ApplyPropertyChanges()
        {
            foreach (NodeModel node in Diagram.Nodes)
            {
                if (node.Id == nodeItem.ID)
                {
                    switch (node.GetType().Name)
                    {
                        case "StartModel":
                            StartModel startModel = node as StartModel;
                            startModel.Properties = nodeItem;
                            break;
                        case "EndModel":
                            EndModel endModel = node as EndModel;
                            endModel.Properties = nodeItem;
                            break;
                        case "DiamondModel":
                            DiamondModel diamondModel = node as DiamondModel;
                            diamondModel.Properties = nodeItem;
                            break;
                        case "RectangleModel":
                            RectangleModel rectangleModel = node as RectangleModel;
                            rectangleModel.Properties = nodeItem;
                            break;
                        case "TriangleModel":
                            TriangleModel triangleModel = node as TriangleModel;
                            triangleModel.Properties = nodeItem ;
                            
                            break;
                        default:
                            break;
                    }
                    node.Refresh();

                    break;
                }
            }
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
        /// Drop a dragged element onto the canvas and select it
        /// </summary>
        /// <param name="e"></param>
        private void OnDrop(DragEventArgs e)
        {
            if (draggedType == null) // Unkown item
                return;

            // Get the drop position
            var position = Diagram.GetRelativeMousePoint(e.ClientX, e.ClientY);

            // Create a new node at the position
            NodeModel newNode = Diagram.Nodes.Add(NewNode(position.X, position.Y, (ShapeType) draggedType));

            // Select the new node
            Diagram.SelectModel(newNode, true);

            // Clear the drag type
            draggedType = null;

            ValidateDiagram();
        }

        /// <summary>
        /// Validate the diagram. Diagram status is indicated top right
        /// </summary>
        private void ValidateDiagram()
        {
            int startCount = 0;
            int endCount = 0;

            ValidationReason = "";

            // Make sure there is a start and end node. Each diagram may have only on of each
            foreach (NodeModel node in Diagram.Nodes)
            {
                if (node.GetType().Name == "StartModel")
                {
                    startCount++;
                }
                if (node.GetType().Name == "EndModel")
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

            // Check that each node is linked to at least one other
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

            // Make sure each link starts and ends at a node
            foreach (BaseLinkModel link in Diagram.Links)
            {
                if (!link.IsAttached)
                {
                    IsDiagramValid = false;
                    ValidationReason = ValidationReason + "&#x2022 All links must have a start and end element.<br>";
                    break;
                }
            }

            // Update the status display
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
