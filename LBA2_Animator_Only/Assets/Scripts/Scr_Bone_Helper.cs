using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scr_Bone_Helper : MonoBehaviour
{
    public int index;
    public bool selected;

    public float rotX;
    public float rotY;
    public float rotZ;

    public short finalX;
    public short finalY;
    public short finalZ;

    private float bonePos;

    private Scr_Controller controller;

    // Start is called before the first frame update
    void Start()
    {
        controller = GameObject.Find("Controller").GetComponent<Scr_Controller>();
    }

    float normalizeValue(float value, float minValue, float maxValue)
    {
        float scaledValue = Mathf.Clamp(value / maxValue, minValue, maxValue);

        return scaledValue;
    }

    // Update is called once per frame
    void Update()
    {
        if (name == "bone_helper_" + index.ToString())
            {
                GameObject theBone = GameObject.Find("bone_" + index.ToString());
                Quaternion boneQuat = theBone.transform.localRotation;
                    

                if (index == 0)
                    {
                    if (controller.animMovement == false)
                        {
                        // stay in place, but show movement vector
                        /*theBone.transform.position = new Vector3(controller.myFrames[controller.currentFrame - 1].moveX * controller.vertexScale,
                                                                 controller.myFrames[controller.currentFrame - 1].moveY * controller.vertexScale,
                                                                 controller.myFrames[controller.currentFrame - 1].moveZ * controller.vertexScale);*/
                        
                        /*theBone.transform.position = Vector3.Lerp(theBone.transform.position, new Vector3(controller.myFrames[controller.currentFrame - 1].moveX * controller.vertexScale,
                                                                                                          controller.myFrames[controller.currentFrame - 1].moveY * controller.vertexScale,
                                                                                                          controller.myFrames[controller.currentFrame - 1].moveZ * controller.vertexScale),
                                                                                                          (controller.frameTime + controller.myFrames[controller.currentFrame - 1].speed) * controller.frameScale);*/
                        }
                    else
                        {
                        // keep moving along the vector
                        /*theBone.transform.Translate(controller.myFrames[controller.currentFrame - 1].moveX * (controller.vertexScale * (controller.myFrames[controller.currentFrame - 1].speed * controller.frameScale)),
                                                    controller.myFrames[controller.currentFrame - 1].moveY * (controller.vertexScale * (controller.myFrames[controller.currentFrame - 1].speed * controller.frameScale)),
                                                    controller.myFrames[controller.currentFrame - 1].moveZ * (controller.vertexScale * (controller.myFrames[controller.currentFrame - 1].speed * controller.frameScale)));*/
                        };
                    };

                // best one thus far
                if (index != 0)
                    {
                    if (index != 1)
                        {
                        if (controller.animPlaying == false)
                            {
                            theBone.transform.localRotation = Quaternion.Euler(Vector3.right * rotX);
                            theBone.transform.localRotation *= Quaternion.Euler(Vector3.back * rotZ);
                            theBone.transform.localRotation *= Quaternion.Euler(Vector3.down * rotY);
                            };
                        
                        // get bone's localRotation from next frame so we can smoothly lerp from one to the other
                        //nextFrameQuat = controller.myFrames[controller.currentFrame].bonesQuat[index];
                        //theBone.transform.localRotation = nextFrameQuat;

                        //theBone.transform.localRotation = Quaternion.Euler(Vector3.right * rotX);
                        //theBone.transform.localRotation *= Quaternion.Euler(Vector3.back * rotZ);
                        //theBone.transform.localRotation *= Quaternion.Euler(Vector3.down * rotY);
                        };
                    
                    if (controller.animPlaying == true)
                        {

                        if (index != 1)
                            {
                                boneQuat = Quaternion.Euler(Vector3.right * controller.myFrames[controller.currentFrame - 1].bonesX[index]);
                                boneQuat *= Quaternion.Euler(Vector3.back * controller.myFrames[controller.currentFrame - 1].bonesZ[index]);
                                boneQuat *= Quaternion.Euler(Vector3.down * controller.myFrames[controller.currentFrame - 1].bonesY[index]);

                            theBone.transform.localRotation = Quaternion.Slerp(theBone.transform.localRotation, boneQuat,
                                                                               (controller.frameTime + controller.myFrames[controller.currentFrame - 1].speed) * controller.frameScale);
                            
                            };

                        rotX = controller.myFrames[controller.currentFrame - 1].bonesX[index];
                        rotY = controller.myFrames[controller.currentFrame - 1].bonesY[index];
                        rotZ = controller.myFrames[controller.currentFrame - 1].bonesZ[index];
                        };

                    finalX = (short)(rotX * controller.rotationScale);
                    finalY = (short)(rotY * controller.rotationScale);
                    finalZ = (short)(rotZ * controller.rotationScale);
                    };
            }
        }
    }
