using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Simulation.Objects
{
    public class Direction : MonoBehaviour
    {
        public static Action<float> OnAngleChanged;
        public static Action<float> OnPowerChanged;

        [SerializeField] private GameObject positionDetector;//どこに向かっているのかため
        private float _angle = 0;
        private float _power;
        public float power { get { return _power; } set { _power = value; } }
        // Start is called before the first frame update
        void Start()
        {
            SetAngle(_angle);
            power = 2;
        }

        /// <summary>
        /// direction を指定した場所に移動する
        /// </summary>
        /// <param name="position">位置</param>
        public void SetPositionTo(Vector3 position)
        {
            this.transform.position = position;
        }

        /// <summary>
        /// 現在の位置のgameobject獲得
        /// </summary>
        /// <returns>transform</returns>
        public Transform GetPositionDetector()
        {
            return positionDetector.transform;
        }

        /// <summary>
        /// 角度追加
        /// </summary>
        /// <param name="angle"></param>
        public void AddAngle(int angle = 10)
        {
            _angle += angle;
            SetAngle(_angle);
        }

        /// <summary>
        /// 角度追加
        /// </summary>
        /// <param name="power"></param>
        public void AddPower(float power = 0.5f)
        {
            _power += power;
            OnPowerChanged?.Invoke(_power);
        }

        /// <summary>
        /// 指定した角度に回転する
        /// </summary>
        /// <param name="angle">角度</param>
        public void SetAngle(float angle)
        {
            Quaternion rotation = this.transform.rotation;
            rotation.eulerAngles = new Vector3(90, 0, angle);
            this.transform.rotation = rotation;

            OnAngleChanged?.Invoke(GetAngle());
        }

        /// <summary>
        /// 角度ゲット
        /// </summary>
        /// <returns></returns>
        public float GetAngle()
        {
            return _angle;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
    
