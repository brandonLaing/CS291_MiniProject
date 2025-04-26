using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS291MiniProject
{
  public struct MapPath
  {
    public List<MapNode> Nodes;

    public int TotalWeight()
    {
      int totalWeight = 0;
      for (int i = 0; i < Nodes.Count - 1; i++)
      {
        totalWeight += Nodes[i].GetWeightBetweenNode(Nodes[i + 1]);
      }

      return totalWeight;
    }
  }

  public class MapGraph
  {
    public List<MapNode> Nodes;
    public MapPath currentPath;

    private MapNode selectedStartLoctation;
    private MapNode selectedEndLocation;

    public MapGraph()
    {
      Nodes = new List<MapNode>();
      currentPath = new MapPath();
    }

    public void NormalizePositions(Size panelSize, int padding = 50)
    {
      if (Nodes.Count == 0) return;

      int minX = Nodes.Min(n => n.Position.X);
      int maxX = Nodes.Max(n => n.Position.X);
      int minY = Nodes.Min(m => m.Position.Y); 
      int maxY = Nodes.Max(m => m.Position.Y);

      if (minX == maxX) maxX++;
      if (minY == maxY) maxY++;

      float scaleX = (panelSize.Width - 2 * padding) / (float)(maxX - minX);
      float scaleY = (panelSize.Height - 2 * padding) / (float)(maxY - minY);

      foreach (var node in Nodes)
      {
        node.Position = new Point(
          padding + (int)((node.Position.X - minX) * scaleX),
          padding + (int)((node.Position.Y - minY) * scaleY));
      }
    }

    public bool NodeNameExistis(string name)
    {
      return Nodes.Any(n =>
      string.Equals(n.name, name, StringComparison.OrdinalIgnoreCase));
    }
  }
}
