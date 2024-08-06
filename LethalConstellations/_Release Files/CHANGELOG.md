# Lethal Constellations Change Log

All notable changes to this project will be documented in this file.
 
The format is based on [Keep a Changelog](http://keepachangelog.com/).

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