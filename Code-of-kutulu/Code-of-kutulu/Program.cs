using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;


class Entity
{
    public int id;
    public int x;
    public int y;
    public GraphCase position;
}

class Explorer : Entity
{
    public int hp;
}

class Wanderer : Entity
{
    public int remainingTimeMoves;
    public Explorer target;
    public int remainingTimeNextCall;
    public bool isSpawned;
}

class GraphCase
{
    public GraphCase up;
    public GraphCase right;
    public GraphCase down;
    public GraphCase left;
    public int x;
    public int y;
    public int index;
    public bool isPortalInvocator;
    public List<Wanderer> wanderers;
    public List<Explorer> explorers;
    
    public void actionMove(string msg = "")
    {
        Console.WriteLine("MOVE "+x+" "+y+" "+msg);
    }
}


class Player
{
    static void Main(string[] args)
    {
        string[] inputs;
        Game game = new Game(int.Parse(Console.ReadLine()), int.Parse(Console.ReadLine()));
        List<GraphCase> graphCases = new List<GraphCase>();
        for (int i = 0; i < game.height; i++)
        {
            int j = 0;
            foreach (char c in Console.ReadLine())
            {
                if (c == '.' || c == 'w')
                    graphCases.Add(game.addGraphCase(j,i,c=='w'));
                j++;
            }
        }
        game.optimiseGraph(graphCases);
        graphCases=null;
        inputs = Console.ReadLine().Split(' ');
        game.sanityLossLonely = int.Parse(inputs[0]); // how much sanity you lose every turn when alone, always 3 until wood 1
        game.sanityLossGroup = int.Parse(inputs[1]); // how much sanity you lose every turn when near another player, always 1 until wood 1
        game.wandererSpawnTime = int.Parse(inputs[2]); // how many turns the wanderer take to spawn, always 3 until wood 1
        game.wandererLifeTime = int.Parse(inputs[3]); // how many turns the wanderer is on map after spawning, always 40 until wood 1
        int entityCount = int.Parse(Console.ReadLine()); // the first given entity corresponds to your explorer
        for (int i = 0; i < entityCount; i++)
        {
            inputs = Console.ReadLine().Split(' ',5);
            game.addEntity(inputs[0], int.Parse(inputs[1]), int.Parse(inputs[2]), int.Parse(inputs[3]), int.Parse(inputs[4]));
        }
        game.process();
        
        // game loop
        while (true)
        {
            entityCount = int.Parse(Console.ReadLine()); // the first given entity corresponds to your explorer
            List<int> actualEntity = new List<int>();
            for (int i = 0; i < entityCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int id = int.Parse(inputs[1]);
                actualEntity.Add(id);
                game.updateEntity(inputs[0], id, int.Parse(inputs[2]), int.Parse(inputs[3]), int.Parse(inputs[4]), int.Parse(inputs[5]), int.Parse(inputs[6]));
            }
            game.removeEntities(actualEntity, entityCount);
            game.process();
        }
    }
}
class Game
{
     public int width;
     public int height;
     public GraphCase[,] graph;// [x,y]
     public int lengthGraph;
     public GraphCase[] graphCase;
     public List<Explorer> explorers;
     public List<Wanderer> wanderers;
     public int sanityLossLonely;
     public int sanityLossGroup;
     public int wandererSpawnTime;
     public int wandererLifeTime;
     
     public Game(int width, int height)
     {
         this.width = width;
         this.height = height;
         this.graph = new GraphCase[width, height];
         this.explorers = new List<Explorer>();
         this.wanderers = new List<Wanderer>();
     }

     public GraphCase addGraphCase(int x, int y, bool isPortalInvocator)
     {
         GraphCase gc = new GraphCase();
         gc.x = x;
         gc.y = y;
         gc.isPortalInvocator = isPortalInvocator;
         gc.explorers = new List<Explorer>();
         gc.wanderers = new List<Wanderer>();
         graph[x, y] = gc;
         if(x>0 && graph[x-1, y] != null)
         {
             gc.left = graph[x-1, y];
             gc.left.right = gc;
         }
         if(y>0 && graph[x, y-1] != null)
         {
             gc.up = graph[x, y-1];
             gc.up.down = gc;
         }
         return gc;
     }

     public void optimiseGraph(List<GraphCase> graphCases)
     {
         lengthGraph = graphCases.Count;
         graphCase = new GraphCase[lengthGraph];
         for (int i = 0; i < lengthGraph; i++)
         {
             GraphCase gc = graphCases[i];
             graphCase[i] = gc;
             gc.index = i;
         }
     }
     
     public void addEntity(string type, int id, int x, int y, int param0)
     {
         if(type == "EXPLORER")
         {
             Explorer e = new Explorer();
             e.id = id;
             e.x = x;
             e.y = y;
             e.hp = param0;
             graph[x, y].explorers.Add(e);
             explorers.Add(e);
         }
         else if(type == "WANDERER")
         {
             Wanderer w = new Wanderer();
             w.id = id;
             w.x = x;
             w.y = y;
             w.remainingTimeMoves = param0;
             w.isSpawned = false;
             graph[x, y].wanderers.Add(w);
             wanderers.Add(w);
         }
     }
     
     public void updateEntity(string type, int id, int x, int y, int param0, int param1, int param2)
     {
         if(type == "EXPLORER")
         {
             Explorer e = explorers.Find(x => x.id == id);
             if (e != null)
             {
                 if (e.x != x || e.y != y)
                 {
                     e.x = x; 
                     e.y = y;
                     e.position.explorers.Remove(e);
                     e.position = graph[x, y];
                     e.position.explorers.Add(e);
                 }
                 e.hp = param0;
                 return;
             }
         }
         else if(type == "WANDERER")
         {
             Wanderer w = wanderers.Find(x => x.id == id);
             if (w != null)
             {
                 if (w.x != x || w.y != y)
                 {
                     w.x = x;
                     w.y = y;
                     w.position.wanderers.Remove(w);
                     w.position = graph[x, y];
                     w.position.wanderers.Add(w);
                 }
                 w.remainingTimeMoves = param0;
                 w.remainingTimeNextCall = param2;
                 w.isSpawned = param1 == 1;
                 w.target = param2 == -1 ? null : explorers.Find(x => x.id == param2); 
                 return;
             }
         }
         addEntity(type, id, x, y, param0);
     }
     
     public void removeEntities(List<int> actualEntityIds, int actualEntityIdsLength)
     {
         if (actualEntityIdsLength != explorers.Count + wanderers.Count)
         {
             int diff = actualEntityIds.Count - (explorers.Count + wanderers.Count);
             foreach (Wanderer w in wanderers)
                 if (!actualEntityIds.Contains(w.id))
                 {
                     w.position.wanderers.Remove(w);
                     wanderers.Remove(w);
                     diff--;
                     if (diff == 0)
                         return;
                 }
             foreach (Explorer e in explorers)
                 if (!actualEntityIds.Contains(e.id))
                 {
                     e.position.explorers.Remove(e);
                     explorers.Remove(e);
                     diff--;
                     if (diff == 0)
                         return;
                 } 
         }
     }

     public void actionWait(string msg = "")
     {
         Console.WriteLine("WAIT "+msg);
     }
     
     public void actionMove(int x, int y, string msg = "")
     {
         Console.WriteLine("MOVE "+x+" "+y+" "+msg);
     }

     public string process()
     {
         return null;
     }
     
}