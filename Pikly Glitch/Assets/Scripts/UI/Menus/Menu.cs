using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace Pikl.UI {
	public class Menu : MonoBehaviour
	{
        public int menuDepth = 1;
        public GameObject defaultSelected;

		Animator _animator;
		CanvasGroup _canvasGroup;

		public bool IsOpen
		{
			get { return false;/*return _animator.GetBool("IsOpen");*/ }
			private set { /*_animator.SetBool("IsOpen", value);*/ }
		}

		public void Awake()
		{
            _animator = GetComponent<Animator>();
            _canvasGroup = GetComponent<CanvasGroup>();

   //         RectTransform rect = GetComponent<RectTransform>();
			//rect.offsetMax = rect.offsetMin = new Vector2(0, 0);
		}

        public virtual void OnUpdate() {
            //if (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Open")) {
            //    _canvasGroup.blocksRaycasts = _canvasGroup.interactable = false;
            //} else {
            //    _canvasGroup.blocksRaycasts = _canvasGroup.interactable = true;
            //}

            if (CheckBackButton())
                UIMgr.I.OnBackButtonPress();
        }

        public void InteractableOn() {
            _canvasGroup.blocksRaycasts = _canvasGroup.interactable = true;
        }

        public void InteractableOff() {
            _canvasGroup.blocksRaycasts = _canvasGroup.interactable = false;
        }

        public virtual void Open()
        {
            /*if (InputMgr.PlayerOneControlScheme.Name != "KeyboardAndMouse")
                Cursor.visible = false;*/
            
            SetFocus();
            IsOpen = true;
        }

        public virtual void Close()
        {
            IsOpen = false;
        }

        public virtual void SetFocus() {
            if (defaultSelected != null)
                EventSystem.current.SetSelectedGameObject(defaultSelected);
        }

        bool CheckBackButton() {
            //if (UIMgr.I.CurrentMenu is GameUI || UIMgr.I.CurrentMenu is MainMenu || UIMgr.I.CurrentMenu is GameComplete || UIMgr.I.CurrentMenu is End)
            //    return false;

            /*if (InputMgr.PlayerOneControlScheme.Name == XInputDotNetAdapter.) {
                return (InputMgr.GetKeyDown(KeyCode.Escape));
            } else {
                return (InputMgr.GetButtonDown("Cancel"));
            */
            //}
            return false;
        }
    }
}