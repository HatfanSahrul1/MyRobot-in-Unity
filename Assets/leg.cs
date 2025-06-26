using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class leg : MonoBehaviour
{
    public GameObject joint;

    void Update(){
        if(joint != null){
            joint.transform.position = new Vector3(joint.transform.position.x, 2.5f, joint.transform.position.z);
        }
    }
}
