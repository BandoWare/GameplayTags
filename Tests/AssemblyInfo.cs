using BandoWare.GameplayTags;

[assembly: GameplayTag("Test", flags: GameplayTagFlags.HideInEditor)]
[assembly: GameplayTag("Test.A.B.C0", flags: GameplayTagFlags.HideInEditor)]
[assembly: GameplayTag("Test.A.B.C1", flags: GameplayTagFlags.HideInEditor)]
[assembly: GameplayTag("Test.D", flags: GameplayTagFlags.HideInEditor)]

[assembly: GameplayTag("Test.Parent", flags: GameplayTagFlags.HideInEditor)]
[assembly: GameplayTag("Test.Parent.FirstChild", flags: GameplayTagFlags.HideInEditor)]
[assembly: GameplayTag("Test.Parent.SecondChild", flags: GameplayTagFlags.HideInEditor)]
[assembly: GameplayTag("Test.Parent.SecondChild.Grandson", flags: GameplayTagFlags.HideInEditor)]

