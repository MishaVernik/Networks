using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using GraphSharp.Controls;

namespace Networks
{
    public class PocGraphLayout : GraphLayout<PocVertex, PocEdge, PocGraph> { }



    public class MainWindowViewModel : INotifyPropertyChanged
    {
        #region Data

        private string layoutAlgorithmType;
        private PocGraph graph;
        private List<String> layoutAlgorithmTypes = new List<string>();
        private int count;
        #endregion

        #region Ctor
        public MainWindowViewModel()
        {
            Graph = new PocGraph(true);


            List<PocVertex> existingVertices = new List<PocVertex>();
            existingVertices.Add(new PocVertex("Router 1", true)); //0
            existingVertices.Add(new PocVertex("Router 2", false)); //1
            existingVertices.Add(new PocVertex("Router 3", true)); //2
            existingVertices.Add(new PocVertex("Router 4", true)); //3


            foreach (PocVertex vertex in existingVertices)
                Graph.AddVertex(vertex);


            //add some edges to the graph
            AddNewGraphEdge(existingVertices[0], existingVertices[1]);
            AddNewGraphEdge(existingVertices[0], existingVertices[2]);
            AddNewGraphEdge(existingVertices[0], existingVertices[3]);
            AddNewGraphEdge(existingVertices[3], existingVertices[1]);

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

            //Pick a default Layout Algorithm Type
            LayoutAlgorithmType = "LinLog";
        }
        #endregion

        public void AddNewVertex()
        {
            var vertex = new PocVertex("Router " + (this.Graph.VertexCount + 1).ToString(), true);
            Graph.AddVertex(vertex);
            //AddNewGraphEdge(existingVertices[0], existingVertices[1]);
        }
        public void ReLayoutGraph()
        {
            graph = new PocGraph(true);
            count++;



            List<PocVertex> existingVertices = new List<PocVertex>();
            existingVertices.Add(new PocVertex(String.Format("Barn Rubble{0}", count), true)); //0
            existingVertices.Add(new PocVertex(String.Format("Frank Zappa{0}", count), false)); //1
            existingVertices.Add(new PocVertex(String.Format("Gerty CrinckleBottom{0}", count), true)); //2


            foreach (PocVertex vertex in existingVertices)
                Graph.AddVertex(vertex);


            //add some edges to the graph
            AddNewGraphEdge(existingVertices[0], existingVertices[1]);
            AddNewGraphEdge(existingVertices[0], existingVertices[2]);


            NotifyPropertyChanged("Graph");
        }



        #region Private Methods
        private PocEdge AddNewGraphEdge(PocVertex from, PocVertex to)
        {
            string edgeString = string.Format("{0}-{1} Connected", from.ID, to.ID);

            PocEdge newEdge = new PocEdge(edgeString, from, to);
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
