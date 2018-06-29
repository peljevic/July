using System;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using RC3.Graphs;
using RC3.WFC;
using RC3.Unity.WFCDemo;


namespace RC3.Graphs
{

    [CreateAssetMenu(menuName = "RC3/WFC Demo/GraphAnalysisManager")]
    public class GraphAnalysisManager : MonoBehaviour
    {
        #region Member Variables
        private ProcessingUtil _graphprocessing = new ProcessingUtil();

        [SerializeField]
        private TileGraphExtractor _graphExtractor;

        [SerializeField]
        private TileModelManager _tileModelManager;
        private TileModel _tilemodel;

        [SerializeField]
        private SharedAnalysisEdgeGraph _analysisGraph;
        private GraphVisualizer _graphvisualizer;

        private int _graphviz = 0;

        #endregion

        #region Constructors
        private void Awake()
        {
            if (_tileModelManager != null)
            {
                _tilemodel = _tileModelManager.TileModel;
            }

            _analysisGraph.Initialize();

            _graphvisualizer = GetComponent<GraphVisualizer>();
        }

        #endregion

        #region Public Methods
        #endregion

        #region Private Methods

        void Start()
        {
            EdgeGraph testgraph = new EdgeGraph(15);
            ProcessingUtil graphprocessing = new ProcessingUtil();

            for (int i = 0; i < 20; i++)
            {
                testgraph.AddVertex();
            }

            //TestGraph | Build graph
            //first connected component set of edges
            testgraph.AddEdge(1, 2);
            testgraph.AddEdge(2, 3);
            testgraph.AddEdge(3, 1);
            testgraph.AddEdge(3, 5);
            testgraph.AddEdge(5, 6);
            testgraph.AddEdge(6, 4);
            testgraph.AddEdge(4, 5);
            testgraph.AddEdge(7, 6);
            testgraph.AddEdge(3, 8);
            testgraph.AddEdge(8, 1);
            testgraph.AddEdge(6, 9);
            testgraph.AddEdge(9, 4);
            testgraph.AddEdge(8, 10);
            testgraph.AddEdge(10, 11);
            testgraph.AddEdge(11, 12);
            testgraph.AddEdge(12, 10);

            //second component (not connected to first) 
            testgraph.AddEdge(15, 18);
            testgraph.AddEdge(18, 19);
            testgraph.AddEdge(17, 18);

            //TestGraph | Analysis
            int componentcount = 0;
            int closurecount = 0;
            List<HashSet<int>> components = new List<HashSet<int>>();
            _graphprocessing.CountClosures(testgraph, out componentcount, out closurecount, out components);
            float closurerate = (float)closurecount / (float)testgraph.EdgeCount;

            //TestGraph | Debug Print Results
            Debug.Log("TestGraph | Components Count = " + componentcount);
            for (int i = 0; i < components.Count; i++)
            {
                HashSet<int> set = components[i];
                string setstring = string.Join(",", components[i]);
                Debug.Log("TestGraph | ConnectedComponent# " + (i + 1) + " = " + setstring);
            }

            float[] normalizedcomponents = _graphprocessing.RemapComponentsToArray(testgraph, components);
            string normalizedcomponentsstring = string.Join(",", normalizedcomponents);
            Debug.Log("TestGraph | NormalizedComponents = " + normalizedcomponentsstring);
            Debug.Log($"TestGraph | NormalizedComponentsCount =  {normalizedcomponents.ToArray().Length}");
            Debug.Log("TestGraph | Closures Count = " + closurecount);
            Debug.Log("TestGraph | Closures Rate = " + closurerate);

        }

        void Update()
        {
            KeyPressMethod();
        }

        private void UpdateAnalysis()
        {

            if (_graphExtractor != null && _tileModelManager != null)
            {
                if (_tileModelManager.Status == CollapseStatus.Complete)
                {
                    _graphExtractor.ExtractSharedEdgeGraph(_analysisGraph);

                    //Extracted Graph | Analysis
                    //analyze/get # of closures / strongly connected components
                    int closurecount = 0;
                    int componentcount = 0;
                    List<HashSet<int>> connectedComponents = new List<HashSet<int>>();
                    _graphprocessing.CountClosures(_analysisGraph.Graph, out componentcount, out closurecount, out connectedComponents);
                    float closureRate = (float)closurecount / (float)_analysisGraph.Graph.EdgeCount;

                    Debug.Log($"GRAPH ANALYSIS MANAGER | CONNECTED COMPONENTS COUNT {connectedComponents.Count}");


                    //normalized/remapped components to an array for graph coloring
                    float[] normalizedcomponents = _graphprocessing.RemapComponentsToArray(_analysisGraph.Graph, connectedComponents);
                    float[] normalizedcomponentsbysize = _graphprocessing.RemapComponentsSizeToArray(_analysisGraph.Graph, connectedComponents);

                    //analyze/get 1) ground support sources, 2) list of vertex depths 3) max depth 
                    List<int> sources = _graphprocessing.GetGroundSources(_analysisGraph.Graph, _analysisGraph.Vertices, 2f);
                    int[] depths = _graphprocessing.DepthsFromGroundSources(_analysisGraph.Graph, _analysisGraph.Vertices, 2f);
                    int maxdepth = _graphprocessing.MaxDepth(depths);

                    //analyze/get 1) unreachable vertices, 2) remapped vertex depths between 0,1, 3) edgeless vertices
                    float[] normalizeddepths = new float[_analysisGraph.Graph.VertexCount];
                    List<int> unreachablevertices = new List<int>();
                    List<int> edgelessvertices = new List<int>();
                    _graphprocessing.RemapGraphDepths(_analysisGraph.Graph, depths, 0, 1, out normalizeddepths, out unreachablevertices, out edgelessvertices);

                    //store analysis in the shared analysis graph scriptable object - VIEW THIS DATA ON A UI CANVAS
                    _analysisGraph.ClosuresCount = closurecount;
                    _analysisGraph.ConnectedComponents = connectedComponents;
                    _analysisGraph.NormalizedComponents = normalizedcomponents;
                    _analysisGraph.NormalizedComponentsBySize = normalizedcomponentsbysize;
                    _analysisGraph.ConnectedComponentsCount = componentcount;
                    _analysisGraph.Sources = sources;
                    _analysisGraph.Depths = depths;
                    _analysisGraph.NormalizedDepths = normalizeddepths;
                    _analysisGraph.MaxDepth = maxdepth;
                    _analysisGraph.UnreachableVertices = unreachablevertices;
                    _analysisGraph.EdgelessVertices = edgelessvertices;

                    //Extracted Graph | Debug Print Results
                    Debug.Log("Exracted Graph | ComponentsCount = " + _analysisGraph.ConnectedComponentsCount);
                    for (int i = 0; i < connectedComponents.Count; i++)
                    {
                        HashSet<int> set = connectedComponents[i];
                        string setstring = string.Join(",", connectedComponents[i]);
                        Debug.Log("Exracted Graph | ConnectedComponent# " + (i + 1) + " = " + setstring);
                    }

                    string normalizedcomponentsstring = string.Join(",", _analysisGraph.NormalizedComponents);
                    string normalizedcomponentsbysizestring = string.Join(",", _analysisGraph.NormalizedComponentsBySize);

                    Debug.Log("Exracted Graph | NormalizedComponents = " + normalizedcomponentsstring);
                    Debug.Log("Exracted Graph | NormalizedComponentsBySize = " + normalizedcomponentsbysizestring);

                    Debug.Log("Exracted Graph | Closures Count = " + _analysisGraph.ClosuresCount);
                    Debug.Log("Exracted Graph | Closures Rate = " + _analysisGraph.ClosureRate);

                    string sourcesstring = string.Join(",", _analysisGraph.Sources);
                    string depthsstring = string.Join(",", _analysisGraph.Depths);
                    Debug.Log("Exracted Graph | Depths = " + depthsstring);
                    Debug.Log("Exracted Graph | Max Depth = " + _analysisGraph.MaxDepth);
                    Debug.Log("Exracted Graph | SourcesCount = " + _analysisGraph.SourcesCount);
                    Debug.Log("Exracted Graph | Sources = " + sourcesstring);

                    string normalizeddepthsstring = string.Join(",", _analysisGraph.NormalizedDepths);
                    string unreachablevrtsstring = string.Join(",", _analysisGraph.UnreachableVertices);
                    string edgelessvrtsstring = string.Join(",", _analysisGraph.EdgelessVertices);

                    Debug.Log("Exracted Graph | Normalized Depths = " + normalizeddepthsstring);
                    Debug.Log("Exracted Graph | Unreachable Vertices Count = " + _analysisGraph.UnreachableVerticesCount);
                    Debug.Log("Exracted Graph | Unreachable Vertices = " + unreachablevrtsstring);
                    Debug.Log("Exracted Graph | Edgeless Vertices Count = " + _analysisGraph.EdgelessVerticesCount);
                    Debug.Log("Exracted Graph | Edgeless Vertices = " + edgelessvrtsstring);

                }
            }
            else
            {
                Debug.Log("No Graph Extractor OR WFC Incomplete");
            }
        }

        private void UpdateGraphMesh()
        {
            if (_graphvisualizer != null)
            {
                _graphvisualizer.CreateMesh();
            }

            else
            {
                Debug.Log("No Graph Visualizer Component Attached!");
            }
        }


        private void KeyPressMethod()
        {
            if (Input.GetKeyDown(KeyCode.U))
            {
                UpdateAnalysis();
                UpdateGraphMesh();
            }

            if (Input.GetKeyDown(KeyCode.V))
            {
                if (_graphviz < 3)
                {
                    _graphviz++;
                    if (_graphviz == 1)
                    {
                        _graphvisualizer.VizMode = GraphVisualizer.RenderMode.Components;

                    }

                    if (_graphviz == 2)
                    {
                        _graphvisualizer.VizMode = GraphVisualizer.RenderMode.ComponentsSize;
                    }

                    if (_graphviz == 3)
                    {
                        _graphvisualizer.VizMode = GraphVisualizer.RenderMode.StressAnalysis;
                    }

                }
                else
                {
                    _graphviz = 0;
                    _graphvisualizer.VizMode = GraphVisualizer.RenderMode.DepthFromSource;
                }

                _graphvisualizer.SetVizColors();
            }
        }

        #endregion

        #region Public Properties

        public IEdgeGraph Graph
        {
            get { return _analysisGraph.Graph; }
        }

        public SharedAnalysisEdgeGraph AnalysisGraph
        {
            get { return _analysisGraph; }
        }

        #endregion

    }
}
