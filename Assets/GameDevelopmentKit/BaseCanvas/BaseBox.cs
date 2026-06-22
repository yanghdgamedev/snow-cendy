using UnityEngine;
using UnityEngine.Events;

namespace GameDevelopmentKit.Scripts
{
    public abstract class BaseBox : MonoBehaviour
    {
        [SerializeField] private bool isSpaceCamera = true;
        [SerializeField] private string canvasName;
        public UnityAction OnClose;
        protected Canvas _canvas;
    
        private const int BASE_INDEX_LAYER = 200;
        private static int MaxIndex = BASE_INDEX_LAYER;

        protected virtual void Awake()
        {
            _canvas = GetComponent<Canvas>();
        }
    
        private static int GetIndex() {
            MaxIndex++;
            return MaxIndex;
        }

        protected virtual void OnEnable()
        {
            _canvas = this.GetComponent<Canvas>();
            if (isSpaceCamera) {
                _canvas.renderMode = RenderMode.ScreenSpaceCamera;
                _canvas.worldCamera = Camera;
            }
            SetLayer();
        }

        private static Camera _camera;
        private Camera Camera
        {
            get
            {
                if (_camera == null)
                {
                    _camera = Camera.main;
                    if (_camera == null)
                    {
                        // WDDebug.LogError("CameraUI not found");
                    }
                }

                return _camera;
            }
        }
    
        public virtual void Show() {
            gameObject.SetActive(true);
        }
    
        public virtual BaseBox Close(UnityAction callback) {
            OnClose += callback;
            Close();
            return this;
        }

        public virtual void Close()
        {
            gameObject.SetActive(false);
        }
    
        private void SetLayer() {
            _canvas.sortingOrder = GetIndex();
        }
    
        protected virtual void OnDisable() {
            if (OnClose != null) {
                OnClose?.Invoke();
                OnClose = null;
            }
        }
    }
}
