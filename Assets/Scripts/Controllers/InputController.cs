using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Simulation.Controllers
{
    public class InputController : MonoBehaviour
    {
        public static Action<bool> OnTouched;
        public static Action<Vector3> OnMoved;

        private bool _isTouchStarted = false;

        private Camera _mainCamera;

        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        /// <summary>
        /// GameController のUpdate, 1フレームごと呼び出される
        /// </summary>
        public void OnUpdate()
        {
            //マウス処理
            CheckMouse();

            //タッチ処理
            CheckTouch();

            //キーボード処理
            CheckKeyboard();
        }

        /// <summary>
        /// pcマウスのinput 処理
        /// </summary>
        private void CheckMouse()
        {
            if (Input.GetMouseButton(0))
            {
                OnTouched?.Invoke(true);
                _isTouchStarted = true;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                OnTouched?.Invoke(false);
                _isTouchStarted = false;
            }

            if (_isTouchStarted)
            {
                Vector3 mousePosition = _mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 5));

                OnMoved?.Invoke(mousePosition);
            }
            
        }

        /// <summary>
        /// タッチのinput 処理
        /// </summary>
        private void CheckTouch()
        {

            if (Input.touchCount > 0)
            {

                Touch touch = Input.GetTouch(0);
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        _isTouchStarted = true;
                        OnTouched?.Invoke(true);
                        break;
                    case TouchPhase.Ended:
                        _isTouchStarted = false;
                        OnTouched?.Invoke(false);
                        break;
                    case TouchPhase.Moved:
                        if (_isTouchStarted)
                            OnMoved?.Invoke(touch.position);
                        break;
                }
            }
        }

        /// <summary>
        /// keyboardのinput の処理
        /// </summary>
        private void CheckKeyboard()
        {
            //todo
        }
    }

}

