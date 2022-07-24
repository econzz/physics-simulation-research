using Simulation.Objects;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Simulation.Controllers
{
    /// <summary>
    /// デバッグ機能用
    /// </summary>
    public struct MyDebug
    {
        public bool IsDebug { get; set; }
        public void Log(object obj)
        {
            if (IsDebug == false)//本番の場合は debug log をスキップ
                return;

            Debug.Log(obj);
        }
    }

    /// <summary>
    /// ゲーム全体のコントローラ、他のscriptのUpdate, FixedUpdate はこの中１本化にする
    /// </summary>
    public class GameController : MonoBehaviour
    {
        public enum GAME_STATE
        {
            STANDBY,
            BALLS_MOVING,
            AI_THINKING,
            STOP_MOVEMENT
        }

        //他のスクリプトやゲームオブジェクトがイベントにsubscribe できるように管理する。
        public static Action OnStartGame;
        public static Action OnStartTurn;
        public static Action OnEndTurn;
        public static Action<GAME_STATE> OnStateChange;
        

        //game state
        public static GAME_STATE GameState;  

        [SerializeField] private bool _isDebug = true;
        [SerializeField] private InputController _inputController;
        [SerializeField] private PhysicsSimulationController _simulationController;
        [SerializeField] private Ball[] _collectionBalls;
        [SerializeField] private Target[] _collectionTargets;
        [SerializeField] private GameObject[] _collidables;
        [SerializeField] private Direction _directionForce;

        public static MyDebug CustomDebug;

        private Scene _mainScene;

        // Start is called before the first frame update
        void Start()
        {
            CustomDebug = new MyDebug();
            CustomDebug.IsDebug = _isDebug;

            Physics.autoSimulation = false;

            _mainScene = SceneManager.GetActiveScene();

            RegisterCallback();
            _simulationController.Initialize(_collectionBalls,_collectionTargets,_collidables);
            StartGame();
        }

        /// <summary>
        /// ゲームを初期スタート
        /// </summary>
        private void StartGame()
        {

            OnStartGame?.Invoke();
            StartTurn();
        }

        /// <summary>
        /// 毎回ターン始めるときに呼び出される
        /// </summary>
        private void StartTurn()
        {
            ChangeState(GAME_STATE.STANDBY);
            OnStartTurn?.Invoke();
        }

        /// <summary>
        /// 毎回ターン終了ときに呼び出される
        /// </summary>
        private void EndTurn()
        {
            OnEndTurn?.Invoke();

        }

        /// <summary>
        /// ゲームのステートを変更するための関数
        /// </summary>
        /// <param name="gameState"></param>
        private void ChangeState(GAME_STATE gameState)
        {
            GameState = gameState;

            if(GameState == GAME_STATE.STANDBY)
            {
                _directionForce.SetPositionTo(_collectionBalls[0].transform.position);
                _directionForce.gameObject.SetActive(true);
            }
            else
            {
                _directionForce.gameObject.SetActive(false);
            }
            OnStateChange?.Invoke(GameState);
        }
        

        /// <summary>
        /// イベントコールバック登録
        /// </summary>
        private void RegisterCallback()
        {
            InputController.OnTouched += InputTouched;
            InputController.OnMoved += InputMoved;

            UIController.OnStartSimulationButton += StartSimulation;
            UIController.OnAddAngleButton += AddAngle;
            UIController.OnAddPowerButton += AddPower;
            UIController.OnShootButton += Shoot;

            PhysicsSimulationController.OnSimulationComplete += OnSimulationComplete;

            foreach(Target target in _collectionTargets)
            {
                target.OnTargetHit += OnTargetHit;
            }
        }

        /// <summary>
        /// イベントコールバックunregister
        /// </summary>
        private void OnDestroy()
        {
            InputController.OnTouched -= InputTouched;
            InputController.OnMoved -= InputMoved;

            UIController.OnStartSimulationButton -= StartSimulation;
            UIController.OnAddAngleButton -= AddAngle;
            UIController.OnShootButton -= Shoot;

            PhysicsSimulationController.OnSimulationComplete -= OnSimulationComplete;

            foreach (Target target in _collectionTargets)
            {
                target.OnTargetHit -= OnTargetHit;
            }
        }

        /// <summary>
        /// ターゲットがヒットされるときのコールバック
        /// </summary>
        /// <param name="hitTarget">どちらのターゲットがヒットされる</param>
        /// <param name="hitBall">どちらのボールがヒットした？</param>
        private void OnTargetHit(Target hitTarget, Ball hitBall)
        {
            if (hitTarget == null || hitBall == null)
                return;

            if(hitBall.ballId != 0)//プレイのボール以外
                hitBall.gameObject.SetActive(false);
        }

        /// <summary>
        /// タッチ したとき、or マウスでクリックされたかどうかのとき、この関数呼び出される
        /// </summary>
        /// <param name="isTouched"></param>
        private void InputTouched(bool isTouched)
        {
           // CustomDebug.Log("test touch " + isTouched);
        }

        /// <summary>
        /// タッチorマウスで移動するとき
        /// </summary>
        /// <param name="touchPosition"></param>
        private void InputMoved(Vector3 touchPosition)
        {
            //CustomDebug.Log("test touch " + touchPosition);
        }
        
        /// <summary>
        /// ボタンシミュレーション押されたとき
        /// </summary>
        private void StartSimulation()
        {

            StartCoroutine(_simulationController.StartThinking(_directionForce));
        }

        /// <summary>
        /// 角度追加
        /// </summary>
        private void AddAngle()
        {
            _directionForce.AddAngle();
        }

        /// <summary>
        /// パワー追加
        /// </summary>
        private void AddPower()
        {
            _directionForce.AddPower();
        }

        /// <summary>
        /// ボールを打つ
        /// </summary>
        private void Shoot()
        {
            Vector3 dir = _collectionBalls[0].transform.position - _directionForce.GetPositionDetector().position;
            dir = dir.normalized;
            _collectionBalls[0].ShootTo(dir, _directionForce.power);

            ChangeState(GAME_STATE.BALLS_MOVING);
        }

        /// <summary>
        /// シミュレーション完了時に呼び出される関数
        /// </summary>
        /// <param name="bestAngle">simulationによって一番いい結果の角度</param>
        /// <param name="power">simulationによって一番いい結果のパワー</param>
        /// <param name="hittedAmount">このsimulation結果で、ボールの入れた数</param>
        private void OnSimulationComplete(float bestAngle, float power, int hittedAmount)
        {
            _directionForce.SetAngle(bestAngle);
            _directionForce.power = power;

        }

        // Update is called once per frame
        void Update()
        {
            //input のアップデート
            _inputController.OnUpdate();
        }


        void FixedUpdate()
        {
            _mainScene.GetPhysicsScene().Simulate(Time.fixedDeltaTime);
            switch (GameState)
            {
                case GAME_STATE.BALLS_MOVING:
                    if (IsAllBallStopped(_collectionBalls))
                    {
                        ChangeState(GAME_STATE.STANDBY);
                    }
                    break;
            }
        }

        /// <summary>
        /// 全ボールがまだ転がっているかどうかチェックをする
        /// </summary>
        /// <param name="ballCollection">ボール配列</param>
        /// <returns>true:全ボール止まっています。false:まだ転がっています</returns>
        public static bool IsAllBallStopped(Ball[] ballCollection)
        {
            bool ballsMoving = false;
            
            foreach (Ball ball in ballCollection)
            {
                ball.OnUpdate();
                if (ball.isMoving)
                {
                    ballsMoving = true;
                    break;
                }


                if (ballsMoving == false)
                {
                    GameController.CustomDebug.Log("all ball stopped");
                    
                    return true;
                }
            }

            return false;
        }
    }

}
