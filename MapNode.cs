using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CS291MiniProject
{
  public class MapNode
  {
    #region Variables
    private List<MapEdge> _vertices;
    public List<MapEdge> Vertices
    { 
      get 
      { 
        return _vertices;
      } 
    }

    public readonly int nodeId;
    public string name;
    public Point Position { get; set; }
    #endregion

    public MapNode(int id)
    {
      this.nodeId = id;
      this._vertices = new List<MapEdge>();
      this.Position = Point.Empty;
    }

    public void AddVertex(MapEdge v)
    {
      _vertices.Add(v);
    }

    public MapNode[] GetConnetedNodes()
    {
      List<MapNode> nodes = new List<MapNode>();
      foreach (var v in _vertices)
      {
        if (v.StartNode == this)
          nodes.Add(v.EndNode);
        else 
          nodes.Add(v.StartNode);
      }

      return nodes.ToArray();
    }

    public int GetWeightBetweenNode(MapNode node)
    {
      foreach (var v in _vertices)
      {
        if (v.StartNode.Equals(node) || v.EndNode.Equals(node))
        {
          return v.Weight;
        }
      }

      return -1;
    }
  }
}
