using UnityEngine;

/*
    This script calculates a perlin noise texture.
*/
public class ExampleClass : MonoBehaviour {

    // Create a texture and fill it with Perlin noise.
    // Try varying the xOrg, yOrg and scale values in the inspector
    // while in Play mode to see the effect they have on the noise.

    // Width and height of the texture in pixels.
    public int pixWidth;
    public int pixHeight;
    // The origin of the sampled area in the plane.
    public float xOrg;
    public float yOrg;
    // The number of cycles of the basic noise pattern that are repeated
    // over the width and height of the texture.
    public float scale = 1.0F;

    private Texture2D noiseTex;
    private Color[] pix;
    private Renderer rend;

    void Start() {
        rend = GetComponent<Renderer>();
        CalcNoise();
    }

    void CalcNoise() {
        // Set up the texture and a Color array to hold pixels during processing.
        noiseTex = new Texture2D(pixWidth, pixHeight);
        pix = new Color[noiseTex.width * noiseTex.height];
        rend.material.mainTexture = noiseTex;

        float[,] noiseResult = new float[noiseTex.width, noiseTex.height];

        int y = 0;
        // For each pixel in the texture...
        while (y < noiseTex.height) {
            int x = 0;
            while (x < noiseTex.width) {
                // Get a sample from the corresponding position in the noise plane
                // and create a greyscale pixel from it.
                float xCoord = xOrg + x / noiseTex.width * scale;
                float yCoord = yOrg + y / noiseTex.height * scale;
                float sample = Mathf.PerlinNoise(xCoord, yCoord);

                noiseResult[x, y] = sample;
                Debug.Log(noiseResult[x, y]);

                pix[(y * noiseTex.width + x)] = new Color(sample, sample, sample);
                x++;
            }
            y++;
        }
        // Copy the pixel data to the texture and load it into the GPU.
        noiseTex.SetPixels(pix);
        noiseTex.Apply();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Space))
            CalcNoise();
    }
}