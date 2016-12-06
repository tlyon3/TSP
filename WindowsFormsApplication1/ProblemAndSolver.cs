using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using WindowsFormsApplication1;

namespace TSP
{

    class ProblemAndSolver
    {

        private class TSPSolution
        {
            /// <summary>
            /// we use the representation [cityB,cityA,cityC] 
            /// to mean that cityB is the first city in the solution, cityA is the second, cityC is the third 
            /// and the edge from cityC to cityB is the final edge in the path.  
            /// You are, of course, free to use a different representation if it would be more convenient or efficient 
            /// for your data structure(s) and search algorithm. 
            /// </summary>
            public ArrayList
                Route;

            /// <summary>
            /// constructor
            /// </summary>
            /// <param name="iroute">a (hopefully) valid tour</param>
            public TSPSolution(ArrayList iroute)
            {
                Route = new ArrayList(iroute);
            }

            /// <summary>
            /// Compute the cost of the current route.  
            /// Note: This does not check that the route is complete.
            /// It assumes that the route passes from the last city back to the first city. 
            /// </summary>
            /// <returns></returns>
            public double costOfRoute()
            {
                // go through each edge in the route and add up the cost. 
                int x;
                City here;
                double cost = 0D;

                for (x = 0; x < Route.Count - 1; x++)
                {
                    here = Route[x] as City;
                    cost += here.costToGetTo(Route[x + 1] as City);
                }

                // go from the last city to the first. 
                here = Route[Route.Count - 1] as City;
                cost += here.costToGetTo(Route[0] as City);
                return cost;
            }
        }

        #region Private members 

        /// <summary>
        /// Default number of cities (unused -- to set defaults, change the values in the GUI form)
        /// </summary>
        // (This is no longer used -- to set default values, edit the form directly.  Open Form1.cs,
        // click on the Problem Size text box, go to the Properties window (lower right corner), 
        // and change the "Text" value.)
        private const int DEFAULT_SIZE = 25;

        /// <summary>
        /// Default time limit (unused -- to set defaults, change the values in the GUI form)
        /// </summary>
        // (This is no longer used -- to set default values, edit the form directly.  Open Form1.cs,
        // click on the Time text box, go to the Properties window (lower right corner), 
        // and change the "Text" value.)
        private const int TIME_LIMIT = 60;        //in seconds

        private const int CITY_ICON_SIZE = 5;


        // For normal and hard modes:
        // hard mode only
        private const double FRACTION_OF_PATHS_TO_REMOVE = 0.20;

        /// <summary>
        /// the cities in the current problem.
        /// </summary>
        private City[] Cities;
        /// <summary>
        /// a route through the current problem, useful as a temporary variable. 
        /// </summary>
        private ArrayList Route;
        /// <summary>
        /// best solution so far. 
        /// </summary>
        private TSPSolution bssf; 

        /// <summary>
        /// how to color various things. 
        /// </summary>
        private Brush cityBrushStartStyle;
        private Brush cityBrushStyle;
        private Pen routePenStyle;


        /// <summary>
        /// keep track of the seed value so that the same sequence of problems can be 
        /// regenerated next time the generator is run. 
        /// </summary>
        private int _seed;
        /// <summary>
        /// number of cities to include in a problem. 
        /// </summary>
        private int _size;

        /// <summary>
        /// Difficulty level
        /// </summary>
        private HardMode.Modes _mode;

        /// <summary>
        /// random number generator. 
        /// </summary>
        private Random rnd;

        /// <summary>
        /// time limit in milliseconds for state space search
        /// can be used by any solver method to truncate the search and return the BSSF
        /// </summary>
        private int time_limit;
		#endregion

		#region Public members

		public bool initialBssf = false;
		public double currentBssf;
        /// <summary>
        /// These three constants are used for convenience/clarity in populating and accessing the results array that is passed back to the calling Form
        /// </summary>
        public const int COST = 0;           
        public const int TIME = 1;
        public const int COUNT = 2;
        
        public int Size
        {
            get { return _size; }
        }

        public int Seed
        {
            get { return _seed; }
        }
        #endregion

        #region Constructors
        public ProblemAndSolver()
        {
            this._seed = 1; 
            rnd = new Random(1);
            this._size = DEFAULT_SIZE;
            this.time_limit = TIME_LIMIT * 1000;                  // TIME_LIMIT is in seconds, but timer wants it in milliseconds

            this.resetData();
        }

        public ProblemAndSolver(int seed)
        {
            this._seed = seed;
            rnd = new Random(seed);
            this._size = DEFAULT_SIZE;
            this.time_limit = TIME_LIMIT * 1000;                  // TIME_LIMIT is in seconds, but timer wants it in milliseconds

            this.resetData();
        }

        public ProblemAndSolver(int seed, int size)
        {
            this._seed = seed;
            this._size = size;
            rnd = new Random(seed);
            this.time_limit = TIME_LIMIT * 1000;                        // TIME_LIMIT is in seconds, but timer wants it in milliseconds

            this.resetData();
        }
        public ProblemAndSolver(int seed, int size, int time)
        {
            this._seed = seed;
            this._size = size;
            rnd = new Random(seed);
            this.time_limit = time*1000;                        // time is entered in the GUI in seconds, but timer wants it in milliseconds

            this.resetData();
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Reset the problem instance.
        /// </summary>
        private void resetData()
        {

            Cities = new City[_size];
            Route = new ArrayList(_size);
            bssf = null;

            if (_mode == HardMode.Modes.Easy)
            {
                for (int i = 0; i < _size; i++)
                    Cities[i] = new City(rnd.NextDouble(), rnd.NextDouble());
            }
            else // Medium and hard
            {
                for (int i = 0; i < _size; i++)
                    Cities[i] = new City(rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble() * City.MAX_ELEVATION);
            }

            HardMode mm = new HardMode(this._mode, this.rnd, Cities);
            if (_mode == HardMode.Modes.Hard)
            {
                int edgesToRemove = (int)(_size * FRACTION_OF_PATHS_TO_REMOVE);
                mm.removePaths(edgesToRemove);
            }
            City.setModeManager(mm);

            cityBrushStyle = new SolidBrush(Color.Black);
            cityBrushStartStyle = new SolidBrush(Color.Red);
            routePenStyle = new Pen(Color.Blue,1);
            routePenStyle.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// make a new problem with the given size.
        /// </summary>
        /// <param name="size">number of cities</param>
        public void GenerateProblem(int size, HardMode.Modes mode)
        {
            this._size = size;
            this._mode = mode;
            resetData();
        }

        /// <summary>
        /// make a new problem with the given size, now including timelimit paremeter that was added to form.
        /// </summary>
        /// <param name="size">number of cities</param>
        public void GenerateProblem(int size, HardMode.Modes mode, int timelimit)
        {
            this._size = size;
            this._mode = mode;
            this.time_limit = timelimit*1000;                                   //convert seconds to milliseconds
            resetData();
        }

        /// <summary>
        /// return a copy of the cities in this problem. 
        /// </summary>
        /// <returns>array of cities</returns>
        public City[] GetCities()
        {
            City[] retCities = new City[Cities.Length];
            Array.Copy(Cities, retCities, Cities.Length);
            return retCities;
        }

        /// <summary>
        /// draw the cities in the problem.  if the bssf member is defined, then
        /// draw that too. 
        /// </summary>
        /// <param name="g">where to draw the stuff</param>
        public void Draw(Graphics g)
        {
            float width  = g.VisibleClipBounds.Width-45F;
            float height = g.VisibleClipBounds.Height-45F;
            Font labelFont = new Font("Arial", 10);

            // Draw lines
            if (bssf != null)
            {
                // make a list of points. 
                Point[] ps = new Point[bssf.Route.Count];
                int index = 0;
                foreach (City c in bssf.Route)
                {
                    if (index < bssf.Route.Count -1)
                        g.DrawString(" " + index +"("+c.costToGetTo(bssf.Route[index+1]as City)+")", labelFont, cityBrushStartStyle, new PointF((float)c.X * width + 3F, (float)c.Y * height));
                    else 
                        g.DrawString(" " + index +"("+c.costToGetTo(bssf.Route[0]as City)+")", labelFont, cityBrushStartStyle, new PointF((float)c.X * width + 3F, (float)c.Y * height));
                    ps[index++] = new Point((int)(c.X * width) + CITY_ICON_SIZE / 2, (int)(c.Y * height) + CITY_ICON_SIZE / 2);
                }

                if (ps.Length > 0)
                {
                    g.DrawLines(routePenStyle, ps);
                    g.FillEllipse(cityBrushStartStyle, (float)Cities[0].X * width - 1, (float)Cities[0].Y * height - 1, CITY_ICON_SIZE + 2, CITY_ICON_SIZE + 2);
                }

                // draw the last line. 
                g.DrawLine(routePenStyle, ps[0], ps[ps.Length - 1]);
            }

            // Draw city dots
            foreach (City c in Cities)
            {
                g.FillEllipse(cityBrushStyle, (float)c.X * width, (float)c.Y * height, CITY_ICON_SIZE, CITY_ICON_SIZE);
            }

        }

        /// <summary>
        ///  return the cost of the best solution so far. 
        /// </summary>
        /// <returns></returns>
        public double costOfBssf ()
        {
            if (bssf != null)
                return (bssf.costOfRoute());
            else
                return -1D; 
        }

        /// <summary>
        /// This is the entry point for the default solver
        /// which just finds a valid random tour 
        /// </summary>
        /// <returns>results array for GUI that contains three ints: cost of solution, time spent to find solution, number of solutions found during search (not counting initial BSSF estimate)</returns>
        public string[] defaultSolveProblem()
        {
            int i, swap, temp, count=0;
            string[] results = new string[3];
            int[] perm = new int[Cities.Length];
            Route = new ArrayList();
            Random rnd = new Random();
            Stopwatch timer = new Stopwatch();

            timer.Start();

            do
            {
                for (i = 0; i < perm.Length; i++)                                 // create a random permutation template
                    perm[i] = i;
                for (i = 0; i < perm.Length; i++)
                {
                    swap = i;
                    while (swap == i)
                        swap = rnd.Next(0, Cities.Length);
                    temp = perm[i];
                    perm[i] = perm[swap];
                    perm[swap] = temp;
                }
                Route.Clear();
                for (i = 0; i < Cities.Length; i++)                            // Now build the route using the random permutation 
                {
                    Route.Add(Cities[perm[i]]);
                }
                bssf = new TSPSolution(Route);
                count++;
            } while (costOfBssf() == double.PositiveInfinity);                // until a valid route is found
            timer.Stop();

            results[COST] = costOfBssf().ToString();                          // load results array
            results[TIME] = timer.Elapsed.ToString();
            results[COUNT] = count.ToString();

            return results;
        }

        /// <summary>
        /// performs a Branch and Bound search of the state space of partial tours
        /// stops when time limit expires and uses BSSF as solution
        /// </summary>
        /// <returns>results array for GUI that contains three ints: cost of solution, time spent to find solution, number of solutions found during search (not counting initial BSSF estimate)</returns>
        public string[] bBSolveProblem()
        {
            string[] results = new string[3];
			Stopwatch timer = new Stopwatch();
			//set initial bssff
			initialBssf = true;
			greedySolveProblem();
			int validRoutes = 0;
			int prunes = 0;
			int states = 0;
			int updates = 0;
			int maxNumberOfStatesStored = 0;
			ArrayList routeResult = new ArrayList();
			State currentNode = new State(new RMatrix(Cities), Cities[0], 0, Cities.Length);
			PQueue queue = new PQueue(Cities.Length);
			states++;
			if(currentNode.bssfCost < bssf.costOfRoute()){
				queue.add(currentNode);
			}
			timer.Start();
			Console.WriteLine("Started bb timer");
			while(!queue.empty()){
				long time = timer.ElapsedMilliseconds;
				if (time > time_limit){
					Console.WriteLine("Timed out");
					break;
				}
				//get next node
				currentNode = queue.deleteMin();
				if(currentNode.bssfCost >= bssf.costOfRoute()){
					prunes++;
					queue.pruneCurrentPath();
					continue;
				}
				//check if we have completed route
				if(currentNode.route.Count == Cities.Length){
					double cost = currentNode.city.costToGetTo(Cities[0]);
					//check if we can get back to the starting city
					if(!double.IsPositiveInfinity(cost)){
						validRoutes++;
						//check if route is better that bssf
						if(currentNode.currentCost + cost < bssf.costOfRoute()){
							currentNode.currentCost += cost;
							updates++;
							bssf = new TSPSolution(currentNode.route);
						}
					}
				}
				//expand node
				for (int i = 0; i < Cities.Length;i++){
					if(currentNode.visited.Contains(i)){
						continue;
					}
					double costToCity = currentNode.city.costToGetTo(Cities[i]);
					// if we can reach the city
					if(!double.IsPositiveInfinity(costToCity)){
						//create a new state for the next possible city
						State node = new State(currentNode, costToCity, i, Cities[i], Cities.Length);
						states++;
						//if the lower bound for the new state is lower than the bssf, add to queue
						if(node.bssfCost < bssf.costOfRoute()){
							queue.add(node);
							//update maxNumberOfStatesStored counter
							if(queue.count > maxNumberOfStatesStored){
								maxNumberOfStatesStored = queue.count;
							}
						}
						else {
							prunes++;
						}
					}
				}
			}
			timer.Stop();
			Console.WriteLine("Stopped bb timer");
			Console.WriteLine("-------------------");
			Console.WriteLine("Results:");
			Console.WriteLine("Number of solutions: " + validRoutes);
			Console.WriteLine("Max number of states: " + maxNumberOfStatesStored);
			Console.WriteLine("Number of updates:" + updates);
			Console.WriteLine("Number of states:" + states);
			Console.WriteLine("Number of prunes: " + prunes);
			Console.WriteLine("-------------------");

			results[COST] = bssf.costOfRoute().ToString();
			double elapsedTime = timer.Elapsed.TotalSeconds;;
			results[TIME] = elapsedTime.ToString();
			results[COUNT] = validRoutes.ToString();

            return results;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////
        // These additional solver methods will be implemented as part of the group project.
        ////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// finds the greedy tour starting from each city and keeps the best (valid) one
        /// </summary>
        /// <returns>results array for GUI that contains three ints: cost of solution, time spent to find solution, number of solutions found during search (not counting initial BSSF estimate)</returns>
        public string[] greedySolveProblem()
        {
             			Stopwatch timer = new Stopwatch(); 			string[] results = new string[3]; 			double bestSoFar = double.MaxValue; 			Boolean[] visited = new Boolean[Cities.Length]; 			int currentCity; 			int numberOfSolutions = 0;  			//starting cityy 			timer.Start(); 			for (int i = 0; i < Cities.Length;i++){ 				 				double totalCost = 0; 				//if we have passed the time limit 				if(timer.Elapsed.Seconds > time_limit){ 					break; 				} 				//reset visited array 				for (int j = 0; j < Cities.Length;j++){ 					visited[j] = false; 				}  				visited[i] = true; 				Route = new ArrayList(); 				Route.Add(Cities[i]); 				currentCity = i;  				//go through each other city 				for (int j = 0; j < Cities.Length;j++){ 					int nextCity = getNextCity(j, visited); 					totalCost += Cities[j].costToGetTo(Cities[nextCity]); 					visited[nextCity] = true; 					Route.Add(Cities[nextCity]); 				} 				numberOfSolutions++; 				//if there was a solution and it was better that the bestSoFar 				if(totalCost < bestSoFar && totalCost != double.PositiveInfinity){
					Route.RemoveAt(Route.Count - 1); 					bssf = new TSPSolution(Route); 					bestSoFar = totalCost; 					//don't draw it if it is initial; 					if(initialBssf){ 						currentBssf = bestSoFar; 						return null; 					} 					timer.Stop();  					results[COST] = bestSoFar.ToString(); 					results[TIME] = timer.Elapsed.Seconds.ToString(); 					results[COUNT] = numberOfSolutions.ToString(); 				} 			}  			timer.Stop(); 			results[COST] = bestSoFar.ToString(); 			results[TIME] = timer.Elapsed.Seconds.ToString(); 			results[COUNT] = numberOfSolutions.ToString();  			return results;         }  		private int getNextCity(int currentCity, Boolean[] visited){ 			double minCost = double.MaxValue; 			int minCity = currentCity; 			for (int i = 0; i < Cities.Length;i++){ 				if(visited[i] || i==currentCity){ 					continue; 				}	 				else { 					double tempCost = Cities[currentCity].costToGetTo(Cities[i]); 					if(tempCost < minCost){ 						minCost = tempCost; 						minCity = i; 					} 				} 			} 			return minCity; 
        }

        public string[] fancySolveProblem()
        {
            string[] results = new string[3];
			//set initial bssf
			initialBssf = true;
			greedySolveProblem();
			Stopwatch timer = new Stopwatch();
            // TODO bssf = output of greedy algorithm
            int numCities = Cities.Length;
			int solutions = 0;
			bool improved = true;
			timer.Start();
			// while(bestChild.route < bssf.route
			while (improved) {
				long time = timer.ElapsedMilliseconds;
				if (time > time_limit) {
					Console.WriteLine("Fancy timed out");
					break;
				}

				for (int i = 0; i < numCities - 1; i++) {
					for (int k = 0; k < numCities - 1; k++) {
						// swap i and kk
						TSPSolution checkSolution = swap(i, k);
						solutions++;
						double cost = checkSolution.costOfRoute();
						// check to see if it's a better solution than best child
						if (cost < bssf.costOfRoute()) {
							// if so, set best child be a solutionn
							bssf = checkSolution;
						}
						//if didn't improve, found local optimum
						else {
							improved = false;
						}
					}
				}
			}
			timer.Stop();
			results[COST] = bssf.costOfRoute().ToString();    // load results into array here, replacing these dummy values
			results[TIME] = timer.Elapsed.Seconds.ToString();
			results[COUNT] = solutions.ToString();

            return results;
        }

		TSPSolution swap(int i, int k)
        {
            ArrayList swapped = new ArrayList();

            int numCities = Cities.Length;
            for(int c = 0; i <= i - 1; ++c)
            {
                swapped.Add(Cities[c]);
            }

            int dec = 0;
            for (int c = i; c <= k; ++c)
            {
                swapped.Add(Cities[k - dec]);
                dec++;
            }

            for(int c = k + 1; c < numCities; ++c)
            {
                swapped.Add(Cities[c]);
            }
			return new TSPSolution(swapped);
        }
        #endregion
    }

}
