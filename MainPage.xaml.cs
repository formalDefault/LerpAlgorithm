using SkiaSharp;
using System;
using System.Timers;

namespace SkiaAnimations
{
    public class Node
    {
        public SKPoint Position { get; set; }
        public SKPoint TargetPosition { get; set; }
    }

    public class Edge
    {
        public Node Start { get; set; }
        public Node End { get; set; }
    }
    public partial class MainPage : ContentPage
    {
        private float x;
        private float y;
        private float amplitude = 100;
        private float frequency = 0.05f;
        private float phase;
        private float radius = 30;
        private SKColor color = SKColors.Blue;
        private System.Timers.Timer timer;
        private List<Node> nodes;
        private List<Edge> edges;
        private Random random;
        private float gradientOffset;


        public MainPage()
        {
            InitializeComponent();
            var navigationPage = this.Parent as NavigationPage;
            if (navigationPage != null)
            {
                navigationPage.BarTextColor = Colors.Black;
            }
            // Initialize random generator
            random = new Random();

            // Initialize nodes and edges
            InitializeGraph();

            // Initialize starting position
            x = 0;
            y = 200;

            // Initialize timer for animation
            timer = new System.Timers.Timer(16); // Approximately 60 FPS
            timer.Elapsed += OnTimerElapsed;
            timer.Start();
        }

        private void InitializeGraph()
        {
            modalOverlay.IsVisible = true;

            nodes = new List<Node>();

            // Create 20 nodes with random positions
            for (int i = 0; i < 20; i++)
            {
                var position = new SKPoint(
                    (float)(random.NextDouble() * canvasView.CanvasSize.Width),
                    (float)(random.NextDouble() * canvasView.CanvasSize.Height)
                );
                nodes.Add(new Node { Position = position, TargetPosition = position });
            }

            edges = new List<Edge>();

            // Ensure each node is connected to at least one other node
            for (int i = 0; i < nodes.Count; i++)
            {
                int nextNodeIndex = (i + 1) % nodes.Count;
                edges.Add(new Edge { Start = nodes[i], End = nodes[nextNodeIndex] });
            }

            // Create additional random edges between nodes
            int additionalEdges = 40; // You can adjust the number of additional edges as needed
            for (int i = 0; i < additionalEdges; i++)
            {
                var startNode = nodes[random.Next(nodes.Count)];
                var endNode = nodes[random.Next(nodes.Count)];
                if (startNode != endNode && !edges.Any(e => (e.Start == startNode && e.End == endNode) || (e.Start == endNode && e.End == startNode)))
                {
                    edges.Add(new Edge { Start = startNode, End = endNode });
                }
            }
        }


        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            // Animate nodes towards their target positions
            foreach (var node in nodes)
            {
                // Update target position randomly within bounds
                if (random.NextDouble() < 0.01)
                {
                    node.TargetPosition = new SKPoint(
                        (float)(random.NextDouble() * canvasView.CanvasSize.Width),
                        (float)(random.NextDouble() * canvasView.CanvasSize.Height)
                    );
                }

                // Interpolate position towards target
                node.Position = Lerp(node.Position, node.TargetPosition, 0.007f);
            }
             
            // Invalidate canvas to redraw
            canvasView.InvalidateSurface();
        }

        private SKPoint Lerp(SKPoint start, SKPoint end, float t)
        {
            return new SKPoint(
                start.X + (end.X - start.X) * t,
                start.Y + (end.Y - start.Y) * t                
            );
        }

        private void canvasView_PaintSurface(object sender, SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            var info = e.Info;

            // Create gradient background
            var gradient = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Shader = SKShader.CreateLinearGradient(
                    new SKPoint(0, 0),
                    new SKPoint(info.Width, info.Height),
                    new SKColor[] { SKColors.Black, SKColors.DarkRed },
                    new float[] { 0.005f, 1 - gradientOffset },
                    SKShaderTileMode.Decal)
            };

            canvas.DrawRect(info.Rect, gradient);

            // Draw edges
            var edgePaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.DarkGray,
                StrokeWidth = 3,
                IsStroke = true
            };

            foreach (var edge in edges)
            {
                canvas.DrawLine(edge.Start.Position, edge.End.Position, edgePaint);
            }

            // Draw nodes
            var nodePaint = new SKPaint
            {
                Style = SKPaintStyle.StrokeAndFill,
                Color = SKColors.White, 
            };

            foreach (var node in nodes)
            {
                canvas.DrawCircle(node.Position, 20, nodePaint);
            }
        }


        private async void OnCloseModalClicked(object sender, EventArgs e)
        {
            await modal.ScaleTo(modal.Scale + 0.1, 100); 
            await modal.ScaleTo(0, 250); 
            modalOverlay.IsVisible = false;
        }
         
    }

}
