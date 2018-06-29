using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RC3.Graphs;
using RC3.WFC;


using SpatialSlur.Core;
using SpatialSlur.Field;
using SpatialSlur.Unity;

namespace RC3.Unity.WFCDemo
{
    public class ColorJoints : MonoBehaviour
    {
        public Color[] Spectrum;

        [Range(0.0f, 10000.0f)]
        public float MaxForce = 1000.0f;

        [Range(0.0f, 10000.0f)]
        public float MaxTorque = 1000.0f;

        private float BreakForce = Mathf.Infinity;
        private float BreakTorque = Mathf.Infinity;

        private Rigidbody[] _bodies;
        private Material[] _materials;
        private List<FixedJoint> _joints;
        //private FixedJoint[][] _joints; // TODO maintain array of joints per body, colour bodies by average torque/force

        [SerializeField] private SharedDigraph _grid;
        [SerializeField] private TileSet _tileSet;
        [SerializeField] private float _maxSpeed = 0;

        private Vector3[] _positions;

        List<VertexObject> _verts;
        private Digraph _graph;

        private CollapseStatus _status;
        private int _kinematicTiles = 0;
        private float _kinematicPercent = 0;


        private void Awake()
        {
            _graph = _grid.Graph;
            _verts = _grid.VertexObjects;

            _bodies = new Rigidbody[_verts.Count];
            _joints = new List<FixedJoint>();
            Spectrum = new Color[_verts.Count];

        }

        /// <summary>
        /// This was START
        /// 
        /// </summary>
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.X))
            {
                CacheRigidbodies();
                AddJoints();
                CacheJoints();
                CacheMaterials();
                AddGravity();

                StartCoroutine(UpdateBodyColors());
            }
        }

        void AddGravity()
        {
            foreach(var v in _verts)
            {
                v.Body.useGravity = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Filter(Vector3 vector)
        {
            const float f = 2.0f * Mathf.PI;
            vector.x *= f; // CountX;
            vector.y *= f; // CountY;
            vector.z *= f; // CountZ;

            //return ImplicitSurfaces.Gyroid(vector) > 0.5;
            return ImplicitSurfaces.Diamond(vector) > 0.6;
        }

        private void CacheRigidbodies()
        {
            for (int i = 0; i < _verts.Count; i++)
            {
                if (_verts[i].Body == null) continue;

                else { _bodies[i] = _verts[i].Body; }
            }
        }

        private void AddJoints()
        {
            for (int i = 0; i < _verts.Count; i++)
            {
                var v = _verts[i];

                var allNeigh = v.Tile.Labels;

                for (int j = 0; j < allNeigh.Length; j++)
                {
                    var neighbour = _graph.GetVertexNeighborOut(i, j);
                    var vn = _verts[neighbour];
                    //int dirCounter = 0;

                    if (allNeigh[j] != "0" && v != vn)
                    {
                        var vJoint = v.gameObject.AddComponent<FixedJoint>();
                        vJoint.connectedBody = vn.GetComponent<Rigidbody>();

                        vJoint.breakForce = BreakForce;
                        vJoint.breakTorque = BreakTorque;

                        v.Joints(vJoint);
                        //StoreJoints(vJoint, i, dirCounter);

                    }
                }
            }
        }

        //private void StoreJoints(FixedJoint joint, int vertex, int count)
        //{
        //    // _joints[vertex][count] = joint; 
        //}

        private void CacheJoints()
        {
            foreach (var v in _verts)
            {
                var joints = v.GetJoints;

                foreach (var j in joints)
                {
                    _joints.Add(j);
                    Debug.Log($"Joints {_joints.Count}");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void CacheMaterials()
        {
            _materials = new Material[_verts.Count];

            for (int i = 0; i <_verts.Count; i++)
            {
                var v = _verts[i];

                var m = _materials[i] = v.GetComponent<MeshRenderer>().material;
                m.color = Spectrum[0];
            }

            //for (int i = 0; i < _bodies.Length; i++)
            //{
            //    var b = _bodies[i];
            //    if (b == null) continue;

                //    var m = _materials[i] = b.gameObject.GetComponent<MeshRenderer>().material;
                //    m.color = Spectrum[0];
                //}
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="selection"></param>
        private void Fix(IEnumerable<int> selection)
        {
            foreach (var b in _bodies.TakeSelection(selection))
            {
                if (b == null) continue;
                b.isKinematic = true;
                //SetMaterial(b.gameObject);
            }
        }
        
        /*
        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameObject"></param>
        private void SetMaterial(GameObject gameObject)
        {
            var material = gameObject.GetComponent<MeshRenderer>().material;
            material.color = new Color32(255, 0, 100, 0);
        }
        */
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator UpdateBodyColors()
        {
            const float factor = 0.75f;

            while (true)
            {
                for (int i = 0; i < _materials.Length; i++)
                {
                    var m = _materials[i];

                    if (m != null)
                        m.color = Color.Lerp(m.color, GetTorqueColor(i), factor);
                }

                yield return new WaitForSeconds(0.1f);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private Color GetTorqueColor(int index)
        {
            var v = _verts[index];
            var joints = v.GetJoints;
               

            float sum = 0.0f;
            int count = 0;

            foreach (var j in joints)
            {
                if (j != null)
                {
                    sum += j.currentTorque.sqrMagnitude;
                    count++;
                }
            }

            if (count == 0)
                return Spectrum[0];

            return Lerp(Spectrum, sum / (count * MaxTorque));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private Color GetForceColor(int index)
        {
            var v = _verts[index];
            var joints = v.GetJoints;


            float sum = 0.0f;
            int count = 0;

            foreach (var j in joints)
            {
                if (j != null)
                {
                    sum += j.currentForce.sqrMagnitude;
                    count++;
                }
            }

            if (count == 0)
                return Spectrum[0];

            return Lerp(Spectrum, sum / (count * MaxTorque));
        }


        /// <summary>
        /// TODO move into SlurUnity
        /// </summary>
        /// <param name="colors"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        public static Color Lerp(IReadOnlyList<Color> colors, float factor)
        {
            int last = colors.Count - 1;
            int i;
          //  object SlurMathf = null;
            factor = SlurMath.Fract(factor * last, out i);
                //Fract(factor * last, out i);

            if (i < 0)
                return colors[0];
            else if (i >= last)
                return colors[last];

            return Color.LerpUnclamped(colors[i], colors[i + 1], factor);
        }


        /// <summary>
        /// 
        /// </summary>
        public void ResetGrid()
        {
           // ResetBodies();
            // ResetJoints();
        }



#if (false)

          private void CreateJoints()
        {
            _joints = new FixedJoint[_bodies.Length][];

            for (int i = 0; i < _bodies.Length; i++)
                _joints[i] = new FixedJoint[6];

            int countXY = CountX * CountY;
            int index = 0;

            for (int k = 0; k < CountZ; k++)
            {
                for (int j = 0; j < CountY; j++)
                {
                    for (int i = 0; i < CountX; i++, index++)
                    {
                        var bodyA = _bodies[index];

                        if (bodyA == null)
                            continue;

                        var jointsA = _joints[index];

                        // -x
                        if (i > 0)
                        {
                            var bodyB = _bodies[index - 1];

                            if (bodyB != null)
                                ConnectBodies(bodyA, jointsA, bodyB, _joints[index - 1], 0);
                        }

                        // -y
                        if (j > 0)
                        {
                            var bodyB = _bodies[index - CountX];

                            if (bodyB != null)
                                ConnectBodies(bodyA, jointsA, bodyB, _joints[index - CountX], 1);
                        }

                        // -z
                        if (k > 0)
                        {
                            var bodyB = _bodies[index - countXY];

                            if (bodyB != null)
                                ConnectBodies(bodyA, jointsA, bodyB, _joints[index - countXY], 2);
                        }
                    }
                }
            }
        }



                /// <summary>
        /// 
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        private void ConnectBodies(Rigidbody bodyA, Rigidbody bodyB)
        {
            var joint = bodyA.gameObject.AddComponent<FixedJoint>();
            joint.connectedBody = bodyB;

            // joint.breakForce = BreakForce;
            // joint.breakTorque = BreakTorque;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        private void ConnectBodies(Rigidbody bodyA, FixedJoint[] jointsA, Rigidbody bodyB, FixedJoint[] jointsB, int index)
        {
            var joint = bodyA.gameObject.AddComponent<FixedJoint>();
            joint.connectedBody = bodyB;

            joint.breakForce = BreakForce;
            joint.breakTorque = BreakTorque;

            jointsA[index] = jointsB[index + 3] = joint;
        }



        /*
        /// <summary>
        /// 
        /// </summary>
        private void ResetJoints()
        {
            for(int i = 0; i < _joints.Length; i++)
            {
                var joints = _joints[i];
                if (joints == null) continue;
                for(int j = 0; j < 3; j++)
                {
                    var j = joints[j];
                    if (j == null) continue;
                    // TODO repair joint? check API
                }
            }
        }
        */

        #region Selection Enumerators

        /// <summary>
        /// 
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        private IEnumerable<int> LayerXY(int k)
        {
            int offset = k * CountX * CountY;

            for (int j = 0; j < CountY; j++)
            {
                for (int i = 0; i < CountX; i++)
                    yield return i + j * CountX + offset;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        private IEnumerable<int> LayerXZ(int j)
        {
            int offset = j * CountX;
            int countXY = CountX * CountY;

            for (int k = 0; k < CountZ; k++)
            {
                for (int i = 0; i < CountX; i++)
                    yield return i + offset + k * countXY;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private IEnumerable<int> Block(int i0, int j0, int k0, int i1, int j1, int k1)
        {
            int countXY = CountX * CountY;

            for (int k = k0; k < k1; k++)
            {
                for (int j = j0; j < j1; j++)
                {
                    for (int i = i0; i < i1; i++)
                        yield return i + j * CountX + k * countXY;
                }
            }
        }

        #endregion
#endif
    }
}

///// <summary>
///// 
///// </summary>
//private void CreateGrid(Func<Vector3, bool> predicate)
//{
//    _bodies = new Rigidbody[CountX * CountY * CountZ];
//    int index = 0;

//    for (int k = 0; k < CountZ; k++)
//    {
//        for (int j = 0; j < CountY; j++)
//        {
//            for (int i = 0; i < CountX; i++, index++)
//            {
//                var p = new Vector3(i, j, k);
//                if (!predicate(p)) continue;

//                var body = Instantiate(Prefab, transform);
//                body.transform.localPosition = p;

//                _bodies[index] = body;
//            }
//        }
//    }
//}

/*
  /// <summary>
  /// 
  /// </summary>
  private void CreateJoints()
  {
      int countXY = CountX * CountY;
      int index = 0;
      for (int k = 0; k < CountZ; k++)
      {
          for (int j = 0; j < CountY; j++)
          {
              for (int i = 0; i < CountX; i++, index++)
              {
                  var bodyA = _bodies[index];
                  if (bodyA == null) continue;
                  // -x
                  if(i > 0)
                  {
                      var bodyB = _bodies[index - 1];
                      if (bodyB != null) ConnectBodies(bodyA, bodyB);
                  }
                  // -y
                  if (j > 0)
                  {
                      var bodyB = _bodies[index - CountX];
                      if (bodyB != null) ConnectBodies(bodyA, bodyB);
                  }
                  // -z
                  if (k > 0)
                  {
                      var bodyB = _bodies[index - countXY];
                      if (bodyB != null) ConnectBodies(bodyA, bodyB);
                  }
              }
          }
      }
  }
  */
