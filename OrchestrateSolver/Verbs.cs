using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchestrateSolver
{
	public static class Verbs
	{
		public const int Count = 26;
		public enum Id
	    {
		    Trade = 0,
		    Build = 1,
		    Cook = 2,        
		    MakeMedicine = 3,
		    Procreate = 4,        
		    Fish = 5,        
		    Forage = 6,        
		    Hunt = 7,			
		    MakeMachinery = 8,			
		    MakeEnergy = 9,			
		    MakeTools = 10,			
		    RaiseCattle = 11,			
		    Mine = 12,			
		    Print3D = 13,			
		    Compute = 14,			
		    Farm = 15,			
		    Innovate = 16,			
		    Repair = 17,			
		    Socialize = 18,			
		    ChopTrees = 19,		
		    Teach = 20,		
		    Write = 21,		
		    Brew = 22,		
		    Luxuriate = 23,		
		    Recycle = 24,		
		    Read = 25
	    }	
	    
	    public static readonly Verb Trade = VerbFactory.Begin(Verbs.Id.Trade, 'A', "Trade")
	        .With(Resource.Buildings, -25)
	        .With(Resource.Food, -25)
	        .With(Resource.Joy, 25);

        public static readonly Verb Build = VerbFactory.Begin(Verbs.Id.Build, 'B', "Build")
	        .With(Resource.Stone, -25)
	        .With(Resource.Wood, -25)
	        .With(Resource.Tools, -25)
	        .With(Resource.Buildings, 75);

        public static readonly Verb Cook = VerbFactory.Begin(Verbs.Id.Cook, 'C', "Cook")
	        .With(Resource.Wood, -25)
	        .With(Resource.Buildings, -25)
	        .With(Resource.Ingredients, -25)
	        .With(Resource.Food, 75);
        
        public static readonly Verb MakeMedicine = VerbFactory.Begin(Verbs.Id.MakeMedicine, 'D', "Make Medicine")
	        .With(Resource.Buildings, -25)
	        .With(Resource.Herbs, -25)
	        .With(Resource.People, 100)
	        .With(Resource.Knowledge, -25);

        public static readonly Verb Procreate = VerbFactory.Begin(Verbs.Id.Procreate, 'E', "Procreate")
	        .With(Resource.Food, -75)
	        .With(Resource.People, 75);
        
        public static readonly Verb Fish = VerbFactory.Begin(Verbs.Id.Fish, 'F', "Fish")
			.With(Resource.Tools, -25)
			.With(Resource.Ingredients, 25);
        
        public static readonly Verb Forage = VerbFactory.Begin(Verbs.Id.Forage, 'G', "Forage")
			.With(Resource.Tools, -50)
			.With(Resource.Ingredients, 25)
			.With(Resource.Herbs, 25);
        
        public static readonly Verb Hunt = VerbFactory.Begin(Verbs.Id.Hunt, 'H', "Hunt")
			.With(Resource.Tools, -25)
			.With(Resource.Ingredients, 25);
			
        public static readonly Verb MakeMachinery = VerbFactory.Begin(Verbs.Id.MakeMachinery, 'I', "Make Machinery")
			.With(Resource.Tools, -25)
			.With(Resource.Buildings, -25)
			.With(Resource.Knowledge, -25)
			.With(Resource.Machinery, 75);
			
        public static readonly Verb MakeEnergy = VerbFactory.Begin(Verbs.Id.MakeEnergy, 'J', "Make Energy")
			.With(Resource.Tools, -25)
			.With(Resource.Buildings, -25)
			.With(Resource.Knowledge, -25)
			.With(Resource.Energy, 100);
			
        public static readonly Verb MakeTools = VerbFactory.Begin(Verbs.Id.MakeTools, 'K', "Make Tools")
			.With(Resource.Stone, -25)
			.With(Resource.Wood, -25)
			.With(Resource.Tools, 50);
			
        public static readonly Verb RaiseCattle = VerbFactory.Begin(Verbs.Id.RaiseCattle, 'L', "Raise Cattle")
			.With(Resource.Buildings, -25)
			.With(Resource.Food, 75)
			.With(Resource.People, -25);
			
        public static readonly Verb Mine = VerbFactory.Begin(Verbs.Id.Mine, 'M', "Mine")
			.With(Resource.Stone, 50);
			
        public static readonly Verb Print3D = VerbFactory.Begin(Verbs.Id.Print3D, 'N', "3D Print")
			.With(Resource.Tools, 75)
			.With(Resource.Buildings, 75)
			.With(Resource.Knowledge, -25)
			.With(Resource.Energy, -25)
			.With(Resource.Computers, -25);
			
        public static readonly Verb Compute = VerbFactory.Begin(Verbs.Id.Compute, 'O', "Compute")
			.With(Resource.Buildings, -25)
			.With(Resource.Knowledge, -25)
			.With(Resource.Energy, -25)
			.With(Resource.Machinery, -25)
			.With(Resource.Computers, 100);
			
        public static readonly Verb Farm = VerbFactory.Begin(Verbs.Id.Farm, 'P', "Farm")
			.With(Resource.Tools, -25)
			.With(Resource.Buildings, -25)
			.With(Resource.Herbs, 25)
			.With(Resource.Food, 25);
			
        public static readonly Verb Innovate = VerbFactory.Begin(Verbs.Id.Innovate, 'Q', "Innovate")
			.With(Resource.Knowledge, -25)
			.With(Resource.Books, -25)
			.With(Resource.Machinery, 50)
			.With(Resource.Computers, 50);
			
        public static readonly Verb Repair = VerbFactory.Begin(Verbs.Id.Repair, 'R', "Repair")
			.With(Resource.Tools, 25)
			.With(Resource.Buildings, 25)
			.With(Resource.People, -25);
			
        public static readonly Verb Socialize = VerbFactory.Begin(Verbs.Id.Socialize, 'S', "Socialize")
			.With(Resource.People, -25)
			.With(Resource.Energy, -25)
			.With(Resource.Computers, -25)
			.With(Resource.Joy, 100);
			
		public static readonly Verb ChopTrees = VerbFactory.Begin(Verbs.Id.ChopTrees, 'T', "Chop Trees")
			.With(Resource.Wood, 50);
		
		public static readonly Verb Teach = VerbFactory.Begin(Verbs.Id.Teach, 'U', "Teach")
			.With(Resource.People, -75)
			.With(Resource.Knowledge, 75);
		
		public static readonly Verb Write = VerbFactory.Begin(Verbs.Id.Write, 'V', "Write")
			.With(Resource.Knowledge, -25)
			.With(Resource.Books, 50);
		
		public static readonly Verb Brew = VerbFactory.Begin(Verbs.Id.Brew, 'W', "Brew")
			.With(Resource.Buildings, -25)
			.With(Resource.Herbs, -25)
			.With(Resource.People, -25)
			.With(Resource.Joy, 75);
		
		public static readonly Verb Luxuriate = VerbFactory.Begin(Verbs.Id.Luxuriate, 'X', "Luxuriate")
			.With(Resource.Joy, -50)
			.With(Resource.Points, 25);
		
		public static readonly Verb Recycle = VerbFactory.Begin(Verbs.Id.Recycle, 'Y', "Recycle")
			.With(Resource.Stone, 50)
			.With(Resource.Wood, 50)
			.With(Resource.Buildings, -25)
			.With(Resource.People, -25);
		
		public static readonly Verb Read = VerbFactory.Begin(Verbs.Id.Read, 'Z', "Read")
			.With(Resource.People, -25)
			.With(Resource.Knowledge, 50)
			.With(Resource.Books, -25)
			.With(Resource.Joy, 50);

		public static readonly IReadOnlyList<Verb> All = new List<Verb>()
		{
			Trade, Build, Cook, MakeMedicine, Procreate, Fish, Forage, Hunt, MakeMachinery, MakeEnergy, MakeTools,
			RaiseCattle, Mine, Print3D, Compute, Farm, Innovate, Repair, Socialize, ChopTrees, Teach, Write, Brew,
			Luxuriate, Recycle, Read
		}.AsReadOnly();
		
		public static IReadOnlyList<Verb> Where(Predicate<Verb> predicate)
		{
			return All.Where(t => predicate(t)).ToList().AsReadOnly();
		}
    }
}