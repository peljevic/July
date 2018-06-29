using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RC3.Unity.WFCDemo
{
    /// <summary>
    /// 
    /// </summary>
    public class VertexObject : RC3.Unity.VertexObject
    {
        [SerializeField] private Tile _tile;

        private GameObject _child;
        private MeshFilter _filter;
        private MeshRenderer _renderer;
        private Vector3 _scale;

        private Rigidbody _rigidbody;
        private MeshCollider _collider;
        private bool _kinematicCount = false;

        private float _height;
        private int _areaValue = 0;
        private float _sunExposureValue;
        private int _suncollisions = 0;

        private int _tileIndex = 0;
        private Vector3 _position;


        private List<FixedJoint> _joints;

        //On domains!!!
        //private VertexObject[] _neighbors;
        private float _neighDensity = 0;
        private int _neighPopulation = 0;

        /// <summary>
        /// 
        /// </summary>
        public Tile Tile
        {
            get { return _tile; }
            set
            {
                _tile = value;
                OnSetTile();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private void Awake()
        {
            // _neighbors = new VertexObject[14];

            _child = transform.GetChild(0).gameObject;
            _filter = GetComponent<MeshFilter>();
            _renderer = GetComponent<MeshRenderer>();
            _scale = transform.localScale;

            //added
            _rigidbody = GetComponent<Rigidbody>();
            _height = GetComponent<Transform>().position.y;
            _collider = GetComponent<MeshCollider>();
            _collider.enabled = false;
            _position = transform.position;

            _joints = new List<FixedJoint>();

            // _tileTypeCounter = GetComponent<TileTypeCounter>();

            OnSetTile();
        }

        public int TileIndex
        {
            get { return _tileIndex; }
            set { _tileIndex = value; }
        }

        public Vector3 Position
        {
            get { return _position; }
            set { value = _position; }
        }

        public void Joints(FixedJoint joint)
        {
            _joints.Add(joint);
        }

        public List<FixedJoint> GetJoints
        {
            get { return _joints; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        private void OnSetTile()
        {
            transform.localScale = _scale;

            if (_tile == null)
            {
                _filter.sharedMesh = null;
                _renderer.sharedMaterial = null;
                _child.SetActive(true);
                return;
            }

            _filter.sharedMesh = _tile.Mesh;
            _renderer.sharedMaterial = _tile.Material;
            _rigidbody.drag = _tile.Drag;
            _areaValue = _tile.Area;
            _tile.CountThisType++;

            var colMesh = _tile.Mesh;
            if (_filter.sharedMesh != null)
            {
                _collider.sharedMesh = _tile.Mesh;
                _collider.enabled = false;
            }

            _child.SetActive(false);
        }

        public int SunCollisions
        {
            get { return _suncollisions; }
            set { _suncollisions = value; }
        }

        public float Height
        {
            set { _height = value; }
            get { return _height; }
        }

        public int Area
        {
            set { _areaValue = value; }
            get { return _areaValue; }
        }

        public Rigidbody Body
        {
            get { return _rigidbody; }
        }

        public float Velocity
        {
            get { return _rigidbody.velocity.magnitude; }
        }

        public MeshCollider Collider
        {
            get { return _collider; }
        }

        public MeshRenderer Renderer
        {
            get { return _renderer; }
        }

        public float Density
        {
            get { return _neighDensity; }

        }

        public int NeighbourPopulation
        {
            get { return _neighPopulation; }

        }

        public void AddedNeighbour()
        {
            _neighPopulation++;
            _neighDensity = _neighPopulation / 14 * 100;

        }

        public void AvailableNeigbours()
        {
            _neighPopulation++;
            _neighDensity = _neighPopulation / 14 * 100;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="size"></param>
        public void Reduce(float factor)
        {
            transform.localScale = _scale * factor;
        }


        /// <summary>
        /// 
        /// </summary>
        public void Collapse()
        {
            // TODO reset scale and swap symbol
        }
    }
}
