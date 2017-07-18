using LINQtoCSV;
using NET_DBToFlatFile.DataContexts;
using NET_DBToFlatFile.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NET_DBToFlatFile.Controllers
{
    public class FlatFileController : Controller
    {
        // GET: FlatFile
        public ActionResult Index()
        {
            var context = new DBContext();
            
            IEnumerable<UploadedData> list = context.UploadedDatas;

            return View(list);
        }

        // POST: FlatFile/Export/1
        [HttpPost]
        public ActionResult Export(int[] ExportItem)
        {
            bool useExtension = ConfigurationManager.AppSettings["useExtension"].Equals("1");
            bool useQuotes = ConfigurationManager.AppSettings["useQuotes"].Equals("1");
            bool exportedFileNameIncludeDate = ConfigurationManager.AppSettings["exportedFileNameIncludeDate"].Equals("1");
            string _exportedFileName = ConfigurationManager.AppSettings["exportedFileName"];
            string _path = ConfigurationManager.AppSettings["rootPath"];
            string _fileExtenstion = useExtension ? ".csv" : "";
            string _fileName = exportedFileNameIncludeDate ? string.Format("{0}({1}){2}", _exportedFileName, DateTime.Now.ToString("dd/MM/yyyy"), _fileExtenstion) : string.Format("{0}{1}", _exportedFileName, _fileExtenstion);
            string _savePath = Path.Combine(Server.MapPath(_path), _fileName);
            char _separator = ConfigurationManager.AppSettings["separator"].ToCharArray()[0];

            using (var context = new DBContext())
            {
                var listObj = context.UploadedDatas.Where(o => ExportItem.Contains(o.ID)).ToList();

                byte[] bytes = null;
                using (var ms = new MemoryStream())
                {
                    TextWriter tw = new StreamWriter(ms);

                    CsvContext cc = new CsvContext();
                    cc.Write<UploadedData>(listObj, tw, new CsvFileDescription()
                    {
                        SeparatorChar = separator,
                        QuoteAllFields = useQuotes,
                        EnforceCsvColumnAttribute = true
                    });

                    tw.Flush();
                    ms.Position = 0;
                    bytes = ms.ToArray();

                    return File(bytes, System.Net.Mime.MediaTypeNames.Application.Octet, _fileName);
                }

            }
        }

        // POST: FlatFile/Export/1
        
        public FileResult Export(int Id)
        {
            bool useExtension = ConfigurationManager.AppSettings["useExtension"].Equals("1");
            bool useQuotes = ConfigurationManager.AppSettings["useQuotes"].Equals("1");
            bool exportedFileNameIncludeDate = ConfigurationManager.AppSettings["exportedFileNameIncludeDate"].Equals("1");
            string _exportedFileName = ConfigurationManager.AppSettings["exportedFileName"];
            string _path = ConfigurationManager.AppSettings["rootPath"];
            string _fileExtenstion = useExtension ? ".csv" : "";
            string _fileName = exportedFileNameIncludeDate ? string.Format("{0}({1}){2}", _exportedFileName, DateTime.Now.ToString("dd/MM/yyyy"), _fileExtenstion) : string.Format("{0}{1}", _exportedFileName, _fileExtenstion);
            string _savePath = Path.Combine(Server.MapPath(_path), _fileName);
            char _separator = ConfigurationManager.AppSettings["separator"].ToCharArray()[0];


            using (var context = new DBContext())
            {
                var listObj = context.UploadedDatas.Where(o => o.ID == Id).ToList();

                byte[] bytes = null;
                using (var ms = new MemoryStream())
                {
                    TextWriter tw = new StreamWriter(ms);

                    CsvContext cc = new CsvContext();
                    cc.Write<UploadedData>(listObj, tw, new CsvFileDescription() {
                        SeparatorChar = _separator,
                        QuoteAllFields = useQuotes,
                        EnforceCsvColumnAttribute = true
                    });

                    tw.Flush();
                    ms.Position = 0;
                    bytes = ms.ToArray();

                    return File(bytes, System.Net.Mime.MediaTypeNames.Application.Octet, _fileName);
                }
                
            }
    
        }

        // GET: FlatFile/UploadAndSave
        /// <summary>
        /// Show UploadAndSave Page.
        /// </summary>
        /// <returns>UploadAndSave View</returns>
        public ActionResult UploadAndSave()
        {
            if (TempData["prevUploadStatus"] != null)
            {
                ViewBag.MessageColor = Convert.ToBoolean(TempData["prevUploadStatus"]) ? "Green" : "Red";
                ViewBag.Message = Convert.ToBoolean(TempData["prevUploadStatus"]) ? "Success" : "Failed";
                ViewBag.Status = true;
            }
            else
            {
                ViewBag.Status = false;
            }
            TempData.Remove("prevUploadStatus");

            return View();
        }

        // POST: FlatFile/UploadAndSave
        /// <summary>
        /// Upload file and save data to db.
        /// </summary>
        /// <param name="file">Uploaded file</param>
        /// <returns>UploadAndSave View with previous upload status</returns>
        [HttpPost]
        public ActionResult UploadAndSave(HttpPostedFileBase file)

        {
            try
            {
                if (file != null && file.ContentLength > 0)
                {
                    bool useExtension = ConfigurationManager.AppSettings["useExtension"].Equals("1");
                    bool useRealFileName = ConfigurationManager.AppSettings["useRealFileName"].Equals("1");
                    string _path = ConfigurationManager.AppSettings["rootPath"];
                    string _fileExtenstion = useExtension ? Path.GetExtension(file.FileName) : "";
                    string _fileName = useRealFileName ? Path.GetFileName(file.FileName) : string.Format("{0}{1}", Guid.NewGuid().ToString(), _fileExtenstion);
                    string _savePath = Path.Combine(Server.MapPath(_path), _fileName);
                    //string _absolutePath = HttpContext.Server.MapPath(_savePath);

                    using (var reader = new BinaryReader(file.InputStream))
                    {
                        // Save to path                   
                        if (System.IO.File.Exists(_savePath))
                        {
                            System.IO.File.Delete(_savePath);
                        }
                        file.SaveAs(_savePath);
                        // END Save to path
                    }

                    char separator = ConfigurationManager.AppSettings["separator"].ToCharArray()[0];
                    bool haveHeader = ConfigurationManager.AppSettings["haveHeader"].Equals("1");

                    if (haveHeader)
                    {
                        // replace first line with header from web.config
                        string fileHeader = ConfigurationManager.AppSettings["header"];
                        string[] lines = System.IO.File.ReadAllLines(_savePath);
                        string[] newLines = new string[lines.Length + 1];
                        newLines[0] = fileHeader;
                        for (int i = 0; i < lines.Length; i++)
                        {
                            newLines[i + 1] = lines[i];
                        }
                        System.IO.File.WriteAllLines(_savePath, newLines);
                    }

                    // Convert file to Linq    
                    CsvContext cc = new CsvContext();
                    List<UploadedData> list = cc.Read<UploadedData>(_savePath, new CsvFileDescription
                    {
                        SeparatorChar = separator,
                        FirstLineHasColumnNames = haveHeader,
                        FileCultureName = "en-US", // default is the current culture
                        EnforceCsvColumnAttribute = !haveHeader
                    }).ToList();

                    // Insert to DB
                    using (var context = new DBContext())
                    {
                        context.UploadedDatas.AddRange(list);
                        context.SaveChanges();
                    }

                    TempData["prevUploadStatus"] = true;
                    return RedirectToAction("UploadAndSave");
                }
                else
                {
                    // Do something if no file uploaded
                    return RedirectToAction("UploadAndSave");
                }

            }
            catch (Exception ex)
            {
                // set to false if doesn't pass validation
                TempData["prevUploadStatus"] = false;
                return RedirectToAction("UploadAndSave");
            }
        }
    }
}