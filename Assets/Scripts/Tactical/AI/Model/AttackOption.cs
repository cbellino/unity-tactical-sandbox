using UnityEngine;
using System.Collections.Generic;
using Tactical.Core.Enums;
using Tactical.Core.Extensions;
using Tactical.Grid.Component;
using Tactical.Actor.Component;

namespace Tactical.AI.Model {

	public class AttackOption {

		private class Mark {
			public Tile tile;
			public bool isMatch;

			public Mark (Tile tile, bool isMatch) {
				this.tile = tile;
				this.isMatch = isMatch;
			}
		}

		public Tile target;
		public Directions direction;
		public List<Tile> areaTargets = new List<Tile>();
		public bool isCasterMatch;
		public Tile bestMoveTile { get; private set; }
		public int bestAngleBasedScore { get; private set; }
		private List<Mark> marks = new List<Mark>();
		private List<Tile> moveTargets = new List<Tile>();

		public void AddMoveTarget (Tile tile) {
			// Dont allow moving to a tile that would negatively affect the caster
			if (!isCasterMatch && areaTargets.Contains(tile)) {
				return;
			}
			moveTargets.Add(tile);
		}

		public void AddMark (Tile tile, bool isMatch) {
			marks.Add (new Mark(tile, isMatch));
		}

		// Scores the option based on how many of the targets are of the desired type
		public int GetScore (Unit caster, Ability ability) {
			GetBestMoveTarget(caster, ability);
			if (bestMoveTile == null) {
				return 0;
			}

			int score = 0;
			for (int i = 0; i < marks.Count; ++i) {
				if (marks[i].isMatch) {
					score++;
				} else {
					score--;
				}
			}

			if (isCasterMatch && areaTargets.Contains(bestMoveTile)) {
				score++;
			}

			return score;
		}

		// Returns the tile which is the most effective point for the caster to attack from
		private void GetBestMoveTarget (Unit caster, Ability ability) {
			if (moveTargets.Count == 0) {
				return;
			}

			if (IsAbilityAngleBased(ability)) {
				bestAngleBasedScore = int.MinValue;
				Tile startTile = caster.tile;
				Directions startDirection = caster.dir;
				caster.dir = direction;

				var bestOptions = new List<Tile>();
				for (int i = 0; i < moveTargets.Count; ++i) {
					caster.Place(moveTargets[i]);
					int score = GetAngleBasedScore(caster);
					if (score > bestAngleBasedScore) {
						bestAngleBasedScore = score;
						bestOptions.Clear();
					}

					if (score == bestAngleBasedScore) {
						bestOptions.Add(moveTargets[i]);
					}
				}

				caster.Place(startTile);
				caster.dir = startDirection;

				FilterBestMoves(bestOptions);
				bestMoveTile = bestOptions[Random.Range(0, bestOptions.Count)];
			} else {
				bestMoveTile = moveTargets[Random.Range(0, moveTargets.Count)];
			}
		}

		// Indicates whether the angle of attack is an important factor in the
		// application of this ability
		private bool IsAbilityAngleBased (Ability ability) {
			bool isAngleBased = false;
			for (int i = 0; i < ability.transform.childCount; ++i) {
				HitRate hr = ability.transform.GetChild(i).GetComponent<HitRate>();
				if (hr.IsAngleBased) {
					isAngleBased = true;
					break;
				}
			}
			return isAngleBased;
		}

		// Scores the option based on how many of the targets are a match
		// and considers the angle of attack to each mark
		private int GetAngleBasedScore (Unit caster) {
			int score = 0;
			for (int i = 0; i < marks.Count; ++i) {
				int value = marks[i].isMatch ? 1 : -1;
				int multiplier = MultiplierForAngle(caster, marks[i].tile);
				score += value * multiplier;
			}
			return score;
		}

		private void FilterBestMoves (List<Tile> list) {
			if (!isCasterMatch) {
				return;
			}

			bool canTargetSelf = false;
			for (int i = 0; i < list.Count; ++i) {
				if (areaTargets.Contains(list[i])) {
					canTargetSelf = true;
					break;
				}
			}

			if (canTargetSelf) {
				for (int i = list.Count - 1; i >= 0; --i) {
					if (!areaTargets.Contains(list[i])) {
						list.RemoveAt(i);
					}
				}
			}
		}

		private int MultiplierForAngle (Unit caster, Tile tile) {
			if (tile.content == null) {
				return 0;
			}

			Unit defender = tile.content.GetComponentInChildren<Unit>();
			if (defender == null) {
				return 0;
			}

			Facings facing = caster.GetFacing(defender);
			if (facing == Facings.Back) {
				return 90;
			}
			if (facing == Facings.Side) {
				return 75;
			}
			return 50;
		}

	}


}
