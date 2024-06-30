using Colossal.Serialization.Entities;

using Game.Net;
using Game.Prefabs;
using Game.Serialization;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadBuilder.Systems
{
	[HarmonyLib.HarmonyPatch(typeof(ResolvePrefabsSystem), "OnUpdate")]
	public class MyPatchClass
	{
		// Prefix patch method
		public static void Prefix() 
		{
			Mod.Log.Info("PrefabSystem 1");
			// Code to execute before the original method
		}

		// Postfix patch method
		public static void Postfix() 
		{
			Mod.Log.Info("PrefabSystem 2");
			// Code to execute after the original method
		}
	}
}
