namespace AwwScrap.Helpers
{
	class Definitions
	{
		/* Original Intent (Model textures based on original idea, so recorded for later use)
		 * Scrap 1: 3.0 Iron, 1.25 Nickle, 0.75 Cobalt
		 * Scrap 2: 3.75 Silicon
		 * Scrap 3: 3.75 Iron, 5.0 Stone, 1.25 Silver
		 * Scrap 4: 7.5 Iron, 2.5 Cobalt, 0.25 Gold, 0.1 Platinum
		 * Scrap 5: 1.25 Silver, 2.50 Gold, 55.0 Cobalt, 150.0 Iron
		 * Scrap 6: 15.0 Iron, 17.5 Nickel, 5.0 Silver
		 * Scrap 7: 1.25 Iron, 2.5 Nickel
		 * Scrap 8: 0.2 Silicon, 1.0 Iron
		 * Scrap 9: 0.13 Silicon, 0.5 Magnesium
		 * Scrap 10: 2.5 Nickel, 2.0 Silicon
		 * Scrap 11: 2.5 Iron, 0.25 Silicon, 0.5 Nickel
		 * Scrap 12: No custom model; reuse standard scrap, name: Iron Scrap -- 5.25 Iron
		 * Scrap Default: Anything that doesn't fit the above criteria
		 * 
		 * 
		 * Modified Intent (Each component will have dedicated scrap @ 0.75 original creation value)
		 *
		 * Bulletproof Glass: 
		 *		5.0 Silicon 
		 *		Scrap 2 @ 3.75 Silicon
		 *
		 * Computer: 
		 *		0.17 Iron, 0.07 Silicon 
		 *		Scrap 8 @ 0.1275 Iron, 0.0525 Silicon
		 *
		 * Construction Component: 
		 *		3.33 Iron 
		 *		Scrap 12 @ 2.4975 Iron
		 *
		 * Detector Component: 
		 *		1.67 Iron, 5.0 Nickel 
		 *		Scrap 7 @ 1.2525 Iron, 3.75 Nickel
		 *
		 * Display: 
		 *		0.33 Iron, 1.67 Silicon
		 *		Scrap 8 @ 0.2475 Iron, 1.2525 Silicon
		 *
		 * Explosives: 
		 *		0.17 Silicon, 0.67 Magnesium
		 *		Scrap 9 @ 0.2475 Iron, 1.2525 Silicon
		 *
		 * Girder: 
		 *		2.33 Iron
		 *		Scrap 12 @ 1.7475 Iron
		 *
		 * Gravity Component: 
		 *		1.67 Silver, 3.33 Gold, 73.33 Cobalt, 200.0 Iron
		 *		Scrap 5 @ 1.2525 Silver, 2.4975 Gold, 54.9975 Cobalt, 150.0 Iron
		 *
		 * Interior Plate: 
		 *		1.17 Iron 
		 *		Scrap 12 @ 0.8875 Iron
		 *
		 * Large Steel Tube: 
		 *		10.0 Iron 
		 *		Scrap 12 @ 7.5 Iron
		 *
		 * Medical Component: 
		 *		20.0 Iron, 23.33 Nickel, 6.67 Silver
		 *		Scrap 6 @ 15.0 Iron, 17.4975 Nickel, 5.0025 Silver
		 *
		 * Metal Grid: 
		 *		4.0 Iron, 1.67 Nickel, 1.0 Cobalt
		 *		Scrap 1 @ 3.0 Iron, 1.2525 Nickel, 0.75 Cobalt
		 *
		 * Motor: 
		 *		6.67 Iron, 1.67 Nickel 
		 *		Scrap 7 @ 5.0025 Iron, 1.2525 Nickel
		 *
		 * Power Cell: 
		 *		3.33 Iron, 0.33 Silicon, 0.67 Nickel 
		 *		Scrap 11 @ 2.4975 Iron, 0.2475 Silicon, 0.5025 Nickel
		 *
		 * Radio-comm Component: 
		 *		2.67 Iron, 0.33 Silicon 
		 *		Scrap 8 @ 2.0025 Iron, 0.2475 Silicon
		 *
		 * Reactor Component: 
		 *		5.0 Iron, 6.67 Gravel, 1.67 Silver
		 *		Scrap 3 @ 3.75 Iron, 5.0025 Gravel, 1.2525 Silver
		 *
		 * Small Steel Tube: 
		 *		1.67 Iron 
		 *		Scrap 12 @ 1.2525 Iron
		 *
		 * Solar Cell: 
		 *		3.33 Nickel, 2.67 Silicon 
		 *		Scrap 10 @ 2.4975 Nickel, 2.0025 Silicon
		 *
		 * Steel Plate: 
		 *		7.0 Iron 
		 *		Scrap 12 @ 5.25 Iron
		 *
		 * Superconductor: 
		 *		3.33 Iron, 0.67 Gold 
		 *		Scrap 4 @ 2.4975 Iron, 0.5025 Gold
		 *
		 * Thruster Component: 
		 *		10 Iron, 3.33 Cobalt, 0.33 Gold, 0.13 Platinum 
		 *		Scrap 4 @ 7.5 Iron, 2.4975 Cobalt, 0.2475 Gold, 0.0975 Platinum
		 *
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
			{ MyStringHash.GetOrCompute("Thrust"), "ThrustScrap" }

			<BlueprintClassEntries>
				<Entry Class="Ingots" BlueprintSubtypeId="BulletproofGlassToIngot" />
				<Entry Class="Ingots" BlueprintSubtypeId="ComputerToIngot" />
				<Entry Class="Ingots" BlueprintSubtypeId="ConstructionToIngot" />
				<Entry Class="Ingots" BlueprintSubtypeId="DetectorToIngot" />
				<Entry Class="Ingots" BlueprintSubtypeId="DisplayToIngot" />
				<Entry Class="Ingots" BlueprintSubtypeId="ExplosivesToIngot" />
				<Entry Class="Ingots" BlueprintSubtypeId="GirderToIngot" />
				<Entry Class="Ingots" BlueprintSubtypeId="GravityGeneratorToIngot" />
				<Entry Class="Ingots" BlueprintSubtypeId="InteriorPlateToIngot" />
				<Entry Class="Ingots" BlueprintSubtypeId="LargeTubeToIngot" />
				<Entry Class="Ingots" BlueprintSubtypeId="MedicalToIngot" />
				<Entry Class="Ingots" BlueprintSubtypeId="MetalGridToIngot" />
				<Entry Class="Ingots" BlueprintSubtypeId="MotorToIngot" />
				<Entry Class="Ingots" BlueprintSubtypeId="PowerCellToIngot" />
				<Entry Class="Ingots" BlueprintSubtypeId="RadioCommunicationToIngot" />
				<Entry Class="Ingots" BlueprintSubtypeId="ReactorToIngot" />
				<Entry Class="Ingots" BlueprintSubtypeId="SmallTubeToIngot" />
				<Entry Class="Ingots" BlueprintSubtypeId="SolarCellToIngot" />
				<Entry Class="Ingots" BlueprintSubtypeId="SteelPlateToIngot" />
				<Entry Class="Ingots" BlueprintSubtypeId="SuperconductorToIngot" />
				<Entry Class="Ingots" BlueprintSubtypeId="ThrustToIngot" />
			</BlueprintClassEntries>
		 *
		 */
	}
}
