
using Blazor.Diagrams;
using Blazor.Diagrams.Core.Anchors;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.PathGenerators;
using Blazor.Diagrams.Core.Routers;
using Blazor.Diagrams.Models;
using Blazor.Diagrams.Options;
using Microsoft.AspNetCore.Components.Web;
using Workflow.Components;
using Workflow.Models;
using Workflow.Structures;
using static Workflow.Structures.Enumerators;

namespace Workflow.Pages
{
    public partial class FlowDiagram
    {
        private BlazorDiagram Diagram = new BlazorDiagram();

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

            // We created new custom shapes - they need to be registered on the canvas before they can be used
            Diagram.RegisterComponent<StartModel, Start>();
            Diagram.RegisterComponent<EndModel, End>();
            Diagram.RegisterComponent<RectangleModel, Components.Rectangle>();
            Diagram.RegisterComponent<DiamondModel, Diamond>();
            Diagram.RegisterComponent<TriangleModel, Triangle>();
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
            var node = new NodeModel();

            switch (type)
            {
                case ShapeType.Start:
                    node = new StartModel(new Point(x, y));
                    node.AddPort(PortAlignment.Left);
                    node.AddPort(PortAlignment.Right);
                    break;
                case ShapeType.End:
                    node = new EndModel(new Point(x, y));
                    node.AddPort(PortAlignment.Left);
                    node.AddPort(PortAlignment.Right);
                    break;
                case ShapeType.Triangle:
                    node = new TriangleModel(new Point(x, y));
                    node.AddPort(PortAlignment.Top);
                    node.AddPort(PortAlignment.Left);
                    node.AddPort(PortAlignment.Right);
                    break;
                case ShapeType.Rectangle:
                    node = new RectangleModel(new Point(x, y));
                    node.AddPort(PortAlignment.Top);
                    node.AddPort(PortAlignment.Bottom);
                    node.AddPort(PortAlignment.Left);
                    node.AddPort(PortAlignment.Right);
                    break;
                case ShapeType.Diamond:
                    node = new DiamondModel(new Point(x, y));
                    node.AddPort(PortAlignment.Top);
                    node.AddPort(PortAlignment.Bottom);
                    node.AddPort(PortAlignment.Left);
                    node.AddPort(PortAlignment.Right);
                    break;
                default:
                    node = new NodeModel(new Point(x, y));
                    node.AddPort(PortAlignment.Left);
                    node.AddPort(PortAlignment.Right);
                    break;
            }

            return node;
        }

        /// <summary>
        /// Clear the Diagram canvas, removing all nodes, links etc.
        /// </summary>
        /// <returns></returns>
        private async Task Clear()
        {
            Diagram.Nodes.Clear();
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
        }
    }
}
