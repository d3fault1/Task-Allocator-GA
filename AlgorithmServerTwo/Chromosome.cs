using Common;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Randomizations;
using System.Collections.Generic;

namespace AlgorithmServerTwo
{
    class Chromosome : ChromosomeBase
    {
        readonly double runtime;
        readonly List<ProcessorData> processors;
        readonly List<TaskData> tasks;
        public override IChromosome CreateNew()
        {
            return new Chromosome(tasks, processors, runtime);
        }

        public override Gene GenerateGene(int geneIndex)
        {
            int rand = RandomizationProvider.Current.GetInt(0, tasks[geneIndex].EligibleProcessors.Length);
            return new Gene(tasks[geneIndex].EligibleProcessors[rand]);
        }

        public Chromosome(List<TaskData> tasks, List<ProcessorData> processors, double runtime) : base(tasks.Count)
        {
            this.runtime = runtime;
            this.processors = processors;
            this.tasks = tasks;
            CreateGenes();
        }
    }
}
