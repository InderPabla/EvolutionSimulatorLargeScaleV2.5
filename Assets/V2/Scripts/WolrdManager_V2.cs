﻿using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine.SceneManagement;


public class WolrdManager_V2 : MonoBehaviour
{

    public GameObject creaturePrefab;
    public GameObject linePrefab;
    public TextMesh tileDataText;
    public AncestryTreeMaker ancestryTree;
    public GUINetDraw netDrawer;
    public bool runInBackground = false;

    private int sizeX = 100;
    private int sizeY = 100;

    private int totalCreaturesCount = 0;
    private int totalSpeciesCount = 0;

    public int playSpeed = 1;
    public int slowFactor = 1;


    private int slowFactorCounter = 0;

    public static float WORLD_CLOCK = 0f;
    private bool textureLoaded = false;

    private TileMap_V2 map_v2;

    private List<Creature_V2> creatureList;
    private List<Species> speciesList;

    private bool rightMouseDown;
    private bool leftMouseDown;
    private Vector3 initialMousePosition;
    private Vector3 finalMousePosition;
    private Vector3 initialCameraPosition;
    private int brainCalculations = 0;
    private bool visionState = false;

    private List<Color> speciesColor = new List<Color>();

    public RectTransform dropDownRect;
    public RectTransform settingRect;

    private Vector3 mouseCoordsScreen;
    private Vector3 mouseCoordsScreenDrag;
    private Vector3 mouseCoordsWorld;
    private bool paused = false;

    private bool isDropDropdown = false;
    private bool isSettings = false;
    private bool isSelected = false;
    private Vector3 clickLocation;
    private Vector2 markerLocationTopLeft;
    private Vector2 markerLocationBottomRight;
    private bool dragged = false;
    private int dropDownClickCount = 0;

    private string saveFile = "world_snapshot.lses";
    private NEATConsultor consultor;


    public static Semaphore sem;


    NetworkThread[] netWorkers;
    Thread[] netThreads;
    Color[] threadColors;

    CreatureOperationThread operationWorker;
    Thread operationThread;
    DynamicHyperParameter param;

    private string mapMakingScene = "sim_v2_map";
    private System.Random random = new System.Random(DateTime.Now.Millisecond);

    private string[] indexColorsString = new string[] {
        "#000000", "#FFFF00", "#1CE6FF", "#FF34FF", "#FF4A46", "#008941", "#006FA6", "#A30059",
        "#FFDBE5", "#7A4900", "#0000A6", "#63FFAC", "#B79762", "#004D43", "#8FB0FF", "#997D87",
        "#5A0007", "#809693", "#FEFFE6", "#1B4400", "#4FC601", "#3B5DFF", "#4A3B53", "#FF2F80",
        "#61615A", "#BA0900", "#6B7900", "#00C2A0", "#FFAA92", "#FF90C9", "#B903AA", "#D16100",
        "#DDEFFF", "#000035", "#7B4F4B", "#A1C299", "#300018", "#0AA6D8", "#013349", "#00846F",
        "#372101", "#FFB500", "#C2FFED", "#A079BF", "#CC0744", "#C0B9B2", "#C2FF99", "#001E09",
        "#00489C", "#6F0062", "#0CBD66", "#EEC3FF", "#456D75", "#B77B68", "#7A87A1", "#788D66",
        "#885578", "#FAD09F", "#FF8A9A", "#D157A0", "#BEC459", "#456648", "#0086ED", "#886F4C",

        "#34362D", "#B4A8BD", "#00A6AA", "#452C2C", "#636375", "#A3C8C9", "#FF913F", "#938A81",
        "#575329", "#00FECF", "#B05B6F", "#8CD0FF", "#3B9700", "#04F757", "#C8A1A1", "#1E6E00",
        "#7900D7", "#A77500", "#6367A9", "#A05837", "#6B002C", "#772600", "#D790FF", "#9B9700",
        "#549E79", "#FFF69F", "#201625", "#72418F", "#BC23FF", "#99ADC0", "#3A2465", "#922329",
        "#5B4534", "#FDE8DC", "#404E55", "#0089A3", "#CB7E98", "#A4E804", "#324E72", "#6A3A4C",
        "#83AB58", "#001C1E", "#D1F7CE", "#004B28", "#C8D0F6", "#A3A489", "#806C66", "#222800",
        "#BF5650", "#E83000", "#66796D", "#DA007C", "#FF1A59", "#8ADBB4", "#1E0200", "#5B4E51",
        "#C895C5", "#320033", "#FF6832", "#66E1D3", "#CFCDAC", "#D0AC94", "#7ED379", "#012C58"
    };



    private Color[] indexColors;



    /// <summary>
    /// Sets the texture.
    /// </summary>
    /// <param name="tex">Tex.</param>
    public void SetTexture(Texture2D tex)
    {
        Application.runInBackground = this.runInBackground;
        param = GetComponent<DynamicHyperParameter>();
        param.LoadHyperParameter(this);


        //Debug.Log("MAKING NETWORKING");
        sem = new Semaphore(param.numberOfTheads, param.numberOfTheads);
        netWorkers = new NetworkThread[param.numberOfTheads];
        netThreads = new Thread[param.numberOfTheads];
        for (int i = 0; i < param.numberOfTheads; i++)
        {
            netWorkers[i] = new NetworkThread(i);
        }

        /*operationWorker = new CreatureOperationThread(this);
        operationThread = new Thread(new ThreadStart(operationWorker.Worker));
        operationThread.Start();*/

        indexColors = new Color[indexColorsString.Length];
        for (int i = 0; i < indexColors.Length; i++)
        {
            indexColors[i] = HexToColor(indexColorsString[i].Substring(1));

        }

        threadColors = new Color[16];
        for (int i = 0; i < 16; i++)
        {
            //Color color = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
            threadColors[i] = indexColors[i];
        }

        map_v2 = new TileMap_V2(tex, sizeX, sizeY, param.climate, param.worldDeltaTime, param.seedSoilFracture, param.seedSoilColor, param.seedWater, param.seedSoil, param.seedSoilPower, param.seedFirt, param.seedSoilFirt, param.seedSoilFirtPower, param.useCustomMap, param.nutritionPower, param.nutritionAmplitude);
        Debug.Log("Inital Template Map Made");
    }


    Color HexToColor(string hex)
    {
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        return new Color32(r, g, b, 255);
    }

    // Update is called once per frame
    void Update()
    {
        if (textureLoaded == true)
        {

            if (paused == false)
            {

                //Debug.Log(slowFactorCounter+" "+slowFactor);
                if (slowFactorCounter >= slowFactor)
                {
                    //Debug.Log("----------------"); Debug.Log("----------------"); Debug.Log("----------------"); Debug.Log("----------------"); Debug.Log("----------------"); Debug.Log("----------------");

                    for (int itteration = 0; itteration < playSpeed; itteration++)
                    {


                        int count = creatureList.Count;
                        int numPerEach = count / netWorkers.Length;

                        for (int i = 0; i < netWorkers.Length; i++)
                        {

                            if (i < netWorkers.Length - 1)
                                netWorkers[i].Set(creatureList, (numPerEach * i), numPerEach * (i + 1));
                            else
                                netWorkers[i].Set(creatureList, (numPerEach * i), count);

                            netThreads[i] = new Thread(new ThreadStart(netWorkers[i].Worker));

                            sem.WaitOne();

                            netThreads[i].Start();
                        }


                        for (int i = 0; i < param.numberOfTheads; i++)
                            sem.WaitOne();

                        for (int creatureIndex = 0; creatureIndex < creatureList.Count; creatureIndex++)
                        {
                            creatureList[creatureIndex].UpdateAfterNetwork();
                            if (creatureList[creatureIndex].IsAlive() == false)
                            {

                                brainCalculations -= creatureList[creatureIndex].GetBrain().GetCalculations();

                                Species species = creatureList[creatureIndex].GetSpecies();
                                species.Remove(creatureList[creatureIndex]);

                                if (species.IsEmpty())
                                {
                                    Color color = species.GetColor();
                                    speciesColor.Add(color);

                                    int i = species.GetID();
                                    int low = 0, high = speciesList.Count - 1, mid;
                                    int index = -1;
                                    while (low <= high)
                                    {
                                        mid = (low + high) / 2;
                                        if (i < speciesList[mid].GetID())
                                            high = mid - 1;

                                        else if (i > speciesList[mid].GetID())
                                            low = mid + 1;

                                        else
                                        {
                                            index = mid;
                                            break;
                                        }
                                    }

                                    if (index != -1)
                                    {
                                        speciesList.RemoveAt(index);
                                    }

                                    //speciesList.Remove(species);

                                }

                                creatureList.RemoveAt(creatureIndex--);

                                //Debug.Log("Dead");
                                if (creatureList.Count < param.minCreatureCount)
                                {
                                    //Debug.Log("Spawn");
                                    CreateCreature();

                                    //operationWorker.CreateOperation();
                                }
                            }
                        }

                        for (int i = 0; i < param.numberOfTheads; i++)
                            sem.Release();


                        WORLD_CLOCK += param.worldDeltaTime;
                    }

                    slowFactorCounter = 0;
                }

                if (playSpeed < param.playSpeedVisual)
                {

                    float creatureCount = creatureList.Count;
                    Vector3 cameraPos = Camera.main.transform.position;
                    float height = 2 * Camera.main.orthographicSize;
                    float width = height * Camera.main.aspect;
                    Vector3 cameraSize = new Vector3(width, height, 0f);

                    Vector3 bottomLeft = cameraPos - (cameraSize / 2) - new Vector3(1, 1, 0);
                    Vector3 topRight = cameraPos + (cameraSize / 2) + new Vector3(1, 1, 0);

                    if (hide == true)
                    {
                        for (int creatureIndex = 0; creatureIndex < creatureCount; creatureIndex++)
                        {
                            creatureList[creatureIndex].Show();
                        }
                        hide = false;

                        Debug.Log("Showing All Creatures");
                    }


                    for (int creatureIndex = 0; creatureIndex < creatureCount; creatureIndex++)
                    {
                        creatureList[creatureIndex].UpdateRender(visionState, bottomLeft, topRight, param.drawThreadColor, param.speciesTileVisual);

                    }
                }
                else
                {
                    if (hide == false)
                    {
                        float creatureCount = creatureList.Count;
                        for (int creatureIndex = 0; creatureIndex < creatureCount; creatureIndex++)
                        {
                            creatureList[creatureIndex].Hide();
                        }
                        hide = true;

                        Debug.Log("Hiding All Creatures");
                    }
                }

                WorkRenderBacklog(playSpeed < param.playSpeedVisual);


                map_v2.Apply(playSpeed, playSpeed < param.playSpeedVisual);

                map_v2.ColorSpeciesTerritory(speciesList, playSpeed < param.playSpeedVisual, param.speciesTileVisual);


            }
            slowFactorCounter++;

        }

        CameraMovement();
        GUIStates();


    }
    bool hide = true;

    /// <summary>
    /// Handles the GUI states of the buttons.
    /// </summary>
    void GUIStates()
    {
        if (creatureList != null)
        {
            netDrawer.SetWorldDrawInformation(totalCreaturesCount, creatureList.Count, playSpeed > 1 ? playSpeed : (float)playSpeed / (float)slowFactor, brainCalculations);
        }

        if (netDrawer.createButtonState == 2)
        {
            if (textureLoaded == false)
            {
                netDrawer.SetStarted(true);
            }
            else
            {
                Debug.Log("SettingsButtonAction");
                isSettings = !isSettings;
                paused = isSettings;

                if (paused == true)
                {
                    settingRect.position = new Vector2(Screen.width * 0.65f, Screen.height / 2f);
                }
                else
                {
                    settingRect.position = new Vector2(-1000, -1000);
                }
            }

            MakeWorld();

        }
        else if (netDrawer.fastButtonState == 2)
        {
            if (slowFactor > 1)
            {
                slowFactor = slowFactor / 2;
                if (slowFactor <= 0)
                {
                    slowFactor = 1;
                }
            }
            else
            {
                playSpeed = playSpeed * 2;
            }
        }
        else if (netDrawer.slowButtonState == 2 || (netDrawer.slowButtonState > 0 && playSpeed > 4))
        {

            if (playSpeed > 1)
            {
                playSpeed = playSpeed / 2;

                if (playSpeed > 4)
                    playSpeed = 4;

                if (playSpeed <= 0)
                {
                    playSpeed = 1;
                }
            }
            else
            {
                slowFactor = slowFactor * 2;
            }
        }
        else if (netDrawer.visionButtonState == 2)
        {
            visionState = !visionState;
        }
        else if (netDrawer.saveButtonState == 2 && textureLoaded == true)
        {
            SaveButtonAction();
        }
        else if (netDrawer.loadButtonState == 2)
        {
            if (textureLoaded == false)
            {
                netDrawer.SetStarted(true);
            }
            LoadButtonAction();
        }
        else if (netDrawer.mapButtonState == 2)
        {
            SceneManager.LoadScene(mapMakingScene);
        }

        if (Input.GetMouseButtonDown(0))
        {
            leftMouseDown = true;
            mouseCoordsScreenDrag = mouseCoordsScreen;
            initialMousePosition = mouseCoordsWorld;
            finalMousePosition = mouseCoordsWorld;
            /*leftMouseDown = true;

            if (netDrawer.isGUISelected == false)
            {
                if (isDropDropdown == false)
                {
                    dropDownRect.position = mouseCoordsScreen + new Vector3(dropDownRect.rect.width / 2f, dropDownRect.rect.height / 2f);
                    isDropDropdown = true;
                    paused = true;
                    clickLocation = mouseCoordsWorld;
                    count = 1;
                }
            }*/
            /*if (dragged == false && mouseCoordsScreenOld!=)
            {

            }*/
        }
        else if (Input.GetMouseButtonUp(0))
        {

            /*leftMouseDown = false;

            if (count == 0)
            {
                if (isSelected == true)
                {
                    dropDownRect.position = new Vector2(-100, -100);
                    isSelected = false;
                    isDropDropdown = false;
                    paused = false;
                }
                else if (isSelected == false && isDropDropdown == true)
                {
                    dropDownRect.position = new Vector2(-100, -100);
                    isSelected = false;
                    isDropDropdown = false;
                    paused = false;
                }
            }
            count--;*/

            if (netDrawer.isGUISelected == false && textureLoaded == true && isSettings == false)
            {
                if (isDropDropdown == false && dragged == false)
                {
                    dropDownRect.position = mouseCoordsScreen + new Vector3(dropDownRect.rect.width / 2f, dropDownRect.rect.height / 2f);

                    isDropDropdown = true;
                    paused = true;
                    clickLocation = mouseCoordsWorld;
                    dropDownClickCount = 1;
                }
                else if (dropDownClickCount == 0)
                {
                    if (isSelected == true)
                    {
                        dropDownRect.position = new Vector2(-1000, -1000);
                        isSelected = false;
                        isDropDropdown = false;
                        paused = false;
                    }
                    else if (isSelected == false && isDropDropdown == true)
                    {
                        dropDownRect.position = new Vector2(-1000, -1000);
                        isSelected = false;
                        isDropDropdown = false;
                        paused = false;
                    }
                }
                dropDownClickCount = 0;
            }

            dragged = false;
            leftMouseDown = false;
        }

        if (leftMouseDown == true)
        {
            if (dragged == false && (mouseCoordsScreenDrag.x != mouseCoordsScreen.x || mouseCoordsScreenDrag.y != mouseCoordsScreen.y))
            {
                dragged = true;
            }

            if (dragged == true)
            {
                finalMousePosition = mouseCoordsWorld;

                Vector2 point1 = initialMousePosition;
                Vector2 point2 = finalMousePosition;

                if (initialMousePosition.x > finalMousePosition.x)
                {
                    float temp = point1.x;
                    point1.x = point2.x;
                    point2.x = temp;
                }

                if (initialMousePosition.y > finalMousePosition.y)
                {
                    float tempy = point1.y;
                    point1.y = point2.y;
                    point2.y = tempy;
                }

                Rect rect = new Rect(point1.x, point1.y, point2.x - point1.x, point2.y - point1.y);
                for (int i = 0; i < sizeX; i++)
                {
                    for (int j = 0; j < sizeY; j++)
                    {
                        if (rect.Contains(new Vector2(j, i)))
                        {
                            map_v2.SetSelected(j, i);
                        }
                        else
                        {
                            map_v2.RemoveSelected(j, i);
                        }
                    }
                }
            }
        }
    }

    private bool ZDown = false;
    private bool XDown = false;



    /// <summary>
    /// Controls the camera movement.
    /// </summary>
    void CameraMovement()
    {
        if (isSettings == false)
        {
            if (Input.GetKeyDown(KeyCode.Z) == true)
                ZDown = true;
            if (Input.GetKeyUp(KeyCode.Z) == true)
                ZDown = false;

            if (Input.GetKeyDown(KeyCode.X) == true)
                XDown = true;
            if (Input.GetKeyUp(KeyCode.X) == true)
                XDown = false;

            if (Input.GetKeyUp(KeyCode.Delete) == true)
                DeleteSelectedButtonAction();

            if (Input.GetKeyUp(KeyCode.Space) == true)
                FindBiggestAncestoryTreeButtonAction();

            mouseCoordsScreen = Input.mousePosition;
            mouseCoordsWorld = Camera.main.ScreenToWorldPoint(mouseCoordsScreen);
            Vector3 tempMouseCoordsScreen = mouseCoordsScreen;

            //rT.position = mouseCoordsScreen;

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

            TileDataTextPlacement(mouseCoordsWorld);
        }

    }


    /// <summary>
    /// Button to handle the normal brain function.
    /// </summary>
    public void NormalBrainButtonAction()
    {
        Debug.Log("NormalBrainButtonAction");
        isSelected = true;

        List<Creature_V2> creatures = map_v2.ExistCreatureAtTile((int)clickLocation.x, (int)clickLocation.y);
        if (creatures != null)
        {
            creatures = creatures.OrderBy(o => o.GetID()).ToList();
            for (int i = 0; i < creatures.Count; i++)
            {
                if (creatures[i].CollisionCheckWithPoint(clickLocation) == true)
                {
                    ancestryTree.ResetAllNodes();
                    netDrawer.ResetBrain();


                    ancestryTree.MakeTree(creatures[i]);
                    netDrawer.SetBrain(ancestryTree.GetSelectedCreature().GetBrain(), ancestryTree.GetTreeDataList(), ancestryTree.GetSelectedCreature());

                }
            }
        }
    }

    /// <summary>
    /// Button to handle the difference brain function.
    /// </summary>
    public void DifferenceBrainButtonAction()
    {
        Debug.Log("DifferenceBrainButtonAction");
        isSelected = true;

        List<Creature_V2> creatures = map_v2.ExistCreatureAtTile((int)clickLocation.x, (int)clickLocation.y);
        if (creatures != null)
        {
            creatures = creatures.OrderBy(o => o.GetID()).ToList();

            for (int i = 0; i < creatures.Count; i++)
            {
                if (creatures[i].CollisionCheckWithPoint(clickLocation) == true)
                {
                    if (netDrawer.IsBrainNull() == true)
                    {
                        ancestryTree.ResetAllNodes();
                        netDrawer.ResetBrain();
                        ancestryTree.MakeTree(creatures[i]);
                        netDrawer.SetBrain(ancestryTree.GetSelectedCreature().GetBrain(), ancestryTree.GetTreeDataList(), ancestryTree.GetSelectedCreature());
                    }
                    else
                    {
                        //IMPORTANT FOR MAKING DIFFERENCE NET!
                        /*NEATBrain differenceNet = NEATBrain.MakeDifferentialBrain(netDrawer.GetBrain(), creatures[i].GetBrain());
                        netDrawer.AddDifferenceNetwork(differenceNet);*/

                        netDrawer.AddDifferenceConnection(creatures[i].GetBrain());
                    }
                }
            }
        }
    }

    /// <summary>
    /// Button action to delete the selected.
    /// </summary>
    public void DeleteSelectedButtonAction()
    {
        Debug.Log("DeleteSelectedButtonAction");
        isSelected = true;

        map_v2.DeleteAllBodiesOnSelected();
    }

    /// <summary>
    /// Button action to find the oldest.
    /// </summary>
    public void FindOldestButtonAction()
    {
        Debug.Log("FindOldestButtonAction");
        isSelected = true;

        List<Creature_V2> allSelectedCreatures = map_v2.GetAllBodiesOnSelected();
        allSelectedCreatures = allSelectedCreatures.OrderBy(o => o.GetID()).ToList();

        ancestryTree.ResetAllNodes();
        netDrawer.ResetBrain();

        if (allSelectedCreatures.Count > 0)
        {
            ancestryTree.MakeTree(allSelectedCreatures[0]);
            netDrawer.SetBrain(ancestryTree.GetSelectedCreature().GetBrain(), ancestryTree.GetTreeDataList(), ancestryTree.GetSelectedCreature());
        }
    }

    /// <summary>
    /// Button action to find the youngest.
    /// </summary>
    public void FindYoungestButtonAction()
    {
        Debug.Log("FindYoungestButtonAction");
        isSelected = true;

        List<Creature_V2> allSelectedCreatures = map_v2.GetAllBodiesOnSelected();
        allSelectedCreatures = allSelectedCreatures.OrderByDescending(o => o.GetID()).ToList();

        ancestryTree.ResetAllNodes();
        netDrawer.ResetBrain();

        if (allSelectedCreatures.Count > 0)
        {
            ancestryTree.MakeTree(allSelectedCreatures[0]);
            netDrawer.SetBrain(ancestryTree.GetSelectedCreature().GetBrain(), ancestryTree.GetTreeDataList(), ancestryTree.GetSelectedCreature());
        }
    }

    /// <summary>
    /// Button action to find the biggest size.
    /// </summary>
    public void FindBiggestSizeButtonAction()
    {
        Debug.Log("FindBiggestSizeButtonAction");
        isSelected = true;

        List<Creature_V2> allSelectedCreatures = map_v2.GetAllBodiesOnSelected();
        allSelectedCreatures = allSelectedCreatures.OrderByDescending(o => o.GetRadius()).ToList();

        ancestryTree.ResetAllNodes();
        netDrawer.ResetBrain();

        if (allSelectedCreatures.Count > 0)
        {
            ancestryTree.MakeTree(allSelectedCreatures[0]);
            netDrawer.SetBrain(ancestryTree.GetSelectedCreature().GetBrain(), ancestryTree.GetTreeDataList(), ancestryTree.GetSelectedCreature());
        }
    }

    /// <summary>
    /// Button action to find the biggest brain.
    /// </summary>
    public void FindBiggestBrainButtonAction()
    {
        Debug.Log("FindBiggestBrainButtonAction");
        isSelected = true;

        List<Creature_V2> allSelectedCreatures = map_v2.GetAllBodiesOnSelected();
        allSelectedCreatures = allSelectedCreatures.OrderByDescending(o => o.GetBrain().GetCalculations()).ToList();

        ancestryTree.ResetAllNodes();
        netDrawer.ResetBrain();

        if (allSelectedCreatures.Count > 0)
        {
            ancestryTree.MakeTree(allSelectedCreatures[0]);
            netDrawer.SetBrain(ancestryTree.GetSelectedCreature().GetBrain(), ancestryTree.GetTreeDataList(), ancestryTree.GetSelectedCreature());
        }
    }

    public void KillSpeciesButtonAction()
    {
        Debug.Log("KillSpeciesButtonAction");
        isSelected = true;

        List<Creature_V2> creatures = map_v2.ExistCreatureAtTile((int)clickLocation.x, (int)clickLocation.y);
        if (creatures != null)
        {
            creatures = creatures.OrderBy(o => o.GetID()).ToList();
            for (int i = 0; i < creatures.Count; i++)
            {
                if (creatures[i].CollisionCheckWithPoint(clickLocation) == true)
                {
                    creatures[i].GetSpecies().KillSpecies();
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Button action to find the biggest ancestory tree.
    /// </summary>
    public void FindBiggestAncestoryTreeButtonAction()
    {
        Debug.Log("FindBiggestAncestoryTree");
        isSelected = true;

        List<Creature_V2> allSelectedCreatures = map_v2.GetAllBodiesOnSelected();
        allSelectedCreatures = allSelectedCreatures.OrderByDescending(o => o.GetChildCount()).ToList();

        ancestryTree.ResetAllNodes();
        netDrawer.ResetBrain();

        if (allSelectedCreatures.Count > 0)
        {
            ancestryTree.MakeTree(allSelectedCreatures[0]);
            netDrawer.SetBrain(ancestryTree.GetSelectedCreature().GetBrain(), ancestryTree.GetTreeDataList(), ancestryTree.GetSelectedCreature());
        }
    }

    /// <summary>
    /// Button action to move the selected.
    /// </summary>
    public void MoveSelectedButtonAction()
    {
        Debug.Log("MoveSelectedButtonAction");
        isSelected = true;

        List<Creature_V2> allSelectedCreatures = map_v2.GetAllBodiesOnSelected();
        allSelectedCreatures = allSelectedCreatures.OrderByDescending(o => o.GetBrain().GetCalculations()).ToList();

        if (allSelectedCreatures.Count > 0)
        {
            Vector3 averageLocation = Vector3.zero;
            for (int i = 0; i < allSelectedCreatures.Count; i++)
            {
                averageLocation += allSelectedCreatures[i].position;
            }

            averageLocation /= allSelectedCreatures.Count;
            Vector3 displacement = clickLocation - averageLocation;
            displacement.z = 0;
            for (int i = 0; i < allSelectedCreatures.Count; i++)
            {
                allSelectedCreatures[i].position += displacement;
            }
        }

    }

    /// <summary>
    /// Drawing heat map
    /// </summary>
    public void DrawHeatMapButtonAction()
    {
        Debug.Log("DrawHeatMapButtonAction");
        map_v2.ToggleDrawHeat();
    }

    /// <summary>
    /// Switching heat types between heated metal and incandecent
    /// </summary>
    public void HeatTypeButtonAction()
    {
        Debug.Log("HeatTypeButtonAction");
        map_v2.ToggleHeatType();
    }

    /// <summary>
    /// Switching heat types between heated metal and incandecent
    /// </summary>
    public void DrawSpeciesMapButtonAction()
    {
        Debug.Log("DrawSpeciesMapButtonAction");
        param.speciesTileVisual = !param.speciesTileVisual;
    }

    /// <summary>
    /// Toggling thread color for creature. 
    /// Just a visual representation to show which creature belongs to which thread. 
    /// </summary>
    public void DrawThreadColorButtonAction()
    {
        Debug.Log("DrawThreadColorButtonAction");
        param.drawThreadColor = !param.drawThreadColor;
    }

    public void NukeButtonAction()
    {
        Debug.Log("NukeButtonAction");
        map_v2.NukeTiles(5, (int)clickLocation.x, (int)clickLocation.y);
    }


    /// <summary>
    /// Button action to save data.
    /// </summary>
    private void SaveButtonAction()
    {
        Debug.Log("SaveButtonAction");

        StreamWriter writer = new StreamWriter(saveFile);

        writer.Write(WORLD_CLOCK + " ");

        writer.Write(param.layer.Length + " ");
        for (int i = 0; i < param.layer.Length; i++)
            writer.Write(param.layer[i] + " ");

        writer.Write(creatureList.Count + " ");
        writer.Write(totalCreaturesCount + " ");

        writer.Write(param.initialVisionAngles.Length + " ");
        for (int i = 0; i < param.initialVisionAngles.Length; i++)
            writer.Write(param.initialVisionAngles[i] + " ");


        for (int i = 0; i < creatureList.Count; i++)
        {
            creatureList[i].StreamWriteCreatureData(writer);
        }

        Tile_V2[,] tiles = map_v2.GetTilesArray();

        for (int j = 0; j < 100; j++)
        {
            for (int k = 0; k < 100; k++)
            {
                writer.Write(tiles[j, k].currentEnergy + " ");
            }
        }


        /*writer.Write(maxHiddenNeurons + " ");
        writer.Write(creatureList.Count + " ");
        writer.Write(totalCreaturesCount + " ");
        consultor.StreamWriteConsultorGenome(writer);

        for (int i = 0; i < creatureList.Count; i++)
        {
            creatureList[i].StreamWriteCreatureData(writer);
        }

        Tile_V2[,] tiles = map_v2.GetTilesArray();

        for (int j = 0; j < 100; j++)
        {
            for (int k = 0; k < 100; k++)
            {
                writer.Write(tiles[j, k].currentEnergy + " ");
            }
        }*/

        writer.Close();
    }

    /// <summary>
    /// Button action to load data.
    /// </summary>
    private void LoadButtonAction()
    {
        if (textureLoaded == false && File.Exists(saveFile))
        {
            Debug.Log("Load Button Action");
            StreamReader reader = new StreamReader(saveFile);
            creatureList = new List<Creature_V2>();
            speciesList = new List<Species>();
            netDrawer.SetSpeciesList(speciesList);
            for (int i = 0; i < indexColors.Length; i++)
            {
                speciesColor.Add(indexColors[i]);
            }

            for (int i = 0; i < 1000 - indexColors.Length; i++)
            {
                speciesColor.Add(new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f)));
            }

            string[] readAll = reader.ReadToEnd().Split(' ');
            reader.Close();

            int actualLength = readAll.Length - 1;
            int index = 0;

            WORLD_CLOCK = float.Parse(readAll[index]);
            index++;

            //MLP layers
            int layerSize = int.Parse(readAll[index]);
            param.layer = new int[layerSize];
            int[] fixedLayer = new int[layerSize];
            index++;
            for (int i = 0; i < layerSize; i++)
            {
                param.layer[i] = int.Parse(readAll[index]);

                if (i < param.layer.Length - 2)
                {
                    fixedLayer[i] = param.layer[i] + param.layer[i + 1];
                }
                else
                {
                    fixedLayer[i] = param.layer[i];
                }

                index++;
            }

            for (int i = 0; i < param.layer.Length; i++)
            {
                fixedLayer[i] = param.layer[i];
                if (i < param.layer.Length - 2)
                {
                    fixedLayer[i] = param.layer[i] + param.layer[i + 1];
                }
                else
                {
                    fixedLayer[i] = param.layer[i];
                }
            }


            int numberOfCreatures = int.Parse(readAll[index]);
            index++;
            totalCreaturesCount = int.Parse(readAll[index]);
            index++;

            param.numberOfEyeSensors = (int)float.Parse(readAll[index]); index++;
            float[] visions = new float[param.numberOfEyeSensors];

            for (int j = 0; j < param.numberOfEyeSensors; j++)
            {
                visions[j] = float.Parse(readAll[index]); index++;
            }
            param.initialVisionAngles = visions;

            //totalCreaturesCount = 0;
            Debug.Log("Number Of Creatures: " + numberOfCreatures + ", Total Creature Count: " + totalCreaturesCount);
            for (int i = 0; i < numberOfCreatures; i++)
            {
                int ID = int.Parse(readAll[index]); index++;
                float timeLived = float.Parse(readAll[index]); index++;
                int generation = int.Parse(readAll[index]); index++;
                String creatureName = readAll[index]; index++;
                String parentNames = readAll[index]; index++;

                float energy = float.Parse(readAll[index]); index++;
                float life = float.Parse(readAll[index]); index++;

                Vector2 position = new Vector2(float.Parse(readAll[index]), float.Parse(readAll[index + 1])); index++; index++;
                float rotation = float.Parse(readAll[index]); index++;
                float veloForward = float.Parse(readAll[index]); index++;
                float veloAngular = float.Parse(readAll[index]); index++;

                float bodyHue = float.Parse(readAll[index]); index++;
                float eggBirthTimer = float.Parse(readAll[index]); index++;
                float predLevel = float.Parse(readAll[index]); index++;

                int numberOfAngles = (int)float.Parse(readAll[index]); index++;
                float[] visionAngles = new float[numberOfAngles];
                float[] visionDistances  = new float[numberOfAngles];
                for (int j = 0; j < numberOfAngles; j++)
                {
                    visionAngles[j] = float.Parse(readAll[index]); index++;
                }

                for (int j = 0; j < numberOfAngles; j++)
                {
                    visionDistances[j] = float.Parse(readAll[index]); index++;
                }

                //float[] visionAngles = new float[] { float.Parse(readAll[index + 15]), float.Parse(readAll[index + 16]), float.Parse(readAll[index + 17]), float.Parse(readAll[index + 18]) };

                //index += 19; //increment

                float[][][] weights;

                //Weights Initilization
                /*List<float[][]> weightsList = new List<float[][]>();

                for (int j = 1; j < fixedLayer.Length; j++)
                {
                    List<float[]> layerWeightsList = new List<float[]>(); //layer weights list

                    int neuronsInPreviousLayer = fixedLayer[j - 1];

                    for (int k = 0; k < fixedLayer[j]; k++)
                    {
                        float[] neuronWeights = new float[neuronsInPreviousLayer]; //neruons weights

                        for (int l = 0; l < neuronsInPreviousLayer; l++)
                        {
                            neuronWeights[l] = float.Parse(readAll[index]); index++;
                        }

                        layerWeightsList.Add(neuronWeights);
                    }
                    weightsList.Add(layerWeightsList.ToArray());
                }*/

                List<float[][]> weightsList = new List<float[][]>();

                for (int ii = 1; ii < param.layer.Length; ii++)
                {
                    List<float[]> layerWeightsList = new List<float[]>(); //layer weights list



                    for (int jj = 0; jj < param.layer[ii]; jj++)
                    {
                        int neuronsInPreviousLayer = fixedLayer[ii - 1];

                        if ((ii >= param.layer.Length - 2) || (jj < param.layer[ii]))
                        {
                            //neuronsInPreviousLayer = hiddenLayers[i];
                            float[] neuronWeights = new float[neuronsInPreviousLayer]; //neruons weights

                            //set the weights randomly between 1 and -1
                            for (int kk = 0; kk < neuronsInPreviousLayer; kk++)
                            {
                                neuronWeights[kk] = float.Parse(readAll[index]); index++;
                            }

                            layerWeightsList.Add(neuronWeights);
                        }
                        else
                        {
                            float[] neuronWeights = new float[0]; //neruons weights

                            layerWeightsList.Add(neuronWeights);
                        }


                    }
                    weightsList.Add(layerWeightsList.ToArray());
                }

                weights = weightsList.ToArray(); //convert list to array


                //Debug.Log(i);
                //CreateCreature(ID, generation, creatureName, parentNames, energy, life, position, rotation, veloForward, veloAngular, bodyHue, eggBirthTimer, predLevel, visionAngles, weights);
                BehaviourGenome genome = new BehaviourGenome(bodyHue, eggBirthTimer, predLevel, visionAngles, visionDistances, param.minVisionLength);
                CreateCreature(ID, timeLived, generation, creatureName, parentNames, position, weights, genome, energy, life, rotation, veloForward, veloAngular);


            }

            float[,] currentEnergyArray = new float[100, 100];

            for (int j = 0; j < 100; j++)
            {
                for (int k = 0; k < 100; k++)
                {
                    currentEnergyArray[j, k] = float.Parse(readAll[index]);
                    index++; //increment
                }
            }


            map_v2.SetCurrentEnergy(currentEnergyArray);



            /*consultor = NEATConsultor.GetInstance();

            brainCalculations = 0;

            for (int i = 0; i < indexColors.Length; i++)
            {
                speciesColor.Add(indexColors[i]);     
            }

            for (int i = 0; i < 1000-indexColors.Length; i++)
            {
                speciesColor.Add(new Color(UnityEngine.Random.Range(0f,1f),UnityEngine.Random.Range(0f, 1f),UnityEngine.Random.Range(0f, 1f)));
            }

            maxHiddenNeurons = int.Parse(readAll[index]);
            index++;//increment
            int numberOfCreatures = int.Parse(readAll[index]);
            index++;//increment
            totalCreaturesCount = int.Parse(readAll[index]);
            index++;//increment
            int consultorGenomeSize = int.Parse(readAll[index]);
            index++;//increment

            for (int i = 0; i < consultorGenomeSize; i++)
            {
                int innovation = int.Parse(readAll[index]);
                int inNode = int.Parse(readAll[index + 1]);
                int outNode = int.Parse(readAll[index + 2]);
                Gene gene = new Gene(innovation, inNode, outNode, 0f, true);
                consultor.AddGene(gene);

                index += 3; //increment
            }

            for (int i = 0; i < numberOfCreatures; i++)
            {
                int ID = int.Parse(readAll[index]);
                int generation = int.Parse(readAll[index + 1]);
                String creatureName = readAll[index + 2];
                String parentNames = readAll[index + 3];

                float energy = float.Parse(readAll[index + 4]);
                float life = float.Parse(readAll[index + 5]);

                Vector2 position = new Vector2(float.Parse(readAll[index + 6]), float.Parse(readAll[index + 7]));
                float rotation = float.Parse(readAll[index + 8]);
                float veloForward = float.Parse(readAll[index + 9]);
                float veloAngular = float.Parse(readAll[index + 10]);

                float bodyHue = float.Parse(readAll[index + 11]);
                float eggBirthTimer = float.Parse(readAll[index + 12]);
                float predLevel = float.Parse(readAll[index + 13]);
                float[] visionAngles = new float[] { float.Parse(readAll[index + 14]), float.Parse(readAll[index + 15]), float.Parse(readAll[index + 16]), float.Parse(readAll[index + 17]) };

                index += 18; //increment

                int numberOfGenes = int.Parse(readAll[index]);
                index++; //increment

                int usedHiddenNeuronIndex = int.Parse(readAll[index]);
                index++; //increment

                List<Gene> genome = new List<Gene>();

                for (int j = 0; j < numberOfGenes; j++)
                {
                    int inNode = int.Parse(readAll[index]);
                    int outNode = int.Parse(readAll[index + 1]);
                    float weight = float.Parse(readAll[index + 2]);
                    bool active = int.Parse(readAll[index + 3]) == 1 ? true : false;
                    int inno = consultor.GetInnovationNumber(inNode, outNode);
                    Gene gene = new Gene(inno, inNode, outNode, weight, active);
                    genome.Add(gene);
                    index += 4; //increment
                }

                CreateCreature(ID, generation, creatureName, parentNames, energy, life, position, rotation, veloForward, veloAngular, bodyHue, eggBirthTimer, predLevel, visionAngles, usedHiddenNeuronIndex, genome);

            }

            float[,] currentEnergyArray = new float[100, 100];

            for (int j = 0; j < 100; j++)
            {
                for (int k = 0; k < 100; k++)
                {
                    currentEnergyArray[j, k] = float.Parse(readAll[index]);
                    index++; //increment
                }
            }

            map_v2.SetCurrentEnergy(currentEnergyArray);*/

            textureLoaded = true;
            Debug.Log("Map Made From Loading LSES File.");
        }

        /*if (File.Exists(filename))
        {
            if (textureLoaded == false)
            {
                textureLoaded = true;


                StreamReader reader = new StreamReader(filename);
                creatureList = new List<Creature_V2>();

                string[] readAll = reader.ReadToEnd().Split(' ');
                int actualLength = readAll.Length - 1;
                int index = 0;

                //make brain network
                int brainLength = int.Parse(readAll[0]);
                brainNetwork = new int[brainLength];
                index++;

                for (int i = 0; i < brainLength; i++, index++)
                {
                    brainNetwork[i] = int.Parse(readAll[index]);
                }
                brainCalculations = new Brain_V2(brainNetwork, -1, 0, 0, 0, 0, 0, 0).GetCalculations();

                int numberOfCreatures = int.Parse(readAll[index]);
                index++;

                for (int creatureIndex = 0; creatureIndex < numberOfCreatures; creatureIndex++)
                {
                    string name = readAll[index]; index++;
                    string parnetNames = readAll[index]; index++;
                    float energy = float.Parse(readAll[index]); index++;
                    float life = float.Parse(readAll[index]); index++;
                    Vector2 position = new Vector2(float.Parse(readAll[index]), float.Parse(readAll[index + 1])); index += 2;
                    float rotation = float.Parse(readAll[index]); index++;
                    float veloForward = float.Parse(readAll[index]); index++;
                    float veloAngular = float.Parse(readAll[index]); index++;
                    float[][][] weights;

                    //Weights Initilization
                    List<float[][]> weightsList = new List<float[][]>();

                    for (int i = 1; i < brainNetwork.Length; i++)
                    {
                        List<float[]> layerWeightsList = new List<float[]>(); //layer weights list

                        int neuronsInPreviousLayer = brainNetwork[i - 1];

                        for (int j = 0; j < brainNetwork[i]; j++)
                        {
                            float[] neuronWeights = new float[neuronsInPreviousLayer]; //neruons weights

                            for (int k = 0; k < neuronsInPreviousLayer; k++)
                            {
                                neuronWeights[k] = float.Parse(readAll[index]); index++;
                            }

                            layerWeightsList.Add(neuronWeights);
                        }
                        weightsList.Add(layerWeightsList.ToArray());
                    }

                    weights = weightsList.ToArray(); //convert list to array

                    CreateCreature(energy, life, veloForward, veloAngular, name, parnetNames, position, rotation, weights);
                }

                float[,] currentEnergyArray = new float[100, 100];

                for (int j = 0; j < 100; j++)
                {
                    for (int k = 0; k < 100; k++)
                    {
                        currentEnergyArray[j, k] = float.Parse(readAll[index]);
                        index++;
                    }
                }

                map_v2.SetCurrentEnergy(currentEnergyArray);

                reader.Close();
            }
        }*/
    }


    /* public void KillButtonAction()
     {
         Debug.Log("DeleteButtonAction");
         isSelected = true;

         List<Creature_V2> creatures = map_v2.ExistCreatureAtTile((int)clickLocation.x, (int)clickLocation.y);
         creatures = creatures.OrderBy(o => o.GetID()).ToList();

         for (int i = 0; i < creatures.Count; i++)
         {
             creatures[i].KillWithEnergy();
         }
     }*/

    /// <summary>
    /// Places the text data at the tile where the mouse is.
    /// </summary>
    /// <param name="mouse">Mouse.</param>
    private void TileDataTextPlacement(Vector2 mouse)
    {
        if (map_v2.IsValidLocation((int)mouse.x, (int)mouse.y))
        {
            tileDataText.text = map_v2.TileToString((int)mouse.x, (int)mouse.y);
            tileDataText.transform.position = new Vector3((int)mouse.x + 0.5f, (int)mouse.y + 0.5f, tileDataText.transform.position.z);
        }
    }



    /* private void ButtonActionCheck()
     {
         if (Input.GetKeyDown(KeyCode.Delete))
         {
             //OnDeletePress();
         }
         else if (Input.GetKeyDown(KeyCode.Space))
         {
             //OnSpacePress();
         }
         else if (Input.GetKeyDown(KeyCode.B))
         {
             //OnBPress();
         }
     }*/

    /*public void OnDeletePress()
    {
        map_v2.DeleteAllBodiesOnSelected();
    }

    public void OnSpacePress()
    {
        List<Creature_V2> allSelectedCreatures = map_v2.GetAllBodiesOnSelected();
        allSelectedCreatures = allSelectedCreatures.OrderBy(o => o.GetID()).ToList();

        ancestryTree.ResetAllNodes();
        netDrawer.ResetBrain();

        if (allSelectedCreatures.Count > 0)
        {
            ancestryTree.MakeTree(allSelectedCreatures[0]);
            netDrawer.SetBrain(ancestryTree.GetSelectedCreature().GetBrain(), ancestryTree.GetTreeDataList(),ancestryTree.GetSelectedCreature());
        }
    }

    public void OnBPress()
    {
        List<Creature_V2> allSelectedCreatures = map_v2.GetAllBodiesOnSelected();
        allSelectedCreatures = allSelectedCreatures.OrderBy(o => o.GetID()).ToList();
        //ancestryTree.ResetAllNodes();
        //netDrawer.ResetBrain();

        if (allSelectedCreatures.Count > 0)
        {
            if (netDrawer.IsBrainNull() == true)
            {
                ancestryTree.ResetAllNodes();
                netDrawer.ResetBrain();
                ancestryTree.MakeTree(allSelectedCreatures[0]);
                netDrawer.SetBrain(ancestryTree.GetSelectedCreature().GetBrain(), ancestryTree.GetTreeDataList(), ancestryTree.GetSelectedCreature());
            }
            else
            {
                NEATBrain differenceNet = NEATBrain.MakeDifferentialBrain(netDrawer.GetBrain(), allSelectedCreatures[0].GetBrain());
                netDrawer.AddDifferenceNetwork(differenceNet);
            }
        }
    }*/

    /// <summary>
    /// Makes the world.
    /// </summary>
    public void MakeWorld()
    {
        if (textureLoaded == false)
        {
            consultor = NEATConsultor.GetInstance();
            //operationThread.Start();

            brainCalculations = 0;

            creatureList = new List<Creature_V2>();
            speciesList = new List<Species>();
            netDrawer.SetSpeciesList(speciesList);
            for (int i = 0; i < indexColors.Length; i++)
            {
                speciesColor.Add(indexColors[i]);
            }

            for (int i = 0; i < 1000 - indexColors.Length; i++)
            {
                speciesColor.Add(new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f)));
            }

            

            Debug.Log("Making World");
            textureLoaded = true;

            for (int i = 0; i < param.minCreatureCount; i++)
            {
                CreateCreature();
                //operationWorker.CreateOperation();
            }
        }
    }

    /// <summary>
    /// Creates a creature.
    /// </summary>
    public void CreateCreature()
    {
        float energy = 1f;
        float life = 1f;
        float veloForward = 0f;
        float veloAngular = 0f;

        int[] randomTile = map_v2.RandomFloorTile();
        Vector3 bodyPosition = new Vector3(randomTile[0] + 0.5f, randomTile[1] + 0.5f, -2);
        float rotation = 0f;//UnityEngine.Random.Range(0f, 360f);
        Vector3 leftPos = Vector3.zero;
        Vector3 rightPos = Vector3.zero;


        //NEATBrain brain = new NEATBrain(inputNeurons, outputNeurons, maxHiddenNeurons, totalCreaturesCount);
        RNN brain = new RNN(totalCreaturesCount, param.layer, param.speciesSimilarityScore, param.mutationNumber, param.mutationSign,
            param.mutationRandom, param.mutationIncrease, param.mutationDecrease, param.mutationWeakerParentFactor, param.initialVisionAngles, param.minVisionLength);
        brain.Mutate();

        Creature_V2 creature = new Creature_V2(totalCreaturesCount, 0, brain,
            new HSBColor(1f, 1f, 1f), bodyPosition, leftPos, rightPos, 0.5f, rotation, param.worldDeltaTime,
            param.creatureSize / 2f, energy, energy, life, param.minLife, param.lifeDecrease, param.eatDamage, param.veloDamage,
            param.angDamage, param.fightDamage, veloForward, veloAngular, map_v2, this, "WORLD", param.speciesSimilarityScore, threadColors,
            param.minBirthEnergy, param.minFightMaturity, param.birthEnergyCost, param.birthLifeCost, param.fightEnergyCost, param.fightLifeCost, 0,
            param.eggLife, param.eggLife, param.eggFactor, param.eggPrematureBirthDamage, param.waterDamage, param.fastCompute);
        creatureList.Add(creature);
        totalCreaturesCount++;

        if (param.speciesGroupingAlgorithm == 0)
            AddCreatureToSpeciesListWithRandomCheck(creature, null);
        else if (param.speciesGroupingAlgorithm == 1)
            AddCreatureToSpeciesListWithAllheck(creature);
        else
            AddCreatureToSpeciesListWithPercentageCheck(creature);

        brainCalculations += brain.GetCalculations();

        /*if (playSpeed >= param.playSpeedVisual)
        {
            creature.Hide();
        }*/

        renderBacklog.Add(creature);
        totalCreaturesCount++;
    }



    public void CreateCreature(int ID, float timeLived, int generation, string creatureName, string parentNames, Vector2 position, float[][][] weights, BehaviourGenome genome, float energy, float life, float rotation, float veloForward, float veloAngular)
    {
        float initialEnergy = 1f;

        //if (energy >2f)
        //Debug.Log("YES "+ energy);
        //energy = 5f;

        Vector3 bodyPosition = new Vector3(position.x, position.y, -2);
        Vector3 leftPos = Vector3.zero;
        Vector3 rightPos = Vector3.zero;

        //NEATBrain brain = new NEATBrain(inputNeurons, outputNeurons, maxHiddenNeurons, totalCreaturesCount);
        //RNN brain = new RNN(totalCreaturesCount, param.layer, param.speciesSimilarityScore);
        RNN brain = new RNN(ID, param.layer, param.speciesSimilarityScore, weights, creatureName, genome, param.mutationNumber, param.mutationSign,
            param.mutationRandom, param.mutationIncrease, param.mutationDecrease, param.mutationWeakerParentFactor, param.minVisionLength);
        //brain.Mutate();

        //Debug.Log(life+" "+energy);
        Creature_V2 creature = new Creature_V2(ID, generation, brain,
            new HSBColor(1f, 1f, 1f), bodyPosition, leftPos, rightPos, 0.5f, rotation, param.worldDeltaTime,
            param.creatureSize / 2f, initialEnergy, energy, life, param.minLife, param.lifeDecrease, param.eatDamage,
            param.veloDamage, param.angDamage, param.fightDamage, veloForward, veloAngular, map_v2, this, parentNames,
            param.speciesSimilarityScore, threadColors, param.minBirthEnergy, param.minFightMaturity, param.birthEnergyCost, param.birthLifeCost,
            param.fightEnergyCost, param.fightLifeCost, timeLived, param.eggLife, param.eggLife, param.eggFactor, param.eggPrematureBirthDamage,
            param.waterDamage, param.fastCompute);
        creatureList.Add(creature);
        //totalCreaturesCount++;

        if (param.speciesGroupingAlgorithm == 0)
            AddCreatureToSpeciesListWithRandomCheck(creature, null);
        else if (param.speciesGroupingAlgorithm == 1)
            AddCreatureToSpeciesListWithAllheck(creature);
        else
            AddCreatureToSpeciesListWithPercentageCheck(creature);

        brainCalculations += brain.GetCalculations();

        /*if (playSpeed >= param.playSpeedVisual)
        {
            creature.Hide();
        }*/
        renderBacklog.Add(creature);
        /*if (totalCreaturesCount > 10000)
            totalCreaturesCount = 0;
        totalCreatureCountVisual++;*/
    }

    /// <summary>
    /// Creates the creature from one parent.
    /// </summary>
    /// <param name="parent">Parent.</param>
    public void CreateCreature(Creature_V2 parent)
    {
        float energy = 1f;
        float life = 1f;
        float veloForward = 0f;
        float veloAngular = 0f;

        float parentRoationFixed = parent.rotation - 90f;
        Vector3 unit = new Vector3(Mathf.Cos(parentRoationFixed * Mathf.Deg2Rad), Mathf.Sin(parentRoationFixed * Mathf.Deg2Rad), 0f);

        Vector3 bodyPosition = parent.position + (unit * 2f * parent.GetRadius());
        float rotation = parent.rotation;//UnityEngine.Random.Range(0f, 360f);
        bodyPosition.z = -2;
        Vector3 leftPos = Vector3.zero;
        Vector3 rightPos = Vector3.zero;

        //NEATBrain brain = new NEATBrain(parent.GetBrain(), totalCreaturesCount);
        RNN brain = new RNN(parent.GetBrain(), totalCreaturesCount, param.speciesSimilarityScore, param.minVisionLength);
        brain.Mutate();


        Creature_V2 creature = new Creature_V2(totalCreaturesCount, parent.GetGeneration() + 1, brain, new HSBColor(1f, 1f, 1f), bodyPosition, leftPos, rightPos, 0.5f,
            rotation, param.worldDeltaTime, param.creatureSize / 2f, energy, energy, life,
            param.minLife, param.lifeDecrease, param.eatDamage, param.veloDamage, param.angDamage, param.fightDamage, veloForward, veloAngular,
            map_v2, this, parent.GetName(), param.speciesSimilarityScore, threadColors, param.minBirthEnergy, param.minFightMaturity,
            param.birthEnergyCost, param.birthLifeCost, param.fightEnergyCost, param.fightLifeCost, 0, param.eggLife, param.eggLife,
            param.eggFactor, param.eggPrematureBirthDamage, param.waterDamage, param.fastCompute);
        creatureList.Add(creature);
        totalCreaturesCount++;

        parent.AddChildren(creature);

        if (param.speciesGroupingAlgorithm == 0)
            AddCreatureToSpeciesListWithRandomCheck(creature, parent);
        else if (param.speciesGroupingAlgorithm == 1)
            AddCreatureToSpeciesListWithAllheck(creature);
        else
            AddCreatureToSpeciesListWithPercentageCheck(creature);

        brainCalculations += brain.GetCalculations();

        /*if (playSpeed >= param.playSpeedVisual)
        {
            creature.Hide();
        }*/
        renderBacklog.Add(creature);
        totalCreaturesCount++;
    }

    /// <summary>
    /// Creates a creature from two parents.
    /// </summary>
    /// <param name="parent1">Parent1.</param>
    /// <param name="parent2">Parent2.</param>
    public void CreateCreature(Creature_V2 parent1, Creature_V2 parent2)
    {
        /*if(parent1.GetTimeLived()> parent2.GetTimeLived())
             CreateCreature(parent1);
         else
             CreateCreature(parent2);*/

        float energy = 1f;
        float life = 1f;
        float veloForward = 0f;
        float veloAngular = 0f;

        float parentRoationFixed = parent1.rotation - 90f;
        Vector3 unit = new Vector3(Mathf.Cos(parentRoationFixed * Mathf.Deg2Rad), Mathf.Sin(parentRoationFixed * Mathf.Deg2Rad), 0f);
        Vector3 bodyPosition = parent1.position + (unit * 2f * parent1.GetRadius());
        float rotation = parent1.rotation;//UnityEngine.Random.Range(0f, 360f);
        bodyPosition.z = -2;
        Vector3 leftPos = Vector3.zero;
        Vector3 rightPos = Vector3.zero;


        //NEATBrain brain = NEATBrain.Corssover(parent1.GetBrain(), parent2.GetBrain(), parent1.GetTimeLived(), parent2.GetTimeLived(), totalCreaturesCount);
        bool isParent1Strong = false;
        if (parent1.GetEnergy() > parent2.GetEnergy())
            isParent1Strong = true;

        RNN brain = new RNN(isParent1Strong == true ? parent1.GetBrain() : parent2.GetBrain(), totalCreaturesCount, param.speciesSimilarityScore, param.minVisionLength);
        if (isParent1Strong)
            brain.Mutate(parent2.GetBrain().GetWeights(), parent2.GetEnergy() / parent1.GetEnergy());
        else
            brain.Mutate(parent2.GetBrain().GetWeights(), parent1.GetEnergy() / parent2.GetEnergy());


        string parentNames = parent1.GetName() + "@" + parent2.GetName();
        int generationNumber = parent1.GetTimeLived() > parent2.GetTimeLived() ? parent1.GetGeneration() + 1 : parent2.GetGeneration() + 1;

        if (parent1.GetSpecies().GetID() != parent2.GetSpecies().GetID())
        {
            float risk = (Mathf.Pow(RNN.BrainSimilarityScore(parent1.GetBrain(), parent2.GetBrain()), 2f));
            life = 0.1f;
            energy = 0.1f;
            life = life * risk;
            energy = energy * risk;
        }

        Creature_V2 creature = new Creature_V2(totalCreaturesCount, generationNumber, brain, new HSBColor(1f, 1f, 1f), bodyPosition, leftPos, rightPos, 0.5f, rotation,
            param.worldDeltaTime, param.creatureSize / 2f, energy, energy, life, param.minLife, param.lifeDecrease,
            param.eatDamage, param.veloDamage, param.angDamage, param.fightDamage, veloForward, veloAngular, map_v2, this, parentNames,
            param.speciesSimilarityScore, threadColors, param.minBirthEnergy, param.minFightMaturity, param.birthEnergyCost, param.birthLifeCost,
            param.fightEnergyCost, param.fightLifeCost, 0, param.eggLife, param.eggLife, param.eggFactor, param.eggPrematureBirthDamage,
            param.waterDamage, param.fastCompute);
        creatureList.Add(creature);
        totalCreaturesCount++;

        parent1.AddChildren(creature);
        parent2.AddChildren(creature);

        if (param.speciesGroupingAlgorithm == 0)
            AddCreatureToSpeciesListWithRandomCheck(creature, isParent1Strong == true ? parent1 : parent2);
        else if (param.speciesGroupingAlgorithm == 1)
            AddCreatureToSpeciesListWithAllheck(creature);
        else
            AddCreatureToSpeciesListWithPercentageCheck(creature);

        brainCalculations += brain.GetCalculations();

        /*if (playSpeed >= param.playSpeedVisual)
        {
            creature.Hide();
        }*/
        renderBacklog.Add(creature);
        totalCreaturesCount++;
    }


    /// <summary>
    /// Adding creature to species list by matching the creature to atleast one creature in any species. Otherwise, the creature is a new species.
    /// </summary>
    /// <param name="creature">Creature.</param>
    public void AddCreatureToSpeciesListWithRandomCheck(Creature_V2 creature, Creature_V2 parent)
    {
        if (speciesList.Count == 0)
        {
            int randomColorIndex = (int)(random.NextDouble() * speciesColor.Count);//UnityEngine.Random.Range(0, speciesColor.Count);
            Species species = new Species(speciesColor[randomColorIndex], totalSpeciesCount);
            totalSpeciesCount++;
            speciesColor.RemoveAt(randomColorIndex);

            species.Add(creature);
            speciesList.Add(species);
            creature.SetSpecies(species);
        }
        else
        {
            bool found = false;

            if (parent == null)
            {
                for (int i = 0; i < speciesList.Count; i++)
                {
                    List<Creature_V2> population = speciesList[i].GetPopulation();
                    int randomIndex = (int)(random.NextDouble() * population.Count);//UnityEngine.Random.Range(0, population.Count);
                    if (creature.SameSpecies(population[randomIndex]) == true)
                    {
                        speciesList[i].Add(creature);
                        creature.SetSpecies(speciesList[i]);
                        found = true;
                        break;
                    }
                }
            }
            else
            {
                Species parentSpecies = parent.GetSpecies();
                int randomParentIndex = (int)(random.NextDouble() * parentSpecies.GetPopulation().Count);//UnityEngine.Random.Range(0, parentSpecies.GetPopulation().Count);

                if (/*creature.SameSpecies(parent) == true &&*/ creature.SameSpecies(parentSpecies.GetPopulation()[randomParentIndex]) == true)
                {
                    parentSpecies.Add(creature);
                    creature.SetSpecies(parentSpecies);
                    found = true;
                }
                else
                {
                    for (int i = 0; i < speciesList.Count; i++)
                    {
                        List<Creature_V2> population = speciesList[i].GetPopulation();
                        int randomIndex = (int)(random.NextDouble() * population.Count);//UnityEngine.Random.Range(0, population.Count);
                        if (speciesList[i].GetID() != parentSpecies.GetID() && creature.SameSpecies(population[randomIndex]) == true)
                        {
                            speciesList[i].Add(creature);
                            creature.SetSpecies(speciesList[i]);
                            found = true;
                            break;
                        }
                    }
                }
            }



            if (found == false)
            {
                int randomColorIndex = (int)(random.NextDouble() * speciesColor.Count);//UnityEngine.Random.Range(0, speciesColor.Count);
                Species species = new Species(speciesColor[randomColorIndex], totalSpeciesCount);
                totalSpeciesCount++;
                speciesColor.RemoveAt(randomColorIndex);

                species.Add(creature);
                speciesList.Add(species);
                creature.SetSpecies(species);
            }
        }

    }



    /// <summary>
    /// Adding creature to species list by matching the creature fully in atleast one species list. 
    ///Otherwise, the creature is a new species.
    /// </summary>
    /// <param name="creature">Creature.</param>
    public void AddCreatureToSpeciesListWithAllheck(Creature_V2 creature)
    {

        if (speciesList.Count == 0)
        {
            int randomColorIndex = (int)(random.NextDouble() * speciesColor.Count);//UnityEngine.Random.Range(0, speciesColor.Count);
            Species species = new Species(speciesColor[randomColorIndex], totalSpeciesCount);
            totalSpeciesCount++;
            speciesColor.RemoveAt(randomColorIndex);

            species.Add(creature);
            speciesList.Add(species);
            creature.SetSpecies(species);
        }
        else
        {
            bool found = false;
            for (int i = 0; i < speciesList.Count; i++)
            {
                List<Creature_V2> population = speciesList[i].GetPopulation();
                int match = 0;
                for (int j = 0; j < population.Count; j++)
                {
                    if (creature.SameSpecies(population[j]) == true)
                    {
                        match++;
                    }
                    else
                    {
                        break;
                    }
                }

                if (match == population.Count)
                {
                    //population.Add(creature);
                    speciesList[i].Add(creature);
                    creature.SetSpecies(speciesList[i]);
                    found = true;
                    break;
                }
            }

            if (found == false)
            {
                int randomColorIndex = (int)(random.NextDouble() * speciesColor.Count);//UnityEngine.Random.Range(0, speciesColor.Count);
                Species species = new Species(speciesColor[randomColorIndex], totalSpeciesCount);
                totalSpeciesCount++;
                speciesColor.RemoveAt(randomColorIndex);

                species.Add(creature);
                speciesList.Add(species);
                creature.SetSpecies(species);
            }
        }
    }


    /// <summary>
    /// Adding creature to species list by matching a certain percentange of the population
    ///Otherwise, the creature is a new species.
    /// </summary>
    /// <param name="creature">Creature.</param>
    public void AddCreatureToSpeciesListWithPercentageCheck(Creature_V2 creature)
    {

        if (speciesList.Count == 0)
        {
            int randomColorIndex = (int)(random.NextDouble() * speciesColor.Count);//UnityEngine.Random.Range(0, speciesColor.Count);
            Species species = new Species(speciesColor[randomColorIndex], totalSpeciesCount);
            totalSpeciesCount++;
            speciesColor.RemoveAt(randomColorIndex);

            species.Add(creature);
            speciesList.Add(species);
            creature.SetSpecies(species);
        }
        else
        {
            bool found = false;
            for (int i = 0; i < speciesList.Count; i++)
            {
                List<Creature_V2> population = speciesList[i].GetPopulation();
                int match = 0;
                int minMatch = (int)(population.Count * param.speciesGroupingAlgorithm);
                for (int j = 0; j < population.Count; j++)
                {
                    if (creature.SameSpecies(population[j]) == true)
                    {
                        match++;
                        if (match == minMatch)
                        {
                            //population.Add(creature);
                            speciesList[i].Add(creature);
                            creature.SetSpecies(speciesList[i]);
                            found = true;
                            break;
                        }
                    }
                }


            }

            if (found == false)
            {
                int randomColorIndex = (int)(random.NextDouble() * speciesColor.Count);//UnityEngine.Random.Range(0, speciesColor.Count);
                Species species = new Species(speciesColor[randomColorIndex], totalSpeciesCount);
                totalSpeciesCount++;
                speciesColor.RemoveAt(randomColorIndex);

                species.Add(creature);
                speciesList.Add(species);
                creature.SetSpecies(species);
            }
        }
    }
    
    public class CreatureOperationThread
    {

        public List<Operation> operations;
        //ConcurrentQueue<Operation> operations;
        private WolrdManager_V2 manager;
        public bool running = true;
        public CreatureOperationThread(WolrdManager_V2 manager)
        {
            this.manager = manager;
            //operations = new System.Collections.Concurrent.ConcurrentQueue<Operation>();
        }

        public void CreateOperation()
        {
            try {
                //operations.Enqueue(new Operation(Operation.Op.NEW));

            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        public void CreateOperation(Creature_V2 parent)
        {
            //operations.Enqueue(new Operation(Operation.Op.ASEXUAL, parent));
        }

        public void CreateOperation(Creature_V2 parent1, Creature_V2 parent2)
        {
            //operations.Enqueue(new Operation(Operation.Op.SEXUAL, parent1, parent2));
        }

        public void CreateOperation(int ID, float timeLived, int generation, string creatureName, string parentNames, Vector2 position, float[][][] weights, BehaviourGenome genome, float energy, float life, float rotation, float veloForward, float veloAngular)
        {
            //operations.Enqueue(new Operation(Operation.Op.LOAD, ID, timeLived, generation, creatureName, parentNames, position, weights, genome, energy, life, rotation, veloForward, veloAngular));
        }


        public void Worker()
        {
            //RNN brain = new RNN(0, new int[] { 25, 10, 10, 9 }, 0, 1000, 0.2f, 0.2f, 1, 1, 1, new float[] {0,0,0,0});
            int k = 0;
            while (running)
            {
                
                /*if (operations.IsEmpty == false) //operations.Count > 0)
                {
                    while (operations.IsEmpty == false)
                    {
                        Operation operation = null;
                        operations.TryDequeue(out operation);
                        if (operation != null)
                        {
                            if (operation.type == Operation.Op.ASEXUAL)
                            {
                                manager.CreateCreature(operation.parent1);
                            }
                            else if (operation.type == Operation.Op.SEXUAL)
                            {
                                manager.CreateCreature(operation.parent1, operation.parent2);
                            }
                            else if (operation.type == Operation.Op.NEW)
                            {
                                manager.CreateCreature();
                                Debug.Log("SPAWNNING NEW!!");
                            }
                            else
                            {
                                manager.CreateCreature(); //change to match load!
                            }
                        }
                    }*/
                    
                    /*Debug.Log(operations.Count);
                    for (int i = 0; i < operations.Count; i++)
                    {

                        Operation operation = operations[i];

                        if (operation.type == Operation.Op.ASEXUAL)
                        {
                            manager.CreateCreature(operation.parent1);
                        }
                        else if (operation.type == Operation.Op.SEXUAL)
                        {
                            manager.CreateCreature(operation.parent1, operation.parent2);
                        }
                        else if (operation.type == Operation.Op.NEW)
                        {
                            manager.CreateCreature();
                            Debug.Log("SPAWNNING NEW!!");
                        }
                        else
                        {
                            manager.CreateCreature(); //change to match load!
                        }

                        operations.RemoveAt(i--);
                    }
                }*/
                
            }

        }
    };

    void OnDestroy()
    {
        //#if UNITY_EDITOR
            Debug.Log("Closing Thread");
            if (operationThread!=null && operationThread.IsAlive)
            {
                try
                {
                    operationWorker.running = false;
                    operationThread.Abort();
                    Debug.Log(operationThread.IsAlive); //true (must be false)
                }
                catch (Exception error)
                {
                    Debug.Log(error);
                }
            }
        //#endif
    }

    void OnApplicationQuit()
    {
        Debug.Log("Closing Thread");
        if (operationThread != null && operationThread.IsAlive)
        {
            try
            {
                operationWorker.running = false;
                operationThread.Abort();
                Debug.Log(operationThread.IsAlive); //true (must be false)
            }
            catch (Exception error)
            {
                Debug.Log(error);
            }
        }
    }

    public void min_creature_count(float value)
    {
        param.minCreatureCount = (int)value;
        param.sliderText["min_creature_count"].text = (int)value + "";
    }

    public void climate_(float value)
    {
        param.climate = value;
        param.sliderText["climate_"].text = value + "";
        map_v2.SetClimate(param.climate);
    }

    public void food_nutrition_power(float value)
    {
        param.nutritionPower = (int)value;
        param.sliderText["food_nutrition_power"].text = value + "";
        map_v2.SetNutritionPower(param.nutritionPower);
    }

    public void food_nutrition_amplitude(float value)
    {
        param.nutritionAmplitude = value;
        param.sliderText["food_nutrition_amplitude"].text = value + "";
        map_v2.SetNutritionAmplitude(param.nutritionAmplitude);
    }

    public void world_delta_time(float value)
    {
        param.worldDeltaTime = value;
        param.sliderText["world_delta_time"].text = value + "";
        map_v2.SetWorldDeltaTime(param.worldDeltaTime);

        for (int i = 0; i < creatureList.Count; i++)
        {
            creatureList[i].delta_time(value);
        }
    }

    public void min_life(float value)
    {
        param.minLife = value;
        param.sliderText["min_life"].text = value + "";

        for (int i = 0; i < creatureList.Count; i++)
        {
            creatureList[i].min_life(value);
        }
    }

    public void life_decrease(float value)
    {
        param.lifeDecrease = value;
        param.sliderText["life_decrease"].text = value + "";

        for (int i = 0; i < creatureList.Count; i++)
        {
            creatureList[i].life_decrease(value);
        }
    }

    public void eat_damage(float value)
    {
        param.eatDamage = value;
        param.sliderText["eat_damage"].text = value + "";

        for (int i = 0; i < creatureList.Count; i++)
        {
            creatureList[i].eat_damage(value);
        }
    }

    public void velo_damage(float value)
    {
        param.veloDamage = value;
        param.sliderText["velo_damage"].text = value + "";

        for (int i = 0; i < creatureList.Count; i++)
        {
            creatureList[i].velo_damage(value);
        }
    }

    public void ang_damage(float value)
    {
        param.angDamage = value;
        param.sliderText["ang_damage"].text = value + "";

        for (int i = 0; i < creatureList.Count; i++)
        {
            creatureList[i].ang_damage(value);
        }
    }

    public void fight_damage(float value)
    {
        param.fightDamage = value;
        param.sliderText["fight_damage"].text = value + "";

        for (int i = 0; i < creatureList.Count; i++)
        {
            creatureList[i].fight_damage(value);
        }
    }

    public void min_energy_to_birth(float value)
    {
        param.minBirthEnergy = value;
        param.sliderText["min_energy_to_birth"].text = value + "";

        for (int i = 0; i < creatureList.Count; i++)
        {
            creatureList[i].min_energy_to_birth(value);
        }
    }

    public void min_fight_maturity(float value)
    {
        param.minFightMaturity = value;
        param.sliderText["min_fight_maturity"].text = value + "";

        for (int i = 0; i < creatureList.Count; i++)
        {
            creatureList[i].min_fight_maturity(value);
        }
    }

    public void birth_energy_cost(float value)
    {
        param.birthEnergyCost = value;
        param.sliderText["birth_energy_cost"].text = value + "";

        for (int i = 0; i < creatureList.Count; i++)
        {
            creatureList[i].birth_energy_cost(value);
        }
    }

    public void birth_life_cost(float value)
    {
        param.birthLifeCost = value;
        param.sliderText["birth_life_cost"].text = value + "";

        for (int i = 0; i < creatureList.Count; i++)
        {
            creatureList[i].birth_life_cost(value);
        }
    }

    public void fight_energy_cost(float value)
    {
        param.fightEnergyCost = value;
        param.sliderText["fight_energy_cost"].text = value + "";

        for (int i = 0; i < creatureList.Count; i++)
        {
            creatureList[i].fight_energy_cost(value);
        }
    }


    public void fight_life_cost(float value)
    {
        param.fightLifeCost = value;
        param.sliderText["fight_life_cost"].text = value + "";

        for (int i = 0; i < creatureList.Count; i++)
        {
            creatureList[i].fight_life_cost(value);
        }
    }

    public void egg_life(float value)
    {
        param.eggLife = value;
        param.sliderText["egg_life"].text = value + "";
    }

    public void egg_factor(float value)
    {
        param.eggFactor = value;
        param.sliderText["egg_timer"].text = value + "";
    }

    public void egg_premature_birth_damage(float value)
    {
        param.eggPrematureBirthDamage = value;
        param.sliderText["egg_premature_birth_damage"].text = value + "";
    }

    public void water_damage(float value)
    {
        param.waterDamage = value;
        param.sliderText["water_damage"].text = value + "";

        for (int i = 0; i < creatureList.Count; i++)
        {
            creatureList[i].water_damage(value);
        }
    }

    public void creature_size(float value)
    {
        param.creatureSize = value;
        param.sliderText["creature_size"].text = value + "";

        for (int i = 0; i < creatureList.Count; i++)
        {
            creatureList[i].creature_size(value);
        }
    }

    public void mutation_number(float value)
    {
        param.mutationNumber = value;
        param.sliderText["mutation_number"].text = value + "";

        for (int i = 0; i < creatureList.Count; i++)
        {
            creatureList[i].mutation_number(value);
        }
    }

    public void mutation_weaker_parent_factor(float value)
    {
        param.mutationWeakerParentFactor = value;
        param.sliderText["mutation_weaker_parent_factor"].text = value + "";

        for (int i = 0; i < creatureList.Count; i++)
        {
            creatureList[i].mutation_weaker_parent_factor(value);
        }
    }

    public void mutation_sign(float value)
    {
        param.mutationSign = value;
        param.sliderText["mutation_sign"].text = value + "";

        for (int i = 0; i < creatureList.Count; i++)
        {
            creatureList[i].mutation_sign(value);
        }
    }

    public void mutation_random(float value)
    {
        param.mutationRandom = value;
        param.sliderText["mutation_random"].text = value + "";

        for (int i = 0; i < creatureList.Count; i++)
        {
            creatureList[i].mutation_random(value);
        }
    }

    public void mutation_increase(float value)
    {
        param.mutationIncrease = value;
        param.sliderText["mutation_increase"].text = value + "";

        for (int i = 0; i < creatureList.Count; i++)
        {
            creatureList[i].mutation_increase(value);
        }
    }

    public void mutation_decrease(float value)
    {
        param.mutationDecrease = value;
        param.sliderText["mutation_decrease"].text = value + "";

        for (int i = 0; i < creatureList.Count; i++)
        {
            creatureList[i].mutation_decrease(value);
        }
    }

    public void species_similarity_score(float value)
    {
        param.speciesSimilarityScore = value;
        param.sliderText["species_similarity_score"].text = value + "";

        for (int i = 0; i < creatureList.Count; i++)
        {
            creatureList[i].species_similarity_score(value);
        }
    }

    public void species_grouping_algorithm(float value)
    {
        param.speciesGroupingAlgorithm = value;
        param.sliderText["species_grouping_algorithm"].text = value + "";

    }

    public void min_vision_length(float value)
    {
        param.minVisionLength = value;
        param.sliderText["min_vision_length"].text = value + "";
    }


    /// <summary>
    /// Contains a woker method which a thread will run and compute creatu      re feedforward update for 1 tick. 
    /// </summary>
    public class NetworkThread
    {
        int threadID;
        int min, max;
        List<Creature_V2> creatureList;

        public NetworkThread(int threadID) { this.threadID = threadID; }

        /// <summary>
        /// NOT USED ANYMORE!
        /// Creating new NetworkThread along with Thread class every tick causes a massive memory leak. 
        /// </summary>
        /// <param name="creatureList"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public NetworkThread(List<Creature_V2> creatureList, int min, int max) { this.creatureList = creatureList; this.min = min; this.max = max; }

        /// <summary>
        /// Set work details for the thread (previously set through constructor)
        /// </summary>
        /// <param name="creatureList"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public void Set(List<Creature_V2> creatureList, int min, int max)
        {
            this.creatureList = creatureList;
            this.min = min;
            this.max = max;
        }

        /// <summary>
        /// Work a thread must do for 1 tick.
        /// </summary>
        public void Worker()
        {
            for (int i = min; i < max; i++)
            {
                creatureList[i].UpdateCreature(threadID);
            }
            sem.Release(); //release semaphore after work is done.
        }
    };

    List<Creature_V2> renderBacklog = new List<Creature_V2>();
    public void WorkRenderBacklog(bool render)
    {

        if (render == true)
        {
            for (int i = 0; i < renderBacklog.Count; i++)
            {
                if (renderBacklog[i].IsAlive() == true)
                {
                    GameObject creatureGameObject = Instantiate(creaturePrefab, renderBacklog[i].position, creaturePrefab.transform.rotation) as GameObject;
                    GameObject leftLineGameObject = Instantiate(linePrefab) as GameObject;
                    GameObject rightLineGameObject = Instantiate(linePrefab) as GameObject;
                    leftLineGameObject.transform.parent = creatureGameObject.transform;
                    rightLineGameObject.transform.parent = creatureGameObject.transform;

                    LineRenderer leftLine = leftLineGameObject.GetComponent<LineRenderer>();
                    LineRenderer rightLine = rightLineGameObject.GetComponent<LineRenderer>();
                    leftLine.SetWidth(0.02f, 0.02f);
                    rightLine.SetWidth(0.02f, 0.02f);

                    GameObject spikeLineGameObject = Instantiate(linePrefab) as GameObject;
                    spikeLineGameObject.transform.parent = creatureGameObject.transform;
                    LineRenderer spikeLine = spikeLineGameObject.GetComponent<LineRenderer>();
                    spikeLine.SetWidth(0.02f, 0.02f);

                    LineRenderer[] lineSensor = new LineRenderer[param.numberOfEyeSensors];
                    for (int j = 0; j < lineSensor.Length; j++)
                    {
                        GameObject newLine = Instantiate(linePrefab) as GameObject;
                        newLine.transform.parent = creatureGameObject.transform;
                        lineSensor[j] = newLine.GetComponent<LineRenderer>();
                        lineSensor[j].SetWidth(0.02f, 0.02f);
                    }
                    creatureGameObject.transform.GetChild(1).GetComponent<TextMesh>().text = renderBacklog[i].GetName();
                    creatureGameObject.transform.localScale = new Vector3(param.creatureSize, param.creatureSize, 1f);

                    renderBacklog[i].SetRenderComponents(!hide, creatureGameObject.transform, leftLine, rightLine, lineSensor, spikeLine);

                }
                renderBacklog.RemoveAt(i--);
            }
        }
        else
        {
            for (int i = 0; i < renderBacklog.Count; i++)
            {
                if (renderBacklog[i].IsAlive() == false)
                {
                    renderBacklog.RemoveAt(i--);
                }
            }
        }
    }
}