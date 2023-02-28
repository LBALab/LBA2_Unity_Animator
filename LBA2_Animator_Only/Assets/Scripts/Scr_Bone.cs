using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scr_Bone : MonoBehaviour
    {
    public bool selected;

    public int index;

    public float rotX;
    public float rotY;
    public float rotZ;

    /*public float rotXFinal;
    public float rotYFinal;
    public float rotZFinal;

    public float rotX360;
    public float rotY360;
    public float rotZ360;*/

    //public Quaternion rotQuat;

    private Scr_Controller controller;

    private Mesh theMesh;

    int[] triangleIndices = new int[6]{0, 1, 2, 3, 4, 5};

    // Start is called before the first frame update
    void Start()
        {
        controller = GameObject.Find("Controller").GetComponent<Scr_Controller>();

        //if (index == 3)
            //{
            /*gameObject.AddComponent<MeshCollider>();

            theMesh = new Mesh();
            gameObject.GetComponent<MeshCollider>().sharedMesh = theMesh;

            theMesh.vertices = controller.vertsBone3;
            theMesh.triangles = triangleIndices;

	        theMesh.RecalculateBounds();
	        theMesh.RecalculateNormals();*/
            //};
        }

    void checkAndSetAngle(int index1, int index2, int index3, float newX, float newY, float newZ)
        {
        /*if (rotX360 == Mathf.Ceil(controller.rotTable360[index1]) && 
            rotY360 == Mathf.Ceil(controller.rotTable360[index2]) && 
            rotZ360 == Mathf.Ceil(controller.rotTable360[index3]))
            transform.eulerAngles = new Vector3(newX, newY, newZ);*/
        }

    // Update is called once per frame
    void Update()
        {
        //transform.rotation = Quaternion.Euler(rotX, 0, -rotZ);
        //transform.rotation *= Quaternion.Euler(0, -rotY, 0);

        //transform.localRotation = Quaternion.Euler(Vector3.right * rotX);
        //transform.localRotation *= Quaternion.Euler(Vector3.up * rotY);

        if (index != 0)
        {
            //Vector3 newVec = new Vector3();
            //newVec = transform.parent.rotation.ToEulerAngles();
            //transform.localRotation *= transform.parent.localRotation;
        }

        // Clamp the X rotations.
        /*if (rotX < 0)
            rotX += 4090;
        if (rotX > 4096)
            rotX -= 4090;

        // Clamp the Y rotations.
        if (rotY < 0)
            rotY += 4090;
        if (rotY > 4096)
            rotY -= 4090;

        // Clamp the Z rotations.
        if (rotZ < 0)
            rotZ += 4090;
        if (rotZ > 4096)
            rotZ -= 4090;*/

        // Clamp the X rotations.
        /*if (rotX360 < 0)
            rotX360 += 359;
        if (rotX360 > 360)
            rotX360 -= 359;

        // Clamp the Y rotations.
        if (rotY360 < 0)
            rotY360 += 359;
        if (rotY360 > 360)
            rotY360 -= 359;

        // Clamp the Z rotations.
        if (rotZ360 < 0)
            rotZ360 += 359;
        if (rotZ360 > 360)
            rotZ360 -= 359;*/

        /*rotX = rotX360 * 11.333333f;
        rotY = rotY360 * 11.333333f;
        rotZ = rotZ360 * 11.333333f;*/



        //if (name == "bone_2")
        //{
            //transform.Rotate(new Vector3(0, 1, 0), 0.2f);
            //transform.localRotation = new Quaternion(0, 90, 0, 1);

            //transform.localRotation = Quaternion.Euler(10, 0, 0) * Quaternion.Euler(0, 90, 0);
            //transform.localRotation = Quaternion.Euler(transform.localRotation.x, transform.localRotation.y, transform.localRotation.z) * Quaternion.Euler(0, 0, 90);
            //transform.localRotation = Quaternion.Euler(transform.localRotation.x, transform.localRotation.y, transform.localRotation.z) * Quaternion.Euler(90, 90, 0);

            //transform.localRotation = GetRotation(angles);
        //}


        // convert to LBA2 scene units
        // the rotation values are mostly correct, except they're still off as seen in LBA2 model viewer
        // we can't just multiply by a negative number - it's more complicated than that
        // rotation order seems to be in euler angles (ZYX) and also seems to be dependent mostly on world space instead of local space - which is strange
        /*rotX = Mathf.Abs(transform.localEulerAngles.x * 11.333333f);
        rotY = -4096 + Mathf.Abs(transform.localEulerAngles.y * 11.333333f);
        rotZ = 4096 - Mathf.Abs(transform.localEulerAngles.z * 11.333333f);

        // values to be written to the file
        rotXFinal = Mathf.Abs(rotX);
        rotYFinal = Mathf.Abs(rotY);
        rotZFinal = Mathf.Abs(rotZ);*/

        // https://forum.unity.com/threads/how-to-rotate-object-around-one-axis.446638/
        //transform.localRotation *= Quaternion.AngleAxis(Time.deltaTime*5, GetRotation(transform.localEulerAngles) as Quaternion);



    if (selected == true)
            {
            if (controller.mouseDrag == true)
                {
                //rotX += controller.mousePosPrev.y * 0.1f;
                //rotY += controller.mousePosPrev.x * 0.1f;
                };
            };
        }

    private void OnMouseEnter()
        {
        if (Input.GetMouseButton(0))
            {
            //selected = !selected;
            selected = true;
            }
        }
    }