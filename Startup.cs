using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using WebSupergoo.ABCpdf12;

namespace ABCpdfAppService {
	public class Startup {
		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services) {

		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
			if(env.IsDevelopment()) {
				app.UseDeveloperExceptionPage();
			}

			app.UseRouting();

			app.UseEndpoints(endpoints => {
				endpoints.MapGet("/", async context => {
					context.Response.Clear();

					try {
						XSettings.InstallLicense("<paste your license here>");
						using(Doc doc = new Doc()) {

							doc.HtmlOptions.Engine = EngineType.WebKit;
							doc.HtmlOptions.Media = MediaType.Screen;
							int id = doc.AddImageUrl("https://www.websupergoo.com");

							// AddImageUrl only adds the first page. The following code adds any additional pages:
							while(doc.Chainable(id)) {
								doc.Page = doc.AddPage();
								id = doc.AddImageToChain(id);
							}
							// Compress output:
							for(int i = 1; i <= doc.PageCount; i++) {
								doc.PageNumber = i;
								doc.Flatten();
							}
							// This will ensure page is served in a web-efficient manner:
							doc.SaveOptions.Linearize = true;
							// Finally serve the page:
							context.Response.ContentType = "Application/Pdf";
							await context.Response.BodyWriter.WriteAsync(doc.GetData());
						}
					} catch(Exception ex) {
						await context.Response.WriteAsync(ex.Message);
					}
				});
			});
		}
	}
}
