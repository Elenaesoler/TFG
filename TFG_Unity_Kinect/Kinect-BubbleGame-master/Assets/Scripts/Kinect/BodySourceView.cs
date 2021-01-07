﻿using UnityEngine;
//using UnityEditor.UIElements.UIElementsEditor;
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

    //
    //public int interpolationFramesCount = 180; //numero de frames para interpolar dos posiciones
    //int elapsedFrames = 0;

    public List<Vector3> coordenadaMano = new List<Vector3>();  //esta lista almacena la posicion de la mano

    //crear los vectores de hombro-mano, hombro-hombro, cadera-cadera y de cadera-hombro
    //con hombro-cadera y hombro-mano sacamos el angulo que forman 
    //con hombro-hombro y cadera-cadera comprobar que se mantienen paralelas o con igual angulo que el inicial


    // 
    //timestamp para iniciar la grabacion en un instante y cuando pasen 10seg por ejemplo, que pare de grabar. 
    //que coja la coordenada cada 30 frames por ejemplo
    // que coja t=0 t=0+30 t=0+60 ... y pare a los 10 segundos. con un while(timestamp 

    private Dictionary<ulong, GameObject> mBodies = new Dictionary<ulong, GameObject>();
    private List<JointType> _joints = new List<JointType>
    {
        JointType.HandLeft,
        JointType.HandRight,
        JointType.Head,             //añadido
        JointType.ShoulderRight,  //añadido
        JointType.ShoulderLeft,   //añadido
        JointType.HipLeft,        //añadido
        JointType.HipRight,       //añadido

    };
   
    //void start()
    //{
    //    //meter timeStamp
    //    bool modoCaptura = true;
    //    String timeStamp = GetTimestamp(DateTime.Now);
    //}
    void Update()
    {
        Debug.Log("Dentro de update() -- BodySourceView");
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

        
    private GameObject CreateBodyObject(ulong id)
    {
        // Create body parent
        Debug.Log("Drntro de create body");
        GameObject body = new GameObject("Body:" + id);

        // Create joints
        //foreach (JointType joint in _joints)    
        //{
            // Create Object
            GameObject newJoint = Instantiate(mJointObject);  //
            newJoint.name = JointType.HandRight.ToString();  //donde pone JointType.HandRight antes habia variable joint del foreach

            // Parent to body
            newJoint.transform.parent = body.transform;
        //}

        return body;
    }
    
    private void UpdateBodyObject(Body body, GameObject bodyObject)
    {
        // Update joints
        //foreach (JointType _joint in _joints)
        // {
        Debug.Log("Estoy en update body");
             // Get new target position
         Joint sourceJoint = body.Joints[JointType.HandRight]; //_joint por JointType.HandRight
        Vector3 targetPosition = GetVector3FromJoint(sourceJoint);
        targetPosition.z = 0;  // posicion z=0 SIEMPRE para que la pompa y la mano esten en la misma coordenada en 3D

        Debug.Log("Estoy en update -- grabar");

        //string _timeStamp = GetTimeStamp();
        // Get joint, set new position
        Transform jointObject = bodyObject.transform.Find(JointType.HandRight.ToString()); //el cuerpo que reconoce 
        jointObject.position = targetPosition;

        if (coordenadaMano.Count == 0)
        {
            coordenadaMano.Add(targetPosition);
            return;
        }

        Debug.Log(coordenadaMano.Count);

        #region distancia entre coordenada actual y coordenada anterior
        //calculo de la distancia entre la posicion actual de la mano y la anterior.
        //al inicio no existe ninguna coordenada coordenadaMano.Count ==0 entonces añade la primera 
        //despues, es cuando comienza a calcular distancias. 

        float dist = Vector3.Distance(targetPosition, coordenadaMano[coordenadaMano.Count - 1]);

       // if(dist < 0.01) //mientras la mano no se quede quieta, que la distacia se mayor a esta por ejemplo (0.01)
        

            if (dist >= 0.6 )    //si la distancia entre la posicion actual (targetposition) y la anterior es mayr o igual a 0,6, guardala como nueva coordenada de la mano
            { 
                coordenadaMano.Add(targetPosition);
          
                Debug.Log("targetposition "+ targetPosition); 
                foreach (Vector3 coordenadaGuardada in coordenadaMano)
                {
                    Debug.Log("coordenadaMano " + coordenadaGuardada);
                }
            
         
                Debug.Log("Dist"+ dist);
                Debug.Log(coordenadaMano.Count);
            #endregion
        }
        //Debug.Log(targetPosition); //imprime en consola las coordenadas de los joints que tengo en _joints
        
       
        #region crear txt con las coordenadas

        //coordenadaMano.ToString();  //las coordenads de tipo vector3 las paso  string para usarlas en el txt
        //TextWriter tw = new StreamWriter("archivoNuevo.txt");
        //tw.WriteLine(List.coordenadaMano);

        ////bucle que meta cada coordenadaMano en la variable coordinates
        //Vector3 coordinates = coordinates.Add(coordenadaMano);
        //System.IO.File.WriteAllLines(@"C:\Users\Nena\Desktop\TFG\WriteText.txt", coordinates);
        //Debug.Log(coordinates);
        #endregion
    }



    private Vector3 GetVector3FromJoint(Joint joint)
    {
        return new Vector3(joint.Position.X * 10, joint.Position.Y * 10, joint.Position.Z * 10);
    }
}
