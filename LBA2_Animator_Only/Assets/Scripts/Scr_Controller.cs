using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Scr_Controller : MonoBehaviour
    {
    // Folder properties.
    public string LM2Folder = "/LM2s/";
    public string LM2String = "Hat_Kid_LBA2.lm2";
    public string ANMFolder = "/ANMs/";
    public string ANMString = "ANIM_CUSTOM.anm";




    // ANM properties.
    private const short ANIM_FRAMEOFF = 0x00;
    private const short ANIM_BONESOFF = 0x02;
    private const short ANIM_LOOPSOFF = 0x04;
    private const short ANIM_STARTOFF = 0x08;




    // LBA2 model properties.
    private const short BONESNUM =     0x20;
    private const short BONESOFF =     0x24;
    private const short VERTICESNUM =  0x28;
    private const short VERTICESOFF =  0x2C;
    private const short NORMALSNUM =   0x30;
    private const short NORMALSOFF =   0x34;
    private const short TRIANGLESNUM = 0x40;
    private const short TRIANGLESOFF = 0x44;

    private const short MAX_FRAMES =   20;  // only 20 for now
    private const short MAX_BONES =    30;  // only 30 bones (LBA2 limitation)
    private const short MAX_POLYGONS = 530; // only 530 (LBA2 limitation)
    private const short MAX_COLORS =   10;  // since we only have 10 colors for now

    public short bones;
    public short bonesLoaded; // for loading animations
    public short vertices;
    public short normals;
    public short triangles;

    public short bonesOffset;
    public short verticesOffset;
    public short normalsOffset;
    public short trianglesOffset;




    // LBA2 model struct properties.
    [System.Serializable]
    public struct BONE
        {
        public int index;

        public short parentBone;
        public short parentVertex;
        public short numOfVertices;
        public short padding;
        };

    [System.Serializable]
    public struct POLYGON
        {
        public short polygonType;
        public short amountTris;
        public short amountBytes;
        public short padding1;
        public short V1;
        public short V2;
        public short V3;
        public short padding2;
        public short color;
        public short padding3;
        };

    [System.Serializable]
    public enum LBA2Colors
        {
        LBA2_COLORS_BLACK_ID =  0, // black 
        LBA2_COLORS_BROWN_ID =  1, // brown
        LBA2_COLORS_TAN_ID =    2, // tan
        LBA2_COLORS_GRAY_ID =   3, // gray
        LBA2_COLORS_RED_ID =    4, // red
        LBA2_COLORS_ORANGE_ID = 5, // orange
        LBA2_COLORS_YELLOW_ID = 6, // yellow
        LBA2_COLORS_GREEN_ID =  7, // green
        LBA2_COLORS_BLUE_ID =   8, // blue
        LBA2_COLORS_PURPLE_ID = 9, // purple

        LBA2_COLORS_BLACK =  4096, // black (actual value)
        LBA2_COLORS_BROWN =  4112, // brown (actual value)
        LBA2_COLORS_TAN =    4128, // tan (actual value)
        LBA2_COLORS_GRAY =   4144, // gray (actual value)
        LBA2_COLORS_RED =    4160, // red (actual value)
        LBA2_COLORS_ORANGE = 4176, // orange (actual value)
        LBA2_COLORS_YELLOW = 4192, // yellow (actual value)
        LBA2_COLORS_GREEN =  4224, // green (actual value)
        LBA2_COLORS_BLUE =   4288, // blue (actual value)
        LBA2_COLORS_PURPLE = 4304, // purple (actual value)
        };

    [System.Serializable]
    public struct ANM
        {
        public short speed; // frame speed

        public short moveX; // movement x
        public short moveY; // movement y
        public short moveZ; // movement z

        public short lockX; // air-lock x
        public short lockY; // air-lock y
        public short lockZ; // air-lock z

        public short padding1;

        public short[] bonesType;
        public float[] bonesX;
        public float[] bonesY;
        public float[] bonesZ;

        public Quaternion[] bonesQuat; // used in editor - only for smooth quaternion lerping
        };




    //private GameObject theLM2;
    private GameObject[] theLM2 = new GameObject[MAX_COLORS];

    public BONE[] myBones = new BONE[MAX_BONES];
    public short[] myVerticesParent = new short[MAX_POLYGONS];
    public short[] myNormalsParent = new short[MAX_POLYGONS];
    public POLYGON[] myPolygons = new POLYGON[MAX_POLYGONS];

    public ANM[] myFrames = new ANM[MAX_FRAMES]; // 20 frames for now
    public ANM singleFrame = new ANM(); // used as a buffer for copying and pasting

    // Unity model properties.
    private Vector3[] originalVertices; // default posed model
    private Vector3[] newVertices;
    private Vector3[] newNormals;
    private Vector3[,] newTriangles;
    private int[] newTriangleIndices;

    private int[,] trianglesColorsIndices; // this is used to sort the correct colors to the correct polygons
    private int[] newTriangleIndicesBlack;
    private int[] newTriangleIndicesBrown;
    private int[] newTriangleIndicesTan;
    private int[] newTriangleIndicesGray;
    private int[] newTriangleIndicesRed;
    private int[] newTriangleIndicesOrange;
    private int[] newTriangleIndicesYellow;
    private int[] newTriangleIndicesGreen;
    private int[] newTriangleIndicesBlue;
    private int[] newTriangleIndicesPurple;

    private Color colBlack =  new Color(0.0f, 0.0f, 0.0f);
    private Color colBrown =  new Color(128 * 0.004f, 64 * 0.004f, 0);
    private Color colTan =    new Color(255 * 0.004f, 210 * 0.004f, 164 * 0.004f);
    private Color colGray =   new Color(192 * 0.004f, 192 * 0.004f, 192 * 0.004f);
    private Color colRed =    new Color(1, 0, 0);
    private Color colOrange = new Color(255 * 0.004f, 128 * 0.004f, 0);
    private Color colYellow = new Color(215 * 0.004f, 215 * 0.004f, 0);
    private Color colGreen =  new Color(0, 1, 0);
    private Color colBlue =   new Color(0, 0, 1);
    private Color colPurple = new Color(128 * 0.004f, 0, 255 * 0.004f);

    public short frames = 1;
    public short frameLoop = 0;
    public float frameTime = 0.0f;
    public short currentFrame = 1;
    public bool  frameAltered = false;
    public float frameSpeedAltered = 0.0f;
    public GameObject[] frameButtons;



    private int nearestVertIndex;
    private Vector3 nearestBone;

    private GameObject highlightGameObject;
    private Mesh highlightTriangle;
    private Vector3[] highlightVertices = new Vector3[3];
    public Material highlightMat;

    private GameObject onionSkinGameObject; // used for showing the previous frame in an animation
    private Mesh onionSkinMesh;
    public Material onionMat;
    private Vector3[] onionSkinVertices;
    private bool onionSkinEnabled;

    private Mesh[] theMesh = new Mesh[MAX_COLORS];
    public Material[] theMat = new Material[MAX_COLORS];
    private bool[] meshUsed = new bool[MAX_COLORS];




    public bool modelLoaded = false;
    public bool animPlaying = false;
    public bool animMovement = true; // whether the model stays at origin or not when testing movement




    public bool mouseDrag;
    public Vector3 mousePos;
    public Vector3 mousePosPrev;

    private GameObject selectedBone;
    public bool boneSelected = false;

    public Vector3 newPos;
    public RaycastHit hit;
    public Ray ray;

    public float vertexScale = 0.01f;
    public float rotationScale = 11.333333f; // do not edit as this is crucial to bone rotations
    public float frameScale = 0.001f; // do not edit as this is crucial to animation speed




    void Start()
    {
        // populate frame data
        myFrames = new ANM[MAX_FRAMES];

        for (int i = 0; i < MAX_FRAMES; i++)
            {
            myFrames[i] = new ANM();

            myFrames[i].bonesType = new short[MAX_BONES]; // create arrays
            myFrames[i].bonesX = new float[MAX_BONES];
            myFrames[i].bonesY = new float[MAX_BONES];
            myFrames[i].bonesZ = new float[MAX_BONES];

            myFrames[i].bonesQuat = new Quaternion[MAX_BONES];
            };

        for (int i = 0; i < MAX_BONES; i++)
            {
            for (int j = 0; j < MAX_FRAMES; j++)
                {
                myFrames[j].bonesType[i] = new short(); // fill arrays
                myFrames[j].bonesX[i] = new float();
                myFrames[j].bonesY[i] = new float();
                myFrames[j].bonesZ[i] = new float();

                myFrames[j].bonesQuat[i] = new Quaternion();
                };
            };

        singleFrame.bonesType = new short[MAX_BONES];

        singleFrame.bonesX = new float[MAX_BONES];
        singleFrame.bonesY = new float[MAX_BONES];
        singleFrame.bonesZ = new float[MAX_BONES];

        singleFrame.bonesQuat = new Quaternion[MAX_BONES];




        // highlight triangle
        highlightTriangle = new Mesh();
        for (int i = 0; i < 3; i++)
            highlightVertices[i] = new Vector3();

        highlightGameObject = new GameObject();
        highlightGameObject.name = "Highlight_Triangle";

        highlightGameObject.AddComponent<MeshFilter>();
        highlightGameObject.AddComponent<MeshRenderer>();

        onionSkinMesh = new Mesh();
        onionSkinGameObject = new GameObject();
        onionSkinGameObject.name = "Onion_Skin";

        onionSkinGameObject.AddComponent<MeshFilter>();
        onionSkinGameObject.AddComponent<MeshRenderer>();



        // populate list with all LM2 models that exist in the folder
        List<string> modelOptions = new List<string>{ };
        List<string> anmOptions = new List<string>{ };

        // get all of the model files from directory - easier than making our own list by hand
        DirectoryInfo dirLM2 = new DirectoryInfo(Application.dataPath + LM2Folder);
        FileInfo[] infoLM2 = dirLM2.GetFiles("*.lm2"); // get all files with the LM2 extension

        foreach (FileInfo f in infoLM2)
            modelOptions.Add(f.Name.Replace(Application.dataPath, "")); // get only the file name, not the entire directory name

        DirectoryInfo dirANM = new DirectoryInfo(Application.dataPath + ANMFolder);
        FileInfo[]  infoANM = dirANM.GetFiles("*.anm"); // get all files with the ANM extension

        foreach (FileInfo f in infoANM)
            anmOptions.Add(f.Name.Replace(Application.dataPath, "")); // get only the file name, not the entire directory name

        //GameObject.Find("Dropdown_LM2").GetComponent<Dropdown>().ClearOptions();
        GameObject.Find("Dropdown_LM2").GetComponent<Dropdown>().AddOptions(modelOptions);
        GameObject.Find("Dropdown_ANM").GetComponent<Dropdown>().AddOptions(anmOptions);
    }

    void Update()
    {
        GameObject.Find("Text_Frames").GetComponent<Text>().text = "Frames: " + frames.ToString();
        GameObject.Find("Text_Frame_Loop").GetComponent<Text>().text = "Frame Loop: " + frameLoop.ToString();
        GameObject.Find("Text_Current_Frame").GetComponent<Text>().text = "Current Frame: " + currentFrame.ToString();




        if (modelLoaded == true && boneSelected == true)
        {
            GameObject.Find("Text_Selected_Bone").GetComponent<Text>().text = "Selected Bone: " + selectedBone.name.Replace("bone_helper_", "").ToString();

            GameObject.Find("Text_Bone_Info").GetComponent<Text>().text = "Press ESCAPE when you're finished rotating the bone.";
        }
        else
        {
            GameObject.Find("Text_Selected_Bone").GetComponent<Text>().text = "Selected Bone: None";

            if (frameAltered == true)
                GameObject.Find("Text_Bone_Info").GetComponent<Text>().text = "Frame altered. Hit 'Record Frame' to save current keyframe.";
            else if (animPlaying == false && frameSpeedAltered != myFrames[currentFrame - 1].speed)
                GameObject.Find("Text_Bone_Info").GetComponent<Text>().text = "Frame speed altered. Hit 'Record Frame' to save current keyframe.";
            else
                GameObject.Find("Text_Bone_Info").GetComponent<Text>().text = "Bone Info:";

            //else if (frameSpeedAltered != myFrames[currentFrame - 1].speed)
            //GameObject.Find("Text_Bone_Info").GetComponent<Text>().text = "Frame speed altered. Hit 'Record Frame' to save current keyframe.";
            //else
            //GameObject.Find("Text_Bone_Info").GetComponent<Text>().text = "Bone Info:";
        };

        GameObject.Find("Text_Frame_Speed").GetComponent<Text>().text = "Frame Speed: " + myFrames[currentFrame - 1].speed.ToString();
        if (animPlaying != true)
            myFrames[currentFrame - 1].speed = (short)GameObject.Find("Slider_Frame_Speed").GetComponent<Slider>().value;

        // right mouse button
        if (Input.GetMouseButton(1))
        {
            mouseDrag = true;

            mousePosPrev.x = Input.mousePosition.x - (Screen.width / 2); // get center of screen
            mousePosPrev.x = mousePosPrev.x * 0.01f; // scale it down
            mousePosPrev.x -= mousePos.x;
        }
        else
        {
            mouseDrag = false;

            mousePos.x = Input.mousePosition.x - (Screen.width / 2); // get center of screen
            mousePos.x = mousePos.x * 0.01f; // scale it down
        };




        if (animPlaying == true)
        {
            // begin animation
            //frameTime += 0.05f;

            // the bigger the frame speed, the longer the frame lasts
            frameTime += (Time.deltaTime);

            // go to next frame
            if (frameTime > (myFrames[currentFrame - 1].speed * frameScale)) // best one
            {
                currentFrame += 1;

                frameTime = 0.0f;
            };

            // restart animation
            if (currentFrame > frames)
            {
                currentFrame = (short)(frameLoop + 1); // start from where the loop begins
            };
        };




        if (modelLoaded == true)
        {
            if (Input.GetKey(KeyCode.Q))
                Camera.main.transform.Translate(0, 0.5f, 0);
            if (Input.GetKey(KeyCode.E))
                Camera.main.transform.Translate(0, -0.5f, 0);

            if (Input.GetKey(KeyCode.W))
                Camera.main.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 0.5f;
            if (Input.GetKey(KeyCode.S))
                Camera.main.transform.position = Camera.main.transform.position + Camera.main.transform.forward * -0.5f;

            if (Input.GetKey(KeyCode.A))
                Camera.main.transform.Translate(-0.5f, 0, 0);
            if (Input.GetKey(KeyCode.D))
                Camera.main.transform.Translate(0.5f, 0, 0);

            if (mouseDrag == true)
                Camera.main.transform.Rotate(0, mousePosPrev.x, 0);

            // rotate the model
            if (mouseDrag == true)
            {
                //GameObject.Find("bone_0").transform.Rotate(0, -mousePosPrev.x, 0);
                //GameObject.Find("Direction_Arrow").transform.Rotate(0, -mousePosPrev.x, 0);
            };

            // Update the vertices.
            for (int i = 0; i < vertices; i++)
            {
                newVertices[i] = GameObject.Find("vertex_" + i.ToString()).transform.position;

                // rotate the vertex from its bone's parent vertex
                GameObject.Find("vertex_" + i.ToString()).transform.localRotation *= GameObject.Find("vertex_" + i.ToString()).transform.parent.localRotation;
            };

            // update the model
            for (int i = 0; i < MAX_COLORS; i++)
            {
                theMesh[i].vertices = newVertices;

                theMesh[i].RecalculateBounds();
                theMesh[i].RecalculateNormals();
            };

            // though each mesh exists, we still need to check if it actually has any elements
            // if it does, then we will update it
            if (meshUsed[0] == true) theLM2[0].GetComponent<MeshCollider>().sharedMesh = theMesh[0];
            if (meshUsed[1] == true) theLM2[1].GetComponent<MeshCollider>().sharedMesh = theMesh[1];
            if (meshUsed[2] == true) theLM2[2].GetComponent<MeshCollider>().sharedMesh = theMesh[2];
            if (meshUsed[3] == true) theLM2[3].GetComponent<MeshCollider>().sharedMesh = theMesh[3];
            if (meshUsed[4] == true) theLM2[4].GetComponent<MeshCollider>().sharedMesh = theMesh[4];
            if (meshUsed[5] == true) theLM2[5].GetComponent<MeshCollider>().sharedMesh = theMesh[5];
            if (meshUsed[6] == true) theLM2[6].GetComponent<MeshCollider>().sharedMesh = theMesh[6];
            if (meshUsed[7] == true) theLM2[7].GetComponent<MeshCollider>().sharedMesh = theMesh[7];
            if (meshUsed[8] == true) theLM2[8].GetComponent<MeshCollider>().sharedMesh = theMesh[8];
            if (meshUsed[9] == true) theLM2[9].GetComponent<MeshCollider>().sharedMesh = theMesh[9];




            // Update the onion skin model only on frames where necessary.
            if (onionSkinEnabled == true)
            {
                if (animPlaying == false)
                {
                    if (currentFrame != 1)
                        onionSkinGameObject.GetComponent<MeshRenderer>().enabled = true;
                    else
                        onionSkinGameObject.GetComponent<MeshRenderer>().enabled = false;

                    for (int i = 0; i < vertices; i++)
                    {
                        onionSkinVertices[i] = GameObject.Find("vertex_onion_" + i.ToString()).transform.position;

                        // rotate the vertex from its bone's parent vertex
                        GameObject.Find("vertex_onion_" + i.ToString()).transform.localRotation *= GameObject.Find("vertex_onion_" + i.ToString()).transform.parent.localRotation;
                    };

                    onionSkinMesh.vertices = onionSkinVertices;

                    onionSkinMesh.RecalculateBounds();
                    onionSkinMesh.RecalculateNormals();
                }
                else
                {
                    onionSkinGameObject.GetComponent<MeshRenderer>().enabled = false;
                };
            }
            else
            {
                onionSkinGameObject.GetComponent<MeshRenderer>().enabled = false;
            };




            if (animPlaying == false)
            {
                if (boneSelected == true)
                {
                    GameObject.Find("Rotation_Arrow").transform.position = GameObject.Find(selectedBone.name.Replace("helper_", "").ToString()).transform.position;
                    GameObject.Find("Rotation_Arrow").transform.LookAt(Camera.main.transform.position);

                    if (Input.GetKey(KeyCode.X))
                    {
                        selectedBone.GetComponent<Scr_Bone_Helper>().rotX = Input.mousePosition.x - (Screen.width / 2);
                        frameAltered = true;
                    };

                    if (Input.GetKey(KeyCode.C))
                    {
                        selectedBone.GetComponent<Scr_Bone_Helper>().rotY = Input.mousePosition.x - (Screen.width / 2);
                        frameAltered = true;
                    };

                    if (Input.GetKey(KeyCode.Z))
                    {
                        selectedBone.GetComponent<Scr_Bone_Helper>().rotZ = Input.mousePosition.x - (Screen.width / 2);
                        frameAltered = true;
                    };

                    if (Input.GetKey(KeyCode.Escape))
                        boneSelected = false;
                }
                else
                {
                    GameObject.Find("Rotation_Arrow").transform.position = Vector3.zero;
                    GameObject.Find("Rotation_Arrow").transform.localRotation = Quaternion.identity;
                };

                ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider != null)
                    {
                        newPos = hit.point;

                        //Debug.Log(hit.triangleIndex);
                        //Debug.DrawLine(newVertices[myPolygons[hit.triangleIndex].V1], newVertices[myPolygons[hit.triangleIndex].V2]);
                        //Debug.DrawLine(newVertices[myPolygons[hit.triangleIndex].V2], newVertices[myPolygons[hit.triangleIndex].V3]);
                        //Debug.DrawLine(newVertices[myPolygons[hit.triangleIndex].V3], newVertices[myPolygons[hit.triangleIndex].V1]);

                        // we can find out what triangle we are touching by returning data from the hitpoint
                        highlightVertices[0] = newVertices[myPolygons[hit.triangleIndex].V1];
                        highlightVertices[1] = newVertices[myPolygons[hit.triangleIndex].V2];
                        highlightVertices[2] = newVertices[myPolygons[hit.triangleIndex].V3];

                        highlightTriangle.vertices = highlightVertices;
                        highlightTriangle.triangles = new int[3] { 0, 2, 1 };

                        highlightGameObject.GetComponent<MeshFilter>().mesh = highlightTriangle;
                        highlightGameObject.GetComponent<MeshRenderer>().material = highlightMat;

                        highlightTriangle.RecalculateBounds();
                        highlightTriangle.RecalculateNormals();

                        // get distance from hit point to the highlighted vertices
                        // return the index of the closest one and use that for selecting bones
                        nearestVertIndex = getSmallestInTriangle(hit.point, highlightVertices);

                        //Debug.Log(nearestVertIndex);
                        //Debug.DrawLine(hit.point, highlightVertices[nearestVertIndex])

                        //nearestBone = newVertices[myBones[myPolygons[hit.triangleIndex].V1].parentVertex];
                        //nearestBone = myVerticesParent[myBones[myPolygons[hit.triangleIndex].V1].parentVertex];
                        //nearestBone = newVertices[myVerticesParent[myPolygons[hit.triangleIndex].V1]];
                        //nearestBone = newVertices[myVerticesParent[myBones[myPolygons[hit.triangleIndex].V1].parentVertex]];
                        //Debug.DrawLine(hit.point, nearestBone);

                        if (Input.GetMouseButton(0))
                        {
                            // check the vertex's parent and rotate that specific bone - exclude the first two bones
                            if (nearestVertIndex == 0)
                            {
                                if (myVerticesParent[myPolygons[hit.triangleIndex].V1] != 0 || myVerticesParent[myPolygons[hit.triangleIndex].V1] != 1)
                                {
                                    // rotate bone from our position
                                    if (boneSelected == false)
                                    {
                                        selectedBone = GameObject.Find("bone_helper_" + myVerticesParent[myPolygons[hit.triangleIndex].V1]);

                                        boneSelected = true;
                                    };
                                }
                            }
                            else if (nearestVertIndex == 1)
                            {
                                if (myVerticesParent[myPolygons[hit.triangleIndex].V2] != 0 || myVerticesParent[myPolygons[hit.triangleIndex].V2] != 1)
                                {
                                    // rotate bone from our position
                                    if (boneSelected == false)
                                    {
                                        selectedBone = GameObject.Find("bone_helper_" + myVerticesParent[myPolygons[hit.triangleIndex].V2]);

                                        boneSelected = true;
                                    };
                                }
                            }
                            else if (nearestVertIndex == 2)
                            {
                                if (myVerticesParent[myPolygons[hit.triangleIndex].V3] != 0 || myVerticesParent[myPolygons[hit.triangleIndex].V3] != 1)
                                {
                                    // rotate bone from our position
                                    if (boneSelected == false)
                                    {
                                        selectedBone = GameObject.Find("bone_helper_" + myVerticesParent[myPolygons[hit.triangleIndex].V3]);

                                        boneSelected = true;
                                    };
                                }
                            };
                        };
                    };
                };
            };
        };
    }

    public int getSmallestInTriangle(Vector3 theHit, Vector3[] verts)
    {
        int i;
        int index = 0;
        float temp = Vector3.Distance(theHit, verts[0]);

        for (i = 0; i < 3; i++)
        {
            if (temp > Vector3.Distance(theHit, verts[i]))
            {
                index = i;
                temp = Vector3.Distance(theHit, verts[i]);
            };
        };

        return index;
    }

    public void loadLM2()
        {
        //GameObject theLM2 = new GameObject();

        if (modelLoaded == false)
        {
            Dropdown theDropdown = GameObject.Find("Dropdown_LM2").GetComponent<Dropdown>();

            // create a separate game object for each group of polygon colors
            // it's better than making every polygon a separate game object
            for (int i = 0; i < MAX_COLORS; i++)
            {
                theLM2[i] = new GameObject();

                //theLM2.name = LM2String.Replace(".lm2", ""); // remove the file extension
                theLM2[i].name = theDropdown.options[theDropdown.value].text.Replace(".lm2", ""); // remove the file extension

                theLM2[i].AddComponent<MeshFilter>();
                theLM2[i].AddComponent<MeshCollider>();
                theLM2[i].AddComponent<MeshRenderer>();

                theMesh[i] = new Mesh();
                theLM2[i].GetComponent<MeshFilter>().mesh = theMesh[i];
                theLM2[i].GetComponent<MeshCollider>().sharedMesh = theMesh[i];
            };

            theLM2[0].name = (theLM2[0].name + "_Black");
            theLM2[1].name = (theLM2[1].name + "_Brown");
            theLM2[2].name = (theLM2[2].name + "_Tan");
            theLM2[3].name = (theLM2[3].name + "_Gray");
            theLM2[4].name = (theLM2[4].name + "_Red");
            theLM2[5].name = (theLM2[5].name + "_Orange");
            theLM2[6].name = (theLM2[6].name + "_Yellow");
            theLM2[7].name = (theLM2[7].name + "_Green");
            theLM2[8].name = (theLM2[8].name + "_Blue");
            theLM2[9].name = (theLM2[9].name + "_Purple");
            



            //FileStream theStream = new FileStream(Application.dataPath + LM2Folder + LM2String, FileMode.Open);
            FileStream theStream = new FileStream(Application.dataPath + LM2Folder + theDropdown.options[theDropdown.value].text, FileMode.Open);
            BinaryReader theReader = new BinaryReader(theStream);

            theReader.BaseStream.Position = BONESNUM;
            bones = theReader.ReadInt16();

            theReader.BaseStream.Position = BONESOFF;
            bonesOffset = theReader.ReadInt16();

            theReader.BaseStream.Position = VERTICESNUM;
            vertices = theReader.ReadInt16();

            theReader.BaseStream.Position = VERTICESOFF;
            verticesOffset = theReader.ReadInt16();

            theReader.BaseStream.Position = NORMALSNUM;
            normals = theReader.ReadInt16();

            theReader.BaseStream.Position = NORMALSOFF;
            normalsOffset = theReader.ReadInt16();

            theReader.BaseStream.Position = TRIANGLESNUM;
            triangles = theReader.ReadInt16();




            int currentVertex = 0;

            int currentTriangleBlack = 0;
            int currentTriangleBrown = 0;
            int currentTriangleTan = 0;
            int currentTriangleGray = 0;
            int currentTriangleRed = 0;
            int currentTriangleOrange = 0;
            int currentTriangleYellow = 0;
            int currentTriangleGreen = 0;
            int currentTriangleBlue = 0;
            int currentTrianglePurple = 0;

            originalVertices = new Vector3[vertices];
            newVertices = new Vector3[vertices]; // Fill up the vertex array.
            newNormals = new Vector3[normals]; // Fill up the normal array.
            newTriangleIndices = new int[triangles * 3]; // Fill up the triangle vertex indices array.
            newTriangles = new Vector3[triangles, 3]; // Fill up the triangle vertex positions array.

            newTriangleIndicesBlack = new int[triangles * 3]; // Fill up all color arrays.
            newTriangleIndicesBrown = new int[triangles * 3];
            newTriangleIndicesTan = new int[triangles * 3];
            newTriangleIndicesGray = new int[triangles * 3];
            newTriangleIndicesRed = new int[triangles * 3];
            newTriangleIndicesOrange = new int[triangles * 3];
            newTriangleIndicesYellow = new int[triangles * 3];
            newTriangleIndicesGreen = new int[triangles * 3];
            newTriangleIndicesBlue = new int[triangles * 3];
            newTriangleIndicesPurple = new int[triangles * 3];
            trianglesColorsIndices = new int[MAX_COLORS, MAX_POLYGONS];

            onionSkinVertices = new Vector3[vertices];




            theReader.BaseStream.Position = TRIANGLESOFF;
            trianglesOffset = theReader.ReadInt16();

            // Get bones.
            theReader.BaseStream.Position = bonesOffset;
            for (int i = 0; i < bones; i++)
            {
                myBones[i].index = i;

                myBones[i].parentBone = theReader.ReadInt16();
                myBones[i].parentVertex = theReader.ReadInt16();
                myBones[i].numOfVertices = theReader.ReadInt16();
                myBones[i].padding = theReader.ReadInt16();
            };

            // Get vertices.
            theReader.BaseStream.Position = verticesOffset;
            for (int i = 0; i < vertices; i++)
            {
                Vector3 pos = new Vector3();

                pos.x = -theReader.ReadInt16(); // Flip the X-axis.
                pos.y = theReader.ReadInt16();
                pos.z = theReader.ReadInt16();

                newVertices[i] = pos;

                myVerticesParent[i] = theReader.ReadInt16();
            };

            // reset this variable
            currentVertex = 0;

            // Create vertex game objects and re-orient vertex positions in relation to bones so they can be moved.
            for (int i = 0; i < vertices; i++)
            {
                GameObject vertex = new GameObject();
                vertex.name = "vertex_" + i.ToString();

                newVertices[i] += newVertices[myBones[myVerticesParent[i]].parentVertex];
                originalVertices[i] = newVertices[i];

                vertex.transform.position = newVertices[i] * vertexScale;




                GameObject vertex_onion = new GameObject();
                vertex_onion.name = "vertex_onion_" + i.ToString();

                vertex_onion.transform.position = newVertices[i] * vertexScale;
            };

            // Create bone game objects as handles so the user can translate and rotate them.
            for (int i = 0; i < bones; i++)
            {
                GameObject bone = new GameObject();
                bone.name = "bone_" + i.ToString();

                bone.transform.position = newVertices[myBones[i].parentVertex] * vertexScale;

                bone.AddComponent<Scr_Bone>();
                bone.GetComponent<Scr_Bone>().index = i;




                GameObject boneHelper = new GameObject();
                boneHelper.name = "bone_helper_" + i.ToString();

                boneHelper.AddComponent<Scr_Bone_Helper>();
                boneHelper.GetComponent<Scr_Bone_Helper>().index = i;




                GameObject boneOnion = new GameObject();
                boneOnion.name = "bone_onion_" + i.ToString();

                boneOnion.transform.position = newVertices[myBones[i].parentVertex] * vertexScale;

                boneOnion.AddComponent<Scr_Bone_Helper>();
                boneOnion.GetComponent<Scr_Bone_Helper>().index = i;
            };

            // Parent vertices to bones.
            for (int i = 0; i < vertices; i++)
            {
                GameObject vertex = GameObject.Find("vertex_" + i.ToString());

                for (int j = 0; j < bones; j++)
                    if (myVerticesParent[i] == j)
                        vertex.transform.SetParent(GameObject.Find("bone_" + j.ToString()).transform);




                GameObject vertex_onion = GameObject.Find("vertex_onion_" + i.ToString());

                for (int j = 0; j < bones; j++)
                    if (myVerticesParent[i] == j)
                        vertex_onion.transform.SetParent(GameObject.Find("bone_onion_" + j.ToString()).transform);
            };

            // Parent bones to parent bones.
            for (int i = 0; i < bones; i++)
            {
                if (i != 0)
                {
                    GameObject bone = GameObject.Find("bone_" + i.ToString());

                    bone.transform.parent = GameObject.Find("bone_" + myBones[i].parentBone.ToString()).transform;
                    //bone.transform.SetParent(GameObject.Find("bone_" + myBones[i].parentBone.ToString()).transform);




                    GameObject bone_onion = GameObject.Find("bone_onion_" + i.ToString());

                    bone_onion.transform.parent = GameObject.Find("bone_onion_" + myBones[i].parentBone.ToString()).transform;
                };
            };

            // Get normals.
            theReader.BaseStream.Position = normalsOffset;
            for (int i = 0; i < normals; i++)
            {
                Vector3 pos = new Vector3();

                pos.x = theReader.ReadInt16();
                pos.y = theReader.ReadInt16();
                pos.z = theReader.ReadInt16();

                newNormals[i] = pos;

                myNormalsParent[i] = theReader.ReadInt16();

                pos.x = (pos.x * 0.2f);
                pos.y = (pos.y * 0.2f);
                pos.z = (pos.z * 0.2f);
            };

            // Get triangles.
            theReader.BaseStream.Position = trianglesOffset;
            for (int i = 0; i < triangles; i++)
            {
                myPolygons[i].polygonType = theReader.ReadInt16();
                myPolygons[i].amountTris = theReader.ReadInt16();
                myPolygons[i].amountBytes = theReader.ReadInt16();
                myPolygons[i].padding1 = theReader.ReadInt16();
                myPolygons[i].V1 = theReader.ReadInt16();
                myPolygons[i].V2 = theReader.ReadInt16();
                myPolygons[i].V3 = theReader.ReadInt16();
                myPolygons[i].padding2 = theReader.ReadInt16();
                myPolygons[i].color = theReader.ReadInt16();
                myPolygons[i].padding3 = theReader.ReadInt16();
            };

            // Get triangle indices.
            for (int i = 0; i < triangles; i++)
            {
                newTriangleIndices[currentVertex + 0] = myPolygons[i].V2;
                newTriangleIndices[currentVertex + 1] = myPolygons[i].V1;
                newTriangleIndices[currentVertex + 2] = myPolygons[i].V3;

                currentVertex += 3;
            };

            currentVertex = 0;

            // Construct Unity triangles from LBA2 triangles.
            for (int i = 0; i < triangles; i++)
            {
                // Triangle vertex 1.
                newTriangles[i, 0].x = newVertices[myPolygons[i].V1].x;
                newTriangles[i, 0].y = newVertices[myPolygons[i].V1].y;
                newTriangles[i, 0].z = newVertices[myPolygons[i].V1].z;

                // Triangle vertex 2.
                newTriangles[i, 1].x = newVertices[myPolygons[i].V2].x;
                newTriangles[i, 1].y = newVertices[myPolygons[i].V2].y;
                newTriangles[i, 1].z = newVertices[myPolygons[i].V2].z;

                // Triangle vertex 3.
                newTriangles[i, 2].x = newVertices[myPolygons[i].V3].x;
                newTriangles[i, 2].y = newVertices[myPolygons[i].V3].y;
                newTriangles[i, 2].z = newVertices[myPolygons[i].V3].z;

                // Check and set colors.
                if (myPolygons[i].color == (short)LBA2Colors.LBA2_COLORS_BLACK) // black
                {
                    trianglesColorsIndices[(char)LBA2Colors.LBA2_COLORS_BLACK_ID, currentTriangleBlack] = i;
                    currentTriangleBlack += 1;

                    newTriangleIndicesBlack[currentVertex + 0] = myPolygons[i].V2;
                    newTriangleIndicesBlack[currentVertex + 1] = myPolygons[i].V1;
                    newTriangleIndicesBlack[currentVertex + 2] = myPolygons[i].V3;
                    currentVertex += 3;

                    meshUsed[(char)LBA2Colors.LBA2_COLORS_BLACK_ID] = true;
                };
                if (myPolygons[i].color == (short)LBA2Colors.LBA2_COLORS_BROWN)
                {
                    trianglesColorsIndices[(char)LBA2Colors.LBA2_COLORS_BROWN_ID, currentTriangleBrown] = i;
                    currentTriangleBrown += 1;

                    newTriangleIndicesBrown[currentVertex + 0] = myPolygons[i].V2;
                    newTriangleIndicesBrown[currentVertex + 1] = myPolygons[i].V1;
                    newTriangleIndicesBrown[currentVertex + 2] = myPolygons[i].V3;
                    currentVertex += 3;

                    meshUsed[(char)LBA2Colors.LBA2_COLORS_BROWN_ID] = true;
                };
                if (myPolygons[i].color == (short)LBA2Colors.LBA2_COLORS_TAN)
                {
                    trianglesColorsIndices[(char)LBA2Colors.LBA2_COLORS_TAN_ID, currentTriangleTan] = i;
                    currentTriangleTan += 1;

                    newTriangleIndicesTan[currentVertex + 0] = myPolygons[i].V2;
                    newTriangleIndicesTan[currentVertex + 1] = myPolygons[i].V1;
                    newTriangleIndicesTan[currentVertex + 2] = myPolygons[i].V3;
                    currentVertex += 3;

                    meshUsed[(char)LBA2Colors.LBA2_COLORS_TAN_ID] = true;
                };
                if (myPolygons[i].color == (short)LBA2Colors.LBA2_COLORS_GRAY)
                {
                    trianglesColorsIndices[(char)LBA2Colors.LBA2_COLORS_GRAY_ID, currentTriangleGray] = i;
                    currentTriangleGray += 1;

                    newTriangleIndicesGray[currentVertex + 0] = myPolygons[i].V2;
                    newTriangleIndicesGray[currentVertex + 1] = myPolygons[i].V1;
                    newTriangleIndicesGray[currentVertex + 2] = myPolygons[i].V3;
                    currentVertex += 3;

                    meshUsed[(char)LBA2Colors.LBA2_COLORS_GRAY_ID] = true;
                };
                if (myPolygons[i].color == (short)LBA2Colors.LBA2_COLORS_RED)
                {
                    trianglesColorsIndices[(char)LBA2Colors.LBA2_COLORS_RED_ID, currentTriangleRed] = i;
                    currentTriangleRed += 1;

                    newTriangleIndicesRed[currentVertex + 0] = myPolygons[i].V2;
                    newTriangleIndicesRed[currentVertex + 1] = myPolygons[i].V1;
                    newTriangleIndicesRed[currentVertex + 2] = myPolygons[i].V3;
                    currentVertex += 3;

                    meshUsed[(char)LBA2Colors.LBA2_COLORS_RED_ID] = true;
                };
                if (myPolygons[i].color == (short)LBA2Colors.LBA2_COLORS_ORANGE)
                {
                    trianglesColorsIndices[(char)LBA2Colors.LBA2_COLORS_ORANGE_ID, currentTriangleOrange] = i;
                    currentTriangleOrange += 1;

                    newTriangleIndicesOrange[currentVertex + 0] = myPolygons[i].V2;
                    newTriangleIndicesOrange[currentVertex + 1] = myPolygons[i].V1;
                    newTriangleIndicesOrange[currentVertex + 2] = myPolygons[i].V3;
                    currentVertex += 3;

                    meshUsed[(char)LBA2Colors.LBA2_COLORS_ORANGE_ID] = true;
                };
                if (myPolygons[i].color == (short)LBA2Colors.LBA2_COLORS_YELLOW)
                {
                    trianglesColorsIndices[(char)LBA2Colors.LBA2_COLORS_YELLOW_ID, currentTriangleYellow] = i;
                    currentTriangleYellow += 1;

                    newTriangleIndicesYellow[currentVertex + 0] = myPolygons[i].V2;
                    newTriangleIndicesYellow[currentVertex + 1] = myPolygons[i].V1;
                    newTriangleIndicesYellow[currentVertex + 2] = myPolygons[i].V3;
                    currentVertex += 3;

                    meshUsed[(char)LBA2Colors.LBA2_COLORS_YELLOW_ID] = true;
                };
                if (myPolygons[i].color == (short)LBA2Colors.LBA2_COLORS_GREEN)
                {
                    trianglesColorsIndices[(char)LBA2Colors.LBA2_COLORS_GREEN_ID, currentTriangleGreen] = i;
                    currentTriangleGreen += 1;

                    newTriangleIndicesGreen[currentVertex + 0] = myPolygons[i].V2;
                    newTriangleIndicesGreen[currentVertex + 1] = myPolygons[i].V1;
                    newTriangleIndicesGreen[currentVertex + 2] = myPolygons[i].V3;
                    currentVertex += 3;

                    meshUsed[(char)LBA2Colors.LBA2_COLORS_GREEN_ID] = true;
                };
                if (myPolygons[i].color == (short)LBA2Colors.LBA2_COLORS_BLUE)
                {
                    trianglesColorsIndices[(char)LBA2Colors.LBA2_COLORS_BLUE_ID, currentTriangleBlue] = i;
                    currentTriangleBlue += 1;

                    newTriangleIndicesBlue[currentVertex + 0] = myPolygons[i].V2;
                    newTriangleIndicesBlue[currentVertex + 1] = myPolygons[i].V1;
                    newTriangleIndicesBlue[currentVertex + 2] = myPolygons[i].V3;
                    currentVertex += 3;

                    meshUsed[(char)LBA2Colors.LBA2_COLORS_BLUE_ID] = true;
                };
                if (myPolygons[i].color == (short)LBA2Colors.LBA2_COLORS_PURPLE)
                {
                    trianglesColorsIndices[(char)LBA2Colors.LBA2_COLORS_PURPLE_ID, currentTrianglePurple] = i;
                    currentTrianglePurple += 1;

                    newTriangleIndicesPurple[currentVertex + 0] = myPolygons[i].V2;
                    newTriangleIndicesPurple[currentVertex + 1] = myPolygons[i].V1;
                    newTriangleIndicesPurple[currentVertex + 2] = myPolygons[i].V3;
                    currentVertex += 3;

                    meshUsed[(char)LBA2Colors.LBA2_COLORS_PURPLE_ID] = true;
                };
            };

            // Scale down vertices.
            for (int i = 0; i < vertices; i++)
            {
                newVertices[i] *= vertexScale;
            };

            theReader.Close();




            // Assign elements to Unity mesh.
            for (int i = 0; i < MAX_COLORS; i++)
                {
                // all meshes share the same vertices and normals but different triangles
                theMesh[i].vertices = newVertices;
                theMesh[i].normals = newNormals;

                // might want to look into combining meshes
                // but this should do well enough for now
                if (i == 0) theMesh[i].triangles = newTriangleIndicesBlack;
                if (i == 1) theMesh[i].triangles = newTriangleIndicesBrown;
                if (i == 2) theMesh[i].triangles = newTriangleIndicesTan;
                if (i == 3) theMesh[i].triangles = newTriangleIndicesGray;
                if (i == 4) theMesh[i].triangles = newTriangleIndicesRed;
                if (i == 5) theMesh[i].triangles = newTriangleIndicesOrange;
                if (i == 6) theMesh[i].triangles = newTriangleIndicesYellow;
                if (i == 7) theMesh[i].triangles = newTriangleIndicesGreen;
                if (i == 8) theMesh[i].triangles = newTriangleIndicesBlue;
                if (i == 9) theMesh[i].triangles = newTriangleIndicesPurple;

                theLM2[i].GetComponent<MeshRenderer>().material = theMat[i];

                theMesh[i].RecalculateBounds();
                theMesh[i].RecalculateNormals();
                };

            onionSkinMesh.vertices = onionSkinVertices;
            onionSkinMesh.triangles = newTriangleIndices;

            onionSkinGameObject.GetComponent<MeshFilter>().mesh = onionSkinMesh;
            onionSkinGameObject.GetComponent<MeshRenderer>().material = onionMat;

            onionSkinMesh.RecalculateBounds();
            onionSkinMesh.RecalculateNormals();

            /*CombineInstance[] myCombine = new CombineInstance[MAX_COLORS];
            for (int i = 1; i < MAX_COLORS; i++)
                myCombine[i].mesh = theMesh[i];

            theLM2[0].GetComponent<MeshFilter>().sharedMesh.CombineMeshes(myCombine, false, false);*/

            modelLoaded = true;
            };
        }

    public void recordFrame()
    {
        frameAltered = false;

        frameSpeedAltered = myFrames[currentFrame - 1].speed;

        myFrames[currentFrame - 1].speed = (short)GameObject.Find("Slider_Frame_Speed").GetComponent<Slider>().value;

        for (int i = 0; i < bones; i++)
        {
            myFrames[currentFrame - 1].bonesX[i] = GameObject.Find("bone_helper_" + i.ToString()).GetComponent<Scr_Bone_Helper>().rotX;
            myFrames[currentFrame - 1].bonesY[i] = GameObject.Find("bone_helper_" + i.ToString()).GetComponent<Scr_Bone_Helper>().rotY;
            myFrames[currentFrame - 1].bonesZ[i] = GameObject.Find("bone_helper_" + i.ToString()).GetComponent<Scr_Bone_Helper>().rotZ;
        }
    }

    public void addFrame()
    {
        if (frames < MAX_FRAMES)
            frames += 1;
    }

    public void removeFrame()
    {
        if (frames > 1)
            frames -= 1;
    }

    public void copyFrame()
    {
        for (int i = 0; i < bones; i++)
            {
            singleFrame.speed = myFrames[currentFrame - 1].speed;

            singleFrame.moveX = myFrames[currentFrame - 1].moveX;
            singleFrame.moveY = myFrames[currentFrame - 1].moveY;
            singleFrame.moveZ = myFrames[currentFrame - 1].moveZ;

            singleFrame.lockX = myFrames[currentFrame - 1].lockX;
            singleFrame.lockY = myFrames[currentFrame - 1].lockY;
            singleFrame.lockZ = myFrames[currentFrame - 1].lockZ;

            singleFrame.bonesX[i] = myFrames[currentFrame - 1].bonesX[i];
            singleFrame.bonesY[i] = myFrames[currentFrame - 1].bonesY[i];
            singleFrame.bonesZ[i] = myFrames[currentFrame - 1].bonesZ[i];
            };
    }

    public void pasteFrame()
    {
        for (int i = 0; i < bones; i++)
            {
            myFrames[currentFrame - 1].speed = singleFrame.speed;

            myFrames[currentFrame - 1].moveX = singleFrame.moveX;
            myFrames[currentFrame - 1].moveY = singleFrame.moveY;
            myFrames[currentFrame - 1].moveZ = singleFrame.moveZ;

            myFrames[currentFrame - 1].lockX = singleFrame.lockX;
            myFrames[currentFrame - 1].lockY = singleFrame.lockY;
            myFrames[currentFrame - 1].lockZ = singleFrame.lockZ;

            myFrames[currentFrame - 1].bonesX[i] = singleFrame.bonesX[i];
            myFrames[currentFrame - 1].bonesY[i] = singleFrame.bonesY[i];
            myFrames[currentFrame - 1].bonesZ[i] = singleFrame.bonesZ[i];
            };
        loadFrame();
    }

    public void nextFrame()
    {
        // reset the speed if we didn't record the frame already
        if (frameSpeedAltered != myFrames[currentFrame - 1].speed)
            myFrames[currentFrame - 1].speed = (short)frameSpeedAltered;

        if (currentFrame < frames)
            currentFrame += 1;

        frameAltered = false;
        boneSelected = false;

        loadFrame();
    }

    public void previousFrame()
    {
        // reset the speed if we didn't record the frame already
        if (frameSpeedAltered != myFrames[currentFrame - 1].speed)
            myFrames[currentFrame - 1].speed = (short)frameSpeedAltered;

        if (currentFrame > 1)
            currentFrame -= 1;

        frameAltered = false;
        boneSelected = false;

        loadFrame();
    }

    public void resetFrame()
    {
        for (int i = 0; i < bones; i++)
            {
            GameObject.Find("bone_helper_" + i.ToString()).GetComponent<Scr_Bone_Helper>().rotX = 0.0f;
            GameObject.Find("bone_helper_" + i.ToString()).GetComponent<Scr_Bone_Helper>().rotY = 0.0f;
            GameObject.Find("bone_helper_" + i.ToString()).GetComponent<Scr_Bone_Helper>().rotZ = 0.0f;
            };

        frameAltered = true;
    }

    public void loadFrame()
    {
        frameSpeedAltered = myFrames[currentFrame - 1].speed;

        GameObject.Find("Slider_Frame_Speed").GetComponent<Slider>().value = myFrames[currentFrame - 1].speed;

        for (int i = 0; i < bones; i++)
            {
            GameObject.Find("bone_helper_" + i.ToString()).GetComponent<Scr_Bone_Helper>().rotX = myFrames[currentFrame - 1].bonesX[i];
            GameObject.Find("bone_helper_" + i.ToString()).GetComponent<Scr_Bone_Helper>().rotY = myFrames[currentFrame - 1].bonesY[i];
            GameObject.Find("bone_helper_" + i.ToString()).GetComponent<Scr_Bone_Helper>().rotZ = myFrames[currentFrame - 1].bonesZ[i];
            };
    }

    public void enableMovement()
    {
        animMovement = !animMovement;

        if (animMovement == true)
            GameObject.Find("Button_Enable_Movement").GetComponent<Image>().color = Color.gray;
        else
            GameObject.Find("Button_Enable_Movement").GetComponent<Image>().color = Color.white;
    }

    public void enableOnionSkin()
    {
        onionSkinEnabled = !onionSkinEnabled;

        if (onionSkinEnabled == true)
            GameObject.Find("Button_Enable_Onion_Skin").GetComponent<Image>().color = Color.gray;
        else
            GameObject.Find("Button_Enable_Onion_Skin").GetComponent<Image>().color = Color.white;
    }

    public void playANM()
    {
        if (modelLoaded == true)
        {
        if (animPlaying == false)
            {
                for (int i = 0; i < frameButtons.Length; i++)
                    frameButtons[i].SetActive(false);

                frameTime = 0.0f;

                boneSelected = false;
                frameAltered = false;

                GameObject.Find("Button_Play_ANM").transform.GetChild(0).GetComponent<Text>().text = "Stop ANM";
            }
        else
            {
                for (int i = 0; i < frameButtons.Length; i++)
                    frameButtons[i].SetActive(true);

                currentFrame = 1;

                GameObject.Find("Button_Play_ANM").transform.GetChild(0).GetComponent<Text>().text = "Play ANM";
            };

        animPlaying = !animPlaying;
        loadFrame();
        };
    }

    public void loadANM()
    {
        if (modelLoaded == true)
            {
            Dropdown theDropdown = GameObject.Find("Dropdown_ANM").GetComponent<Dropdown>();

            //FileStream theStream = new FileStream(Application.dataPath + ANMFolder + ANMString, FileMode.Open);
            FileStream theStream = new FileStream(Application.dataPath + ANMFolder + theDropdown.options[theDropdown.value].text, FileMode.Open);
            BinaryReader theReader = new BinaryReader(theStream);

            // get frames
            theReader.BaseStream.Position = ANIM_FRAMEOFF;
            frames = theReader.ReadInt16();

            // get bones
            theReader.BaseStream.Position = ANIM_BONESOFF;
            //bones = theReader.ReadInt16();
            bonesLoaded = theReader.ReadInt16();

            // get frame loop
            theReader.BaseStream.Position = ANIM_LOOPSOFF;
            frameLoop = theReader.ReadInt16();

            // get frame data
            theReader.BaseStream.Position = ANIM_STARTOFF;

            for (int i = 0; i < frames; i++)
            {
                myFrames[i].speed = theReader.ReadInt16();

                myFrames[i].moveX = theReader.ReadInt16();
                myFrames[i].moveY = theReader.ReadInt16();
                myFrames[i].moveZ = theReader.ReadInt16();

                myFrames[i].lockX = theReader.ReadInt16();
                myFrames[i].lockY = theReader.ReadInt16();
                myFrames[i].lockZ = theReader.ReadInt16();
                myFrames[i].padding1 = theReader.ReadInt16();

                for (int j = 0; j < bonesLoaded; j++)
                {
                    if (j != 0)
                    {
                    myFrames[i].bonesType[j] = theReader.ReadInt16();

                    if (myFrames[i].bonesType[j] == 0x00) // rotation
                        {
                        myFrames[i].bonesX[j] = theReader.ReadInt16() / rotationScale;
                        myFrames[i].bonesY[j] = theReader.ReadInt16() / rotationScale;
                        myFrames[i].bonesZ[j] = theReader.ReadInt16() / rotationScale;
                        }
                    else // translation
                        {
                        myFrames[i].bonesX[j] = theReader.ReadInt16();
                        myFrames[i].bonesY[j] = theReader.ReadInt16();
                        myFrames[i].bonesZ[j] = theReader.ReadInt16();
                        };
                    };
                }
            }

            theReader.Close();

            loadFrame();
            };
    }

    public void saveANM()
        {
            float fX;
            float fY;
            float fZ;

            Dropdown theDropdown = GameObject.Find("Dropdown_ANM").GetComponent<Dropdown>();

            //FileStream myFileStream = new FileStream(Application.dataPath + ANMFolder + ANMString, FileMode.Create);
            FileStream myFileStream = new FileStream(Application.dataPath + ANMFolder + theDropdown.options[theDropdown.value].text, FileMode.Create); // overwrite the existing animation
            BinaryWriter myBinaryWriter = new BinaryWriter(myFileStream);

            myBinaryWriter.Seek(0x00, SeekOrigin.Begin);

            myBinaryWriter.Write((Int16)frames); // frames
            myBinaryWriter.Write((Int16)bones); // bones
            myBinaryWriter.Write((Int16)frameLoop); // frame loop
            myBinaryWriter.Write((Int16)0x00); // padding

            for (int i = 0; i < frames; i++) // frames
                {
                myBinaryWriter.Write((Int16)myFrames[i].speed); // frame speed

                myBinaryWriter.Write((Int16)myFrames[i].moveX); // movement x
                myBinaryWriter.Write((Int16)myFrames[i].moveY); // movement y
                myBinaryWriter.Write((Int16)myFrames[i].moveZ); // movement z

                myBinaryWriter.Write((Int16)myFrames[i].lockX); // air-lock x
                myBinaryWriter.Write((Int16)myFrames[i].lockY); // air-lock y
                myBinaryWriter.Write((Int16)myFrames[i].lockZ); // air-lock z
                myBinaryWriter.Write((Int16)0x00); // air-lock padding

                for (int j = 1; j < bones; j++) // the very first bone is excluded since it's just the origin of the model
                    {
                    if (j == 1)
                        {
                        myBinaryWriter.Write((Int16)0x01); // bone type: translation

                        myBinaryWriter.Write((Int16)0x00); // bone x
                        myBinaryWriter.Write((Int16)0x00); // bone y
                        myBinaryWriter.Write((Int16)0x00); // bone z
                        }
                    else
                        {
                        myBinaryWriter.Write((Int16)0x00); // bone type: rotation
                        
                        GameObject bone = GameObject.Find("bone_helper_" + j.ToString());

                        // best results for framed animations
                        bone.GetComponent<Scr_Bone_Helper>().rotX = myFrames[i].bonesX[j];
                        bone.GetComponent<Scr_Bone_Helper>().rotY = myFrames[i].bonesY[j];
                        bone.GetComponent<Scr_Bone_Helper>().rotZ = myFrames[i].bonesZ[j];
                        
                        fX = (short)(bone.GetComponent<Scr_Bone_Helper>().rotX * rotationScale);
                        fY = (short)(bone.GetComponent<Scr_Bone_Helper>().rotY * rotationScale);
                        fZ = (short)(bone.GetComponent<Scr_Bone_Helper>().rotZ * rotationScale);

                        myBinaryWriter.Write((Int16) fX); // bone x
                        myBinaryWriter.Write((Int16) fY); // bone y
                        myBinaryWriter.Write((Int16) fZ); // bone z
                        }
                    };
                };

        myBinaryWriter.Close();
        }
    }