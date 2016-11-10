using System;
using System.Linq;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.TestManagement.Client;
using Microsoft.TeamFoundation.Build.Common;
using Microsoft.TeamFoundation.Common;
using System.Data;

namespace ConsoleApplication5
{
    class Program
    {

        public static class Constant
        {
            public static string PROCESSEDFILENAME = "ProcessedFileName";
            public static string TESTID = "TestID";
            public static string TESTNAME = "TestName";
            public static string TESTOUTCOME = "TestOutcome";
            public static string ERRORMESSAGE = "ErrorMessage";
            public static string BUILD = "Build";
            public static string BUILDDEFNITION = "BuildDefinition";
            public static string BUILDFAILUREREASON = "BuildFailureReason";


        }

        public class ResultTables
        {
            public DataTable CreateTestResultTable()
            {
                // Define the new datatable
                DataTable dt = new DataTable();
                DataColumn dc;
                dc = new DataColumn(Constant.PROCESSEDFILENAME);
                dt.Columns.Add(dc);
                dc = new DataColumn(Constant.TESTID);
                dt.Columns.Add(dc);
                dc = new DataColumn(Constant.TESTNAME);
                dt.Columns.Add(dc);
                dc = new DataColumn(Constant.TESTOUTCOME);
                dt.Columns.Add(dc);
                dc = new DataColumn(Constant.ERRORMESSAGE);
                dt.Columns.Add(dc);
                return dt;
            }

            public DataTable BuildFailureTable()
            {
                // Define the new datatable
                DataTable dt = new DataTable();
                DataColumn dc;
                dc = new DataColumn(Constant.BUILD);
                dt.Columns.Add(dc);
                dc = new DataColumn(Constant.BUILDDEFNITION);
                dt.Columns.Add(dc);
                dc = new DataColumn(Constant.BUILDFAILUREREASON);
                dt.Columns.Add(dc);
                return dt;
            }

        }
        
        public static QueryOptions BuildDef { get; private set; }

        static void Main(string[] args)
        {

            Uri tfsCollectionURL = new Uri("http://sqlbuvsts01:8080/");
            string tfsProjectName = "SQL Server";

            TfsTeamProjectCollection tfs = new TfsTeamProjectCollection(tfsCollectionURL);
            tfs.EnsureAuthenticated();
            var _tms = tfs.GetService<ITestManagementService>();
            
            IBuildServer buildServer = (IBuildServer)tfs.GetService(typeof(IBuildServer));

            //     foreach (TeamProject proj in teamProjects)
            //     {

            var defs = buildServer.QueryBuildDefinitions(tfsProjectName);

            System.Console.WriteLine(string.Format("Team Project: {0}", tfsProjectName));

            //foreach (IBuildDefinition def in defs)
            //{

            //[O][BI_AZURE_DEV][BI_ALL_Cloud][Gated]   BI_Azure_Dev 2646146
            //[O][BI_AZURE_DEV][BI_ALL_Root][Gated]    BI_Azure_Dev 2646198
            //  var dteService = Package.GetGlobalService(typeof(DTE)) as DTE;
            // var tfBuild = (dteService.GetObject("Microsoft.VisualStudio.TeamFoundation.Build.TeamFoundationServerEx") as TeamFoundationServerEx);
            //[O][BI_AZURE_DEV][BI_ALL2][Rolling] Id: 2648274
            //:  [O][BI_AZURE_DEV][BI_ALL][Rolling] Id: 2649261
            

            var def = "[O][BI_Azure_dev][BIAzure_Ship][OnDemand]";
            IBuildDetailSpec spec = buildServer.CreateBuildDetailSpec(tfsProjectName, def);
            spec.MaxBuildsPerDefinition = 1;
            spec.QueryOrder = BuildQueryOrder.FinishTimeDescending;
            //spec.MinFinishTime = DateTime.Now.AddHours(48); //
            spec.BuildNumber = "BI_Azure_BISNext 13.0.1700.374.01";
            var builds = buildServer.QueryBuilds(spec);
            int j = builds.Builds.Length;
            Console.WriteLine(" value : {0} ", builds.Builds.Length);
            //for (int i = 0; i < builds.Builds.Length; i++)
            //{

            var buildDetail = builds.Builds[0];

            //   System.Console.WriteLine("   {0} - {1} - {2} - {3}", def, buildDetail.Status, buildDetail.FinishTime, buildDetail.BuildNumber);
            //   tfBuild.OpenBuild(buildDetail.Uri);
            Uri buildDetailUri = builds.Builds[0].Uri;
            var proj = _tms.TestControllers; //.GetTeamProject(tfsProjectName);
            var testRuns2 = _tms.GetTeamProject(tfsProjectName);
            var testRuns = _tms.GetTeamProject(tfsProjectName).TestRuns.ByBuild(buildDetailUri);

            var activityTrackingNodes = InformationNodeConverters.GetActivityTrackingNodes(buildDetail);
            foreach (var activity in activityTrackingNodes)
            {
                if (activity.State != "Canceled" && (activity.Node.Children.Nodes.Count() == 0 || (activity.Node.Children.Nodes.Any(x => x.Type == "BuildMessage") && activity.DisplayName != "Sequence")))
                {
                    if (activity.FinishTime.ToString() == "1/1/0001 12:00:00 AM")
                        continue;
                    Console.WriteLine(activity.DisplayName + ":" + (activity.FinishTime - activity.StartTime));
                    Console.WriteLine(activity.State);
                }
            }
            // System.Console.WriteLine(string.Format("   {0} - {1} - {2} - {3}", buildDefinition.Name, buildDetail.Status.ToString(), buildDetail.FinishTime, buildDetail.BuildNumber, buildDetail.TestStatus));



            var _buildSummarychildNodes = InformationNodeConverters.GetCustomSummaryInformationNodes(buildDetail);

            foreach (var buildSummaryNode in _buildSummarychildNodes)
            {
                Console.WriteLine("build Summary node  :  {0} : ", buildSummaryNode.Node);
                //Console.WriteLine("build Summary node  :  {0} - {1} - {2 } - {3}: ", buildSummaryNode.Message, buildSummaryNode.Node, buildSummaryNode.SectionName ,buildSummaryNode.GetType());

                Console.WriteLine(buildSummaryNode.Node.Fields);

            }

            var _buildProjectNodes = InformationNodeConverters.GetTopLevelProjects(buildDetail);

            foreach (var buildProjectNode in _buildProjectNodes)
            {
                Console.WriteLine("buildProjectNode  :  {0} : ", buildProjectNode.Node);
                Console.WriteLine("buildProjectNode  errors :  {0} : ", buildProjectNode.GetErrors("error"));
            }


            //var _buildSteps = InformationNodeConverters.GetCustomSummaryInformation(buildDetail);

            //foreach (var buildStep in _buildSteps)
            //{
            //    Console.WriteLine("_buildStep  :  {0} : ", buildStep);

            //}


            foreach (IBuildInformationNode node in buildDetail.Information.Nodes)
            {
                //Console.Write("Parent node Info : {0} ", node);

                Console.WriteLine("Children count: - {0} ", node.Children.Nodes.Count());

                //Console.WriteLine("message : - {0} ", message.ToString());

                //var message = buildDetail.Information.GetNodesByType(InformationTypes.BuildMessage);
                //Console.WriteLine("message : - {0} ", message);



                foreach (var Field in node.Fields)
                {

                    Console.WriteLine("Field of parent node Name: {0} - value : {1}", Field.Key, Field.Value);
                    Console.WriteLine("new parent node line");



                }

                foreach (IBuildInformationNode childNode in node.Children.Nodes)
                {
                    //Console.WriteLine("Child Node details {0}:", childNode.Fields);



                    foreach (var cField in childNode.Fields)
                    {

                        Console.WriteLine("Field of child node Name : {0} - value : {1} : {2}", cField.Key, cField.Value);


                    }

                }


                foreach (var testRun in testRuns)
                {
                    Console.WriteLine(string.Format("{0}", testRun.Title));
                    Console.WriteLine(string.Format("TestRunId: {0} | TestPlanId: {1}", testRun.Id, testRun.TestPlanId));
                    Console.WriteLine(string.Format("TestSettingsId: {0} | TestEnvironmentId {1} ", testRun.TestSettingsId, testRun.TestEnvironmentId));

                    var totalTests = testRun.Statistics.TotalTests;

                    foreach (var et in testRun.QueryResultsByOutcome(TestOutcome.Error))
                    {
                        Console.WriteLine(string.Format("{0}: {1} - {2}", et.Outcome, et.TestCaseTitle, et.ErrorMessage));
                    }

                    //foreach (var tp in testRun.QueryResultsByOutcome(TestOutcome.Passed))
                    //{
                    //    Console.WriteLine(string.Format("{0}: {1} ", tp.Outcome, tp.TestCaseTitle));
                    //}

                    foreach (var tf in testRun.QueryResultsByOutcome(TestOutcome.Failed))
                    {
                        Console.WriteLine(string.Format("{0}: {1} - {2}", tf.Outcome, tf.TestCaseTitle, tf.ErrorMessage));
                    }

                    foreach (var tw in testRun.QueryResultsByOutcome(TestOutcome.Warning))
                    {
                        Console.WriteLine(string.Format("{0}: {1} - {2}", tw.Outcome, tw.TestCaseTitle, tw.ErrorMessage));
                    }

                    foreach (var ta in testRun.QueryResultsByOutcome(TestOutcome.Aborted))
                    {
                        Console.WriteLine(string.Format("{0}: {1} - {2}", ta.Outcome, ta.TestCaseTitle, ta.ErrorMessage));
                    }

                    foreach (var tb in testRun.QueryResultsByOutcome(TestOutcome.Blocked))
                    {
                        Console.WriteLine(string.Format("{0}: {1} - {2}", tb.Outcome, tb.TestCaseTitle, tb.ErrorMessage));
                    }

                    foreach (var ti in testRun.QueryResultsByOutcome(TestOutcome.Inconclusive))
                    {
                        Console.WriteLine(string.Format("{0}: {1} - {2}", ti.Outcome, ti.TestCaseTitle, ti.ErrorMessage));
                    }

                    foreach (var to in testRun.QueryResultsByOutcome(TestOutcome.Timeout))
                    {
                        Console.WriteLine(string.Format("{0}: {1} - {2}", to.Outcome, to.TestCaseTitle, to.ErrorMessage));
                    }

                    //    // Get the test results by user by passing in the Test Foundation Identity
                    //    // testRun.QueryResultsByOwner(TeamFoundationIdentity);
                }


                if (testRuns.Count() == 0)
                    Console.WriteLine("No Test Results have been associated with the selected build");

                Console.ReadLine();
            }
        }

            

        private static Type Typeof(ITestManagementService testManagementService)
        {
            throw new NotImplementedException();
        }
    }


    //    var _buildSummaryNodes = InformationNodeConverters.GetCustomSummaryInformationNodes(buildDetail);

    //        foreach (var buildSummaryNode in _buildSummaryNodes)
    //        {
    //            Console.WriteLine("build Summary node  :  {0} : ", buildSummaryNode.Node);
    //            Console.WriteLine("build Summary node  :  {0} - {1} - {2 } - {3}: ", buildSummaryNode.Message, buildSummaryNode.Node, buildSummaryNode.SectionName ,buildSummaryNode.GetType());

    //            Console.WriteLine(buildSummaryNode.Node.Fields);


    //            Console.WriteLine("build Summary node  :  {0} : ", buildSummaryNode.Node.Children);
    //                Console.WriteLine("build Summary node  :  {0} - {1} - {2 } - {3}: ", buildSummaryNode.Message, buildSummaryNode.Node, buildSummaryNode.SectionName ,buildSummaryNode.GetType());
    //         om sai ram
    //}






    // var _buildSummarychildNodes = InformationNodeConverters.GetCustomSummaryInformationNodes(buildDetail);

    //foreach (var buildSummaryNode in buildSummaryNode)
    //{
    //    Console.WriteLine("build Summary node  :  {0} : ", buildSummaryNode.Node);
    //    //Console.WriteLine("build Summary node  :  {0} - {1} - {2 } - {3}: ", buildSummaryNode.Message, buildSummaryNode.Node, buildSummaryNode.SectionName ,buildSummaryNode.GetType());

    //    Console.WriteLine(buildSummaryNode.Node.Fields);

    //}

    //var _buildProjectNodes = InformationNodeConverters.GetTopLevelProjects(buildDetail);

    //    foreach (var buildProjectNode in _buildProjectNodes)
    //    {
    //        Console.WriteLine("buildProjectNode  :  {0} : ", buildProjectNode.Node);
    //       // Console.WriteLine("buildProjectNode  :  {0} : ", buildProjectNode.GetErrors("error"));
    //    }


    //var _buildSteps = InformationNodeConverters.GetCustomSummaryInformation(buildDetail);

    //foreach (var buildStep in _buildSteps)
    //{
    //    Console.WriteLine("_buildStep  :  {0} : ", buildStep);

    //}

    //var buildErrors = InformationNodeConverters.GetBuildErrors(buildDetail);

    //for(int k =0; k < buildErrors.Count;k++)
    //{

    //    Console.WriteLine("build error :  {0} - {1} - {2 } : ", buildErrors[k].Message, buildErrors[k].Node, buildErrors[k].ServerPath);

    //}



    //foreach (IBuildInformationNode node in buildDetail.Information.Nodes)
    //{
    //    //Console.Write("Parent node Info : {0} ", node);

    //    Console.WriteLine("Children count: - {0} ", node.Children.Nodes.Count());

    //    //Console.WriteLine("message : - {0} ", message.ToString());

    //    //var message = buildDetail.Information.GetNodesByType(InformationTypes.BuildMessage);
    //    //Console.WriteLine("message : - {0} ", message);



    //    foreach (var Field in node.Fields)
    //    {

    //        Console.WriteLine("Field of parent node Name: {0} - value : {1}", Field.Key, Field.Value);
    //        Console.WriteLine("new parent node line");



    //    }

    //    foreach (IBuildInformationNode childNode in node.Children.Nodes)
    //    {
    //        //Console.WriteLine("Child Node details {0}:", childNode.Fields);



    //        foreach (var cField in childNode.Fields)
    //        {

    //            Console.WriteLine("Field of child node Name : {0} - value : {1} : {2}", cField.Key, cField.Value);


    //        }

    //    }

    //}


    //List<IBuildInformationNode> topLevelProjectNodes = buildDetail.Information.GetNodesByType(InformationTypes.BuildMessage);


    // Getting the TeamFoundationBuild from DTE Services



    //var activityTrackingNodes = InformationNodeConverters.GetActivityTrackingNodes(buildDetail);
    //foreach (var activity in activityTrackingNodes)
    //{
    //    if (activity.State != "Canceled" && (activity.Node.Children.Nodes.Count() == 0 || (activity.Node.Children.Nodes.Any(x => x.Type == "BuildMessage") && activity.DisplayName != "Sequence")))
    //    {
    //        if (activity.FinishTime.ToString() == "1/1/0001 12:00:00 AM")
    //            continue;
    //            Console.WriteLine(activity.DisplayName + ":" + (activity.FinishTime - activity.StartTime));
    //    }
    //}
    //System.Console.WriteLine(string.Format("   {0} - {1} - {2} - {3}", buildDefinition.Name, buildDetail.Status.ToString(), buildDetail.FinishTime , buildDetail.BuildNumber ,buildDetail.TestStatus ));



    //IBuildServer buildServer = collection.GetService<IBuildServer>();
    //IBuildDetail build = buildServer.GetBuild(buildUri, new String[] { InformationTypes.BuildError, InformationTypes.BuildProject, InformationTypes.BuildWarning }, QueryOptions.None);





    //}

    //System.Console.WriteLine();



}






//}

//}




