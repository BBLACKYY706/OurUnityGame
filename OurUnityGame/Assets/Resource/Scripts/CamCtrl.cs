using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamCtrl : MonoBehaviour
{
    protected struct Broder
    {
        public int min, max;
        public Broder(int min, int max)
        {
            this.min = min;
            this.max = max;
        }
    }
    protected Broder BroderX, BroderY;
    public Transform player;
    void Start()
    {

    }
    void Update()
    {
        Vector3 pos = new Vector3(player.position.x, player.position.y, transform.position.z);
        if (!(pos.x >= BroderX.min && pos.x <= BroderX.max)) return;
        if (!(pos.y >= BroderY.min && pos.y <= BroderY.max)) return;
        transform.position = new Vector3(player.position.x, player.position.y, pos.z);
    }
}
