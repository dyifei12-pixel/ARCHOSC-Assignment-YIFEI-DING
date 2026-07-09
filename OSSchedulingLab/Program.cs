using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// 进程/作业实体类，存储参数与调度计算指标
/// </summary>
public class Job
{
    // 原始输入参数
    public string Name { get; set; }
    public int ArrivalTime { get; set; }
    public int ExecutionTime { get; set; }
    public int Priority { get; set; }

    // 调度输出指标
    public int FinishTime { get; set; }
    public int TurnaroundTime => FinishTime - ArrivalTime;
    public double NormalisedTurnaround => (double)TurnaroundTime / ExecutionTime;

    // RR算法剩余执行时间
    public int RemainingBurst { get; set; }
}

/// <summary>
/// 长程调度器：从磁盘作业池筛选准入内存的进程
/// 准入规则：优先级 > 5
/// </summary>
public static class LongTermScheduler
{
    /// <summary>
    /// 筛选符合优先级条件的作业
    /// </summary>
    public static List<Job> AdmitJobs(List<Job> jobPool)
    {
        return jobPool.Where(job => job.Priority > 5).ToList();
    }

    /// <summary>
    /// 打印所有准入内存的作业信息
    /// </summary>
    public static void PrintAdmittedJobs(List<Job> admittedJobs)
    {
        Console.WriteLine("===== Long-Term Scheduler Admitted Jobs (Priority > 5) =====");
        foreach (var job in admittedJobs)
        {
            Console.WriteLine($"Job {job.Name}, Priority {job.Priority}, Arrival {job.ArrivalTime}, Execution {job.ExecutionTime}");
        }
        Console.WriteLine();
    }
}

/// <summary>
/// 通用工具类：打印甘特图、性能指标表格
/// </summary>
public static class GanttChartPrinter
{
    /// <summary>
    /// 文本形式输出甘特图
    /// </summary>
    public static void PrintGantt(string title, List<string> timeline)
    {
        Console.WriteLine($"\n---------- Gantt Chart: {title} ----------");
        StringBuilder bar = new StringBuilder();
        StringBuilder timeAxis = new StringBuilder();

        for (int t = 0; t < timeline.Count; t++)
        {
            bar.Append(timeline[t]);
            timeAxis.Append(t.ToString().PadLeft(2));
        }
        Console.WriteLine($"Timeline Block: {bar}");
        Console.WriteLine($"Time Axis:      {timeAxis}");
        Console.WriteLine("-------------------------------------------\n");
    }

    /// <summary>
    /// 打印完成时间、周转时间、归一化周转时间，并计算平均值
    /// </summary>
    public static void PrintMetricsTable(List<Job> jobs)
    {
        Console.WriteLine("Process | Finish | Turnaround | Normalised TAT");
        Console.WriteLine("-------------------------------------------");
        foreach (var j in jobs)
        {
            Console.WriteLine($"{j.Name,6} | {j.FinishTime,6} | {j.TurnaroundTime,10} | {j.NormalisedTurnaround,14:F2}");
        }
        double avgTat = jobs.Average(x => x.TurnaroundTime);
        double avgNormTat = jobs.Average(x => x.NormalisedTurnaround);
        Console.WriteLine($"Average Turnaround Time: {avgTat:F2}");
        Console.WriteLine($"Avg Normalised TAT: {avgNormTat:F2}\n");
    }
}

/// <summary>
/// FCFS 先来先服务调度（非抢占式）
/// </summary>
public static class FCFSScheduler
{
    public static void RunFCFS(List<Job> inputJobs)
    {
        // 复制原始数据，防止修改原始进程列表
        List<Job> jobs = new List<Job>();
        foreach (var j in inputJobs)
        {
            jobs.Add(new Job
            {
                Name = j.Name,
                ArrivalTime = j.ArrivalTime,
                ExecutionTime = j.ExecutionTime,
                Priority = j.Priority
            });
        }

        // 按到达时间升序排序
        jobs.Sort((a, b) => a.ArrivalTime.CompareTo(b.ArrivalTime));
        int currentTime = 0;
        List<string> timeline = new List<string>();

        foreach (var job in jobs)
        {
            // CPU空闲，等待进程到达
            while (currentTime < job.ArrivalTime)
            {
                timeline.Add("_");
                currentTime++;
            }
            // 一次性完整执行当前进程
            for (int t = 0; t < job.ExecutionTime; t++)
            {
                timeline.Add(job.Name);
                currentTime++;
            }
            job.FinishTime = currentTime;
        }

        GanttChartPrinter.PrintGantt("FCFS Scheduling", timeline);
        GanttChartPrinter.PrintMetricsTable(jobs);
    }
}

/// <summary>
/// 时间片轮转RR调度 + 带静态优先级的RR拓展
/// </summary>
public static class RRScheduler
{
    /// <summary>
    /// 基础RR调度，无优先级
    /// </summary>
    public static void RunRR(List<Job> inputJobs, int timeQuantum)
    {
        List<Job> jobs = new List<Job>();
        foreach (var j in inputJobs)
        {
            jobs.Add(new Job
            {
                Name = j.Name,
                ArrivalTime = j.ArrivalTime,
                ExecutionTime = j.ExecutionTime,
                RemainingBurst = j.ExecutionTime,
                Priority = j.Priority
            });
        }

        int currentTime = 0;
        Queue<Job> readyQueue = new Queue<Job>();
        List<string> timeline = new List<string>();
        int completedCount = 0;
        int totalJobs = jobs.Count;

        while (completedCount < totalJobs)
        {
            // 将当前时间已到达、未完成的进程加入就绪队列
            var arrived = jobs.Where(x => x.ArrivalTime <= currentTime && x.RemainingBurst > 0 && !readyQueue.Contains(x)).ToList();
            foreach (var p in arrived) readyQueue.Enqueue(p);

            // CPU无进程，空闲等待
            if (readyQueue.Count == 0)
            {
                timeline.Add("_");
                currentTime++;
                continue;
            }

            Job current = readyQueue.Dequeue();
            int runTime = Math.Min(timeQuantum, current.RemainingBurst);

            // 运行对应时间片
            for (int t = 0; t < runTime; t++)
            {
                timeline.Add(current.Name);
                current.RemainingBurst--;
                currentTime++;
            }

            // 未执行完放回队列，执行完毕记录完成时间
            if (current.RemainingBurst > 0)
            {
                readyQueue.Enqueue(current);
            }
            else
            {
                current.FinishTime = currentTime;
                completedCount++;
            }
        }

        GanttChartPrinter.PrintGantt($"Round Robin TQ={timeQuantum}", timeline);
        GanttChartPrinter.PrintMetricsTable(jobs);
    }

    /// <summary>
    /// 拓展：基于静态优先级的RR调度，每次轮换优先运行高优先级进程
    /// </summary>
    public static void RunPriorityRR(List<Job> inputJobs, int timeQuantum)
    {
        List<Job> jobs = new List<Job>();
        foreach (var j in inputJobs)
        {
            jobs.Add(new Job
            {
                Name = j.Name,
                ArrivalTime = j.ArrivalTime,
                ExecutionTime = j.ExecutionTime,
                RemainingBurst = j.ExecutionTime,
                Priority = j.Priority
            });
        }

        int currentTime = 0;
        List<Job> readyList = new List<Job>();
        List<string> timeline = new List<string>();
        int completed = 0;
        int total = jobs.Count;

        while (completed < total)
        {
            var arrived = jobs.Where(x => x.ArrivalTime <= currentTime && x.RemainingBurst > 0 && !readyList.Contains(x)).ToList();
            readyList.AddRange(arrived);

            if (readyList.Count == 0)
            {
                timeline.Add("_");
                currentTime++;
                continue;
            }

            // 就绪列表按优先级降序，高优先级优先执行
            readyList.Sort((a, b) => b.Priority.CompareTo(a.Priority));
            Job current = readyList[0];
            readyList.RemoveAt(0);

            int run = Math.Min(timeQuantum, current.RemainingBurst);
            for (int t = 0; t < run; t++)
            {
                timeline.Add(current.Name);
                current.RemainingBurst--;
                currentTime++;
            }

            if (current.RemainingBurst > 0)
                readyList.Add(current);
            else
            {
                current.FinishTime = currentTime;
                completed++;
            }
        }

        GanttChartPrinter.PrintGantt($"Priority RR TQ={timeQuantum}", timeline);
        GanttChartPrinter.PrintMetricsTable(jobs);
    }
}

/// <summary>
/// 程序入口主函数，运行全部实验模块
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        // 实验Table1 完整进程数据
        List<Job> fullJobPool = new List<Job>()
        {
            new Job { Name = "A", ArrivalTime = 0, ExecutionTime = 3, Priority = 5 },
            new Job { Name = "B", ArrivalTime = 2, ExecutionTime = 6, Priority = 4 },
            new Job { Name = "C", ArrivalTime = 5, ExecutionTime = 5, Priority = 8 },
            new Job { Name = "D", ArrivalTime = 6, ExecutionTime = 3, Priority = 6 },
            new Job { Name = "E", ArrivalTime = 8, ExecutionTime = 6, Priority = 10 },
            new Job { Name = "F", ArrivalTime = 9, ExecutionTime = 2, Priority = 3 },
            new Job { Name = "G", ArrivalTime = 10, ExecutionTime = 6, Priority = 7 }
        };

        // ========== Section 1：Long-Term Scheduling 长程调度 ==========
        Console.WriteLine("========== SECTION 1: LONG TERM SCHEDULER ==========");
        var admittedJobs = LongTermScheduler.AdmitJobs(fullJobPool);
        LongTermScheduler.PrintAdmittedJobs(admittedJobs);

        // ========== Section 2 Task1：FCFS 调度 ==========
        Console.WriteLine("========== SECTION 2 TASK1: FCFS SCHEDULING ==========");
        FCFSScheduler.RunFCFS(fullJobPool);

        // ========== Section 2 Task2：多时间片RR TQ=1,3,4,6 ==========
        Console.WriteLine("========== SECTION 2 TASK2: ROUND ROBIN VARYING TIME QUANTUM ==========");
        int[] timeQuanta = { 1, 3, 4, 6 };
        foreach (int tq in timeQuanta)
        {
            RRScheduler.RunRR(fullJobPool, tq);
        }

        // ========== Section 3：带优先级RR TQ=1、6 ==========
        Console.WriteLine("========== SECTION3: PRIORITY-BASED ROUND ROBIN ==========");
        RRScheduler.RunPriorityRR(fullJobPool, 1);
        RRScheduler.RunPriorityRR(fullJobPool, 6);

        Console.WriteLine("\nAll scheduling simulation tasks completed.");
    }
}