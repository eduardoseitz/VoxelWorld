using UnityEngine;

[CreateAssetMenu(fileName = "NewBlock", menuName = "VOXEL WORLD/BlockType")]
public class BlockType : ScriptableObject
{
    public string blockName;

    // Texture cordinates
    public Vector2[] topUVs = { new Vector2(0.05f, 0.05f), new Vector2(0.0f, 0.05f), new Vector2(0.0f, 0.0f), new Vector2(0.05f, 0.0f) };
    public Vector2[] bottomUVs = { new Vector2(0.05f, 0.05f), new Vector2(0.0f, 0.05f), new Vector2(0.0f, 0.0f), new Vector2(0.05f, 0.0f) };
    public Vector2[] sideUVs = { new Vector2(0.05f, 0.05f), new Vector2(0.0f, 0.05f), new Vector2(0.0f, 0.0f), new Vector2(0.05f, 0.0f) };
    //public Vector2[] frontUVs = { new Vector2(0.05f, 0.05f), new Vector2(0.0f, 0.05f), new Vector2(0.0f, 0.0f), new Vector2(0.05f, 0.0f) };
    //public Vector2[] backUVs = { new Vector2(0.05f, 0.05f), new Vector2(0.0f, 0.05f), new Vector2(0.0f, 0.0f), new Vector2(0.05f, 0.0f) };

}
