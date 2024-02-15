using System.Net;

namespace WebTN_MVC.ExtendNethods
{
    public static class AppExtends
    {
        public static void AddStatusCodePages(this WebApplication app) // or IApplicationBuilder
        {
            app.UseStatusCodePages(appError =>
            {
                appError.Run(async context =>
                {
                    var respone = context.Response;
                    var code = respone.StatusCode;

                    var content = @$"<!DOCTYPE html>
                                        <html lang='vn'

                                        <head>
                                            <meta charset = 'UTF-8'>
                                            <meta name = 'viewport' content = 'width=device-width, initial-scale=1.0'>
                                            <title> Lỗi {code}</title>
                                        </head>

                                        <body>
                                            <p style='color: red; font - size: 30px; '>
                                                Có lỗi xẩy ra {code}- {(HttpStatusCode)code}
                                            </p>
                                        </body>

                                        </html>";

                    await respone.WriteAsync(content);
                });
            });

        }
    }
}