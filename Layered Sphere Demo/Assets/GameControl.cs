using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.SearchService;
using TMPro;
using System.Diagnostics;
using System.Resources;
using Unity.VisualScripting;
using UnityEngine.EventSystems;

public class GameControl : MonoBehaviour
{
    // The game control object is responsible for the primary game loop and state changes.
    // All vars here are top down essential. May be useful to have statics but I'll try to avoid
    Transform mapHolder;
    System.Random rando;
    LayeredSphereMesh mesh;
    PointFeeder feeder;
    World world; // 2/25 Procedural data from a single seed number goes here
    GameData gameData; // 2/25 Data that can not be procedurally recreated goes here
    [SerializeField]
    EditorSettings editorSettings;
    [SerializeField]
    ColorSettings colorSettings;
    //ResourceBuilder resourceBuilder;
    //ResourceNode[,] resourceNodes; // doesn't feel right but for now it's here
    GameInput input;
    Camera gameCamera;
    bool cameraMoved;
    bool updateTrophicIndices = false;

    // Game Windows
    Image window_nodeData;
    Image window_resourceLookup;
    TMP_InputField field_resourceLookup;
    TMP_Dropdown dropdown_resourceLookup;

    // Window states
    bool windowFlag_nodeData;
    bool windowFlag_resourceLookup;

    // Contexts
    bool context_mainMenu;
    bool context_worldView;
    bool context_window;
    bool context_regionShading;
    bool context_heatmapShading;
    bool context_resourceShading;
    bool context_overlapShading;

    bool state_loading;
    bool state_BuildingWorld;
    int state_BuildStep;
    bool state_RenderReady;
    //bool state_PopulateResources;
    bool state_ResourcesReady;
    bool state_ControlHold;

    int heatmapType = 0;
    int heatmapTypeLimit = 3;
    int overlapType = 0;
    int overlapTypeLimit = 5;

    int rb_step = 0;
    int rb_subStep = 0;
    int rb_loops = 0;

    const float ROTATIONSPEEDFLAT = 1f;


    // Start is called before the first frame update
    void Start()
    {
        // start loads the main menu and cache
        gameData = new GameData(new GameSettings(editorSettings));
        CleanUp();
        rando = new System.Random();
        SetFlags();
        FindWindows();
        gameCamera = Camera.main;
        cameraMoved = false;
        feeder = new PointFeeder();
        //resourceBuilder = new ResourceBuilder();
        //resourceBuilder.ProcessTrophicIndices();
        // Key Interface object would start here
        input = new GameInput(Camera.main);
        //Persistence persistence = new Persistence("/gamedata.json");
        // Canvas can be retrieved here
    }

    // Update is called once per frame
    void Update()
    {
        // Update runs animation, interprets player commands, and changes the game's state
        // minimal blocking code ideally
        if (state_BuildingWorld)
        {
            StepBuildWorld();
        }
        //if (state_PopulateResources)
        //{
        //    StepPopulateResources();
        //}
        if ((state_RenderReady) && (state_loading))
        {
            state_ResourcesReady = true;
            state_loading = false;
        }
        if (state_RenderReady)
        {
            if (feeder.HasUpdates())
                mesh.DeltaUpdateLocalMesh(feeder, world.Get_grid(), gameData);
        }

        KeyCaller();

    }

    private void SetFlags()
    {
        windowFlag_nodeData = false;
        windowFlag_resourceLookup = false;

        context_mainMenu = true;
        context_worldView = false;
        context_window = false;
        context_regionShading = false;
        context_heatmapShading = false;
        context_resourceShading = false;

        state_loading = false;
        state_BuildingWorld = false;
        state_BuildStep = 0;
        state_RenderReady = false;
        //state_PopulateResources = false;
    }

    private void FindWindows()
    {
        GameObject canvas = GameObject.Find("Canvas");
        Image[] windows = canvas.GetComponentsInChildren<Image>(true);
        int pos = 0;
        while (pos < windows.Length)
        {
            string name = windows[pos].gameObject.name;
            bool found = false;
            switch (name)
            {
                case "NodeWindow":
                    window_nodeData = windows[pos];
                    found = true;
                    break;
                //case "ResourceLookupWindow":
                //    window_resourceLookup = windows[pos];
                //    found = true;
                //    break;
                //case "ResourceLookupField":
                //    field_resourceLookup = windows[pos].transform.GetComponent<TMP_InputField>();
                //    dropdown_resourceLookup = windows[pos].transform.GetComponent<TMP_Dropdown>();
                //    break;
                default:
                    break;
            }
            if (!found)
                UnityEngine.Debug.Log(windows[pos].gameObject.name + " had no corresponding game window.");

            pos++;
        }
    }

    // -----------------------------------------------------------------------------------------

    private void CleanUp()
    {
        if (transform.Find(GameSettings.HOLDERNAME))
        {
            DestroyImmediate(transform.Find(GameSettings.HOLDERNAME).gameObject);
        }
        mapHolder = new GameObject(GameSettings.HOLDERNAME).transform;
        mapHolder.parent = this.transform;
    }

    // -----------------------------------------------------------------------------------------

    private void KeyCaller()
    {
        if (!state_ControlHold)
        {
            if (input.Check())
            {
                if (input.flag_escape)
                {
                    if (!context_mainMenu)
                    {
                        if (windowFlag_nodeData)
                        {
                            window_nodeData.gameObject.SetActive(false);
                            windowFlag_nodeData = false;
                            context_window = false;
                            context_worldView = true;
                        }
                        if (windowFlag_resourceLookup)
                        {
                            window_resourceLookup.gameObject.SetActive(false);
                            field_resourceLookup.gameObject.SetActive(false);
                            windowFlag_resourceLookup = false;
                            context_window = false;
                            context_worldView = true;
                        }
                    }

                }
                if (input.flag_enter)
                {
                    if (context_mainMenu)
                    {
                        // going to try to break these up into steps to be completed during frame updates
                        //CleanUp();
                        //BuildWorld();
                        //mesh = new LayeredSphereMesh(worldSettings, colorSettings);
                        //mesh.PrepSmallMesh(world); // activeMap available after this
                        //mesh.Get_builtWorld().transform.SetParent(mapHolder);
                        //float xLength = world.Get_grid().Get_Nodes().GetLength(0);
                        //float yLength = world.Get_grid().Get_Nodes().GetLength(1);
                        //Vector2 camPos = new Vector2(gameCamera.transform.position.x, gameCamera.transform.position.y);
                        //Vector2 feedStart = camPos * (worldSettings.radius + 5f);
                        //Vector2 gridOffset = new Vector2(xLength / 2f, yLength / 2f);
                        //feeder = new PointFeeder(new Point2D(feedStart + gridOffset), 18, new PointValidator(world.Get_grid().Get_Nodes()), mesh.Get_activeMap());

                        //resourceBuilder.PopulateInitialResources(world.Get_grid(), rando);
                        //mesh.InjectLocalUVSFromRegions(world.Get_rivers().Get_drainages().Get_rawCells(), world.Get_grid());

                        state_BuildingWorld = true;
                        context_mainMenu = false;
                        context_worldView = true;
                        state_ControlHold = true;
                    }

                    //if ((state_RenderReady) && context_worldView && (!state_ResourcesReady))
                    //{
                    //    state_PopulateResources = true;
                    //    state_ControlHold = true;
                    //}

                    //if (context_window)
                    //{
                    //    if (windowFlag_resourceLookup)
                    //    {
                    //        if (field_resourceLookup != null)
                    //        {
                    //            string id = field_resourceLookup.text;
                    //            //Debug.Log("Received request for lookup of resource #" + id);
                    //            Resource testFind = gameData.GetResource(id);
                    //            if (testFind != null)
                    //            {
                    //                //UnityEngine.Debug.Log(id + " is " + testFind.Get_label());
                    //                if (testFind.GetType() == typeof(Plant))
                    //                {
                    //                    Plant plantSource = (Plant)testFind;
                    //                    //UnityEngine.Debug.Log(plantSource.Get_label() + " has " + plantSource.Get_variants().Count + " variants");
                    //                    int pos = 0;
                    //                    List<IterableBinaryBubbleCell> cells = new List<IterableBinaryBubbleCell>();
                    //                    while (pos < plantSource.data.variants.Count)
                    //                    {
                    //                        cells.Add(plantSource.data.variants[pos].Get_cell());
                    //                        UnityEngine.Debug.Log(plantSource.label + " variant #" + pos + " has " + plantSource.data.variants[pos].Get_cell().Get_interior().Count + " nodes");
                    //                        pos++;
                    //                    }
                    //                }
                    //                if (testFind.GetType() == typeof(Animal))
                    //                {
                    //                    Animal animalSource = (Animal)testFind;
                    //                    int pos = 0;
                    //                    List<IterableBinaryBubbleCell> cells = new List<IterableBinaryBubbleCell>();
                    //                    while (pos < animalSource.data.variants.Count)
                    //                    {
                    //                        cells.Add(animalSource.data.variants[pos].Get_cell());
                    //                        UnityEngine.Debug.Log(animalSource.label + " variant #" + pos + " has " + animalSource.data.variants[pos].Get_cell().Get_interior().Count + " nodes");
                    //                        pos++;
                    //                    }
                    //                }
                    //                mesh.InjectResourceUVS(feeder, testFind, world.Get_grid());
                    //                context_resourceShading = true;
                    //                mesh.Get_colorGenerator().ToggleRegionalShading(true);
                    //            }
                    //        }
                    //        else
                    //            UnityEngine.Debug.Log("Field object not found.");
                    //    }
                    //}

                }
                if (input.flag_regionShading)
                {
                    if (context_regionShading)
                        context_regionShading = false;
                    else
                    {
                        context_resourceShading = false;
                        context_heatmapShading = false;
                        context_regionShading = true;
                        mesh.InjectLocalUVSFromRegions(world.Get_rivers().Get_drainages().Get_rawCells(), world.Get_grid());
                    }
                    mesh.Get_colorGenerator().ToggleRegionalShading(context_regionShading);
                }
                if (input.flag_heatmapShading)
                {
                    if (context_heatmapShading)
                    {
                        heatmapType++;
                        if (heatmapType >= heatmapTypeLimit)
                        {
                            heatmapType = 0;
                            context_heatmapShading = false;
                            mesh.Get_colorGenerator().ToggleRegionalShading(context_heatmapShading);
                        }
                    }
                    else
                    {
                        context_resourceShading = false;
                        context_heatmapShading = true;
                        context_regionShading = false;
                        if (!context_regionShading)
                            mesh.Get_colorGenerator().ToggleRegionalShading(context_heatmapShading);
                    }

                    if ((context_heatmapShading)/* && (heatmapType != 0)*/)
                        switch (heatmapType)
                        {
                            case 0:
                                UnityEngine.Debug.Log("Rendering Mean High Temperature");
                                break;
                            case 1:
                                UnityEngine.Debug.Log("Rendering Precipitation");
                                break;
                            case 2:
                                UnityEngine.Debug.Log("Rendering Ocean Surface Temperature");
                                break;
                        }
                    mesh.InjectHeatmapUVS(feeder, heatmapType, world.Get_grid());
                }
                if (input.flag_overlapShading)
                {
                    if (context_overlapShading)
                    {
                        overlapType++;
                        if (overlapType >= overlapTypeLimit)
                        {
                            overlapType = 0;
                            context_overlapShading = false;
                            mesh.Get_colorGenerator().ToggleRegionalShading(context_overlapShading);
                        }
                    }
                    else
                    {
                        context_resourceShading = false;
                        context_overlapShading = true;
                        context_regionShading = false;
                        if (!context_regionShading)
                            mesh.Get_colorGenerator().ToggleRegionalShading(context_overlapShading);
                    }

                    //if (context_overlapShading)
                    //{
                    //    Resource[] resourceSet = null;
                    //    switch (overlapType)
                    //    {
                    //        default:
                    //        case 0:
                    //            UnityEngine.Debug.Log("Rendering Plant Diversity");
                    //            resourceSet = gameData.plants;
                    //            break;
                    //        case 1:
                    //            UnityEngine.Debug.Log("Rendering Insect Diversity");
                    //            resourceSet = gameData.insects;
                    //            //resourceSet = LinearizeTrophics(gameData.trophicInsects);
                    //            break;
                    //        case 2:
                    //            UnityEngine.Debug.Log("Rendering Marine Fish Diversity");
                    //            resourceSet = gameData.marineFish;
                    //            //resourceSet = LinearizeTrophics(gameData.trophicMarineFish);
                    //            break;
                    //        case 3:
                    //            UnityEngine.Debug.Log("Rendering Animal Diversity");
                    //            resourceSet = gameData.animals;
                    //            //resourceSet = LinearizeTrophics(gameData.trophicAnimals);
                    //            break;
                    //        case 4:
                    //            UnityEngine.Debug.Log("Rendering Mineral Diversity");
                    //            resourceSet = gameData.minerals;
                    //            break;


                    //    }
                    //    if (resourceSet != null)
                    //    {
                    //        //List<IterableBinaryBubbleCell> cells = CollectCells(resourceSet);
                    //        //mesh.InjectOverlapUVS(feeder, overlapType, world.Get_grid(), cells);
                    //    }
                    //}
                }
                if (input.flag_zoomOut)
                {
                    feeder.IncreaseRadius(mesh.Get_activeMap()); // zooming out increases the radius to render
                }
                if (input.flag_zoomIn)
                {
                    feeder.DecreaseRadius(mesh.Get_activeMap());
                }

                if (input.flag_nodeData)
                {
                    if (context_worldView)
                    {
                        switch (heatmapType)
                        {
                            case 0:
                                if (!windowFlag_nodeData)
                                {
                                    window_nodeData.gameObject.SetActive(true);
                                    windowFlag_nodeData = true;
                                }
                                TMP_Text temperatureText = window_nodeData.GetComponentInChildren<TMP_Text>();
                                float temperature = world.Get_grid().Get_node(input.nodeData.location).Get_meanHighTemperature();
                                temperatureText.SetText(input.nodeData.location.GetStringForm() + ":" + input.nodeData.subNode.ToString() + " " + temperature);

                                break;
                            case 1:
                                if (!windowFlag_nodeData)
                                {
                                    window_nodeData.gameObject.SetActive(true);
                                    windowFlag_nodeData = true;
                                }
                                TMP_Text precipText = window_nodeData.GetComponentInChildren<TMP_Text>();
                                float precip = world.Get_grid().Get_node(input.nodeData.location).Get_precipitation();
                                precipText.SetText(input.nodeData.location.GetStringForm() + ":" + input.nodeData.subNode.ToString() + " " + precip);
                                break;
                            case 2:
                                if (!windowFlag_nodeData)
                                {
                                    window_nodeData.gameObject.SetActive(true);
                                    windowFlag_nodeData = true;
                                }
                                TMP_Text oceanTempText = window_nodeData.GetComponentInChildren<TMP_Text>();
                                float oceanTemp = world.Get_grid().Get_node(input.nodeData.location).Get_oceanSurfaceTemperature();
                                oceanTempText.SetText(input.nodeData.location.GetStringForm() + ":" + input.nodeData.subNode.ToString() + " " + oceanTemp);
                                break;
                        }

                        context_window = true;
                        context_worldView = false;
                    }
                }

                if (input.flag_resourceLookup)
                {
                    if (context_worldView)
                    {
                        if (!windowFlag_resourceLookup)
                        {
                            window_resourceLookup.gameObject.SetActive(true);
                            field_resourceLookup.gameObject.SetActive(true);
                            windowFlag_resourceLookup = true;
                        }
                        context_window = true;
                        context_worldView = false;
                    }


                }

                if (input.flag_saveToFile)
                {
                    //if (context_worldView)
                    //{
                    //    CSVHandler handler = new CSVHandler();
                    //    handler.WriteCSV(resourceBuilder.GetTerrestrialResources(), "TestSave");
                    //}

                }

                //if (input.flag_loadFile)
                //{
                //    Persistence persistence = new Persistence();
                //    gameData = persistence.LoadData();
                //    gameData.LoadResourceData();
                //    state_loading = true;
                //    state_BuildingWorld = true;
                //    context_mainMenu = false;
                //    context_worldView = true;
                //    state_ControlHold = true;
                //}

                //if (input.flag_debugResources)
                //{
                //    // perform tests on the resource list to validate the integrity
                //    // checks needed are:
                //    // duplicates (X)
                //    // that each resource has a detail
                //    // that the detail label matches the label for a given ID (X)

                //    // these checks may be added to the CSV handler
                //    gameData.PerformDuplicatesCheck();
                //    gameData.UpdateTrophicIndices(gameData, false, true, true);
                //}

                HandleCameraMovement();
            }
        }

    }

    //private Resource[] LinearizeTrophics(Resource[][] in_trophicSet)
    //{
    //    List<Resource> toRet = new List<Resource>();
    //    int pos = 0;
    //    while (pos < in_trophicSet.Length)
    //    {
    //        int subPos = 0;
    //        while (subPos < in_trophicSet[pos].Length)
    //        {
    //            toRet.Add(in_trophicSet[pos][subPos]);
    //            subPos++;
    //        }
    //        pos++;
    //    }
    //    return toRet.ToArray();
    //}

    //private List<IterableBinaryBubbleCell> CollectCells(TerrestrialResource[] in_resources)
    //{
    //    List<IterableBinaryBubbleCell> toRet = new List<IterableBinaryBubbleCell>();
    //    int pos = 0;
    //    while (pos < in_resources.Length)
    //    {
    //        int subPos = 0;
    //        List<ResourceVariant> variants = in_resources[pos]
    //        while (subPos < variants.Count)
    //        {
    //            toRet.Add(variants[subPos].Get_cell());
    //            subPos++;
    //        }
    //        pos++;
    //    }
    //    return toRet;
    //}

    private void HandleCameraMovement()
    {
        if ((input.flag_rightClick) || (input.flag_up) || (input.flag_down) || (input.flag_left) || (input.flag_right))
        {
            cameraMoved = true;
            // Right click and drag
            if (input.flag_rightClick)
            {
                Vector2 displacement = input.clickDirection - input.clickOrigin;
                displacement = displacement.normalized;
                gameCamera.transform.Translate(displacement * Time.deltaTime * (Vector2.Distance(input.clickOrigin, input.clickDirection) / 1000));
                gameCamera.transform.position = gameCamera.transform.position.normalized * (gameData.settings.radius + 5);
                gameCamera.transform.LookAt(Vector3.zero);
                //Debug.Log(Vector2.Distance(input.clickOrigin, input.clickDirection));
            }
            if (input.flag_rightClickUp)
                mesh.CleanupMesh(feeder);

            // WASD 
            if (input.flag_up)
            {
                gameCamera.transform.Translate(Vector3.up * Time.deltaTime * ROTATIONSPEEDFLAT);
                gameCamera.transform.position = gameCamera.transform.position.normalized * (gameData.settings.radius + 5);
                gameCamera.transform.LookAt(Vector3.zero);
            }
            if (input.flag_down)
            {
                gameCamera.transform.Translate(-Vector3.up * Time.deltaTime * ROTATIONSPEEDFLAT);
                gameCamera.transform.position = gameCamera.transform.position.normalized * (gameData.settings.radius + 5);
                gameCamera.transform.LookAt(Vector3.zero);
            }
            if (input.flag_left)
            {
                gameCamera.transform.Translate(-Vector3.right * Time.deltaTime * ROTATIONSPEEDFLAT);
                gameCamera.transform.position = gameCamera.transform.position.normalized * (gameData.settings.radius + 5);
                gameCamera.transform.LookAt(Vector3.zero);
            }
            if (input.flag_right)
            {
                gameCamera.transform.Translate(Vector3.right * Time.deltaTime * ROTATIONSPEEDFLAT);
                gameCamera.transform.position = gameCamera.transform.position.normalized * (gameData.settings.radius + 5);
                gameCamera.transform.LookAt(Vector3.zero);
            }

            UpdateLocationFeeder();
        }
        else
            cameraMoved = false;
    }

    private void UpdateLocationFeeder()
    {
        Vector3 location = gameCamera.transform.position.normalized * (gameData.settings.radius + 5);
        Ray locationRay = new Ray(location, -location);
        RaycastHit hit;
        NodeData nodeData;
        if (Physics.Raycast(locationRay, out hit))
        {
            if (hit.collider != null)
            {
                nodeData = hit.transform.GetComponent<NodeData>();
                feeder.UpdateCenter(nodeData.location, mesh.Get_activeMap());
                //mesh.UpdateLocalMesh(feeder, world.Get_grid());
            }
        }
    }


    //private void BuildWorld()
    //{
    //    if (gameData.settings.randomize)
    //        gameData.settings.worldSeed = rando.Next(int.MinValue, int.MaxValue);
    //    rando = new System.Random(gameData.settings.worldSeed);
    //    world = new World(rando, gameData);
    //    //OutAnalysis();
    //}

    private void OutAnalysis()
    {
        bool[] analysis = world.Get_analyzer().Get_analysis();
        int seed = gameData.settings.worldSeed;
        string message = "World " + seed + " is ";
        if (analysis[0])
        {
            BiogeographicZone[] zones = world.Get_biogeologicZones().Get_solids(world.Get_grid());
            float ratio = zones[1].Get_surfaceArea() / zones[0].Get_surfaceArea();
            message += "New/Old World (" + ratio + "), ";
        }
        if (analysis[1])
            message += "Sisters, ";
        if (analysis[2])
            message += "Continents, ";
        if (analysis[3])
            message += "Archipelago, ";
        if (analysis[4])
            message += "Pangaea, ";
        if (analysis[5])
            message += "Atolls, ";
        if (analysis[6])
            message += "No Type, ";

        UnityEngine.Debug.Log(message);
    }

    private void StepBuildWorld()
    {
        switch (state_BuildStep)
        {
            default:
            case 0:
                UnityEngine.Debug.Log("Control Hold enabled. Building World");
                if (!state_loading)
                    gameData = new GameData(new GameSettings(editorSettings));
                CleanUp();
                break;
            case 1:
                world = new World();
                if (!state_loading)
                {
                    if (gameData.settings.randomize)
                    {
                        gameData.settings.worldSeed = rando.Next(int.MinValue, int.MaxValue);

                    }
                }
                rando = new System.Random(gameData.settings.worldSeed);
                world.BuildStep_Initialize(rando, gameData);
                break;
            case 2:
                world.BuildStep_DiamondSquare(rando, gameData);
                break;
            case 3:
                world.BuildStep_Smoothing(gameData);
                break;
            case 4:
                world.BuildStep_SubGrid(gameData);
                break;
            case 5:
                world.BuildStep_NodeType(gameData);
                break;
            case 6:
                world.BuildStep_WaterAmalgamation();
                break;
            case 7:
                world.BuildStep_LandAmalgamation();
                break;
            case 8:
                world.BuildStep_RiverSystem(rando);
                break;
            case 9:
                world.BuildStep_Climate(gameData);
                break;
            case 10:
                //world.BuildStep_Geology(rando);
                break;
            case 11:
                world.BuildStep_Biogeography(gameData);
                break;
            case 12:
                BuildStep_RenderPrep();
                break;
        }
        state_BuildStep++;
        if (state_BuildStep > 12)
        {
            state_BuildStep = 0;
            state_BuildingWorld = false;
            state_RenderReady = true;
            state_ControlHold = false;
            UnityEngine.Debug.Log("Control Hold disabled. World has been built");
        }
    }

    private void BuildStep_RenderPrep()
    {
        mesh = new LayeredSphereMesh(gameData, colorSettings);
        mesh.PrepSmallMesh(world, gameData); // activeMap available after this
        mesh.Get_builtWorld().transform.SetParent(mapHolder);
        float xLength = world.Get_grid().Get_Nodes().GetLength(0);
        float yLength = world.Get_grid().Get_Nodes().GetLength(1);
        Vector2 camPos = new Vector2(gameCamera.transform.position.x, gameCamera.transform.position.y);
        Vector2 feedStart = camPos * (gameData.settings.radius + 5f);
        Vector2 gridOffset = new Vector2(xLength / 2f, yLength / 2f);
        feeder = new PointFeeder(new Point2D(feedStart + gridOffset), 18, new PointValidator(world.Get_grid().Get_Nodes()), mesh.Get_activeMap());
        mesh.InjectLocalUVSFromRegions(world.Get_rivers().Get_drainages().Get_rawCells(), world.Get_grid());
    }

    //private void StepPopulateResources()
    //{
    //    ResourceBuilder resourceBuilder = new ResourceBuilder();
    //    resourceBuilder.SetPoints(world);

    //    switch (state_BuildStep)
    //    {
    //        default:
    //        case 0: // Initialize and group
    //            UnityEngine.Debug.Log("Control Hold enabled. Building Resources");
    //            gameData = resourceBuilder.BuildInitialResources(gameData);
    //            gameData.resourceNodes = resourceBuilder.BuildNodes(world.Get_grid().Get_Nodes(), 32, 20);
    //            resourceBuilder.InitializeStones(gameData, rando);
    //            resourceBuilder.InitializeMinerals(gameData, rando);

    //            state_BuildStep++;
    //            break;
    //        case 1: // Soil
    //            gameData = resourceBuilder.PopulateSoils(gameData, world.Get_grid(), rando);

    //            state_BuildStep++;
    //            break;
    //        case 2: // Stones
    //            gameData = resourceBuilder.PopulateStoneLayers(gameData, world.Get_grid(), rando, 5);

    //            state_BuildStep++;
    //            break;
    //        case 3: // Minerals
    //            gameData = resourceBuilder.PopulateMinerals(gameData, world.Get_grid(), rando);

    //            state_BuildStep++;
    //            break;
    //        case 4: // Marine Plant Seeding
    //            if (resourceBuilder.SeedMarinePlantStep(ref gameData, rb_step, world.Get_grid(), rando))
    //            {
    //                state_BuildStep++;
    //                rb_step = 0;
    //                rb_subStep = 0;
    //                UnityEngine.Debug.Log("Marine Plants seeding is complete.");
    //            }
    //            else
    //            {
    //                MarinePlant[] marinePlantSet = gameData.marinePlants; /*resourceBuilder.Get_marinePlants();*/
    //                rb_step++;
    //                if (rb_step < marinePlantSet.Length)
    //                    UnityEngine.Debug.Log("Seeding Marine Plant step is at " + (rb_step + 1) + "/" + marinePlantSet.Length + " which is " + marinePlantSet[rb_step].Get_id() + ":" + marinePlantSet[rb_step].Get_label());
    //            }
    //            //resourceBuilder.StartStepMarinePlants();
    //            break;
    //        case 5: // Marine Plants
    //            gameData = resourceBuilder.PopulateMarinePlants(gameData, world.Get_grid(), rando);

    //            state_BuildStep++;
    //            //int marinePlantPreviousSetSize = resourceBuilder.GetActiveSetSize();
    //            //int marinePlantCurrentSetSize = resourceBuilder.StepPopulateMarinePlants(rb_step, ref resourceNodes, world.Get_grid(), rando);
    //            //if (marinePlantCurrentSetSize == 0)
    //            //{
    //            //    state_BuildStep++;
    //            //    List<TerrestrialResource> finished = resourceBuilder.Get_finishedSet();
    //            //    string label = finished[finished.Count - 1].Get_label();
    //            //    UnityEngine.Debug.Log("Populating Marine Plants is completed with " + label + " and " + rb_loops + " loops");
    //            //    rb_step = 0;
    //            //    rb_loops = 0;
    //            //    resourceBuilder.FinishStepMarinePlants();
    //            //}
    //            //else
    //            //{
    //            //    if (marinePlantCurrentSetSize == marinePlantPreviousSetSize)
    //            //        rb_step++;
    //            //    else
    //            //    {
    //            //        List<TerrestrialResource> finished = resourceBuilder.Get_finishedSet();
    //            //        string label = finished[finished.Count - 1].Get_label();
    //            //        UnityEngine.Debug.Log(label + " completed at loop #" + rb_loops + " with " + marinePlantCurrentSetSize + " remaining.");
    //            //    }

    //            //    if (rb_step >= marinePlantCurrentSetSize)
    //            //    {
    //            //        rb_step = 0;
    //            //        rb_loops++;
    //            //    }
    //            //}
    //            break;
    //        case 6: // Plant Seeding
    //            if (resourceBuilder.SeedPlantsStep(ref gameData, rb_step, world.Get_grid(), rando, 20))
    //            {
    //                state_BuildStep++;
    //                rb_step = 0;
    //                rb_subStep = 0;
    //                UnityEngine.Debug.Log("Seeding Plants is complete");
    //            }
    //            else
    //            {
    //                Plant[] plantSet = gameData.plants;/*resourceBuilder.Get_plants();*/
    //                rb_step++;
    //                if (rb_step < plantSet.Length)
    //                    UnityEngine.Debug.Log("Plant Seeding step is at " + rb_step + "/" + plantSet.Length + " which is " + plantSet[rb_step].Get_id() + ":" + plantSet[rb_step].Get_label());
    //            }
    //            //resourceBuilder.StartStepPlants();
    //            break;
    //        case 7: // Plants
    //            gameData = resourceBuilder.PopulatePlants(gameData, world.Get_grid(), rando, 20);

    //            state_BuildStep++;
    //            //int plantPreviousSetSize = resourceBuilder.GetActiveSetSize();
    //            //int plantCurrentSetSize = resourceBuilder.StepPlants(rb_step, ref resourceNodes, world.Get_grid(), rando, 20);
    //            //if (plantCurrentSetSize == 0)
    //            //{
    //            //    state_BuildStep++;
    //            //    List<TerrestrialResource> finished = resourceBuilder.Get_finishedSet();
    //            //    string label = finished[finished.Count - 1].Get_label();
    //            //    UnityEngine.Debug.Log("Populating Plants is completed with " + label + " and " + rb_loops + " loops");
    //            //    rb_step = 0;
    //            //    rb_loops = 0;
    //            //    resourceBuilder.FinishStepPlants();
    //            //}
    //            //else
    //            //{
    //            //    if (plantCurrentSetSize == plantPreviousSetSize)
    //            //        rb_step++;
    //            //    else
    //            //    {
    //            //        List<TerrestrialResource> finished = resourceBuilder.Get_finishedSet();
    //            //        string label = finished[finished.Count - 1].Get_label();
    //            //        UnityEngine.Debug.Log(label + " completed at loop #" + rb_loops + " with " + plantCurrentSetSize + " remaining.");
    //            //    }

    //            //    if (rb_step >= plantCurrentSetSize)
    //            //    {
    //            //        rb_step = 0;
    //            //        rb_loops++;
    //            //    }
    //            //}
    //            break;
    //        case 8: // Biomes
    //            gameData = resourceBuilder.ProcessPlantPopulations(gameData, world.Get_grid(), rando, 20);
    //            gameData = resourceBuilder.ProcessSimpleBiomes(gameData, world.Get_grid());

    //            state_BuildStep++;
    //            break;
    //        case 9: // Insect Seeding
    //            if (resourceBuilder.SeedInsectStep(ref gameData, rb_step, rb_subStep, world.Get_grid(), rando))
    //            {
    //                state_BuildStep++;
    //                rb_step = 0;
    //                rb_subStep = 0;
    //                UnityEngine.Debug.Log("Seeding Insects is complete");
    //            }
    //            else
    //            {
    //                Insect[][] insectSet = gameData.Get_trophicInsects();
    //                if (rb_subStep < insectSet[rb_step].Length)
    //                    rb_subStep++;
    //                else
    //                {
    //                    rb_step++;
    //                    rb_subStep = 0;
    //                }
    //                if ((rb_step < insectSet.Length) && (rb_subStep < insectSet[rb_step].Length))
    //                    UnityEngine.Debug.Log("Seeding Insect step is at " + (rb_step + 1) + "/" + insectSet.Length + " and " + (rb_subStep + 1) + insectSet[rb_step].Length + " which is " + insectSet[rb_step][rb_subStep].Get_id() + ":" + insectSet[rb_step][rb_subStep].Get_label());
    //            }
    //            //resourceBuilder.StartStepPopulateInsects();
    //            break;
    //        case 10: // Insects
    //            gameData = resourceBuilder.PopulateInsects(gameData, world.Get_grid(), rando);

    //            state_BuildStep++;

    //            //int insectPreviousSetSize = resourceBuilder.GetActiveSetSize();
    //            //int insectCurrentSetSize = resourceBuilder.StepPopulateInsects(rb_step, ref resourceNodes, world.Get_grid(), rando);
    //            //if (insectCurrentSetSize == 0)
    //            //{
    //            //    state_BuildStep++;
    //            //    List<TerrestrialResource> finished = resourceBuilder.Get_finishedSet();
    //            //    string label = finished[finished.Count - 1].Get_label();
    //            //    UnityEngine.Debug.Log("Populating Insects is completed with " + label + " and " + rb_loops + " loops");
    //            //    rb_step = 0;
    //            //    rb_loops = 0;
    //            //    resourceBuilder.FinishStepPopulateInsects();
    //            //}
    //            //else
    //            //{
    //            //    if (insectCurrentSetSize == insectPreviousSetSize)
    //            //        rb_step++;
    //            //    else
    //            //    {
    //            //        List<TerrestrialResource> finished = resourceBuilder.Get_finishedSet();
    //            //        string label = finished[finished.Count - 1].Get_label();
    //            //        UnityEngine.Debug.Log(label + " completed at loop #" + rb_loops + " with " + insectCurrentSetSize + " remaining.");
    //            //    }

    //            //    if (rb_step >= insectCurrentSetSize)
    //            //    {
    //            //        rb_step = 0;
    //            //        rb_loops++;
    //            //    }
    //            //}
    //            break;
    //        case 11: // Freshwater Fish Seeding (Not Implemented Yet)
    //            state_BuildStep++;
    //            break;
    //        case 12: // Freshwater Fish (Not Implemented Yet)
    //            state_BuildStep++;
    //            break;
    //        case 13: // Marine Fish Seeding
    //            if (resourceBuilder.SeedMarineFishStep(ref gameData, rb_step, rb_subStep, world.Get_grid(), rando))
    //            {
    //                state_BuildStep++;
    //                rb_step = 0;
    //                rb_subStep = 0;
    //                UnityEngine.Debug.Log("Marine Fish seeding is complete.");
    //            }
    //            else
    //            {
    //                MarineFish[][] fishSet = gameData.Get_trophicMarineFish();
    //                if (rb_subStep < fishSet[rb_step].Length)
    //                    rb_subStep++;
    //                else
    //                {
    //                    rb_step++;
    //                    rb_subStep = 0;
    //                }
    //                if ((rb_step < fishSet.Length) && (rb_subStep < fishSet[rb_step].Length))
    //                    UnityEngine.Debug.Log("Seeding Marine Fish step is at " + (rb_step + 1) + "/" + fishSet.Length + " and " + (rb_subStep + 1) + "/" + fishSet[rb_step].Length + " which is " + fishSet[rb_step][rb_subStep].Get_id() + ":" + fishSet[rb_step][rb_subStep].Get_label());
    //            }
    //            //resourceBuilder.StartStepPopulateMarineFish();
    //            break;
    //        case 14: // Marine Fish
    //            gameData = resourceBuilder.PopulateMarineFish(gameData, world.Get_grid(), rando);

    //            state_BuildStep++;

    //            // this process takes far far too long and needs optimization
    //            // simply using the reject list doesn't actually seem to help
    //            //int marineFishPreviousSetSize = resourceBuilder.GetActiveSetSize();
    //            ////int marineFishCurrentSetSize = resourceBuilder.StepPopulateMarineFish(rb_step, ref resourceNodes, world.Get_grid(), rando);
    //            //int marineFishCurrentSetSize = resourceBuilder.StepValidPopulateMarineFish(rb_step, ref resourceNodes, world.Get_grid(), rando);
    //            //if (marineFishCurrentSetSize == 0)
    //            //{
    //            //    state_BuildStep++;
    //            //    List<TerrestrialResource> finished = resourceBuilder.Get_finishedSet();
    //            //    string label = finished[finished.Count - 1].Get_label();
    //            //    UnityEngine.Debug.Log("Populating Marine Fish is completed with " + label + " and " + rb_loops + " loops");
    //            //    rb_step = 0;
    //            //    rb_loops = 0;
    //            //    resourceBuilder.FinishStepPopulateMarineFish();
    //            //}
    //            //else
    //            //{
    //            //    if (marineFishCurrentSetSize == marineFishPreviousSetSize)
    //            //        rb_step++;
    //            //    else
    //            //    {
    //            //        List<TerrestrialResource> finished = resourceBuilder.Get_finishedSet();
    //            //        string label = finished[finished.Count - 1].Get_label();
    //            //        UnityEngine.Debug.Log(label + " completed at loop #" + rb_loops + " with " + marineFishCurrentSetSize + " remaining.");
    //            //    }

    //            //    if (rb_step >= marineFishCurrentSetSize)
    //            //    {
    //            //        rb_step = 0;
    //            //        rb_loops++;
    //            //    }
    //            //}
    //            break;
    //        case 15: // Animal Seeding
    //            if (resourceBuilder.SeedAnimalStep(ref gameData, rb_step, rb_subStep, world.Get_grid(), rando))
    //            {
    //                state_BuildStep++;
    //                rb_step = 0;
    //                rb_subStep = 0;
    //                UnityEngine.Debug.Log("Seeding Animals is complete");
    //            }
    //            else
    //            {
    //                Animal[][] animalSet = gameData.Get_trophicAnimals();
    //                if (rb_subStep < animalSet[rb_step].Length)
    //                    rb_subStep++;
    //                else
    //                {
    //                    rb_step++;
    //                    rb_subStep = 0;
    //                }
    //                if ((rb_step < animalSet.Length) && (rb_subStep < animalSet[rb_step].Length))
    //                    UnityEngine.Debug.Log("Seeding Animal step is at " + (rb_step + 1) + "/" + animalSet.Length + " and " + rb_subStep + "/" + animalSet[rb_step].Length + " which is " + animalSet[rb_step][rb_subStep].Get_id() + ":" + animalSet[rb_step][rb_subStep].Get_label());
    //            }
    //            //resourceBuilder.StartStepPopulateAnimals();
    //            break;
    //        case 16: // Animals
    //            gameData = resourceBuilder.PopulateAnimals(gameData, world.Get_grid(), rando);

    //            state_BuildStep++;

    //            //int animalPreviousSetSize = resourceBuilder.GetActiveSetSize();
    //            //int animalCurrentSetSize = resourceBuilder.StepPopulateAnimals(rb_step, ref resourceNodes, world.Get_grid(), rando);
    //            //if (animalCurrentSetSize == 0)
    //            //{
    //            //    state_BuildStep++;
    //            //    List<TerrestrialResource> finished = resourceBuilder.Get_finishedSet();
    //            //    string label = finished[finished.Count - 1].Get_label();
    //            //    UnityEngine.Debug.Log("Populating Animals is completed with " + label + " and " + rb_loops + " loops");
    //            //    rb_step = 0;
    //            //    rb_loops = 0;
    //            //    resourceBuilder.FinishStepPopulateAnimals();
    //            //}
    //            //else
    //            //{
    //            //    if (animalCurrentSetSize == animalPreviousSetSize)
    //            //        rb_step++;
    //            //    else
    //            //    {
    //            //        List<TerrestrialResource> finished = resourceBuilder.Get_finishedSet();
    //            //        string label = finished[finished.Count - 1].Get_label();
    //            //        UnityEngine.Debug.Log(label + " completed at loop #" + rb_loops + " with " + animalCurrentSetSize + " remaining.");
    //            //    }

    //            //    if (rb_step >= animalCurrentSetSize)
    //            //    {
    //            //        rb_step = 0;
    //            //        rb_loops++;
    //            //    }
    //            //}
    //            break;
    //    }
    //    if (state_BuildStep > 16)
    //    {
    //        state_BuildStep = 0;
    //        state_PopulateResources = false;
    //        state_ResourcesReady = true;
    //        state_ControlHold = false;
    //        UnityEngine.Debug.Log("Control Hold disabled. Resources have been built");

    //        Persistence persistence = new Persistence("/gamedata.json");
    //        persistence.SaveData(gameData);
    //        UnityEngine.Debug.Log("Game state saved to file");
    //    }
}

