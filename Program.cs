using System;
using Octokit;
using Octokit.Internal;
using Microsoft.Extensions.Configuration;

/*
export DOTNET_REFERENCE_ASSEMBLIES_PATH=/Library/Frameworks/Mono.framework/Versions/Current/lib/mono/xbuild-frameworks/
*/

namespace ConsoleApplication
{
    public class Program
    {
    
        public static GitHubClient GetGitHubClient(string username, string password, string owner)
		{			
			var basicAuth = new Credentials(username, password);
			var github = new GitHubClient(new ProductHeaderValue("muratg.ghfixer"));
			github.Credentials = basicAuth;
	
            return github;
        }
        public static void Main(string[] args)
        {
            var config = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
				.AddEnvironmentVariables()
				.Build();
            
            var gh = GetGitHubClient(config["ghusername"], config["ghpassword"], "aspnet");
            
            var releases = gh.Release.GetAll("aspnet","mvc").Result;
            
            foreach (var release in releases)
            {
                Console.WriteLine("{0}\n",release.TagName.ToString());
            }
                        
            Console.WriteLine("Hello World!");
        }
    }
}
