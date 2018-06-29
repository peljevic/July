using System.Linq;

namespace RC3.Unity.WFCDemo
{
    public class RemapJoints : RC3.Graphs.ProcessingUtil
    {
       SharedDigraph _sharedDigraph;
       SharedAnalysisEdgeGraph _analysisGraph;

       private float[] _averageForces;
       private float[] _averageTorques;
       private float _maxForce;
       private float _minForce;
       private float _maxTorque;
       private float _minTorque;


        public void CollectStructureInformation()
        {
            for(int i=0; i< _sharedDigraph.VertexObjects.Count; i++)
            {
                var v = _sharedDigraph.VertexObjects[i];
                _analysisGraph.ForceStress[i] = _averageForces[i] = v.JointAvgForce();
                _analysisGraph.TorqueStress[i] = _averageTorques[i] = v.JointAvgTorque();
                        
               _analysisGraph.MaxForce = _maxForce = _averageForces.Max();
               _analysisGraph.MaxTorque = _maxTorque = _averageTorques.Max();

                _minForce = _averageForces.Min();
                _minTorque = _averageTorques.Min();


            }
        }

        public void Remap()
        {
            CollectStructureInformation();
            RemapValues(_averageForces, _minForce, _maxForce);
            RemapValues(_averageTorques, _minTorque, _maxTorque);
        }
    }
}