using LiteDB;
using Microsoft.AspNetCore.Mvc;
using tx_sharedocs.Models;

namespace tx_sharedocs.Controllers {
    public class DocumentController : Controller {
        [HttpPost]
        public bool CreateDummyDocument(string CompanyName) {

            // create a unique ID
            string guid = Guid.NewGuid().ToString();

            using (TXTextControl.ServerTextControl tx = new TXTextControl.ServerTextControl()) {

                // create ServerTextControl and load template
                tx.Create();
                tx.Load("App_Data/vendor.tx", TXTextControl.StreamType.InternalUnicodeFormat);

                using (TXTextControl.DocumentServer.MailMerge mailMerge = new TXTextControl.DocumentServer.MailMerge()) {
                    mailMerge.TextComponent = tx;

                    // flatten PDF
                    mailMerge.FormFieldMergeType = TXTextControl.DocumentServer.FormFieldMergeType.Replace;

                    // create dummy merge data with barcode URL
                    VendorData data = new VendorData() {
                        vendor_name = CompanyName,
                        document_qr = Url.Action("View", "Home", new { id = guid }, protocol: Request.Scheme)
                    };

                    // merge template
                    mailMerge.MergeObject(data);
                }

                // save results and store in database
                byte[] results;
                tx.Save(out results, TXTextControl.BinaryStreamType.InternalUnicodeFormat);

                using (MemoryStream ms = new MemoryStream(results)) {

                    using (var db = new LiteDatabase("App_Data/MyData.db")) {

                        var col = db.GetCollection<Document>("documents");

                        var document = new Document {
                            Name = "dummy_" + guid + ".tx",
                            DocumentId = guid,
                            Created = DateTime.Now
                        };

                        col.Insert(document);
                        var fs = db.FileStorage;
                        fs.Upload("$/documents/" + "dummy_" + guid + ".tx", "dummy_" + guid + ".tx", ms);
                    }

                }

                return true;
            }
        }
    }
}
