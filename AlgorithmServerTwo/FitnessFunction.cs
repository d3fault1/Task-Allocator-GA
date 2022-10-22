using Common;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Fitnesses;
using System.Collections.Generic;
using System.Linq;

namespace AlgorithmServerTwo
{
    class FitnessFunction : IFitness
    {
        List<TaskData> tasks;
        List<ProcessorData> processors;
        double runtime, total_energy;
        double[] times;
        public double Evaluate(IChromosome chromosome)
        {
            times = new double[processors.Count];
            total_energy = 0;
            var genes = chromosome.GetGenes();
            for (int i = 0; i < genes.Length; i++)
            {
                int p = (int)genes[i].Value;
                double requiredTime = (tasks[i].Time * tasks[i].ReferenceFrequency) / processors[p].Frequency;
                double energy = processors[p].Energy * requiredTime + tasks[i].LocalEnergy + tasks[i].RemoteEnergy;
                times[p] += requiredTime;
                total_energy += energy;
            }
            double max = times.Max();
            if (max > runtime) return 0;
            else return 1 / total_energy;
        }

        public FitnessFunction(List<TaskData> tasks, List<ProcessorData> processors, double runtime)
        {
            this.tasks = tasks;
            this.processors = processors;
            this.runtime = runtime;
        }
    }
}
