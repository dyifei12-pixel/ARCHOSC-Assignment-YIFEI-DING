using System;
using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        // Input number of processes and resource types
        Console.Write("Enter number of processes: ");
        int processCount = int.Parse(Console.ReadLine());
        Console.Write("Enter number of resource types: ");
        int resourceCount = int.Parse(Console.ReadLine());

        // Input Allocation Matrix
        int[,] allocation = new int[processCount, resourceCount];
        Console.WriteLine("\nEnter Allocation Matrix (each line space-separated values):");
        for (int i = 0; i < processCount; i++)
        {
            Console.Write($"P{i + 1}: ");
            string[] parts = Console.ReadLine().Split();
            for (int j = 0; j < resourceCount; j++)
            {
                allocation[i, j] = int.Parse(parts[j]);
            }
        }

        // Input Max Demand Matrix
        int[,] maxDemand = new int[processCount, resourceCount];
        Console.WriteLine("\nEnter Maximum Demand Matrix:");
        for (int i = 0; i < processCount; i++)
        {
            Console.Write($"P{i + 1}: ");
            string[] parts = Console.ReadLine().Split();
            for (int j = 0; j < resourceCount; j++)
            {
                maxDemand[i, j] = int.Parse(parts[j]);
            }
        }

        // Input Available resources
        int[] available = new int[resourceCount];
        Console.Write("\nEnter available resources space-separated: ");
        string[] availParts = Console.ReadLine().Split();
        for (int j = 0; j < resourceCount; j++)
        {
            available[j] = int.Parse(availParts[j]);
        }

        // Calculate Need matrix = Max - Allocation
        int[,] need = new int[processCount, resourceCount];
        for (int i = 0; i < processCount; i++)
        {
            for (int j = 0; j < resourceCount; j++)
            {
                need[i, j] = maxDemand[i, j] - allocation[i, j];
            }
        }

        // Banker's Algorithm to find safe sequence
        bool[] finished = new bool[processCount];
        List<string> safeSequence = new List<string>();
        int[] work = (int[])available.Clone();
        bool foundProcess;

        do
        {
            foundProcess = false;
            for (int i = 0; i < processCount; i++)
            {
                if (!finished[i])
                {
                    bool canAllocate = true;
                    for (int j = 0; j < resourceCount; j++)
                    {
                        if (need[i, j] > work[j])
                        {
                            canAllocate = false;
                            break;
                        }
                    }

                    if (canAllocate)
                    {
                        // Release allocated resources after process finishes
                        for (int j = 0; j < resourceCount; j++)
                        {
                            work[j] += allocation[i, j];
                        }
                        finished[i] = true;
                        safeSequence.Add($"P{i + 1}");
                        foundProcess = true;
                    }
                }
            }
        } while (foundProcess);

        // Judge safe state
        bool isSafe = true;
        foreach (bool f in finished)
        {
            if (!f)
            {
                isSafe = false;
                break;
            }
        }

        // Output result
        Console.WriteLine("\n===== Banker's Algorithm Result =====");
        if (isSafe)
        {
            Console.WriteLine("Safe Sequence: " + string.Join(" → ", safeSequence));
            Console.WriteLine("System is in a safe state.");
        }
        else
        {
            Console.WriteLine("No valid safe sequence exists.");
            Console.WriteLine("System is in an unsafe state (deadlock risk).");
        }
    }
}
