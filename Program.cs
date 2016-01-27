using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Octokit;
using Microsoft.Extensions.Configuration;

/*
export DOTNET_REFERENCE_ASSEMBLIES_PATH=/Library/Frameworks/Mono.framework/Versions/Current/lib/mono/xbuild-frameworks/
*/

namespace GHFixer
{
    public class Program
    {
        public static GHCredentials GetGHCredentials(IConfigurationRoot config)
        {
            return new GHCredentials
            {
                Owner = config["ghowner"],
                Username = config["ghusername"],
                Password = config["ghpassword"]
            };
        }

        public static GitHubClient GetGitHubClient(GHCredentials ghcreds)
        {
            var basicAuth = new Credentials(ghcreds.Username, ghcreds.Password);
            var github = new GitHubClient(new ProductHeaderValue("muratg.ghfixer"));
            github.Credentials = basicAuth;

            return github;
        }

        public static void DumpMilestones(GitHubClient gh, string owner)
        {
            var repos = gh.Repository.GetAllForOrg(owner);

            using (var fs = File.CreateText(owner + "_milestones.csv"))
            {
                fs.WriteLine("owner, reponame, id, currentname, newname");

                foreach (var repo in repos.Result)
                {
                    var milestones = gh.Issue.Milestone.GetAllForRepository(owner, repo.Name, new MilestoneRequest { State = ItemState.All });

                    foreach (var ms in milestones.Result)
                    {
                        fs.WriteLine("{0}, {1}, {2}, {3}, {3}", owner, repo.Name, ms.Number, ms.Title, ms.Title);
                    }
                }
            }
        }

        public static void UpdateMilestones(GitHubClient gh, string owner)
        {
            var lines = File.ReadAllLines(owner + "_milestones_updated.csv")
                .ToList()
                .Skip(1);   // header of the CSV
            var changed = 0;

            Console.WriteLine();

            foreach (var line in lines)
            {
                var parseLine = line.Split(new char[] { ',' }, 5);
                var pOwner = parseLine[0].Trim();
                var pRepo = parseLine[1].Trim();
                var pMilestoneId = int.Parse(parseLine[2].Trim()); // TryParse
                var pMilestoneName = parseLine[3].Trim();  
                var pMilestoneNewName = parseLine[4].Trim();

                if (owner == pOwner && pMilestoneNewName != "" && pMilestoneName != pMilestoneNewName)
                {
                    Console.Write("Change !!! ");
                    Console.WriteLine("{0} -- {1} -- {2} -- {3} -- {4}", pOwner, pRepo, pMilestoneId, pMilestoneName, pMilestoneNewName);

                    try 
                    {
                        var qms = gh.Issue.Milestone.Get(pOwner, pRepo, pMilestoneId).Result;
                        Console.WriteLine(qms.Title);
                        if (qms.Title == pMilestoneNewName) 
                        {
                            Console.WriteLine("Name is already updated");
                        }
                        else 
                        {
                            Console.WriteLine("Changing to {0}", pMilestoneNewName);
                            gh.Issue.Milestone.Update(pOwner, pRepo, pMilestoneId, new MilestoneUpdate { Title = pMilestoneNewName });
                            changed++;     
                            qms = gh.Issue.Milestone.Get(pOwner, pRepo, pMilestoneId).Result;
                            Console.WriteLine("New name {0}", qms.Title); 
                        }    
                        
                    //queried name.. make sure matchgh.Repository.Get(owner, "").Result.Name 
                    //var x = gh.Issue.Milestone.Get(owner, "", 1); // with the ID.. compare names.
                    //gh.Issue.Milestone.Update(owner, x.Result.Title, 1, new MilestoneUpdate{});
                    //var y = new MilestoneUpdate();                    
                    }
                    catch (Exception ex) 
                    {
                        Console.WriteLine("*** Failed for {0}/{1}/{2}", pOwner, pRepo, pMilestoneId);
                        Console.WriteLine("  With Exception: {0}", ex.Message);
                    }
                }
                else
                {
                    //Console.Write("    Dont change !!! ");
                    //Console.WriteLine("{0} -- {1} -- {2} -- {3} -- {4}", pOwner, pRepo, pMilestoneId, pMilestoneName, pMilestoneNewName);
                }
            }

            Console.WriteLine("\n{0} milestone(s) are updated", changed);

        }

        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                Console.WriteLine("{0}", args[0]);
            }
            
            var config = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            var ghcreds = GetGHCredentials(config);
            var gh = GetGitHubClient(ghcreds);
            var owner = ghcreds.Owner;

            //DumpMilestones(gh, owner);
            UpdateMilestones(gh, owner);
        }
    }
}
