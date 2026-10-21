// The scenarios set process environment variables, which every test class
// shares: run them one at a time.
[assembly: Xunit.CollectionBehavior(DisableTestParallelization = true)]
