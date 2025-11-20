using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mtree
{
    /// <summary>
    /// Class that allows to randomly select an index with a weigthed probability
    /// </summary>
    public class WeightedRandom
    {
        private float[] probs;
        private int[] alias;

        public WeightedRandom(float[] weights)
        {
            PrepareWeights(weights);
        }

        private void PrepareWeights(float[] weights)
        {
            Queue<int> small = new Queue<int>();
            Queue<int> large = new Queue<int>();
            int n = weights.Length;

            alias = new int[n];
            probs = new float[n];

            float[] scaledWeights = new float[n];

            for (int i = 0; i < weights.Length; i++)
            {
                scaledWeights[i] = weights[i] * n;

                if (scaledWeights[i] >= 1)
                    large.Enqueue(i);
                else
                    small.Enqueue(i);
            }

            while (small.Count > 0 && large.Count > 0)
            {
                int l = small.Dequeue();
                int g = large.Dequeue();
                probs[l] = scaledWeights[l];
                alias[l] = g;
                scaledWeights[g] = scaledWeights[g] + scaledWeights[l] - 1;
                if (scaledWeights[g] < 1)
                    small.Enqueue(g);
                else
                    large.Enqueue(g);
            }
            while (large.Count > 0)
            {
                probs[large.Dequeue()] = 1;
            }
            while (small.Count > 0)
            {
                probs[small.Dequeue()] = 1;
            }
        }

        public int GetRandomIndex(System.Random random)
        {
            int i = random.Next(0, probs.Length);
            if (random.NextDouble() < probs[i])
                return i;
            else
                return alias[i];
        }

    }
}
