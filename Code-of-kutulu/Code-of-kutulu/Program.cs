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
    public List<GraphCase> pathfinding;
}

class Explorer : Entity
{
    public int hp;

    public bool isProximity(Explorer other)
    {
        // distance 2 distance Manathan
        return Math.Abs(x - other.x) + Math.Abs(y - other.y) <= 2;
    }

    public bool isProximity(GraphCase pos)
    {
        // distance 2 distance Manathan
        return Math.Abs(x - pos.x) + Math.Abs(y - pos.y) <= 2;
    }
}

class Wanderer : Entity
{
    public int remainingTimeMoves;
    public Explorer target;
    public int remainingTimeNextCall;
    public bool isSpawned;
}

class GraphCase : IEnumerable
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
        Console.WriteLine("MOVE " + x + " " + y + " " + msg);
    }

    public IEnumerator GetEnumerator()
    {
        if (up != null)
            yield return up;
        if (right != null)
            yield return right;
        if (down != null)
            yield return down;
        if (left != null)
            yield return left;
    }
}

public class Player
{
    static public void Main(string[] args)
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
                    graphCases.Add(game.addGraphCase(j, i, c == 'w'));
                j++;
            }
        }

        game.optimiseGraph(graphCases);
        graphCases = null;
        inputs = Console.ReadLine().Split(' ');
        game.sanityLossLonely =
            int.Parse(inputs[0]); // how much sanity you lose every turn when alone, always 3 until wood 1
        game.sanityLossGroup =
            int.Parse(inputs[1]); // how much sanity you lose every turn when near another player, always 1 until wood 1
        game.wandererSpawnTime =
            int.Parse(inputs[2]); // how many turns the wanderer take to spawn, always 3 until wood 1
        game.wandererLifeTime =
            int.Parse(inputs[3]); // how many turns the wanderer is on map after spawning, always 40 until wood 1
        int entityCount = int.Parse(Console.ReadLine()); // the first given entity corresponds to your explorer
        for (int i = 0; i < entityCount; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            game.addEntity(inputs[0], int.Parse(inputs[1]), int.Parse(inputs[2]), int.Parse(inputs[3]),
                int.Parse(inputs[4]), i == 0);
        }

        game.firstProcess();

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
                game.updateEntity(inputs[0], id, int.Parse(inputs[2]), int.Parse(inputs[3]), int.Parse(inputs[4]),
                    int.Parse(inputs[5]), int.Parse(inputs[6]), i == 0);
            }

            game.removeEntities(actualEntity, entityCount - 1);
            game.process();
        }
    }
}

class Game
{
    public int width;
    public int height;
    public GraphCase[,] graph; // [x,y]
    public int lengthGraph;
    public GraphCase[] graphCase;
    public List<Explorer> explorers;
    public List<Wanderer> wanderers;
    public int sanityLossLonely;
    public int sanityLossGroup;
    public int wandererSpawnTime;
    public int wandererLifeTime;
    public Explorer myExplorer;
    private int[] backTrack;
    List<Explorer> explorersSortDistance;
    List<Wanderer> wanderersSortDistance;

    public Game(int width, int height)
    {
        this.width = width;
        this.height = height;
        graph = new GraphCase[width, height];
        explorers = new List<Explorer>();
        wanderers = new List<Wanderer>();
    }
    
    public void print(string msg)
    {
        Console.Error.WriteLine("[DEBUG] " + msg);
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
        if (x > 0 && graph[x - 1, y] != null)
        {
            gc.left = graph[x - 1, y];
            gc.left.right = gc;
        }

        if (y > 0 && graph[x, y - 1] != null)
        {
            gc.up = graph[x, y - 1];
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

    public void addEntity(string type, int id, int x, int y, int param0, bool isMe)
    {
        if (isMe)
        {
            myExplorer = new Explorer();
            myExplorer.x = x;
            myExplorer.y = y;
            myExplorer.position = graph[x, y];
            myExplorer.hp = param0;
            myExplorer.id = id;
            return;
        }

        if (type == "EXPLORER")
        {
            Explorer e = new Explorer();
            e.id = id;
            e.x = x;
            e.y = y;
            e.position = graph[x, y];
            e.hp = param0;
            graph[x, y].explorers.Add(e);
            explorers.Add(e);
        }
        else if (type == "WANDERER")
        {
            Wanderer w = new Wanderer();
            w.id = id;
            w.x = x;
            w.y = y;
            w.position = graph[x, y];
            w.remainingTimeMoves = param0;
            w.isSpawned = false;
            graph[x, y].wanderers.Add(w);
            wanderers.Add(w);
        }
    }

    public void updateEntity(string type, int id, int x, int y, int param0, int param1, int param2, bool isMe)
    {
        if (isMe)
        {
            myExplorer.x = x;
            myExplorer.y = y;
            myExplorer.position = graph[x, y];
            myExplorer.hp = param0;
            return;
        }

        if (type == "EXPLORER")
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
        else if (type == "WANDERER")
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
                w.target = param2 == -1
                    ? null
                    : (param2 == myExplorer.id ? myExplorer : explorers.Find(x => x.id == param2));
                return;
            }
        }

        addEntity(type, id, x, y, param0, false);
    }

    public void removeEntities(List<int> actualEntityIds, int actualEntityIdsLength)
    {
        if (actualEntityIdsLength != explorers.Count + wanderers.Count)
        {
            int diff = actualEntityIds.Count - (explorers.Count + wanderers.Count);
            int i = 0;
            while (i < wanderers.Count && diff > 0)
            {
                Wanderer w = wanderers[i];
                if (!actualEntityIds.Contains(w.id))
                {
                    w.position.wanderers.Remove(w);
                    wanderers.Remove(w);
                    diff--;
                }

                i++;
            }

            i = 0;
            while (i < explorers.Count && diff > 0)
            {
                Explorer e = explorers[i];
                if (!actualEntityIds.Contains(e.id))
                {
                    e.position.explorers.Remove(e);
                    explorers.Remove(e);
                    diff--;
                }

                i++;
            }
        }
    }

    public void actionWait(string msg = "")
    {
        Console.WriteLine("WAIT " + msg);
    }

    public void actionMove(int x, int y, string msg = "")
    {
        Console.WriteLine("MOVE " + x + " " + y + " " + msg);
    }

    public void firstProcess()
    {
        backTrack = new int[lengthGraph];
        process();
    }

    private void pathFindings()
    {
        for (int i = 0; i < lengthGraph; i++)
            backTrack[i] = -1;
        int deep = 0;
        Queue<GraphCase> queueA = new Queue<GraphCase>();
        Queue<GraphCase> queueB = new Queue<GraphCase>();
        queueA.Enqueue(myExplorer.position);
        backTrack[myExplorer.position.index] = -2;
        explorersSortDistance = new List<Explorer>();
        wanderersSortDistance = new List<Wanderer>();
        while (queueA.Count != 0)
        {
            while (queueA.Count != 0)
            {
                GraphCase c = queueA.Dequeue();
                if (c.wanderers.Count != 0)
                {
                    bool isTargetingMe = false;
                    foreach (Wanderer w in c.wanderers)
                    {
                        if (true || w.target == myExplorer)
                        {
                            wanderersSortDistance.Add(w);
                            isTargetingMe = true;
                            break;
                        }
                    }

                    if (isTargetingMe)
                        continue;
                }

                if (c.up != null && backTrack[c.up.index] == -1)
                {
                    backTrack[c.up.index] = c.index;
                    queueB.Enqueue(c.up);
                }

                if (c.down != null && backTrack[c.down.index] == -1)
                {
                    backTrack[c.down.index] = c.index;
                    queueB.Enqueue(c.down);
                }

                if (c.left != null && backTrack[c.left.index] == -1)
                {
                    backTrack[c.left.index] = c.index;
                    queueB.Enqueue(c.left);
                }

                if (c.right != null && backTrack[c.right.index] == -1)
                {
                    backTrack[c.right.index] = c.index;
                    queueB.Enqueue(c.right);
                }

                if (c.explorers.Count != 0)
                    explorersSortDistance.Add(c.explorers[0]);
            }

            (queueA, queueB) = (queueB, queueA);
            deep++;
        }

        foreach (var e in wanderersSortDistance)
        {
            e.pathfinding = new List<GraphCase>();
            int index = e.position.index;
            while (index != -2)
            {
                e.pathfinding.Insert(0, graphCase[index]);
                index = backTrack[index];
            }
        }

        foreach (var e in explorersSortDistance)
        {
            e.pathfinding = new List<GraphCase>();
            int index = e.position.index;
            while (index != -2)
            {
                e.pathfinding.Insert(0, graphCase[index]);
                index = backTrack[index];
            }
        }
    }

    private GraphCase choiseAI()
    {
        print("myPos: "+myExplorer.x+" "+myExplorer.y);
        Explorer proximityExplorer = null;
        foreach (var e in explorers)
        {
            if (e.isProximity(myExplorer))
            {
                print("OtherExplorer x " + e.x + " y " + e.y);
                proximityExplorer = e;
                break;
            }
        }

        GraphCase wandererNearestDirection = null;
        Wanderer wandererNearest = null;
        foreach (var w in wanderersSortDistance)
        {
            if (w.pathfinding.Count > 1)
                wandererNearestDirection = w.pathfinding[1];
            else
                wandererNearestDirection = w.position;
            wandererNearest = w;
            print("Wanderer x:" + w.x + " y:" + w.y+" direction x:"+wandererNearestDirection.x+" y:"+wandererNearestDirection.y);
            break;
        }

        if (wandererNearestDirection != null)
        {
            if (proximityExplorer != null && wandererNearest.pathfinding.Count > 2)
            {
                foreach (Explorer e in explorersSortDistance)
                {
                    if (proximityExplorer.isProximity(e) && e.pathfinding.Count > 1 && e.pathfinding[1] != wandererNearestDirection)
                    {
                        print("AI choise: A");
                        return e.pathfinding[1];
                    }
                }
            }

            foreach (var e in explorersSortDistance)
            {
                if (e.pathfinding.Count > 1 && e.pathfinding[1] != wandererNearestDirection)
                {
                    print("AI choise: C");
                    return e.pathfinding[1];
                }
            }

            foreach (GraphCase np in myExplorer.position)
            {
                if (wandererNearestDirection != np)
                {
                    print("AI choise: D");
                    return np;
                }
            }
            
            print("AI choise: E");
            return null;
        }
        if (proximityExplorer == null)
            foreach (var e in explorersSortDistance)
            {
                print("AI choise: G");
                return e.pathfinding[1];
            }

        print("AI choise: H");
        return null;
    }

    public void process()
    {
        pathFindings();
        GraphCase newPos = choiseAI();
        if(newPos == null || newPos==myExplorer.position)
            actionWait();
        else
            newPos.actionMove();
    }
}