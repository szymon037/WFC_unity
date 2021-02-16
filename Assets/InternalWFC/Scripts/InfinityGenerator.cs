using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InfinityGenerator : MonoBehaviour
{
    private Transform player;
    public string setName = "city";
    public int chunkSide = 5;
    public int chunkHeight = 5;
    private int tileSize = 4;
    public static Dictionary<(int, int), Chunk> chunksDictionary = new Dictionary<(int, int), Chunk>();

    public static int[,] chunkDir = new int[4, 2] { { -1, 0 }, { 1, 0 }, { 0, 1 }, { 0, -1 } }; // L R F B
    public static int[] opposite = new int[4] { 1, 0, 3, 2 };

    int currentX = 0, currentZ = 0;
    public static int solveLimit = 30;

    public static float chunkWorldSize;
    private Vector3 currentRoundedCamPos = Vector3.zero;
    private Vector3 lastRoundedCamPos = Vector3.zero;
    [SerializeField]
    private int generationRadius = 2;
    (int, int)[] chunkStack;
    private int chunkStackSize = 0;

    public class Chunk
    {
        public Chunk(int x, int z) 
        {
            this.x = x;
            this.z = z;
            solution = null;
            slices = new int[4][];
            solved = false;
            chunkGO = new GameObject("Chunk x" + x.ToString() + " z" + z.ToString());
            chunkGO.transform.position = new Vector3(x * chunkWorldSize, 0f, z * chunkWorldSize);
            visited = true;
        }
        public int x, z;
        public static string setName;
        public static int chunkSize;
        public static int chunkDepth;
        public static int tileSize;

        int[] solution;
        public int[][] slices;
        public bool solved;
        public bool visited;
        GameObject chunkGO;

        private void CalculateSlices()
        {
            for (int i = 0; i < 4; i++)
                slices[i] = new int[chunkDepth * chunkSize];

            int x = 0, y = 0, z = 0;
            
            // L
            for (y = 0; y < chunkDepth; y++)
                for (z = 0; z < chunkSize; z++)
                    slices[0][y * chunkSize + z] = solution[ID(x, y, z)];

            // R
            x = chunkSize - 1; y = 0; z = 0;
            for (y = 0; y < chunkDepth; y++)
                for (z = 0; z < chunkSize; z++)
                    slices[1][y * chunkSize + z] = solution[ID(x, y, z)];

            // F
            x = 0; y = 0; z = chunkSize - 1;
            for (y = 0; y < chunkDepth; y++)
                for (x = 0; x < chunkSize; x++)
                    slices[2][y * chunkSize + x] = solution[ID(x, y, z)];

            // B
            x = 0; y = 0; z = 0;
            for (y = 0; y < chunkDepth; y++)
                for (x = 0; x < chunkSize; x++)
                    slices[3][y * chunkSize + x] = solution[ID(x, y, z)];
        }

        public void GenerateChunk()
        {
            int[][] neighbours = new int[4][];

            for (int d = 0; d < 4; d++)
            {
                int x2 = x + chunkDir[d, 0];
                int z2 = z + chunkDir[d, 1];

                if (chunksDictionary.ContainsKey((x2, z2)))
                    neighbours[d] = chunksDictionary[(x2, z2)].slices[opposite[d]];
                else
                    neighbours[d] = null;
            }

            int counter = 0;
            Model model = null;
            do
            {
                model = new TiledModel(chunkSize, chunkSize, chunkDepth, neighbours, chunkGO.transform);

                solved = model.Solve();
                counter++;
            } while (!solved && counter < solveLimit);

            if (!solved)
                return;

            solution = new int[model.grid.Length];
            for (int i = 0; i < model.grid.Length; i++)
                solution[i] = model.grid[i]._tileIndex;

            CalculateSlices();
        }

        public void SetVisited(bool value)
        {
            visited = value;
        }

        public void RemoveChunk()
        {
            Destroy(chunkGO);
        }
        public int ID(int x, int y, int z)
        {
            return x + y * chunkSize * chunkSize + z * chunkSize;
        }
    }

    
    void Start()
    {
        if (setName != null)
            TilesManager.LoadTilesTiled(setName, true, false);
        if (TilesManager.tilesTiled == null)
        {
            Debug.LogError("Could not load tiles!");
            return;
        }

        tileSize = TilesManager.tileSize;

        player = Camera.main.transform;

        chunkWorldSize = chunkSide * tileSize;

        chunksDictionary.Clear();
        Chunk.setName = setName;
        Chunk.chunkSize = chunkSide;
        Chunk.chunkDepth = chunkHeight;
        Chunk.tileSize = tileSize;

        currentRoundedCamPos = RoundCamPos();
        chunkStack = new (int, int)[generationRadius * generationRadius * 4];

        StartCoroutine(RuntimeChunkGeneration());
    }

    void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            currentX++;
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            currentX--;
        if (Input.GetKeyDown(KeyCode.UpArrow))
            currentZ++;
        if (Input.GetKeyDown(KeyCode.DownArrow))
            currentZ--;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!chunksDictionary.ContainsKey((currentX, currentZ)))
            {
                Chunk chunk = new Chunk(currentX, currentZ);
                chunksDictionary.Add((currentX, currentZ), chunk);
            }
            else if (!chunksDictionary[(currentX, currentZ)].solved)
                chunksDictionary[(currentX, currentZ)].GenerateChunk();
        }

        if (Input.GetKeyDown(KeyCode.R))
            ReloadScene();

        // CHUNK GENERATION
        currentRoundedCamPos = RoundCamPos();
        if (lastRoundedCamPos != currentRoundedCamPos)
        {
            GenerateChunks();
            lastRoundedCamPos = currentRoundedCamPos;
        }
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private Vector3 RoundCamPos()
    {
        return new Vector3(Mathf.Floor(player.transform.position.x / chunkWorldSize), 0f, Mathf.Floor(player.transform.position.z / chunkWorldSize));
    }

    private void GenerateChunks()
    {
        foreach (KeyValuePair<(int, int), Chunk> entry in chunksDictionary)
            entry.Value.visited = false;

        for (int x = -generationRadius + (int)currentRoundedCamPos.x; x <= generationRadius + (int)currentRoundedCamPos.x; x++)
            for (int z = -generationRadius + (int)currentRoundedCamPos.z; z <= generationRadius + (int)currentRoundedCamPos.z; z++)
                if (!chunksDictionary.ContainsKey((x, z)))
                {
                    if (chunkStackSize < chunkStack.Length)
                    {
                        Chunk chunk = new Chunk(x, z);
                        chunksDictionary.Add((x, z), chunk);

                        chunkStack[chunkStackSize] = (x, z);
                        chunkStackSize++;
                    }
                }
                else
                {
                    chunksDictionary[(x, z)].visited = true;
                }

        foreach (Chunk chunk in chunksDictionary.Values.ToList())
            if (!chunk.visited)
            {
                chunk.RemoveChunk();
                chunksDictionary.Remove((chunk.x, chunk.z));
            }
    }

    private IEnumerator RuntimeChunkGeneration()
    {
        while (true)
        {
            if (chunkStackSize > 0)
            {
                int x = chunkStack[chunkStackSize - 1].Item1;
                int z = chunkStack[chunkStackSize - 1].Item2;

                if (chunksDictionary.ContainsKey((x, z)))
                    chunksDictionary[(x, z)].GenerateChunk();

                chunkStackSize--;
            }

            yield return new WaitForFixedUpdate();
        }
    }
}
