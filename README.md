🚂To-dooooo:
In Pathfinder, change the GetFCost function to use a binary tree to search.
In Pathfinder change it from getting breakable and cost from tilemap to getting it from TileMapLayer.cs's grid

Add actual levels
Add before playing where you place blocks and start, then enemies spawn.
Have it be top down.
Add coins to gui.
Change the blocks from worldspace to gui maybe? Add a T-grid to them.
Add more enemies.
Add a level script that handles what enemies spawn from what spawnpoints.
Add more towers.
Add non-generic Towers, projectiles 
Give The chest a healthbar and add a way for game to end
Add enemies shrinking when dying.
Make the enemies break the blocks slower, for fucks sake they break it soooo quickly.

Alot more
___________________________________________________________________________________________________

Main Idea:
- Tower defence game
- Physics based

*TileMap*
- SourceId:1 tiles should kill enemies immidietly, they are holes.
- SourceId:0 should store cost: breakable: and health:
- Use Grid.cs to keep track of health

*Enemies*
- Enemies go towoards chest
- Enemies take more knockback the less health
- Enemies can break blocks
- Enemies only break blocks if the pathfinding cost is less

*Tower*
- Some towers should deal damage
- Some should be slowdown or add a knockback multiplier when enemies are stunned.
