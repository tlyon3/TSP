using System;
using System.Collections;
using System.Collections.Generic;
using TSP;

namespace WindowsFormsApplication1 {
	 class State {
		
		public RMatrix matrix; 		public double currentCost; 		public City city; 		public HashSet<int> visited; 		public int node; 		public ArrayList route; 		public double bssfCost;  		public State(State prevState, double extraCost, int node, City city, int size) { 			this.node = node; 			this.city = city; 			this.currentCost = prevState.currentCost + extraCost; 			this.visited = new HashSet<int>(prevState.visited); 			this.visited.Add(node); 			this.matrix = new RMatrix(prevState.matrix, size); 			this.bssfCost = matrix.update(prevState.node, node, size) + prevState.bssfCost; 			this.route = (ArrayList)prevState.route.Clone(); 			this.route.Add(city); 		}  		public State(RMatrix m, City c, int n, int s) { 			this.matrix = m; 			this.bssfCost = m.computeLowerBound(s); 			this.city = c; 			this.node = n; 			this.visited = new HashSet<int>(); 			this.visited.Add(n); 			this.route = new ArrayList(); 			this.route.Add(c); 		} 	} 
}
