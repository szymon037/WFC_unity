using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InfinityGenerator : MonoBehaviour
{
    public GameObject chunkIndicator;
    public static int chunkSize = 15;
    public static int chunkDepth = 1;
    public static int tileSize = 2;
    public static string setName = "Knots";
    public static Dictionary<(int, int), Chunk> chunksDictionary = new Dictionary<(int, int), Chunk>();

    public static int[,] chunkDir = new int[4, 2] { { -1, 0 }, { 1, 0 }, { 0, 1 }, { 0, -1 } }; // L R F B
    public static int[] opposite = new int[4] { 1, 0, 3, 2 };

    int currentX = 0, currentZ = 0;
    public static int solveLimit = 500;

    public struct Chunk
    {
        public Chunk(int x, int z) 
        {
            this.x = x;
            this.z = z;
            solution = null;
            slices = new int[4][];
            solved = false;

            GenerateChunk();
        }
        int x, z;

        int[] solution;
        public int[][] slices;
        public bool solved;

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
            TiledModel tm;
            do
            {
                tm = new TiledModel(chunkSize, chunkSize, chunkDepth, 2, true, setName, neighbours, x, z);
                solved = tm.Solve();
                counter++;
            } while (!solved && counter < solveLimit);

            if (!solved)
                return;

            solution = new int[tm.grid.Length];
            for (int i = 0; i < tm.grid.Length; i++)
                solution[i] = tm.grid[i]._tileIndex;

            CalculateSlices();
        }
    }

    
    void Start()
    {
        chunksDictionary.Clear();
        Chunk a = new Chunk(currentX, currentZ);
        chunksDictionary.Add((currentX, currentZ), a);
        chunkIndicator = Instantiate(chunkIndicator, Vector3.zero, Quaternion.identity);
        chunkIndicator.transform.localScale = new Vector3(chunkSize * tileSize, 1f, chunkSize * tileSize) * 0.1f;
        TeleportChunkIndicator();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            currentX++;
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            currentX--;
        if (Input.GetKeyDown(KeyCode.UpArrow))
            currentZ++;
        if (Input.GetKeyDown(KeyCode.DownArrow))
            currentZ--;

        TeleportChunkIndicator();

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
    }

    public static int ID(int x, int y, int z)
    {
        return x + y * chunkSize * chunkSize + z * chunkSize;
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void TeleportChunkIndicator()
    {
        chunkIndicator.transform.position = new Vector3(currentX * chunkSize * tileSize, 0f, currentZ * chunkSize * tileSize) + new Vector3(chunkSize * tileSize - tileSize, 0f, chunkSize * tileSize - tileSize) * 0.5f;
    }
}
