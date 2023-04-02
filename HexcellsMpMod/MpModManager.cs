using System;
using System.Collections.Generic;
using System.Linq;
using ENet;
using HexcellsMultiplayer;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Reflection;

namespace HexcellsMpMod
{
	public class MpModManager : MonoBehaviour
	{
		public static MpModManager Instance { get; private set; }
		public bool InSession => client != null && client.Connected;
		public bool IsHosting => server != null;
		private bool InGameScene => inGameScene;

		// Tweaks/settings
		public float MistakePenalty = 60f;

		// Client/server
		private Server server;
		private Client client;

		// References to Hexcells classes
		private MusicDirector musicDirector = null;
		private HexScoring scoring = null;

		// SteamManager reference (class is marked as internal so we use reflection)
		private Type steamManagerType;
		private PropertyInfo steamManagerInitializedProperty;

		// Our state
		private float uiScale = 1f;
		private bool isInitialized = false;
		private float timer = 0f;
		private HexPeer.State internalState = HexPeer.State.InMenu;
		private float timeSinceLastUpdate = 0f;
		private bool inGameScene = false;
		private string connectToHost = string.Empty;

		// GUI things
		private GUIStyle boxStyle;
		private GUIStyle boxStyleDone;
		private GUIStyle boxStyleError;
		private GUIStyle buttonBoxStyle;
		private GUIStyle boxStyleInfo;
		private Texture2D boxShadow;

		private void Awake()
		{
			if (MpModManager.Instance != null)
			{
				Destroy(gameObject);
				return;
			}
			DontDestroyOnLoad(this.gameObject);

			MpModManager.Instance = this;
			Library.Initialize();

			steamManagerType = Type.GetType("SteamManager, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
			steamManagerInitializedProperty = steamManagerType.GetProperty("Initialized", BindingFlags.Static | BindingFlags.Public);

			uiScale = Mathf.Max(1f, Screen.height / 1080f);
			boxShadow = Utils.MakeBoxShadow(32);
			SceneManager.sceneLoaded += OnSceneLoaded;
		}

		private void OnDestroy()
		{
			Disconnect();
			if(server != null)
            {
				server.Dispose();
            }
			Library.Deinitialize();
		}

		public void Host()
		{
			server = new Server();
			server.Start(6666);
		}

		public void Join(string address)
		{
			Disconnect();
			try
			{
				string personaName = SteamFriends.GetPersonaName();
				client = new Client(personaName)
				{
					OnGameStart = new Action<int, bool>(StartGameCB),
					OnGameEnd = new Action(QuitGameCB)
				};
				client.Connect(address, 6666);
			}
			catch (Exception ex)
			{
				Debug.Log($"MpModManger Join failed because: {ex.Message}");
			}
		}

		private bool IsSteamInitialized()
		{
			return (bool)steamManagerInitializedProperty.GetValue(null, null);
		}

		private void Update()
		{
			if (!isInitialized && IsSteamInitialized())
				Initialize();

			if (client != null)
				client.UpdateLoop();
			if (server != null)
				server.UpdateLoop();

			if (client != null)
			{
				float score = 0f;
				float time = 0f;
				if (InGameScene)
				{
					time = GetTime();
					score = (float)scoring.tilesRemoved / (float)scoring.totalNumberOfTiles;
					if (score < 1f)
					{
						timer += Time.deltaTime;
					}
					else
					{
						internalState = HexPeer.State.InPuzzleComplete;
					}
				}

				timeSinceLastUpdate += Time.deltaTime;
				if (timeSinceLastUpdate >= 1f)
				{
					timeSinceLastUpdate -= 1f;
					client.SendUpdatePacket(time, score, internalState);
				}
			}
		}

		private void StartGameCB(int seed, bool hardMode)
		{
			int num = seed % 9;
			if (num < 2)
				musicDirector.ChangeTrack((MusicDirector.Track)(1 - num));
			else
				musicDirector.ChangeTrack((MusicDirector.Track)num);

			GameObject.Find("Game Manager(Clone)").GetComponent<OptionsManager>().currentOptions.levelGenHardModeActive = hardMode;
			GameObject.Find("Game Manager(Clone)").GetComponent<GameManagerScript>().seedNumber = seed.ToString();
			GameObject.Find("Game Manager(Clone)").GetComponent<GameManagerScript>().isLoadingSavedLevelGenState = false;
			GameObject.Find("Fader").GetComponent<FaderScript>().FadeOut(37);
			GameObject.Find("Loading Text").GetComponent<LoadingText>().FadeIn();
		}

		public void HostStartGame(int seed, bool hardMode)
		{
			if (server != null)
			{
				server.StartGame(seed, hardMode);
			}
		}

		private void QuitGameCB()
		{
			GameObject.Find("Fader").GetComponent<FaderScript>().FadeOut(0);
		}

		public void HostQuitGame()
		{
			if (server != null)
			{
				server.StopGame();
			}
		}

		private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			musicDirector = GameObject.Find("Music Director(Clone)").GetComponent<MusicDirector>();

			GameObject gameObject = GameObject.Find("Score Text");
			if (gameObject != null)
				scoring = gameObject.GetComponent<HexScoring>();
			else
				scoring = null;

			bool wasInGameScene = InGameScene;
			inGameScene = (scene.name == "Level Generator");
			internalState = (InGameScene ? HexPeer.State.InPuzzle : HexPeer.State.InMenu);

			// reset timer only if we're entering into the game scene from the menu
			// if not, then we assume that the player  has restarted the level
			// and could use prior knowledge to cheat a better time
			// maybe we should just disable the retry button entirely?
			if (InGameScene && !wasInGameScene)
			{
				timer = 0f;
			}
		}

		private float GetTime()
		{
			if (scoring == null)
				return 0f;
			return timer + (MistakePenalty * scoring.numberOfMistakesMade);
		}

		private void InitStyles()
		{
			if (boxStyle != null)
				return;

			boxStyle = new GUIStyle(GUI.skin.box);
			boxStyle.normal.background = Utils.MakeTex(2, 2, new Color32(5, 164, 235, 255));
			boxStyle.alignment = TextAnchor.MiddleRight;
			boxStyle.fontSize = Mathf.RoundToInt(12f * uiScale);
			boxStyle.normal.textColor = Color.white;
			boxStyle.richText = true;
			boxStyle.padding = new RectOffset(8, 8, 2, 2);
			
			boxStyleDone = new GUIStyle(boxStyle);
			boxStyleDone.normal.background = Utils.MakeTex(2, 2, Color.white);
			boxStyleDone.normal.textColor = Color.white;

			boxStyleError = new GUIStyle(boxStyle);
			boxStyleError.normal.background = Utils.MakeTex(2, 2, new Color32(255, 0, 0, 255));
			boxStyleError.alignment = TextAnchor.MiddleCenter;
			boxStyleError.normal.textColor = Color.white;

			boxStyleInfo = new GUIStyle(boxStyle);
			boxStyleInfo.normal.background = Utils.MakeTex(2, 2, new Color32(5, 162, 235, 255));
			boxStyleInfo.alignment = TextAnchor.MiddleCenter;
			boxStyleInfo.normal.textColor = Color.white;

			buttonBoxStyle = new GUIStyle(boxStyleDone);
			buttonBoxStyle.alignment = TextAnchor.MiddleCenter;
			buttonBoxStyle.normal.textColor = Color.black;
		}

		private void OnGUI()
		{
			if (client == null)
				return;
			if (boxStyle == null)
				InitStyles();

			if (client != null && !client.Connected)
			{
				DrawStatusBar();
			}
			else
			{
				DrawPeerCards();
			}
		}

		private void Initialize()
		{
			string[] commandLineArgs = Environment.GetCommandLineArgs();
			for (int i = 0; i < commandLineArgs.Length; i++)
			{
				string arg = commandLineArgs[i];
				if (arg == "-mphost")
				{
					Host();
					Join("127.0.0.1");
				}
				if (arg == "-mpjoin" && i != commandLineArgs.Length - 1)
				{
					connectToHost = commandLineArgs[i + 1];
					Join(connectToHost);
					i++;
				}
			}
			Application.runInBackground = (client != null || server != null);
			isInitialized = true;
		}

		private IEnumerable<HexPeer> GetScoreboard()
		{
			foreach (HexPeer peer in client.Peers.Where((HexPeer x) => x.CurrentState == HexPeer.State.InPuzzleComplete).OrderBy((HexPeer x) => x.Time))
				yield return peer;
			foreach (HexPeer peer in client.Peers.Where((HexPeer x) => x.CurrentState == HexPeer.State.InPuzzle).OrderByDescending((HexPeer x) => x.Progress))
				yield return peer;
			foreach (HexPeer peer in client.Peers.Where((HexPeer x) => x.CurrentState == HexPeer.State.InMenu).OrderByDescending((HexPeer x) => x.Name))
				yield return peer;
		}

		private bool BoxButton(Rect rect, string text)
		{
			Color storeGuiColor = GUI.color;

			UnityEngine.Event current = UnityEngine.Event.current;
			if (rect.Contains(current.mousePosition))
			{
				GUI.color = new Color(0.05f, 0.6f, 0.9f, 1f);
			}
			GUI.Box(rect, text, buttonBoxStyle);
			GUI.color = storeGuiColor;

			return current.isMouse && rect.Contains(current.mousePosition) && current.type == UnityEngine.EventType.MouseDown;
		}

		private void Disconnect()
		{
			if (client != null)
			{
				client.Disconnect();
				client.Dispose();
				client = null;
			}
		}

		private void DrawStatusBar()
		{
			Rect statusBarRect = new Rect(0f, 0f, Screen.width, 48f * uiScale);
			if (client.CurrentState == Client.State.Connecting)
			{
				GUI.Box(statusBarRect, "Connecting to " + connectToHost + "...", boxStyleInfo);
				return;
			}
			if (client.CurrentState == Client.State.Disconnected)
			{
				Rect dismissBtnRect = new Rect(statusBarRect.xMax - (100f * uiScale), 8f * uiScale, 92f * uiScale, 32f * uiScale);
				Rect retryBtnRect = new Rect(statusBarRect.xMax - (200f * uiScale), 8f * uiScale, 92f * uiScale, 32f * uiScale);

				GUI.Box(statusBarRect, "No connection to host.", boxStyleError);
				if (BoxButton(retryBtnRect, "Retry"))
				{
					Join(connectToHost);
				}
				if (BoxButton(dismissBtnRect, "Dismiss"))
				{
					Disconnect();
				}
			}
		}

		private void DrawPeerCards()
		{
			int count = client.Peers.Count;
			var scoreboard = GetScoreboard();

			float spacing = 16f;
			float totalSpacing = Math.Max(0, client.Peers.Count - 1) * spacing;

			float cardWidth = 180f * uiScale;
			float cardHeight = 64f * uiScale;
			float totalCardWidth = cardWidth * count;

			float screenCenter = (float)Screen.width / 2f;
			float cardWidthHalf = cardWidth / 2f;

			float drawOriginX = screenCenter - (count * cardWidthHalf) - (totalSpacing * 0.5f);
			float drawOriginY = 4f;
			float drawEndX = drawOriginX + totalCardWidth + totalSpacing;
			float drawEndY = drawOriginY + cardHeight;

			// detect mouse hovering card area and set UI color accordingly
			Vector3 mousePosition = Input.mousePosition;
			bool mouseHoveringUI = (mousePosition.x >= drawOriginX && mousePosition.x <= drawEndX) 
							    && (mousePosition.y > (Screen.height - drawEndY));
			
			Color oldUIColor = GUI.color;
			GUI.color = (mouseHoveringUI) ? new Color(1f, 1f, 1f, 0.5f) : Color.white;

			// draw the cards
			int numDrawn = 0;
			foreach (HexPeer hexPeer in scoreboard)
			{
				Rect cardRect = new Rect(drawOriginX + (numDrawn * (cardWidth + spacing)), drawOriginY, cardWidth, cardHeight);

				string cardText;
				if (hexPeer.CurrentState == HexPeer.State.InMenu)
				{
					cardText = hexPeer.Name + "\n<b>In Menu</b>";
				}
				else
				{
					float time = (hexPeer.CurrentState != HexPeer.State.InPuzzle) ? hexPeer.Time : hexPeer.SmoothedTime;
					TimeSpan timeSpan = TimeSpan.FromSeconds(time);
					string timeText = $"{Mathf.FloorToInt((float)timeSpan.TotalMinutes):D2}:{timeSpan.Seconds:D2}:{timeSpan.Milliseconds:D2}";
					int progressInt = (int)(100f * hexPeer.Progress);

					if (hexPeer.CurrentState == HexPeer.State.InPuzzleComplete)
					{
						cardText = $"<color=#444444ff>{hexPeer.Name}</color>\n<b><color=#9d9d9dff>{timeText}</color>\n<color=#444444ff>Puzzle Complete</color></b>";
					}
					else
					{
						cardText = $"{hexPeer.Name}\n<b>{timeText}\nProgress: {progressInt}%</b>";
					}
				}

				float boxShadowOffset = 16f * uiScale;
				var style = (hexPeer.CurrentState == HexPeer.State.InPuzzleComplete) ? boxStyleDone : boxStyle;

				GUI.DrawTexture(new Rect(cardRect.x - boxShadowOffset, cardRect.y + boxShadowOffset, cardRect.width, cardRect.height), boxShadow);
				GUI.DrawTexture(cardRect, style.normal.background);
				GUI.Label(cardRect, cardText.ToUpperInvariant(), style);
				numDrawn++;
			}

			// restore UI color
			GUI.color = oldUIColor;
		}
	}
}