using Simulation.Controllers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Simulation.Objects
{
    public class Ball : MonoBehaviour
    {
        private Rigidbody _rigidBody;
        private SphereCollider _sphereCollider;


        [SerializeField] private bool _isPlayerBall;
        public bool isPlayerBall
        {
            get { return _isPlayerBall; }
        }
        [SerializeField] private int _ballId;
        public int ballId
        {
            get { return _ballId; }
        }
        
        private bool _isInSimulation;
        public bool isInSimulation
        {
            get { return _isInSimulation; }
            set { _isInSimulation = value; }
        }
        private bool _ballActive;
        public bool ballActive
        {
            get { return _ballActive; }
            set { _ballActive = value; }
        }
        private bool _isMoving;
        public bool isMoving
        {
            get { return _isMoving; }
        }

        private float minEnergy = 0.1f;

        private void Awake()
        {
            _rigidBody = GetComponent<Rigidbody>();
            _sphereCollider = GetComponent<SphereCollider>();
        }

        // Start is called before the first frame update
        void Start()
        {
            GameController.OnStartGame += OnStartGame;
            GameController.OnStartTurn += OnStartTurn;
            _isInSimulation = false;
            _ballActive = true;
        }

        public void ToggleMeshRenderer(bool isVisible)
        {
            this.GetComponent<MeshRenderer>().enabled = isVisible;
        }

        public void SetBallSimulation()
        {
            _isInSimulation = true;
           // ToggleMeshRenderer(false);
        }

        /// <summary>
        /// ボールに力を加わる
        /// </summary>
        /// <param name="direction">方向</param>
        /// <param name="power">力</param>
        public void ShootTo(Vector3 direction, float power)
        {
            Vector3 shotPower = direction * power;

            _rigidBody.AddForceAtPosition(shotPower, transform.position, ForceMode.Impulse);
        }

        /// <summary>
        /// ゲームのスタート時にこのコールバックを呼び出す
        /// </summary>
        private void OnStartGame()
        {
            this.gameObject.SetActive(true);
            _ballActive = true;
            _rigidBody.Sleep();
            _isMoving = false;
        }

        /// <summary>
        /// スタートターン時にこのコールバックを呼び出す
        /// </summary>
        private void OnStartTurn()
        {
            GameController.CustomDebug.Log("onstart turn ball id "+_ballId);
            _rigidBody.Sleep();//スタートターンは一応強制的にボール止まる
            _isMoving = false;
        }

        /// <summary>
        /// GameController のUpdate, 1フレームごと呼び出される
        /// </summary>
        public void OnUpdate()
        {
            //Debug.Log("ball ismoving"+_isMoving);
            //ball の velocity と angular velocity の magnitude を 指定した値より到達したら、強制的にストップ
            if (_ballActive && (_rigidBody.velocity.magnitude < minEnergy && _sphereCollider.radius * _rigidBody.angularVelocity.magnitude < minEnergy))
            {
                _rigidBody.Sleep();
                _isMoving = false;
            }

            if (!_rigidBody.IsSleeping() && !_rigidBody.velocity.Equals(Vector3.zero) && !_rigidBody.angularVelocity.Equals(Vector3.zero))
                _isMoving = true;
        }

    }

}
