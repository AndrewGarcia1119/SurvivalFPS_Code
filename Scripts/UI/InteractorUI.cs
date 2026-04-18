using ShooterSurvival.GameSystems;
using ShooterSurvival.Util;
using TMPro;
using UnityEngine;

namespace ShooterSurvival.UI
{
    public class InteractorUI : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text interactorText;

        private Interactor interactor;
        bool gotInteractor = false;
        string currentText;
        // Update is called once per frame
        void Update()
        {
            if (!interactor & !gotInteractor) //gotInteractor boolean prevents regetting the Interactor when it becomes null after disconnection
            {
                var no = NetworkUtil.GetLocalPlayer();
                if (no)
                {
                    interactor = no.GetComponent<Interactor>();
                    gotInteractor = true;
                }
                else
                {
                    return;
                }
            }
            if (interactorText)
            {
                interactorText.gameObject.SetActive(interactor.TryGetCurrentInteractiveText(out currentText));
                interactorText.text = currentText;
            }
        }
    }

}