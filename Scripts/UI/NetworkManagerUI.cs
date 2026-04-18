using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

namespace ShooterSurvival.UI
{
	public class NetworkManagerUI : MonoBehaviour
	{
		[SerializeField]
		private Button serverButton, hostButton, clientButton;
		// Start is called once before the first execution of Update after the MonoBehaviour is created

		// Awake is called when the script instance is being loaded.
		void Start()
		{
			serverButton.onClick.AddListener(startServer);
			hostButton.onClick.AddListener(startHost);
			clientButton.onClick.AddListener(startClient);
		}

		void startServer()
		{
			NetworkManager.Singleton.StartServer();
		}

		void startHost()
		{
			NetworkManager.Singleton.StartHost();
		}

		void startClient()
		{
			NetworkManager.Singleton.StartClient();
		}
	}

}