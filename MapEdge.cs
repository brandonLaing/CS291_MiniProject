using System;
using System.Drawing;

namespace CS291MiniProject
{
  public class MapEdge
  {
    #region Properties
    public MapNode StartNode { get; private set; }
    public MapNode EndNode { get; private set; }
    public int Weight { get; private set; }

    // Calculated properties for drawing
    public Point StartPosition => StartNode.Position;
    public Point EndPosition => EndNode.Position;
    #endregion

    #region Constructors
    public MapEdge(MapNode startNode, MapNode endNode, int weight)
    {
      if (startNode == null || endNode == null)
        throw new ArgumentNullException("Nodes cannot be null");

      if (weight <= 0)
        throw new ArgumentException("Weight must be positive");

      StartNode = startNode;
      EndNode = endNode;
      Weight = weight;
    }
    #endregion

    #region Methods
    public bool ConnectsSameNodes(MapEdge other)
    {
      return (this.StartNode == other.StartNode && this.EndNode == other.EndNode) ||
             (this.StartNode == other.EndNode && this.EndNode == other.StartNode);
    }

    public bool ContainsNode(MapNode node)
    {
      return StartNode == node || EndNode == node;
    }

    public MapNode GetOtherNode(MapNode node)
    {
      if (StartNode == node) return EndNode;
      if (EndNode == node) return StartNode;
      return null;
    }
    #endregion

    #region Overrides
    public override bool Equals(object obj)
    {
      return obj is MapEdge edge &&
             ((StartNode == edge.StartNode && EndNode == edge.EndNode) ||
              (StartNode == edge.EndNode && EndNode == edge.StartNode)) &&
             Weight == edge.Weight;
    }

    public override int GetHashCode()
    {
      // Order doesn't matter for hash code
      int hash1 = StartNode.GetHashCode() ^ EndNode.GetHashCode();
      int hash2 = EndNode.GetHashCode() ^ StartNode.GetHashCode();
      return (hash1 > hash2 ? hash1 : hash2) ^ Weight.GetHashCode();
    }

    public override string ToString()
    {
      return $"{StartNode.name} ←[{Weight}]→ {EndNode.name}";
    }
    #endregion
  }
}