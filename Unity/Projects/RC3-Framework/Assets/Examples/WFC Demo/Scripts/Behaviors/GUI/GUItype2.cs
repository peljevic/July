
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
        // [SerializeField] private TileSet _tileSet;

        private int _unassigned;
        private int[] _counts;


        private void Start()
        {
            TakeTileCounter();

        }

        private void Update()
        {

        }

        void TakeTileCounter()
        {
            if (_counts == null)
            {
                _counts = _counter.CounterFull;

            }
        }

        private void OnGUI()
        {
            GUI.skin = mySkin;


            //GUI.Label(new Rect(new Vector2(Screen.width - 120, 100), new Vector2(250, 100)), "graph capacity : " + _graph.VertexCount.ToString());
            //GUI.Label(new Rect(new Vector2(Screen.width - 120, 120), new Vector2(250, 100)), "unassigned : " + _unassigned.ToString());

            for (int i = 0; i < _counts.Length; i++)
            {
                if (_counts[i] > 0)
                    GUI.Label(new Rect(new Vector2(Screen.width - 120, 140 + 20 * i), new Vector2(250, 100)), "tile type " + i + " : " + _counts[i].ToString());
            }

        }
    }
}
