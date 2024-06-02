using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class Track : MonoBehaviour
{
    public List<Tile> tiles;
    public Tile startingTile;
    public TileSet tileSet;
    public GameObject chunkPlane;
    public Mesh borderPreset;
    public Mesh borderColliderPreset;
    public Material presetMaterial;
    public int chunkSize = 50;
    public int maxAttempts = 3;
    public int maxMissSteps = 500;
    public Vector2Int startPosition;

    public bool IsGenerated { get; private set; }
    public float Distance = 0;

    [SerializeField, HideInInspector]
    public List<Chunk> chunks = new List<Chunk>();

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ClearTrack()
    {
        if (chunks == null)
        {
            chunks = new List<Chunk>();
            return;
        }
        foreach (var chunk in chunks)
        {
            if (chunk)
                chunk.Clear();
        }

        chunks.Clear();

        foreach (Transform child in transform)
        {
            if (Application.isEditor)
                DestroyImmediate(child.gameObject);
            else
                Destroy(child.gameObject);
        }

        IsGenerated = false;
    }

    public async Task Generate()
    {
        ClearTrack();

        var gen = new TrackGenerator(chunkSize, maxAttempts, maxMissSteps, tileSet.tiles);

        TileDirection[] exitDirections = new TileDirection[]
        {
            TileDirection.East,
            TileDirection.South,
            TileDirection.West,
        };

        TilePosition startPosition = new TilePosition(new Vector2Int(5, 0), TileDirection.North);

        GenerateTrackSettings startGenSettings = new GenerateTrackSettings(startPosition, exitDirections[0], chunkSize, new List<Tile>() { tileSet.startTile });

        Chunk startingChunk = await GenerateChunk(gen, startGenSettings, Vector2Int.zero, "StartChunk");
        startingChunk.centerPathStartingDistance = 0;
        chunks.Add(startingChunk);



        //TilePosition exitPosition = new TilePosition(new Vector2Int(Mathf.RoundToInt(WorldConstants.ChunkSize / 2), WorldConstants.ChunkSize - 1), TileDirection.North);

        //Vector2Int prevOffset = Vector2Int.zero;
        //TilePosition nextTilePosition = exitPosition.NextInDirection();


        for (int i = 0; i < 3; i++)
        {
            var nextTilePosition = chunks[i].nextTilePosition;

            Vector2Int newChunkPosition = chunks[i].offset + nextTilePosition.direction.ToVector();

            Vector2Int trimmedEntryPos = new Vector2Int(
                ((nextTilePosition.position.x % chunkSize) + chunkSize) % chunkSize,
                ((nextTilePosition.position.y % chunkSize) + chunkSize) % chunkSize);

            GenerateTrackSettings genSettings;
            if (i == 2)
            {
                var endPos = startPosition.NextInDirection(-1);

                Vector2Int trimmedEndPos = new Vector2Int(
                    ((endPos.position.x % chunkSize) + chunkSize) % chunkSize,
                    ((endPos.position.y % chunkSize) + chunkSize) % chunkSize);

                genSettings = new GenerateTrackSettings(new TilePosition(trimmedEntryPos, nextTilePosition.direction), new TilePosition(trimmedEndPos, startPosition.direction), chunkSize);
            }
            else
            {
                genSettings = new GenerateTrackSettings(new TilePosition(trimmedEntryPos, nextTilePosition.direction), exitDirections[i + 1], chunkSize);
            }
            Chunk chunk = await GenerateChunk(gen, genSettings, newChunkPosition, $"Chunk_{i}");

            chunk.centerPathStartingDistance = chunks[i].centerPathEndDistance;

            chunks.Add(chunk);
        }

        //Regenerate Starting Chunk

        //Chunk startingChunk = await GenerateChunk(gen, )

        Distance = chunks.Last().centerPathEndDistance;

        GenerateGroundPlane();

        IsGenerated = true;
    }

    public Vector3 GetStartingPosition(int index)
    {
        //TODO
        return Vector3.zero;
    }

    public int GetChunkIdOfPosition(Vector3 position)
    {
        for (int i = 0; i < chunks.Count; i++)
        {
            if (chunks[i].bounds.Contains(position))
                return i;
        }
        return -1;
    }

    public Vector3 GetPositionAtDistance(float distance)
    {
        var moduloDistance = distance % Distance;

        for (int i = 0; i < chunks.Count; i++)
        {
            if (moduloDistance < chunks[i].centerPathEndDistance)
            {
                return chunks[i].GetPositionByDistance(moduloDistance);
            }
        }

        //Debug.LogError($"No Position for distance {distance} found!");

        return Vector3.zero;
    }

    async Task<Chunk> GenerateChunk(TrackGenerator gen, GenerateTrackSettings genSettings, Vector2Int offset, string name)
    {
        GameObject chunkGO = new GameObject(name);
        chunkGO.transform.parent = transform;
        chunkGO.transform.localPosition = new Vector3(offset.x, 0, offset.y) * chunkSize * WorldConstants.TileSize;
        var chunk = chunkGO.AddComponent<Chunk>();

        var res = await Task.Run(() => gen.GenerateTrack(genSettings));

        List<Tile> segments = new List<Tile>();

        if (res != null)
        {
            for (int i = 0; i < res.segments.Count; i++)
            {
                //var obj = Instantiate(res[i].tile, new Vector3(res[i].pos.position.x + offset.x * chunkSize, 0, res[i].pos.position.y + offset.y * chunkSize) * WorldConstants.TileSize - Quaternion.Euler(0, res[i].pos.direction * 90, 0) * res[i].tile.TransformMaskPointToLocal(res[i].tile.mask.entryPosition.position), Quaternion.Euler(0, res[i].pos.direction * 90, 0), parent.transform);
                var obj = Instantiate(res.segments[i].tile, chunkGO.transform);

                obj.transform.localPosition = res.segments[i].TransformTileToChunkPosition4(res.segments[i].tile.localCenter); //new Vector3(res.segments[i].pos.position.x, 0, res.segments[i].pos.position.y) * WorldConstants.TileSize - Quaternion.Euler(0, res.segments[i].pos.direction * 90, 0) * res.segments[i].tile.TransformMaskPointToLocal(res.segments[i].tile.mask.entryPosition.position) + new Vector3(WorldConstants.TileSize, 0, WorldConstants.TileSize) / 2;
                obj.transform.localRotation = Quaternion.Euler(0, TileDirection.GetAngle(res.segments[i].tile.mask.entryPosition.direction, res.segments[i].pos.direction) * 90, 0);

                segments.Add(obj.GetComponent<Tile>());
            }
        }

        //Add border
        chunk.rightBorder = res.borderRight;
        chunk.leftBorder = res.borderLeft;
        chunk.centerPath = res.centerPath;

        GenerateBorder(chunk.leftBorder, chunk.gameObject);
        GenerateBorder(chunk.rightBorder, chunk.gameObject);

        //Add chunk plane
        //if (chunkPlane)
        //{
        //    var plane = Instantiate(chunkPlane, chunkGO.transform);
        //    plane.transform.localPosition = new Vector3(chunkSize, 0, chunkSize) * WorldConstants.TileSize / 2;
        //    plane.transform.localScale = new Vector3(chunkSize * WorldConstants.TileSize / 2, 1, chunkSize * WorldConstants.TileSize / 2);
        //}
        chunk.track = segments;
        chunk.offset = offset;
        chunk.nextTilePosition = res.segments.Last().next;
        chunk.bounds = new Bounds(chunk.transform.position + new Vector3(1, 0, 1) * chunkSize / 2 * WorldConstants.TileSize - new Vector3(0.5f, 0, 0.5f) * WorldConstants.TileSize, new Vector3(1, 0, 1) * chunkSize * WorldConstants.TileSize + Vector3.up * 100);

        return chunk;
    }

    private Chunk GenerateStartingChunk(TrackGenerator gen, TilePosition entryPosition, TilePosition exitPosition, Vector2Int offset)
    {
        GameObject chunkGO = new GameObject("Chunk_Start");
        chunkGO.transform.parent = transform;
        chunkGO.transform.localPosition = new Vector3(offset.x, 0, offset.y) * chunkSize * WorldConstants.TileSize;
        var chunk = chunkGO.AddComponent<Chunk>();

        TilePositionTransformation trans = new TilePositionTransformation(exitPosition, tileSet.startTile.mask.exitPosition);

        TilePosition entryPos = trans.TransformTilePosition(tileSet.startTile.mask.entryPosition);


        var res = gen.GenerateTrackFromTileList(new List<Tile> { tileSet.startTile }, entryPos);

        gen.GenerateTrack(new GenerateTrackSettings(entryPosition, entryPos.NextInDirection(-1), chunkSize));

        List<Tile> segments = new List<Tile>();

        if (res != null)
        {
            for (int i = 0; i < res.segments.Count; i++)
            {
                //var obj = Instantiate(res[i].tile, new Vector3(res[i].pos.position.x + offset.x * chunkSize, 0, res[i].pos.position.y + offset.y * chunkSize) * WorldConstants.TileSize - Quaternion.Euler(0, res[i].pos.direction * 90, 0) * res[i].tile.TransformMaskPointToLocal(res[i].tile.mask.entryPosition.position), Quaternion.Euler(0, res[i].pos.direction * 90, 0), parent.transform);
                var obj = Instantiate(res.segments[i].tile, chunkGO.transform);

                obj.transform.localPosition = res.segments[i].TransformTileToChunkPosition4(res.segments[i].tile.localCenter); //new Vector3(res.segments[i].pos.position.x, 0, res.segments[i].pos.position.y) * WorldConstants.TileSize - Quaternion.Euler(0, res.segments[i].pos.direction * 90, 0) * res.segments[i].tile.TransformMaskPointToLocal(res.segments[i].tile.mask.entryPosition.position) + new Vector3(WorldConstants.TileSize, 0, WorldConstants.TileSize) / 2;
                obj.transform.localRotation = Quaternion.Euler(0, TileDirection.GetAngle(res.segments[i].tile.mask.entryPosition.direction, res.segments[i].pos.direction) * 90, 0);

                segments.Add(obj.GetComponent<Tile>());
            }
        }

        //Add border
        chunk.rightBorder = res.borderRight;
        chunk.leftBorder = res.borderLeft;
        chunk.centerPath = res.centerPath;

        GenerateBorder(chunk.leftBorder, chunk.gameObject);
        GenerateBorder(chunk.rightBorder, chunk.gameObject);

        //Add chunk plane
        //if (chunkPlane)
        //{
        //    var plane = Instantiate(chunkPlane, chunkGO.transform);
        //    plane.transform.localPosition = new Vector3(chunkSize, 0, chunkSize) * WorldConstants.TileSize / 2;
        //    plane.transform.localScale = new Vector3(chunkSize * WorldConstants.TileSize / 2, 1, chunkSize * WorldConstants.TileSize / 2);
        //}
        chunk.track = segments;
        chunk.offset = offset;
        chunk.nextTilePosition = res.segments.Last().next;
        chunk.bounds = new Bounds(chunk.transform.position + new Vector3(1, 0, 1) * chunkSize / 2 * WorldConstants.TileSize - new Vector3(0.5f, 0, 0.5f) * WorldConstants.TileSize, new Vector3(1, 0, 1) * chunkSize * WorldConstants.TileSize + Vector3.up * 100);

        return chunk;
    }

    private GameObject GenerateBorder(Path border, GameObject parent)
    {
        GameObject borderGO = new GameObject("Border");
        borderGO.transform.parent = parent.transform;
        borderGO.transform.localPosition = Vector3.zero;

        if (borderPreset)
        {
            var mf = borderGO.AddComponent<MeshFilter>();
            var mr = borderGO.AddComponent<MeshRenderer>();
            mr.material = presetMaterial;
            mf.sharedMesh = new Mesh();
            PathMeshGenerator.GenerateMesh(border, borderPreset, mf.sharedMesh, 20, -3);
        }
        if (borderColliderPreset)
        {
            var coll = borderGO.AddComponent<MeshCollider>();
            coll.sharedMesh = new Mesh();
            PathMeshGenerator.GenerateMesh(border, borderColliderPreset, coll.sharedMesh, 6, -3f);
        }
        return borderGO;
    }

    private void GenerateGroundPlane()
    {
        Vector2 center = Vector2.zero;
        float maxDistance = 0;
        for (int i = 0; i < chunks.Count; i++)
        {
            center += chunks[i].transform.localPosition.XZ();
        }

        center /= chunks.Count;

        for (int i = 0; i < chunks.Count; i++)
        {
            var pos = chunks[i].transform.localPosition.XZ();

            maxDistance = Mathf.Max(maxDistance, Mathf.Abs(pos.x - center.x));
            maxDistance = Mathf.Max(maxDistance, Mathf.Abs(pos.y - center.y));
        }


        var plane = Instantiate(chunkPlane, transform);
        plane.transform.localPosition = center.X0Z() + new Vector3(1, 0, 1) * chunkSize * WorldConstants.TileSize / 2;

        float size = 2 * (maxDistance + chunkSize * WorldConstants.TileSize);

        plane.transform.localScale = new Vector3(size, 1, size);

    }
}
