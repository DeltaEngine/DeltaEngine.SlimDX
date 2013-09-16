using System;
using System.Collections.Generic;
using DeltaEngine.Core;
using DeltaEngine.Datatypes;
using DeltaEngine.Scenes;
using DeltaEngine.Scenes.UserInterfaces.Controls;

namespace $safeprojectname$
{
	public class LevelBathRoom : Scene, IDisposable
	{
		public LevelBathRoom()
		{
			InitializeGameManager();
			CreateLevel();
		}

		private void InitializeGameManager()
		{
			manager = new Manager(6.0f) {
				playerData = new Manager.PlayerData {
					ResourceFinances = 190
				}
			};
		}

		private readonly List<string> inactiveButtonsTagList = new List<string> {
			Names.ButtonAcidTower,
			Names.ButtonImpactTower
		};
		private Manager manager;

		private void CreateLevel()
		{
			bathRoom = new Level(Names.LevelsBathRoom);
			manager.Level = bathRoom;
			SetupBathRoomScene();
		}

		private Level bathRoom;

		private void SetupBathRoomScene()
		{
			bathRoomScene = new CreateSceneFromXml(Names.XmlSceneBathroom, Messages.BathRoomMessages());
			foreach (InteractiveButton button in bathRoomScene.InteractiveButtonList)
				AttachButtonEvents(button.GetTags() [0], button);
		}

		private CreateSceneFromXml bathRoomScene;

		private void AttachButtonEvents(string buttonName, InteractiveButton button)
		{
			switch (buttonName)
			{
				case Names.ButtonRefresh:
					button.Clicked += Refresh;
					break;
				case Names.ButtonCreep:
					button.Clicked += SpawnCreep;
					break;
				case Names.ButtonNext:
					button.Clicked += NextDialogue;
					break;
				case Names.ButtonContinue:
					button.Clicked += LoadLivingRoomLevel;
					break;
			}
		}

		private void Refresh()
		{
			manager.Dispose();
			bathRoomScene.MessageCount = 0;
		}

		private void SpawnCreep()
		{
			if (!manager.Contains<InputCommands>())
				manager.Add(new InputCommands(manager, inactiveButtonsTagList));

			var gridData = bathRoom.Get<Level.GridData>();
			var randomWaypointsList = SelectRandomWaypointList(gridData);
			var startGridPos = randomWaypointsList [0];
			var position = Game.CameraAndGrid.Grid.PropertyMatrix [startGridPos.Item1, 
				startGridPos.Item2].MidPoint;
			manager.CreateCreep(position, Names.CreepCottonMummy, MovementData(startGridPos, 
				randomWaypointsList.GetRange(1, randomWaypointsList.Count - 1)));
		}

		private static List<Tuple<int, int>> SelectRandomWaypointList(Level.GridData data)
		{
			var randomNo = Randomizer.Current.Get(0, 1);
			return data.CreepPathsList [randomNo];
		}

		private static MovementInGrid.MovementData MovementData(Tuple<int, int> startPos, 
			List<Tuple<int, int>> waypoints)
		{
			return new MovementInGrid.MovementData {
				Velocity = new Vector(0.2f, 0.0f, 0.2f),
				StartGridPos = startPos,
				Waypoints = waypoints
			};
		}

		private void NextDialogue()
		{
			if (++bathRoomScene.MessageCount > Messages.BathRoomMessages().Length)
				SpawnCreep();

			bathRoomScene.NextDialogue();
		}

		private void LoadLivingRoomLevel()
		{
			Dispose();
			new LevelLivingRoom();
		}

		public new void Dispose()
		{
			bathRoomScene.Dispose();
			bathRoom.Dispose();
			manager.Dispose();
		}
	}
}