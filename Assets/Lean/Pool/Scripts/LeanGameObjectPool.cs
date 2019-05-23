using UnityEngine;
using System.Collections.Generic;
using Lean.Common;
#if UNITY_EDITOR
using UnityEditor;

namespace Lean.Pool
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(LeanGameObjectPool))]
	public class LeanGameObjectPool_Inspector : LeanInspector<LeanGameObjectPool>
	{
		protected override void DrawInspector()
		{
			BeginError(Any(t => t.Prefab == null));
				Draw("Prefab", "The prefab this pool controls.");
			EndError();
			Draw("Notification", "If you need to peform a special action when a prefab is spawned or despawned, then this allows you to control how that action is performed.\nSendMessage = The prefab clone is sent the OnSpawn and OnDespawn messages.\nBroadcastMessage = The prefab clone and all its children are sent the OnSpawn and OnDespawn messages.\nPoolableEvent = The prefab clone's LeanPoolable component is used.");
			Draw("Preload", "Should this pool preload some clones?");
			Draw("Capacity", "Should this pool have a maximum amount of spawnable clones?");
			Draw("Recycle", "If the pool reaches capacity, should new spawns force older ones to despawn?");
			Draw("Persist", "Should this pool be marked as DontDestroyOnLoad?");
			Draw("Stamp", "Should the spawned clones have their clone index appended to their name?");
			Draw("Warnings", "Should detected issues be output to the console?");

			EditorGUILayout.Separator();

			EditorGUI.BeginDisabledGroup(true);
				EditorGUILayout.IntField("Spawned", Target.Spawned);
				EditorGUILayout.IntField("Despawned", Target.Despawned);
				EditorGUILayout.IntField("Total", Target.Total);
			EditorGUI.EndDisabledGroup();
		}

		[MenuItem("GameObject/Lean/Pool", false, 1)]
		private static void CreateLocalization()
		{
			var gameObject = new GameObject(typeof(LeanGameObjectPool).Name);

			Undo.RegisterCreatedObjectUndo(gameObject, "Create LeanGameObjectPool");

			gameObject.AddComponent<LeanGameObjectPool>();

			Selection.activeGameObject = gameObject;
		}
	}
}
#endif

namespace Lean.Pool
{
	/// <summary>This component allows you to pool GameObjects, giving you a very fast alternative to Instantiate and Destroy.
	/// Pools also have settings to preload, recycle, and set the spawn capacity, giving you lots of control over your spawning.</summary>
	[HelpURL(LeanPool.HelpUrlPrefix + "LeanGameObjectPool")]
	[AddComponentMenu(LeanPool.ComponentPathPrefix + "GameObject Pool")]
	public class LeanGameObjectPool : MonoBehaviour, ISerializationCallbackReceiver
	{
		[System.Serializable]
		public class Delay
		{
			public GameObject Clone;
			public float      Life;
		}

		public enum NotificationType
		{
			None,
			SendMessage,
			BroadcastMessage,
			PoolableInterface
		}

		/// <summary>All active and enabled pools in the scene.</summary>
		public static List<LeanGameObjectPool> Instances = new List<LeanGameObjectPool>();

		/// <summary>The prefab this pool controls.</summary>
		public GameObject Prefab;

		/// <summary>If you need to peform a special action when a prefab is spawned or despawned, then this allows you to control how that action is performed.
		/// NOTE: SendMessage = The prefab clone is sent the OnSpawn and OnDespawn messages.
		/// NOTE: BroadcastMessage = The prefab clone and all its children are sent the OnSpawn and OnDespawn messages.
		/// NOTE: PoolableEvent = The prefab clone's LeanPoolable component is used.</summary>
		public NotificationType Notification = NotificationType.PoolableInterface;

		/// <summary>Should this pool preload some clones?</summary>
		public int Preload;

		/// <summary>Should this pool have a maximum amount of spawnable clones?</summary>
		public int Capacity;

		/// <summary>If the pool reaches capacity, should new spawns force older ones to despawn?</summary>
		public bool Recycle;

		/// <summary>Should this pool be marked as DontDestroyOnLoad?</summary>
		public bool Persist;

		/// <summary>Should the spawned clones have their clone index appended to their name?</summary>
		public bool Stamp;

		/// <summary>Should detected issues be output to the console?</summary>
		public bool Warnings = true;

		/// <summary>This stores all spawned clones in a list. This is used when Recycle is enabled, because knowing the spawn order must be known. This list is also used during serialization.</summary>
		[SerializeField]
		private List<GameObject> spawnedClonesList = new List<GameObject>();

		/// <summary>This stores all spawned clones in a hash set. This is used when Recycle is disabled, because their storage order isn't important. This allows us to quickly find the Clone associated with the specified GameObject.</summary>
		private HashSet<GameObject> spawnedClonesHashSet = new HashSet<GameObject>();

		/// <summary>All the currently despawned prefab instances.</summary>
		[SerializeField]
		private List<GameObject> despawnedClones = new List<GameObject>();

		/// <summary>All the delayed destruction objects.</summary>
		[SerializeField]
		private List<Delay> delays = new List<Delay>();

		private static List<IPoolable> tempPoolables = new List<IPoolable>();

		/// <summary>Find the pool responsible for handling the specified prefab.</summary>
		public static bool TryFindPoolByPrefab(GameObject prefab, ref LeanGameObjectPool foundPool)
		{
			for (var i = Instances.Count - 1; i >= 0; i--)
			{
				var pool = Instances[i];

				if (pool.Prefab == prefab)
				{
					foundPool = pool; return true;
				}
			}

			return false;
		}

		/// <summary>Find the pool responsible for handling the specified prefab clone.
		/// NOTE: This can be an expensive operation if you have many large pools.</summary>
		public static bool TryFindPoolByClone(GameObject clone, ref LeanGameObjectPool pool)
		{
			for (var i = Instances.Count - 1; i >= 0; i--)
			{
				pool = Instances[i];

				// Search hash set
				if (pool.spawnedClonesHashSet.Contains(clone) == true)
				{
					return true;
				}

				// Search list
				for (var j = pool.spawnedClonesList.Count - 1; j >= 0; j--)
				{
					if (pool.spawnedClonesList[j] == clone)
					{
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>Returns the amount of spawned clones.</summary>
		public int Spawned
		{
			get
			{
				return spawnedClonesList.Count + spawnedClonesHashSet.Count;
			}
		}

		/// <summary>Returns the amount of despawned clones.</summary>
		public int Despawned
		{
			get
			{
				return despawnedClones.Count;
			}
		}

		/// <summary>Returns the total amount of spawned and despawned clones.</summary>
		public int Total
		{
			get
			{
				return Spawned + Despawned;
			}
		}

		/// <summary>This will either spawn a previously despanwed/preloaded clone, recycle one, create a new one, or return null.</summary>
		public void Spawn()
		{
			Spawn(transform.position, transform.rotation);
		}

		public GameObject Spawn(Vector3 position, Quaternion rotation, Transform parent = null)
		{
			var clone = default(GameObject);

			TrySpawn(position, rotation, parent, ref clone);

			return clone;
		}

		/// <summary>This will either spawn a previously despanwed/preloaded clone, recycle one, create a new one, or return null.</summary>
		public bool TrySpawn(Vector3 position, Quaternion rotation, Transform parent, ref GameObject clone)
		{
			if (Prefab != null)
			{
				// Spawn a previously despanwed/preloaded clone?
				for (var i = despawnedClones.Count - 1; i >= 0; i--)
				{
					clone = despawnedClones[i];

					despawnedClones.RemoveAt(i);

					if (clone != null)
					{
						SpawnClone(clone, position, rotation, parent);

						return true;
					}

					if (Warnings == true) Debug.LogWarning("This pool contained a null despawned clone, did you accidentally destroy it?", this);
				}

				// Make a new clone?
				if (Capacity <= 0 || Total < Capacity)
				{
					clone = CreateClone(position, rotation, parent);

					// Add clone to spawned list
					if (Recycle == true)
					{
						spawnedClonesList.Add(clone);
					}
					else
					{
						spawnedClonesHashSet.Add(clone);
					}

					InvokeOnSpawn(clone);

					return true;
				}

				// Recycle?
				if (Recycle == true && TryDespawnOldest(ref clone, false) == true)
				{
					SpawnClone(clone, position, rotation, parent);

					return true;
				}
			}
			else
			{
				if (Warnings == true) Debug.LogWarning("You're attempting to spawn from a pool with a null prefab", this);
			}

			return false;
		}

		[ContextMenu("Despawn Oldest")]
		public void DespawnOldest()
		{
			var clone = default(GameObject);

			TryDespawnOldest(ref clone, true);
		}

		private bool TryDespawnOldest(ref GameObject clone, bool registerDespawned)
		{
			MergeSpawnedClonesToList();

			// Loop through all spawnedClones from the front (oldest) until one is found
			while (spawnedClonesList.Count > 0)
			{
				clone = spawnedClonesList[0];

				spawnedClonesList.RemoveAt(0);

				if (clone != null)
				{
					DespawnNow(clone, registerDespawned);

					return true;
				}

				if (Warnings == true) Debug.LogWarning("This pool contained a null spawned clone, did you accidentally destroy it?", this);
			}

			return false;
		}

		[ContextMenu("Despawn All")]
		public void DespawnAll()
		{
			// Merge
			MergeSpawnedClonesToList();

			// Despawn
			for (var i = spawnedClonesList.Count - 1; i >= 0; i--)
			{
				var clone = spawnedClonesList[i];

				if (clone != null)
				{
					DespawnNow(clone);
				}
			}

			spawnedClonesList.Clear();

			// Clear all delays
			for (var i = delays.Count - 1; i >= 0; i--)
			{
				LeanClassPool<Delay>.Despawn(delays[i]);
			}

			delays.Clear();
		}

		/// <summary>This will either instantly despawn the specified gameObject, or delay despawn it after t seconds.</summary>
		public void Despawn(GameObject clone, float t = 0.0f)
		{
			if (clone != null)
			{
				// Delay the despawn?
				if (t > 0.0f)
				{
					DespawnWithDelay(clone, t);
				}
				// Despawn now?
				else
				{
					TryDespawn(clone);

					// If this clone was marked for delayed despawn, remove it
					for (var i = delays.Count - 1; i >= 0; i--)
					{
						var delay = delays[i];

						if (delay.Clone == clone)
						{
							delays.RemoveAt(i);
						}
					}
				}
			}
			else
			{
				if (Warnings == true) Debug.LogWarning("You're attempting to despawn a null gameObject", this);
			}
		}

		/// <summary>This method will create an additional prefab clone and add it to the despawned list.</summary>
		[ContextMenu("Preload One More")]
		public void PreloadOneMore()
		{
			if (Prefab != null)
			{
				// Create clone
				var clone = CreateClone(Vector3.zero, Quaternion.identity, null);

				// Add clone to despawned list
				despawnedClones.Add(clone);

				// Deactivate it
				clone.SetActive(false);

				// Move it under this GO
				clone.transform.SetParent(transform, false);

				if (Warnings == true && Capacity > 0 && Total > Capacity) Debug.LogWarning("You've preloaded more than the pool capacity, please verify you're preloading the intended amount.", this);
			}
			else
			{
				if (Warnings == true) Debug.LogWarning("Attempting to preload a null prefab.", this);
			}
		}

		/// <summary>This will preload the pool based on the Preload setting.</summary>
		[ContextMenu("Preload All")]
		public void PreloadAll()
		{
			if (Preload > 0)
			{
				if (Prefab != null)
				{
					for (var i = Total; i < Preload; i++)
					{
						PreloadOneMore();
					}
				}
				else if (Warnings == true)
				{
					if (Warnings == true) Debug.LogWarning("Attempting to preload a null prefab", this);
				}
			}
		}

		protected virtual void Awake()
		{
			PreloadAll();

			if (Persist == true)
			{
				DontDestroyOnLoad(this);
			}
		}

		protected virtual void OnEnable()
		{
			Instances.Add(this);
		}

		protected virtual void OnDisable()
		{
			Instances.Remove(this);
		}

		protected virtual void Update()
		{
			// Decay the life of all delayed destruction calls
			for (var i = delays.Count - 1; i >= 0; i--)
			{
				var delay = delays[i];

				delay.Life -= Time.deltaTime;

				// Skip to next one?
				if (delay.Life > 0.0f)
				{
					continue;
				}

				// Remove and pool delay
				delays.RemoveAt(i); LeanClassPool<Delay>.Despawn(delay);

				// Finally despawn it after delay
				if (delay.Clone != null)
				{
					Despawn(delay.Clone);
				}
				else
				{
					if (Warnings == true) Debug.LogWarning("Attempting to update the delayed destruction of a prefab clone that no longer exists, did you accidentally delete it?", this);
				}
			}
		}

		private void DespawnWithDelay(GameObject clone, float t)
		{
			// If this object is already marked for delayed despawn, update the time and return
			for (var i = delays.Count - 1; i >= 0; i--)
			{
				var delay = delays[i];

				if (delay.Clone == clone)
				{
					if (t < delay.Life)
					{
						delay.Life = t;
					}

					return;
				}
			}

			// Create delay
			var newDelay = LeanClassPool<Delay>.Spawn() ?? new Delay();

			newDelay.Clone = clone;
			newDelay.Life  = t;

			delays.Add(newDelay);
		}

		private void TryDespawn(GameObject clone)
		{
			if (spawnedClonesHashSet.Remove(clone) == true || spawnedClonesList.Remove(clone) == true)
			{
				DespawnNow(clone);
			}
			else
			{
				if (Warnings == true) Debug.LogWarning("You're attempting to despawn a GameObject that wasn't spawned from this pool, make sure your Spawn and Despawn calls match.", clone);
			}
		}

		private void DespawnNow(GameObject clone, bool register = true)
		{
			// Add clone to despawned list
			if (register == true)
			{
				despawnedClones.Add(clone);
			}

			// Messages?
			InvokeOnDespawn(clone);

			// Deactivate it
			clone.SetActive(false);

			// Move it under this GO
			clone.transform.SetParent(transform, false);
		}

		private GameObject CreateClone(Vector3 position, Quaternion rotation, Transform parent)
		{
			var clone = Instantiate(Prefab, position, rotation);

			if (Stamp == true)
			{
				clone.name = Prefab.name + " " + Total;
			}
			else
			{
				clone.name = Prefab.name;
			}

			clone.transform.SetParent(parent, false);

			return clone;
		}

		private void SpawnClone(GameObject clone, Vector3 position, Quaternion rotation, Transform parent)
		{
			// Register
			if (Recycle == true)
			{
				spawnedClonesList.Add(clone);
			}
			else
			{
				spawnedClonesHashSet.Add(clone);
			}

			// Update transform
			var cloneTransform = clone.transform;

			cloneTransform.localPosition = position;
			cloneTransform.localRotation = rotation;

			cloneTransform.SetParent(parent, false);

			// Activate
			clone.SetActive(true);

			// Notifications
			InvokeOnSpawn(clone);
		}

		private void InvokeOnSpawn(GameObject clone)
		{
			switch (Notification)
			{
				case NotificationType.SendMessage: clone.SendMessage("OnSpawn", SendMessageOptions.DontRequireReceiver); break;
				case NotificationType.BroadcastMessage: clone.BroadcastMessage("OnSpawn", SendMessageOptions.DontRequireReceiver); break;
				case NotificationType.PoolableInterface: clone.GetComponents(tempPoolables); for (var i = tempPoolables.Count - 1; i >= 0; i--) tempPoolables[i].OnSpawn(); break;
			}
		}

		private void InvokeOnDespawn(GameObject clone)
		{
			switch (Notification)
			{
				case NotificationType.SendMessage: clone.SendMessage("OnDespawn", SendMessageOptions.DontRequireReceiver); break;
				case NotificationType.BroadcastMessage: clone.BroadcastMessage("OnDespawn", SendMessageOptions.DontRequireReceiver); break;
				case NotificationType.PoolableInterface: clone.GetComponents(tempPoolables); for (var i = tempPoolables.Count - 1; i >= 0; i--) tempPoolables[i].OnDespawn(); break;
			}
		}

		private void MergeSpawnedClonesToList()
		{
			if (spawnedClonesHashSet.Count > 0)
			{
				spawnedClonesList.AddRange(spawnedClonesHashSet);

				spawnedClonesHashSet.Clear();
			}
		}

		public void OnBeforeSerialize()
		{
			MergeSpawnedClonesToList();
		}

		public void OnAfterDeserialize()
		{
			if (Recycle == false)
			{
				for (var i = spawnedClonesList.Count - 1; i >= 0; i--)
				{
					var clone = spawnedClonesList[i];

					spawnedClonesHashSet.Add(clone);
				}

				spawnedClonesList.Clear();
			}
		}
	}
}