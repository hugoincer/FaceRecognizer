using System;
using System.Collections.Generic;
using System.Drawing;

namespace Lab4
{
    public class Hopfield
    {
        int amountOfNeurons;
        int[,] matrixOfWeights;
        Dictionary<int[], string> collectionOfImages = new Dictionary<int[], string>();

        public Hopfield(int imageArea)
        {
            this.amountOfNeurons = imageArea;
            matrixOfWeights = new int[amountOfNeurons, amountOfNeurons];
        }

        public void AddImage(int[] inputVector, string nameOfClass)
        {
            collectionOfImages.Add(inputVector, nameOfClass);
        }

        public int[] ConvertImageInInputVector(Bitmap image)
        {
            int[] inputVector = new int[amountOfNeurons * 2];

            int length = (int)Math.Sqrt(amountOfNeurons);

            for (int i = 0; i < image.Height; i++)
            {
                for (int j = 0; j < image.Width; j++)
                {
                    var pixel = image.GetPixel(j, i);
                    var grayPixel = (30 * pixel.R + 59 * pixel.G + 11 * pixel.B) / 100;

                    inputVector[i * length + j] = grayPixel < 128 ? 1 : -1;                    
                }
            }
            return inputVector;
        }

        private bool IsBlackPixel(Color pixel)
        {
            return ((pixel.R < 128) || (pixel.G < 128) || (pixel.B < 128));
        }

        public void LearningOfNetwork(int[] inputVector)
        {
            for (int i = 0; i < amountOfNeurons; i++)
            {
                for (int j = 0; j < amountOfNeurons; j++)
                {
                    if (i == j)
                    {
                        matrixOfWeights[i, j] = 0;
                    }
                    else
                    {
                        matrixOfWeights[i, j] += inputVector[i] * inputVector[j];
                    }
                }
            }
        }

        public string GetNameInputVector(int[] inputVector)
        {
            foreach (KeyValuePair<int[], string> standardVector in collectionOfImages)
            {
                int[] vectorFromCollectionOfStandard = standardVector.Key;
                bool concurrency = true;
                for (int i = 0; i < amountOfNeurons; i++)
                {
                    if (inputVector[i] != vectorFromCollectionOfStandard[i])
                    {
                        concurrency = false;
                        break;
                    }
                }

                if (concurrency)
                {
                    return standardVector.Value;
                }
            }
            return null;
        }

        public string ClassifedInputVector(int[] inputVector)
        {
            Random rand = new Random();
            int maxAmountOfIteration = 30000;
            var probVector = new Prob() { Weight = -1, Name = "Неопределено" };
            for (int i = 0; i < maxAmountOfIteration; i++)
            {
                int neuron = 0;
                int r = rand.Next(0, amountOfNeurons);

                for (int j = 0; j < amountOfNeurons; j++)
                {
                    neuron += inputVector[j] * matrixOfWeights[j, r];
                }
                inputVector[r] = neuron < 0 ? -1 : 1;

                string classNameOfInputVector = GetNameInputVector(inputVector);
                if (classNameOfInputVector != null)
                {
                    return classNameOfInputVector;
                }
                var prob = GetProbabilityVector(inputVector);
                if (prob.Weight>probVector.Weight)
                {
                    probVector = prob;
                }
            }
            return probVector.Name;
        }

        public Prob GetProbabilityVector (int[] inputVector)
        {
            Prob result = new Prob() { Weight = -1, Name = "Неопределен" };
            foreach (KeyValuePair<int[], string> standardVector in collectionOfImages)
            {
                int[] vectorFromCollectionOfStandard = standardVector.Key;
                int exactNeurons = 0;
                for (int i = 0; i < amountOfNeurons; i++)
                {
                    if (inputVector[i] == vectorFromCollectionOfStandard[i])
                    {
                        exactNeurons++;                        
                    }
                }
                double current = exactNeurons * inputVector.Length / standardVector.Key.Length;
                if (result.Weight< current)
                {
                    result.Weight = current;
                    result.Name = standardVector.Value;
                }
                
            }
            return result;
        }
    }

    public struct Prob
    {
        public string Name;
        public double Weight;
    }
}
