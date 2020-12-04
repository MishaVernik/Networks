using System;
using System.Windows;

namespace Networks.Algorithms
{
    public class DijkstrasAlgorithm
    {
        public static readonly int NO_PARENT = -1;

        // Function that implements Dijkstra's  
        // single source shortest path  
        // algorithm for a graph represented  
        // using adjacency matrix  
        // representation  
        public static void dijkstra(int[,] adjacencyMatrix, int startVertex)
        {
            try
            {
                int nVertices = adjacencyMatrix.GetLength(0);

                // shortestDistances[i] will hold the  
                // shortest distance from src to i  
                int[] shortestDistances = new int[nVertices];

                // added[i] will true if vertex i is  
                // included / in shortest path tree  
                // or shortest distance from src to  
                // i is finalized  
                bool[] added = new bool[nVertices];

                // Initialize all distances as  
                // INFINITE and added[] as false  
                for (int vertexIndex = 0; vertexIndex < nVertices; vertexIndex++)
                {
                    shortestDistances[vertexIndex] = int.MaxValue;
                    added[vertexIndex] = false;
                }

                // Distance of source vertex from  
                // itself is always 0  
                shortestDistances[startVertex] = 0;

                // Parent array to store shortest  
                // path tree  
                int[] parents = new int[nVertices];

                // The starting vertex does not  
                // have a parent  
                parents[startVertex] = NO_PARENT;
                // MessageBox.Show("WORKS");
                // Find shortest path for all  
                // vertices  
                for (int i = 1; i < nVertices; i++)
                {

                    // Pick the minimum distance vertex  
                    // from the set of vertices not yet  
                    // processed. nearestVertex is  
                    // always equal to startNode in  
                    // first iteration.  
                    int nearestVertex = -1;
                    int shortestDistance = int.MaxValue;
                    for (int vertexIndex = 0; vertexIndex < nVertices; vertexIndex++)
                    {
                        if (!added[vertexIndex] &&
                            shortestDistances[vertexIndex] <
                            shortestDistance)
                        {
                            nearestVertex = vertexIndex;
                            shortestDistance = shortestDistances[vertexIndex];
                        }
                    }

                    // Mark the picked vertex as  
                    // processed  
                    MessageBox.Show(nearestVertex.ToString());
                    added[nearestVertex] = true;

                    // Update dist value of the  
                    // adjacent vertices of the  
                    // picked vertex.  
                    for (int vertexIndex = 0; vertexIndex < nVertices; vertexIndex++)
                    {
                        //
                        int edgeDistance = adjacencyMatrix[nearestVertex, vertexIndex];

                        if (edgeDistance > 0
                            && ((shortestDistance + edgeDistance) <
                                shortestDistances[vertexIndex]))
                        {
                            parents[vertexIndex] = nearestVertex;
                            shortestDistances[vertexIndex] = shortestDistance +
                                                            edgeDistance;
                        }
                    }

                }

                printSolution(startVertex, shortestDistances, parents);
            }
            catch (Exception e)
            {

                throw e;
            }

            MessageBox.Show("WORKS3");
        }


        // A utility function to print  
        // the constructed distances  
        // array and shortest paths  
        public static void printSolution(int startVertex,
                                        int[] distances,
                                        int[] parents)
        {

            int nVertices = distances.Length;
            Console.Write("Vertex\t Distance\tPath");

            for (int vertexIndex = 0;
                    vertexIndex < nVertices;
                    vertexIndex++)
            {
                if (vertexIndex != startVertex)
                {
                    Console.Write("\n" + startVertex + " -> ");
                    Console.Write(vertexIndex + " \t\t ");
                    Console.Write(distances[vertexIndex] + "\t\t");
                    printPath(vertexIndex, parents);
                }
            }
        }

        // Function to print shortest path  
        // from source to currentVertex  
        // using parents array  
        public static void printPath(int currentVertex,
                                    int[] parents)
        {

            // Base case : Source node has  
            // been processed  
            if (currentVertex == NO_PARENT)
            {
                return;
            }
            printPath(parents[currentVertex], parents);
            Console.Write(currentVertex + " ");
        }

    }
}
