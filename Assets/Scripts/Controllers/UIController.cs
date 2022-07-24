using Simulation.Objects;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Simulation.Controllers
{
    public class UIController : MonoBehaviour
    {
        public static Action OnStartSimulationButton;
        public static Action OnAddAngleButton;
        public static Action OnAddPowerButton;
        public static Action OnShootButton;

        [SerializeField] private Text _situationText;
        [SerializeField] private Text _currentAngleText;
        [SerializeField] private Text _currentPowerText;

        // Start is called before the first frame update
        void Start()
        {
            PhysicsSimulationController.OnSimulationComplete += OnSimulationComplete;

            Direction.OnAngleChanged += OnAngleChanged;
            Direction.OnPowerChanged += OnPowerChanged;
        }

        /// <summary>
        /// Directionの角度を変更するたびに呼び出される
        /// </summary>
        /// <param name="angle">角度</param>
        private void OnAngleChanged(float angle)
        {
            _currentAngleText.text = "Current Angle: " + angle;
        }

        /// <summary>
        /// Directionのパワーを変更するたびに呼び出される
        /// </summary>
        /// <param name="angle">パワー</param>
        private void OnPowerChanged(float power)
        {
            _currentPowerText.text = "Current Power: " + power;
        }

        /// <summary>
        /// シミュレーション完了時に呼び出される関数
        /// </summary>
        /// <param name="bestAngle">simulationによって一番いい結果の角度</param>
        /// <param name="power">simulationによって一番いい結果のパワー</param>
        /// <param name="hittedAmount">このsimulation結果で、ボールの入れた数</param>
        private void OnSimulationComplete(float bestAngle, float power, int hittedAmount)
        {
            _situationText.text = "Best angle: "+bestAngle+" \n power: "+power+" \n hitted ball: "+hittedAmount;

        }


        /// <summary>
        /// simulation button 押したとき
        /// </summary>
        public void OnStartSimulationButtonPressed()
        {
            
            OnStartSimulationButton?.Invoke();
        }

        /// <summary>
        /// simulation button 押したとき
        /// </summary>
        public void OnAddAngleButtonPressed()
        {

            OnAddAngleButton?.Invoke();
        }

        /// <summary>
        /// simulation button 押したとき
        /// </summary>
        public void OnAddPowerButtonPressed()
        {

            OnAddPowerButton?.Invoke();
        }

        /// <summary>
        /// shoot 押したとき
        /// </summary>
        public void OnShootButtonPressed()
        {

            OnShootButton?.Invoke();
        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}
