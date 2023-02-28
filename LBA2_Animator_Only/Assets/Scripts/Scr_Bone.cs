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

    private Scr_Controller controller;

    private Mesh theMesh;

    int[] triangleIndices = new int[6]{0, 1, 2, 3, 4, 5};

    // Start is called before the first frame update
    void Start()
        {
        controller = GameObject.Find("Controller").GetComponent<Scr_Controller>();
        }

    // Update is called once per frame
    void Update()
        {
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