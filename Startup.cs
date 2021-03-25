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
						// When running as an App Service we need to install the license in code otherwise you will
						// get a licensing exception. ABCpdf automatically installs a trial license if no license is
						// found.
						// Your trial license may be found by uncommenting the line below WHEN RUNNING ON YOUR LOCAL
						// COMPUTER. NB XSettings.Key returns an empty string for purchased licenses.
						// throw new Exception(XSettings.Key);

						// Paste either the ABCpdf 12 key provided at time of purchase, or the value obtained from
						// uncommenting the line above. This must be set correctly prior to running on Azure.
						XSettings.InstallLicense("PASTE YOUR LICENSE HERE");

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
					await context.Response.CompleteAsync();

				});
			});
		}
	}
}
