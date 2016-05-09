using UnityEngine;
using Tactical.Core;
using Tactical.Battle.Controller;

namespace Tactical.Actor.Component {

	public class PoisonStatusEffect : StatusEffect {

		private Unit owner;

		private void OnEnable () {
			owner = GetComponentInParent<Unit>();
			if (owner) {
				this.AddObserver(OnNewTurn, TurnOrderController.TurnCompletedNotification, owner);
			}
		}

		private void OnDisable () {
			this.RemoveObserver(OnNewTurn, TurnOrderController.TurnCompletedNotification, owner);
		}

		private void OnNewTurn (object sender, object args) {
			Stats s = GetComponentInParent<Stats>();
			int currentHP = s[StatType.HP];
			int maxHP = s[StatType.MHP];
			int reduce = Mathf.FloorToInt(maxHP * 0.1f);
			s.SetValue(StatType.HP, (currentHP - reduce), false);
		}

	}

}
