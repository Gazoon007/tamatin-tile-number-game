using System;
using Core.Helper;
using Selection_Mode;
using Tile;
using UnityEngine;

namespace Core
{
	/// <summary>
	/// Responsible for save turn value, counting, and also update the turn.
	/// </summary>
	public class GameManager : Singleton<GameManager>
	{
		[SerializeField] private SelectionModeData[] selectionModes;

		private PlayerController _playerController;
		private int _tilesZeroValueQty;
		private SelectionModeData _selectionMode;
		private int _turn = 1;

		public Action OnFinishedGame;
		public int Turn => _turn;
		public SelectionModeData SelectionMode => _selectionMode;
		public SelectionModeData[] SelectionModes => selectionModes;
		
		private void OnEnable()
		{
			_playerController = GetComponent<PlayerController>();

			TileGenerator.Instance.OnGeneratorFinished += GetReferenceOfAllTiles;
			if (_playerController != null)
				_playerController.OnClickToTile += UpdateTurn;
		}

		private void OnDisable()
		{
			if(!gameObject.scene.isLoaded) return;
			TileGenerator.Instance.OnGeneratorFinished -= GetReferenceOfAllTiles;
			_playerController.OnClickToTile -= UpdateTurn;
		}


		private void GetReferenceOfAllTiles()
		{
			// This is passing the delegated method check game state to tile manager, so we let the TileManager to
			// iterate list of tiles, but for the tile itself we want to execute our delegated method that we pass from
			// game manager
			//
			// This is controversial, a kinda bit messy, but not violate SRP, also there is string parameter that i don't
			// like.
			// TODO: Think another solution if possible for the greater cleanliness of the code!
			TileManager.Instance.ExecuteMethodPerTile("OnReachedZeroValueProp", CheckGameState);
		}

		private void CheckGameState()
		{
			_tilesZeroValueQty += 1;
			if (Turn > 1 && _tilesZeroValueQty == TileManager.Instance.TotalTiles)
			{
				FinishGame();
			}
		}

		private void FinishGame()
		{
			OnFinishedGame?.Invoke();
			ResetGame();
		}

		private void UpdateTurn()
		{
			_turn += 1;
		}

		public void ResetGame()
		{
			_turn = 1;
			_tilesZeroValueQty = 0;
		}

		public void ChangeSelection(Selections selection, Vector2 attackAndRangeValue)
		{
			_selectionMode = null;
			foreach (var selectionMode in selectionModes)
			{
				if (selection == selectionMode.selection && 
				    (int)attackAndRangeValue.x == selectionMode.attackValue &&
				    (int)attackAndRangeValue.y == selectionMode.rangeTile)
					_selectionMode = selectionMode;
			}
		}
	}
}