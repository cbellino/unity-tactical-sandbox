using UnityEngine;

namespace Tactical {

	using Data;

	public class GameManager : MonoBehaviour {

		public static GameManager instance;
		public string firstSceneName = "Level1";

		private DataManager dataManager;

		private void Awake() {
			// Check if instance already exists
			if (instance == null) {
				// if not, set instance to this
				instance = this;
			}
			// If instance already exists and it's not this
			else if (instance != this) {
				// Then destroy this. This enforces our singleton pattern, meaning
				// there can only ever be one instance of a GameManager.
				Destroy(gameObject);
			}

      // Sets this to not be destroyed when reloading scene.
      DontDestroyOnLoad(gameObject);

      // Get the data manager and load all the data.
			dataManager = GetComponent<DataManager>();
			dataManager.LoadAll();

			UnityEngine.SceneManagement.SceneManager.LoadScene(firstSceneName);
		}

	}

}
