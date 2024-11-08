﻿using UnityEngine;

namespace DevPenguin.VOXELWORLD
{
    [CreateAssetMenu(fileName = "NewBlock", menuName = "VOXEL WORLD/Block")]
    public class Block : ScriptableObject
    {
        #region Declarations
        public int blockType = 0; // -1 equals air
        public string screenName;
        // public enum Category{Solid, Liquid, Prop, Door, etc};
        // public int id;
        public int minDepthLayer = 0;
        public int maxDepthLayer = 100;
        [Range(0.01f,1f)]
        public float generationChance = 1;

        [SerializeField] private Vector2 topUVPosition = new Vector2(0, 0);
        [SerializeField] private Vector2 sideUVPosition = new Vector2(0, 0);
        [SerializeField] private Vector2 bottomUVPosition = new Vector2(0, 0);

        private static int GRID_SIZE = 16;
        private static float BORDER_WIDTH = 0.0024f;

        private Vector2[] _uv;
        #endregion

        #region Helper Methods
        public Vector2[] GetTopUV()
        {
            _uv = new Vector2[]
            {
                new Vector2((topUVPosition[0]+1) / GRID_SIZE  - BORDER_WIDTH, (topUVPosition[1]+1) / GRID_SIZE - BORDER_WIDTH),
                new Vector2((topUVPosition[0] / GRID_SIZE  + BORDER_WIDTH) , (topUVPosition[1]+1) / GRID_SIZE - BORDER_WIDTH),
                new Vector2((topUVPosition[0]) / GRID_SIZE + BORDER_WIDTH, (topUVPosition[1]) / GRID_SIZE + BORDER_WIDTH),
                new Vector2((topUVPosition[0]+1) / GRID_SIZE - BORDER_WIDTH, (topUVPosition[1]) / GRID_SIZE + BORDER_WIDTH)
            };

            return _uv;
        }

        public Vector2[] GetBottomUV()
        {
            _uv = new Vector2[]
            {
                new Vector2((bottomUVPosition[0]+1) / GRID_SIZE  - BORDER_WIDTH, (bottomUVPosition[1]+1) / GRID_SIZE - BORDER_WIDTH),
                new Vector2((bottomUVPosition[0] / GRID_SIZE  + BORDER_WIDTH) , (bottomUVPosition[1]+1) / GRID_SIZE - BORDER_WIDTH),
                new Vector2((bottomUVPosition[0]) / GRID_SIZE + BORDER_WIDTH, (bottomUVPosition[1]) / GRID_SIZE + BORDER_WIDTH),
                new Vector2((bottomUVPosition[0]+1) / GRID_SIZE - BORDER_WIDTH, (bottomUVPosition[1]) / GRID_SIZE + BORDER_WIDTH)
            };

            return _uv;
        }

        public Vector2[] GetSideUV()
        {
            _uv = new Vector2[]
            {
                new Vector2((sideUVPosition[0]+1) / GRID_SIZE  - BORDER_WIDTH, (sideUVPosition[1]+1) / GRID_SIZE - BORDER_WIDTH),
                new Vector2((sideUVPosition[0] / GRID_SIZE  + BORDER_WIDTH) , (sideUVPosition[1]+1) / GRID_SIZE - BORDER_WIDTH),
                new Vector2((sideUVPosition[0]) / GRID_SIZE + BORDER_WIDTH, (sideUVPosition[1]) / GRID_SIZE + BORDER_WIDTH),
                new Vector2((sideUVPosition[0]+1) / GRID_SIZE - BORDER_WIDTH, (sideUVPosition[1]) / GRID_SIZE + BORDER_WIDTH)
            };

            return _uv;
        }
        #endregion
    }
}
