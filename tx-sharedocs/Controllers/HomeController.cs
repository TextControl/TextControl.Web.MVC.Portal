using LiteDB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using tx_sharedocs.Models;

namespace tx_sharedocs.Controllers {
    public class HomeController : Controller {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger) {
            _logger = logger;
        }

        public IActionResult Index() {

            // read all stored documents and return as model
            using (var db = new LiteDatabase("App_Data/MyData.db")) {

                var col = db.GetCollection<Document>("documents");

                col.EnsureIndex(x => x.Name);

                var results = col.Query().ToList();

                return View(results);
            }
            
        }

        public IActionResult View(string id) {

            // retrieve document by id and return document as model
            using (var db = new LiteDatabase("App_Data/MyData.db")) {

                var fs = db.FileStorage;

                using (MemoryStream ms = new MemoryStream()) {
                    fs.Download("$/documents/dummy_" + id + ".tx", ms);
                    var fileBytes = ms.ToArray();

                    ViewDocument viewDoc = new ViewDocument() {
                        Name = "dummy" + id + ".tx",
                        BinaryData = Convert.ToBase64String(fileBytes)
                    };

                    return View(viewDoc);
                }
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}