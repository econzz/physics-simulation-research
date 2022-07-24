using Simulation.Objects;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Simulation.Controllers
{
    public class PhysicsSimulationController : MonoBehaviour
    {
        public static Action<float, float, int> OnSimulationComplete;

        [SerializeField] private Material _playBallSimulationMaterial;
        [SerializeField] private Material _ballSimulationMaterial;
        private Scene _simulationScene;
        private PhysicsScene _simulationPhysicsScene;

        private List<Ball> _allBallsInPhysicsScene = new List<Ball>();
        private Ball[] _allBallsInRealScene;

        private List<Ball> _hittedBallsInPhysicsScene = new List<Ball>();

        // Start is called before the first frame update
        void Start()
        {

        }

        /// <summary>
        /// 初期化をする
        /// </summary>
        /// <param name="balls">ボールの配列</param>
        /// <param name="targets">ターゲットの配列</param>
        /// <param name="collidables">その他のcollideがあるgameobject</param>
        public void Initialize(Ball[] balls, Target[] targets, GameObject[] collidables)
        {
            
            _simulationScene = SceneManager.CreateScene("physics-simulation-scene", new CreateSceneParameters(LocalPhysicsMode.Physics3D));
            _simulationPhysicsScene = _simulationScene.GetPhysicsScene();

            _allBallsInPhysicsScene.Clear();
            _allBallsInRealScene = balls;
            PrepareSimulationScene(balls, targets, collidables);
        }

        /// <summary>
        /// simulation用のシーンにgameobjectを全部initializeする
        /// </summary>
        /// <param name="balls">ボールの配列</param>
        /// <param name="targets">ターゲットの配列</param>
        /// <param name="collidables">その他のcollideがあるgameobject</param>
        private void PrepareSimulationScene(Ball[] balls, Target[] targets, GameObject[] collidables)
        {
            for (int i = 0; i < balls.Length; i++)
            {
                Ball inSimulationBall = Instantiate(balls[i]);
                inSimulationBall.SetBallSimulation();
                inSimulationBall.transform.position = balls[i].transform.position;
                inSimulationBall.transform.rotation = balls[i].transform.rotation;
                inSimulationBall.ToggleMeshRenderer(false);
                if (i == 0)
                {
                    inSimulationBall.transform.name = "ReferenceWhiteBall";
                    inSimulationBall.GetComponent<MeshRenderer>().sharedMaterial = _playBallSimulationMaterial;
                }
                else
                {
                    inSimulationBall.transform.name = "ReferenceOtherBall" + i;
                    inSimulationBall.GetComponent<MeshRenderer>().sharedMaterial = _ballSimulationMaterial;
                }

                SceneManager.MoveGameObjectToScene(inSimulationBall.gameObject, _simulationScene);
                _allBallsInPhysicsScene.Add(inSimulationBall);

            }

            for (int i = 0; i < targets.Length; i++)
            {
                Target inSimulationTarget = Instantiate(targets[i]);
                inSimulationTarget.OnTargetHit += OnTargetHitInSimulation;
                inSimulationTarget.transform.position = targets[i].transform.position;
                inSimulationTarget.transform.rotation = targets[i].transform.rotation;

                SceneManager.MoveGameObjectToScene(inSimulationTarget.gameObject, _simulationScene);
            }

            foreach (GameObject collidable in collidables)
            {
                GameObject temp = Instantiate(collidable);
                temp.transform.position = collidable.transform.position;
                temp.transform.rotation = collidable.transform.rotation;
                Destroy(temp.GetComponent<MeshRenderer>());
                SceneManager.MoveGameObjectToScene(temp.gameObject, _simulationScene);
            }
        }

        /// <summary>
        /// simulation シーンに、ターゲットがボールと衝突する際に、情報を保存する
        /// </summary>
        /// <param name="hitTarget">どちらのターゲット</param>
        /// <param name="hitBall">どちらのボール</param>
        private void OnTargetHitInSimulation(Target hitTarget, Ball hitBall)
        {
            if (hitTarget == null || hitBall == null)
                return;

            hitBall.GetComponent<Rigidbody>().Sleep();

            if(hitBall.ballId != 0) //プレイヤーボールではないのみ
                _hittedBallsInPhysicsScene.Add(hitBall);
        }

        /// <summary>
        /// 10角度ずつ検索して、360度シミュレーション終わったら一番いい結果をinvoke する
        /// </summary>
        /// <param name="currentDirection"></param>
        /// <returns></returns>
        public IEnumerator StartThinking(Direction currentDirection)
        {
            
            int counter = 0;

            float bestAngle = 0;
            int bestHitted = 0;
            while (counter < 36)//36回
            {
                _hittedBallsInPhysicsScene.Clear();
                SyncWithReal();

                

                Ball playerBallSimulation = _allBallsInPhysicsScene[0];

                Vector3 dir = playerBallSimulation.transform.position - currentDirection.GetPositionDetector().position;
                dir = dir.normalized;
                _allBallsInPhysicsScene[0].ShootTo(dir, currentDirection.power);
                bool isAllBallStop = false;
                do
                {
                    _simulationPhysicsScene.Simulate(Time.fixedDeltaTime);
                    isAllBallStop = GameController.IsAllBallStopped(_allBallsInPhysicsScene.ToArray());

                    
                } while (isAllBallStop == false);

                if(_hittedBallsInPhysicsScene.Count > bestHitted)
                {
                    bestHitted = _hittedBallsInPhysicsScene.Count;
                    bestAngle = currentDirection.GetAngle();
                }

                //yield return new WaitForSeconds(0.5f);

                counter++;
                currentDirection.AddAngle();

                

                yield return null;
            }

            

            GameController.CustomDebug.Log("bestHitted = " + bestHitted + " bestAngle " + bestAngle);

            OnSimulationComplete?.Invoke(bestAngle, currentDirection.power, bestHitted);

            ShowPredictionBall(true);
            yield return new WaitForSeconds(1.0f);
            ShowPredictionBall(false);
        }

        public void ShowPredictionBall(bool enabled)
        {
            foreach (Ball ball in _allBallsInPhysicsScene)
            {
                ball.ToggleMeshRenderer(enabled);
            }
        }

        /// <summary>
        /// simulation シーンにあるボールをリアルシーンと位置を同期させる
        /// </summary>
        private void SyncWithReal()
        {

            for (int i = 0; i < _allBallsInPhysicsScene.Count; i++)
            {
                Ball ballInPhysics = _allBallsInPhysicsScene[i];
                ballInPhysics.gameObject.SetActive(true);
                ballInPhysics.GetComponent<Rigidbody>().Sleep();
                ballInPhysics.transform.position = _allBallsInRealScene[i].transform.position;
                ballInPhysics.transform.rotation = _allBallsInRealScene[i].transform.rotation;
            }

        }

        /// <summary>
        /// GameController のfixedUpdate, 1フレームごと呼び出される
        /// </summary>
        public void OnUpdate()
        {

        }
    }

}
