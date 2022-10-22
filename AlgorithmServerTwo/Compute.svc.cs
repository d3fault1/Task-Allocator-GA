using Common;
using GeneticSharp.Domain;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Terminations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AlgorithmServerTwo
{
    public class Compute : ICompute
    {
        public List<TaskData> Tasks { get; set; }
        public List<ProcessorData> Processors { get; set; }
        public double Runtime { get; set; }

        private List<int[]> retval = new List<int[]>();
        private IChromosome best = null;

        public List<int[]> Run(List<TaskData> Tasks, List<ProcessorData> Processors, double Runtime, double Timeout)
        {

            this.Tasks = Tasks;
            this.Processors = Processors;
            this.Runtime = Runtime;

            try
            {
                var selection = new TournamentSelection();
                var crossover = new OnePointCrossover();
                var mutation = new DisplacementMutation();
                var fitness = new FitnessFunction(Tasks, Processors, Runtime);
                var chromosome = new Chromosome(Tasks, Processors, Runtime);
                var population = new Population(600, 1100, chromosome);


                GeneticAlgorithm ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation);
                ga.Termination = new OrTermination(new ITermination[] { new GenerationNumberTermination(20000), new TimeEvolvingTermination(TimeSpan.FromMinutes(Timeout)) });
                ga.GenerationRan += GenerationRan;
                Console.WriteLine("Search Initiated.......");
                ga.Start();
                double en, tm;
                tm = getMaxTime(ga.BestChromosome);
                en = 1 / ga.BestChromosome.Fitness.Value;
                Console.WriteLine("Best solution found has Time: {0}, Energy: {1}", tm, en);
                retval.Reverse();
                return retval;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        private void GenerationRan(object sender, EventArgs e)
        {
            var ga = (GeneticAlgorithm)sender;
            var genes = ga.BestChromosome.GetGenes();
            var member = new int[genes.Length];
            for (int i = 0; i < genes.Length; i++)
            {
                member[i] = (int)genes[i].Value;
                Console.Write(genes[i].Value + " ");
            }
            if (ga.BestChromosome.CompareTo(best) != 0) retval.Add(member);
            best = ga.BestChromosome;
            double en, tm;
            tm = getMaxTime(ga.BestChromosome);
            en = 1 / ga.BestChromosome.Fitness.Value;
            Console.WriteLine(String.Format("Generation best has Time: {0}, Energy: {1}", tm, en));
        }

        private double getMaxTime(IChromosome chromosome)
        {
            double[] times = new double[Processors.Count];
            var genes = chromosome.GetGenes();
            for (int i = 0; i < genes.Length; i++)
            {
                int p = (int)genes[i].Value;
                double time = Tasks[i].Time * Tasks[i].ReferenceFrequency / Processors[p].Frequency;
                times[p] += time;
            }
            return times.Max();
        }
    }
}
