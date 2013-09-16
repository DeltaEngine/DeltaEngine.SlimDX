﻿using DeltaEngine.Datatypes;
using DeltaEngine.Entities;
using DeltaEngine.Rendering.Shapes;

namespace DeltaEngine.Rendering.Graphs
{
	/// <summary>
	/// Renders a set of axes at the origin.
	/// </summary>
	internal class RenderAxes
	{
		public void Refresh(Graph graph)
		{
			if (graph.Visibility == Visibility.Show && Visibility == Visibility.Show)
				ShowAxes(graph);
			else
				HideAxes();
		}

		public Visibility Visibility { get; set; }

		private void ShowAxes(Graph graph)
		{
			renderLayer = graph.RenderLayer + RenderLayerOffset;
			viewport = graph.Viewport;
			drawArea = graph.DrawArea;
			clippingBounds = Rectangle.FromCorners(
				ToQuadratic(viewport.BottomLeft, viewport, drawArea),
				ToQuadratic(viewport.TopRight, viewport, drawArea));
			Point origin = graph.Origin;
			SetAxis(XAxis, ToQuadratic(new Point(viewport.Left, origin.Y), viewport, drawArea),
				ToQuadratic(new Point(viewport.Right, origin.Y), viewport, drawArea));
			SetAxis(YAxis, ToQuadratic(new Point(origin.X, viewport.Top), viewport, drawArea),
				ToQuadratic(new Point(origin.X, viewport.Bottom), viewport, drawArea));
		}

		private int renderLayer;
		private const int RenderLayerOffset = 2;
		private Rectangle viewport;
		private Rectangle drawArea;
		private Rectangle clippingBounds;

		public readonly Line2D XAxis = new Line2D(Point.Zero, Point.Zero, Color.White)
		{
			Visibility = Visibility.Hide
		};

		public readonly Line2D YAxis = new Line2D(Point.Zero, Point.Zero, Color.White)
		{
			Visibility = Visibility.Hide
		};

		private static Point ToQuadratic(Point point, Rectangle viewport, Rectangle drawArea)
		{
			float borderWidth = viewport.Width * Graph.Border;
			float borderHeight = viewport.Height * Graph.Border;
			float x = (point.X - viewport.Left + borderWidth) / (viewport.Width + 2 * borderWidth);
			float y = (point.Y - viewport.Top + borderHeight) / (viewport.Height + 2 * borderHeight);
			return new Point(drawArea.Left + x * drawArea.Width, drawArea.Bottom - y * drawArea.Height);
		}

		private void SetAxis(Line2D axis, Point startPoint, Point endPoint)
		{
			axis.StartPoint = startPoint;
			axis.EndPoint = endPoint;
			axis.RenderLayer = renderLayer;
			axis.Clip(clippingBounds);
		}

		internal void HideAxes()
		{
			XAxis.Visibility = Visibility.Hide;
			YAxis.Visibility = Visibility.Hide;
		}
	}
}