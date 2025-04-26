using System.Diagnostics;
using System.Drawing.Drawing2D;

namespace CS291MiniProject
{
  public partial class Form1 : Form
  {
    #region Variables
    private MapGraph mapGraph = new MapGraph();
    private Button saveButton;
    private Button loadButton;
    private ListBox suggestionList;
    private TextBox searchTextBox;
    private Panel mapPanel;
    private MapNode selectedNode = null;
    private Point dragOffset;
    private MapNode firstNodeForEdge = null;
    #endregion

    #region Properties
    private static string saveFileDirectory
    {
      get
      {
        return Path.Combine(Directory.GetCurrentDirectory(), "SaveFiles");
      }
    }
    #endregion

    public Form1()
    {
      InitializeComponent();
      InitUI();
    }

    private void InitUI()
    {
      this.Text = "Campus Map Editor";
      this.Size = new Size(1200, 800);
      this.FormBorderStyle = FormBorderStyle.FixedSingle;

      CreateFileControls();
      CreateMapPanel();
      CreateNodeControls();
    }

    private void CreateFileControls()
    {
      // Search textbox
      searchTextBox = new TextBox
      {
        Location = new Point(10, 10),
        Width = 200
      };
      searchTextBox.Click += SearchTextBox_Click;
      searchTextBox.GotFocus += SearchTextBox_GotFocus;
      searchTextBox.KeyDown += SearchTextBox_KeyDown;

      // Suggestion list
      suggestionList = new ListBox()
      {
        Location = new Point(10, 40),
        Width = 200,
        Height = 100,
        Visible = false
      };

      // Save button
      saveButton = new Button
      {
        Text = "Save",
        Location = new Point(10, 150),
        Size = new Size(95, 30)
      };

      // Load button
      loadButton = new Button
      {
        Text = "Load",
        Location = new Point(115, 150),
        Size = new Size(95, 30)
      };

      // Add controls to form
      this.Controls.Add(searchTextBox);
      this.Controls.Add(suggestionList);
      this.Controls.Add(saveButton);
      this.Controls.Add(loadButton);

      // Event handlers
      searchTextBox.TextChanged += SearchTextBox_TextChanged;
      suggestionList.Click += SuggestionList_Click;
      saveButton.Click += SaveButton_Click;
      loadButton.Click += LoadButton_Click;

      // Ensure save directory exists
      if (!Directory.Exists(saveFileDirectory))
        Directory.CreateDirectory(saveFileDirectory);
    }

    private void CreateMapPanel()
    {
      mapPanel = new Panel
      {
        Location = new Point(220, 10),
        Size = new Size(950, 740),
        BorderStyle = BorderStyle.FixedSingle,
        BackColor = Color.White
      };

      mapPanel.MouseDown += MapPanel_MouseDown;
      mapPanel.MouseMove += MapPanel_MouseMove;
      mapPanel.MouseUp += MapPanel_MouseUp;
      mapPanel.Paint += MapPanel_Paint;

      this.Controls.Add(mapPanel);
    }

    private void CreateNodeControls()
    {
      Button addNodeButton = new Button
      {
        Text = "Add Node",
        Location = new Point(10, 200),
        Size = new Size(200, 30)
      };
      addNodeButton.Click += AddNodeButton_Click;

      Button addEdgeButton = new Button
      {
        Text = "Add Edge",
        Location = new Point(10, 240),
        Size = new Size(200, 30)
      };
      addEdgeButton.Click += AddEdgeButton_Click;

      this.Controls.Add(addNodeButton);
      this.Controls.Add(addEdgeButton);
    }

    #region Event Handlers
    private void MapPanel_Paint(object sender, PaintEventArgs e)
    {
      e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

      Font nodeFont = new Font(this.Font.FontFamily, 10, FontStyle.Bold);
      Font weightFont = new Font(this.Font.FontFamily, 8, FontStyle.Regular);
      Font nameFont = new Font(this.Font.FontFamily, 9, FontStyle.Bold);

      // Draw all edges first
      foreach (var node in mapGraph.Nodes)
      {
        foreach (var edge in node.Vertices)
        {
          if (edge.StartNode == node) // Only draw once per edge
          {
            e.Graphics.DrawLine(Pens.Black,
                edge.StartNode.Position,
                edge.EndNode.Position);

            // Draw weight label
            PointF midPoint = new PointF(
                (edge.StartNode.Position.X + edge.EndNode.Position.X) / 2,
                (edge.StartNode.Position.Y + edge.EndNode.Position.Y) / 2);

            DrawOutlinedText(e.Graphics,
              edge.Weight.ToString(),
              weightFont,
              Brushes.White,
              Brushes.Black,
              new PointF(midPoint.X - 5, midPoint.Y - 7));
          }
        }
      }

      // Draw all nodes
      foreach (var node in mapGraph.Nodes)
      {
        Color nodeColor = (node == selectedNode) ? Color.Red : Color.Blue;
        using (Brush brush = new SolidBrush(nodeColor))
        {
          e.Graphics.FillEllipse(brush,
              node.Position.X - 15, node.Position.Y - 15, 30, 30);

          using (var format = new StringFormat())
          {
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;

            RectangleF idRect = new RectangleF(
              node.Position.X - 15,
              node.Position.Y - 15,
              30, 30);

            e.Graphics.DrawString(
              node.nodeId.ToString(),
              this.Font, 
              Brushes.White, 
              idRect, 
              format);


          }
          SizeF textSize = e.Graphics.MeasureString(node.name, this.Font);
          DrawOutlinedText(e.Graphics,
            node.name,
            nameFont,
            Brushes.Black,
            Brushes.White,
            new PointF(node.Position.X - (textSize.Width / 2),
            node.Position.Y - 40));
        }
      }

      // Draw temporary edge if we're creating one
      if (firstNodeForEdge != null && selectedNode != null && firstNodeForEdge != selectedNode)
      {
        e.Graphics.DrawLine(Pens.Green,
            firstNodeForEdge.Position, selectedNode.Position);
      }
    }

    private void MapPanel_MouseDown(object sender, MouseEventArgs e)
    {
      // Check if we clicked on a node
      selectedNode = mapGraph.Nodes.FirstOrDefault(n =>
          Math.Sqrt(Math.Pow(e.X - n.Position.X, 2) + Math.Pow(e.Y - n.Position.Y, 2)) <= 15);

      if (selectedNode != null)
      {
        dragOffset = new Point(e.X - selectedNode.Position.X, e.Y - selectedNode.Position.Y);
      }
      else if (firstNodeForEdge == null)
      {
        // If not clicking on a node and not creating an edge, clear selection
        selectedNode = null;
      }

      mapPanel.Invalidate();
    }

    private void MapPanel_MouseUp(object sender, MouseEventArgs e)
    {
      // Handle edge creation
      if (firstNodeForEdge != null && selectedNode != null && firstNodeForEdge != selectedNode)
      {
        string input = Microsoft.VisualBasic.Interaction.InputBox(
            "Enter edge weight:", "Add Edge", "100");

        if (int.TryParse(input, out int weight) && weight > 0)
        {
          var edge = new MapEdge(firstNodeForEdge, selectedNode, weight);
          firstNodeForEdge.AddVertex(edge);
          selectedNode.AddVertex(edge);
        }
      }
      firstNodeForEdge = null;
      mapPanel.Invalidate();
    }

    private void MapPanel_MouseMove(object sender, MouseEventArgs e)
    {
      if (selectedNode != null && e.Button == MouseButtons.Left)
      {
        int newX = Math.Clamp(e.X - dragOffset.X, 15, mapPanel.Width - 15);
        int newY = Math.Clamp(e.Y - dragOffset.Y, 15, mapPanel.Height - 15);

        selectedNode.Position = new Point(newX, newY);
        mapPanel.Invalidate();
      }
    }

    private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Enter && suggestionList.Items.Count > 0)
      {
        suggestionList.SelectedIndex = 0;

        searchTextBox.Text = suggestionList.Items[0].ToString();


        searchTextBox.SelectionStart = searchTextBox.Text.Length;
        searchTextBox.SelectionLength = 0;

        LoadButton_Click(null, EventArgs.Empty);

        e.Handled = true;
        e.SuppressKeyPress = true;

        suggestionList.Visible = false;
      }
      else if (e.KeyCode == Keys.Escape)
      {
        suggestionList.Visible = false;
        searchTextBox.Select(0, 0);
        e.Handled = true;
      }
    }

    private void SearchTextBox_TextChanged(object sender, EventArgs e)
    {
      string searchText = searchTextBox.Text.ToLower();
      string[] allFiles = Directory.GetFiles(saveFileDirectory, "*.txt");

      var matchingFiles = allFiles
          .Select(Path.GetFileName)
          .Where(f => f.ToLower().Contains(searchText))
          .ToArray();

      suggestionList.Items.Clear();
      suggestionList.Items.AddRange(matchingFiles);
      suggestionList.Visible = matchingFiles.Length > 0;
    }

    private void SearchTextBox_Click(object sender, EventArgs e)
    {
      SearchTextBox_TextChanged(sender, e);
    }

    private void SearchTextBox_GotFocus(object sender, EventArgs e)
    {
      SearchTextBox_TextChanged(sender, e);
    }

    private void AddNodeButton_Click(object sender, EventArgs e)
    {
      string name;
      do
      {
        name = Microsoft.VisualBasic.Interaction.InputBox(
          "Enter a location name:", "Add New Location");

        if (string.IsNullOrWhiteSpace(name))
          return;

        if (mapGraph.NodeNameExistis(name))
        {
          MessageBox.Show(
            $"Location is alread named '{name}'\nEnter a differnt name",
            "Duplicate Name",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
        }
        else
          break;
      }
      while (true);

      int newId = mapGraph.Nodes.Count > 0 ? mapGraph.Nodes.Max(n => n.nodeId) + 1 : 1;

      var newNode = new MapNode(newId)
      {
        Position = new Point(mapPanel.Width / 2, mapPanel.Height / 2),
        name = name.Trim()
      };

      mapGraph.Nodes.Add(newNode);
      mapPanel.Invalidate();
    }

    private void AddEdgeButton_Click(object sender, EventArgs e)
    {
      if (selectedNode != null)
      {
        firstNodeForEdge = selectedNode;
        MessageBox.Show("Now click on the second node to connect to");
      }
      else
      {
        MessageBox.Show("Please select a node first");
      }
    }

    private void SaveButton_Click(object sender, EventArgs e)
    {
      if (string.IsNullOrWhiteSpace(searchTextBox.Text))
      {
        MessageBox.Show("Please enter a filename");
        return;
      }

      string filePath = Path.Combine(saveFileDirectory, searchTextBox.Text);
      if (!filePath.EndsWith(".txt"))
        filePath += ".txt";

      try
      {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
          // Write nodes
          foreach (var node in mapGraph.Nodes)
          {
            writer.WriteLine($"Location,{node.nodeId},{node.name},{node.Position.X},{node.Position.Y}");
          }

          // Write edges (only write once per edge pair)
          HashSet<string> writtenEdges = new HashSet<string>();
          foreach (var node in mapGraph.Nodes)
          {
            foreach (var edge in node.Vertices)
            {
              string edgeKey = $"{Math.Min(edge.StartNode.nodeId, edge.EndNode.nodeId)},{Math.Max(edge.StartNode.nodeId, edge.EndNode.nodeId)}";
              if (!writtenEdges.Contains(edgeKey))
              {
                writer.WriteLine($"Path,{edge.StartNode.nodeId},{edge.EndNode.nodeId},{edge.Weight}");
                writtenEdges.Add(edgeKey);
              }
            }
          }
        }
        MessageBox.Show("Graph saved successfully!");
      }
      catch (Exception ex)
      {
        MessageBox.Show($"Error saving file: {ex.Message}");
      }
    }

    private void LoadButton_Click(object sender, EventArgs e)
    {
      if (string.IsNullOrWhiteSpace(searchTextBox.Text))
      {
        MessageBox.Show("Please select a file to load");
        return;
      }

      string filePath = Path.Combine(saveFileDirectory, searchTextBox.Text);
      if (!File.Exists(filePath))
      {
        MessageBox.Show("File does not exist");
        return;
      }

      try
      {
        mapGraph = new MapGraph();
        var lines = File.ReadAllLines(filePath);
        Dictionary<int, MapNode> nodeDict = new Dictionary<int, MapNode>();

        foreach (var line in lines)
        {
          var parts = line.Split(',');
          if (parts.Length < 2) continue;

          if (parts[0] == "Location")
          {
            int id = int.Parse(parts[1]);
            string name = parts[2];
            int x = int.Parse(parts[3]);
            int y = int.Parse(parts[4]);

            var node = new MapNode(id)
            {
              name = name,
              Position = new Point(x, y)
            };
            nodeDict[id] = node;
            mapGraph.Nodes.Add(node);
          }
          else if (parts[0] == "Path")
          {
            int fromId = int.Parse(parts[1]);
            int toId = int.Parse(parts[2]);
            int weight = int.Parse(parts[3]);

            if (nodeDict.TryGetValue(fromId, out var fromNode) &&
                nodeDict.TryGetValue(toId, out var toNode))
            {
              var edge = new MapEdge(fromNode, toNode, weight);
              fromNode.AddVertex(edge);
              toNode.AddVertex(edge);
            }
          }
        }

        mapGraph.NormalizePositions(mapPanel.ClientSize);
        mapPanel.Invalidate();
        MessageBox.Show("Graph loaded successfully!");
      }
      catch (Exception ex)
      {
        MessageBox.Show($"Error loading file: {ex.Message}");
      }
    }

    private void SuggestionList_Click(object sender, EventArgs e)
    {
      if (suggestionList.SelectedItem != null)
      {
        searchTextBox.Text = suggestionList.SelectedItem.ToString();
        suggestionList.Visible = false;
      }
    }

    #endregion

    private void DrawOutlinedText(Graphics g, string text, Font font, Brush textBrush, Brush outlineBrush, PointF position, int outlineWidth = 2)
    {
      for (int x = -outlineWidth; x < outlineWidth; x++)
      {
        for (int y = -outlineWidth; y <= outlineWidth; y++)
        {
          if (x != 0 || y != 0)
          {
            g.DrawString(text, font, outlineBrush,
              position.X + x,
              position.Y + y);
          }
        }
      }

      g.DrawString(text, font, textBrush, position);
    }

  }
}