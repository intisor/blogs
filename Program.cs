using Statiq.App;
using Statiq.Web;
using blogs;

return await Bootstrapper
  .Factory
  .CreateWeb(args)
  .ModifyPipeline(nameof(Statiq.Web.Pipelines.Content), pipeline => 
  {
      var apiKey = Environment.GetEnvironmentVariable("SMALLURL_API_KEY");
      if (!string.IsNullOrEmpty(apiKey))
      {
          pipeline.PostProcessModules.Add(new SmallUrlModule(apiKey));
      }
  })
  .RunAsync();