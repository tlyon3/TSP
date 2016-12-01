using System;
using System.Collections.Generic;

namespace WindowsFormsApplication1 {
	class PQueue { 		public int count; 		ChildrenQueue current;
		//keep track of the nodes at each level of the tree 		List<ChildrenQueue>[] levels; 		public int max; 
 		public PQueue(int citiesSize) { 			levels = new List<ChildrenQueue>[citiesSize];
			for (int i = 0; i < levels.Length;i++){
				levels[i] = new List<ChildrenQueue>();
			} 		}  		public void add(State state) { 			count++; 			ChildrenQueue child = new ChildrenQueue(state); 			if (current != null) { 				current.children.Add(child); 			}
			//add node into correct level 			levels[state.route.Count - 1].Add(child); 			if (count > max) { 				max = count; 			} 		} 
 		public State deleteMin() { 			count--;
			//find leaf node 			if (current != null && current.children.Count > 0) {
				//find smallest path 				int minIndex = 0; 				for (int i = 0; i < current.children.Count; i++) { 					if (current.children[i].state.bssfCost < current.children[minIndex].state.bssfCost) { 						minIndex = i; 					} 				} 				//reset the currentNode 				ChildrenQueue child = current.children[minIndex]; 				current.children.RemoveAt(minIndex); 				levels[child.state.route.Count - 1].Remove(child); 				current = child; 			} 			else { 				List<ChildrenQueue> highestLevel = null; 				for (int i = 0; i < levels.Length; i++) { 					if (levels[i].Count != 0) { 						highestLevel = levels[i]; 						break; 					} 				} 				int minIndex = 0; 				for (int i = 0; i < highestLevel.Count; i++) { 					if (highestLevel[i].state.bssfCost < highestLevel[minIndex].state.bssfCost) { 						minIndex = i; 					} 				} 				current = highestLevel[minIndex]; 				highestLevel.Remove(current); 			} 			return current.state; 		}

		public void pruneCurrentPath(){
			prune(current.children);
		}  		public void prune(List<ChildrenQueue> children) {
			//remove current state and its children 			for (int i = 0; i < children.Count; i++) { 				levels[children[i].state.route.Count - 1].Remove(children[i]); 				count--; 				prune(children[i].children); 			} 		}  		public bool empty() { 			return count == 0; 		} 	}
	//lets the current state know its children 	class ChildrenQueue { 		public State state; 		public List<ChildrenQueue> children; 		public ChildrenQueue(State state) { 			this.state = state; 			this.children = new List<ChildrenQueue>(); 		} 	} 
}
