using System;
using System.Collections.Generic;
using System.IO;
using SenseNet.Diagnostics;

namespace SenseNet.Portal.Setup
{
    public partial class Cleanup : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var filesToDelete = new List<string>
                                    {
                                        Server.MapPath("/Default.aspx"),
                                        Server.MapPath("/Cleanup.aspx")
                                    };
            var foldersToDelete = new List<string>
                                      {
                                          Server.MapPath("/IISConfig"),
                                          Server.MapPath("/Install"),
                                          Server.MapPath("/Root"),
                                          Server.MapPath("/Scripts")
                                      };

            var removeFilesDelegate = new RemoveInstallFilesDelegate(RemoveInstallFiles);
            removeFilesDelegate.BeginInvoke(filesToDelete, foldersToDelete, null, null);

            Response.Redirect("/");
        }

        private delegate void RemoveInstallFilesDelegate(List<string> filesToDelete, List<string> foldersToDelete);

        private static void RemoveInstallFiles(List<string> filesToDelete, List<string> foldersToDelete)
        {
            int retryCount;

            foreach (var filePath in filesToDelete)
            {
                retryCount = 0;

                while (retryCount < 3)
                {
                    try
                    {
                        File.Delete(filePath);
                        retryCount = 100;
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteException(new Exception("Install cleanup error: could not delete " + filePath, ex));
                        retryCount++;
                    }
                }
            }

            foreach (var folderPath in foldersToDelete)
            {
                retryCount = 0;

                while (retryCount < 3)
                {
                    try
                    {
                        Directory.Delete(folderPath, true);
                        retryCount = 100;
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteException(new Exception("Install cleanup error: could not delete " + folderPath, ex));
                        retryCount++;
                    }
                }
            }
        }
    }
}