
# Bot In Random Maze
A random maze generation, with keys for the bot collect and unlock exit.

This project was made to practice Maze Generation Algorithm, to understand how it works and his implementation.
After that, to use the maze for something, I decide to create a simple bot that would try to escape from maze.

### Note
To this project I used the RayCast class from Unity, to detect and find the best direction to bot moves, then eventualy unlock and go to maze exit. However isn't best form, but is simple and can be useful, case you want that bot goes to diferent paths before exit from maze.

To find a better and faster path to exit is common use the [Dijkstra's Algorithm](https://www.freecodecamp.org/news/dijkstras-shortest-path-algorithm-visual-introduction/#:~:text=Dijkstra's%20Algorithm%20finds%20the%20shortest,node%20and%20all%20other%20nodes.), a powerfull Pathfinding algorithm.

>"Where you can find the shortest path between nodes in a graph. Particularly, you can find the shortest path from a node (called the "source node") to all other nodes in the graph, producing a shortest-path tree".
    
You can implement it, modify and at the end get something similary or even better that using raycast.

<div>
  <img src="https://github.com/HenriqueRCampos/UnityHTracking/assets/107483658/2a16e3cf-e6f6-4ede-a46c-9932ee9be00b" width=px align="center"/>
</div>

## How it Works
Every movement of bot is tracked, like paths that he already completed(where there aren't keys anymore), current walked path and the maze exit when he find it.
  - Completed 🟩
  - Maze exit path 🟦
  - Maze exit point ⬛
  - Bot 🟥
  - Keys 🟡
> "Current walked path" its just a reference for bot, it isn't dreawed.

To up, down, left and right of the bot, it has a line RayCast with a maximum distance set to one, it is used to get a GameObject collider, like:
  - Wall;
  - Key;
  - Maze exit;
  - Or return null, if don't colliede with nothing.


Using this data, the bot recive a direction to move, taking into account some rules, he:
  - Can't back to walked path until it resets(it reset when, all the possible directions are blocked);
  - Can't enter in completed path;
  - Can't exit from maze until collect all keys;
  - Always collect keys detected by RayCast;


 <!-- <img src="https://github.com/HenriqueRCampos/Bot_At_Random_Maze/assets/107483658/5233e8af-fe25-4c31-83c7-f2993a5a7041" width=250px/> -->
<!--   <img src="https://github.com/HenriqueRCampos/HockeyGame/assets/107483658/7993aad2-1c4c-4c44-9116-ed31e4075003" width=50px align="left"/>
  <img src="https://github.com/HenriqueRCampos/Bot_At_Random_Maze/assets/107483658/5233e8af-fe25-4c31-83c7-f2993a5a7041" width=500px/>
  <img src="https://github.com/HenriqueRCampos/HockeyGame/assets/107483658/adb0d153-3e2a-4c3d-b872-37a8fa9619c4"  width=50px align="right"/> -->

## Watch a demo
https://github.com/HenriqueRCampos/Bot_At_Random_Maze/assets/107483658/20964f1c-240d-43ad-8c75-7548e80e7dc7

## References
Tutorial: [How to Make a Maze Generation Algorithm in Unity](https://youtu.be/OutlTTOm17M)
