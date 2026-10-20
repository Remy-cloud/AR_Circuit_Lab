using System.Collections.Generic;
using UnityEngine;

namespace ARCircuitsLab.Circuit
{
    /// <summary>
    /// Works out the voltage and current of every part using Ohm's law (I = V / R)
    /// and Kirchhoff's current law (nodal analysis).
    /// Plain C# with no scene objects needed, so it is easy to test on its own.
    /// </summary>
    public class CircuitSolver
    {
        // A tiny leak to ground for every node keeps loose parts from breaking the maths.
        private const double Leak = 1e-9;

        public struct Result
        {
            public float batteryCurrent;   // largest battery current (amps)
            public bool shortCircuit;
        }

        public Result Solve(IReadOnlyList<CircuitComponent> parts, IReadOnlyList<Wire> wires, float shortCircuitCurrent)
        {
            // 1. Every terminal is a node in the circuit.
            var node = new Dictionary<Terminal, int>();
            foreach (CircuitComponent p in parts)
            {
                node[p.A] = node.Count;
                node[p.B] = node.Count;
            }
            int n = node.Count;
            var result = new Result();
            if (n == 0) return result;

            var g = new double[n, n];   // conductance matrix
            var i = new double[n];      // current pushed into each node
            for (int k = 0; k < n; k++) g[k, k] = Leak;

            // 2. Each part and each wire adds its conductance (1 / R) between its two nodes.
            foreach (CircuitComponent p in parts)
            {
                float r = p.GetResistance();
                if (float.IsInfinity(r) || r <= 0f) continue;
                int a = node[p.A], b = node[p.B];
                Stamp(g, a, b, 1.0 / r);

                // A battery is modelled as a current source (EMF / r) beside its internal resistance.
                if (p is Battery battery)
                {
                    double push = battery.Emf / r;
                    i[b] += push;
                    i[a] -= push;
                }
            }
            foreach (Wire w in wires)
            {
                if (!node.TryGetValue(w.A, out int a) || !node.TryGetValue(w.B, out int b)) continue;
                Stamp(g, a, b, 1.0 / Wire.Resistance);
            }

            // 3. Solve G · V = I for the voltage at every node.
            double[] v = SolveLinear(g, i, n);

            // 4. Ohm's law gives the current through each part and wire.
            foreach (CircuitComponent p in parts)
            {
                double va = v[node[p.A]], vb = v[node[p.B]];
                float r = p.GetResistance();
                if (p is Battery battery)
                {
                    double current = (battery.Emf - (vb - va)) / r;   // flows from - to + inside the battery
                    p.ApplyResult((float)(vb - va), (float)current);
                    result.batteryCurrent = Mathf.Max(result.batteryCurrent, Mathf.Abs((float)current));
                }
                else if (float.IsInfinity(r))
                {
                    p.ApplyResult((float)(va - vb), 0f);
                }
                else
                {
                    p.ApplyResult((float)(va - vb), (float)((va - vb) / r));
                }
            }
            foreach (Wire w in wires)
            {
                if (!node.TryGetValue(w.A, out int a) || !node.TryGetValue(w.B, out int b)) continue;
                w.ApplyResult((float)((v[a] - v[b]) / Wire.Resistance));
            }

            // 5. Far more current than any bulb needs means the battery is shorted.
            result.shortCircuit = result.batteryCurrent > shortCircuitCurrent;
            return result;
        }

        private static void Stamp(double[,] g, int a, int b, double conductance)
        {
            g[a, a] += conductance;
            g[b, b] += conductance;
            g[a, b] -= conductance;
            g[b, a] -= conductance;
        }

        // Gaussian elimination with partial pivoting.
        private static double[] SolveLinear(double[,] a, double[] b, int n)
        {
            for (int col = 0; col < n; col++)
            {
                int pivot = col;
                for (int r = col + 1; r < n; r++)
                    if (System.Math.Abs(a[r, col]) > System.Math.Abs(a[pivot, col])) pivot = r;
                if (pivot != col)
                {
                    for (int c = 0; c < n; c++) (a[col, c], a[pivot, c]) = (a[pivot, c], a[col, c]);
                    (b[col], b[pivot]) = (b[pivot], b[col]);
                }
                for (int r = col + 1; r < n; r++)
                {
                    double f = a[r, col] / a[col, col];
                    if (f == 0) continue;
                    for (int c = col; c < n; c++) a[r, c] -= f * a[col, c];
                    b[r] -= f * b[col];
                }
            }
            var x = new double[n];
            for (int r = n - 1; r >= 0; r--)
            {
                double sum = b[r];
                for (int c = r + 1; c < n; c++) sum -= a[r, c] * x[c];
                x[r] = sum / a[r, r];
            }
            return x;
        }
    }
}
