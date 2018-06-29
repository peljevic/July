using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RC3.Unity.WFCDemo
{
    public class GUItype2 : MonoBehaviour
    {
        [SerializeField] private GUISkin mySkin;
        [SerializeField] private TileTypeCounter _counter;
        [SerializeField] private GameObject _prefabCircleBar;
        // [SerializeField] private TileSet _tileSet;

        private int _unassigned;
        private int[] _counts;
        private int _totalArea = 0;
        private int _tilesTouchingGround = 0;
        private float _stabilityPercent = 0;

        public Texture _fillingTexture;

        private List<float> _stats;

        private void Start()
        {
            TakeTileCounter();

            
        }

        private void Update()
        {
            TakeRemainingStats();
        }

        void TakeTileCounter()
        {
            if (_counts == null)
            {
                _counts = _counter.CounterFull;

            }
        }

        void TakeRemainingStats()
        {
            _unassigned = _counter.RemainingPositions;
        
            _totalArea = _counter.CurrentArea;
       
            _tilesTouchingGround = _counter.GroundSupport;

            _stabilityPercent = _counter.PositionsOnGroundPercent;

           // TakeStats();

          // Debug.Log($" COUNTER |||||| total area {_totalArea}");
        }

        void TakeStats()
        {
            if(_stats==null)
            {
                _stats.Add(_unassigned);
                _stats.Add(_totalArea);
                _stats.Add(_tilesTouchingGround);
                _stats.Add(_stabilityPercent);
            }
        }

        private void OnGUI()
        {
            GUI.skin = mySkin;

            if (!_fillingTexture)
            {
                Debug.LogError("Assign a Texture in the inspector.");
                return;
            }

            GUI.DrawTextureWithTexCoords(new Rect(new Vector2(10,140), new Vector2(_stabilityPercent * 100, 10)), _fillingTexture, (new Rect(10, 10,_stabilityPercent*100, _stabilityPercent * 100)));//ScaleMode.ScaleToFit, true, 10.0F);

            //new Vector2(10, 140 + 20), new Vector2(250, 100)

            GUI.Label(new Rect(new Vector2(10, 140 + 20), new Vector2(250, 100)),$"Current area is {_totalArea}");
            GUI.Label(new Rect(new Vector2(10, 140 + 40), new Vector2(250, 100)), $"Tiles on the ground: {_tilesTouchingGround}");
            GUI.Label(new Rect(new Vector2(10, 140 + 60), new Vector2(250, 100)), $"Percent of grounded tiles: {_stabilityPercent*100}%");
            GUI.Label(new Rect(new Vector2(10, 140 + 80), new Vector2(250, 100)), $"Not yet assigned: {_unassigned}");

            //GUI.Label(new Rect(new Vector2(Screen.width - 120, 100), new Vector2(250, 100)), "graph capacity : " + _graph.VertexCount.ToString());
            //GUI.Label(new Rect(new Vector2(Screen.width - 120, 120), new Vector2(250, 100)), "unassigned : " + _unassigned.ToString());

            for (int i = 0; i < _counts.Length; i++)
            {
                if (_counts[i] > 0)
                  GUI.Label(new Rect(new Vector2(Screen.width - 120, 140 + 20 * i), new Vector2(250, 100)), $"tile type { i} {_counts[i]}");

                var bar  = Instantiate(_prefabCircleBar);
                bar.transform.position = new Vector3(Screen.width - 120, 140 + 20 * i);
            }

        }
    }
}