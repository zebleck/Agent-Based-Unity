#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TilemapCreator3D;
using System.IO;

public class ProceduralTileGenerator : MonoBehaviour
{
    private enum RiverPattern
    {
        Isolated,    // Mask 0  - No connections
        End,         // Mask 16 - One side (rotates)
        Straight,    // Mask 24 - Opposite sides (rotates)
        Corner,      // Mask 18 - Adjacent sides (rotates)
        TJunction,   // Mask 26 - Three sides (rotates)
        Cross        // Mask 90 - All four sides
    }

    private const string GENERATED_PATH = "Assets/ProceduralTiles/Generated/";
    private const string MESH_PATH = GENERATED_PATH + "Meshes/";
    private const string MATERIAL_PATH = GENERATED_PATH + "Materials/";
    private const string TILE_PATH = GENERATED_PATH + "Tiles/";

    #region Menu Items

    [MenuItem("Tools/Procedural Tiles/Generate All")]
    public static void GenerateAll()
    {
        Debug.Log("Starting procedural tile generation...");

        GenerateMeshesOnly();
        GenerateTilesOnly();

        Debug.Log("Tile generation complete!");
    }

    [MenuItem("Tools/Procedural Tiles/Generate Meshes Only")]
    public static void GenerateMeshesOnly()
    {
        Debug.Log("Generating meshes...");

        // Generate plain meshes
        for (int i = 1; i <= 3; i++)
        {
            Mesh plainMesh = GeneratePlainMesh(i);
            string path = MESH_PATH + $"Plain_Var0{i}.asset";
            SaveAsset(plainMesh, path);
        }

        // Generate river meshes
        foreach (RiverPattern pattern in System.Enum.GetValues(typeof(RiverPattern)))
        {
            Mesh riverMesh = GenerateRiverMesh(pattern);
            string path = MESH_PATH + $"River_{pattern}.asset";
            SaveAsset(riverMesh, path);
        }

        Debug.Log("Meshes generated successfully!");
    }

    [MenuItem("Tools/Procedural Tiles/Generate Tiles Only")]
    public static void GenerateTilesOnly()
    {
        Debug.Log("Generating tile assets...");

        // Create materials
        Material plainMat = CreatePlainsMaterial();
        Material riverMat = CreateRiverMaterial();

        // Create tiles
        CreatePlainsMultiTile(plainMat);
        CreateRiverAutoTile(riverMat);

        Debug.Log("Tile assets generated successfully!");
    }

    [MenuItem("Tools/Procedural Tiles/Clear Generated Assets")]
    public static void ClearGeneratedAssets()
    {
        if (Directory.Exists(GENERATED_PATH))
        {
            Directory.Delete(GENERATED_PATH, true);
            File.Delete(GENERATED_PATH.TrimEnd('/') + ".meta");
            AssetDatabase.Refresh();
            Debug.Log("Cleared all generated assets");
        }
    }

    #endregion

    #region Plain Mesh Generation

    private static Mesh GeneratePlainMesh(int variation)
    {
        Mesh mesh = new Mesh();
        mesh.name = $"Plain_Var0{variation}";

        // Flat plane at y = 0, centered at origin (0,0,0)
        // Vertices span from -0.5 to 0.5 on X and Z axes
        Vector3[] vertices = new Vector3[]
        {
            new Vector3(-0.5f, 0, -0.5f),  // Bottom-left
            new Vector3(0.5f, 0, -0.5f),   // Bottom-right
            new Vector3(0.5f, 0, 0.5f),    // Top-right
            new Vector3(-0.5f, 0, 0.5f)    // Top-left
        };

        // Two triangles to make a quad (top face only)
        int[] triangles = new int[]
        {
            0, 2, 1,  // First triangle
            0, 3, 2   // Second triangle
        };

        // UVs for texture mapping
        Vector2[] uvs = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1)
        };

        // Vertex colors (green with slight color variation per variant)
        float colorVar = variation * 0.05f;
        Color[] colors = new Color[]
        {
            new Color(0.3f + colorVar, 0.8f - colorVar, 0.3f + colorVar),
            new Color(0.3f + colorVar, 0.8f - colorVar, 0.3f + colorVar),
            new Color(0.3f + colorVar, 0.8f - colorVar, 0.3f + colorVar),
            new Color(0.3f + colorVar, 0.8f - colorVar, 0.3f + colorVar)
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.colors = colors;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    #endregion

    #region River Mesh Generation

    private static Mesh GenerateRiverMesh(RiverPattern pattern)
    {
        Mesh mesh = new Mesh();
        mesh.name = $"River_{pattern}";

        // All river meshes are sunken slightly (y = -0.1 to 0)
        float waterTop = 0f;
        float waterBottom = -0.1f;

        Vector3[] vertices;
        int[] triangles;

        switch (pattern)
        {
            case RiverPattern.Isolated:
                CreateIsolatedRiver(out vertices, out triangles, waterTop, waterBottom);
                break;
            case RiverPattern.End:
                CreateEndRiver(out vertices, out triangles, waterTop, waterBottom);
                break;
            case RiverPattern.Straight:
                CreateStraightRiver(out vertices, out triangles, waterTop, waterBottom);
                break;
            case RiverPattern.Corner:
                CreateCornerRiver(out vertices, out triangles, waterTop, waterBottom);
                break;
            case RiverPattern.TJunction:
                CreateTJunctionRiver(out vertices, out triangles, waterTop, waterBottom);
                break;
            case RiverPattern.Cross:
                CreateCrossRiver(out vertices, out triangles, waterTop, waterBottom);
                break;
            default:
                CreateIsolatedRiver(out vertices, out triangles, waterTop, waterBottom);
                break;
        }

        // UVs and colors
        Vector2[] uvs = new Vector2[vertices.Length];
        Color[] colors = new Color[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
            colors[i] = new Color(0.2f, 0.5f, 0.9f, 0.7f);
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.colors = colors;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    private static void CreateIsolatedRiver(out Vector3[] vertices, out int[] triangles, float top, float bottom)
    {
        // Small pond in center (centered at origin)
        float innerRad = 0.3f;
        int segments = 8;

        vertices = new Vector3[segments * 2 + 2]; // Ring + center top and bottom
        vertices[0] = new Vector3(0f, top, 0f);
        vertices[1] = new Vector3(0f, bottom, 0f);

        for (int i = 0; i < segments; i++)
        {
            float angle = (i / (float)segments) * Mathf.PI * 2f;
            float x = Mathf.Cos(angle) * innerRad;
            float z = Mathf.Sin(angle) * innerRad;

            vertices[2 + i * 2] = new Vector3(x, top, z);
            vertices[2 + i * 2 + 1] = new Vector3(x, bottom, z);
        }

        triangles = new int[segments * 12]; // Top, bottom, and sides
        int idx = 0;

        for (int i = 0; i < segments; i++)
        {
            int next = (i + 1) % segments;

            // Top triangle
            triangles[idx++] = 0;
            triangles[idx++] = 2 + i * 2;
            triangles[idx++] = 2 + next * 2;

            // Bottom triangle
            triangles[idx++] = 1;
            triangles[idx++] = 2 + next * 2 + 1;
            triangles[idx++] = 2 + i * 2 + 1;

            // Side quad (2 triangles)
            triangles[idx++] = 2 + i * 2;
            triangles[idx++] = 2 + i * 2 + 1;
            triangles[idx++] = 2 + next * 2;

            triangles[idx++] = 2 + next * 2;
            triangles[idx++] = 2 + i * 2 + 1;
            triangles[idx++] = 2 + next * 2 + 1;
        }
    }

    private static void CreateEndRiver(out Vector3[] vertices, out int[] triangles, float top, float bottom)
    {
        // River flows from BOTTOM edge, ends in center (centered at origin)
        // This is the canonical orientation - AutoTile will rotate it for other directions
        vertices = new Vector3[]
        {
            // Top surface
            new Vector3(-0.2f, top, -0.5f), new Vector3(0.2f, top, -0.5f),
            new Vector3(0.2f, top, 0f), new Vector3(-0.2f, top, 0f),
            // Bottom surface
            new Vector3(-0.2f, bottom, -0.5f), new Vector3(0.2f, bottom, -0.5f),
            new Vector3(0.2f, bottom, 0f), new Vector3(-0.2f, bottom, 0f)
        };

        triangles = new int[]
        {
            // Top
            0, 1, 2, 0, 2, 3,
            // Bottom
            4, 6, 5, 4, 7, 6,
            // Sides
            0, 5, 1, 0, 4, 5,
            2, 6, 7, 2, 7, 3,
            3, 7, 4, 3, 4, 0,
            1, 5, 6, 1, 6, 2
        };
    }

    private static void CreateStraightRiver(out Vector3[] vertices, out int[] triangles, float top, float bottom)
    {
        // River flows from BOTTOM to TOP (centered at origin)
        // Canonical orientation for AutoTile rotation
        vertices = new Vector3[]
        {
            // Top surface
            new Vector3(-0.2f, top, -0.5f), new Vector3(0.2f, top, -0.5f),
            new Vector3(0.2f, top, 0.5f), new Vector3(-0.2f, top, 0.5f),
            // Bottom surface
            new Vector3(-0.2f, bottom, -0.5f), new Vector3(0.2f, bottom, -0.5f),
            new Vector3(0.2f, bottom, 0.5f), new Vector3(-0.2f, bottom, 0.5f)
        };

        triangles = new int[]
        {
            // Top
            0, 1, 2, 0, 2, 3,
            // Bottom
            4, 6, 5, 4, 7, 6,
            // Sides
            3, 7, 4, 3, 4, 0,
            1, 5, 6, 1, 6, 2
        };
    }

    private static void CreateCornerRiver(out Vector3[] vertices, out int[] triangles, float top, float bottom)
    {
        // River flows from BOTTOM edge, turns RIGHT (centered at origin)
        // Canonical orientation for AutoTile rotation
        vertices = new Vector3[]
        {
            // Vertical part (from bottom edge)
            new Vector3(-0.2f, top, -0.5f), new Vector3(0.2f, top, -0.5f),
            new Vector3(0.2f, top, 0f), new Vector3(-0.2f, top, 0f),
            // Horizontal part (to right edge)
            new Vector3(0.2f, top, -0.2f), new Vector3(0.5f, top, -0.2f),
            new Vector3(0.5f, top, 0.2f), new Vector3(0.2f, top, 0.2f),
            // Bottom vertices (same x,z as top)
            new Vector3(-0.2f, bottom, -0.5f), new Vector3(0.2f, bottom, -0.5f),
            new Vector3(0.2f, bottom, 0f), new Vector3(-0.2f, bottom, 0f),
            new Vector3(0.2f, bottom, -0.2f), new Vector3(0.5f, bottom, -0.2f),
            new Vector3(0.5f, bottom, 0.2f), new Vector3(0.2f, bottom, 0.2f)
        };

        triangles = new int[]
        {
            // Top surface - vertical part
            0, 1, 2, 0, 2, 3,
            // Top surface - horizontal part
            4, 5, 6, 4, 6, 7,
            // Bottom surface - vertical part
            8, 10, 9, 8, 11, 10,
            // Bottom surface - horizontal part
            12, 14, 13, 12, 15, 14,
            // Sides of vertical part
            0, 8, 9, 0, 9, 1,
            3, 11, 8, 3, 8, 0,
            // Sides of horizontal part
            5, 14, 6, 5, 13, 14,
            4, 12, 13, 4, 13, 5
        };
    }

    private static void CreateTJunctionRiver(out Vector3[] vertices, out int[] triangles, float top, float bottom)
    {
        // River flows from BOTTOM, LEFT, and RIGHT (T-shape, centered at origin)
        // Canonical orientation for AutoTile rotation
        vertices = new Vector3[]
        {
            // Horizontal bar (left-right)
            new Vector3(-0.5f, top, -0.2f), new Vector3(0.5f, top, -0.2f),
            new Vector3(0.5f, top, 0.2f), new Vector3(-0.5f, top, 0.2f),
            // Vertical stem (from bottom)
            new Vector3(-0.2f, top, -0.5f), new Vector3(0.2f, top, -0.5f),
            new Vector3(0.2f, top, 0.2f), new Vector3(-0.2f, top, 0.2f),
            // Bottom vertices
            new Vector3(-0.5f, bottom, -0.2f), new Vector3(0.5f, bottom, -0.2f),
            new Vector3(0.5f, bottom, 0.2f), new Vector3(-0.5f, bottom, 0.2f),
            new Vector3(-0.2f, bottom, -0.5f), new Vector3(0.2f, bottom, -0.5f),
            new Vector3(0.2f, bottom, 0.2f), new Vector3(-0.2f, bottom, 0.2f)
        };

        triangles = new int[]
        {
            // Top - horizontal
            0, 1, 2, 0, 2, 3,
            // Top - vertical
            4, 5, 6, 4, 6, 7,
            // Bottom - horizontal
            8, 10, 9, 8, 11, 10,
            // Bottom - vertical
            12, 14, 13, 12, 15, 14,
            // Sides
            0, 8, 9, 0, 9, 1,
            3, 11, 8, 3, 8, 0,
            4, 12, 13, 4, 13, 5
        };
    }

    private static void CreateCrossRiver(out Vector3[] vertices, out int[] triangles, float top, float bottom)
    {
        // River flows in all four directions (+ shape, centered at origin)
        vertices = new Vector3[]
        {
            // Horizontal bar
            new Vector3(-0.5f, top, -0.2f), new Vector3(0.5f, top, -0.2f),
            new Vector3(0.5f, top, 0.2f), new Vector3(-0.5f, top, 0.2f),
            // Vertical bar
            new Vector3(-0.2f, top, -0.5f), new Vector3(0.2f, top, -0.5f),
            new Vector3(0.2f, top, 0.5f), new Vector3(-0.2f, top, 0.5f),
            // Bottom vertices
            new Vector3(-0.5f, bottom, -0.2f), new Vector3(0.5f, bottom, -0.2f),
            new Vector3(0.5f, bottom, 0.2f), new Vector3(-0.5f, bottom, 0.2f),
            new Vector3(-0.2f, bottom, -0.5f), new Vector3(0.2f, bottom, -0.5f),
            new Vector3(0.2f, bottom, 0.5f), new Vector3(-0.2f, bottom, 0.5f)
        };

        triangles = new int[]
        {
            // Top - horizontal
            0, 1, 2, 0, 2, 3,
            // Top - vertical
            4, 5, 6, 4, 6, 7,
            // Bottom - horizontal
            8, 10, 9, 8, 11, 10,
            // Bottom - vertical
            12, 14, 13, 12, 15, 14,
            // Sides - horizontal
            0, 8, 9, 0, 9, 1,
            3, 11, 8, 3, 8, 0,
            // Sides - vertical
            4, 12, 13, 4, 13, 5,
            7, 15, 14, 7, 14, 6
        };
    }

    #endregion

    #region Material Creation

    private static Material CreatePlainsMaterial()
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.name = "PlainMaterial";
        mat.color = new Color(0.3f, 0.8f, 0.3f); // Green
        mat.SetFloat("_Metallic", 0f);
        mat.SetFloat("_Glossiness", 0.2f);

        string path = MATERIAL_PATH + "PlainMaterial.mat";
        SaveAsset(mat, path);

        return mat;
    }

    private static Material CreateRiverMaterial()
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.name = "RiverMaterial";
        mat.color = new Color(0.2f, 0.5f, 0.9f, 0.7f); // Blue with transparency

        // Enable transparency
        mat.SetFloat("_Mode", 3); // Transparent mode
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;

        mat.SetFloat("_Metallic", 0.5f);
        mat.SetFloat("_Glossiness", 0.9f);

        string path = MATERIAL_PATH + "RiverMaterial.mat";
        SaveAsset(mat, path);

        return mat;
    }

    #endregion

    #region Tile Asset Creation

    private static void CreatePlainsMultiTile(Material material)
    {
        MultiTile tile = ScriptableObject.CreateInstance<MultiTile>();
        tile.Material = material;
        tile.CollisionLayer = 0;
        tile.NavigationArea = 0;
        tile.PreviewOrientation = new Vector2(20, 20);

        // Load the 3 plain meshes
        Mesh[] plainMeshes = new Mesh[3];
        for (int i = 0; i < 3; i++)
        {
            string meshPath = MESH_PATH + $"Plain_Var0{i + 1}.asset";
            plainMeshes[i] = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
        }

        tile.Variants = new TileInfo[3];
        for (int i = 0; i < 3; i++)
        {
            tile.Variants[i] = new TileInfo
            {
                Mesh = plainMeshes[i],
                CollisionMesh = null,
                Collision = TileCollision.Box
            };
        }

        string path = TILE_PATH + "Plains.asset";
        SaveAsset(tile, path);

        Debug.Log($"Created Plains MultiTile at {path}");
    }

    private static void CreateRiverAutoTile(Material material)
    {
        AutoTile tile = ScriptableObject.CreateInstance<AutoTile>();
        tile.Material = material;
        tile.CollisionLayer = 4; // Water layer
        tile.NavigationArea = 1; // Unwalkable
        tile.PreviewOrientation = new Vector2(0, 90);
        tile.EightBitMask = false; // 4-bit masking
        tile.NoBorder = true; // Borders connect
        tile.Orientation2D = false;
        tile.Isolate = false; // Can merge with flags

        // Load river meshes
        Mesh isolated = AssetDatabase.LoadAssetAtPath<Mesh>(MESH_PATH + "River_Isolated.asset");
        Mesh end = AssetDatabase.LoadAssetAtPath<Mesh>(MESH_PATH + "River_End.asset");
        Mesh straight = AssetDatabase.LoadAssetAtPath<Mesh>(MESH_PATH + "River_Straight.asset");
        Mesh corner = AssetDatabase.LoadAssetAtPath<Mesh>(MESH_PATH + "River_Corner.asset");
        Mesh tjunction = AssetDatabase.LoadAssetAtPath<Mesh>(MESH_PATH + "River_TJunction.asset");
        Mesh cross = AssetDatabase.LoadAssetAtPath<Mesh>(MESH_PATH + "River_Cross.asset");

        // Create variants with proper masks
        tile.Variants = new TileInfoMask[6];

        // Variant 0: Isolated (mask 0)
        tile.Variants[0] = new TileInfoMask
        {
            Info = new TileInfo { Mesh = isolated, CollisionMesh = null, Collision = TileCollision.BoxExtend },
            Mask = new TileMaskCompound(TileMask.None, TileMaskCompound.CompoundType.Single)
        };

        // Variant 1: End (mask 64 - Bottom, rotates)
        tile.Variants[1] = new TileInfoMask
        {
            Info = new TileInfo { Mesh = end, CollisionMesh = null, Collision = TileCollision.BoxExtend },
            Mask = new TileMaskCompound(TileMask.Bottom, TileMaskCompound.CompoundType.Rotated)
        };

        // Variant 2: Straight (mask 66 - Bottom+Top, rotates)
        tile.Variants[2] = new TileInfoMask
        {
            Info = new TileInfo { Mesh = straight, CollisionMesh = null, Collision = TileCollision.BoxExtend },
            Mask = new TileMaskCompound(TileMask.Bottom | TileMask.Top, TileMaskCompound.CompoundType.Rotated)
        };

        // Variant 3: Corner (mask 80 - Bottom+Right, rotates)
        tile.Variants[3] = new TileInfoMask
        {
            Info = new TileInfo { Mesh = corner, CollisionMesh = null, Collision = TileCollision.BoxExtend },
            Mask = new TileMaskCompound(TileMask.Bottom | TileMask.Right, TileMaskCompound.CompoundType.Rotated)
        };

        // Variant 4: T-Junction (mask 88 - Bottom+Left+Right, rotates)
        tile.Variants[4] = new TileInfoMask
        {
            Info = new TileInfo { Mesh = tjunction, CollisionMesh = null, Collision = TileCollision.BoxExtend },
            Mask = new TileMaskCompound(TileMask.Bottom | TileMask.Left | TileMask.Right, TileMaskCompound.CompoundType.Rotated)
        };

        // Variant 5: Cross (mask 90 - all four sides)
        tile.Variants[5] = new TileInfoMask
        {
            Info = new TileInfo { Mesh = cross, CollisionMesh = null, Collision = TileCollision.BoxExtend },
            Mask = new TileMaskCompound(TileMask.Left | TileMask.Right | TileMask.Top | TileMask.Bottom, TileMaskCompound.CompoundType.Single)
        };

        string path = TILE_PATH + "River.asset";
        SaveAsset(tile, path);

        Debug.Log($"Created River AutoTile at {path}");
    }

    #endregion

    #region Utility Methods

    private static void SaveAsset(Object asset, string path)
    {
        // Ensure directory exists
        string directory = Path.GetDirectoryName(path);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Check if asset already exists
        Object existingAsset = AssetDatabase.LoadAssetAtPath<Object>(path);
        if (existingAsset != null)
        {
            EditorUtility.CopySerialized(asset, existingAsset);
            AssetDatabase.SaveAssets();
        }
        else
        {
            AssetDatabase.CreateAsset(asset, path);
        }

        AssetDatabase.Refresh();
    }

    #endregion
}
#endif
