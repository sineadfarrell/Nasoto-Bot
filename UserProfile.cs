// stores user's profile 
using System;
using System.Collections.Generic;

public class UserProfile
{
    public string Name { get; set; }
  
    // The list of companies the user wants to review.

    public Nullable<int> NumberOfModules {get; set;}
    public List<string> ModulesTaken { get; set; } = new List<string>();

    public string Stage {get; set;}
}