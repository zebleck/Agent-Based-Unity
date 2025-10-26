# Procedural Tile Mesh Creation Guide

## Key Insights & Lessons Learned

This document contains important insights discovered while creating procedural tile meshes for the Tilemap Creator 3D system.

---

## Critical: Mesh Coordinate System

### ✅ CORRECT: Centered at Origin
Meshes MUST be **centered at (0, 0, 0)**, spanning from **-0.5 to 0.5** on X and Z axes.

```csharp
// CORRECT - Centered at origin
Vector3[] vertices = new Vector3[]
{
    new Vector3(-0.5f, 0, -0.5f),  // Bottom-left
    new Vector3(0.5f, 0, -0.5f),   // Bottom-right
    new Vector3(0.5f, 0, 0.5f),    // Top-right
    new Vector3(-0.5f, 0, 0.5f)    // Top-left
};
```

### ❌ INCORRECT: Offset from Origin
Do NOT create meshes spanning from (0 to 1).

```csharp
// WRONG - Will cause tiles to appear offset by 1 unit diagonally
Vector3[] vertices = new Vector3[]
{
    new Vector3(0, 0, 0),
    new Vector3(1, 0, 0),
    new Vector3(1, 0, 1),
    new Vector3(0, 0, 1)
};
```

### Why This Matters
- The Tilemap3D system expects meshes centered at the origin
- Unity's rotation system rotates around the mesh's local origin (0,0,0)
- Incorrect positioning causes tiles to appear shifted from where you click
- This applies to ALL tile types: SingleTile, MultiTile, and AutoTile

### How to Verify
Check mesh bounds in Unity Inspector:
- **Center should be**: (0, y, 0) or close to it
- **Size should be**: (1.0, height, 1.0) or smaller

---

## AutoTile: Canonical Orientation

### The Rotation System
AutoTiles with `CompoundType.Rotated` need a **canonical base orientation**. The system then rotates this base mesh to handle all 4 directions.

### Best Practice: Use BOTTOM Edge as Base
Design your rotatable AutoTile meshes to flow FROM the **BOTTOM edge (z = -0.5)**.

```csharp
// Example: End tile - river flows from BOTTOM edge to center
new Vector3(-0.2f, top, -0.5f),  // Starts at bottom edge (z = -0.5)
new Vector3(0.2f, top, -0.5f),
new Vector3(0.2f, top, 0f),      // Ends at center (z = 0)
new Vector3(-0.2f, top, 0f),
```

### Why Bottom Edge?
- Consistent with typical Unity coordinate systems
- Easier to visualize in top-down view
- Matches the AutoTile mask system's expectations
- Less confusion during debugging

### AutoTile Mask Configuration
When using Bottom as canonical direction:

```csharp
// End tile (one connection)
Mask: TileMask.Bottom (value: 64)
Type: Rotated

// Straight tile (opposite connections)
Mask: TileMask.Bottom | TileMask.Top (value: 66)
Type: Rotated

// Corner tile (adjacent connections)
Mask: TileMask.Bottom | TileMask.Right (value: 80)
Type: Rotated

// T-Junction tile (three connections)
Mask: TileMask.Bottom | TileMask.Left | TileMask.Right (value: 88)
Type: Rotated

// Cross tile (all four connections)
Mask: TileMask.Left | TileMask.Right | TileMask.Top | TileMask.Bottom (value: 90)
Type: Single (no rotation needed)
```

---

## MultiTile vs AutoTile

### MultiTile
- **Purpose**: Random variation (grass, rocks, decorative elements)
- **Structure**: Simple array of TileInfo
- **Use Case**: When you want visual variety but no connection logic
- **Example**: Plains with 3 color variations

```csharp
MultiTile plainsTile = ScriptableObject.CreateInstance<MultiTile>();
plainsTile.Variants = new TileInfo[3];
for (int i = 0; i < 3; i++)
{
    plainsTile.Variants[i] = new TileInfo
    {
        Mesh = plainMeshes[i],
        Collision = TileCollision.Box
    };
}
```

### AutoTile
- **Purpose**: Smart connection based on neighbors (rivers, roads, walls)
- **Structure**: Array of TileInfoMask with connection patterns
- **Use Case**: When tiles should connect seamlessly based on adjacent tiles
- **Example**: Rivers that auto-connect

```csharp
AutoTile riverTile = ScriptableObject.CreateInstance<AutoTile>();
riverTile.EightBitMask = false;  // 4-bit = simpler, 6 unique meshes
riverTile.NoBorder = true;       // Borders count as connections
riverTile.Isolate = false;       // Can merge with other tiles via flags
```

---

## 4-Bit vs 8-Bit AutoTile Masking

### 4-Bit Masking (Simpler)
- Checks only cardinal directions: Top, Bottom, Left, Right
- **Requires 6 unique meshes** (with rotation):
  1. Isolated (0 connections)
  2. End (1 connection) - rotates
  3. Straight (2 opposite) - rotates
  4. Corner (2 adjacent) - rotates
  5. T-Junction (3 connections) - rotates
  6. Cross (4 connections)
- **Total variants needed**: 6
- Good for: Roads, simple rivers, pipes

### 8-Bit Masking (Complex)
- Checks cardinal AND diagonal neighbors
- Requires up to **47 unique meshes** for complete coverage
- Allows for smooth corner blending
- Good for: Detailed rivers, terrain blending
- More work but much better visual quality

---

## Common Issues & Solutions

### Issue: Tiles Appear Offset
**Symptom**: Tiles appear 1 unit diagonally from where you click
**Cause**: Mesh vertices not centered at origin
**Solution**: Change vertices to span from -0.5 to 0.5, not 0 to 1

### Issue: AutoTile Doesn't Rotate Correctly
**Symptom**: Some rotations work, others don't
**Cause**: Mesh not designed in canonical orientation
**Solution**: Redesign mesh to flow FROM the bottom edge, update mask to use TileMask.Bottom

### Issue: AutoTile Shows Wrong Pattern
**Symptom**: Rivers don't connect properly
**Cause**: Mask configuration doesn't match mesh geometry
**Solution**: Verify mask values match the actual neighbor connections your mesh handles

### Issue: Tiles Don't Appear at All
**Symptom**: Nothing renders when painting
**Cause**: Missing TilemapMesh module
**Solution**: Add TilemapMesh module to Tilemap3D GameObject

---

## Debugging Tips

### 1. Check Mesh Bounds
Select mesh asset in Project window, check Inspector:
- Center should be near (0, y, 0)
- Size should fit within 1x1x1 unit cube

### 2. Test Single Tiles First
Before testing connections:
1. Paint ONE isolated tile
2. Verify it appears at correct position
3. Then test connections

### 3. Compare with WorldMap Samples
The sample tiles in `Assets/3d tiles/Samples/Tilemap Creator 3D/1.0.1/World Map/` are working references:
- Check their mesh bounds
- Compare AutoTile mask configurations
- Use as templates for your own tiles

### 4. Use Debug Visualizations
Enable Gizmos in Scene view to see:
- Grid overlay
- Tile preview cursor
- Collision bounds

---

## Mesh Generation Best Practices

### 1. Keep Geometry Simple
- Use as few vertices as possible
- Tilemap combines many meshes - low poly counts matter
- Rivers: 20-30 vertices per pattern is sufficient

### 2. Consistent Width
For connecting tiles (rivers, roads):
- Use consistent channel width (e.g., 0.4 units wide)
- Center the channel at x=0 or z=0 for straight sections
- Helps rotations align perfectly

### 3. Proper Normals & UVs
```csharp
mesh.RecalculateNormals();  // Auto-generate normals
mesh.RecalculateBounds();   // Update bounds after vertex changes
```

### 4. Vertex Colors
Useful for variation without textures:
```csharp
Color[] colors = new Color[vertices.Length];
for (int i = 0; i < vertices.Length; i++)
{
    colors[i] = new Color(0.3f, 0.8f, 0.3f); // Green
}
mesh.colors = colors;
```

---

## Material Setup

### Plains (Opaque)
```csharp
Material mat = new Material(Shader.Find("Standard"));
mat.color = new Color(0.3f, 0.8f, 0.3f);
mat.SetFloat("_Metallic", 0f);
mat.SetFloat("_Glossiness", 0.2f);
```

### Rivers (Transparent)
```csharp
Material mat = new Material(Shader.Find("Standard"));
mat.color = new Color(0.2f, 0.5f, 0.9f, 0.7f);

// Enable transparency
mat.SetFloat("_Mode", 3);
mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
mat.SetInt("_ZWrite", 0);
mat.EnableKeyword("_ALPHABLEND_ON");
mat.renderQueue = 3000;

mat.SetFloat("_Metallic", 0.5f);
mat.SetFloat("_Glossiness", 0.9f); // Shiny water
```

---

## Performance Considerations

### Collision Types
Choose wisely for performance:
- **TileCollision.Box**: Best performance, use for most tiles
- **TileCollision.BoxExtend**: Good for layered tiles (extends upward)
- **TileCollision.MeshConvex**: Moderate cost, use sparingly
- **TileCollision.MeshComplex**: Expensive, avoid if possible

### Mesh Combining
The TilemapMesh module combines tiles with the same material:
- Use texture atlas for multiple tile types
- Share materials when possible
- Each unique material = separate draw call

---

## Future Improvements

Ideas for extending the system:

1. **Add 8-bit river AutoTile** for smoother corners
2. **Create road AutoTile** that connects with rivers via flags
3. **Add height variation** to plains (small hills)
4. **Texture atlas** instead of vertex colors
5. **Animated water shader** for rivers
6. **Custom editor window** for easier tile generation

---

## Summary Checklist

When creating new procedural tiles:

- [ ] Meshes centered at (0, 0, 0)
- [ ] Vertices span from -0.5 to 0.5 on X and Z
- [ ] Bounds verified in Unity Inspector
- [ ] AutoTile canonical direction uses BOTTOM edge
- [ ] Mask values match mesh geometry
- [ ] Normals and bounds recalculated
- [ ] Materials properly configured
- [ ] Tested with single isolated tile first
- [ ] Tested connections in all 4 directions
- [ ] Collision type chosen appropriately

---

*Document created: 2025*
*Based on practical experience building the Plains & River tileset*
