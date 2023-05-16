using BenchmarkDotNet.Running;
using Matrix4Net.Benchmarks.MatrixOperations.Benchmarks;
using Matrix4Net.Benchmarks.Utitlities.Benchmarks;

var summary = BenchmarkRunner.Run<MultiplicationBenchmark>();