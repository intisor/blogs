using Statiq.App;
using Statiq.Web;
using blogs;
using System;

return await Bootstrapper
  .Factory
  .CreateWeb(args)
  .ConfigureEngine(engine => 
  {
      var apiKey = Environment.GetEnvironmentVariable("SMALLURL_API_KEY");
      var apiBase = Environment.GetEnvironmentVariable("SMALLURL_API_BASE");
      if (!string.IsNullOrEmpty(apiKey))
      {
          var module = new SmallUrlModule(apiKey, apiBase);
          foreach (var pipeline in engine.Pipelines)
          {
              pipeline.Value.PostProcessModules.Add(module);
          }
      }
  })
  .RunAsync();