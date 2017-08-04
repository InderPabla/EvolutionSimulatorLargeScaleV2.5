using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class MapManager : MonoBehaviour {


    private bool ZDown = false;
    private bool XDown = false;
    private Vector3 mouseCoordsScreen;
    private Vector3 mouseCoordsWorld;
    private bool rightMouseDown = false;
    private bool leftMouseDown = false;
    private Vector3 initialMousePosition;
    private Vector3 initialCameraPosition; 
    private Texture2D texture;
    private bool textureLoaded = false;
    private Vector2 mouseTilePos;
    private int sizeX = 100;
    private int sizeY = 100;
    private int penSize = 0;
    private int penType = 0;

    private float hue = 0f;
    private float sat = 1f;
    private float bri = 1f;


    public Slider penSlider;
    public Text penSliderText;
    public Dropdown penDropdown;

    public RectTransform foodPanel;
    public RectTransform mutatePanel;
    public RectTransform waterPanel;
    public RectTransform spawnPanel;

    private Color tileDead = new Color(0.804f, 0.804f, 0.804f);
    private Color tileHighlight = new Color(0.4f,0.24f,0.8f);

    public Slider foodColorSlider;
    public Slider foodMaxSlider;
    public Image foodImage;
    public Toggle restrictToggle;
    //private HSBColor[,] tilesHSB = new HSBColor[100,100];
    private Tile_V2[,] tiles = new Tile_V2[100, 100];
    public Slider waterSlider;

    public InputField fileInput;
   
    private int[,] spawnRestricted = new int[100, 100];
    float maxFood = 1f;
    float currentFood = 1f;
    
    public void Start()
    {
        penSlider.onValueChanged.AddListener(OnPenSizeChanged);
        foodColorSlider.onValueChanged.AddListener(OnFoodColorChanged);
        foodMaxSlider.onValueChanged.AddListener(OnFoodMaxChanged);  
    }


    void Update ()
    {
        if (textureLoaded == true)
        {
            
            mouseTilePos = new Vector2((int)mouseCoordsWorld.x, (int)mouseCoordsWorld.y);
            if (mouseCoordsWorld.x < 0)
                mouseTilePos.x = -1;
            if (mouseCoordsWorld.y < 0)
                mouseTilePos.y = -1;

            for (int i = 0; i < sizeY; i++)
            {
                for (int j = 0; j < sizeX; j++)
                {
                    texture.SetPixel(j, i, tiles[i,j].detail.ToColor());

                    if (!(foodPanel.localPosition.x != -1000 || mutatePanel.localPosition.x != -1000 || waterPanel.localPosition.x != -1000))
                    {
                        
                        if (spawnRestricted[i,j] == 1)
                        {
                            texture.SetPixel(j,i, Color.gray);
                        }
                    }

                }
            }

            switch (penType)
            {
                case 0: SquareBrush((int)mouseTilePos.x, (int)mouseTilePos.y); break;
                case 1: CircleBrush((int)mouseTilePos.x, (int)mouseTilePos.y); break;
                case 2: TriangleBrush((int)mouseTilePos.x, (int)mouseTilePos.y); break;
                default: SquareBrush((int)mouseTilePos.x, (int)mouseTilePos.y); break;
            }

            texture.Apply();
            CameraMovement();
        }
    }

    public void AffectTile(int x, int y)
    {
        if (leftMouseDown == true)
        {
            if (foodPanel.localPosition.x != -1000)
            {
                tiles[y, x].detail = new HSBColor(hue, maxFood, bri);
                tiles[y, x].currentEnergy = maxFood;
                tiles[y, x].maxEnergy = maxFood;
                tiles[y, x].type = Tile_V2.TILE_FERT;
                //texture.SetPixel(x, y, tiles[y, x].detail.ToColor());
            }
            else if (mutatePanel.localPosition.x != -1000)
            {
                tiles[y, x].detail = new HSBColor(0f, 0f, 1f);
                tiles[y, x].type = Tile_V2.TILE_INFERT;
                //texture.SetPixel(x, y, tiles[y, x].detail.ToColor());
            }
            else if (waterPanel.localPosition.x != -1000)
            {
                tiles[y, x].detail = new HSBColor(0f, 0f, 0f);
                tiles[y, x].type = Tile_V2.TILE_WATER;
                //texture.SetPixel(x, y, tiles[y, x].detail.ToColor());
            }
            else if (spawnPanel.localPosition.x != -1000)
            {
                if(restrictToggle.isOn)
                    spawnRestricted[y, x] = 1;
                else
                    spawnRestricted[y, x] = 0;
                /*if (spawnRestricted[y, x] == 1)
                {
                    texture.SetPixel(x, y, Color.gray);
                }*/
            }
        }
    }

    public void TriangleBrush(int x, int y)
    {
        int x1 = x - penSize;
        int y1 = y - penSize+1;

        int x2 = x + penSize;
        int y2 = y - penSize + 1;

        int x3 = x;
        int y3 = y + penSize;


        if (IsValidLocation(x, y) == true)
        {
            for (int ip = y - penSize; ip <= y + penSize; ip++)
            {
                for (int jp = x - penSize; jp <= x + penSize; jp++)
                {
                    if (IsValidLocation(jp, ip))
                    {

                        double ABC = Math.Abs(x1 * (y2 - y3) + x2 * (y3 - y1) + x3 * (y1 - y2));
                        double ABP = Math.Abs(x1 * (y2 - ip) + x2 * (ip - y1) + jp * (y1 - y2));
                        double APC = Math.Abs(x1 * (ip - y3) + jp * (y3 - y1) + x3 * (y1 - ip));
                        double PBC = Math.Abs(jp * (y2 - y3) + x2 * (y3 - ip) + x3 * (ip - y2));

                        if (ABP + APC + PBC == ABC)
                        {
                            //texture.SetPixel(jp, ip, tileHighlight);
                            AffectTile(jp,ip);
                        }
                    }
                }
            }

        }
    }

    public void CircleBrush(int x, int y)
    {
        if (IsValidLocation(x, y) == true)
        {
            for (int ip = y - penSize; ip <= y + penSize; ip++)
            {
                for (int jp = x - penSize; jp <= x + penSize; jp++)
                {
                    if (IsValidLocation(jp, ip) &&  (((ip-y)*(ip-y)) + ((jp - x) * (jp - x))) <= (penSize-1)*(penSize-1))
                    {
                        //texture.SetPixel(jp, ip, tileHighlight);
                        AffectTile(jp, ip);
                    }
                }
            }

        }
    }

    public void SquareBrush(int x, int y)
    {
        if (IsValidLocation(x, y) == true)
        {
            for (int ip = y - penSize; ip <= y + penSize; ip++)
            {
                for (int jp = x - penSize; jp <= x + penSize ; jp++)
                {
                    if (IsValidLocation(jp, ip))
                    {
                        //texture.SetPixel(jp, ip, tileHighlight);
                        AffectTile(jp, ip);
                    }
                }
            }

        }
    }

    public bool IsValidLocation(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < sizeX && y < sizeY)
        {
            return true;
        }

        return false;
    }

    public void OnPenSizeChanged(float penSize)
    {
        this.penSize = (int)penSize-1;
        penSliderText.text = (int)penSize + "";
    }

  
    public void OnFoodColorChanged(float value)
    {
        hue = value;
        foodImage.color = new HSBColor(hue, sat, bri).ToColor();
    }

    public void OnFoodMaxChanged(float value)
    {
        maxFood = value;
        sat = value;

        bri = 1f - (0.25f - (sat * 0.25f));

        foodImage.color = new HSBColor(hue,sat,bri).ToColor();
    }

    public void OnFoodCurrentChanged(float value)
    {
        sat = value;
        bri = 1f - (0.25f - (sat * 0.25f));
        foodImage.color = new HSBColor(hue, sat, bri).ToColor();
    }

    public void OnPenTypeChanged()
    {
        penType = penDropdown.value;
    }


    public void FoodPanelButton()
    {
        SwitchPanels(foodPanel);
    }

    public void MutatePanelButton()
    {
        SwitchPanels(mutatePanel);
    }

    public void WaterPanelButton()
    {
        SwitchPanels(waterPanel);
    }

    public void SpawnPanelButton()
    {
        SwitchPanels(spawnPanel);
    }


    public void NoPanelButton()
    {
        SwitchPanels(null);
    }

    public void SwitchPanels(RectTransform rect)
    {
        foodPanel.localPosition = new Vector2(-1000f, -1000f);
        mutatePanel.localPosition = new Vector3(-1000f, -1000f);
        waterPanel.localPosition = new Vector3(-1000f, -1000f);
        spawnPanel.localPosition = new Vector3(-1000f, -1000f);

        if (rect != null)
        {
            rect.localPosition = new Vector2(25f, -47.75f);
            
        }
    }

    public void SaveButtonAction()
    {
        Regex regexItem = new Regex("^[a-zA-Z0-9 ]*$");
        string fileName = fileInput.text;
        if (!(fileName.Contains(" ") || fileName.Equals("")) && regexItem.IsMatch(fileName) ==true)
        {
            StreamWriter writer = new StreamWriter(fileName+".lsesmap");
            for (int i = 0; i < sizeY; i++)
            {
                for (int j = 0; j < sizeX; j++)
                {
                    if (tiles[i, j].type == Tile_V2.TILE_FERT)
                    {
                        writer.Write(tiles[i, j].type + " " + tiles[i, j].detail.h + " " + tiles[i, j].maxEnergy + " " + spawnRestricted[i, j] + " ");
                    }
                    else if (tiles[i, j].type == Tile_V2.TILE_INFERT)
                    {
                        writer.Write(tiles[i, j].type + " " + 1 + " " + spawnRestricted[i, j] + " ");
                    }
                    else if (tiles[i, j].type == Tile_V2.TILE_WATER)
                    {
                        writer.Write(tiles[i, j].type + " " + 1 + " " + spawnRestricted[i, j] + " ");
                    }
                }
            }
            writer.Close();

        }
    }

    public void LoadButtonAction()
    {
        Regex regexItem = new Regex("^[a-zA-Z0-9 ]*$");
        string fileName = fileInput.text;
        if (!(fileName.Contains(" ") || fileName.Equals("")) && regexItem.IsMatch(fileName) == true &&System.IO.File.Exists(fileName+".lsesmap") == true)
        {
            StreamReader reader = new StreamReader(fileName + ".lsesmap");
            string[] readAll = reader.ReadToEnd().Split(' ');
            reader.Close();
            int index = 0;

            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {

                    float type = float.Parse(readAll[index]);
                    index++;
                    if (type == Tile_V2.TILE_FERT)
                    {

                        float hue = float.Parse(readAll[index]);
                        index++;
                        float maxEnergy = float.Parse(readAll[index]);
                        index++;
                        float restrict = float.Parse(readAll[index]);
                        index++;

                        tiles[y, x].type = Tile_V2.TILE_FERT;
                        tiles[y, x].currentEnergy = maxEnergy;
                        tiles[y, x].maxEnergy = maxEnergy;

                        tiles[y, x].detail = new HSBColor(hue, maxEnergy, 1f - (0.25f - (maxEnergy * 0.25f)));

                        spawnRestricted[y, x] = (int)restrict;
                    }
                    else if (type == Tile_V2.TILE_INFERT)
                    {
                        float value = float.Parse(readAll[index]);
                        index++;
                        float restrict = float.Parse(readAll[index]);
                        index++;

                        tiles[y, x].type = Tile_V2.TILE_INFERT;
                        tiles[y, x].currentEnergy = 0f;
                        tiles[y, x].detail = new HSBColor(0,0,1);
                        spawnRestricted[y, x] = (int)restrict;
                    }
                    else if (type == Tile_V2.TILE_WATER)
                    {
                        float value = float.Parse(readAll[index]);
                        index++;
                        float restrict = float.Parse(readAll[index]);
                        index++;

                        tiles[y, x].type = Tile_V2.TILE_WATER;
                        tiles[y, x].currentEnergy = 0f;
                        tiles[y, x].detail = new HSBColor(0,0,0);
                        spawnRestricted[y, x] = (int)restrict;

                    }
                }
            }
        }
    }

    /// <summary>
    /// Sets the texture.
    /// </summary>
    /// <param name="tex">Tex.</param>
    public void SetTexture(Texture2D tex)
    {
        Debug.Log("Called");

        this.texture = tex;

        for (int i = 0; i < sizeY; i++)
        {
            for (int j = 0; j < sizeX; j++)
            {
                //tilesHSB[i, j] = HSBColor.FromColor(tileDead);
                tiles[i, j] = new Tile_V2();
                tiles[i, j].detail = new HSBColor(0, 0, 0);
                tiles[i, j].type = Tile_V2.TILE_WATER;
                tiles[i, j].currentEnergy = 0f;
                tiles[i, j].maxEnergy = 0f;
                texture.SetPixel(j,i,tiles[i, j].detail.ToColor());
            }
        }

        textureLoaded = true;
    }
    

    /// <summary>
    /// Controls the camera movement.
    /// </summary>
    void CameraMovement()
    {
        if (Input.GetKeyDown(KeyCode.Z) == true)
            ZDown = true;
        if (Input.GetKeyUp(KeyCode.Z) == true)
            ZDown = false;

        if (Input.GetKeyDown(KeyCode.X) == true)
            XDown = true;
        if (Input.GetKeyUp(KeyCode.X) == true)
            XDown = false;


        mouseCoordsScreen = Input.mousePosition;

        mouseCoordsWorld = Camera.main.ScreenToWorldPoint(mouseCoordsScreen);
        Vector3 tempMouseCoordsScreen = mouseCoordsScreen;

        //rT.position = mouseCoordsScreen;
        if (Input.GetMouseButtonDown(0))
        {
            leftMouseDown = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            leftMouseDown = false;
        }


        if (Input.GetMouseButtonDown(1))
        {
            rightMouseDown = true;
            initialMousePosition = Input.mousePosition;
            initialCameraPosition = Camera.main.transform.position;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            rightMouseDown = false;
        }

        if (rightMouseDown == true)
        {
            float ratio = (23f / Camera.main.orthographicSize) * 25f;
            tempMouseCoordsScreen = (initialMousePosition - tempMouseCoordsScreen) / ratio;
            Vector3 cameraPos = initialCameraPosition + tempMouseCoordsScreen;
            cameraPos.z = -111;

            Camera.main.transform.position = cameraPos;
        }

        float factorZoomOut = 0f;
        float factorZoomIn = 0f;

        if (Input.GetAxis("Mouse ScrollWheel") > 0) // forward
        {
            factorZoomOut = 3f;
        }
        else if (ZDown == true && XDown == false)
        {
            factorZoomOut = 0.75f;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            factorZoomIn = 3f;
        }
        else if (ZDown == false && XDown == true)
        {
            factorZoomIn = 1f;
        }

        if (factorZoomOut > 0f)
        {
            Camera.main.orthographicSize -= (Camera.main.orthographicSize / 23f) * factorZoomOut;
            if (Camera.main.orthographicSize < 0.01f)
                Camera.main.orthographicSize = 0.01f;


            Vector3 cameraPos = Camera.main.transform.position;
            Vector2 goToCameraPos = mouseCoordsWorld;

            cameraPos.z = 0;
            Vector2 posChange = Vector2.Lerp(cameraPos, mouseCoordsWorld, 0.1f);

            cameraPos = posChange;
            cameraPos.z = -111f;
            Camera.main.transform.position = cameraPos;
        }
        else if (factorZoomIn > 0)
        {
            Camera.main.orthographicSize += (Camera.main.orthographicSize / 23f) * factorZoomIn;
        }
    }

   
}


