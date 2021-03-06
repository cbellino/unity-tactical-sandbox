using UnityEngine;
using Tactical.Core.Enums;
using Tactical.Actor.Component;

namespace Tactical.Actor.Factory {

	public static class UnitFactory {

		public static GameObject Create (string name, int level) {
			UnitRecipe recipe = Resources.Load<UnitRecipe>("Unit Recipes/" + name);
			if (recipe == null) {
				Debug.LogError("No Unit Recipe for name: " + name);
				return null;
			}
			return Create(recipe, level);
		}

		public static GameObject Create (UnitRecipe recipe, int level) {
			GameObject obj = InstantiatePrefab("Units/" + recipe.model);
			obj.name = recipe.name;
			obj.AddComponent<Unit>();
			AddStats(obj);
			AddMovementType(obj, recipe.movementType);
			obj.AddComponent<Status>();
			obj.AddComponent<Equipment>();
			AddJob(obj, recipe.job);
			AddRank(obj, level);
			obj.AddComponent<Health>();
			obj.AddComponent<Mana>();
			AddAttack(obj, recipe.attack);
			AddAbilityCatalog(obj, recipe.abilityCatalog);
			AddAlliance(obj, recipe.alliance);
			AddAttackPattern(obj, recipe.strategy);
			return obj;
		}

		private static GameObject InstantiatePrefab (string name) {
			GameObject prefab = Resources.Load<GameObject>(name);
			if (prefab == null) {
				Debug.LogError("No Prefab for name: " + name);
				return new GameObject(name);
			}
			GameObject instance = GameObject.Instantiate(prefab);
			instance.name = instance.name.Replace("(Clone)", "");

			return instance;
		}

		private static void AddStats (GameObject obj) {
			Stats s = obj.AddComponent<Stats>();
			s.SetValue(StatTypes.LVL, 1, false);
		}

		private static void AddJob (GameObject obj, string name) {
			GameObject instance = InstantiatePrefab("Jobs/" + name);
			instance.transform.SetParent(obj.transform);
			Job job = instance.GetComponent<Job>();
			job.Employ();
			job.LoadDefaultStats();
		}

		private static void AddMovementType (GameObject obj, MovementTypes type) {
			switch (type) {
			case MovementTypes.Walk:
				obj.AddComponent<WalkMovement>();
				break;
			case MovementTypes.Fly:
				obj.AddComponent<FlyMovement>();
				break;
			case MovementTypes.Teleport:
				obj.AddComponent<TeleportMovement>();
				break;
			}
		}

		private static void AddAlliance (GameObject obj, Alliances type) {
			Tactical.Actor.Component.Alliance alliance = obj.AddComponent<Tactical.Actor.Component.Alliance>();
			alliance.type = type;
		}

		private static void AddRank (GameObject obj, int level) {
			ExperienceRank rank = obj.AddComponent<ExperienceRank>();
			rank.Init(level);
		}

		private static void AddAttack (GameObject obj, string name) {
			GameObject instance = InstantiatePrefab("Abilities/" + name);
			instance.transform.SetParent(obj.transform);
		}

		private static void AddAbilityCatalog (GameObject obj, string name) {
			var main = new GameObject("Ability Catalog");
			main.transform.SetParent(obj.transform);
			main.AddComponent<AbilityCatalog>();

			if (name == "") {
				Debug.Log("No Ability Catalog Recipe specified.");
				return;
			}

			AbilityCatalogRecipe recipe = Resources.Load<AbilityCatalogRecipe>("Ability Catalog Recipes/" + name);

			if (recipe == null) {
				Debug.LogError("No Ability Catalog Recipe Found: " + name);
				return;
			}

			for (int i = 0; i < recipe.categories.Length; ++i) {
				var category = new GameObject( recipe.categories[i].name );
				category.transform.SetParent(main.transform);

				for (int j = 0; j < recipe.categories[i].entries.Length; ++j) {
					if (recipe.categories[i].entries[j] != "") {
						string abilityName = string.Format("Abilities/{0}/{1}", recipe.categories[i].name, recipe.categories[i].entries[j]);
						GameObject ability = InstantiatePrefab(abilityName);
						ability.name = recipe.categories[i].entries[j];
						ability.transform.SetParent(category.transform);
					}
				}
			}
		}

		private static void AddAttackPattern (GameObject obj, string name) {
			Driver driver = obj.AddComponent<Driver>();
			if (string.IsNullOrEmpty(name)) {
				driver.normal = Drivers.Human;
			} else {
				driver.normal = Drivers.Computer;
				GameObject instance = InstantiatePrefab("Attack Patterns/" + name);
				instance.transform.SetParent(obj.transform);
			}
		}

	}

}
