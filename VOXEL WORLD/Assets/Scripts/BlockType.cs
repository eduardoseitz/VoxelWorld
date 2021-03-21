using UnityEngine;

[CreateAssetMenu(fileName = "NewBlock", menuName = "VOXEL WORLD/BlockType")]
public class BlockType : ScriptableObject
{
    #region Declarations
    public string screenName;
    // public enum Category{Solid, Liquid, Prop, Door, etc};
    // public int id;
    public int minLayer = 0;
    public int maxLayer = 100;

    [SerializeField] private Vector2 topUVPosition = new Vector2(0, 0);
    [SerializeField] private Vector2 sideUVPosition = new Vector2(0, 0);
    [SerializeField] private Vector2 bottomUVPosition = new Vector2(0, 0);

    private static int GRID_SIZE = 20;
    private static float BORDER_WIDTH = 0.002f;

    private Vector2[] _uv;
    #endregion

    #region Helper Methods
    public Vector2[] GetTopUV()
    {
        _uv = new Vector2[]{
            new Vector2((topUVPosition[0]+1) / GRID_SIZE  - BORDER_WIDTH, (topUVPosition[1]+1) / GRID_SIZE - BORDER_WIDTH),
            new Vector2((topUVPosition[0] / GRID_SIZE  + BORDER_WIDTH) , (topUVPosition[1]+1) / GRID_SIZE - BORDER_WIDTH),
            new Vector2((topUVPosition[0]) / GRID_SIZE + BORDER_WIDTH, (topUVPosition[1]) / GRID_SIZE + BORDER_WIDTH),
            new Vector2((topUVPosition[0]+1) / GRID_SIZE - BORDER_WIDTH, (topUVPosition[1]) / GRID_SIZE + BORDER_WIDTH)
        };

        return _uv;
    }

    public Vector2[] GetBottomUV()
    {
        _uv = new Vector2[]{
            new Vector2((bottomUVPosition[0]+1) / GRID_SIZE  - BORDER_WIDTH, (bottomUVPosition[1]+1) / GRID_SIZE - BORDER_WIDTH),
            new Vector2((bottomUVPosition[0] / GRID_SIZE  + BORDER_WIDTH) , (bottomUVPosition[1]+1) / GRID_SIZE - BORDER_WIDTH),
            new Vector2((bottomUVPosition[0]) / GRID_SIZE + BORDER_WIDTH, (bottomUVPosition[1]) / GRID_SIZE + BORDER_WIDTH),
            new Vector2((bottomUVPosition[0]+1) / GRID_SIZE - BORDER_WIDTH, (bottomUVPosition[1]) / GRID_SIZE + BORDER_WIDTH)
        };

        return _uv;
    }

    public Vector2[] GetSideUV()
    {
        _uv = new Vector2[]{
            new Vector2((sideUVPosition[0]+1) / GRID_SIZE  - BORDER_WIDTH, (sideUVPosition[1]+1) / GRID_SIZE - BORDER_WIDTH),
            new Vector2((sideUVPosition[0] / GRID_SIZE  + BORDER_WIDTH) , (sideUVPosition[1]+1) / GRID_SIZE - BORDER_WIDTH),
            new Vector2((sideUVPosition[0]) / GRID_SIZE + BORDER_WIDTH, (sideUVPosition[1]) / GRID_SIZE + BORDER_WIDTH),
            new Vector2((sideUVPosition[0]+1) / GRID_SIZE - BORDER_WIDTH, (sideUVPosition[1]) / GRID_SIZE + BORDER_WIDTH)
        };

        return _uv;
    }
    #endregion
}
