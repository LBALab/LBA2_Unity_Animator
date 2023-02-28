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
    private short ANIM_FRAMEOFF = 0x00;
    private short ANIM_BONESOFF = 0x02;
    private short ANIM_LOOPSOFF = 0x04;
    private short ANIM_STARTOFF = 0x08;



    
    // LBA2 model properties.
    private short BONESNUM =     0x20;
    private short BONESOFF =     0x24;
    private short VERTICESNUM =  0x28;
    private short VERTICESOFF =  0x2C;
    private short NORMALSNUM =   0x30;
    private short NORMALSOFF =   0x34;
    private short TRIANGLESNUM = 0x40;
    private short TRIANGLESOFF = 0x44;

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

        //public Matrix4x4 theMatrix;
        //public Vector3 boneRot;

        //public float rotX;
        //public float rotY;
        //public float rotZ;
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

        //public short[] bonesX; // rotation / translation X
        //public short[] bonesY; // rotation / translation Y
        //public short[] bonesZ; // rotation / translation Z

        public short[] bonesType;
        public float[] bonesX;
        public float[] bonesY;
        public float[] bonesZ;

        public Quaternion[] bonesQuat; // used in editor - only for smooth quaternion lerping
        };



    private GameObject theLM2;
    public BONE[] myBones = new BONE[30];
    public short[] myVerticesParent = new short[530];
    public short[] myNormalsParent = new short[530];
    public POLYGON[] myPolygons = new POLYGON[530];

    public ANM[] myFrames = new ANM[20]; // 20 frames for now
    public ANM singleFrame = new ANM(); // used as a buffer for copying and pasting

    // Unity model properties.
    private Vector3[] originalVertices; // default posed model
    private Vector3[] newVertices;
    private Vector3[] newNormals;
    private Vector3[,] newTriangles;
    private int[] newTriangleIndices;
    private Color[] newColors;
    private Color[] oldColors; // used for highlighting the model

    // for mesh colliders
    //public Vector3[,] vertsBones = new Vector3[30,100]; // each bone has at least 100 verts

    // not super efficient, but it'll do for now
        /*public Vector3[] vertsBone0 = new Vector3[200];
        public Vector3[] vertsBone1 = new Vector3[200];
        public Vector3[] vertsBone2 = new Vector3[200];
        public Vector3[] vertsBone3 = new Vector3[200];
        public Vector3[] vertsBone4;
        public Vector3[] vertsBone5;
        public Vector3[] vertsBone6;
        public Vector3[] vertsBone7;
        public Vector3[] vertsBone8;
        public Vector3[] vertsBone9;
        public Vector3[] vertsBone10;
        public Vector3[] vertsBone11;
        public Vector3[] vertsBone12;
        public Vector3[] vertsBone13;
        public Vector3[] vertsBone14;
        public Vector3[] vertsBone15;
        public Vector3[] vertsBone16;
        public Vector3[] vertsBone17;
        public Vector3[] vertsBone18;
        public Vector3[] vertsBone19;
        public Vector3[] vertsBone20;*/

    /*private Vector3[] newVertices = new Vector3[530]; // Fill up the vertex array.
    private Vector3[] newNormals = new Vector3[530]; // Fill up the normal array.
    private Vector3[,] newTriangleIndices = new int[530 * 3]; // Fill up the triangle vertex indices array.
    private int[] newTriangles = new Vector3[530, 3]; // Fill up the triangle vertex positions array.
    private Color[] newColors = new Color[530]; // Fill up the color array.*/

    //public int[] rotTable360 = new int[359]; // rotation tables to convert from 360 degrees to 4096 LBA2 scene units
    //public int[] rotTable4096 = new int[359]; // this entire process has been a complete bitch for years - this really is the only way


    public short frames = 1;
    public short frameLoop = 0;
    public float frameTime = 0.0f;
    public short currentFrame = 1;
    public bool  frameAltered = false;
    public float frameSpeedAltered = 0.0f;
    public GameObject[] frameButtons;


    private Mesh theMesh;




    public Material matWhite;




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
        //Debug.Log(getTriangleFromBone());

        // populate frame data
        myFrames = new ANM[20];

        for (int i = 0; i < 20; i++)
            {
            myFrames[i] = new ANM();

            myFrames[i].bonesType = new short[30]; // create arrays
            myFrames[i].bonesX = new float[30];
            myFrames[i].bonesY = new float[30];
            myFrames[i].bonesZ = new float[30];

            myFrames[i].bonesQuat = new Quaternion[30];
            }

        for (int i = 0; i < 30; i++)
            {
            for (int j = 0; j < 20; j++)
                {
                /*myFrames[j].bonesX[i] = new short();
                myFrames[j].bonesY[i] = new short();
                myFrames[j].bonesZ[i] = new short();*/

                myFrames[j].bonesType[i] = new short(); // fill arrays
                myFrames[j].bonesX[i] = new float();
                myFrames[j].bonesY[i] = new float();
                myFrames[j].bonesZ[i] = new float();

                myFrames[j].bonesQuat[i] = new Quaternion();
                };
            };

        singleFrame.bonesType = new short[30];

        singleFrame.bonesX = new float[30];
        singleFrame.bonesY = new float[30];
        singleFrame.bonesZ = new float[30];

        singleFrame.bonesQuat = new Quaternion[30];

        /*vertsBone0 = new Vector3[200];
        vertsBone1 = new Vector3[200];
        vertsBone2 = new Vector3[200];
        vertsBone3 = new Vector3[200];*/



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

        //frameSpeedAltered = myFrames[currentFrame - 1].speed;

        /*if (frameSpeedAltered != myFrames[currentFrame - 1].speed)
            {

            };*/

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
            //frameTime += (myFrames[currentFrame - 1].speed * Time.deltaTime) * frameScale;
            //frameTime += (Time.deltaTime * (myFrames[currentFrame - 1].speed));

            //frameTime += (myFrames[currentFrame - 1].speed / frameScale);
            //frameTime += (myFrames[currentFrame - 1].speed * frameScale);
            //frameTime += (myFrames[currentFrame - 1].speed * frameScale) * myFrames[currentFrame - 1].speed;
            //frameTime += (myFrames[currentFrame - 1].speed * frameScale) / ((myFrames[currentFrame - 1].speed * frameScale)*10);

            // go to next frame
            //if (frameTime > 1.0f)
            if (frameTime > (myFrames[currentFrame - 1].speed * frameScale)) // best one
            //if (frameTime > (myFrames[currentFrame - 0].speed * frameScale))
            //if (frameTime > (Time.deltaTime / myFrames[currentFrame - 1].speed * frameScale))
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
            theMesh.vertices = newVertices;

            theMesh.RecalculateBounds();
	        theMesh.RecalculateNormals();

            theLM2.GetComponent<MeshCollider>().sharedMesh = theMesh;



            if (animPlaying == false)
                {
                if (boneSelected == true)
                    {
                    GameObject.Find("Rotation_Arrow").transform.position = GameObject.Find(selectedBone.name.Replace("helper_","").ToString()).transform.position;
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
                };

            ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
                {
                if (hit.collider != null)
                    {
                    newPos = hit.point;

                    //Debug.Log("hit!");

                    // loop through vertices
                    for (int i = 0; i < theMesh.vertices.Length; i++)
                        {
                        // check for closest ones
                        if (Vector3.Distance(newPos, theMesh.vertices[i]) < 0.5f)
                            {
                            // draw a line indicating we have that vertex
                            Debug.DrawLine(Vector3.zero, theMesh.vertices[i]);


                            // color the vertex red, indicating we can select it
                            //oldColors[i].r = 255;
                            //update the colors
                            //theMesh.colors = oldColors;


                            if (Input.GetMouseButton(0))
                                {
                                // check the vertex's parent and rotate that specific bone - exclude the first two bones
                                if (myVerticesParent[i] != 0 || myVerticesParent[i] != 1)
                                    {
                                    // rotate bone from our position
                                    if (boneSelected == false)
                                        {
                                        selectedBone = GameObject.Find("bone_helper_" + myVerticesParent[i]);

                                        boneSelected = true;
                                        };
                                    };
                                };
                            }
                        /*else
                            {
                            theMesh.colors = newColors;
                            };*/
                        };
                    };
                };

            };
        }

    public int[] getTriangleFromBone()
    {
        int[] myInt = new int[3];

        // need to get polygon whose vertex indices are linked to the same bone

        /*if (newVertices[myPolygons[0].V1].x == newVertices[myVerticesParent[2]].x &&
            newVertices[myPolygons[0].V1].y == newVertices[myVerticesParent[2]].y &&
            newVertices[myPolygons[0].V1].z == newVertices[myVerticesParent[2]].z)
        {

        }*/
        
        /*Debug.Log("px: " + newVertices[myPolygons[0].V1].x);
        Debug.Log("py: " + newVertices[myPolygons[0].V1].y);
        Debug.Log("pz: " + newVertices[myPolygons[0].V1].z);*/

        return myInt;
    }

    public void loadLM2()
        {
        //GameObject theLM2 = new GameObject();

        if (modelLoaded == false)
            {
            Dropdown theDropdown = GameObject.Find("Dropdown_LM2").GetComponent<Dropdown>();

            theLM2 = new GameObject();

            //theLM2.name = LM2String.Replace(".lm2", ""); // remove the file extension
            theLM2.name = theDropdown.options[theDropdown.value].text.Replace(".lm2", ""); // remove the file extension



            theLM2.AddComponent<MeshFilter>();
            theLM2.AddComponent<MeshCollider>();
            theLM2.AddComponent<MeshRenderer>();

            theMesh = new Mesh();
            theLM2.GetComponent<MeshFilter>().mesh = theMesh;
            theLM2.GetComponent<MeshCollider>().sharedMesh = theMesh;



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
            originalVertices = new Vector3[vertices];
            newVertices = new Vector3[vertices]; // Fill up the vertex array.
            newNormals = new Vector3[normals]; // Fill up the normal array.
            newTriangleIndices = new int[triangles * 3]; // Fill up the triangle vertex indices array.
            newTriangles = new Vector3[triangles, 3]; // Fill up the triangle vertex positions array.
            newColors = new Color[vertices]; // Fill up the color array.
            oldColors = new Color[vertices];



            theReader.BaseStream.Position = TRIANGLESOFF;
            trianglesOffset = theReader.ReadInt16();

            // Get bones.
            theReader.BaseStream.Position = bonesOffset;
            for (int i = 0; i < bones; i++)
                {
                myBones[i].index = i;

                myBones[i].parentBone =    theReader.ReadInt16();
                myBones[i].parentVertex =  theReader.ReadInt16();
                myBones[i].numOfVertices = theReader.ReadInt16();
                myBones[i].padding =       theReader.ReadInt16();
                };

            // Get vertices.
            theReader.BaseStream.Position = verticesOffset;
            for (int i = 0; i < vertices; i++)
                {
                Vector3 pos = new Vector3();

                pos.x = -theReader.ReadInt16(); // Flip the X-axis.
                pos.y =  theReader.ReadInt16();
                pos.z =  theReader.ReadInt16();

                newVertices[i] = pos;

                myVerticesParent[i] = theReader.ReadInt16();

                /*if (myVerticesParent[i] == 3)
                    {
                    vertsBone3[currentVertex] = pos * vertexScale;
                    currentVertex += 1;
                    };*/
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
                };

            // Create bone game objects as handles so the user can translate and rotate them.
            for (int i = 0; i < bones; i++)
                {
                GameObject bone = new GameObject();
                bone.name = "bone_" + i.ToString();
            
                bone.transform.position = newVertices[myBones[i].parentVertex] * vertexScale;

                //bone.AddComponent<BoxCollider>();
                //bone.GetComponent<BoxCollider>().size = new Vector3(0.1f, 0.1f, 0.1f);

                bone.AddComponent<Scr_Bone>();
                bone.GetComponent<Scr_Bone>().index = i;



                GameObject boneHelper = new GameObject();
                boneHelper.name = "bone_helper_" + i.ToString();

                boneHelper.AddComponent<Scr_Bone_Helper>();
                boneHelper.GetComponent<Scr_Bone_Helper>().index = i;

                //myBones[i].theMatrix = bone.transform.worldToLocalMatrix;

                //Debug.Log(myBones[i].theMatrix);
                };

            // Parent vertices to bones.
            for (int i = 0; i < vertices; i++)
                {
                GameObject vertex = GameObject.Find("vertex_" + i.ToString());

                for (int j = 0; j < bones; j++)
                    if (myVerticesParent[i] == j)
                        vertex.transform.SetParent(GameObject.Find("bone_" + j.ToString()).transform);
                };

            // Parent bones to parent bones.
            for (int i = 0; i < bones; i++)
                {
                if (i != 0)
                    {
                    GameObject bone = GameObject.Find("bone_" + i.ToString());
                
                    bone.transform.parent = GameObject.Find("bone_" + myBones[i].parentBone.ToString()).transform;
                    //bone.transform.SetParent(GameObject.Find("bone_" + myBones[i].parentBone.ToString()).transform);
                    };
                };

            // Get normals.
            theReader.BaseStream.Position = normalsOffset;
            for (int i = 0; i < normals; i++)
                {
                Vector3 pos = new Vector3();

                pos.x =     theReader.ReadInt16();
                pos.y =     theReader.ReadInt16();
                pos.z =     theReader.ReadInt16();

                newNormals[i] = pos;

                myNormalsParent[i] =  theReader.ReadInt16();

                pos.x = (pos.x * 0.2f);
                pos.y = (pos.y * 0.2f);
                pos.z = (pos.z * 0.2f);
                };
    
            // Get triangles.
            theReader.BaseStream.Position = trianglesOffset;
            for (int i = 0; i < triangles; i++)
                {
                myPolygons[i].polygonType = theReader.ReadInt16();
                myPolygons[i].amountTris =  theReader.ReadInt16();
                myPolygons[i].amountBytes = theReader.ReadInt16();
                myPolygons[i].padding1 =    theReader.ReadInt16();
                myPolygons[i].V1 =          theReader.ReadInt16();
                myPolygons[i].V2 =          theReader.ReadInt16();
                myPolygons[i].V3 =          theReader.ReadInt16();
                myPolygons[i].padding2 =    theReader.ReadInt16();
                myPolygons[i].color =       theReader.ReadInt16();
                myPolygons[i].padding3 =    theReader.ReadInt16();
                };
        
            // Get triangle indices.
            for (int i = 0; i < triangles; i++)
                {
                newTriangleIndices[currentVertex + 0] = myPolygons[i].V2;
                newTriangleIndices[currentVertex + 1] = myPolygons[i].V1;
                newTriangleIndices[currentVertex + 2] = myPolygons[i].V3;

                currentVertex += 3;
                };

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
                if (myPolygons[i].color == 4096) // black
                    {
                    newColors[myPolygons[i].V1] = new Color(0, 0, 0);
                    newColors[myPolygons[i].V2] = new Color(0, 0, 0);
                    newColors[myPolygons[i].V3] = new Color(0, 0, 0);

                    oldColors[myPolygons[i].V1] = new Color(0, 0, 0);
                    oldColors[myPolygons[i].V2] = new Color(0, 0, 0);
                    oldColors[myPolygons[i].V3] = new Color(0, 0, 0);
                    };
                if (myPolygons[i].color == 4112) // brown
                    {
                    newColors[myPolygons[i].V1] = new Color(128 * 0.004f, 64 * 0.004f, 0);
                    newColors[myPolygons[i].V2] = new Color(128 * 0.004f, 64 * 0.004f, 0);
                    newColors[myPolygons[i].V3] = new Color(128 * 0.004f, 64 * 0.004f, 0);

                    oldColors[myPolygons[i].V1] = new Color(128 * 0.004f, 64 * 0.004f, 0);
                    oldColors[myPolygons[i].V2] = new Color(128 * 0.004f, 64 * 0.004f, 0);
                    oldColors[myPolygons[i].V3] = new Color(128 * 0.004f, 64 * 0.004f, 0);
                    };
                if (myPolygons[i].color == 4128) // tan
                    {
                    newColors[myPolygons[i].V1] = new Color(255 * 0.004f, 210 * 0.004f, 164 * 0.004f);
                    newColors[myPolygons[i].V2] = new Color(255 * 0.004f, 210 * 0.004f, 164 * 0.004f);
                    newColors[myPolygons[i].V3] = new Color(255 * 0.004f, 210 * 0.004f, 164 * 0.004f);

                    oldColors[myPolygons[i].V1] = new Color(255 * 0.004f, 210 * 0.004f, 164 * 0.004f);
                    oldColors[myPolygons[i].V2] = new Color(255 * 0.004f, 210 * 0.004f, 164 * 0.004f);
                    oldColors[myPolygons[i].V3] = new Color(255 * 0.004f, 210 * 0.004f, 164 * 0.004f);
                    };
                if (myPolygons[i].color == 4144) // gray
                    {
                    newColors[myPolygons[i].V1] = new Color(192 * 0.004f, 192 * 0.004f, 192 * 0.004f);
                    newColors[myPolygons[i].V2] = new Color(192 * 0.004f, 192 * 0.004f, 192 * 0.004f);
                    newColors[myPolygons[i].V3] = new Color(192 * 0.004f, 192 * 0.004f, 192 * 0.004f);

                    oldColors[myPolygons[i].V1] = new Color(192 * 0.004f, 192 * 0.004f, 192 * 0.004f);
                    oldColors[myPolygons[i].V2] = new Color(192 * 0.004f, 192 * 0.004f, 192 * 0.004f);
                    oldColors[myPolygons[i].V3] = new Color(192 * 0.004f, 192 * 0.004f, 192 * 0.004f);
                    };
                if (myPolygons[i].color == 4160) // red
                    {
                    newColors[myPolygons[i].V1] = new Color(1, 0, 0);
                    newColors[myPolygons[i].V2] = new Color(1, 0, 0);
                    newColors[myPolygons[i].V3] = new Color(1, 0, 0);

                    oldColors[myPolygons[i].V1] = new Color(1, 0, 0);
                    oldColors[myPolygons[i].V2] = new Color(1, 0, 0);
                    oldColors[myPolygons[i].V3] = new Color(1, 0, 0);
                    };
                if (myPolygons[i].color == 4176) // orange
                    {
                    newColors[myPolygons[i].V1] = new Color(255 * 0.004f, 128 * 0.004f, 0);
                    newColors[myPolygons[i].V2] = new Color(255 * 0.004f, 128 * 0.004f, 0);
                    newColors[myPolygons[i].V3] = new Color(255 * 0.004f, 128 * 0.004f, 0);

                    oldColors[myPolygons[i].V1] = new Color(255 * 0.004f, 128 * 0.004f, 0);
                    oldColors[myPolygons[i].V2] = new Color(255 * 0.004f, 128 * 0.004f, 0);
                    oldColors[myPolygons[i].V3] = new Color(255 * 0.004f, 128 * 0.004f, 0);
                    };
                if (myPolygons[i].color == 4192) // yellow
                    {
                    newColors[myPolygons[i].V1] = new Color(215 * 0.004f, 215 * 0.004f, 0);
                    newColors[myPolygons[i].V2] = new Color(215 * 0.004f, 215 * 0.004f, 0);
                    newColors[myPolygons[i].V3] = new Color(215 * 0.004f, 215 * 0.004f, 0);

                    oldColors[myPolygons[i].V1] = new Color(215 * 0.004f, 215 * 0.004f, 0);
                    oldColors[myPolygons[i].V2] = new Color(215 * 0.004f, 215 * 0.004f, 0);
                    oldColors[myPolygons[i].V3] = new Color(215 * 0.004f, 215 * 0.004f, 0);
                    };
                if (myPolygons[i].color == 4224) // green
                    {
                    newColors[myPolygons[i].V1] = new Color(0, 1, 0);
                    newColors[myPolygons[i].V2] = new Color(0, 1, 0);
                    newColors[myPolygons[i].V3] = new Color(0, 1, 0);

                    oldColors[myPolygons[i].V1] = new Color(0, 1, 0);
                    oldColors[myPolygons[i].V2] = new Color(0, 1, 0);
                    oldColors[myPolygons[i].V3] = new Color(0, 1, 0);
                    };
                if (myPolygons[i].color == 4288) // blue
                    {
                    newColors[myPolygons[i].V1] = new Color(0, 0, 1);
                    newColors[myPolygons[i].V2] = new Color(0, 0, 1);
                    newColors[myPolygons[i].V3] = new Color(0, 0, 1);

                    oldColors[myPolygons[i].V1] = new Color(0, 0, 1);
                    oldColors[myPolygons[i].V2] = new Color(0, 0, 1);
                    oldColors[myPolygons[i].V3] = new Color(0, 0, 1);
                    };
                if (myPolygons[i].color == 4304) // purple
                    {
                    newColors[myPolygons[i].V1] = new Color(128 * 0.004f, 0, 255 * 0.004f);
                    newColors[myPolygons[i].V2] = new Color(128 * 0.004f, 0, 255 * 0.004f);
                    newColors[myPolygons[i].V3] = new Color(128 * 0.004f, 0, 255 * 0.004f);

                    oldColors[myPolygons[i].V1] = new Color(128 * 0.004f, 0, 255 * 0.004f);
                    oldColors[myPolygons[i].V2] = new Color(128 * 0.004f, 0, 255 * 0.004f);
                    oldColors[myPolygons[i].V3] = new Color(128 * 0.004f, 0, 255 * 0.004f);
                    };
                };

            // Scale down vertices.
            for (int i = 0; i < vertices; i++)
                {
                newVertices[i] *= vertexScale;
                };

            theReader.Close();
    



            // Assign elements to Unity mesh.
            theMesh.vertices = newVertices;
            //theMesh.normals = newNormals;
            //theMesh.uv = newUV;
            theMesh.triangles = newTriangleIndices;
            theMesh.colors = newColors;

            theLM2.GetComponent<MeshRenderer>().material = matWhite;

	        theMesh.RecalculateBounds();
	        theMesh.RecalculateNormals();

            modelLoaded = true;

            //Debug.Log(getTriangleFromBone());
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


                    // does this even work?
                    /*if (i != 0)
                        {
                        myFrames[i].bonesQuat[j] = myFrames[i - 1].bonesQuat[j];
                        };*/



                    myFrames[i].bonesType[j] = theReader.ReadInt16();

                    // very similar to the way we exported our animation, except we do the reverse and divide instead of multiply
                    /*myFrames[i].bonesX[j] = theReader.ReadInt16() / 11.333333f;
                    myFrames[i].bonesY[j] = theReader.ReadInt16() / 11.333333f;
                    myFrames[i].bonesZ[j] = theReader.ReadInt16() / 11.333333f;*/

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
                        
                        // https://forum.unity.com/threads/getting-objects-local-rotation-in-0-360-degrees-on-a-single-axis-consistently-without-flipping.374966/
                        //Vector3 forwardVector = bone.transform.rotation * Vector3.forward;
                        //float radianAngle = Mathf.Atan2(forwardVector.z, forwardVector.x);
                        //float degreeAngle = radianAngle * Mathf.Rad2Deg;

                        /*float rotX = (4096 - bone.transform.eulerAngles.x);
                        float rotY = (4096 - bone.transform.eulerAngles.y);
                        float rotZ = (4096 - bone.transform.eulerAngles.z);*/

                        /*float rotX =  RoundAngle2(bone.transform.eulerAngles.x) * 11.333333f;
                        float rotY =  RoundAngle2(bone.transform.eulerAngles.y) * 11.333333f;
                        float rotZ =  RoundAngle2(bone.transform.eulerAngles.z) * 11.333333f;*/

                        // best results thus far
                        //myBinaryWriter.Write((Int16) bone.GetComponent<Scr_Bone_Helper>().finalX); // bone x
                        //myBinaryWriter.Write((Int16) bone.GetComponent<Scr_Bone_Helper>().finalY); // bone y
                        //myBinaryWriter.Write((Int16) bone.GetComponent<Scr_Bone_Helper>().finalZ); // bone z

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