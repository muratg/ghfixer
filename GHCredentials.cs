using System;
using Octokit;
using Microsoft.Extensions.Configuration;

namespace GHFixer
{
	public class GHCredentials
	{
		public string Owner { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }

	}	
	
}