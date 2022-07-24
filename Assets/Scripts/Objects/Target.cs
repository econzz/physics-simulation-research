using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Simulation.Objects
{
    public class Target : MonoBehaviour
    {
        public Action<Target,Ball> OnTargetHit;
        [SerializeField] private int _targetId;

        // Start is called before the first frame update
        void Start()
        {

        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<Ball>())
            {
                Ball hitBall = other.GetComponent<Ball>();
                
                OnTargetHit?.Invoke(this,hitBall);
            }
        }
    }

}
