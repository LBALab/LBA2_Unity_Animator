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
                // go in order: Z, Y, X
                
                //GameObject.Find("bone_3").transform.rotation = Quaternion.Euler(0, 0, -rotZ);
                //GameObject.Find("bone_3").transform.rotation *= Quaternion.Euler(0, -rotY, 0);
                //GameObject.Find("bone_3").transform.rotation *= Quaternion.AngleAxis(rotX, new Vector3(Vector3.forward.magnitude, 0, 0));

                //GameObject.Find("bone_3").transform.rotation = Quaternion.AngleAxis(-rotZ, new Vector3(0, 1, 0));
                //GameObject.Find("bone_3").transform.rotation *= Quaternion.AngleAxis(-rotY, new Vector3(0, 0, 1));
                //GameObject.Find("bone_3").transform.rotation *= Quaternion.AngleAxis(rotX, Vector3.right);

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
                        // some animations are a bit funny looking with the lerp method
                        // might need to implement this another way
                        /*rotX = Mathf.Lerp(rotX, controller.myFrames[controller.currentFrame - 1].bonesX[index], controller.frameTime);
                        rotY = Mathf.Lerp(rotY, controller.myFrames[controller.currentFrame - 1].bonesY[index], controller.frameTime);
                        rotZ = Mathf.Lerp(rotZ, controller.myFrames[controller.currentFrame - 1].bonesZ[index], controller.frameTime);*/

                        if (index != 1)
                            {
                            //if (controller.currentFrame < controller.frames) // slerp to next frame
                                //{
                                boneQuat = Quaternion.Euler(Vector3.right * controller.myFrames[controller.currentFrame - 1].bonesX[index]);
                                boneQuat *= Quaternion.Euler(Vector3.back * controller.myFrames[controller.currentFrame - 1].bonesZ[index]);
                                boneQuat *= Quaternion.Euler(Vector3.down * controller.myFrames[controller.currentFrame - 1].bonesY[index]);
                                //}
                            //else if (controller.currentFrame == controller.frames) // slerp to frame loop
                                //{
                                //boneQuat = Quaternion.Euler(Vector3.right * controller.myFrames[controller.frameLoop - 1].bonesX[index]);
                                //boneQuat *= Quaternion.Euler(Vector3.back * controller.myFrames[controller.frameLoop - 1].bonesZ[index]);
                                //boneQuat *= Quaternion.Euler(Vector3.down * controller.myFrames[controller.frameLoop - 1].bonesY[index]);

                                //Debug.Log("hey");

                                //boneQuat = Quaternion.Euler(Vector3.right * controller.myFrames[controller.frameLoop].bonesX[index]);
                                //boneQuat *= Quaternion.Euler(Vector3.back * controller.myFrames[controller.frameLoop].bonesZ[index]);
                                //boneQuat *= Quaternion.Euler(Vector3.down * controller.myFrames[controller.frameLoop].bonesY[index]);

                                /*if (controller.frameLoop != 0)
                                    {
                                    boneQuat = Quaternion.Euler(Vector3.right * controller.myFrames[controller.frameLoop].bonesX[index]);
                                    boneQuat *= Quaternion.Euler(Vector3.back * controller.myFrames[controller.frameLoop].bonesZ[index]);
                                    boneQuat *= Quaternion.Euler(Vector3.down * controller.myFrames[controller.frameLoop].bonesY[index]);
                                    }
                                else
                                    {
                                    boneQuat = Quaternion.Euler(Vector3.right * controller.myFrames[controller.frameLoop + 1].bonesX[index]);
                                    boneQuat *= Quaternion.Euler(Vector3.back * controller.myFrames[controller.frameLoop + 1].bonesZ[index]);
                                    boneQuat *= Quaternion.Euler(Vector3.down * controller.myFrames[controller.frameLoop + 1].bonesY[index]);
                                    }*/
                                //}

                            //theBone.transform.localRotation = Quaternion.Slerp(theBone.transform.localRotation, boneQuat, 0.1f);
                            //theBone.transform.localRotation = Quaternion.Slerp(theBone.transform.localRotation, boneQuat, controller.frameTime);
                            theBone.transform.localRotation = Quaternion.Slerp(theBone.transform.localRotation, boneQuat,
                                                                               (controller.frameTime + controller.myFrames[controller.currentFrame - 1].speed) * controller.frameScale);

                            //theBone.transform.localRotation = Quaternion.Slerp(theBone.transform.localRotation, boneQuat,
                            //normalizeValue(controller.myFrames[controller.currentFrame - 1].speed, 0, 4096));

                            /*theBone.transform.localRotation = Quaternion.Slerp(theBone.transform.localRotation, boneQuat,
                            controller.frameTime + normalizeValue((short)GameObject.Find("Slider_Frame_Speed").GetComponent<Slider>().value, 0, 4096) * 1.5f);*/

                            //theBone.transform.localRotation = Quaternion.Slerp(theBone.transform.localRotation, boneQuat, bonePos);
                            //bonePos = normalizeValue((short)GameObject.Find("Slider_Frame_Speed").GetComponent<Slider>().value, 0, 4096) * Time.deltaTime;

                            // best results thus far
                            //if (controller.currentFrame < controller.frames)
                                //{
                                /*}
                            else
                                {
                                if (controller.frameLoop == 0)
                                theBone.transform.localRotation = Quaternion.Slerp(theBone.transform.localRotation, boneQuat,
                                                                               Time.deltaTime / (controller.myFrames[controller.frameLoop].speed * controller.frameScale));
                                else
                                theBone.transform.localRotation = Quaternion.Slerp(theBone.transform.localRotation, boneQuat,
                                                                               Time.deltaTime / (controller.myFrames[controller.frameLoop - 1].speed * controller.frameScale));
                                }*/
                            
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
