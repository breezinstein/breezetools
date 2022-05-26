using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections;

namespace Breezinstein.Tools
{
    public class BreezeToolBox : EditorWindow
    {
        //rotation
        bool randomRotateGroup;
        bool rotX, rotY, rotZ;
        Vector3 minRot, maxRot;

        //distribution
        bool distributeGroup;

        //position
        bool randomPositionGroup;
        bool posX, posY, posZ;
        Vector3 minPos, maxPos;


        //Replace with Gameobject
        GameObject replaceWithThis;
        bool replaceGroup;

        Font replaceFont;
        bool replaceFontGroup;
        // Add menu named "My Window" to the Window menu
        [MenuItem("Breeze Tools/Toolbox")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            BreezeToolBox window = (BreezeToolBox)EditorWindow.GetWindow(typeof(BreezeToolBox));
            window.Show();
        }

        void OnGUI()
        {
            GUILayout.Label("Breeze's Toolbox", EditorStyles.boldLabel);

            #region RANDOM ROTATE
            randomRotateGroup = EditorGUILayout.Foldout(randomRotateGroup, "Random Rotation");
            if (randomRotateGroup)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("x");
                rotX = EditorGUILayout.Toggle(rotX);
                GUILayout.Label("y");
                rotY = EditorGUILayout.Toggle(rotY);
                GUILayout.Label("z");
                rotZ = EditorGUILayout.Toggle(rotZ);
                EditorGUILayout.EndHorizontal();
                minRot = EditorGUILayout.Vector3Field("Minimum Rotation", minRot);
                maxRot = EditorGUILayout.Vector3Field("Maximum Rotation", maxRot);

                if (GUILayout.Button("Apply Rotation"))
                {
                    //Get Selections
                    Transform[] selectedTransforms = Selection.transforms;
                    //Apply Rotations
                    foreach (Transform t in selectedTransforms)
                    {
                        Vector3 rot = t.rotation.eulerAngles;
                        if (rotX) { rot.x = Random.Range(minRot.x, maxRot.x); }
                        if (rotY) { rot.y = Random.Range(minRot.y, maxRot.y); }
                        if (rotZ) { rot.z = Random.Range(minRot.z, maxRot.z); }
                        t.rotation = Quaternion.Euler(rot);
                    }
                }
                //EditorGUILayout.EndToggleGroup();
            }
            #endregion

            #region RANDOM POSITION
            randomPositionGroup = EditorGUILayout.Foldout(randomPositionGroup, "Random Position");
            if (randomPositionGroup)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("x");
                posX = EditorGUILayout.Toggle(posX);
                GUILayout.Label("y");
                posY = EditorGUILayout.Toggle(posY);
                GUILayout.Label("z");
                posZ = EditorGUILayout.Toggle(posZ);
                EditorGUILayout.EndHorizontal();
                minPos = EditorGUILayout.Vector3Field("Minimum Position", minPos);
                maxPos = EditorGUILayout.Vector3Field("Maximum Position", maxPos);

                if (GUILayout.Button("Apply Position"))
                {
                    //Get Selections
                    Transform[] selectedTransforms = Selection.transforms;
                    //Apply Position
                    foreach (Transform t in selectedTransforms)
                    {
                        Vector3 pos = t.localPosition;
                        if (posX) { pos.x = Random.Range(minPos.x, maxPos.x); }
                        if (posY) { pos.y = Random.Range(minPos.y, maxPos.y); }
                        if (posZ) { pos.z = Random.Range(minPos.z, maxPos.z); }
                        t.localPosition = pos;
                    }
                }
            }
            #endregion

            //Distribute Along an Axis
            #region DISTRIBUTE
            distributeGroup = EditorGUILayout.Foldout(distributeGroup, "Distribute");
            if (distributeGroup)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("X Array"))
                {
                    //Get Selections
                    Transform[] selectedTransforms = Selection.transforms;
                    //Get minimum and maximum values
                    float min = selectedTransforms[0].position.x;
                    float max = selectedTransforms[0].position.x;

                    foreach (Transform t in selectedTransforms)
                    {
                        if (t.position.x < min)
                        {
                            min = t.position.x;
                        }
                        else if (t.position.x > max)
                        {
                            max = t.position.x;
                        }
                    }
                    //reposition transforms
                    float increment = (max - min) / (float)(selectedTransforms.Length - 1);
                    for (int i = 0; i < selectedTransforms.Length; i++)
                    {
                        selectedTransforms[i].position = new Vector3(min + (increment * i), selectedTransforms[i].position.y, selectedTransforms[i].position.z);
                    }

                }
                if (GUILayout.Button("Y Array"))
                {
                    //Get Selections
                    Transform[] selectedTransforms = Selection.transforms;
                    //Get minimum and maximum values
                    float min = selectedTransforms[0].position.y;
                    float max = selectedTransforms[0].position.y;

                    foreach (Transform t in selectedTransforms)
                    {
                        if (t.position.y < min)
                        {
                            min = t.position.y;
                        }
                        else if (t.position.y > max)
                        {
                            max = t.position.y;
                        }
                    }
                    //reposition transforms
                    float increment = (max - min) / (float)(selectedTransforms.Length - 1);
                    for (int i = 0; i < selectedTransforms.Length; i++)
                    {
                        selectedTransforms[i].position = new Vector3(selectedTransforms[i].position.x, min + (increment * i), selectedTransforms[i].position.z);
                    }
                }

                if (GUILayout.Button("Z Array"))
                {
                    //Get Selections
                    Transform[] selectedTransforms = Selection.transforms;
                    //Get minimum and maximum values
                    float min = selectedTransforms[0].position.z;
                    float max = selectedTransforms[0].position.z;

                    foreach (Transform t in selectedTransforms)
                    {
                        if (t.position.z < min)
                        {
                            min = t.position.z;
                        }
                        else if (t.position.z > max)
                        {
                            max = t.position.z;
                        }
                    }
                    //reposition transforms
                    float increment = (max - min) / (float)(selectedTransforms.Length - 1);
                    for (int i = 0; i < selectedTransforms.Length; i++)
                    {
                        selectedTransforms[i].position = new Vector3(selectedTransforms[i].position.x, selectedTransforms[i].position.y, min + (increment * i));
                    }
                }
                EditorGUILayout.EndHorizontal();

            }
            #endregion
            #region REPLACE
            replaceGroup = EditorGUILayout.Foldout(replaceGroup, "Replace Objects");
            if (replaceGroup)
            {
                EditorGUILayout.BeginHorizontal();
                replaceWithThis = EditorGUILayout.ObjectField(replaceWithThis, typeof(GameObject), true) as GameObject;
                if (replaceWithThis != null)
                {
                    if (GUILayout.Button("Replace"))
                    {
                        //Get Selections
                        GameObject[] selectedGameObjects = Selection.gameObjects;
                        for (int i = 0; i < selectedGameObjects.Length; i++)
                        {
                            if (selectedGameObjects[i].transform.parent != null)
                            {
                                GameObject temp = Instantiate(replaceWithThis, selectedGameObjects[i].transform.position, selectedGameObjects[i].transform.rotation, selectedGameObjects[i].transform.parent) as GameObject;
                                Undo.RegisterCreatedObjectUndo(temp, temp.name + i);
                            }
                            else
                            {
                                Instantiate(replaceWithThis, selectedGameObjects[i].transform.position, selectedGameObjects[i].transform.rotation);
                            }
                            Undo.DestroyObjectImmediate(selectedGameObjects[i]);
                        }

                    }
                }
                EditorGUILayout.EndHorizontal();

            }
            #endregion

            #region REPLACEFont
            replaceFontGroup = EditorGUILayout.Foldout(replaceFontGroup, "Replace Font");
            if (replaceFontGroup)
            {
                EditorGUILayout.BeginHorizontal();
                replaceFont = EditorGUILayout.ObjectField(replaceFont, typeof(Font), true) as Font;
                if (replaceFont != null)
                {
                    if (GUILayout.Button("Replace Font"))
                    {
                        //Get Selections
                        GameObject[] selectedGameObjects = Selection.gameObjects;
                        for (int i = 0; i < selectedGameObjects.Length; i++)
                        {
                            if (selectedGameObjects[i].GetComponent<Text>() != null)
                            { selectedGameObjects[i].GetComponent<Text>().font = replaceFont; }
                            if (selectedGameObjects[i].GetComponent<Text>() != null)
                            { selectedGameObjects[i].GetComponent<Text>().font = replaceFont; }
                        }

                    }
                }
                EditorGUILayout.EndHorizontal();

            }
            #endregion
        }
    }
}