using System.Collections.Generic;
using VRage.Utils;

namespace AwwScrap.Support
{
	public static class Statics
	{
		public static readonly Dictionary<MyStringHash, ScrapAttributes> ScrapAttributesDictionary = new Dictionary<MyStringHash, ScrapAttributes>
		{
			// Vanilla
			{ MyStringHash.GetOrCompute("BulletproofGlassScrap"), new ScrapAttributes(10.5f, 5.6f) },
			{ MyStringHash.GetOrCompute("ComputerScrap"), new ScrapAttributes(0.14f, 0.7f) },
			{ MyStringHash.GetOrCompute("ConstructionScrap"), new ScrapAttributes(5.6f, 1.4f) },
			{ MyStringHash.GetOrCompute("DetectorScrap"), new ScrapAttributes(3.5f, 4.2f) },
			{ MyStringHash.GetOrCompute("DisplayScrap"), new ScrapAttributes(5.6f, 4.2f) },
			{ MyStringHash.GetOrCompute("ExplosivesScrap"), new ScrapAttributes(1.4f, 1.4f) },
			{ MyStringHash.GetOrCompute("GirderScrap"), new ScrapAttributes(4.2f, 1.4f) },
			{ MyStringHash.GetOrCompute("GravityGeneratorScrap"), new ScrapAttributes(560f, 140f) },
			{ MyStringHash.GetOrCompute("InteriorPlateScrap"), new ScrapAttributes(2.1f, 3.5f) },
			{ MyStringHash.GetOrCompute("LargeTubeScrap"), new ScrapAttributes(17.5f, 26.6f) },
			{ MyStringHash.GetOrCompute("MedicalScrap"), new ScrapAttributes(105f, 112f) },
			{ MyStringHash.GetOrCompute("MetalGridScrap"), new ScrapAttributes(4.2f, 10.5f) },
			{ MyStringHash.GetOrCompute("MotorScrap"), new ScrapAttributes(16.8f, 5.6f) },
			{ MyStringHash.GetOrCompute("PowerCellScrap"), new ScrapAttributes(17.5f, 28f) },
			{ MyStringHash.GetOrCompute("RadioCommunicationScrap"), new ScrapAttributes(5.6f, 49f) },
			{ MyStringHash.GetOrCompute("ReactorScrap"), new ScrapAttributes(17.5f, 5.6f) },
			{ MyStringHash.GetOrCompute("SmallTubeScrap"), new ScrapAttributes(2.8f, 1.4f) },
			{ MyStringHash.GetOrCompute("SolarCellScrap"), new ScrapAttributes(4.2f, 8.4f) },
			{ MyStringHash.GetOrCompute("SteelPlateScrap"), new ScrapAttributes(14f, 2.1f) },
			{ MyStringHash.GetOrCompute("SuperconductorScrap"), new ScrapAttributes(10.5f, 5.6f) },
			{ MyStringHash.GetOrCompute("ThrustScrap"), new ScrapAttributes(28f, 7f) },

			//{ MyStringHash.GetOrCompute("BulletproofGlassScrap"), new ScrapAttributes(11.25f, 4.82f) },
			//{ MyStringHash.GetOrCompute("ComputerScrap"), new ScrapAttributes(0.52f, 0.11f) },
			//{ MyStringHash.GetOrCompute("ConstructionScrap"), new ScrapAttributes(6f, 0.77f) },
			//{ MyStringHash.GetOrCompute("DetectorScrap"), new ScrapAttributes(15f, 1.73f) },
			//{ MyStringHash.GetOrCompute("DisplayScrap"), new ScrapAttributes(4.5f, 1.7f) },
			//{ MyStringHash.GetOrCompute("ExplosivesScrap"), new ScrapAttributes(1.87f, 1.02f) },
			//{ MyStringHash.GetOrCompute("GirderScrap"), new ScrapAttributes(4.5f, 0.57f) },
			//{ MyStringHash.GetOrCompute("GravityGeneratorScrap"), new ScrapAttributes(626.25f, 76.38f) },
			//{ MyStringHash.GetOrCompute("InteriorPlateScrap"), new ScrapAttributes(2.25f, 0.29f) },
			//{ MyStringHash.GetOrCompute("LargeTubeScrap"), new ScrapAttributes(22.5f, 2.86f) },
			//{ MyStringHash.GetOrCompute("MedicalScrap"), new ScrapAttributes(112.5f, 13.02f) },
			//{ MyStringHash.GetOrCompute("MetalGridScrap"), new ScrapAttributes(15f, 1.82f) },
			//{ MyStringHash.GetOrCompute("MotorScrap"), new ScrapAttributes(18.75f, 2.33f) },
			//{ MyStringHash.GetOrCompute("PowerCellScrap"), new ScrapAttributes(9.75f, 1.44f) },
			//{ MyStringHash.GetOrCompute("RadioCommunicationScrap"), new ScrapAttributes(6.75f, 1.08f) },
			//{ MyStringHash.GetOrCompute("ReactorScrap"), new ScrapAttributes(30f, 7.34f) },
			//{ MyStringHash.GetOrCompute("SmallTubeScrap"), new ScrapAttributes(3.75f, 0.48f) },
			//{ MyStringHash.GetOrCompute("SolarCellScrap"), new ScrapAttributes(6.75f, 2.12f) },
			//{ MyStringHash.GetOrCompute("SteelPlateScrap"), new ScrapAttributes(15.75f, 2f) },
			//{ MyStringHash.GetOrCompute("SuperconductorScrap"), new ScrapAttributes(9f, 1.03f) },
			//{ MyStringHash.GetOrCompute("ThrustScrap"), new ScrapAttributes(31.05f, 3.75f) },

			// Mods
			{ MyStringHash.GetOrCompute("FieldEmitterScrap"), new ScrapAttributes(0.217f, 0.595f) }
		};

		public static readonly Dictionary<MyStringHash, string> ComponentDictionary = new Dictionary<MyStringHash, string>
		{
			// Vanilla
			{ MyStringHash.GetOrCompute("BulletproofGlass"), "BulletproofGlassScrap" },
			{ MyStringHash.GetOrCompute("Computer"), "ComputerScrap" },
			{ MyStringHash.GetOrCompute("Construction"), "ConstructionScrap" },
			{ MyStringHash.GetOrCompute("Detector"), "DetectorScrap" },
			{ MyStringHash.GetOrCompute("Display"), "DisplayScrap" },
			{ MyStringHash.GetOrCompute("Explosives"), "ExplosivesScrap" },
			{ MyStringHash.GetOrCompute("Girder"), "GirderScrap" },
			{ MyStringHash.GetOrCompute("GravityGenerator"), "GravityGeneratorScrap" },
			{ MyStringHash.GetOrCompute("InteriorPlate"), "InteriorPlateScrap" },
			{ MyStringHash.GetOrCompute("LargeTube"), "LargeTubeScrap" },
			{ MyStringHash.GetOrCompute("Medical"), "MedicalScrap" },
			{ MyStringHash.GetOrCompute("MetalGrid"), "MetalGridScrap" },
			{ MyStringHash.GetOrCompute("Motor"), "MotorScrap" },
			{ MyStringHash.GetOrCompute("PowerCell"), "PowerCellScrap" },
			{ MyStringHash.GetOrCompute("RadioCommunication"), "RadioCommunicationScrap" },
			{ MyStringHash.GetOrCompute("Reactor"), "ReactorScrap" },
			{ MyStringHash.GetOrCompute("SmallTube"), "SmallTubeScrap" },
			{ MyStringHash.GetOrCompute("SolarCell"), "SolarCellScrap" },
			{ MyStringHash.GetOrCompute("SteelPlate"), "SteelPlateScrap" },
			{ MyStringHash.GetOrCompute("Superconductor"), "SuperconductorScrap" },
			{ MyStringHash.GetOrCompute("Thrust"), "ThrustScrap" },

			// Mods
			{ MyStringHash.GetOrCompute("ShieldComponent"), "FieldEmitterScrap" }
		};

		public static readonly List<MyStringHash> AwwScrapSubTypeIds = new List<MyStringHash>
		{
			// Vanilla
			MyStringHash.GetOrCompute("BulletproofGlassToIngot"),
			MyStringHash.GetOrCompute("ComputerToIngot"),
			MyStringHash.GetOrCompute("ConstructionToIngot"),
			MyStringHash.GetOrCompute("DetectorToIngot"),
			MyStringHash.GetOrCompute("DisplayToIngot"),
			MyStringHash.GetOrCompute("ExplosivesToIngot"),
			MyStringHash.GetOrCompute("GirderToIngot"),
			MyStringHash.GetOrCompute("GravityGeneratorToIngot"),
			MyStringHash.GetOrCompute("InteriorPlateToIngot"),
			MyStringHash.GetOrCompute("LargeTubeToIngot"),
			MyStringHash.GetOrCompute("MedicalToIngot"),
			MyStringHash.GetOrCompute("MetalGridToIngot"),
			MyStringHash.GetOrCompute("MotorToIngot"),
			MyStringHash.GetOrCompute("PowerCellToIngot"),
			MyStringHash.GetOrCompute("RadioCommunicationToIngot"),
			MyStringHash.GetOrCompute("ReactorToIngot"),
			MyStringHash.GetOrCompute("SmallTubeToIngot"),
			MyStringHash.GetOrCompute("SolarCellToIngot"),
			MyStringHash.GetOrCompute("SteelPlateToIngot"),
			MyStringHash.GetOrCompute("SuperconductorToIngot"),
			MyStringHash.GetOrCompute("ThrustToIngot"),

			// Mods
			MyStringHash.GetOrCompute("ShieldComponentToIngot"),
		};

		public static readonly List<string> SKitScrapList = new List<string>()
		{
			"Scrap",
			"ComputerScrap",
			"ConstructionScrap",
			"DisplayScrap",
			"GirderScrap",
			"InteriorPlateScrap",
			"MotorScrap",
			"SolarCellScrap",
			"SteelPlateScrap"
		};

		public static readonly bool SkipTieredTech = true;

	}
}
