using NUnit.Framework;

// ONLY ONE browser at a time
[assembly: NonParallelizable]
[assembly: LevelOfParallelism(1)]
