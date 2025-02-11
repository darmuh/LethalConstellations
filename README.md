# Lethal Constellations

## WARNING: BEFORE ANY UPDATE OF THIS MOD PLEASE BACKUP YOUR EXISTING CONFIG OR YOUR DATA CAN/WILL BE LOST

### Separate your moons list by constellations. 
- This mod will dynamically change your moons list depending on a configurable constellations listing.

**NOTE:** This mod is still in early development. Please report any issues you find as you are essentially a beta tester.

### Current Features:
- If a moon is not inside the current constellation it will be hidden from the moons page and locked (using LLL)
- Contains two config files. A static config and a generated one.
	- Static config will dictate what is generated in the generated config.
	- Generated config is generated after first lobby load.
		- Currently the generated config cannot be modified using LethalConfig.
- Interactive constellations menu!
	- Customizable menu controls
	- No more need to type multiple keywords, just type your constellation keyword and start routing from the menu!
- Customizable keywords/text.
	- Dont like the word constellation? Change it in the config!
	- When adding terminal keywords, this mod will NOT replace any already existing keywords.
	- Customizable text for all different types of terminal commands relating to the constellations.
	- Optional shortcut keywords can also be added to each constellation.
- Current Constellation will be updated on lobby load, works between save files!
- Starter Constellation can be set in config as a specific constellation or random!
- The possible starting constellations can also be fine tuned in the config to a set list when using random starter constellations.
- Moon prices can also be modified by this mod or left to be handled elsewhere.
- Set routing to a specific constellation to cost credits or allow for your constellations to be free.
- By assigning positional data you can also set your constellations to dynamically update it's pricing based on it's current distance using an algorithm of your choice!
- Set a constellation to be a one-time-purchase, this purchase will be remembered for each save file.
	- In order to sync between players this feature requires [LethalNetworkAPI](https://thunderstore.io/c/lethal-company/p/xilophor/LethalNetworkAPI/) to be present.
- Routing to a new constellation will take you to that constellation's default moon.
	- This essentially makes the cost of routing to this moon however much it costs to route to your constellation.
- Set which constellations the company can be routed to in the constellations config
- Hide special constellations from the menu or if you cant afford it (with configuration options)
- Allow for a moon to remain hidden while being assigned to a constellation.
- This mod does not do any patching, all game patching is done by OpenLib & LethalLevelLoader.
- Compatibility with [LethalMoonUnlocks](https://thunderstore.io/c/lethal-company/p/explodingMods/LethalMoonUnlocks/)
- Compatibility with [LateGameUpgrades](https://thunderstore.io/c/lethal-company/p/malco/Lategame_Upgrades/)' Efficient Engines Upgrade

### For other mod devs looking to add compatibility:
- Subscribe to the RouteConstellationSuccess event in NewEvents.cs to call your own code when routing to a new constellation.
- Access ConstellationStuff in Collections.cs for a listing of all constellations and their ClassMapper.cs properties
- Feel free to leave me a message on the [github page](https://github.com/darmuh/LethalConstellations) or the [discord thread](https://discord.com/channels/1168655651455639582/1269406512313405571) with anything you might need to build compatibility or external features.

### Having issues setting up the config?
- Try [OpenLib's Bepinex Config Management Webpage (v0.1.2)](https://darmuh.github.io/OpenLib/OpenLib/Website/Examples/ConfigManagement.html)
	- Simply upload your config file to the page and it will generate a form for the given config file.
	- Once you are done modifying the form on the webpage you can then click the button at the bottom of the page to compare your changes to the original config file.
	- When the page is done comparing your changes, it will save/download a new config file to your device.
	- Replace your original config file with this new one to use it in game!
 - You can also try following along the [steps here](https://thunderstore.io/c/lethal-company/p/darmuh/LethalConstellations/wiki/2563-how-to-use-webconfig/) to use the WebConfig file that you can generate from this mod.

### If you have any ideas on how to make this mod better please feel free to reach out!

- [Icon background](https://chandra.harvard.edu/photo/2024/25th/more.html) 
	- Credit: X-ray: NASA/CXC/Ludwig Maximilian Univ./T. Preibisch et al.; Infrared: NASA/ESA/CSA/STScI; Image processing: NASA/CXC/SAO/N. Wolk
