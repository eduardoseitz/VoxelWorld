using UnityEngine;

[CreateAssetMenu(fileName = "NewBlock", menuName = "VOXEL WORLD/BlockType")]
public class BlockType : ScriptableObject
{
    public string screenName;

    // Texture cordinates
    
    public Vector2[] topUVs = { new Vector2(0.05f, 0.05f), new Vector2(0.0f, 0.05f), new Vector2(0.0f, 0.0f), new Vector2(0.05f, 0.0f) };
    public Vector2[] bottomUVs = { new Vector2(0.05f, 0.05f), new Vector2(0.0f, 0.05f), new Vector2(0.0f, 0.0f), new Vector2(0.05f, 0.0f) };
    public Vector2[] sideUVs = { new Vector2(0.05f, 0.05f), new Vector2(0.0f, 0.05f), new Vector2(0.0f, 0.0f), new Vector2(0.05f, 0.0f) };

    
    //private static int BLOCKS_IN_GRID = 20;
    //[SerializeField] private Vector2 topUVPosition = new Vector2(0, 0);

    //private Vector2[] _uv;

    //public Vector2[] GetTopUV()
    //{
    //    _uv = new Vector2[]{
    //        new Vector2((topUVPosition[0]+1) / BLOCKS_IN_GRID , (topUVPosition[1]+1) / BLOCKS_IN_GRID),
    //        new Vector2((topUVPosition[0]) / BLOCKS_IN_GRID, (topUVPosition[1]+1) / BLOCKS_IN_GRID),
    //        new Vector2((topUVPosition[0]) / BLOCKS_IN_GRID, (topUVPosition[1]) / BLOCKS_IN_GRID),
    //        new Vector2((topUVPosition[0]+1) / BLOCKS_IN_GRID, (topUVPosition[1]) / BLOCKS_IN_GRID)
    //    };

    //    return _uv;
    //}
}
