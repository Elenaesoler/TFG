﻿using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Windows.Kinect;
using Joint = Windows.Kinect.Joint;

public class BodySourceView : MonoBehaviour
{
    public BodySourceManager mBodySourceManager;
    public GameObject mJointObject;

    private static List<Vector3> coordenadaMano = new List<Vector3>();  //esta lista almacena las posiciones de la mano
    public Vector3 targetPosition;
    private static bool grabar=false;
    public GameObject botonGrabar;
    public GameObject botonFin;
    //public GameObject botonJugar;

    public List<Vector3> coordenadaResultMano = new List<Vector3>(xPartVector, yPartVector, zPartVector);

    private Dictionary<ulong, GameObject> mBodies = new Dictionary<ulong, GameObject>();
    private List<JointType> _joints = new List<JointType>
    {
        JointType.HandLeft,
        JointType.HandRight,
        JointType.Head,             
        JointType.ShoulderRight,  
        JointType.ShoulderLeft,   
        //JointType.HipLeft,        
        //JointType.HipRight,       

    };
   
    void Update()
    {
        #region Get Kinect data
        Body[] data = mBodySourceManager.GetData();
        if (data == null)
            return;

        List<ulong> trackedIds = new List<ulong>();
        foreach (var body in data)
        {
            if (body == null)
                continue;

            if (body.IsTracked)
                trackedIds.Add(body.TrackingId);
        }
        #endregion

        #region Delete Kinect bodies
        List<ulong> knownIds = new List<ulong>(mBodies.Keys);
        foreach (ulong trackingId in knownIds)
        {
            if (!trackedIds.Contains(trackingId))
            {
                // Destroy body object
                Destroy(mBodies[trackingId]);

                // Remove from list
                mBodies.Remove(trackingId);
            }
        }
        #endregion

        #region Create Kinect bodies
        foreach (var body in data)
        {
            // If no body, skip
            if (body == null)
                continue;

            if (body.IsTracked)
            {
                // If body isn't tracked, create body
                if (!mBodies.ContainsKey(body.TrackingId))
                    mBodies[body.TrackingId] = CreateBodyObject(body.TrackingId);

                // Update positions
                UpdateBodyObject(body, mBodies[body.TrackingId]);
            }
        }
        #endregion

    }

    public void CallbackBotonGrabar()
    {
        grabar = true;
        Debug.Log("Comienza a grabar el ejercicio y desaparece el boton'Grabar' ");
        botonGrabar.SetActive(false);
        //botonJugar.SetActive(false);
        botonFin.SetActive(true);
    }

    public void CallbackBotonFin()
    {
        grabar = false;
        Debug.Log("Finaliza el ejercicio y desaparece el boton'Finalizar' ");
        botonFin.SetActive(false);
        botonGrabar.SetActive(true);
        //botonJugar.SetActive(true);
    }

    private GameObject CreateBodyObject(ulong id)
    {
        Debug.Log("Dentro de create body");
        // Create body parent
        GameObject body = new GameObject("Body:" + id);

        // Create joints
        foreach (JointType joint in _joints)    
        {
            // Create Object
            GameObject newJoint = Instantiate(mJointObject);  //
            newJoint.name = joint.ToString();  //donde pone JointType.HandRight antes habia variable joint del foreach

            // Parent to body
            newJoint.transform.parent = body.transform;
        }

        return body;
    }
    
    private void UpdateBodyObject(Body body, GameObject bodyObject)
    {
        Debug.Log("grabar-->"+ grabar);
        // Update joints
        foreach (JointType _joint in _joints) //por cada joint de la lista de arriba
        {
            // Get new target position
            Joint sourceJoint = body.Joints[_joints]; //_joint por JointType.HandRight
            targetPosition = GetVector3FromJoint(sourceJoint);
            targetPosition.z = 0;  // posicion z=0 SIEMPRE para que la pompa y la mano esten en 2D

            // Get joint, set new position
            Transform jointObject = bodyObject.transform.Find(_joint.ToString());
            jointObject.position = targetPosition;

        }
        if (grabar == true)
        {
            Debug.Log("empzar a grabar");
            GrabarEjercicio();
        }
}

    public void GrabarEjercicio()
    {
        Debug.Log("Dentro de grabar ejercicio");

        #region Calculo de la distancia entre coordenada actual y coordenada anterior
        //calculo de la distancia entre la posicion actual de la mano y la anterior.
        //al inicio no existe ninguna coordenada coordenadaMano.Count ==0 entonces añade la primera 
        //despues, es cuando comienza a calcular distancias. 

        if (coordenadaMano.Count == 0)
        {
            coordenadaMano.Add(JointType.HandRight.targetPosition);
            return;
        }
        //Debug.Log(coordenadaMano.Count); //imprimir comprobacion 
        //Debug.Log(targetPosition); //imprimir comprobacion 

        float dist = Vector3.Distance(JointType.HandRight.targetPosition, coordenadaMano[coordenadaMano.Count - 1]);
        //Debug.Log(dist);
        #endregion

        #region Imprime y anyade coordenda a la lista de coordenadas almacenadas en coordenadaMano si la distancia es mayor a 2,6(por ejemplo)

        if (dist >= 0.1)    //si la distancia entre la posicion actual (targetposition) y la anterior es mayr o igual a 0,6, guardala como nueva coordenada de la mano
        {
            coordenadaMano.Add(JointType.HandRight.targetPosition);

            //Debug.Log("targetposition " + targetPosition); //imprimir comprobacion 
            foreach (Vector3 coordenadaGuardada in coordenadaMano)
            {
                //Debug.Log("coordenadaMano " + coordenadaGuardada);
            }

            Debug.Log("Dist" + dist); //imprimir comprobacion
            Debug.Log(coordenadaMano.Count);  //imprimir comprobacion
         }
        #endregion
    }

    private Vector3 GetVector3FromJoint(Joint joint)
    {
        return new Vector3(joint.Position.X * 10, joint.Position.Y * 10, joint.Position.Z * 10);
    }

    #region Dividir la lista coordenadasMano en partes y coger la media de cada parte
    public void promedCoordinates()
    {
        if(coordenadaMano.Count != 0)
        {
            short numPartes = 5;
            int tamanyoParte = coordenadaMano.Count / numPartes;

            List<Vector3> coordenadaResultPromed = new List<Vector3>();

            for (int parte = 0; parte < numPartes; parte++)
            {
                float xParte = 0.0f, yParte = 0.0f, zParte = 0.0f;  //Agregue "f" al final para decirle al compilador que es un flotante

                for (int i = 0; i < tamanyoParte; i++)
                {
                    xParte += coordenadaMano[i + parte * tamanyoParte].x;
                    yParte += coordenadaMano[i + parte * tamanyoParte].y;
                    zParte += coordenadaMano[i + parte * tamanyoParte].z;
                }

                          
                float xPartVector = xParte / tamanyoParte;
                float yPartVector = yParte / tamanyoParte;
                float zPartVector = zParte / tamanyoParte;

                
                Debug.Log("Parte" + parte + "coordenada X" + xPartVector);
                Debug.Log("Parte" + parte + "coordenada Y" + yPartVector);
                Debug.Log("Parte" + parte + "coordenada Z" + zPartVector);

                coordenadaResultMano.Add(xPartVector, yPartVector, zPartVector);
                             
            }
            Debug.Log(coordenadaResultPromed);
        }
    }
    #endregion

    #region crear txt con las coordenadas
    public void FileGrabacion()
    {
        using (StreamWriter output = new StreamWriter("Archivo.txt", true))
        {
            foreach (Vector3 argumento in coordenadaResultMano)
                string argumentoX = argumento.X.ToString();
                string argumentoY = argumento.Y.ToString();
                string argumentoZ = argumento.Z.ToString();

            output.WriteLine(argumentoX, argumentoY, argumentoZ);
            Debug.Log("Se ha creado el archivo y añadido " + argumento);
        }
    }
#endregion
}

//crear los vectores de hombro-mano, hombro-hombro, cadera-cadera y de cadera-hombro
//con hombro-cadera y hombro-mano sacamos el angulo que forman 
//con hombro-hombro y cadera-cadera comprobar que se mantienen paralelas o con igual angulo que el inicial

////bucle que meta cada coordenadaMano en la variable coordinates
//Vector3 coordinates = coordinates.Add(coordenadaMano);
//System.IO.File.WriteAllLines(@"C:\Users\Nena\Desktop\TFG\WriteText.txt", coordinates);
//Debug.Log(coordinates);
