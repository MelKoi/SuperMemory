using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DialogueEditor
{
    public class ExampleInputManager : MonoBehaviour
    {
        public KeyCode m_UpKey;
        public KeyCode m_DownKey;
        public KeyCode m_SelectKey;
        public GameObject DialogCanvas;
        [Header("广播监听")]
        public VoidEventSO closeBagPanel;
        public VoidEventSO showBagPanel;
        public VoidEventSO checkPicture;

        private bool bagOpenFlag = false;
        private bool checkPictureFlag = false;
        private void OnEnable()
        {
            showBagPanel.OnEventRaised += ShowBagPanel;
            closeBagPanel.OnEventRaised += CloseBagPanel;
            checkPicture.OnEventRaised += CheckPictureState;
        }
        private void OnDisable()
        {
            showBagPanel.OnEventRaised -= ShowBagPanel;
            closeBagPanel.OnEventRaised -= CloseBagPanel;
            checkPicture.OnEventRaised -= CheckPictureState;
        }

        public void ShowBagPanel()
        {
            bagOpenFlag = true;
            //DialogCanvas.SetActive(false);
        }

        public void CloseBagPanel()
        {
            //DialogCanvas.SetActive(true);
            bagOpenFlag = false;
        }
        public void CheckPictureState() {
            checkPictureFlag = !checkPictureFlag;
        }
        private void Update()
        {
            if (ConversationManager.Instance != null && !bagOpenFlag && !checkPictureFlag)
            {
                UpdateConversationInput();
            }
        }

        private void UpdateConversationInput()
        {
            if (ConversationManager.Instance.IsConversationActive)
            {
                if (Input.GetKeyDown(m_UpKey))
                    ConversationManager.Instance.SelectPreviousOption();
                else if (Input.GetKeyDown(m_DownKey))
                    ConversationManager.Instance.SelectNextOption();
                else if (Input.GetKeyDown(m_SelectKey))
                    ConversationManager.Instance.PressSelectedOption();
            }
        }
    }
}
