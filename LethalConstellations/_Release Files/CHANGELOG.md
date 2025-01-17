# Lethal Constellations Change Log

All notable changes to this project will be documented in this file.
 
The format is based on [Keep a Changelog](http://keepachangelog.com/).

## [0.3.0]

## WARNING: BEFORE UPDATING PLEASE BACKUP YOUR CONFIG OR YOUR DATA CAN/WILL BE LOST

 - Removed Faux Keyword support and regular terminal keyword creation by default in favor of new interactive terminal menu.
	- Menu controls are configurable in the new ``Menu Settings`` config section (main config).
	- You can also choose to display any number of constellations per page depending on the level of detail your menu text has. (From 1 - 100)
	- If you would still like to have regular terminal keyword support for routing to specific constellations, you can use each constellation's ``shortcuts`` config item to add keywords for routing to it's constellation.
	- This should resolve many of the conflicting keyword issues that limited your ability to name your constellations what you wanted.
 - With the new interactive menu, constellation names can be any length and contain any characters with exception to ``\``, ``\n``, ``\``, ``"``, ``[``, and ``]`` due to Bepinex Config limitations.
 - Added new positional data related configuration items, including:
	- ``AddConstellationPositionData`` in the main config will enable configuration items that assign coordinates to each constellation.
		- When the above is enabled, ``PostionalPricingMode`` config items will be created for each constellation in the generated config.
			- Determines how pricing for the constellation will be affected by it's coordinates.
			- ``UseOriginalPrice`` will use constellationPrice configuration item as starting route cost and the CostPerDistanceUnit value will not be used.
			- ``SetPriceByDistance`` (default) will ignore constellationPrice and set constellation's price to a new price value based on it's starting position (relative to the starter constellation).
			- ``None`` will not update pricing in any way based on positional data.
	- ``CostPerDistanceUnit``
		- When ``PostionalPricingMode`` is set to ``SetPriceByDistance``, this value will be used to determine how many credits each unit of distance is worth.
		- Example: A distance of 50 will update the route cost for the constellation to 100 when CostPerDistanceUnit is set to ``2``.
	- Thank you @unluckyjori for the PR on github and assistance in getting this feature started. Even though I ended up rewriting most of your work I do appreciate you getting the ball rolling on this so I could finally get this feature implemented in some fashion. Big shoutout to them.
	- Please also note that the distance feature is still very much in testing. If you find any issues please report them on the [github](https://github.com/darmuh/LethalConstellations) and/or the [discord thread](https://discord.com/channels/1168655651455639582/1269406512313405571).
 - In general, the mod has been lightly reworked so some old issues may have been resolved and on the flip side some old resolved issues may reappear.
	- If you experience any issues with 0.3.0 please revert back to 0.2.8 as a known good version. This mod will not receive any hotfixes until after the new year.

## [0.2.8]
 - Luigi's Mansion hotfix (fixes crashes related to trying to run this mod with this moon)
	- Will now parse extendedLevel.NumberlessPlanetName from LLL to ensure it's compatibile with Bepinex config naming requirements.
	- Tried to ensure existing config items remain unchanged by my fix for this. Please make sure to backup configs before generating a new config with this update.

## [0.2.7]
 - Added ``AcceptableStartingConstellations`` config item to set a specific set of constellations as potential starter constellations when ``StartingConstellation`` is set to ``random``.
	- Leave blank to let random choose ANY valid constellation
	- You can put duplicate entries of a constellation name to raise the odds that it will be chosen, this list does not need to have unique entries.
 - Compiled with latest version of LLL (1.3.13)

## [0.2.6]
 - Added ``StartingConstellation`` config item to set initial constellation when starting a new game (new saves or after being fired)
	- Will auto-route you to the default moon of this constellation when this is set (no charge of credits)
	- Leave blank if you do not want the level/constellation to change on a new save (will likely be experimentation unless another mod changes this)
	- Set to ``~random~`` to route to a random constellation on each new game (new game save or after getting fired)
 - Added ``ReturnToLastConstellationFromCompany`` config item (true/false)
	- When set to true, you will remain in the constellation you travelled to the company from when you leave the company.
	- When set to false, you will be put in the constellation set via ``CompanyDefaultConstellation``.
 - Added ``ManualSetupListing`` config item to streamline initial setup of the mod.
	- This config item requires valid listings in ConstellationList to work.
	- Will match different moon names to each constellation name provided.
	- This does not modify existing generated configs and will only be used when first generating a config item for a moon.
	- If a moon is being generated a configuration item and it does not match any from this list, it will be added to a random constellation.
 - Added ``IsLevelInConstellation`` boolean for other mods to check if a given level is within either the current constellation or a specific constellation
 - Added safeguards against duplicate constellation names, including a warning that displays when duplicates are detected.
	- Will clean internal constellation names listing of any duplicates and display a warning in the log.
 - Added some more safeguards in the LLL generated config portion to protect against rare cases where this mod is given bad/incomplete information

## [0.2.5]
 - Fixed fatal error during automated system creating fauxkeywords from LLL content tags which broke the terminal
 - Added logic to skip content tags that are shorter than 3 characters or contain spaces
 - Fixed an issue I noticed with clientside testing that threw an error when loading in and updating the current constellation.
 - General code cleanup and update to latest version of OpenLib

## [0.2.4]
 - Updated for latest version of OpenLib
 - Fixed issue introduced with latest Openlib update that broke some portions of constellation handling.
 - Fixed issue where constellation names could not have special characters in them.
 - Fixed issue with shortcut config item being parsed with the wrong character.
 - General info command fixes
 - Added new FauxKeywords option to set your constellation names to whatever you want.
	- These 'keywords' will only work from within the constellations menu.
 - Config items generated after lobby load will now show within the LethalConfig menu.
 - Added button to generate webconfig page which can be used to edit the config in a webpage.
	- These will be located within your Bepinex/config/webconfigs folder
 - Added button to apply config codes that you generate from your webconfig pages.
 - Updated default config handling to use LLL tags with Faux Keywords.
 - Added constellations as compatible nouns to route, so you can type route [constellation-name] and it will route you to that constellation.

## [0.2.3]
 - Fixed issue where current constellation could not be calculated when loading a save that was last at the company moon.
	- With networkapi present, this variable will be tracked in your gamesave.
	- Without networkapi present, will resort to your default company constellation as defined by [CompanyDefaultConstellation]
		- If the config item fails to find a matching constellation, will return the first constellation.
 - Updated ThunderStore name to LethalConstellations (the underscore was bothering me)

## [0.2.2]
 - Added nullable to project.
 - Fixed issue where having the default moon for a constellation set to a moon that doesn't exist would break the terminal.
	- This would happen if you removed a moon from your mod profile or if you never set a default for the constellation.
 - Fixed issue where the CurrentConstellation publicized string value would be updated even if you can't afford the constellation.
	- Thanks @xCore
 - Added more error handling for cases where the terminal is broken.
	- and some logging for common things that have failed in the past

## [0.2.1]
 - Fixed issue of non-host players being unable to join the lobby with this mod present.
 - OneTimeUnlocks for Constellations is now dependent on whether or not [LethalNetworkAPI](https://thunderstore.io/c/lethal-company/p/xilophor/LethalNetworkAPI/) is present.
	- This will be a soft compatibility and if LethalNetworkAPI is not present then constellations will fall back to normal purchase behaviour.
 - Changed moon pricing list to use the actual extendedLevel from LLL rather than a string reference of it's name to hopefully fix issues with prices not updating.
 - Publicized the class containing RouteConstellationSuccess so it should now be accessible.
 - Raised maximum moon price to 99999
 - Added orphaned config item check to remove old inactive configs

## [0.2.0]
 - **WARNING** Backup your old config file before updating, some config items have moved/changed.
 - Fixed issue where Moon prices were not being updated at each lobby load
 - Fixed issue where constellations could still route to the company even if the config item was disabled
 - Added new handling for default config generation, will now create constellations based on moon danger level tiers.
	- When [ConstellationList] is left blank, constellations will be generated by moon hazard tiers. (A+,B+, etc.)
	- Constellation price defaults will be determined by default moon which is chosen randomly
 - Added new buyOnce behavior, where a constellation can be purchased one-time and then free for the rest of the save.
	- Utilizes the game save to remember whether a constellation has been purchased already.
	- buyOnce is configurable for each constellation.
 - Added config option to keep a moon hidden even while in it's constellation
 - Moved each constellation's config to it's own section
 - Added RouteConstellationSuccess event for other mods to subscribe to
 - Added isLocked property for other mods to access a constellation and lock it if needed
 - Added optionalParams property for other mods to access to input text into constellations menu via [optionals]
 - Added handling for [currentWeather] to display the default moon's current weather in the constellations menu
 - Updated handling for setting a moons constellation, you can now set multiple constellations for one moon
	- In the moon's constellation config item separate each constellation by a comma to register it for multiple constellations.

## [0.1.7]
 - Fixed issue with LethalConfig soft compatibility.
 - Added more compatibility for LethalMoonUnlocks

## [0.1.6]
 - Publicized some things for future compatibility with LethalMoonUnlocks by explodingMods (xCore)
 - fixed typo in a method name
 - fixed typo in manifest.json referencing incorrect OpenLib version number

## [0.1.5]
 - Added new config item [HideUnaffordableConstellations] to hide constellations you cannot afford
 - Added new config item [AddHintTo] to determine which base menus your hinttext would be added to
 - Changed otherText config items to hintText for better clarity
 - Added new config item [ConstellationsShortcuts] to set shortcut keywords for the constellations menu
 - Moved static config to the base plugin config rather than defining a new one.
 - Added multi-word constellation support
 - Backend changes to optimize constellation handling.
	- New ClassMapper will track constellations and their properties.
	- Replaced 5 different dictionaries with this new class.
 - Changed the generated default moon for each constellation to be a random moon from the list of moons the constellation belongs to.
 - Added new shortcuts config for each constellation to specify shortcut keywords for each constellation.
 - Added isHidden configuration for each constellation to hide special constellations.
 - Added canRouteCompany configuration for each constellation to specify whether you can route to the company building from it or not.
 - Fixed some instances where the customized [ConstellationsWord] was not being used in place of "Constellations"
  
## [0.1.0]
 - Initial Mod Creation.