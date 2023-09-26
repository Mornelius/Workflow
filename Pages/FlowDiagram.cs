
using Blazor.Diagrams;
using Blazor.Diagrams.Algorithms;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Models.Base;
using Blazor.Diagrams.Core.PathGenerators;
using Blazor.Diagrams.Core.Routers;
using Blazor.Diagrams.Options;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Workflow.Components;
using Workflow.Models;
using Workflow.Structures;
using static Workflow.Structures.Enumerators;
using Newtonsoft;
using Newtonsoft.Json;
using Blazor.Diagrams.Core.Anchors;
using static System.Net.Mime.MediaTypeNames;
using Workflow.Shared;

namespace Workflow.Pages
{
    public partial class FlowDiagram
    {
        InputTextArea txtOutput;
        Modal Modal;

        string diagramDescription = "";

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

        private LinkModel? LinkToDelete = null;

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
                        RequireTarget = true,
                        DefaultColor = "black",
                        DefaultSelectedColor = "red",
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
            Diagram.RegisterComponent<RedLinkModel, RedLink>();
            ZoomLevel = Diagram.Zoom;

            ValidateDiagram();
        }

        /// <summary>
        /// Document drawing changes 
        /// </summary>
        private void Diagram_Changed()
        {
            List<ExportNode> exportNodes = new List<ExportNode>();

            foreach (NodeModel node in Diagram.Nodes)
            {
                ExportNode exportNode = new ExportNode();
                exportNode.ID = node.Id;
                exportNode.XPosition = node.Position.X;
                exportNode.YPosition = node.Position.Y;
                if (node.Size != null)
                {
                    exportNode.Width = node.Size.Width;
                    exportNode.Height = node.Size.Height;
                }

                switch (node.GetType().Name)
                {
                    case "StartModel":
                        StartModel startModel = node as StartModel;
                        exportNode.Description = startModel.Properties.Description;
                        exportNode.Name = startModel.Properties.Name;
                        exportNode.Label = startModel.Properties.Label;
                        exportNode.Type = ShapeType.Start;
                        break;
                    case "EndModel":
                        EndModel endModel = node as EndModel;
                        exportNode.Description = endModel.Properties.Description;
                        exportNode.Name = endModel.Properties.Name;
                        exportNode.Label = endModel.Properties.Label;
                        exportNode.Type = ShapeType.End;
                        break;
                    case "DiamondModel":
                        DiamondModel diamondModel = node as DiamondModel;
                        exportNode.Description = diamondModel.Properties.Description;
                        exportNode.Name = diamondModel.Properties.Name;
                        exportNode.Label = diamondModel.Properties.Label;
                        exportNode.Type = ShapeType.Diamond;
                        break;
                    case "RectangleModel":
                        RectangleModel rectangleModel = node as RectangleModel;
                        exportNode.Description = rectangleModel.Properties.Description;
                        exportNode.Name = rectangleModel.Properties.Name;
                        exportNode.Label = rectangleModel.Properties.Label;
                        exportNode.Type = ShapeType.Rectangle;
                        break;
                    case "TriangleModel":
                        TriangleModel triangleModel = node as TriangleModel;
                        exportNode.Description = triangleModel.Properties.Description;
                        exportNode.Name = triangleModel.Properties.Name;
                        exportNode.Label = triangleModel.Properties.Label;
                        exportNode.Type = ShapeType.Triangle;
                        break;
                    default:
                        exportNode.Description = "";
                        exportNode.Name = "";
                        exportNode.Label = "";
                        exportNode.Type = ShapeType.Normal;
                        break;
                }

                List<ExportPort> ports = new List<ExportPort>();

                foreach (PortModel port in node.Ports)
                {
                    ExportPort exportPort = new ExportPort();
                    exportPort.ID = port.Id;
                    exportPort.PortAlignment = port.Alignment;

                    List<ExportLink> links = new List<ExportLink>();

                    foreach (LinkModel link in port.Links)
                    {
                        if (link.IsAttached)
                        {
                            ExportLink exportLink = new ExportLink();

                            exportLink.ID = link.Id;

                            if (((SinglePortAnchor) link.Source).Model != null)
                            {
                                SinglePortAnchor source = (SinglePortAnchor) link.Source;
                                exportLink.StartPortID = source.Port.Id;
                            }
                            if (((SinglePortAnchor) link.Target).Model != null)
                            {
                                SinglePortAnchor target = (SinglePortAnchor) link.Target;
                                exportLink.EndPortID = target.Port.Id;
                            }

                            List<ExportVertice> vertices = new List<ExportVertice>();

                            foreach (LinkVertexModel vertice in link.Vertices)
                            {
                                ExportVertice exportVertice = new ExportVertice();

                                exportVertice.ID = vertice.Id;
                                exportVertice.XPosition = vertice.Position.X;
                                exportVertice.YPosition = vertice.Position.Y;
                                exportVertice.Lenght = vertice.Position.Length;

                                vertices.Add(exportVertice);
                            }

                            exportLink.Vertices = vertices;

                            links.Add(exportLink);
                        }
                    }
                    exportPort.exportLinks.AddRange(links);

                    ports.Add(exportPort);
                }
                exportNode.Ports = ports;

                exportNodes.Add(exportNode);
            }

            var settings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented,
            };

            diagramDescription = JsonConvert.SerializeObject(exportNodes, settings);
        }

        /// <summary>
        /// Update the properties when a new node is selected
        /// </summary>
        /// <param name="obj"></param>
        private void Diagram_SelectionChanged(SelectableModel obj)
        {
            nodeItem = new NodeItem();

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
        /// Define what a link looks like when it is added. Also ensure that each port only has one link attached
        /// </summary>
        /// <param name="obj"></param>
        private void Links_Added(Blazor.Diagrams.Core.Models.Base.BaseLinkModel obj)
        {
            // Stop link add if the start port is already occupied
            foreach (NodeModel node in Diagram.Nodes)
            {
                foreach (PortModel port in node.Ports)
                {
                    if (port.Links.Count > 1)
                    {
                        if (port.Links.Contains(obj))
                        {
                            Diagram.Links.Remove(obj);
                            ValidateDiagram();
                            return;
                        }
                    }
                }
            }

            obj.PathGenerator = new StraightPathGenerator();
            obj.Router = new OrthogonalRouter();
            obj.TargetMarker = LinkMarker.Arrow;
            obj.Segmentable = true;
            obj.TargetAttached += Obj_TargetAttached;
            ValidateDiagram();
        }

        private void Obj_TargetAttached(BaseLinkModel obj)
        {
            Anchor startAnchor = obj.Source;
            Anchor endAnchor = obj.Target;

            foreach (LinkModel link in Diagram.Links)
            {
                if (link.Id != obj.Id &&
                    (
                        ((SinglePortAnchor) link.Source).Port.Id == ((SinglePortAnchor) startAnchor).Port.Id ||
                        ((SinglePortAnchor) link.Target).Port.Id == ((SinglePortAnchor) startAnchor).Port.Id ||
                        ((SinglePortAnchor) link.Source).Port.Id == ((SinglePortAnchor) endAnchor).Port.Id ||
                        ((SinglePortAnchor) link.Target).Port.Id == ((SinglePortAnchor) endAnchor).Port.Id))
                {
                    LinkToDelete = (LinkModel) obj;
                    //ValidateDiagram();
                }
            }
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
                    startNode.Properties.Type = type;
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
                    endNode.Properties.Type = type;
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
                    triangleNode.Properties.Type = type;
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
                    rectangleNode.Properties.Type = type;
                    return rectangleNode;
                case ShapeType.Diamond:
                    var diamondNode = new DiamondModel(new Point(x, y));
                    diamondNode.Position = new Point(x + diamondNode.PositionOffSetX, y + diamondNode.PositionOffSetY);
                    diamondNode.AddPort(PortAlignment.Top);
                    diamondNode.AddPort(PortAlignment.Bottom);
                    diamondNode.AddPort(PortAlignment.Left);
                    //diamondNode.AddPort(PortAlignment.Right);
                    diamondNode.Properties.ID = diamondNode.Id;
                    diamondNode.Properties.Label = "";
                    diamondNode.Properties.Name = "Diamond_" + nodeCounter.ToString();
                    diamondNode.Properties.Description = "Diamond Node";
                    diamondNode.Properties.Type = type;
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

            // Create a new node at the position
            NodeModel newNode = Diagram.Nodes.Add(NewNode(x, y, type));

            // Select the new node
            Diagram.SelectModel(newNode, true);

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
                            triangleModel.Properties = nodeItem;

                            break;
                        default:
                            break;
                    }
                    node.Refresh();
                    ValidateDiagram();
                    break;
                }
            }
        }

        /// <summary>
        /// Copy the JSON desribing the model from the description to the clipboard
        /// </summary>
        /// <returns></returns>
        private async Task Copy()
        {
            try
            {
                await ClipboardService.WriteTextAsync(txtOutput.Value);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Paste a model description to the JSON description and build the model
        /// </summary>
        /// <returns></returns>
        private async Task Paste()
        {
            Clear();
            try
            {
                diagramDescription = await ClipboardService.ReadTextAsync();
                GenerateDiagramFromString(diagramDescription);
            }
            catch
            {
                Console.WriteLine("Cannot read from clipboard");
            }
        }

        /// <summary>
        /// Rebuild the model from its JSON string
        /// </summary>
        /// <param name="value">JSON string containing the model description</param>
        private void GenerateDiagramFromString(string value)
        {
            // Dictionaried to map the original IDs to the new IDs
            Dictionary<string, string> IDList = new Dictionary<string, string>();
            Dictionary<string, string> PortList = new Dictionary<string, string>();
            Dictionary<string, string> newLinks = new Dictionary<string, string>();

            List<ExportNode> exportNodes = new List<ExportNode>();

            try
            {
                exportNodes = JsonConvert.DeserializeObject<List<ExportNode>>(value);

                // First add all the nodes
                foreach (ExportNode node in exportNodes)
                {
                    NodeModel newNode = Diagram.Nodes.Add(NewNode(node.XPosition, node.YPosition, node.Type));

                    switch (node.Type)
                    {
                        case ShapeType.Start:
                            StartModel startModel = newNode as StartModel;
                            startModel.Properties.Name = node.Name;
                            startModel.Properties.Label = node.Label;
                            startModel.Properties.ID = newNode.Id;
                            startModel.Properties.Type = node.Type;
                            break;
                        case ShapeType.End:
                            EndModel endModel = newNode as EndModel;
                            endModel.Properties.Name = node.Name;
                            endModel.Properties.Label = node.Label;
                            endModel.Properties.ID = newNode.Id;
                            endModel.Properties.Type = node.Type;
                            break;
                        case ShapeType.Diamond:
                            DiamondModel diamondModel = newNode as DiamondModel;
                            diamondModel.Properties.Name = node.Name;
                            diamondModel.Properties.Label = node.Label;
                            diamondModel.Properties.ID = newNode.Id;
                            diamondModel.Properties.Type = node.Type;
                            break;
                        case ShapeType.Rectangle:
                            RectangleModel rectangleModel = newNode as RectangleModel;
                            rectangleModel.Properties.Name = node.Name;
                            rectangleModel.Properties.Label = node.Label;
                            rectangleModel.Properties.ID = newNode.Id;
                            rectangleModel.Properties.Type = node.Type;
                            break;
                        case ShapeType.Triangle:
                            TriangleModel triangleModel = newNode as TriangleModel;
                            triangleModel.Properties.Name = node.Name;
                            triangleModel.Properties.Label = node.Label;
                            triangleModel.Properties.ID = newNode.Id;
                            triangleModel.Properties.Type = node.Type;
                            break;
                        default:
                            break;
                    }

                    IDList.Add(node.ID, newNode.Id);

                    // Map the node ports to their new IDs
                    foreach (ExportPort port in node.Ports)
                    {
                        foreach (PortModel portModel in newNode.Ports)
                        {
                            if (port.PortAlignment == portModel.Alignment)
                            {
                                PortList.Add(port.ID, portModel.Id);
                                break;
                            }
                        }
                    }
                }

                // Run through the nodes again to build the links and vertices
                foreach (ExportNode node in exportNodes)
                {
                    double XOffset = 0;
                    double YOffset = 0;

                    switch (node.Type)
                    {
                        case ShapeType.Start:
                            StartModel startModel = new StartModel();
                            XOffset = startModel.PositionOffSetX;
                            YOffset = startModel.PositionOffSetY;
                            break;
                        case ShapeType.End:
                            EndModel endModel = new EndModel();
                            XOffset = endModel.PositionOffSetX;
                            YOffset = endModel.PositionOffSetY;
                            break;
                        case ShapeType.Diamond:
                            DiamondModel diamondModel = new DiamondModel();
                            XOffset = diamondModel.PositionOffSetX;
                            YOffset = diamondModel.PositionOffSetY;
                            break;
                        case ShapeType.Rectangle:
                            RectangleModel rectangleModel = new RectangleModel();
                            XOffset = rectangleModel.PositionOffSetX;
                            YOffset = rectangleModel.PositionOffSetY;
                            break;
                        case ShapeType.Triangle:
                            TriangleModel triangleModel = new TriangleModel();
                            XOffset = triangleModel.PositionOffSetX;
                            YOffset = triangleModel.PositionOffSetY;
                            break;
                        default:
                            break;
                    }

                    foreach (ExportPort port in node.Ports)
                    {
                        foreach (ExportLink link in port.exportLinks)
                        {
                            string startPortID = PortList.GetValueOrDefault(link.StartPortID);
                            string endPortID = PortList.GetValueOrDefault(link.EndPortID);

                            string testEndPort = newLinks.GetValueOrDefault(startPortID);
                            if (testEndPort != endPortID)
                            {
                                LinkModel newLink = Diagram.Links.Add(new LinkModel(GetPort(startPortID), GetPort(endPortID)));
                                newLinks.Add(startPortID, endPortID);

                                foreach (ExportVertice vertice in link.Vertices)
                                {
                                    newLink.Vertices.Add(new LinkVertexModel(newLink, new Point(vertice.XPosition + XOffset, vertice.YPosition + YOffset)));
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                Modal.Open();
            }
        }

        /// <summary>
        /// Search for a PortModel according to its ID and return it 
        /// </summary>
        /// <param name="id">Port ID to search for</param>
        /// <returns></returns>
        private PortModel? GetPort(string id)
        {
            PortModel? result = null;
            bool found = false;

            foreach (NodeModel node in Diagram.Nodes)
            {
                foreach (PortModel port in node.Ports)
                {
                    if (port.Id == id)
                    {
                        found = true;
                        result = port;
                        break;
                    }
                }
                if (found)
                {
                    break;
                }
            }

            return result;
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
            if (LinkToDelete != null)
            {
                Diagram.Links.Remove(LinkToDelete);
            }
            LinkToDelete = null;

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

            bool multiplePortLinks = false;

            // Check that each node is linked to at least one other
            foreach (NodeModel node in Diagram.Nodes)
            {
                int linked = 0;

                foreach (PortModel port in node.Ports)
                {

                    if (port.Links.Count > 0)
                    {
                        if (port.Links.Count > 1)
                        {
                            multiplePortLinks = true;
                        }
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

            if (multiplePortLinks)
            {
                IsDiagramValid = false;
                ValidationReason = ValidationReason + "&#x2022 Each node anchor point may only have one link attached to it.<br>";
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

            Diagram_Changed();
            StateHasChanged();
        }
    }
}
