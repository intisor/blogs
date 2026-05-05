using Statiq.App;
using Statiq.Web;
using blogs;
using System;

return await Bootstrapper
  .Factory
  .CreateWeb(args)
  .ModifyPipeline("Content", pipeline => 
  {
      var apiKey = Environment.GetEnvironmentVariable("SMALLURL_API_KEY");
      if (!string.IsNullOrEmpty(apiKey))
      {
          pipeline.PostProcessModules.Add(new SmallUrlModule(apiKey));
      }
  })
  .ModifyPipeline("Posts", pipeline => 
  {
      var apiKey = Environment.GetEnvironmentVariable("SMALLURL_API_KEY");
      if (!string.IsNullOrEmpty(apiKey))
      {
          pipeline.PostProcessModules.Add(new SmallUrlModule(apiKey));
      }
  })
  .RunAsync();