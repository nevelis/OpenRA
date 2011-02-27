using System;
using System.Collections.Generic;
using System.Drawing;
using OpenRA.FileFormats;
using OpenRA.Mods.RA.Air;
using OpenRA.Traits;
using OpenRA.Mods.RA.Move;

namespace OpenRA.Mods.RA
{
    class ArtOfDefenceScriptInfo : TraitInfo<ArtOfDefenceScript> 
    { 
        
    }

    class ArtOfDefenceScript : IWorldLoaded, ITick
    {
        WaveHandler waveHandler;

        private bool startup = true;
        Dictionary<string, Actor> actors;

        public void WorldLoaded(World w)
        {
            actors = w.WorldActor.Trait<SpawnMapActors>().Actors;
        }
        public void Tick(Actor self)
        {
            if (startup)
            {
                waveHandler = new WaveHandler(self);

                Wave wave1 = new Wave();
                wave1.Add("JEEP", 5);
                wave1.Add("E1", 20);
                wave1.Add("E3", 20);

                Wave wave2 = new Wave();
                wave2.Add("V2RL", 2);
                wave2.Add("E4", 30);

                Wave wave3 = new Wave();
                wave3.Add("2TNK", 15);
                wave3.Add("1TNK", 6);
                wave3.Add("ARTY", 7);
                wave3.Add("V2RL", 2);

                Wave wave4 = new Wave();
                wave4.Add("3TNK", 18);
                wave4.Add("V2RL", 10);

                Wave wave5 = new Wave();
                wave5.Add("4TNK", 10);
                wave5.Add("V2RL", 15);
                wave5.Add("ARTY", 15);
                //wave5.Add("YAK", 15); crash?

                Wave wave6 = new Wave();
                wave6.Add("3TNK", 15);
                wave6.Add("V2RL", 10);
                wave6.Add("FTRK", 5);
                wave6.Add("4TNK", 15);
                wave6.Add("1TNK", 30);
                wave6.Add("E3", 40);
                //wave6.Add("YAK", 30); crash?

                waveHandler.addWave(wave1);
                waveHandler.addWave(wave2);
                waveHandler.addWave(wave3);
                waveHandler.addWave(wave4);
                waveHandler.addWave(wave5);
                waveHandler.addWave(wave6);


                //Don't change
                int tempCounter = 1;
                while (actors.ContainsKey("spawnPoint" + tempCounter.ToString()))
                {
                    this.waveHandler.addSpawnPoint(actors["spawnPoint" + tempCounter.ToString()].Location);
                    tempCounter++;
                }
                if (actors.ContainsKey("attackPoint"))
                {
                    this.waveHandler.attackPosition = actors["attackPoint"].Location;
                }
                if (actors.ContainsKey("buildingPoint"))
                {
                    this.waveHandler.buildingPosition = actors["buildingPoint"].Location;
                }
                Game.AddChatLine(Color.Red, "Info: ", "Your mission is to protect the GAP Generator.");
                Game.AddChatLine(Color.Red, "Info: ", "This generator is a new technology the enemy forces want to se burned to the ground.");
                Game.AddChatLine(Color.Red, "Info: ", "Defend with your lives else you will be defeated.");
                waveHandler.createBuilding();
                startup = false;
                //Don't change
            }
            waveHandler.mainLoop();
        }
    }

    class WaveHandler
    {
        private Actor self;

        public int currentWave { get; set; }      //Keeps track of what's the current wave
        private const int TICKS_TO_SEC = 25;
        private int frameTimer;
        private int secondsTimer;
        private bool gameOver;
        private bool waveIncoming;
        private int waveRest;

        private List<Wave> waves;                 //A list of all waves
        private List<Actor> units;                //A list of all units in a wave
        private List<WaveUnit> unitsLeftToSpawn;  //A temporary list of units left to spawn

        private List<int2> spawnPositions;        //List of all spawnPositions
        public int2 attackPosition;
        public int2 buildingPosition;
        private Actor building;

        public WaveHandler(Actor self)
        {
            this.self = self;

            this.spawnPositions = new List<int2>();
            this.waves = new List<Wave>();      //A list of all waves
            this.units = new List<Actor>();     //A list of all units in a wave
            this.unitsLeftToSpawn = null;       //A temporary list of units left to spawn

            this.waveRest = -1;
            this.currentWave = 0;
            this.frameTimer = 0;    //Increase each frame
            this.secondsTimer = 0;  //Increase each second
            this.gameOver = false;      //If the game ended this should be true.
            this.waveIncoming = false;  //Becomes true when the spawning starts and false when the spawning is done.
            //This is to prevent a bug if you kill all the enemies before the next sub-wave come.
        }

        public void createBuilding()
        {
            for (int i = 0; i < self.World.players.Count; i++)
            {
                if (!self.World.players[i].IsBot && self.World.players[i].PlayerRef.Playable)
                {
                    this.building = self.World.CreateActor("GAP", new TypeDictionary 
                    {
                        new OwnerInit(self.World.players[i].InternalName), 
                        new LocationInit(this.buildingPosition)
                    });
                    break;
                }
            }
        }
        public void addSpawnPoint(int2 position)
        {
            this.spawnPositions.Add(position);
        }

        public void mainLoop()
        {
            if (this.frameTimer % TICKS_TO_SEC == 1)
            {
                this.eachSecond();
            }
            this.frameTimer++;
        }

        private void eachSecond() //This function is called each second
        { 
            if(!this.gameOver)
            {
                if(!this.waveIncoming) //If there is no wave spawning.
                {
                    if(!waveStillAlive())
                    {
                        if(this.currentWave == this.waves.Count)
                        {
                            //All waves completed. (Victory)
                            this.gameOver = true;
                            #region
                            for (int i = 0; i < self.World.players.Count; i++)
                            {
                                if (!self.World.players[i].IsBot && self.World.players[i].PlayerRef.Playable)
                                {
                                    self.World.players[i].WinState = WinState.Won;
                                }
                            }
                            #endregion
                        }
                        else
                        {
                            if(waveRest == -1)
                            {
                                //Start wave timer for next wave.
                                this.waveRest = waves[this.currentWave].getRestTime();
                                Game.AddChatLine(Color.Red, "Info: ", "Wave: {0}. Incomming in {1} seconds.".F((this.currentWave + 1).ToString(), this.waveRest.ToString()));
                            }
                            else
                            {
                                //Countdown the timer
                                if(waveRest == 0)
                                {
                                    //When timer hit 0 - Start Spawning
                                    Game.AddChatLine(Color.Red, "Info: ", "Wave: {0}".F((this.currentWave + 1).ToString()));
                                    this.waveIncoming = this.spawnUnits();
                                    this.secondsTimer = 0;
                                }
                                this.waveRest--;
                            }
                        }
                    }
                }
                else
                {
                    //Wave is spawning finish preduce it.
                    if(this.secondsTimer%5 == 1)
                    {
                        //Each five seconds preduce new sub-wave until all the subwaves are done.
                        this.waveIncoming = this.spawnUnits();
                        if(!this.waveIncoming)
                        {
                            //When all the sub-waves are done.
                            this.currentWave++;
                        }
                    }
                    this.secondsTimer++;
                }

                if(!this.gameOver) //To avoid both winning and losing at the same time.
                {
                    if (building.Destroyed)
                    {
                        //The GAP was destoryed - (Defeat)
                        this.gameOver = true;
                        #region
                        for (int i = 0; i < self.World.players.Count; i++)
                        {

                            if (!self.World.players[i].IsBot && self.World.players[i].PlayerRef.Playable)
                            {
                                self.World.players[i].WinState = WinState.Lost;
                            }

                            //foreach (var a in self.World.Queries.OwnedBy[self.World.players[i]])
                            //{
                            //    a.Kill(a);
                            //}

                            //Sound.PlayToPlayer(self.World.players[i], "");
                        }
                        #endregion
                    }
                }
            }
        }
        public void addWave(Wave wave)
        {
            waves.Add(wave);
        }

        public bool waveStillAlive()
        {
            //remove dead objects
			units.RemoveAll(a => a.Destroyed);
			return units.Count > 0;
        }

        public bool spawnUnits()
        {
            int counter = 0;
            int numberOfUnits = 0;
            if (unitsLeftToSpawn == null)
            {
                unitsLeftToSpawn = this.waves[this.currentWave].sendUnits();
            }
            int size = unitsLeftToSpawn.Count;

            for (int i = 0; i < size; i++)
            {
                numberOfUnits = unitsLeftToSpawn[i].amount;
                for (int amount = 0; amount < numberOfUnits; amount++)
                {
                    var actor = self.World.CreateActor(unitsLeftToSpawn[i].name, new TypeDictionary 
                    {
                        new OwnerInit("Hostile"), 
                        new LocationInit(this.spawnPositions[counter])
                    });

                    actor.Trait<AttackMove>().ResolveOrder(actor, new Order("AttackMove", actor, false) { TargetLocation = this.attackPosition });
                    this.units.Add(actor);

                    unitsLeftToSpawn[i].amount--;
                    if (unitsLeftToSpawn[i].amount == 0)
                    {
                        unitsLeftToSpawn.RemoveAt(i);
                        i--;
                        size--;
                    }

                    counter++;
                    if (counter == this.spawnPositions.Count - 1)
                    {
                        return true; //more to come
                    }
                }
            }
            unitsLeftToSpawn = null;
            return false; //done
        }
    }
    class Wave
    {
        private List<WaveUnit> units;
        private int restTime;

		public Wave() : this(30) { }
        public Wave(int restTime)
        {
            this.restTime = restTime;
            this.units = new List<WaveUnit>();
        }
        public void Add(string unitName, int unitAmount)
        {
            this.units.Add(new WaveUnit(unitName, unitAmount));
        }
        public int getRestTime()
        {
            return this.restTime;
        }
        public List<WaveUnit> sendUnits()
        {
            return this.units;
        }
    }
    class WaveUnit
    {
        public string name { get; set; }
        public int amount { get; set; }

        public WaveUnit(string name, int amount)
        {
            this.name = name;
            this.amount = amount;
        }
    }
}