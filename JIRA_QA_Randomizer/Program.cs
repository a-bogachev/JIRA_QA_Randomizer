using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Atlassian.Jira;
using System.Text.RegularExpressions;

namespace JIRA_QA_Randomizer
{
    class Program
    {
        static void Main(string[] args)
        {
            string issueId="";
            //Checking console parameter.
            //if ( Regex.IsMatch( args[ 0 ], "^[1-9][0-9]*$" ) == false )
            //{
            //    Console.WriteLine( "Please enter only numbers of the issue key. Exit with no changes" );
            //    Environment.Exit( 1 );
            //}
            //else issueId = "demo-" + args[ 0 ];

            // Reading users list.
            List<string> group = new List<string>( );
            try
            {                
                System.IO.StreamReader file = new System.IO.StreamReader( @"users.txt" );
                while ( !file.EndOfStream )
                    group.Add( file.ReadLine( ) );
            }
            catch ( System.IO.FileNotFoundException)
            {
                Console.WriteLine( "Please check existing file users.txt. Exit with no changes" );
                Environment.Exit( 2 );
            }
            
            // Shuffle user list.
            //Random rand = new Random( );
            //var shuffledGroupNames = group.OrderBy( c => rand.Next( ) ).Select( c => c ).ToList( );
            // Creating pairs of users Packager-QA Engineer
            //List<string> group1 = new List<string>( );
            //List<string> group2 = new List<string>( );
            //for (int i=0; i<shuffledGroupNames.Count; i++ )
            //{
            //    if (i < shuffledGroupNames.Count / 2 )
            //    {
            //        group1.Add( shuffledGroupNames[ i ] );
            //    }
            //    else
            //    {
            //        group2.Add( shuffledGroupNames[ i ] );
            //    }
            //}
            // Dublicating issues.
            //for ( int i = 1; i < group1.Count; i++ )
            //{
            //    duplicate( issueId, group1[ i ], group2[ i ] );
            //    duplicate( issueId, group2[ i ], group1[ i ] );
            //}
            //duplicate( issueId, group2[ 0 ], group1[ 0 ] );
            //setPeople( issueId, group1[ 0 ], group2[ 0 ] ); 

            Console.WriteLine( "All tasks created. Press any key." );
            Console.ReadKey();
        }

        private static void setPeople( string issueKey, string packager, string qaEngineer )
        {
            string address = "", login = "", password = "";
            try
            {
                System.IO.StreamReader file = new System.IO.StreamReader( @"config.txt" );
                address = file.ReadLine( );
                login = file.ReadLine( );
                password = file.ReadLine( );
            }
            catch ( System.IO.FileNotFoundException )
            {
                Console.WriteLine( "Please check existing file config.txt. Exit with no changes" );
                Environment.Exit( 2 );
            }
            Jira client = Jira.CreateRestClient( address, login, password );
            try
            {
                Issue newIssue = GetIssue( client, issueKey ).Result;
                var transitions = newIssue.GetAvailableActionsAsync( ).Result;
                foreach (var item in transitions )
                {
                    if (item.Name == "Order Packaging" )
                    {
                        newIssue.WorkflowTransitionAsync( item.Name ).Wait( );
                    }
                }
                newIssue.CustomFields.Add( "Packager", packager );
                newIssue.CustomFields.Add( "QA Engineer", qaEngineer );
                newIssue.Assignee = packager;
                newIssue.SaveChanges( );
            }
            catch ( AggregateException e )
            {
                Console.WriteLine( "Please check if this issue exists." );
                Console.WriteLine( e.Message );
                Environment.Exit( 3 );
            }
        }

        private static async void duplicate( string issueKey, string packager, string qaEngineer)
        {
            string address="", login="", password="";
            try
            {
                System.IO.StreamReader file = new System.IO.StreamReader( @"config.txt" );
                address = file.ReadLine( );
                login = file.ReadLine( );
                password = file.ReadLine( );
            }
            catch ( System.IO.FileNotFoundException )
            {
                Console.WriteLine( "Please check existing file config.txt. Exit with no changes" );
                Environment.Exit( 2 );
            }
            Jira client = Jira.CreateRestClient( address, login, password);
            try
            {
                Issue iss = GetIssue( client, issueKey ).Result;
                Issue duplicatedIssue = client.CreateIssue( "DEMO", iss.ParentIssueKey );
                duplicatedIssue.Summary = iss.Summary;
                duplicatedIssue.Type = iss.Type.Name;
                duplicatedIssue.Priority = iss.Priority.Name;
                if ( iss.CustomFields[ "Application Name" ] != null )
                    duplicatedIssue.CustomFields.Add( "Application Name", iss.CustomFields[ "Application Name" ].Values[ 0 ] );
                if ( iss.CustomFields[ "Application Language" ] != null )
                    duplicatedIssue.CustomFields.Add( "Application Language", iss.CustomFields[ "Application Language" ].Values[ 0 ] );
                if ( iss.CustomFields[ "Application Version" ] != null )
                    duplicatedIssue.CustomFields.Add( "Application Version", iss.CustomFields[ "Application Version" ].Values[ 0 ] );
                if ( iss.CustomFields[ "Application Vendor" ] != null )
                    duplicatedIssue.CustomFields.Add( "Application Vendor", iss.CustomFields[ "Application Vendor" ].Values[ 0 ] );
                if ( iss.CustomFields[ "Packaging technology" ] != null )
                    duplicatedIssue.CustomFields.Add( "Packaging technology", iss.CustomFields[ "Packaging technology" ].Values[ 0 ] );
                duplicatedIssue.Description = iss.Description;

                duplicatedIssue = await duplicatedIssue.SaveChangesAsync( );

                Issue newIssue = GetIssue( client, duplicatedIssue.Key.Value ).Result;
                var transitions = newIssue.GetAvailableActionsAsync( ).Result;
                foreach ( var item in transitions )
                {
                    if ( item.Name == "Order Packaging" )
                    {
                        newIssue.WorkflowTransitionAsync( item.Name ).Wait( );
                    }
                }
                newIssue.CustomFields.Add( "Packager", packager );
                newIssue.CustomFields.Add( "QA Engineer", qaEngineer );
                newIssue.Assignee = packager;                
                newIssue.SaveChanges( );
            }
            catch ( AggregateException e)
            {
                Console.WriteLine( "Please check if this issue exists." );
                Console.WriteLine( e.Message );
                Environment.Exit( 3 );
            }
        }
        private static async Task<Issue> GetIssue(Jira client, string issueId)
        {
            var iss = await client.Issues.GetIssueAsync(issueId);
            return iss;
        }
    }    
}
