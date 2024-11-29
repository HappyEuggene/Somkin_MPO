using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cells0
{
    class Cells0
    {
        private int n;

        private int k;

        private double p;

        private const int TIME_UNIT_MS = 1000;

        private int[] cells;

        private volatile bool running = true;

        private Task[] atomTasks;

        public static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Using: Cells0 N K p");
                return;
            }

            Cells0 cells0 = new Cells0(args);
            cells0.StartSimulation();
        }

        public Cells0(string[] args)
        {
            try
            {
                this.n = int.Parse(args[0]);
                this.k = int.Parse(args[1]);
                this.p = double.Parse(args[2]);

                if (n <= 0 || k <= 0 || p < 0 || p > 1)
                {
                    throw new ArgumentException("Incorrect parameters. Make sure that N and K > 0, 0 ≤ p ≤ 1.");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }

            this.cells = new int[n];
            this.cells[0] = k;
        }

        public void StartSimulation()
        {
            Console.WriteLine($"Initial number of threads: {k}");
            Console.WriteLine($"Duration: 1 minute");
            Console.WriteLine("Snapshots every second:");

            atomTasks = new Task[k];
            for (int i = 0; i < k; i++)
            {
                atomTasks[i] = Task.Run(() => ParticleRun());
            }

            for (int second = 1; second <= 60; second++)
            {
                Thread.Sleep(1000); 
                PrintSnapshot(second);
            }

            running = false;

            try
            {
                Task.WaitAll(atomTasks);
            }
            catch (AggregateException)
            {

            }

            Console.WriteLine("The simulation is complete.");
            VerifyTotalAtoms();
        }

        public int GetCell(int i)
        {
            if (i >= 0 && i < n)
                return cells[i];
            else
                throw new IndexOutOfRangeException();
        }

        public void MoveParticle(int from, int to)
        {
            if (cells[from] > 0) 
            {
                cells[from]--;
                cells[to]++;
            }
        }

        private void PrintSnapshot(int second)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"[{second}s] ");
            for (int i = 0; i < n; i++)
            {
                sb.Append($"{cells[i]} ");
            }
            Console.WriteLine(sb.ToString());
        }

        private void VerifyTotalAtoms()
        {
            int total = 0;
            for (int i = 0; i < n; i++)
            {
                total += cells[i];
            }
            Console.WriteLine($"Initial number of threads: {k}");
            Console.WriteLine($"“Finite number of atoms: {total}");
            if (total != k)
            {
                Console.WriteLine("The total number of atoms has changed");
            }
            else
            {
                Console.WriteLine("The total number of atoms remained unchanged.");
            }
        }

        private void ParticleRun()
        {
            Random random = new Random(Guid.NewGuid().GetHashCode());
            int cell = 0; 

            while (running)
            {
                double m = random.NextDouble();

                int newPos = cell;
                if (m > p)
                {
                    newPos = cell + 1;
                }
                else
                {
                    newPos = cell - 1;
                }

                if (newPos < 0 || newPos >= n)
                {
                    newPos = cell;
                }

                MoveParticle(cell, newPos);
                cell = newPos;

                try
                {
                    Thread.Sleep(TIME_UNIT_MS);
                }
                catch (ThreadInterruptedException)
                {
                    break;
                }
            }
        }
    }
}
