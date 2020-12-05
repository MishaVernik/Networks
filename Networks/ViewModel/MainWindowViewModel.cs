using GraphSharp.Controls;
using Networks.Data;
using Networks.PopupWindows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Networks
{
    public class PocGraphLayout : GraphLayout<PocVertex, PocEdge, PocGraph> { }

    public class MainWindowViewModel : INotifyPropertyChanged
    {
        #region Data

        private readonly List<Double> weights;
        private readonly Random random = new Random();
        private string layoutAlgorithmType;
        private PocGraph graph;
        private List<String> layoutAlgorithmTypes = new List<string>();
        private int count;
        private Int32 computerCounter = 0;
        private Int32 routerCounter = 0;
        #endregion

        #region Ctor
        public MainWindowViewModel()
        {
            Graph = new PocGraph(true);
            Graph.VertexAdded += Graph_VertexAdded;
            weights = new List<double>() { 2, 3, 5, 7, 10, 12, 15, 20, 21, 25, 27, 29 };
            //List<PocVertex> existingVertices = new List<PocVertex>();
            //existingVertices.Add(new PocVertex("Router 1", NetworkType.ROUTER, true)); //0
            //existingVertices.Add(new PocVertex("Router 2", NetworkType.COMPUTER, false)); //1
            //existingVertices.Add(new PocVertex("Router 3", NetworkType.ROUTER, true)); //2
            //existingVertices.Add(new PocVertex("Router 4", NetworkType.ROUTER, true)); //3


            //foreach (PocVertex vertex in existingVertices)
            //    Graph.AddVertex(vertex);


            ////add some edges to the graph
            //AddNewGraphEdge(existingVertices[0], existingVertices[1]);
            //AddNewGraphEdge(existingVertices[0], existingVertices[2]);
            //AddNewGraphEdge(existingVertices[0], existingVertices[3]);
            //AddNewGraphEdge(existingVertices[3], existingVertices[1]);

            //Add Layout Algorithm Types
            layoutAlgorithmTypes.Add("BoundedFR");
            layoutAlgorithmTypes.Add("Circular");
            layoutAlgorithmTypes.Add("CompoundFDP");
            layoutAlgorithmTypes.Add("EfficientSugiyama");
            layoutAlgorithmTypes.Add("FR");
            layoutAlgorithmTypes.Add("ISOM");
            layoutAlgorithmTypes.Add("KK");
            layoutAlgorithmTypes.Add("LinLog");
            layoutAlgorithmTypes.Add("Tree");
            layoutAlgorithmTypes.Add("None");

            //Pick a default Layout Algorithm Type
            LayoutAlgorithmType = "LinLog";
        }
        #endregion

        #region Public Methods
        public void AddNewEdge(String fromRouter, String toRouter)
        {
            var edge = new PocEdge(
               fromRouter.ToString() + "-" + toRouter.ToString(),
               Graph.Vertices.Where(id => id.ID == fromRouter).FirstOrDefault<PocVertex>(),
               Graph.Vertices.Where(id => id.ID == toRouter).FirstOrDefault<PocVertex>(),
              weights[random.Next(0, weights.Count)].ToString());
            Graph.AddEdge(edge);
        }

        public void AddNewVertex(NetworkType netwrokType)
        {
            String name = String.Empty;
            Int32 counter = 0;
            switch (netwrokType)
            {
                case NetworkType.COMPUTER:
                    name = "Computer";
                    counter = computerCounter++;
                    break;
                case NetworkType.ROUTER:
                    name = "Router";
                    counter = routerCounter++;
                    break;
                default:
                    break;
            }
            var vertex = new PocVertex(name + (counter).ToString(), netwrokType, false);


            Graph.AddVertex(vertex);
        }


        private void Graph_VertexAdded(PocVertex vertex)
        {

            //throw new NotImplementedException();
        }

        public void UpdateLayout()
        {
            LayoutAlgorithmType = "None"; 
            NotifyPropertyChanged("Graph");
        }
        public void ReLayoutGraph()
        {
            /* 
             * Input:
             *  weights     -> [2, 3, 5, 7, 10, 12, 15, 20, 21, 25, 27, 29]
             *  avg. power  -> 3 
             * 
             * 
             */
            CreateNewGraphWindow graphWindow = new CreateNewGraphWindow();
            graphWindow.Closed += GraphWindow_Closed;
            graphWindow.Show();
        }

        private void GraphWindow_Closed(object sender, EventArgs e)
        {
            if (sender == null)
            {
                return;
            }
            CreateNewGraphWindow graphWindow = (CreateNewGraphWindow)sender;
            computerCounter = 0;
            routerCounter = 0;
            Graph = new PocGraph(true);
            List<PocVertex> pocVertices = new List<PocVertex>();
            Int32 numberOfNodes = Convert.ToInt32(graphWindow.txtNumberOfNodes.Text);
            Double nodePower = Convert.ToInt32(graphWindow.txtAvgPower.Text);
            Int32 delimiterPC = 3;
            PocVertex vertex = null;
            for (Int32 i = 1; i <= numberOfNodes; i++)
            {
                if (i % delimiterPC == 0)
                {
                    computerCounter++;
                    vertex = new PocVertex("Computer " + (computerCounter).ToString(), NetworkType.COMPUTER, false);
                }
                else
                {
                    routerCounter++;
                    vertex = new PocVertex("Router " + (routerCounter).ToString(), NetworkType.ROUTER, false);
                }

                pocVertices.Add(vertex);

                Graph.AddVertex(vertex);
            }

            int vertexCapacity = Graph.Vertices.Count();
            for (int routerIndex = 0; routerIndex < vertexCapacity; routerIndex++)
            {
                for (int joinIndex = routerIndex + 1; joinIndex < nodePower + routerIndex + 1; joinIndex++)
                {
                    AddNewGraphEdge(pocVertices[routerIndex], pocVertices[joinIndex % (vertexCapacity - 1)]);
                }
            }
            NotifyPropertyChanged("Graph");
        }

        #endregion

        #region Private Methods

        private PocEdge AddNewGraphEdge(PocVertex from, PocVertex to)
        {
            string edgeString = string.Format("{0}-{1}", from.ID, to.ID);
            PocEdge newEdge = new PocEdge(edgeString, from, to, weights[random.Next(0, weights.Count)].ToString());
            Graph.AddEdge(newEdge);
            return newEdge;
        }


        #endregion

        #region Public Properties

        public List<String> LayoutAlgorithmTypes
        {
            get { return layoutAlgorithmTypes; }
        }


        public string LayoutAlgorithmType
        {
            get { return layoutAlgorithmType; }
            set
            {
                layoutAlgorithmType = value;
                NotifyPropertyChanged("LayoutAlgorithmType");
            }
        }



        public PocGraph Graph
        {
            get { return graph; }
            set
            {
                graph = value;
                NotifyPropertyChanged("Graph");
            }
        }
        #endregion

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion
    }
}
