using System;
using TSP;

namespace WindowsFormsApplication1 {
	class RMatrix { 		public double[,] matrix;   		public RMatrix(RMatrix m, int citiesSize){ 			matrix = new double[citiesSize, citiesSize];   			Array.Copy(m.matrix, matrix, m.matrix.Length); 		} 		public RMatrix(City[] cities){ 			matrix = new double[cities.Length, cities.Length];
			for (int i = 0; i < cities.Length;i++){
				for (int j = 0; j < cities.Length;j++){
					if(j==i){
						matrix[i, j] = double.PositiveInfinity;
					}
					else{
						matrix[i, j] = cities[i].costToGetTo(cities[j]);
					}
				}
			} 		} 
		//reduces matrix and returns the lower bound 		public double computeLowerBound(int size){ 			double result = 0;  			//go through each row 			for (int row = 0; row < size;row++){ 				double min = double.PositiveInfinity; 				for (int col = 0; col < size;col++){ 					if(matrix[row,col] < min){ 						min = matrix[row, col]; 					} 				} 				if(!double.PositiveInfinity.Equals(min)){ 					result += min; 					for (int col = 0; col < size;col++){ 						matrix[row, col] -= min; 					} 				} 			}  			//go through each col 			for (int col = 0; col < size;col++){ 				double min = double.PositiveInfinity; 				for (int row = 0; row < size;row++){ 					if(matrix[row,col] < min){ 						min = matrix[row, col]; 					} 				} 				if(!double.IsPositiveInfinity(min)){ 					result += min; 					for (int row = 0; row < size;row++){ 						matrix[row, col] -= min; 					} 				} 			} 			return result; 		}   		public double update(int start, int end, int size){ 			double extraCost = matrix[start, end]; 			for (int i = 0; i < size;i++){ 				matrix[i, end] = double.PositiveInfinity; 				matrix[start, i] = double.PositiveInfinity; 			} 			return computeLowerBound(size) + extraCost; 		} 	} 
}
