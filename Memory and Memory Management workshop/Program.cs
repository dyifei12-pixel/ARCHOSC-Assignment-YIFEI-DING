using System;
using System.Collections.Generic;

// TLB entry stores VPN to PPN mapping
public class TLBEntry
{
    public int VPN;
    public int PPN;
}

// TLB simulator with FIFO replacement policy
public class TLB
{
    private List<TLBEntry> entries = new List<TLBEntry>();
    private const int capacity = 4;
    public int HitCount { get; private set; } = 0;
    public int MissCount { get; private set; } = 0;

    // Look up VPN in TLB, return PPN if hit, null if miss
    public int? Lookup(int vpn)
    {
        foreach (var entry in entries)
        {
            if (entry.VPN == vpn)
            {
                HitCount++;
                return entry.PPN;
            }
        }
        MissCount++;
        return null;
    }

    // Insert new mapping, remove oldest entry when TLB full
    public void Insert(int vpn, int ppn)
    {
        if (entries.Count >= capacity)
            entries.RemoveAt(0);
        entries.Add(new TLBEntry { VPN = vpn, PPN = ppn });
    }

    // Calculate TLB hit rate
    public double GetHitRate()
    {
        int totalAccess = HitCount + MissCount;
        return totalAccess == 0 ? 0 : (double)HitCount / totalAccess;
    }
}

class Program
{
    static void Main(string[] args)
    {
        TLB tlbSimulator = new TLB();

        // Simulate virtual page access sequence
        int[] accessSequence = { 0, 1, 3, 0, 5, 1, 7, 3, 0, 2 };

        // Page table mapping: VPN -> PPN
        Dictionary<int, int> pageTable = new Dictionary<int, int>()
        {
            { 0, 2 },
            { 1, 9 },
            { 3, 5 },
            { 5, 4 },
            { 7, 1 },
            { 2, 8 }
        };

        Console.WriteLine("===== TLB Simulation Output =====");
        foreach (int vpn in accessSequence)
        {
            int? resultPPN = tlbSimulator.Lookup(vpn);
            if (resultPPN == null)
            {
                Console.WriteLine($"TLB MISS | VPN = {vpn}, Fetch PPN from Page Table");
                int realPPN = pageTable[vpn];
                tlbSimulator.Insert(vpn, realPPN);
            }
            else
            {
                Console.WriteLine($"TLB HIT  | VPN = {vpn}, PPN = {resultPPN}");
            }
        }

        // Print final statistics
        int total = tlbSimulator.HitCount + tlbSimulator.MissCount;
        Console.WriteLine("\n===== Simulation Statistics =====");
        Console.WriteLine($"Total memory accesses: {total}");
        Console.WriteLine($"TLB Hit count: {tlbSimulator.HitCount}");
        Console.WriteLine($"TLB Miss count: {tlbSimulator.MissCount}");
        Console.WriteLine($"TLB Hit Rate: {tlbSimulator.GetHitRate():P2}");
    }
}
